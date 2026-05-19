using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;   // ApplicationDbContext がある場所

namespace shinnzinn2026.Controllers
{
    public class StaffListController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffListController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 引数に「page」を追加。何も指定されない時は1ページ目（page = 1）になる
        // 引数に「page」を追加。何も指定されない時は1ページ目（page = 1）になる
        public async Task<IActionResult> StaffList(int page = 1)
        {
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (string.IsNullOrEmpty(loginStaffCd)) return RedirectToAction("Login", "Login");

            var loginUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
            if (loginUser == null || loginUser.ManagerFlag != 1)
            {
                return RedirectToAction("Login", "Login");
            }

            try
            {
                int pageSize = 5;

                int totalItems = await _context.Staffs
                    .Where(s => s.DeleteFlag == 0)
                    .CountAsync();

                int totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

                // 指定されたページの5人だけを切り取って取得
                var staffList = await _context.Staffs
                    .Where(s => s.DeleteFlag == 0) // 削除されていない人間のみ表示
                    .OrderBy(s => s.Id)
                    .Skip((page - 1) * pageSize)  // 前のページまでの分をスキップ
                    .Take(pageSize)               // 5件だけ取得
                    .ToListAsync();

                // 画面（View）でボタンを作るために、ページ情報を「ViewBag」に入れて送る
                ViewBag.CurrentPage = page;
                ViewBag.TotalPages = totalPages;

                return View("StaffList", staffList);
            }
            catch (Exception ex)
            {
                return Content($"エラー発生：{ex.Message}");
            }
        }
    }
}
