using Fracto.Api.Data;
using Fracto.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fracto.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpecializationsController : ControllerBase
    {
        private readonly ApplicationDbContext _db;
        public SpecializationsController(ApplicationDbContext db) => _db = db;

        /// <summary>Get all specializations</summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Specialization>>> GetAll()
            => await _db.Specializations.OrderBy(s => s.Name).ToListAsync();

        /// <summary>Get specialization by id</summary>
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Specialization>> Get(int id)
        {
            var sp = await _db.Specializations.FindAsync(id);
            return sp is null ? NotFound() : sp;
        }

        /// <summary>Create specialization (Admin)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Specialization>> Create([FromBody] Specialization dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest("SpecializationName is required.");

            _db.Specializations.Add(dto);
            await _db.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = dto.Id }, dto);
        }

        /// <summary>Update specialization (Admin)</summary>
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Specialization dto)
        {
            var sp = await _db.Specializations.FindAsync(id);
            if (sp is null) return NotFound();
            sp.Name = dto.Name;
            await _db.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>Delete specialization (Admin)</summary>
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var sp = await _db.Specializations.FindAsync(id);
            if (sp is null) return NotFound();
            _db.Specializations.Remove(sp);
            await _db.SaveChangesAsync();
            return NoContent();
        }
    }
}
