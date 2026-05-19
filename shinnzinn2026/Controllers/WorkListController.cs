using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.ViewModels;
using shinnzinn2026.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

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

            var viewModel = new WorkListViewModel
            {
                SelectedYear = SelectedYear ?? DateTime.Now.Year,
                SelectedMonth = SelectedMonth ?? DateTime.Now.Month,
                LoginStaffCd = loginUser.StaffCd,
                LoginUserName = loginUser.Name ?? "不明",
                IsManager = loginUser.ManagerFlag == 1,
                TargetStaffCd = actualTargetCd
            };

            ViewData["SelectedWeek"] = SelectedWeek ?? 0;
            await LoadTargetData(viewModel, SelectedWeek ?? 0);
            return View(viewModel);
        }

        private async Task LoadTargetData(WorkListViewModel vm, int selectedWeek)
        {
            var targetUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == vm.TargetStaffCd);
            vm.TargetUserName = targetUser?.Name ?? "不明なユーザー";

            var allStaffs = await _context.Staffs.ToListAsync();
            var palette = new[] { "#FFEB3B", "#4AF2A1", "#FF9CF5", "#00E5FF", "#FFB74D", "#B39DDB" };
            int colorIndex = 0;

            var staffLastNameMap = new Dictionary<int, string>();
            var staffColorMap = new Dictionary<int, string>();

            foreach (var s in allStaffs)
            {
                var parts = (s.Name ?? "").Split(new[] { ' ', '　' }, StringSplitOptions.RemoveEmptyEntries);
                staffLastNameMap[s.Id] = parts.Length > 0 ? parts[0] : s.Name;
                staffColorMap[s.Id] = palette[colorIndex % palette.Length];
                colorIndex++;
            }

            var dbWorks = await _context.Works
                .Where(w => w.StaffCd == vm.TargetStaffCd &&
                            w.WorkDate.Year == vm.SelectedYear &&
                            w.WorkDate.Month == vm.SelectedMonth)
                .ToListAsync();

            vm.AttendanceList = new List<WorkModel>();
            vm.EditorNames = new Dictionary<string, string>();
            vm.EditorColors = staffColorMap;

            double total = 0;
            double totalOT = 0;
            double totalNight = 0;

            int daysInMonth = DateTime.DaysInMonth(vm.SelectedYear, vm.SelectedMonth);

            for (int i = 1; i <= daysInMonth; i++)
            {
                if (selectedWeek == 1 && (i < 1 || i > 7)) continue;
                if (selectedWeek == 2 && (i < 8 || i > 14)) continue;
                if (selectedWeek == 3 && (i < 15 || i > 21)) continue;
                if (selectedWeek == 4 && (i < 22 || i > 28)) continue;
                if (selectedWeek == 5 && i < 29) continue;

                var date = new DateTime(vm.SelectedYear, vm.SelectedMonth, i);
                var work = dbWorks.FirstOrDefault(w => w.WorkDate.Date == date);
                string dateKey = date.ToString("yyyyMMdd");

                double dailyTotal = 0;
                double dailyOT = 0;
                double dailyNight = 0;

                if (work != null)
                {
                    vm.AttendanceList.Add(work);

                    bool isPaidLeave = work.Remarks != null && work.Remarks.Contains("[有給:");

                    if (isPaidLeave)
                    {
                        dailyTotal = 8.0;
                    }
                    else if (work.AttendanceTime.HasValue && work.LeaveTime.HasValue)
                    {
                        // 総労働時間
                        dailyTotal = (work.LeaveTime.Value - work.AttendanceTime.Value - (work.RestTime ?? TimeSpan.Zero)).TotalHours;
                        if (dailyTotal < 0) dailyTotal = 0;

                        // 🌟 追加：残業時間（8時間を超えた分）
                        dailyOT = dailyTotal > 8.0 ? dailyTotal - 8.0 : 0;

                        // 🌟 追加：深夜労働（22:00 〜 翌05:00の重複時間を計算）
                        DateTime a = work.AttendanceTime.Value;
                        DateTime l = work.LeaveTime.Value;

                        DateTime p1Start = a.Date.AddDays(-1).AddHours(22);
                        DateTime p1End = a.Date.AddHours(5);
                        DateTime p2Start = a.Date.AddHours(22);
                        DateTime p2End = a.Date.AddDays(1).AddHours(5);

                        double nightOverlap = 0;
                        if (a < p1End && l > p1Start)
                        {
                            var os = a > p1Start ? a : p1Start;
                            var oe = l < p1End ? l : p1End;
                            nightOverlap += (oe - os).TotalHours;
                        }
                        if (a < p2End && l > p2Start)
                        {
                            var os = a > p2Start ? a : p2Start;
                            var oe = l < p2End ? l : p2End;
                            nightOverlap += (oe - os).TotalHours;
                        }
                        dailyNight = Math.Max(0, nightOverlap);
                    }

                    if (work.UpdatedId.HasValue && work.UpdatedId.Value != 0 && staffLastNameMap.ContainsKey((int)work.UpdatedId.Value))
                        vm.EditorNames[dateKey] = staffLastNameMap[(int)work.UpdatedId.Value];
                    else
                        vm.EditorNames[dateKey] = "-";
                }
                else
                {
                    vm.AttendanceList.Add(new WorkModel { Id = 0, StaffCd = vm.TargetStaffCd, WorkDate = date, RestTime = TimeSpan.FromMinutes(60) });
                    vm.EditorNames[dateKey] = "-";
                }

                vm.DailyOvertime[dateKey] = Math.Round(dailyOT, 2);
                vm.DailyNightHours[dateKey] = Math.Round(dailyNight, 2);

                total += dailyTotal;
                totalOT += dailyOT;
                totalNight += dailyNight;
            }

            vm.TotalHours = Math.Round(total, 2);
            vm.TotalOvertimeHours = Math.Round(totalOT, 2);
            vm.TotalNightHours = Math.Round(totalNight, 2);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateWorkRecord(
            long workId, string? attendanceTimeStr, string? leaveTimeStr, string? restTimeStr,
            string? statusType, string? lateReason, string? earlyReason, string? absenceReason, string? paidLeaveType,
            int year, int month, int day, int week, string targetStaffCd)
        {
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            var loginUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
            bool isManager = loginUser?.ManagerFlag == 1;

            WorkModel work;
            bool isNew = false;

            if (workId == 0)
            {
                work = new WorkModel { StaffCd = targetStaffCd, WorkDate = new DateTime(year, month, day), RegistrationTime = DateTime.Now, RegistrantId = loginUser?.Id ?? 0 };
                isNew = true;
            }
            else
            {
                work = await _context.Works.FindAsync(workId);
                if (work == null) return RedirectToAction("WorkList", new { SelectedYear = year, SelectedMonth = month, SelectedWeek = week, targetStaffCd = targetStaffCd });
            }

            string newRemarks = "";
            if (statusType == "late") newRemarks = $"[遅刻:{lateReason ?? ""}]";
            else if (statusType == "early") newRemarks = $"[早退:{earlyReason ?? ""}]";
            else if (statusType == "absence") newRemarks = $"[欠勤:{absenceReason ?? ""}]";
            else if (statusType == "paidLeave") newRemarks = $"[有給:{paidLeaveType ?? "全日"}]";

            work.Remarks = newRemarks;
            work.UpdatedTime = DateTime.Now;
            work.UpdatedId = loginUser?.Id ?? 0;

            if (isManager)
            {
                if (TimeSpan.TryParse(attendanceTimeStr, out var at)) work.AttendanceTime = work.WorkDate.Date.Add(at);
                else if (string.IsNullOrEmpty(attendanceTimeStr)) work.AttendanceTime = null;

                if (TimeSpan.TryParse(leaveTimeStr, out var lt))
                {
                    work.LeaveTime = work.WorkDate.Date.Add(lt);
                    // 🌟 追加：日またぎ（20:00出勤〜02:00退勤など）の保存に対応！
                    if (work.AttendanceTime.HasValue && work.LeaveTime < work.AttendanceTime)
                    {
                        work.LeaveTime = work.LeaveTime.Value.AddDays(1);
                    }
                }
                else if (string.IsNullOrEmpty(leaveTimeStr)) work.LeaveTime = null;

                if (TimeSpan.TryParse(restTimeStr, out var rt)) work.RestTime = rt;
                else if (string.IsNullOrEmpty(restTimeStr)) work.RestTime = null;
            }
            else if (isNew)
            {
                work.RestTime = TimeSpan.FromMinutes(60);
            }

            if (isNew)
            {
                if (work.AttendanceTime.HasValue || work.LeaveTime.HasValue || !string.IsNullOrEmpty(work.Remarks))
                {
                    _context.Works.Add(work); await _context.SaveChangesAsync();
                }
            }
            else
            {
                _context.Works.Update(work); await _context.SaveChangesAsync();
            }

            return RedirectToAction("WorkList", new { SelectedYear = year, SelectedMonth = month, SelectedWeek = week, targetStaffCd = targetStaffCd });
        }
    }
}