using shinnzinn2026.Models;
using System.Collections.Generic;

namespace shinnzinn2026.ViewModels
{
    public class WorkListViewModel
    {
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }
        public string LoginStaffCd { get; set; } = "";
        public string LoginUserName { get; set; } = "";
        public bool IsManager { get; set; } = false;
        public string TargetStaffCd { get; set; } = "";
        public string TargetUserName { get; set; } = "";
        public bool IsViewingSelf => LoginStaffCd == TargetStaffCd;

        public List<WorkModel> AttendanceList { get; set; } = new();

        // 🌟 追加：月間（週間）の各種合計時間
        public double TotalHours { get; set; }
        public double TotalOvertimeHours { get; set; }
        public double TotalNightHours { get; set; }

        public Dictionary<string, string> EditorNames { get; set; } = new();
        public Dictionary<int, string> EditorColors { get; set; } = new();

        // 🌟 追加：日ごとの残業・深夜時間の計算結果をViewに渡す箱
        public Dictionary<string, double> DailyOvertime { get; set; } = new();
        public Dictionary<string, double> DailyNightHours { get; set; } = new();
    }
}