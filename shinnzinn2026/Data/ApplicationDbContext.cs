using Microsoft.EntityFrameworkCore;
using shinnzinn2026.Models; // 作成したStaffModelやWorkModelを使うために必須

namespace shinnzinn2026.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // ここでモデルとデータベースのテーブルを紐づけます
        public DbSet<StaffModel> Staffs { get; set; }
        public DbSet<WorkModel> Works { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // 【エラー防止策】
            // モデル側の [Table] や [Key] 属性だけだと認識漏れが起きることがあるため、
            // ここで「どのテーブルの」「どのカラムが主キーか」を念押しで設定します。

            // staffテーブルの明示的な設定
            modelBuilder.Entity<StaffModel>()
                .ToTable("staff")
                .HasKey(e => e.Id);

            // workテーブルの明示的な設定
            modelBuilder.Entity<WorkModel>()
                .ToTable("work")
                .HasKey(e => e.Id);
        }
    }
}