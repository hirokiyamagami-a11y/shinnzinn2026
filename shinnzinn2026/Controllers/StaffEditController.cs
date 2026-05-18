using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Data;

namespace shinnzinn2026.Controllers
{
    public class StaffEditController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StaffEditController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. 画面を「表示する（GET）」ための扉よ
        [HttpGet]
        public async Task<IActionResult> StaffEdit(string id)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == id);
            if (staff == null) return NotFound();

            return View(staff);
        }

        // 2. 画面から「データを受け取って保存する（POST）」ための扉
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
                    ViewBag.ErrorMessage = "パスワードが一致しません。\nもう一度確認してください。";
                    return View(staff);
                }
                staff.Password = Password;
            }

            // 名前と更新日時を上書き
            staff.Name = Name;
            staff.UpdatedTime = DateTime.Now;

            string? loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (!string.IsNullOrEmpty(loginStaffCd))
            {
                var editor = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);

                if (editor != null)
                {
                    staff.UpdatedId = editor.Id;
                }
            }
            // データベースに変更を確定（保存）
            await _context.SaveChangesAsync();

            // データベースに変更を確定（保存）させる
            await _context.SaveChangesAsync();

            // 成功メッセージを持たせて画面を返す
            ViewBag.SuccessMessage = "変更を保存しました";
            return View(staff);
        }

        // ユーザーをデータベースから完全に削除（物理削除）するアクション
        [HttpPost]
        public async Task<IActionResult> StaffDelete(string StaffCd)
        {
            // 1. 削除したい社員を探す
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == StaffCd);
            if (staff == null) return NotFound();

            // 2. データベースから完全に削除する（DELETE処理）
            _context.Staffs.Remove(staff);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{staff.Name} さんの削除が完了しました。";

            // 3. 削除が終わったら、一覧画面へ戻る
            return RedirectToAction("StaffList", "StaffList");
        }
    }
}