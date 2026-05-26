using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.ViewModels;
using shinnzinn2026.Models;

namespace shinnzinn2026.Controllers
{
    public class WorkListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public WorkListController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> WorkList(int? SelectedYear, int? SelectedMonth, int? SelectedWeek, string? targetStaffCd)
        {
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (string.IsNullOrEmpty(loginStaffCd)) return RedirectToAction("Login", "Login");

            var loginUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
            if (loginUser == null) return RedirectToAction("Login", "Login");

            string actualTargetCd = (loginUser.ManagerFlag == 1 && !string.IsNullOrEmpty(targetStaffCd))
                                    ? targetStaffCd
                                    : loginStaffCd;

            var targetUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == actualTargetCd);
            if (targetUser == null) return RedirectToAction("WorkList");

            var viewModel = new WorkListViewModel
            {
                SelectedYear = SelectedYear ?? DateTime.Now.Year,
                SelectedMonth = SelectedMonth ?? DateTime.Now.Month,
                LoginStaffCd = loginUser.StaffCd,
                LoginUserName = loginUser.Name ?? "不明",
                IsManager = loginUser.ManagerFlag == 1,
                TargetStaffCd = actualTargetCd,
                TargetUserName = targetUser.Name ?? "不明なユーザー"
            };

            var allStaffs = await _context.Staffs.ToListAsync();
            var palette = new[] { "#FFEB3B", "#4AF2A1", "#FF9CF5", "#00E5FF", "#FFB74D", "#B39DDB" };
            int colorIndex = 0;

            var staffLastNameMap = new Dictionary<int, string>();
            var staffColorMap = new Dictionary<int, string>();

