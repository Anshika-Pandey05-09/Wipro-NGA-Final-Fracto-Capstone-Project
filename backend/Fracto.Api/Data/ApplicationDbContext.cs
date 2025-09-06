using Microsoft.EntityFrameworkCore;
using Fracto.Api.Models;

namespace Fracto.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) {}

        public DbSet<AppUser> Users => Set<AppUser>();
        public DbSet<Specialization> Specializations => Set<Specialization>();
        public DbSet<Doctor> Doctors => Set<Doctor>();
        public DbSet<Appointment> Appointments => Set<Appointment>();
        public DbSet<Rating> Ratings => Set<Rating>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.ProfileImagePath)
                .HasDefaultValue("default.png")
                .IsRequired();

            // Ratings mapping (avoid multiple cascade paths)
            modelBuilder.Entity<Rating>(b =>
            {
                b.HasIndex(r => r.AppointmentId).IsUnique();

                b.HasOne(r => r.Appointment)
                    .WithMany()
                    .HasForeignKey(r => r.AppointmentId)
                    .OnDelete(DeleteBehavior.Cascade);   // ok

                b.HasOne(r => r.Doctor)
                    .WithMany()
                    .HasForeignKey(r => r.DoctorId)
                    .OnDelete(DeleteBehavior.Restrict);  // no action

                b.HasOne(r => r.User)
                    .WithMany()
                    .HasForeignKey(r => r.UserId)
                    .OnDelete(DeleteBehavior.Restrict);  // no action
            });

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => new { a.DoctorId, a.AppointmentDate, a.TimeSlot })
                .IsUnique();
        }
    }
}
