using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerPlanController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManpowerPlanController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ManpowerPlan
        [HttpGet]
        public async Task<IActionResult> GetManpowerPlans()
        {
            var manpowerPlans = await _context.ManpowerPlan.ToListAsync();
            return Ok(manpowerPlans);
        }

        // GET: api/ManpowerPlan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetManpowerPlanById(int id)
        {
            var manpowerPlan = await _context.ManpowerPlan.FindAsync(id);
            if (manpowerPlan == null)
            {
                return NotFound("Manpower plan not found");
            }
            return Ok(manpowerPlan);
        }

        // POST: api/ManpowerPlan
        [HttpPost]
        public async Task<IActionResult> CreateManpowerPlan([FromBody] ManpowerPlan plan)
        {
            if (plan == null)
            {
                return BadRequest("Manpower plan is null");
            }

            _context.ManpowerPlan.Add(plan);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetManpowerPlanById), new { id = plan.PlanID }, plan); // Use PlanID instead of Id
        }

        // PUT: api/ManpowerPlan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManpowerPlan(int id, [FromBody] ManpowerPlan plan)
        {
            if (id != plan.PlanID) // Use PlanID instead of Id
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(plan).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ManpowerPlan.Any(e => e.PlanID == id)) // Use PlanID instead of Id
                {
                    return NotFound("Manpower plan not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ManpowerPlan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManpowerPlan(int id)
        {
            var plan = await _context.ManpowerPlan.FindAsync(id);
            if (plan == null)
            {
                return NotFound("Manpower plan not found");
            }

            _context.ManpowerPlan.Remove(plan);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
