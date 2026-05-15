using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using shinnzinn2026.ViewModels; // 新しく作った ViewModel を使う
using System;
using System.Collections.Generic;
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

        // URL: /WorkList/WorkList
        public async Task<IActionResult> WorkList(int? SelectedYear, int? SelectedMonth)
        {
            // 年月が指定されていなければ現在の年月を入れる
            var year = SelectedYear ?? DateTime.Now.Year;
            var month = SelectedMonth ?? DateTime.Now.Month;

            // 1. データベースから該当する年月のデータを取得
            var attendanceData = await _context.Works
                .Where(w => w.WorkDate.Year == year && w.WorkDate.Month == month)
                .OrderBy(w => w.WorkDate)
                .ToListAsync();

            // 2. ViewModel に詰め替える
            var viewModel = new WorkListViewModel
            {
                SelectedYear = year,
                SelectedMonth = month,
                AttendanceList = attendanceData
            };

            // 3. 画面に渡す
            return View(viewModel);
        }
    }
}