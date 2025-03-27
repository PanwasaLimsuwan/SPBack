using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OJTandInspectionSkillController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public OJTandInspectionSkillController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ดึงข้อมูลจากทั้ง EmployeeInfo และ OJTandInspectionSkill โดยใช้ EmpID
        [HttpGet]
        public async Task<IActionResult> GetOJTandInspectionSkills()
        {
            var combinedData = await (from employee in _context.EmployeeInfo
                                      join skill in _context.OJTandInspectionSkill
                                      on employee.EmpID.ToString() equals skill.EmpID
                                      select new 
                                      {
                                          EmpID = employee.EmpID,
                                          FirstName = employee.FirstName,
                                          LastName = employee.LastName,
                                          Division = employee.Division,
                                          Department = employee.Department,
                                          Position = employee.Position,
                                          Email = employee.Email,
                                          CourseNo = skill.CourseNo,
                                          CourseGroup = skill.CourseGroup,
                                          Biz = skill.Biz,
                                          Process = skill.Process,
                                          SkillGroup = skill.SkillGroup,
                                          Active = skill.Active
                                      }).ToListAsync();

            return Ok(combinedData);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOJTandInspectionSkillById(string id)
        {
            var skill = await _context.OJTandInspectionSkill.FindAsync(id);
            if (skill == null)
            {
                return NotFound("OJT and inspection skill not found");
            }
            return Ok(skill);
        }

        [HttpPost]
        public async Task<IActionResult> CreateOJTandInspectionSkill([FromBody] OJTandInspectionSkill skill)
        {
            if (skill == null)
            {
                return BadRequest("OJT and inspection skill is null");
            }

            _context.OJTandInspectionSkill.Add(skill);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetOJTandInspectionSkillById), new { id = skill.CourseNo }, skill);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateOJTandInspectionSkill(string id, [FromBody] OJTandInspectionSkill skill)
        {
            if (id != skill.CourseNo)
            {
                return BadRequest("ID mismatch");
            }

            _context.Entry(skill).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.OJTandInspectionSkill.Any(e => e.CourseNo == id))
                {
                    return NotFound("OJT and inspection skill not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOJTandInspectionSkill(string id)
        {
            var skill = await _context.OJTandInspectionSkill.FindAsync(id);
            if (skill == null)
            {
                return NotFound("OJT and inspection skill not found");
            }

            _context.OJTandInspectionSkill.Remove(skill);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
