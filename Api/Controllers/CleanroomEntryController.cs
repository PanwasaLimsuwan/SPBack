using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleanroomEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CleanroomEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCleanroomEntries()
        {
            var cleanroomEntries = await _context.CleanroomEntry.ToListAsync();
            return Ok(cleanroomEntries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCleanroomEntryById(int id)
        {
            var entry = await _context.CleanroomEntry.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Cleanroom entry not found");
            }
            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCleanroomEntry([FromBody] CleanroomEntry entry)
        {
            if (entry == null)
            {
                return BadRequest("Cleanroom entry is null");
            }

            _context.CleanroomEntry.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetCleanroomEntryById), new { id = entry.CEntryID }, entry);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCleanroomEntry(int id, [FromBody] CleanroomEntry entry)
        {
            if (id != entry.CEntryID)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(entry).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.CleanroomEntry.Any(e => e.CEntryID == id))
                {
                    return NotFound("Cleanroom entry not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCleanroomEntry(int id)
        {
            var entry = await _context.CleanroomEntry.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Cleanroom entry not found");
            }

            _context.CleanroomEntry.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
