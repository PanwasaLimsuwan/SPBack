using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeInfoController : ControllerBase
    {
        private readonly ApplicationDbContext _context; // ประกาศ _context
        private readonly string _connectionString;

        public EmployeeInfoController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
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
                        employees.Add(
                            new EmployeeInfo
                            {
                                // EmpID = (int)reader["EmpID"],
                                // ใช้ GetInt32 สำหรับการดึงข้อมูลประเภท int
                                EmpID = reader["EmpID"] is DBNull ? 0 : (int)reader["EmpID"],

                                // ถ้า GID เป็น null ในฐานข้อมูล, ใช้ค่าเริ่มต้น 0
                                GID = reader["GID"] is DBNull ? 0 : (long)reader["GID"],

                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Division = reader["Division"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                JobGrade = reader["JobGrade"]?.ToString(),

                                // ถ้า BossID เป็น null, ใช้ค่าเริ่มต้น 0
                                BossID = reader["BossID"] is DBNull ? 0 : (int)reader["BossID"],

                                // ถ้า BossGID เป็น null, ใช้ค่าเริ่มต้น 0
                                BossGID = reader["BossGID"] is DBNull ? 0 : (long)reader["BossGID"],

                                CostCenter = reader["CostCenter"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Position = reader["Position"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                Biz = reader["Biz"]?.ToString(),
                                Process = reader["Process"]?.ToString(),
                                // PlanID = Convert.IsDBNull(reader["PlanID"])
                                //     ? null
                                //     : reader["PlanID"].ToString(),
                                // PlanID = reader["PlanID"] is DBNull ? (int?)null : reader.GetInt32(reader.GetOrdinal("PlanID")),
                                PlanID = reader["PlanID"] is DBNull ? 0 : (int)reader["PlanID"],
                            }
                        );
                    }
                }
            }

            return Ok(employees);
        }

        // [HttpGet("get-supervisors")]
        // public async Task<IActionResult> GetSupervisors()
        // {
        //     var supervisors = new List<EmployeeInfo>();

        //     // ใช้ SqlConnection เพื่อดึงข้อมูลจากฐานข้อมูล
        //     using (SqlConnection conn = new SqlConnection(_connectionString))
        //     {
        //         await conn.OpenAsync();

        //         // คิวรีเพื่อดึงข้อมูลพนักงานที่มี Position เป็น "Supervisor"
        //         string query = "SELECT * FROM EmployeeInfo WHERE Position = 'Supervisor', 'Officer'";

        //         using (SqlCommand cmd = new SqlCommand(query, conn))
        //         using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //         {
        //             while (await reader.ReadAsync())
        //             {
        //                 // ดึงข้อมูลพนักงานจากฐานข้อมูลที่ตรงกับเงื่อนไข
        //                 supervisors.Add(
        //                     new EmployeeInfo
        //                     {
        //                         EmpID = (int)reader["EmpID"],
        //                         GID = reader["GID"] is DBNull ? 0 : (long)reader["GID"],
        //                         FirstName = reader["FirstName"]?.ToString(),
        //                         LastName = reader["LastName"]?.ToString(),
        //                         Division = reader["Division"]?.ToString(),
        //                         Department = reader["Department"]?.ToString(),
        //                         Section = reader["Section"]?.ToString(),
        //                         JobGrade = reader["JobGrade"]?.ToString(),
        //                         BossID = reader["BossID"] is DBNull ? 0 : (int)reader["BossID"],
        //                         BossGID = reader["BossGID"] is DBNull ? 0 : (long)reader["BossGID"],
        //                         CostCenter = reader["CostCenter"]?.ToString(),
        //                         ShiftCode = reader["ShiftCode"]?.ToString(),
        //                         Position = reader["Position"]?.ToString(),
        //                         Email = reader["Email"]?.ToString(),
        //                         Biz = reader["Biz"]?.ToString(),
        //                         Process = reader["Process"]?.ToString(),
        //                         PlanID = reader["PlanID"] is DBNull ? 0 : (int)reader["PlanID"],
        //                     }
        //                 );
        //             }
        //         }
        //     }
        //     return Ok(supervisors);
        // }

        [HttpGet("get-leaders")]
