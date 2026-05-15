using shinnzinn2026.Models;
using System.Collections.Generic;

namespace shinnzinn2026.ViewModels
{
    public class WorkListViewModel
    {
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }

        public string UserStaffCd { get; set; } = "";
        public string UserName { get; set; } = "";

        public List<WorkModel> AttendanceList { get; set; } = new();
        public double TotalHours { get; set; }

        // ↓↓↓ 今回のエラーの原因：この2行が足りていませんでした！ ↓↓↓
        public bool IsAuthenticated { get; set; } = false;
        public string? LoginErrorMessage { get; set; }
    }
}