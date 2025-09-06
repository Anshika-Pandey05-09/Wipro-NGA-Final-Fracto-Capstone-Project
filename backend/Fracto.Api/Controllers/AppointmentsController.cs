// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Fracto.Api.Data;
// using Fracto.Api.DTOs;
// using Fracto.Api.Models;

// namespace Fracto.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AppointmentsController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         public AppointmentsController(ApplicationDbContext db) => _db = db;

//         // Book appointment (User/Admin)
//         [HttpPost]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> Book([FromBody] AppointmentDto dto)
//         {
//             if (dto is null) return BadRequest("Invalid payload.");
//             if (string.IsNullOrWhiteSpace(dto.TimeSlot))
//                 return BadRequest("TimeSlot is required.");

//             // check if doctor exists
//             var doctor = await _db.Doctors
//                 .Include(d => d.Specialization)
//                 .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

//             if (doctor == null) return NotFound("Doctor not found.");

//             // check if slot already booked (avoid double booking the same slot)
//             var slotTaken = await _db.Appointments.AnyAsync(a =>
//                 a.DoctorId == dto.DoctorId &&
//                 a.AppointmentDate.Date == dto.AppointmentDate.Date &&
//                 a.TimeSlot == dto.TimeSlot &&
//                 a.Status == "Booked");

//             if (slotTaken) return BadRequest("This slot is already booked.");

//             var appt = new Appointment
//             {
//                 UserId = dto.UserId,
//                 DoctorId = dto.DoctorId,
//                 AppointmentDate = dto.AppointmentDate.Date,
//                 TimeSlot = dto.TimeSlot,
//                 Status = "Booked"
//             };

//             _db.Appointments.Add(appt);
//             await _db.SaveChangesAsync();

//             // Specialization may be null, so guard it
//             var spec = doctor.Specialization != null
//                 ? new { doctor.Specialization.Id, doctor.Specialization.Name }
//                 : null;

//             return Ok(new
//             {
//                 appt.Id,
//                 appt.AppointmentDate,
//                 TimeSlot = appt.TimeSlot ?? string.Empty,
//                 Status = appt.Status ?? "Booked",
//                 Doctor = new
//                 {
//                     doctor.Id,
//                     doctor.Name,
//                     doctor.City,
//                     Specialization = spec
//                 }
//             });
//         }

//         // Cancel appointment (User/Admin)
//         [HttpPut("cancel/{id:int}")]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> Cancel(int id)
//         {
//             var appt = await _db.Appointments.FindAsync(id);
//             if (appt == null) return NotFound();

//             appt.Status = "Cancelled";
//             await _db.SaveChangesAsync();

//             return Ok(new { message = "Appointment cancelled successfully." });
//         }

//         // Get all appointments for a specific user (User/Admin)
//         [HttpGet("user/{userId:int}")]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> ByUser(int userId)
//         {
//             var list = await _db.Appointments
//                 .Where(a => a.UserId == userId)
//                 .Include(a => a.Doctor)
//                     .ThenInclude(d => d.Specialization)
//                 .OrderByDescending(a => a.AppointmentDate)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.AppointmentDate,
//                     TimeSlot = a.TimeSlot ?? string.Empty,
//                     Status = a.Status ?? "Booked",
//                     Doctor = a.Doctor == null ? null : new
//                     {
//                         a.Doctor.Id,
//                         a.Doctor.Name,
//                         a.Doctor.City,
//                         Specialization = a.Doctor.Specialization == null ? null : new
//                         {
//                             a.Doctor.Specialization.Id,
//                             a.Doctor.Specialization.Name
//                         }
//                     }
//                 })
//                 .ToListAsync();

//             return Ok(list);
//         }

//         // ----------------- Admin utilities -----------------

