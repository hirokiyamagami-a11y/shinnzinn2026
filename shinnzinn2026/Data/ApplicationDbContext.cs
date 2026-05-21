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

            // StaffCd を「もう一つの鍵（Principal Key）」として認識させる
            modelBuilder.Entity<StaffModel>()
                .HasAlternateKey(s => s.StaffCd);

            // WorkModel と Staff を StaffCd で紐付けると明示的に宣言する
            modelBuilder.Entity<WorkModel>()
                .HasOne(w => w.Staff)
                .WithMany()
                .HasPrincipalKey(s => s.StaffCd)
                .HasForeignKey(w => w.StaffCd);
        }
    }
}