using Microsoft.AspNetCore.Mvc;
using shinnzinn2026.Data;   // ApplicationDbContextがある場所（環境に合わせて変更）
using shinnzinn2026.Models; // Modelがある場所

namespace shinnzinn2026.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        // DBに接続するための準備
        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 🌟 1. ログイン（打刻）画面を表示する処理
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 🌟 2. 「出勤」ボタンが押された時の処理
        [HttpPost]
        public IActionResult CheckIn(string staffCd, string password)
        {
            // ① 社員CDとパスワードが合っているか（かつ削除されていないか）チェック！
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                // 認証失敗：エラーメッセージを画面に返す
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            // ② 今日すでに打刻していないかチェック！
            var today = DateTime.Today;
            var existWork = _context.Works.FirstOrDefault(w => w.StaffCd == staffCd && w.WorkDate == today);

            if (existWork != null)
            {
                // 二重打刻防止：すでにデータがあればエラーにする
                ViewBag.ErrorMessage = "既に本日の出勤が行われています！";
                return View("Login");
            }

            // ③ データベース（Workテーブル）に新しいデータを登録！
            var newWork = new WorkModel
            {
                StaffCd = staff.StaffCd,
                WorkDate = today,
                AttendanceTime = DateTime.Now, // 今の時間を「出勤時間」に入れる！
                RegistrationTime = DateTime.Now,
                RegistrantId = staff.Id
            };

            _context.Works.Add(newWork);
            _context.SaveChanges(); // ここで実際にDBに保存されます！

            // 成功メッセージを画面に渡す
            ViewBag.Message = $"{staff.Name} さん、おはようございます。\n出勤を記録しました。";
            return View("Login");
        }

        // 🌟 3. 「退勤」ボタンが押された時の処理
        [HttpPost]
        public IActionResult CheckOut(string staffCd, string password)
        {
            // ① 認証チェック
            var staff = _context.Staffs.FirstOrDefault(s => s.StaffCd == staffCd && s.Password == password && s.DeleteFlag == 0);

            if (staff == null)
            {
                ViewBag.ErrorMessage = "社員CDまたはパスワードが間違っています。";
                return View("Login");
            }

            // ② 今日の「出勤データ」を探す
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

            // ③ データベース（Workテーブル）を更新！
            work.LeaveTime = DateTime.Now; // 今の時間を「退勤時間」に入れる！
            work.UpdatedTime = DateTime.Now;
            work.UpdatedId = staff.Id;

            _context.Works.Update(work);
            _context.SaveChanges(); // 変更を保存！

            // 成功メッセージを画面に渡す
            ViewBag.Message = $"{staff.Name} さん、お疲れ様でした。\n退勤を記録しました。";
            return View("Login");
        }
    }
}