using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OJTandInspectionSkillController : ControllerBase
    {
        private readonly string _connectionString;

        public OJTandInspectionSkillController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: api/OJTandInspectionSkill (With Join EmployeeInfo)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        o.CourseNo,
                        o.CourseGroup,
                        o.Biz,
                        o.Process,
                        o.CerNo,
                        o.Active,
                        o.SkillGroup,
                        o.EmpID,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz AS EmployeeBiz,
                        e.Process AS EmployeeProcess
                    FROM OJTandInspectionSkill o
                    LEFT JOIN EmployeeInfo e ON o.EmpID = e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new
                        {
                            courseNo = reader["CourseNo"]?.ToString(),
                            courseGroup = reader["CourseGroup"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            cerNo = reader["CerNo"]?.ToString(),
                            active = reader["Active"] == DBNull.Value ? null : (int?)reader["Active"],
                            skillGroup = reader["SkillGroup"]?.ToString(),
                           EmpID = Convert.ToInt32(reader["EmpID"]),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            employeeBiz = reader["EmployeeBiz"]?.ToString(),
                            employeeProcess = reader["EmployeeProcess"]?.ToString()
                        });
                    }
                }
            }

            return Ok(result);
        }
    }
}
