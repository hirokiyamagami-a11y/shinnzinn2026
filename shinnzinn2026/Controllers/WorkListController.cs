using Microsoft.AspNetCore.Mvc;
using shinnzinn2026.Work_List; // モデルを参照

namespace shinnzinn2026.Controllers
{
    public class WorkListController : Controller
    {
        // メソッド名を WorkList にしました。
        // これで URL は /WorkList/WorkList になります。
        public IActionResult WorkList(int? SelectedYear, int? SelectedMonth)
        {
            var model = new WorkListModel();

            // 検索された値をセット（空なら現在の年月）
            model.SelectedYear = SelectedYear ?? DateTime.Now.Year;
            model.SelectedMonth = SelectedMonth ?? DateTime.Now.Month;

            // 表示用のダミーデータを作成
            model.AttendanceList = new List<AttendanceRecord>();
            for (int i = 1; i <= 5; i++)
            {
                model.AttendanceList.Add(new AttendanceRecord
                {
                    Date = new DateTime(model.SelectedYear, model.SelectedMonth, i),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    BreakTime = 60,
                    Note = "通常勤務"
                });
            }

            // Views/WorkList/WorkList.cshtml を探しに行きます
            return View(model);
        }
    }
}