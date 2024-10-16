using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Phongkham.Models;

namespace Phongkham.Data
{
    public class ApplicationDBcontext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDBcontext(DbContextOptions<ApplicationDBcontext> options) : base(options) { }

        public DbSet<Chuyenmon> Chuyenmons { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Cakham> Cakhams { get; set; }
        public DbSet<KhungGio> KhungGios { get; set; }
        public DbSet<lichKham> lichKhams { get; set; }
        public DbSet<CTlichkham> cTlichkhams { get; set; }
        public DbSet<Loaitintuc> Loaitintucs { get; set; }
        public DbSet<Tintuc> Tintucs { get; set; }
        public DbSet<TintucImage> TintucImages { get; set; }
        public DbSet<CTnhasi> cTnhasis { get; set; }
        public DbSet<Questions> Questions { get; set; }
        public DbSet<Answers> Answers { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<KhungGio>().HasData(
                new KhungGio { Id = 1, TimeSlot = "7:00 - 8:00" },
                new KhungGio { Id = 2, TimeSlot = "8:00 - 9:00" },
                new KhungGio { Id = 3, TimeSlot = "9:00 - 10:00" },
                new KhungGio { Id = 4, TimeSlot = "10:00 - 11:00" },
                new KhungGio { Id = 5, TimeSlot = "13:00 - 14:00" },
                new KhungGio { Id = 6, TimeSlot = "14:00 - 15:00" },
                new KhungGio { Id = 7, TimeSlot = "15:00 - 16:00" },
                new KhungGio { Id = 8, TimeSlot = "16:00 - 17:00" }
            );

            // Configure the foreign keys with NO ACTION on delete
            modelBuilder.Entity<Questions>()
                .HasOne(q => q.Patient)
                .WithMany(u => u.Questions)
                .HasForeignKey(q => q.PatientId) // Thay UserId thành PatientId
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answers>()
                .HasOne(a => a.Dentist)
                .WithMany(u => u.Answers)
                .HasForeignKey(a => a.DentistId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Answers>()
                .HasOne(a => a.Question)
                .WithMany(q => q.Answers)
                .HasForeignKey(a => a.QuestionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
