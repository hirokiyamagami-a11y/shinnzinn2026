using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;   // ApplicationDbContext がある場所
using shinnzinn2026.Models;

namespace shinnzinn2026.Controllers
{
    public class StaffListController : Controller
    {
        public IActionResult StaffList() => View();
        private readonly ApplicationDbContext _context;

        public async Task<IActionResult> Staff_List()
        {
            try
            {
                // DBから社員データを全部取ってくる
                var staffList = await _context.Staffs.OrderBy(s => s.Id).ToListAsync();

                // 取得したリストをViewに渡す
                return View("Staff_List", staffList);
            }
            catch (Exception ex)
            {
                return Content($"エラー発生：{ex.Message}");
            }
        }
    }
}
