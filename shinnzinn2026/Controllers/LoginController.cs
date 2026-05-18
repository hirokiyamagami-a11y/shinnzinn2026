using Microsoft.AspNetCore.Mvc;
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

        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Clear();
            return View();
        }

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

        [HttpPost]
        public IActionResult AuthenticateDetails(string staffCd, string password)
        {
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

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

        // ---------------------------------------------------------
        // ① 画面を開く（基本は共通モーダル内で動くため、予備用です）
        // ---------------------------------------------------------
        [HttpGet]
        public IActionResult Register()
        {
            return View("Login");
        }

        // ---------------------------------------------------------
        // ② データを保存する（共通モーダルからPOST送信されたときに動く）
        // ---------------------------------------------------------
        [HttpPost]
        public IActionResult Register(string staffCd, string name, string password)
        {
            // ① 社員CDの重複チェック
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