
# Fracto Backend (ASP.NET Core)

This README describes the backend API for the Fracto project built with ASP.NET Core and Entity Framework Core.

## Prerequisites
- .NET 6+ SDK (or .NET 7)
- SQL Server (or SQL Server Express / LocalDB)
- EF Core CLI (optional)

## Setup
1. Clone or copy the backend project.
2. Update the `appsettings.json` connection string to point to your SQL Server database.
3. Run EF Core migrations (if using Code First):
   ```bash
   dotnet ef database update
   ```
4. Run the application:
   ```bash
   dotnet run
   ```
5. API will be available at `https://localhost:5001` (or configured port).

## Project Structure (suggested)
- `Controllers/` - API controllers (DoctorsController, AppointmentsController, RatingsController, UsersController, SpecializationsController)
- `Data/` - `ApplicationDbContext` (EF Core DbContext)
- `Models/` - Entity models (Doctor, Appointment, Rating, User, Specialization)
- `DTOs/` - Data Transfer Objects
- `Services/` - Business logic and repository implementations
- `Auth/` - JWT token generation and validation

## Authentication & Authorization
- Implement JWT-based authentication.
- Use role-based authorization for admin endpoints (e.g., CRUD on users, doctor management).
- Secure sensitive endpoints and validate inputs.

## Example DB Models
- Doctors (Id, Name, City, SpecializationId, Rating, StartTime, EndTime, SlotDurationMinutes, ProfileImagePath)
- Specializations (Id, Name)
- Users (Id, Username, Email, PasswordHash, Role, ProfileImagePath)
- Appointments (Id, UserId, DoctorId, AppointmentDate, TimeSlot, Status)
- Ratings (Id, AppointmentId, DoctorId, UserId, Score, Comment, CreatedAt)

## Testing
- Use xUnit + Moq for unit tests.
- Add integration tests for controllers using WebApplicationFactory and an in-memory or test database.
