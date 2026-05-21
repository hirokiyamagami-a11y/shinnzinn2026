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

        // 画面を「表示する（GET）」ための扉
        [HttpGet]
        public async Task<IActionResult> StaffEdit(string id)
        {
            var loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (string.IsNullOrEmpty(loginStaffCd)) return RedirectToAction("Login", "Login");

            var loginUser = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
            if (loginUser == null || loginUser.ManagerFlag != 1) return RedirectToAction("Login", "Login");

            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == id);
            if (staff == null) return NotFound();

            return View(staff);
        }      

         //  画面から「データを受け取って保存する（POST）」ための扉
        [HttpPost]
        public async Task<IActionResult> StaffEdit(string StaffCd, string Name, string Password, string PasswordConfirm)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == StaffCd);
            if (staff == null) return NotFound();

            // 文字数オーバーのチェック（エラーなら画面を戻す）
            if (!string.IsNullOrWhiteSpace(Name) && Name.Length > 24)
            {
                ViewBag.ErrorMessage = "氏名は24文字以内で入力してください。";
                return View(staff); // 変更前の状態のまま画面を返す
            }

            if (!string.IsNullOrEmpty(Password) && Password.Length > 10)
            {
                ViewBag.ErrorMessage = "パスワードは10文字以内で入力してください。";
                return View(staff);
            }

            // パスワードの入力チェック（入力されている時だけ変更）
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

            // 成功メッセージを持たせて画面を返す
            ViewBag.SuccessMessage = "変更を保存しました";
            return View(staff);
        }

        // ユーザーをデータベースから削除
        [HttpPost]
        public async Task<IActionResult> StaffDelete(string StaffCd)
        {
            var staff = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == StaffCd);
            if (staff == null) return NotFound();

            //  削除した人（ログイン中のユーザー）のIDを記録する
            string? loginStaffCd = HttpContext.Session.GetString("LoginStaffCd");
            if (!string.IsNullOrEmpty(loginStaffCd))
            {
                var editor = await _context.Staffs.FirstOrDefaultAsync(s => s.StaffCd == loginStaffCd);
                if (editor != null)
                {
                    staff.UpdatedId = editor.Id;
                }
            }

            // 論理削除
            staff.DeleteFlag = 1;

            staff.UpdatedTime = DateTime.Now;

            //  データベースに変更を確定（更新）
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{staff.Name} さんの削除が完了しました。";
            return RedirectToAction("StaffList", "StaffList");
        }
    }
}