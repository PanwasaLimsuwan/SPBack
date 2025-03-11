using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GateEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetGateEntries()
        {
            var gateEntries = await _context.GateEntry.ToListAsync();
            return Ok(gateEntries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetGateEntryById(int id)
        {
            var entry = await _context.GateEntry.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Gate entry not found");
            }
            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateGateEntry([FromBody] GateEntry entry)
        {
            if (entry == null)
            {
                return BadRequest("Gate entry is null");
            }

            _context.GateEntry.Add(entry);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetGateEntryById), new { id = entry.GateEntryID }, entry);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGateEntry(int id, [FromBody] GateEntry entry)
        {
            if (id != entry.GateEntryID)
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
                if (!_context.GateEntry.Any(e => e.GateEntryID == id))
                {
                    return NotFound("Gate entry not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGateEntry(int id)
        {
            var entry = await _context.GateEntry.FindAsync(id);
            if (entry == null)
            {
                return NotFound("Gate entry not found");
            }

            _context.GateEntry.Remove(entry);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