//         // Get ALL appointments (Admin)
//         [HttpGet]
//         [Authorize(Roles = "Admin")]
//         public async Task<IActionResult> GetAll()
//         {
//             var list = await _db.Appointments
//                 .Include(a => a.Doctor)
//                     .ThenInclude(d => d.Specialization)
//                 .OrderByDescending(a => a.AppointmentDate)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.UserId,
//                     a.DoctorId,
//                     a.AppointmentDate,
//                     TimeSlot = a.TimeSlot ?? string.Empty,
//                     Status = a.Status ?? "Booked",
//                     Doctor = a.Doctor == null ? null : new
//                     {
//                         a.Doctor.Id,
//                         a.Doctor.Name,
//                         a.Doctor.City,
//                         Specialization = a.Doctor.Specialization == null ? null : new
//                         {
//                             a.Doctor.Specialization.Id,
//                             a.Doctor.Specialization.Name
//                         }
//                     }
//                 })
//                 .ToListAsync();

//             return Ok(list);
//         }

//         // Approve appointment (if you want an explicit approval step) (Admin)
//         // If you don't use "Approved" status, you can remove this method.
//         [HttpPut("approve/{id:int}")]
//         [Authorize(Roles = "Admin")]
//         public async Task<IActionResult> Approve(int id)
//         {
//             var appt = await _db.Appointments.FindAsync(id);
//             if (appt == null) return NotFound();
//             if (string.Equals(appt.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
//                 return BadRequest("Cannot approve a cancelled appointment.");

//             appt.Status = "Approved";
//             await _db.SaveChangesAsync();
//             return Ok(new { message = "Appointment approved successfully." });
//         }
//     }
// }

// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Fracto.Api.Data;
// using Fracto.Api.DTOs;
// using Fracto.Api.Models;

// namespace Fracto.Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AppointmentsController : ControllerBase
//     {
//         private readonly ApplicationDbContext _db;
//         public AppointmentsController(ApplicationDbContext db) => _db = db;

//         // ===========================
//         // Book appointment (User/Admin)
//         // ===========================
//         [HttpPost]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> Book([FromBody] AppointmentDto dto)
//         {
//             if (dto is null) return BadRequest("Invalid payload.");
//             if (string.IsNullOrWhiteSpace(dto.TimeSlot))
//                 return BadRequest("TimeSlot is required.");

//             // Check doctor exists
//             var doctor = await _db.Doctors
//                 .Include(d => d.Specialization)
//                 .FirstOrDefaultAsync(d => d.Id == dto.DoctorId);

//             if (doctor == null) return NotFound("Doctor not found.");

//             // Prevent double-booking the same doctor/date/timeslot unless the other is Cancelled
//             var slotTaken = await _db.Appointments.AnyAsync(a =>
//                 a.DoctorId == dto.DoctorId &&
//                 a.AppointmentDate.Date == dto.AppointmentDate.Date &&
//                 a.TimeSlot == dto.TimeSlot &&
//                 (a.Status != "Cancelled"));

//             if (slotTaken) return BadRequest("This slot is already booked.");

//             var appt = new Appointment
//             {
//                 UserId = dto.UserId,
//                 DoctorId = dto.DoctorId,
//                 AppointmentDate = dto.AppointmentDate.Date,
//                 TimeSlot = dto.TimeSlot,
//                 Status = "Pending" // set to "Booked" if you do not require approval
//             };

//             _db.Appointments.Add(appt);
//             await _db.SaveChangesAsync();

//             var spec = doctor.Specialization != null
//                 ? new { doctor.Specialization.Id, doctor.Specialization.Name }
//                 : null;

//             return Ok(new
//             {
//                 appt.Id,
//                 appt.AppointmentDate,
//                 TimeSlot = appt.TimeSlot ?? string.Empty,
//                 Status = appt.Status ?? "Pending",
//                 Doctor = new
//                 {
//                     doctor.Id,
//                     doctor.Name,
//                     doctor.City,
//                     Specialization = spec
//                 }
//             });
//         }

//         // ===========================
//         // Cancel appointment (User/Admin)
//         // ===========================
//         [HttpPut("cancel/{id:int}")]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> Cancel(int id)
//         {
//             var appt = await _db.Appointments.FindAsync(id);
//             if (appt == null) return NotFound();

//             appt.Status = "Cancelled";
//             await _db.SaveChangesAsync();