public async Task<IActionResult> GetLeaders()
{
    var leaders = new List<EmployeeInfo>();

    using (SqlConnection conn = new SqlConnection(_connectionString))
    {
        await conn.OpenAsync();

        // ดึงข้อมูลพนักงานที่มี Position เป็น "Supervisor" หรือ "Officer"
        string query = "SELECT * FROM EmployeeInfo WHERE Position IN ('Supervisor', 'Officer')";

        using (SqlCommand cmd = new SqlCommand(query, conn))
        using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        {
            while (await reader.ReadAsync())
            {
                leaders.Add(
                    new EmployeeInfo
                    {
                        EmpID = (int)reader["EmpID"],
                        GID = reader["GID"] is DBNull ? 0 : (long)reader["GID"],
                        FirstName = reader["FirstName"]?.ToString(),
                        LastName = reader["LastName"]?.ToString(),
                        Division = reader["Division"]?.ToString(),
                        Department = reader["Department"]?.ToString(),
                        Section = reader["Section"]?.ToString(),
                        JobGrade = reader["JobGrade"]?.ToString(),
                        BossID = reader["BossID"] is DBNull ? 0 : (int)reader["BossID"],
                        BossGID = reader["BossGID"] is DBNull ? 0 : (long)reader["BossGID"],
                        CostCenter = reader["CostCenter"]?.ToString(),
                        ShiftCode = reader["ShiftCode"]?.ToString(),
                        Position = reader["Position"]?.ToString(),
                        Email = reader["Email"]?.ToString(),
                        Biz = reader["Biz"]?.ToString(),
                        Process = reader["Process"]?.ToString(),
                        PlanID = reader["PlanID"] is DBNull ? 0 : (int)reader["PlanID"],
                    }
                );
            }
        }
    }
    return Ok(leaders);
}

        // [HttpGet("get-technicians")]
        // public async Task<IActionResult> GetTechnicians()
        // {
        //     var technicians = new List<EmployeeInfo>();

        //     // ใช้ SqlConnection เพื่อดึงข้อมูลจากฐานข้อมูล
        //     using (SqlConnection conn = new SqlConnection(_connectionString))
        //     {
        //         await conn.OpenAsync();

        //         // คิวรีเพื่อดึงข้อมูลพนักงานที่มี Position เป็น "Technician"
        //         string query = "SELECT * FROM EmployeeInfo WHERE Position = 'Technician'";

        //         using (SqlCommand cmd = new SqlCommand(query, conn))
        //         using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
        //         {
        //             while (await reader.ReadAsync())
        //             {
        //                 // ดึงข้อมูลพนักงานจากฐานข้อมูลที่ตรงกับเงื่อนไข
        //                 technicians.Add(
        //                     new EmployeeInfo
        //                     {
        //                         EmpID = (int)reader["EmpID"],
        //                         GID = reader["GID"] is DBNull ? 0 : (long)reader["GID"],
        //                         FirstName = reader["FirstName"]?.ToString(),
        //                         LastName = reader["LastName"]?.ToString(),
        //                         Division = reader["Division"]?.ToString(),
        //                         Department = reader["Department"]?.ToString(),
        //                         Section = reader["Section"]?.ToString(),
        //                         JobGrade = reader["JobGrade"]?.ToString(),
        //                         BossID = reader["BossID"] is DBNull ? 0 : (int)reader["BossID"],
        //                         BossGID = reader["BossGID"] is DBNull ? 0 : (long)reader["BossGID"],
        //                         CostCenter = reader["CostCenter"]?.ToString(),
        //                         ShiftCode = reader["ShiftCode"]?.ToString(),
        //                         Position = reader["Position"]?.ToString(),
        //                         Email = reader["Email"]?.ToString(),
        //                         Biz = reader["Biz"]?.ToString(),
        //                         Process = reader["Process"]?.ToString(),
        //                         PlanID = reader["PlanID"] is DBNull ? 0 : (int)reader["PlanID"],
        //                     }
        //                 );
        //             }
        //         }
        //     }

        //     // ส่งข้อมูลพนักงานที่มี Position เป็น Technician กลับไป
        //     return Ok(technicians);
        // }

        [HttpGet("get-technicians")]
        public async Task<IActionResult> GetTechnicians()
        {
            var technicians = new List<EmployeeInfo>();

            // ใช้ SqlConnection เพื่อดึงข้อมูลจากฐานข้อมูล
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // คิวรีเพื่อดึงข้อมูลพนักงานที่มี Position เป็น "Technician"
                string query = "SELECT * FROM EmployeeInfo WHERE Position = 'Technician'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // ดึงข้อมูลพนักงานจากฐานข้อมูลที่ตรงกับเงื่อนไข
                        technicians.Add(
                            new EmployeeInfo
                            {
                                EmpID = (int)reader["EmpID"],
                                GID = reader["GID"] is DBNull ? 0 : (long)reader["GID"],
                                FirstName = reader["FirstName"]?.ToString(),
                                LastName = reader["LastName"]?.ToString(),
                                Division = reader["Division"]?.ToString(),
                                Department = reader["Department"]?.ToString(),
                                Section = reader["Section"]?.ToString(),
                                JobGrade = reader["JobGrade"]?.ToString(),
                                BossID = reader["BossID"] is DBNull ? 0 : (int)reader["BossID"],
                                BossGID = reader["BossGID"] is DBNull ? 0 : (long)reader["BossGID"],
                                CostCenter = reader["CostCenter"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Position = reader["Position"]?.ToString(),
                                Email = reader["Email"]?.ToString(),
                                Biz = reader["Biz"]?.ToString(),
                                Process = reader["Process"]?.ToString(),
                                PlanID = reader["PlanID"] is DBNull ? 0 : (int)reader["PlanID"],
                            }
                        );
                    }
                }
            }

            // ส่งข้อมูลพนักงานที่มี Position เป็น Technician กลับไป
            return Ok(technicians);
        }


        [HttpPut("edit/{empID}")]
        public async Task<IActionResult> EditEmployee(
            int empID,
            [FromBody] EmployeeInfo employeeInfo
        )
        {
            if (employeeInfo == null || empID != employeeInfo.EmpID)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var existingEmployeeInfo = await _context.EmployeeInfo.FirstOrDefaultAsync(a =>
                    a.EmpID == empID
                );
                if (existingEmployeeInfo == null)
                {
                    return NotFound("Employee not found.");
                }

                // อัปเดตเฉพาะฟิลด์ที่มีการเปลี่ยนแปลง
                if (!string.IsNullOrEmpty(employeeInfo.FirstName))
                {
                    existingEmployeeInfo.FirstName = employeeInfo.FirstName;
                }
                if (!string.IsNullOrEmpty(employeeInfo.LastName))
                {
                    existingEmployeeInfo.LastName = employeeInfo.LastName;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Email))
                {
                    existingEmployeeInfo.Email = employeeInfo.Email;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Division))
                {
                    existingEmployeeInfo.Division = employeeInfo.Division;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Department))
                {
                    existingEmployeeInfo.Department = employeeInfo.Department;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Section))
                {
                    existingEmployeeInfo.Section = employeeInfo.Section;
                }
                if (!string.IsNullOrEmpty(employeeInfo.JobGrade))
                {
                    existingEmployeeInfo.JobGrade = employeeInfo.JobGrade;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Position))
                {
                    existingEmployeeInfo.Position = employeeInfo.Position;
                }
                if (!string.IsNullOrEmpty(employeeInfo.ShiftCode))
                {
                    existingEmployeeInfo.ShiftCode = employeeInfo.ShiftCode;
                }
                if (!string.IsNullOrEmpty(employeeInfo.CostCenter))
                {
                    existingEmployeeInfo.CostCenter = employeeInfo.CostCenter;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Biz))
                {
                    existingEmployeeInfo.Biz = employeeInfo.Biz;
                }
                if (!string.IsNullOrEmpty(employeeInfo.Process))
                {
                    existingEmployeeInfo.Process = employeeInfo.Process;
                }
                // if (!string.IsNullOrEmpty(employeeInfo.PlanID))
                // {
                //     existingEmployeeInfo.PlanID = employeeInfo.PlanID;
                // }
                // ใช้การตรวจสอบ Nullable<int> สำหรับ PlanID
                if (employeeInfo.PlanID != null)
                {
                    existingEmployeeInfo.PlanID = employeeInfo.PlanID;
                }

                // ไม่ต้องบังคับการอัปเดต Role และ PasswordHash
                // คงค่าเดิมในฐานข้อมูล

                // ถ้าไม่มีการอัปเดตในฟิลด์ใด ๆ ให้ส่งกลับเป็นข้อความ "No changes detected"
                if (
                    string.IsNullOrEmpty(existingEmployeeInfo.FirstName)
                    && string.IsNullOrEmpty(existingEmployeeInfo.LastName)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Email)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Division)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Department)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Section)
                    && string.IsNullOrEmpty(existingEmployeeInfo.JobGrade)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Position)
                    && string.IsNullOrEmpty(existingEmployeeInfo.ShiftCode)
                    && string.IsNullOrEmpty(existingEmployeeInfo.CostCenter)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Biz)
                    && string.IsNullOrEmpty(existingEmployeeInfo.Process)
                    // && string.IsNullOrEmpty(existingEmployeeInfo.PlanID)
                    && existingEmployeeInfo.PlanID == null  // ตรวจสอบ PlanID ถ้าไม่เปลี่ยนแปลง
                )
                {
                    return BadRequest("One or more fields are missing or invalid.");
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during edit: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }
        
        [HttpGet("get-user-process")]
        public async Task<IActionResult> GetUserProcess([FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return BadRequest("Email is required.");
            }

            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = "SELECT Process FROM EmployeeInfo WHERE Email = @email";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@email", SqlDbType.NVarChar, 255).Value = email;

                var process = await cmd.ExecuteScalarAsync();

                if (process == null || process == DBNull.Value)
                {
                    return NotFound(new { message = "User not found or Process not assigned" });
                }

                return Ok(new { process = process.ToString() });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetUserProcess: {ex.Message}");
                return Problem(detail: ex.Message, statusCode: 500);
            }
        }
    }
}
