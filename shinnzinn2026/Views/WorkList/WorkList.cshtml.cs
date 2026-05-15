using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace shinnzinn2026.Work_List
{
    public class WorkListModel : PageModel
    {
        // [BindProperty] が「画面」と「プログラム」をつなぐブリッジになります
        [BindProperty(SupportsGet = true)]
        public int SelectedYear { get; set; } = DateTime.Now.Year;

        [BindProperty(SupportsGet = true)]
        public int SelectedMonth { get; set; } = DateTime.Now.Month;

        public List<AttendanceRecord> AttendanceList { get; set; } = new();

        // 検索ボタン（submit）が押されると、この OnGet が動きます
        public void OnGet()
        {
            // 一旦リストを空にする
            AttendanceList.Clear();

            // 検索された年月のデータを3日分だけ「偽造」して作る
            for (int i = 1; i <= 3; i++)
            {
                AttendanceList.Add(new AttendanceRecord
                {
                    Date = new DateTime(SelectedYear, SelectedMonth, i),
                    StartTime = new TimeSpan(9, 0, 0),
                    EndTime = new TimeSpan(18, 0, 0),
                    BreakTime = 60,
                    Note = $"{SelectedMonth}月のテストデータ {i}"
                });
            }
        }
    }

    public class AttendanceRecord
    {
        public DateTime Date { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public int BreakTime { get; set; }
        public string Note { get; set; } = "";
    }
}