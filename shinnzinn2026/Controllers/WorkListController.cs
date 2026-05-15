using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.ViewModels;
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

        // Login画面のボタンが探しに来る「WorkList」という名前の入り口
        [HttpGet]
        public async Task<IActionResult> WorkList(int? SelectedYear, int? SelectedMonth)
        {
            var year = SelectedYear ?? DateTime.Now.Year;
            var month = SelectedMonth ?? DateTime.Now.Month;

            // データベースからデータ取得
            var attendanceData = await _context.Works
                .Where(w => w.WorkDate.Year == year && w.WorkDate.Month == month)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            var viewModel = new WorkListViewModel
            {
                SelectedYear = year,
                SelectedMonth = month,
                AttendanceList = attendanceData
            };

            // 【重要】Views/WorkList/WorkList.cshtml を表示
            return View(viewModel);
        }
    }
}