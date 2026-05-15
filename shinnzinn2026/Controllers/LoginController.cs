using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // 💡 追加：セッション（バトン）を使うために必要
using shinnzinn2026.Data;
using shinnzinn2026.Models;

namespace shinnzinn2026.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌟 1. ログイン（打刻）画面を表示
        [HttpGet]
        public IActionResult Login()
        {
            // 💡 追加：ログイン画面を開いた時は念のため以前のバトンを捨てる（安全対策）
            HttpContext.Session.Clear();
            return View();
        }

        // 🌟 2. 「出勤」ボタン処理 （※一切変更していません）
        [HttpPost]
        public IActionResult CheckIn(string staffCd, string password)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            var today = DateTime.Today;
            var existWork = _context.Works.FirstOrDefault(w => w.StaffCd == staffCd && w.WorkDate == today);

            if (existWork != null)
            {
                ViewBag.ErrorMessage = "既に本日の出勤が行われています！";
                return View("Login");
            }

            var newWork = new WorkModel
            {
                StaffCd = staff.StaffCd,
                WorkDate = today,
                AttendanceTime = DateTime.Now,
                RegistrationTime = DateTime.Now,
                RegistrantId = staff.Id
            };

            _context.Works.Add(newWork);
            _context.SaveChanges();

            ViewBag.Message = $"{staff.Name} さん、おはようございます。\n出勤を記録しました。";
            return View("Login");
        }

        // 🌟 3. 「退勤」ボタン処理 （※一切変更していません）
        [HttpPost]
        public IActionResult CheckOut(string staffCd, string password)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            var today = DateTime.Today;
            var work = _context.Works.FirstOrDefault(w => w.StaffCd == staffCd && w.WorkDate == today);

            if (work == null)
            {
                ViewBag.ErrorMessage = "本日の出勤記録が見つかりません。\n先に出勤をしてください。";
                return View("Login");
            }

            if (work.LeaveTime != null)
            {
                ViewBag.ErrorMessage = "既に退勤が行われています！";
                return View("Login");
            }

            work.LeaveTime = DateTime.Now;
            work.UpdatedTime = DateTime.Now;
            work.UpdatedId = staff.Id;

            _context.Works.Update(work);
            _context.SaveChanges();

            ViewBag.Message = $"{staff.Name} さん、お疲れ様でした。\n退勤を記録しました。";
            return View("Login");
        }

        // 🌟 4. 「詳細」ボタンからの認証・振り分け処理
        [HttpPost]
        public IActionResult AuthenticateDetails(string staffCd, string password)
        {
            // 認証チェック
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            // 💡 ここだけ追加！実績画面に「誰がログインしたか」を教えるためのバトン
            HttpContext.Session.SetString("LoginStaffCd", staff.StaffCd);

            // ManagerFlag による判定 (1: 管理者, 0: 一般)
            if (staff.ManagerFlag == 1)
            {
                // 管理者なら StaffListController の StaffList アクションへ
                return RedirectToAction("StaffList", "StaffList");
            }
            else
            {
                // 一般社員なら WorkListController の WorkList アクションへ
                return RedirectToAction("WorkList", "WorkList");
            }
        }
    }
}