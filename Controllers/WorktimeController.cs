using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorktimeController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WorktimeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWorktimes()
        {
            var worktimes = await _context.Worktime.ToListAsync();
            return Ok(worktimes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWorktimeById(int id)
        {
            var worktime = await _context.Worktime.FindAsync(id);
            if (worktime == null)
            {
                return NotFound("Worktime record not found");
            }
            return Ok(worktime);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWorktime([FromBody] Worktime worktime)
        {
            if (worktime == null)
            {
                return BadRequest("Worktime record is null");
            }

            _context.Worktime.Add(worktime);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWorktimeById), new { id = worktime.WorkTimeID }, worktime);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWorktime(int id, [FromBody] Worktime worktime)
        {
            if (id != worktime.WorkTimeID)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(worktime).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Worktime.Any(e => e.WorkTimeID == id))
                {
                    return NotFound("Worktime record not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWorktime(int id)
        {
            var worktime = await _context.Worktime.FindAsync(id);
            if (worktime == null)
            {
                return NotFound("Worktime record not found");
            }

            _context.Worktime.Remove(worktime);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
