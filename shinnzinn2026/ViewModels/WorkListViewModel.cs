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
        public double TotalHours { get; set; }

        // 🌟 苗字を格納する箱 (Key: 日付文字列)
        public Dictionary<string, string> EditorNames { get; set; } = new Dictionary<string, string>();

        // 🌟 編集者ごとの色を格納する箱 (Key: スタッフID)
        public Dictionary<int, string> EditorColors { get; set; } = new Dictionary<int, string>();
    }
}