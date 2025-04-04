// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class EmployeeInfoController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         // Constructor ที่รับ ApplicationDbContext มาใช้
//         public EmployeeInfoController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/EmployeeInfo
//         // ดึงข้อมูลพนักงานทั้งหมดจากฐานข้อมูล
//         [HttpGet]
//         public async Task<IActionResult> GetEmployees()
//         {
//             var employees = await _context.EmployeeInfo.ToListAsync();
//             return Ok(employees);
//         }

//         // GET: api/EmployeeInfo/{id}
//         // ดึงข้อมูลพนักงานที่มี EmpID ตามที่ระบุ
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetEmployeeById(int id)
//         {
//             var employee = await _context.EmployeeInfo.FindAsync(id);
//             if (employee == null)
//             {
//                 return NotFound("Employee not found");
//             }
//             return Ok(employee);
//         }

//         // POST: api/EmployeeInfo
//         // สร้างพนักงานใหม่
//         [HttpPost]
//         public async Task<IActionResult> CreateEmployee([FromBody] EmployeeInfo employee)
//         {
//             if (employee == null)
//             {
//                 return BadRequest("Employee is null");
//             }

//             _context.EmployeeInfo.Add(employee);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetEmployeeById), new { id = employee.EmpID }, employee);
//         }

//         // PUT: api/EmployeeInfo/{id}
//         // อัปเดตข้อมูลพนักงานตาม EmpID
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeInfo employee)
//         {
//             if (id != employee.EmpID)
//             {
//                 return BadRequest("Employee ID mismatch");
//             }

//             _context.Entry(employee).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.EmployeeInfo.Any(e => e.EmpID == id))
//                 {
//                     return NotFound("Employee not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent(); // HTTP 204 No Content
//         }

//         // DELETE: api/EmployeeInfo/{id}
//         // ลบข้อมูลพนักงานตาม EmpID
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteEmployee(int id)
//         {
//             var employee = await _context.EmployeeInfo.FindAsync(id);
//             if (employee == null)
//             {
//                 return NotFound("Employee not found");
//             }

//             _context.EmployeeInfo.Remove(employee);
//             await _context.SaveChangesAsync();