            foreach (var s in allStaffs)
            {
                var parts = (s.Name ?? "").Split(new[] { ' ', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                staffLastNameMap[s.Id] = parts.Length > 0 ? parts[0] : s.Name;
                staffColorMap[s.Id] = palette[colorIndex % palette.Length];
                colorIndex++;
            }

            var dbWorks = await _context.Works
                .Where(w => w.StaffCd == actualTargetCd &&
                            w.WorkDate.Year == viewModel.SelectedYear &&
                            w.WorkDate.Month == viewModel.SelectedMonth)
                .ToListAsync();

            viewModel.AttendanceList = new List<WorkModel>();
            viewModel.EditorNames = new Dictionary<string, string>();
            viewModel.EditorColors = staffColorMap;

            double total = 0;
            double totalOT = 0;
            double totalNight = 0;

            int daysInMonth = DateTime.DaysInMonth(viewModel.SelectedYear, viewModel.SelectedMonth);
            int selectedWeekNum = SelectedWeek ?? 0;

            for (int i = 1; i <= daysInMonth; i++)
            {
                if (selectedWeekNum == 1 && (i < 1 || i > 7)) continue;
                if (selectedWeekNum == 2 && (i < 8 || i > 14)) continue;
                if (selectedWeekNum == 3 && (i < 15 || i > 21)) continue;
                if (selectedWeekNum == 4 && (i < 22 || i > 28)) continue;
                if (selectedWeekNum == 5 && i < 29) continue;

                var date = new DateTime(viewModel.SelectedYear, viewModel.SelectedMonth, i);
                var work = dbWorks.FirstOrDefault(w => w.WorkDate.Date == date);
                string dateKey = date.ToString("yyyyMMdd");

                double dailyTotal = 0;
                double dailyOT = 0;
                double dailyNight = 0;

                if (work != null)
                {
                    viewModel.AttendanceList.Add(work);
                    bool isSpecialCase = work.Remarks != null && (work.Remarks.Contains("[有給:") || work.Remarks.Contains("[特休:"));

                    if (isSpecialCase)
                    {
                        dailyTotal = 8.0;
                    }
                    else if (work.AttendanceTime.HasValue && work.LeaveTime.HasValue)
                    {
                        var rest = work.RestTime ?? TimeSpan.Zero;
                        dailyTotal = (work.LeaveTime.Value - work.AttendanceTime.Value - rest).TotalHours;
                        if (dailyTotal < 0) dailyTotal = 0;
                        dailyOT = dailyTotal > 8.0 ? dailyTotal - 8.0 : 0;

                        DateTime a = work.AttendanceTime.Value;
                        DateTime l = work.LeaveTime.Value;
                        DateTime p2Start = a.Date.AddHours(22);
                        DateTime p2End = a.Date.AddDays(1).AddHours(5);
                        if (a < p2End && l > p2Start)
                        {
                            var os = a > p2Start ? a : p2Start;
                            var oe = l < p2End ? l : p2End;
                            dailyNight = (oe - os).TotalHours;
                        }
                    }

                    if (work.UpdatedId.HasValue && work.UpdatedId.Value != 0 && staffLastNameMap.ContainsKey(work.UpdatedId.Value))
                        viewModel.EditorNames[dateKey] = staffLastNameMap[work.UpdatedId.Value];
                    else
                        viewModel.EditorNames[dateKey] = "-";
                }
                else
                {
                    viewModel.AttendanceList.Add(new WorkModel { Id = 0, StaffCd = viewModel.TargetStaffCd, WorkDate = date });
                    viewModel.EditorNames[dateKey] = "-";
                }

                viewModel.DailyOvertime[dateKey] = Math.Round(dailyOT, 2);
                viewModel.DailyNightHours[dateKey] = Math.Round(dailyNight, 2);
                total += dailyTotal;
                totalOT += dailyOT;
                totalNight += dailyNight;
            }

            viewModel.TotalHours = Math.Round(total, 2);
            viewModel.TotalOvertimeHours = Math.Round(totalOT, 2);
            viewModel.TotalNightHours = Math.Round(totalNight, 2);

            ViewData["SelectedWeek"] = selectedWeekNum;
            ViewData["SelectedYear"] = viewModel.SelectedYear;
            ViewData["SelectedMonth"] = viewModel.SelectedMonth;
            ViewData["TargetStaffCd"] = actualTargetCd;
            ViewData["IsManagerMode"] = viewModel.IsManager;

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkRecord(
            long workId, string? attendanceTimeStr, string? leaveTimeStr, string? restTimeStr,
            string? dailyStatus, string? lateReason, string? earlyReason, string? absenceReason,
            string? leaveStatus, string? paidLeaveType, string? specialLeaveReason,
            int year, int month, int day, int week, string targetStaffCd)
        {
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            var loginUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
            bool isManager = loginUser?.ManagerFlag == 1;

            WorkModel work;
            bool isNew = false;

            if (workId == 0)
            {
                work = new WorkModel
                {
                    StaffCd = targetStaffCd,
                    WorkDate = new DateTime(year, month, day),
                    RegistrationTime = DateTime.Now,
                    RegistrantId = loginUser?.Id
                };
                isNew = true;
            }
            else
            {
                work = await _context.Works.FindAsync(workId);
                if (work == null) return RedirectToAction("WorkList", new { SelectedYear = year, SelectedMonth = month, SelectedWeek = week, targetStaffCd = targetStaffCd });
            }

            var parts = new List<string>();
            if (dailyStatus == "late") parts.Add($"[遅刻:{lateReason ?? ""}]");
            else if (dailyStatus == "early") parts.Add($"[早退:{earlyReason ?? ""}]");
            else if (dailyStatus == "absence") parts.Add($"[欠勤:{absenceReason ?? ""}]");
            if (leaveStatus == "paidLeave") parts.Add($"[有給:{paidLeaveType ?? "全日"}]");
            else if (leaveStatus == "specialLeave") parts.Add($"[特休:{specialLeaveReason ?? ""}]");

            work.Remarks = parts.Count > 0 ? string.Join(" ", parts) : null;
            work.UpdatedTime = DateTime.Now;
            work.UpdatedId = loginUser?.Id;

            if (isManager)
            {
                if (TimeSpan.TryParse(attendanceTimeStr, out var at)) work.AttendanceTime = work.WorkDate.Date.Add(at);
                else if (string.IsNullOrEmpty(attendanceTimeStr)) work.AttendanceTime = null;

                if (TimeSpan.TryParse(leaveTimeStr, out var lt))
                {
                    work.LeaveTime = work.WorkDate.Date.Add(lt);
                    if (work.AttendanceTime.HasValue && work.LeaveTime < work.AttendanceTime)
                        work.LeaveTime = work.LeaveTime.Value.AddDays(1);
                }
                else if (string.IsNullOrEmpty(leaveTimeStr)) work.LeaveTime = null;
            }

            if (work.AttendanceTime.HasValue && work.LeaveTime.HasValue)
            {
                var noon = work.WorkDate.Date.AddHours(12);
                if (work.AttendanceTime <= noon && work.LeaveTime > noon)
                {
                    work.RestTime = TimeSpan.FromHours(1);
                }
                else
                {
                    work.RestTime = TimeSpan.Zero;
                }
            }
            else
            {
                work.RestTime = TimeSpan.Zero;
            }

            if (isManager)
            {
                if (!string.IsNullOrEmpty(restTimeStr) && TimeSpan.TryParse(restTimeStr, out var rt))
                {
                    work.RestTime = rt;
                }
                else if (string.IsNullOrEmpty(restTimeStr))
                {
                    work.RestTime = TimeSpan.Zero;
                }
            }

            if (isNew) _context.Works.Add(work);
            else _context.Works.Update(work);

            await _context.SaveChangesAsync();
            return RedirectToAction("WorkList", new { SelectedYear = year, SelectedMonth = month, SelectedWeek = week, targetStaffCd = targetStaffCd });
        }
    }
}