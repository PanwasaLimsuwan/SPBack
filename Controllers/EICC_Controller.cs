using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EICC_ControlController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EICC_ControlController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EICC_Control
        [HttpGet]
        public async Task<IActionResult> GetEICCControls()
        {
            var controls = await _context.EICC_Control.ToListAsync();
            return Ok(controls);
        }

        // GET: api/EICC_Control/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEICCControlById(int id)
        {
            var control = await _context.EICC_Control.FindAsync(id);
            if (control == null)
            {
                return NotFound("EICC control not found");
            }
            return Ok(control);
        }

        // POST: api/EICC_Control
        [HttpPost]
        public async Task<IActionResult> CreateEICCControl([FromBody] EICC_Control control)
        {
            if (control == null)
            {
                return BadRequest("EICC control is null");
            }

            _context.EICC_Control.Add(control);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEICCControlById), new { id = control.ControlID }, control); // Use ControlID instead of Id
        }

        // PUT: api/EICC_Control/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEICCControl(int id, [FromBody] EICC_Control control)
        {
            if (id != control.ControlID) // Use ControlID instead of Id
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(control).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EICC_Control.Any(e => e.ControlID == id)) // Use ControlID instead of Id
                {
                    return NotFound("EICC control not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/EICC_Control/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEICCControl(int id)
        {
            var control = await _context.EICC_Control.FindAsync(id);
            if (control == null)
            {
                return NotFound("EICC control not found");
            }

            _context.EICC_Control.Remove(control);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
