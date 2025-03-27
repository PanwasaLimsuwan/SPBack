using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeeklyScheduleController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WeeklyScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetWeeklySchedules()
        {
            var schedules = await _context.WeeklySchedule.ToListAsync();
            return Ok(schedules);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetWeeklyScheduleById(int id)
        {
            var schedule = await _context.WeeklySchedule.FindAsync(id);
            if (schedule == null)
            {
                return NotFound("Weekly schedule not found");
            }
            return Ok(schedule);
        }

        [HttpPost]
        public async Task<IActionResult> CreateWeeklySchedule([FromBody] WeeklySchedule schedule)
        {
            if (schedule == null)
            {
                return BadRequest("Weekly schedule is null");
            }

            _context.WeeklySchedule.Add(schedule);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetWeeklyScheduleById), new { id = schedule.WeekID }, schedule);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateWeeklySchedule(int id, [FromBody] WeeklySchedule schedule)
        {
            if (id != schedule.WeekID)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(schedule).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.WeeklySchedule.Any(e => e.WeekID == id))
                {
                    return NotFound("Weekly schedule not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeeklySchedule(int id)
        {
            var schedule = await _context.WeeklySchedule.FindAsync(id);
            if (schedule == null)
            {
                return NotFound("Weekly schedule not found");
            }

            _context.WeeklySchedule.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
