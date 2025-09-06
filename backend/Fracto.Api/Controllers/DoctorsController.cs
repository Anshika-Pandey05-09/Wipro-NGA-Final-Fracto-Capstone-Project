using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Fracto.Api.Data;
using Fracto.Api.Models;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DoctorsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;

        public DoctorsController(ApplicationDbContext db) => _db = db;

        // GET: api/doctors?city=&specializationId=&minRating=
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<Doctor>), 200)]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? city,
            [FromQuery] int? specializationId,
            [FromQuery] decimal? minRating,
            CancellationToken ct)
        {
            var q = _db.Doctors.AsNoTracking().Include(d => d.Specialization).AsQueryable();

            if (!string.IsNullOrWhiteSpace(city))
            {
                var c = city.Trim().ToLower();
                q = q.Where(d => d.City.ToLower() == c);
            }

            if (specializationId.HasValue)
                q = q.Where(d => d.SpecializationId == specializationId.Value);

            if (minRating.HasValue)
                q = q.Where(d => d.Rating >= minRating.Value);

            return Ok(await q.ToListAsync(ct));
        }

        // GET: api/doctors/5
        [HttpGet("{id:int}")]
        [ProducesResponseType(typeof(Doctor), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Get(int id, CancellationToken ct)
        {
            var doc = await _db.Doctors.AsNoTracking()
                .Include(d => d.Specialization)
                .SingleOrDefaultAsync(d => d.Id == id, ct);

            return doc == null ? NotFound() : Ok(doc);
        }

        [HttpGet("{id:int}/timeslots")]
        [ProducesResponseType(typeof(IEnumerable<string>), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> Timeslots(
     int id,
     [FromQuery] string date,
     CancellationToken ct)
        {
            if (!DateOnly.TryParse(date, out var day))
                return BadRequest("Invalid date. Expected yyyy-MM-dd.");

            var doctor = await _db.Doctors.AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == id, ct);
            if (doctor == null) return NotFound();

            if (doctor.SlotDurationMinutes <= 0)
                return BadRequest("Slot duration must be positive.");
            if (doctor.StartTime >= doctor.EndTime)
                return BadRequest("Doctor working hours are invalid.");

            static string fmt(TimeSpan ts) => ts.ToString(@"hh\:mm");

            var slotDuration = TimeSpan.FromMinutes(doctor.SlotDurationMinutes);
            var allSlots = new List<string>();
            for (var t = doctor.StartTime; t + slotDuration <= doctor.EndTime; t += slotDuration)
                allSlots.Add($"{fmt(t)}-{fmt(t + slotDuration)}");

            var rows = await _db.Appointments.AsNoTracking()
                .Where(a => a.DoctorId == id)
                .Select(a => new { a.AppointmentDate, a.TimeSlot, a.Status })
                .ToListAsync(ct);

            var blocking = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        { "Pending", "Booked", "Completed" };

            bool SameDay(DateTime dt) => dt.Date == day.ToDateTime(TimeOnly.MinValue).Date;

            var booked = rows
                .Where(a => !string.IsNullOrWhiteSpace(a.Status)
                            && blocking.Contains(a.Status!)
                            && a.AppointmentDate is DateTime when
                            && SameDay(when))
                .Select(a => a.TimeSlot)
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var available = allSlots.Except(booked, StringComparer.OrdinalIgnoreCase).ToList();
            return Ok(available);
        }

        // ---------- helpers to compare day safely ----------
        private static bool SameDay(DateTime appointmentDate, DateOnly d)
            => appointmentDate.Date == d.ToDateTime(TimeOnly.MinValue).Date;

        private static bool SameDay(DateTime? appointmentDate, DateOnly d)
            => appointmentDate.HasValue && appointmentDate.Value.Date == d.ToDateTime(TimeOnly.MinValue).Date;

        private static bool SameDay(DateOnly appointmentDate, DateOnly d)
            => appointmentDate == d;

        private static bool SameDay(DateOnly? appointmentDate, DateOnly d)
            => appointmentDate.HasValue && appointmentDate.Value == d;

        // POST: api/doctors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Doctor), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> CreateDoctor([FromBody] DoctorCreateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (dto.StartTime >= dto.EndTime) return BadRequest("StartTime must be before EndTime.");
            if (!await _db.Specializations.AnyAsync(s => s.Id == dto.SpecializationId, ct))
                return BadRequest("Invalid SpecializationId.");

            var entity = new Doctor
            {
                Name = dto.Name.Trim(),
                City = dto.City.Trim(),
                SpecializationId = dto.SpecializationId,
                Rating = dto.Rating,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SlotDurationMinutes = dto.SlotDurationMinutes,
                ProfileImagePath = string.IsNullOrWhiteSpace(dto.ProfileImagePath)
                    ? "default.png"
                    : dto.ProfileImagePath.Trim()
            };

            _db.Doctors.Add(entity);
            await _db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(Get), new { id = entity.Id }, entity);
        }

        // PUT: api/doctors/5
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(typeof(Doctor), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> UpdateDoctor(int id, [FromBody] DoctorUpdateDto dto, CancellationToken ct)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);
            if (dto.StartTime >= dto.EndTime) return BadRequest("StartTime must be before EndTime.");
            if (!await _db.Specializations.AnyAsync(s => s.Id == dto.SpecializationId, ct))
                return BadRequest("Invalid SpecializationId.");

            var existing = await _db.Doctors.SingleOrDefaultAsync(d => d.Id == id, ct);
            if (existing == null) return NotFound();

            existing.Name = dto.Name.Trim();
            existing.City = dto.City.Trim();
            existing.Rating = dto.Rating;
            existing.StartTime = dto.StartTime;
            existing.EndTime = dto.EndTime;
            existing.SlotDurationMinutes = dto.SlotDurationMinutes;
            existing.ProfileImagePath = string.IsNullOrWhiteSpace(dto.ProfileImagePath)
                ? "default.png"
                : dto.ProfileImagePath.Trim();
            existing.SpecializationId = dto.SpecializationId;

            await _db.SaveChangesAsync(ct);
            return Ok(existing);
        }

        // DELETE: api/doctors/5
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> DeleteDoctor(int id, CancellationToken ct)
        {
            var doctor = await _db.Doctors.SingleOrDefaultAsync(d => d.Id == id, ct);
            if (doctor == null) return NotFound();

            _db.Doctors.Remove(doctor);
            await _db.SaveChangesAsync(ct);
            return NoContent();
        }
        
        // GET /api/doctors/available?city=&specializationId=&date=yyyy-MM-dd&minRating=