//             return Ok(new { message = "Appointment cancelled successfully." });
//         }

//         // ===================================================
//         // Get all appointments for a specific user (User/Admin)
//         // ===================================================
//         [HttpGet("user/{userId:int}")]
//         [Authorize(Roles = "User,Admin")]
//         public async Task<IActionResult> ByUser(int userId)
//         {
//             var list = await _db.Appointments
//                 .Where(a => a.UserId == userId)
//                 .Include(a => a.Doctor)
//                     .ThenInclude(d => d.Specialization)
//                 .OrderByDescending(a => a.AppointmentDate)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.AppointmentDate,
//                     TimeSlot = a.TimeSlot ?? string.Empty,
//                     Status = a.Status ?? "Pending",
//                     Doctor = a.Doctor == null ? null : new
//                     {
//                         a.Doctor.Id,
//                         a.Doctor.Name,
//                         a.Doctor.City,
//                         Specialization = a.Doctor.Specialization == null ? null : new
//                         {
//                             a.Doctor.Specialization.Id,
//                             a.Doctor.Specialization.Name
//                         }
//                     }
//                 })
//                 .ToListAsync();

//             return Ok(list);
//         }

//         // ===========================
//         // -------- Admin-only -------
//         // ===========================

//         // Get ALL appointments (Admin)
//         [HttpGet]
//         [Authorize(Roles = "Admin")]
//         public async Task<IActionResult> GetAll()
//         {
//             var list = await _db.Appointments
//                 .Include(a => a.Doctor)
//                     .ThenInclude(d => d.Specialization)
//                 .OrderByDescending(a => a.AppointmentDate)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.UserId,
//                     a.DoctorId,
//                     a.AppointmentDate,
//                     TimeSlot = a.TimeSlot ?? string.Empty,
//                     Status = a.Status ?? "Pending",
//                     Doctor = a.Doctor == null ? null : new
//                     {
//                         a.Doctor.Id,
//                         a.Doctor.Name,
//                         a.Doctor.City,
//                         Specialization = a.Doctor.Specialization == null ? null : new
//                         {
//                             a.Doctor.Specialization.Id,
//                             a.Doctor.Specialization.Name
//                         }
//                     }
//                 })
//                 .ToListAsync();

//             return Ok(list);
//         }

//         // Get only Pending appointments (Admin)
//         [HttpGet("pending")]
//         [Authorize(Roles = "Admin")]
//         public async Task<IActionResult> GetPending()
//         {
//             var list = await _db.Appointments
//                 .Where(a => a.Status == "Pending") // change to "Booked" if you don't use approval
//                 .Include(a => a.Doctor)
//                     .ThenInclude(d => d.Specialization)
//                 .OrderBy(a => a.AppointmentDate)
//                 .Select(a => new
//                 {
//                     a.Id,
//                     a.UserId,
//                     a.DoctorId,
//                     a.AppointmentDate,
//                     TimeSlot = a.TimeSlot ?? string.Empty,
//                     Status = a.Status ?? "Pending",
//                     Doctor = a.Doctor == null ? null : new
//                     {
//                         a.Doctor.Id,
//                         a.Doctor.Name,
//                         a.Doctor.City,
//                         Specialization = a.Doctor.Specialization == null ? null : new
//                         {
//                             a.Doctor.Specialization.Id,
//                             a.Doctor.Specialization.Name
//                         }
//                     }
//                 })
//                 .ToListAsync();

//             return Ok(list);
//         }

//         // Approve appointment (Admin)
//         [HttpPut("approve/{id:int}")]
//         [Authorize(Roles = "Admin")]
//         public async Task<IActionResult> Approve(int id)
//         {
//             var appt = await _db.Appointments.FindAsync(id);
//             if (appt == null) return NotFound();

//             if (string.Equals(appt.Status, "Cancelled", StringComparison.OrdinalIgnoreCase))
//                 return BadRequest("Cannot approve a cancelled appointment.");

//             appt.Status = "Approved";
//             await _db.SaveChangesAsync();
//             return Ok(new { message = "Appointment approved successfully." });
//         }
//     }
// }


