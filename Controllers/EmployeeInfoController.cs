using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeInfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        // Constructor ที่รับ ApplicationDbContext มาใช้
        public EmployeeInfoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/EmployeeInfo
        // ดึงข้อมูลพนักงานทั้งหมดจากฐานข้อมูล
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.EmployeeInfo.ToListAsync();
            return Ok(employees);
        }

        // GET: api/EmployeeInfo/{id}
        // ดึงข้อมูลพนักงานที่มี EmpID ตามที่ระบุ
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            var employee = await _context.EmployeeInfo.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }
            return Ok(employee);
        }

        // POST: api/EmployeeInfo
        // สร้างพนักงานใหม่
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeInfo employee)
        {
            if (employee == null)
            {
                return BadRequest("Employee is null");
            }

            _context.EmployeeInfo.Add(employee);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmpID }, employee);
        }

        // PUT: api/EmployeeInfo/{id}
        // อัปเดตข้อมูลพนักงานตาม EmpID
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeInfo employee)
        {
            if (id != employee.EmpID)
            {
                return BadRequest("Employee ID mismatch");
            }

            _context.Entry(employee).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.EmployeeInfo.Any(e => e.EmpID == id))
                {
                    return NotFound("Employee not found");
                }
                else
                {
                    throw;
                }
            }

            return NoContent(); // HTTP 204 No Content
        }

        // DELETE: api/EmployeeInfo/{id}
        // ลบข้อมูลพนักงานตาม EmpID
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _context.EmployeeInfo.FindAsync(id);
            if (employee == null)
            {
                return NotFound("Employee not found");
            }

            _context.EmployeeInfo.Remove(employee);
            await _context.SaveChangesAsync();

            return NoContent(); // HTTP 204 No Content
        }
    }
}
