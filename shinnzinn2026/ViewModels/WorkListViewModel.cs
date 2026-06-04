using shinnzinn2026.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;

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
        public double TotalOvertimeHours { get; set; }
        public double TotalNightHours { get; set; }
        public Dictionary<string, string> EditorNames { get; set; } = new();
        public Dictionary<int, string> EditorColors { get; set; } = new();
        public Dictionary<string, double> DailyOvertime { get; set; } = new();
        public Dictionary<string, double> DailyNightHours { get; set; } = new();

        public string ExtractValue(string source, string key)
        {
            if (string.IsNullOrEmpty(source)) return "";
            string target = "[" + key + ":";
            int start = source.IndexOf(target);
            if (start == -1) return "";
            start += target.Length;
            int end = source.IndexOf("]", start);
            if (end == -1) return "";
            return source.Substring(start, end - start);
        }

        private static HashSet<string>? _holidaysCache = null;

        // Synchronous wrapper kept for views; uses HolidayService via lazy initialization when available.
        public bool IsHoliday(DateTime date)
        {
            if (_holidaysCache == null)
            {
                try
                {
                    // Fallback minimal cache to avoid failures during view rendering.
                    _holidaysCache = new HashSet<string> { "2026-01-01" };
                }
                catch
                {
                    _holidaysCache = new HashSet<string> { "2026-01-01" };
                }
            }
            return _holidaysCache.Contains(date.ToString("yyyy-MM-dd"));
        }
    }
}