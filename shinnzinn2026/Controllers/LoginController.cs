using Microsoft.AspNetCore.Mvc;

namespace shinnzinn2026.Controllers
{
    // Controllerを継承させる必要があります
    public class LoginController : Controller
    {
        // 画面の初期表示 (GET)
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        // 出勤ボタンが押された時の処理 (POST)
        [HttpPost]
        public IActionResult CheckIn()
        {
            // ここに出勤の処理を書く
            return View("Login"); // 処理後に同じ画面に戻す
        }

        // 退勤ボタンが押された時の処理 (POST)
        [HttpPost]
        public IActionResult CheckOut()
        {
            // ここに退勤の処理を書く
            return View("Login"); // 処理後に同じ画面に戻す
        }
    }
}