[HttpGet("available")]
public async Task<IActionResult> Available(
    [FromQuery] string? city,
    [FromQuery] int? specializationId,
    [FromQuery] string? date,
    [FromQuery] decimal? minRating,
    CancellationToken ct)
{
    if (string.IsNullOrWhiteSpace(date) || !DateOnly.TryParse(date, out var day))
        return BadRequest("date (yyyy-MM-dd) required.");

    var docs = _db.Doctors.AsNoTracking().Include(d => d.Specialization).AsQueryable();

    if (!string.IsNullOrWhiteSpace(city))
        docs = docs.Where(d => d.City.ToLower() == city.ToLower());
    if (specializationId.HasValue)
        docs = docs.Where(d => d.SpecializationId == specializationId.Value);
    if (minRating.HasValue)
        docs = docs.Where(d => d.Rating >= minRating.Value);

    var list = await docs.ToListAsync(ct);

    // Filter by having at least one free slot that day (reuse your slots logic in-memory)
    static string fmt(TimeSpan ts) => ts.ToString(@"hh\:mm");
    var result = new List<Doctor>();

    foreach (var d in list)
    {
        if (d.SlotDurationMinutes <= 0 || d.StartTime >= d.EndTime) continue;
        var dur = TimeSpan.FromMinutes(d.SlotDurationMinutes);
        var all = new List<string>();
        for (var t = d.StartTime; t + dur <= d.EndTime; t += dur)
            all.Add($"{fmt(t)}-{fmt(t + dur)}");

        var dayStart = day.ToDateTime(TimeOnly.MinValue);
        var dayEnd = dayStart.AddDays(1);

        var booked = await _db.Appointments
            .AsNoTracking()
            .Where(a => a.DoctorId == d.Id
                     && a.AppointmentDate >= dayStart
                     && a.AppointmentDate < dayEnd
                     && a.Status != "Cancelled")
            .Select(a => a.TimeSlot)
            .ToListAsync(ct);

        var available = all.Except(booked, StringComparer.OrdinalIgnoreCase).ToList();
        if (available.Count > 0) result.Add(d);
    }

    return Ok(result);
}

    }
}