//             return NoContent(); // HTTP 204 No Content
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeInfoController : ControllerBase
    {
        private readonly string _connectionString;

        public EmployeeInfoController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/EmployeeInfo
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees()
        {
            var employees = new List<EmployeeInfo>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM EmployeeInfo";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        employees.Add(new EmployeeInfo
                        {
                            EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                            GID = reader.IsDBNull(reader.GetOrdinal("GID")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("GID")),
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString(),
                            Division = reader["Division"]?.ToString(),
                            Department = reader["Department"]?.ToString(),
                            Section = reader["Section"]?.ToString(),
                            JobGrade = reader["JobGrade"]?.ToString(),
                            BossID = reader.IsDBNull(reader.GetOrdinal("BossID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BossID")),
                            BossGID = reader.IsDBNull(reader.GetOrdinal("BossGID")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("BossGID")),
                            CostCenter = reader["CostCenter"]?.ToString(),
                            ShiftCode = reader["ShiftCode"]?.ToString(),
                            Position = reader["Position"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            PlanID = reader.GetInt32(reader.GetOrdinal("PlanID"))
                        });
                    }
                }
            }

            return Ok(employees);
        }

        // GET: api/EmployeeInfo/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            EmployeeInfo employee = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "SELECT * FROM EmployeeInfo WHERE EmpID = @EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            employee = new EmployeeInfo
                            {
                                EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                                GID = reader.IsDBNull(reader.GetOrdinal("GID")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("GID")),
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Division = reader["Division"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                JobGrade = reader["JobGrade"]?.ToString(),
                                BossID = reader.IsDBNull(reader.GetOrdinal("BossID")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("BossID")),
                                BossGID = reader.IsDBNull(reader.GetOrdinal("BossGID")) ? null : (long?)reader.GetInt64(reader.GetOrdinal("BossGID")),
                                CostCenter = reader["CostCenter"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Position = reader["Position"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                PlanID = reader.GetInt32(reader.GetOrdinal("PlanID"))
                            };
                        }
                    }
                }
            }

            if (employee == null)
                return NotFound("Employee not found");

            return Ok(employee);
        }

        // POST: api/EmployeeInfo
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeInfo employee)
        {
            if (employee == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"INSERT INTO EmployeeInfo 
                    (GID, FirstName, LastName, Division, Department, Section, JobGrade, BossID, BossGID, CostCenter, ShiftCode, Position, Email, PlanID)
                    VALUES 
                    (@GID, @FirstName, @LastName, @Division, @Department, @Section, @JobGrade, @BossID, @BossGID, @CostCenter, @ShiftCode, @Position, @Email, @PlanID)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GID", (object?)employee.GID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstName", (object?)employee.FirstName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", (object?)employee.LastName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Division", (object?)employee.Division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", (object?)employee.Department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Section", (object?)employee.Section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobGrade", (object?)employee.JobGrade ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossID", (object?)employee.BossID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossGID", (object?)employee.BossGID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostCenter", (object?)employee.CostCenter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", (object?)employee.ShiftCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Position", (object?)employee.Position ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", employee.Email);
                    cmd.Parameters.AddWithValue("@PlanID", employee.PlanID);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Employee created");
        }

        // PUT: api/EmployeeInfo/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeInfo employee)
        {
            if (id != employee.EmpID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"UPDATE EmployeeInfo SET 
                    GID = @GID,
                    FirstName = @FirstName,
                    LastName = @LastName,
                    Division = @Division,
                    Department = @Department,
                    Section = @Section,
                    JobGrade = @JobGrade,
                    BossID = @BossID,
                    BossGID = @BossGID,
                    CostCenter = @CostCenter,
                    ShiftCode = @ShiftCode,
                    Position = @Position,
                    Email = @Email,
                    PlanID = @PlanID
                    WHERE EmpID = @EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", employee.EmpID);
                    cmd.Parameters.AddWithValue("@GID", (object?)employee.GID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstName", (object?)employee.FirstName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", (object?)employee.LastName ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Division", (object?)employee.Division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", (object?)employee.Department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Section", (object?)employee.Section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobGrade", (object?)employee.JobGrade ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossID", (object?)employee.BossID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossGID", (object?)employee.BossGID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostCenter", (object?)employee.CostCenter ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", (object?)employee.ShiftCode ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Position", (object?)employee.Position ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", employee.Email);
                    cmd.Parameters.AddWithValue("@PlanID", employee.PlanID);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("Employee not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/EmployeeInfo/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "DELETE FROM EmployeeInfo WHERE EmpID = @EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", id);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("Employee not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class EmployeeInfoController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public EmployeeInfoController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetEmployees()
//         {
//             var query = _db.Query("SELECT * FROM EmployeeInfo");
//             return _res.Success(query);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetEmployeeById(int id)
//         {
//             var query = _db.Query($"SELECT * FROM EmployeeInfo WHERE EmpID = {id}");
//             if (query.Rows.Count == 0)
//             {
//                 return _res.NotFound("Employee not found");
//             }
//             return _res.Success(query);
//         }

//         [HttpPost]
//         public async Task<JsonResult> CreateEmployee([FromBody] EmployeeInfo employee)
//         {
//             if (employee == null)
//                 return _res.BadRequest("Employee is null");

//             var insertQuery = $@"
//                 INSERT INTO EmployeeInfo (EmpID, FirstName, LastName, Division, Department, Position, Email, ShiftCode, Section)
//                 VALUES ('{employee.EmpID}', '{employee.FirstName}', '{employee.LastName}', '{employee.Division}',
//                         '{employee.Department}', '{employee.Position}', '{employee.Email}', '{employee.ShiftCode}', '{employee.Section}')";

//             var result = _db.QueryPOST(insertQuery);
//             return _res.Created(result);
//         }

//         [HttpPut("{id}")]
//         public async Task<JsonResult> UpdateEmployee(int id, [FromBody] EmployeeInfo employee)
//         {
//             if (id != employee.EmpID)
//                 return _res.BadRequest("Employee ID mismatch");

//             var updateQuery = $@"
//                 UPDATE EmployeeInfo SET 
//                     FirstName = '{employee.FirstName}',
//                     LastName = '{employee.LastName}',
//                     Division = '{employee.Division}',
//                     Department = '{employee.Department}',
//                     Position = '{employee.Position}',
//                     Email = '{employee.Email}',
//                     ShiftCode = '{employee.ShiftCode}',
//                     Section = '{employee.Section}'
//                 WHERE EmpID = {id}";

//             var result = _db.QueryPOST(updateQuery);
//             return _res.Success(result);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> DeleteEmployee(int id)
//         {
//             var deleteQuery = $"DELETE FROM EmployeeInfo WHERE EmpID = {id}";
//             var result = _db.QueryPOST(deleteQuery);
//             return _res.Success(result);
//         }
//     }
// }