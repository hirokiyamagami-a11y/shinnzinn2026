using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shinnzinn2026.Models
{
    [Table("work")]
    public class WorkModel
    {
        // 🌟 新規作成時に休憩時間を 00:00:00 に初期化する設定
        public WorkModel()
        {
            RestTime = TimeSpan.Zero;
        }

        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("staff_cd")]
        public string StaffCd { get; set; } = "";

        // 社員テーブルとの紐付け
        [ForeignKey("StaffCd")]
        public virtual StaffModel? Staff { get; set; }

        [Column("work_date")]
        public DateTime WorkDate { get; set; }

        [Column("attendance_time")]
        public DateTime? AttendanceTime { get; set; }

        [Column("leave_time")]
        public DateTime? LeaveTime { get; set; }

        [Column("rest_time")]
        public TimeSpan? RestTime { get; set; }

        [Column("remarks")]
        public string? Remarks { get; set; }

        [Column("registration_time")]
        public DateTime? RegistrationTime { get; set; }

        // 🌟 修正：StaffModelのIdに合わせて int? に変更
        [Column("registrant_id")]
        public int? RegistrantId { get; set; }

        [ForeignKey("RegistrantId")]
        public virtual StaffModel? Registrant { get; set; }

        [Column("updated_time")]
        public DateTime? UpdatedTime { get; set; }

        // 🌟 修正：StaffModelのIdに合わせて int? に変更
        [Column("updated_id")]
        public int? UpdatedId { get; set; }

        [ForeignKey("UpdatedId")]
        public virtual StaffModel? Updater { get; set; }
    }
}