using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Models;
using System.Reflection.Emit;

namespace shinnzinn2026.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Staff> Staffs { get; set; }

        // もしテーブル名やカラム名を細かく調整したいならここに追加するけど、
        // 今はモデルに [Table("staff")] とか書いてるから、ここは空っぽで大丈夫よ
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
