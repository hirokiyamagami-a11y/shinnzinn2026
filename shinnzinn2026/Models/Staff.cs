using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace shinnzinn2026.Models
{
    [Table("staff")]
    public class Staff
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        [Column("staff_cd")]
        public string StaffCd { get; set; } = "";

        [Column("password")]
        public string Password { get; set; } = "";

        [Column("name")]
        public string? Name { get; set; } // DEFAULT NULL だから ? をつける

        [Column("delete_flag")]
        public short DeleteFlag { get; set; }

        [Column("manager_flag")]
        public short ManagerFlag { get; set; }

        [Column("registration_time")]
        public DateTime? RegistrationTime { get; set; }

        [Column("registrant_id")]
        public long? RegistrantId { get; set; }

        [Column("updated_time")]
        public DateTime? UpdatedTime { get; set; }

        [Column("updated_id")]
        public long? UpdatedId { get; set; }
    }
}
