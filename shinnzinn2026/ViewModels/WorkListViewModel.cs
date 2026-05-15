using shinnzinn2026.Models;

namespace shinnzinn2026.Work_List
{
    public class WorkListViewModel
    {
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        // データベースから取得した WorkModel のリストを保持します
        public List<WorkModel> AttendanceList { get; set; } = new();
    }
}