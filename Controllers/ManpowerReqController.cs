using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerReqController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ManpowerReqController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/ManpowerReq
        [HttpGet]
        public async Task<IActionResult> GetManpowerReqs()
        {
            var manpowerReqs = await _context.ManpowerReq.ToListAsync();
            return Ok(manpowerReqs);
        }

        // GET: api/ManpowerReq/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetManpowerReqById(string id)
        {
            var manpowerReq = await _context.ManpowerReq.FindAsync(id);
            if (manpowerReq == null)
            {
                return NotFound("Manpower request not found");
            }
            return Ok(manpowerReq);
        }

        // POST: api/ManpowerReq
        [HttpPost]
        public async Task<IActionResult> CreateManpowerReq([FromBody] ManpowerReq req)
        {
            if (req == null)
            {
                return BadRequest("Manpower request is null");
            }

            _context.ManpowerReq.Add(req);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetManpowerReqById), new { id = req.MPRID }, req); // Use MPRID instead of Id
        }

        // PUT: api/ManpowerReq/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManpowerReq(string id, [FromBody] ManpowerReq req)
        {
            if (id != req.MPRID) // Use MPRID instead of Id
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(req).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.ManpowerReq.Any(e => e.MPRID == id)) // Use MPRID instead of Id
                {
                    return NotFound("Manpower request not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/ManpowerReq/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManpowerReq(string id)
        {
            var req = await _context.ManpowerReq.FindAsync(id);
            if (req == null)
            {
                return NotFound("Manpower request not found");
            }

            _context.ManpowerReq.Remove(req);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
