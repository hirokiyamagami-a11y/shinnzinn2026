using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.ViewModels;
using Microsoft.AspNetCore.Http;
using System;
using System.Linq;
using System.Threading.Tasks;

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
        public async Task<IActionResult> WorkList(int? SelectedYear, int? SelectedMonth)
        {
            // ================================================================================
            // 【⚠️ 開発用メモ：強制ログイン設定場所 ⚠️】
            // 
            // 本番環境に移行する際は、下の「 ?? "S004" 」の部分を削除してください。
            // ================================================================================

            // 💡 ここを「S0004」からデータベース通りの「S004」に修正しました！
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd") ?? "S004";

            var viewModel = new WorkListViewModel
            {
                SelectedYear = SelectedYear ?? DateTime.Now.Year,
                SelectedMonth = SelectedMonth ?? DateTime.Now.Month,
                IsAuthenticated = !string.IsNullOrEmpty(loginStaffCd)
            };

            if (viewModel.IsAuthenticated)
            {
                await SetUserAttendanceData(viewModel, loginStaffCd!);
            }

            return View(viewModel);
        }

        [HttpPost]
        public async Task<IActionResult> LoginFromModal(string staffCd, string password, int year, int month)
        {
            var user = await _context.Staffs
                .FirstOrDefaultAsync(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (user != null)
            {
                HttpContext.Session.SetString("LoginStaffCd", user.StaffCd);
                return RedirectToAction("WorkList", new { SelectedYear = year, SelectedMonth = month });
            }

            var viewModel = new WorkListViewModel
            {
                SelectedYear = year,
                SelectedMonth = month,
                IsAuthenticated = false,
                LoginErrorMessage = "社員CDまたはパスワードが違います。"
            };
            return View("WorkList", viewModel);
        }

        private async Task SetUserAttendanceData(WorkListViewModel vm, string staffCd)
        {
            // DBから「S004」で検索をかけます
            var user = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == staffCd);

            vm.UserStaffCd = staffCd;

            // 💡 ここも修正：DBから正しく引っ張れた場合は「高橋 次郎」が入ります。
            // 万が一見つからなかった場合のエラーメッセージに変えました。
            vm.UserName = user?.Name ?? "DBに存在しないユーザー";

            vm.AttendanceList = await _context.Works
                .Where(w => w.StaffCd == staffCd &&
                            w.WorkDate.Year == vm.SelectedYear &&
                            w.WorkDate.Month == vm.SelectedMonth)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            double total = 0;
            foreach (var item in vm.AttendanceList)
            {
                if (item.AttendanceTime.HasValue && item.LeaveTime.HasValue)
                {
                    var diff = item.LeaveTime.Value - item.AttendanceTime.Value - (item.RestTime ?? TimeSpan.Zero);
                    total += diff.TotalHours;
                }
            }
            vm.TotalHours = Math.Round(total, 2);
        }
    }
}