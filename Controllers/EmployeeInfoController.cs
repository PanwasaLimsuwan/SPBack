using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

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

        // ✅ GET: api/EmployeeInfo
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
                            EmpID = (int)reader["EmpID"],
                            GID = reader["GID"] as long?,
                            FirstName = reader["FirstName"]?.ToString(),
                            LastName = reader["LastName"]?.ToString(),
                            Division = reader["Division"]?.ToString(),
                            Department = reader["Department"]?.ToString(),
                            Section = reader["Section"]?.ToString(),
                            JobGrade = reader["JobGrade"]?.ToString(),
                            BossID = reader["BossID"] as int?,
                            BossGID = reader["BossGID"] as long?,
                            CostCenter = reader["CostCenter"]?.ToString(),
                            ShiftCode = reader["ShiftCode"]?.ToString(),
                            Position = reader["Position"]?.ToString(),
                            Email = reader["Email"]?.ToString(),
                            Biz = reader["Biz"]?.ToString(),               // ✅ เพิ่ม Biz
                            Process = reader["Process"]?.ToString()       // ✅ เพิ่ม Process
                        });
                    }
                }
            }

            return Ok(employees);
        }

        // ✅ GET: api/EmployeeInfo/1001
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
                                EmpID = (int)reader["EmpID"],
                                GID = reader["GID"] as long?,
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Division = reader["Division"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                JobGrade = reader["JobGrade"]?.ToString(),
                                BossID = reader["BossID"] as int?,
                                BossGID = reader["BossGID"] as long?,
                                CostCenter = reader["CostCenter"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Position = reader["Position"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                Biz = reader["Biz"]?.ToString(),           // ✅ เพิ่ม Biz
                                Process = reader["Process"]?.ToString()   // ✅ เพิ่ม Process
                            };
                        }
                    }
                }
            }

            if (employee == null) return NotFound("Employee not found");
            return Ok(employee);
        }

        // ✅ POST: api/EmployeeInfo
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] EmployeeInfo emp)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    INSERT INTO EmployeeInfo 
                    (EmpID, GID, FirstName, LastName, Division, Department, Section, JobGrade, BossID, BossGID, CostCenter, ShiftCode, Position, Email, Biz, Process) 
                    VALUES 
                    (@EmpID, @GID, @FirstName, @LastName, @Division, @Department, @Section, @JobGrade, @BossID, @BossGID, @CostCenter, @ShiftCode, @Position, @Email, @Biz, @Process)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", emp.EmpID);
                    cmd.Parameters.AddWithValue("@GID", (object?)emp.GID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstName", emp.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", emp.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Division", emp.Division ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Section", emp.Section ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobGrade", emp.JobGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossID", (object?)emp.BossID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossGID", (object?)emp.BossGID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostCenter", emp.CostCenter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", emp.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Position", emp.Position ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", emp.Email ?? "");
                    cmd.Parameters.AddWithValue("@Biz", emp.Biz ?? (object)DBNull.Value);              // ✅ เพิ่ม Biz
                    cmd.Parameters.AddWithValue("@Process", emp.Process ?? (object)DBNull.Value);      // ✅ เพิ่ม Process

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Employee created successfully");
        }

        // ✅ PUT: api/EmployeeInfo/1001
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEmployee(int id, [FromBody] EmployeeInfo emp)
        {
            if (id != emp.EmpID) return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    UPDATE EmployeeInfo SET 
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
                        Biz = @Biz,
                        Process = @Process
                    WHERE EmpID = @EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", emp.EmpID);
                    cmd.Parameters.AddWithValue("@GID", (object?)emp.GID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@FirstName", emp.FirstName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@LastName", emp.LastName ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Division", emp.Division ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Department", emp.Department ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Section", emp.Section ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@JobGrade", emp.JobGrade ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossID", (object?)emp.BossID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@BossGID", (object?)emp.BossGID ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CostCenter", emp.CostCenter ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", emp.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Position", emp.Position ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Email", emp.Email ?? "");
                    cmd.Parameters.AddWithValue("@Biz", emp.Biz ?? (object)DBNull.Value);              // ✅ เพิ่ม Biz
                    cmd.Parameters.AddWithValue("@Process", emp.Process ?? (object)DBNull.Value);      // ✅ เพิ่ม Process

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Employee updated successfully");
        }

        // ✅ DELETE: api/EmployeeInfo/1001
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

            return Ok("Employee deleted successfully");
        }
    }
}
