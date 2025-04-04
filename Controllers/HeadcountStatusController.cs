using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HeadcountStatusController : ControllerBase
    {
        private readonly string _connectionString;

        public HeadcountStatusController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetHeadcountStatus()
        {
            var result = new List<object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        e.EmpID,
                        e.FirstName,
                        e.LastName,
                        g.EntryDateTime,
                        g.GateNo,
                        g.GateStatus,
                        o.Process,
                        o.CourseGroup,
                        o.SkillGroup AS WorkGroup,
                        c.CStatus
                    FROM EmployeeInfo e
                    LEFT JOIN GateEntry g ON e.EmpID = g.EmpID
                    LEFT JOIN OJTandInspectionSkill o ON e.EmpID = o.EmpID
                    LEFT JOIN CleanroomEntry c ON e.EmpID = c.EmpID
                ";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var entryDateTime = reader["EntryDateTime"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["EntryDateTime"];
                        var gateStatus = reader["GateStatus"]?.ToString();
                        var cStatus = reader["CStatus"]?.ToString();

                        string status = (entryDateTime == null) ? "status-missing"
                                     : (cStatus == "NOT IN" && gateStatus == "IN") ? "status-out-cleanroom"
                                     : (cStatus == "IN" && gateStatus == "IN") ? "status-in-cleanroom"
                                     : "status-unknown";

                        result.Add(new
                        {
                            empID = reader["EmpID"]?.ToString(),
                            firstName = reader["FirstName"]?.ToString(),
                            lastName = reader["LastName"]?.ToString(),
                            entryDateTime = entryDateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                            gateNo = reader["GateNo"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            courseGroup = reader["CourseGroup"]?.ToString(),
                            workGroup = reader["WorkGroup"]?.ToString(),
                            status = status
                        });
                    }
                }
            }

            return Ok(result);
        }
    }
}