using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.Models;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AppointmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public AppointmentsController(ApplicationDbContext db) => _db = db;

        // -------------------- USER: Create a booking --------------------
        // POST /api/appointments
        // Body: { userId, doctorId, appointmentDate: "yyyy-MM-dd", timeSlot: "hh:mm-hh:mm" }
        [HttpPost]
        [Authorize] // user must be logged in
        public async Task<IActionResult> Create([FromBody] CreateAppointmentDto dto, CancellationToken ct)
        {
            if (dto == null) return BadRequest("Invalid payload.");

            if (dto.UserId <= 0 || dto.DoctorId <= 0)
                return BadRequest("UserId and DoctorId are required.");

            if (string.IsNullOrWhiteSpace(dto.AppointmentDate))
                return BadRequest("AppointmentDate is required (yyyy-MM-dd).");

            if (string.IsNullOrWhiteSpace(dto.TimeSlot))
                return BadRequest("TimeSlot is required (hh:mm-hh:mm).");

            // parse date
            if (!DateOnly.TryParse(dto.AppointmentDate, out var d))
                return BadRequest("Invalid AppointmentDate. Expected yyyy-MM-dd.");

            // conflict check: same doctor + same day + same slot where not Cancelled
            var dayStart = d.ToDateTime(TimeOnly.MinValue);
            var dayEnd = dayStart.AddDays(1);

            var conflict = await _db.Appointments.AnyAsync(a =>
                a.DoctorId == dto.DoctorId &&
                a.AppointmentDate >= dayStart &&
                a.AppointmentDate < dayEnd &&
                a.TimeSlot.ToLower() == dto.TimeSlot.Trim().ToLower() &&
                a.Status != "Cancelled", ct);

            if (conflict)
                return Conflict("This time slot is no longer available.");

            var entity = new Appointment
            {
                UserId = dto.UserId,
                DoctorId = dto.DoctorId,
                AppointmentDate = dayStart,
                TimeSlot = dto.TimeSlot.Trim(),
                Status = "Pending",
            };

            _db.Appointments.Add(entity);
            await _db.SaveChangesAsync(ct);

            var vm = await ProjectToVm(entity.Id, ct);
            return CreatedAtAction(nameof(GetById), new { id = entity.Id }, vm);
        }

        // -------------------- USER: Get my appointments --------------------
        // GET /api/appointments/user/123
        [HttpGet("user/{userId:int}")]
        [Authorize]
        public async Task<IActionResult> GetMine(int userId, CancellationToken ct)
        {
            var list = await _db.Appointments
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentVm
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    DoctorProfileImagePath = a.Doctor.ProfileImagePath,
                    City = a.Doctor.City,
                    PatientName = "", // optional
                    AppointmentDate = a.AppointmentDate,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status
                })
                .ToListAsync(ct);

            return Ok(list);
        }

        // -------------------- ADMIN: List all (with filters) --------------------
        // GET /api/appointments/admin?status=&city=&dateFrom=yyyy-MM-dd&dateTo=yyyy-MM-dd
        [HttpGet("admin")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AdminList(
            [FromQuery] string? status,
            [FromQuery] string? city,
            [FromQuery] string? dateFrom,
            [FromQuery] string? dateTo,
            CancellationToken ct)
        {
            var q = _db.Appointments
                .AsNoTracking()
                .Include(a => a.Doctor)
                .Include(a => a.User)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                var s = status.Trim();
                q = q.Where(a => a.Status == s);
            }

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = city.Trim().ToLower();
                q = q.Where(a => a.Doctor.City.ToLower() == c);
            }

            if (DateOnly.TryParse(dateFrom, out var from))
            {
                var fromDt = from.ToDateTime(TimeOnly.MinValue);
                q = q.Where(a => a.AppointmentDate >= fromDt);
            }

            if (DateOnly.TryParse(dateTo, out var to))
            {
                var toDt = to.ToDateTime(TimeOnly.MinValue).AddDays(1);
                q = q.Where(a => a.AppointmentDate < toDt);
            }

            var list = await q
                .OrderByDescending(a => a.AppointmentDate)
                .Select(a => new AppointmentVm
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    DoctorProfileImagePath = a.Doctor.ProfileImagePath,
                    City = a.Doctor.City,
                    PatientName = a.User.Username,
                    AppointmentDate = a.AppointmentDate,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status
                })
                .ToListAsync(ct);

            return Ok(list);
        }

        // -------------------- ADMIN: Approve (Pending -> Booked) --------------------
        // POST /api/appointments/{id}/approve
        [HttpPost("{id:int}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(int id, CancellationToken ct)
        {
            var appt = await _db.Appointments.SingleOrDefaultAsync(a => a.Id == id, ct);
            if (appt == null) return NotFound();

            if (appt.Status != "Pending")
                return BadRequest("Only Pending appointments can be approved.");

            appt.Status = "Booked";
            await _db.SaveChangesAsync(ct);

            var vm = await ProjectToVm(id, ct);
            return Ok(vm);
        }

        // -------------------- ADMIN: Cancel (Pending/Booked -> Cancelled) --------------------
        // POST /api/appointments/{id}/cancel
        [HttpPost("{id:int}/cancel")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Cancel(int id, CancellationToken ct)
        {
            var appt = await _db.Appointments.SingleOrDefaultAsync(a => a.Id == id, ct);
            if (appt == null) return NotFound();

            if (appt.Status != "Pending" && appt.Status != "Booked")
                return BadRequest("Only Pending or Booked appointments can be cancelled.");

            appt.Status = "Cancelled";
            await _db.SaveChangesAsync(ct);

            var vm = await ProjectToVm(id, ct);
            return Ok(vm);
        }

        // -------------------- SUPPORT --------------------
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(int id, CancellationToken ct)
        {
            var vm = await ProjectToVm(id, ct);
            if (vm == null) return NotFound();
            return Ok(vm);
        }

        private async Task<AppointmentVm?> ProjectToVm(int id, CancellationToken ct)
        {
            return await _db.Appointments.AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AppointmentVm
                {
                    Id = a.Id,
                    DoctorId = a.DoctorId,
                    DoctorName = a.Doctor.Name,
                    DoctorProfileImagePath = a.Doctor.ProfileImagePath,
                    City = a.Doctor.City,
                    PatientName = a.User.Username,
                    AppointmentDate = a.AppointmentDate,
                    TimeSlot = a.TimeSlot,
                    Status = a.Status
                })
                .SingleOrDefaultAsync(ct);
        }

        // POST /api/appointments/{id}/user-cancel
[HttpPost("{id:int}/user-cancel")]
[Authorize]
public async Task<IActionResult> UserCancel(int id, [FromBody] dynamic body, CancellationToken ct)
{
    // Expecting { userId: 123 }
    int userId;
    try { userId = (int)body.userId; } catch { return BadRequest("userId required."); }

    var appt = await _db.Appointments.SingleOrDefaultAsync(a => a.Id == id, ct);
    if (appt == null) return NotFound();

    if (appt.UserId != userId) return Forbid();

    if (appt.Status != "Pending" && appt.Status != "Booked")
        return BadRequest("Only Pending or Booked appointments can be cancelled by the user.");

    appt.Status = "Cancelled";
    await _db.SaveChangesAsync(ct);
    return Ok(new { message = "Cancelled" });
}

    }

    // -------- DTOs / VMs --------
    public class CreateAppointmentDto
    {
        public int UserId { get; set; }
        public int DoctorId { get; set; }
        public string AppointmentDate { get; set; } = "";  // yyyy-MM-dd
        public string TimeSlot { get; set; } = "";         // "hh:mm-hh:mm"
    }

    public class AppointmentVm
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }
        public string DoctorName { get; set; } = "";
        public string? DoctorProfileImagePath { get; set; }
        public string City { get; set; } = "";
        public string PatientName { get; set; } = "";
        public DateTime AppointmentDate { get; set; }
        public string TimeSlot { get; set; } = "";
        public string Status { get; set; } = "";
    }
}
