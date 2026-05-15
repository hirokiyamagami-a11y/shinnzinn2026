using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shinnzinn2026.Models
{
    [Table("work")]
    [Comment("勤務テーブル")]
    public class Work
    {
        [Key]
        [Column("id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // 自動採番に対応
        public long Id { get; set; }

        [Column("staff_id")]
        [Comment("社員ID（参照用）")]
        public long? StaffId { get; set; } // NOT NULL制約がないため「?」をつけてNull許容にしています

        [Required]
        [Column("staff_cd")]
        [MaxLength(10)]
        [Comment("社員CD（紐づけキー）")]
        public string StaffCd { get; set; }

        [Required]
        [Column("work_date", TypeName = "date")] // PostgreSQLの 'date' 型に合わせる
        [Comment("更新日付")] // ※SQLのコメント通りにしていますが、意味合いは「勤務日」ですね
        public DateTime WorkDate { get; set; }

        [Column("attendance_time")]
        [Comment("出勤時間")]
        public DateTime? AttendanceTime { get; set; }

        [Column("leave_time")]
        [Comment("退勤時間")]
        public DateTime? LeaveTime { get; set; }

        [Column("rest_time")]
        [Comment("休憩時間")]
        public TimeSpan? RestTime { get; set; } // PostgreSQLの 'interval' 型は C#の TimeSpan に対応

        [Column("remarks")]
        [Comment("備考欄")]
        public string? Remarks { get; set; } // 'text' 型は上限なしの string として扱います

        [Column("registration_time")]
        [Comment("登録日時")]
        public DateTime? RegistrationTime { get; set; }

        [Column("registrant_id")]
        [Comment("登録者ID")]
        public long? RegistrantId { get; set; }

        [Column("updated_time")]
        [Comment("更新日時")]
        public DateTime? UpdatedTime { get; set; }

        [Column("updated_id")]
        [Comment("更新者ID")]
        public long? UpdatedId { get; set; }

        [ForeignKey("StaffCd")]
        public virtual Staff? Staff { get; set; }
    }
}