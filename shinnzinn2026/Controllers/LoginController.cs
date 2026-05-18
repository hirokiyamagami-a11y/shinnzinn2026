using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http; // 🌟 必須：セッション（バトン）を使うために追加
using shinnzinn2026.Data;
using shinnzinn2026.Models;
using System;
using System.Linq;

namespace shinnzinn2026.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // --- ログイン（打刻）画面を表示 ---
        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

        // --- 「出勤」ボタン処理 ---
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

        // --- 「退勤」ボタン処理 ---
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

            // 🌟 修正：手動編集データと色を分けるため、自動打刻時はあえて UpdatedId を入れません
            // work.UpdatedId = staff.Id; 

            _context.Works.Update(work);
            _context.SaveChanges();

            ViewBag.Message = $"{staff.Name} さん、お疲れ様でした。\n退勤を記録しました。";
            return View("Login");
        }

        // --- 「詳細」ボタンからの認証・振り分け処理 ---
        [HttpPost]
        public IActionResult AuthenticateDetails(string staffCd, string password)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            // 🌟 必須：これで実績画面に「誰がログインしたか」を教えます
            HttpContext.Session.SetString("LoginStaffCd", staff.StaffCd);

            if (staff.ManagerFlag == 1)
            {
                return RedirectToAction("StaffList", "StaffList");
            }
            else
            {
                return RedirectToAction("WorkList", "WorkList");
            }
        }

        // --- 新規登録画面を開く ---
        [HttpGet]
        public IActionResult Register()
        {
            return View("Login");
        }

        // --- 新規ユーザーを保存する ---
        [HttpPost]
        public IActionResult Register(string staffCd, string name, string password)
        {
            var existStaff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.DeleteFlag == 0);
            if (existStaff != null)
            {
                ViewBag.ErrorMessage = "この社員CDは既に登録されています。";
                return View("Login");
            }

            var newStaff = new StaffModel
            {
                StaffCd = staffCd,
                Name = name,
                Password = password,
                ManagerFlag = 0,
                DeleteFlag = 0,
                RegistrationTime = DateTime.Now
            };

            _context.Staffs.Add(newStaff);
            _context.SaveChanges();

            newStaff.RegistrantId = newStaff.Id;
            _context.SaveChanges();

            ViewBag.Message = $"{newStaff.Name} さんの登録が完了しました！";
            return View("Login");
        }
    }
}