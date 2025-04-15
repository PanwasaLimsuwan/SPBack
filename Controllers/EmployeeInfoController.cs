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
                            Biz = reader["Biz"]?.ToString(),
                            Process = reader["Process"]?.ToString(),
                            PlanID = Convert.IsDBNull(reader["PlanID"]) ? null : reader["PlanID"].ToString(),
                        });
                    }
                }
            }

            return Ok(employees);
        }
    }
}
