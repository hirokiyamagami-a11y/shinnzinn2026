using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;
using shinnzinn2026.Models;

namespace shinnzinn2026.Controllers
{
    public class StaffEditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffEditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌸 1. 画面を「表示する（GET）」ための扉よ！
        // [HttpGet] をつけることで、「リンクから飛んできた時はこっちを開けてね」と明示できるわ。
        [HttpGet]
        public async Task<IActionResult> StaffEdit(string id)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // 🌸 2. 画面から「データを受け取って保存する（POST）」ための扉よ！
        // [HttpPost] があるから、「保存ボタンを押した時」だけこっちが呼ばれるわ。
        [HttpPost]
        public async Task<IActionResult> StaffEdit(string StaffCd, string Name, string Password, string PasswordConfirm)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == StaffCd);
            if (staff == null) return NotFound();

            // パスワードの入力チェック
            if (!string.IsNullOrEmpty(Password) || !string.IsNullOrEmpty(PasswordConfirm))
            {
                if (Password != PasswordConfirm)
                {
                    ViewBag.ErrorMessage = "パスワードが一致しません！もう一度確認しなさい！";
                    return View(staff);
                }
                staff.Password = Password;
            }

            // 名前と更新日時を上書き
            staff.Name = Name;
            staff.UpdatedTime = DateTime.Now;

            // データベースに変更を確定（保存）させる
            await _context.SaveChangesAsync();

            // 成功メッセージを持たせて画面を返す
            ViewBag.SuccessMessage = "変更を保存しました";
            return View(staff);
        }
    }
}