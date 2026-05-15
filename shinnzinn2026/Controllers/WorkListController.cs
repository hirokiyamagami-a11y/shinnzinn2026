using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using shinnzinn2026.Work_List;

namespace shinnzinn2026.Controllers
{
    public class WorkListController : Controller
    {
        private readonly ApplicationDbContext _context;

        // データベースを使えるように準備
        public WorkListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // URL: /WorkList/WorkList
        public async Task<IActionResult> WorkList(int? SelectedYear, int? SelectedMonth)
        {
            var year = SelectedYear ?? DateTime.Now.Year;
            var month = SelectedMonth ?? DateTime.Now.Month;

            // --- データベースからデータを取得 ---
            var attendanceData = await _context.Works
                .Where(w => w.WorkDate.Year == year && w.WorkDate.Month == month)
                // 本来はここでログイン中の StaffCd でも絞り込みます
                // .Where(w => w.StaffCd == "ログインユーザーのCD") 
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            // 画面に渡すモデルにセット
            var viewModel = new WorkListViewModel
            {
                SelectedYear = year,
                SelectedMonth = month,
                AttendanceList = attendanceData
            };

            return View(viewModel);
        }
    }
}