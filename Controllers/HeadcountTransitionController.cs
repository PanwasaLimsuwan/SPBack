using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeadcountTransitionController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HeadcountTransitionController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHeadcountTransitions()
        {
            var transitions = await _context.HeadcountTransition.ToListAsync();
            return Ok(transitions);
        }
// [HttpGet]
// public async Task<IActionResult> GetHeadcountTransitions()
// {
//     var transitions = await _context.HeadcountTransition
//         .Where(t => t.dateTime >= DateTime.Now.AddMonths(-3)) // เอาข้อมูล 3 เดือนล่าสุด
//         .OrderBy(t => t.dateTime)
//         .ToListAsync();
//     return Ok(transitions);
// }




        [HttpGet("{id}")]
        public async Task<IActionResult> GetHeadcountTransitionById(DateTime id)
        {
            var transition = await _context.HeadcountTransition.FindAsync(id);
            if (transition == null)
            {
                return NotFound("Headcount transition not found");
            }
            return Ok(transition);
        }

        [HttpPost]
        public async Task<IActionResult> CreateHeadcountTransition([FromBody] HeadcountTransition transition)
        {
            if (transition == null)
            {
                return BadRequest("Headcount transition is null");
            }

            _context.HeadcountTransition.Add(transition);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetHeadcountTransitionById), new { id = transition.DateTime }, transition);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateHeadcountTransition(DateTime id, [FromBody] HeadcountTransition transition)
        {
            if (id != transition.DateTime)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(transition).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.HeadcountTransition.Any(e => e.DateTime == id))
                {
                    return NotFound("Headcount transition not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteHeadcountTransition(DateTime id)
        {
            var transition = await _context.HeadcountTransition.FindAsync(id);
            if (transition == null)
            {
                return NotFound("Headcount transition not found");
            }

            _context.HeadcountTransition.Remove(transition);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
