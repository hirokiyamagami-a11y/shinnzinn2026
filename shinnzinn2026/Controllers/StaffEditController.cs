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
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == StaffCd);
            if (staff == null) return NotFound();

            // 1. 削除した人（ログイン中のユーザー）のIDを記録する
            string? loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (!string.IsNullOrEmpty(loginStaffCd))
            {
                var editor = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
                if (editor != null)
                {
                    staff.UpdatedId = editor.Id;
                }
            }

            // 2. 物理削除(Remove)をやめて、論理削除(フラグを立てる)に変更！
            staff.DeleteFlag = 1;

            staff.UpdatedTime = DateTime.Now;

            // 3. データベースに変更を確定（更新）
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{staff.Name} さんの削除が完了しました。";
            return RedirectToAction("StaffList", "StaffList");
        }
    }
}