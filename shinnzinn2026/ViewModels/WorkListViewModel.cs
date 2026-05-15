using shinnzinn2026.Models;
using System.Collections.Generic;

namespace shinnzinn2026.ViewModels
{
    public class WorkListViewModel
    {
        public int SelectedYear { get; set; }
        public int SelectedMonth { get; set; }

        // --- ログインしている人（閲覧者）の情報 ---
        public string LoginStaffCd { get; set; } = "";
        public string LoginUserName { get; set; } = "";
        public bool IsManager { get; set; } = false;

        // --- 今画面に表示されている人（対象者）の情報 ---
        public string TargetStaffCd { get; set; } = "";
        public string TargetUserName { get; set; } = "";

        // 自分自身のデータを見ているかどうかの判定用
        public bool IsViewingSelf => LoginStaffCd == TargetStaffCd;

        public List<WorkModel> AttendanceList { get; set; } = new();
        public double TotalHours { get; set; }
    }
}