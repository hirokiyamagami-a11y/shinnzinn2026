using Microsoft.EntityFrameworkCore; // ← これを追加（[Comment]を使うために必要）
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shinnzinn2026.Models
{
    [Table("staff")]
    [Comment("社員マスタ")]
    public class StaffModel
    {
        [Key]
        [Column("id")]
        [Comment("社員ID")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // BIGSERIAL（自動採番）に対応
        public int Id { get; set; }

        [Required] // NOT NULL
        [Column("staff_cd")]
        [MaxLength(10)]
        [Comment("社員CD")]
        public string StaffCd { get; set; } = "";

        [Required] // NOT NULL
        [Column("password")]
        [MaxLength(10)]
        [Comment("パスワード")]
        public string Password { get; set; } = "";

        [Column("name")]
        [MaxLength(24)]
        [Comment("氏名")]
        public string? Name { get; set; }

        [Required]
        [Column("delete_flag")]
        [Comment("削除フラグ")]
        public short DeleteFlag { get; set; }

        [Required]
        [Column("manager_flag")]
        [Comment("管理職フラグ")]
        public short ManagerFlag { get; set; }

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
    }
}