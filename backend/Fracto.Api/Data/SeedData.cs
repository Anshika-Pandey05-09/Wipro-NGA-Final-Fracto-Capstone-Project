using Fracto.Api.Models;

namespace Fracto.Api.Data
{
    public static class SeedData
    {
        public static void Initialize(ApplicationDbContext context)
        {
            // ensure DB created / migrations applied before calling this
            if (!context.Specializations.Any())
            {
                var specs = new[]
                {
                    new Specialization { Name = "Cardiology" },
                    new Specialization { Name = "Dermatology" },
                    new Specialization { Name = "General Physician" },
                    new Specialization { Name = "Neurology" },
                    new Specialization { Name = "Orthopedics" }
                };
                context.Specializations.AddRange(specs);
                context.SaveChanges();
            }

            if (!context.Doctors.Any())
            {
                // use existing specialization IDs - after seeding above they will be 1..n
                var docs = new[]
                {
                    new Doctor { Name="Dr. A", City="Mumbai", SpecializationId=1, Rating=4.7M, StartTime=TimeSpan.Parse("09:00"), EndTime=TimeSpan.Parse("17:00"), SlotDurationMinutes=30, ProfileImagePath="default.png" },
                    new Doctor { Name="Dr. B", City="Delhi", SpecializationId=3, Rating=4.2M, StartTime=TimeSpan.Parse("10:00"), EndTime=TimeSpan.Parse("16:00"), SlotDurationMinutes=30, ProfileImagePath="doctor-a.png" },
                    new Doctor { Name="Dr. C", City="Bangalore", SpecializationId=2, Rating=4.9M, StartTime=TimeSpan.Parse("08:00"), EndTime=TimeSpan.Parse("14:00"), SlotDurationMinutes=30, ProfileImagePath="default.png" },
                    //new Doctor { Name="Dr. C", City="Pune", SpecializationId=4, Rating=4.9, StartTime=TimeSpan.Parse("07:00"), EndTime=TimeSpan.Parse("15:00"), SlotDurationMinutes=30, ProfileImagePath="default.png" }
                };
                context.Doctors.AddRange(docs);
                context.SaveChanges();
            }

            // basic admin user
            if (!context.Users.Any(u => u.Username == "admin"))
            {
                var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<AppUser>();
                var admin = new AppUser
                {
                    Username = "admin",
                    Email = "admin@example.com",
                    Role = "Admin",
                    ProfileImagePath = "doctor-b.png"
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin@123");
                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
