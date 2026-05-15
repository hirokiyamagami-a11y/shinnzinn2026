using shinnzinn2026.Models;
using System.Collections.Generic;

namespace shinnzinn2026.ViewModels
{
    public class WorkListViewModel
    {
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }

        // データベースから取得した WorkModel のリスト
        public List<WorkModel> AttendanceList { get; set; } = new List<WorkModel>();
    }
}