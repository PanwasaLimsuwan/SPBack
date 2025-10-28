using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

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
                        employees.Add(
                            new EmployeeInfo
                            {
                                EmpID = (int)reader["EmpID"],

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
                                PlanID = Convert.IsDBNull(reader["PlanID"])
                                    ? null
                                    : reader["PlanID"].ToString(),
                            }
                        );
                    }
                }
            }

            return Ok(employees);
        }

        [HttpGet("get-supervisors")]
        public async Task<IActionResult> GetSupervisors()
        {
            var supervisors = new List<EmployeeInfo>();

            // ใช้ SqlConnection เพื่อดึงข้อมูลจากฐานข้อมูล
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // คิวรีเพื่อดึงข้อมูลพนักงานที่มี Position เป็น "Supervisor"
                string query = "SELECT * FROM EmployeeInfo WHERE Position = 'Supervisor'";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        // ดึงข้อมูลพนักงานจากฐานข้อมูลที่ตรงกับเงื่อนไข
                        supervisors.Add(
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
                                PlanID = Convert.IsDBNull(reader["PlanID"])
                                    ? null
                                    : reader["PlanID"].ToString(),
                            }
                        );
                    }
                }
            }

            // ส่งข้อมูลพนักงานที่มี Position เป็น Technician กลับไป
            return Ok(supervisors);
        }

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
                                PlanID = Convert.IsDBNull(reader["PlanID"])
                                    ? null
                                    : reader["PlanID"].ToString(),
                            }
                        );
                    }
                }
            }

            // ส่งข้อมูลพนักงานที่มี Position เป็น Technician กลับไป
            return Ok(technicians);
        }
    }
}
