using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public WorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetWorktime(
            [FromQuery] string division,
            [FromQuery] string department,
            [FromQuery] string section,
            [FromQuery] string biz,
            [FromQuery] string process
        )
        {
            var results = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // ✅ Query จาก CalculatedWorktime + Join EmployeeInfo
                var query =
                    @"
                    SELECT 
                        cw.WorktimeID,
                        cw.EmpID,
                        cw.Date,
                        cw.OTHours,
                        cw.Status,
                        ei.Division,
                        ei.Department,
                        ei.Section,
                        ei.Biz,
                        ei.Process
                    FROM CalculatedWorktime cw
                    JOIN EmployeeInfo ei ON cw.EmpID = CAST(ei.EmpID AS VARCHAR)
                    WHERE 1=1";

                // ✅ Filter ตามที่รับมา
                if (!string.IsNullOrEmpty(division))
                    query += " AND ei.Division = @division";
                if (!string.IsNullOrEmpty(department))
                    query += " AND ei.Department = @department";
                if (!string.IsNullOrEmpty(section))
                    query += " AND ei.Section = @section";
                if (!string.IsNullOrEmpty(biz))
                    query += " AND ei.Biz = @biz";
                if (!string.IsNullOrEmpty(process))
                    query += " AND ei.Process = @process";

                using (var cmd = new SqlCommand(query, conn))
                {
                    if (!string.IsNullOrEmpty(division))
                        cmd.Parameters.AddWithValue("@division", division);
                    if (!string.IsNullOrEmpty(department))
                        cmd.Parameters.AddWithValue("@department", department);
                    if (!string.IsNullOrEmpty(section))
                        cmd.Parameters.AddWithValue("@section", section);
                    if (!string.IsNullOrEmpty(biz))
                        cmd.Parameters.AddWithValue("@biz", biz);
                    if (!string.IsNullOrEmpty(process))
                        cmd.Parameters.AddWithValue("@process", process);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(
                                new
                                {
                                    worktimeID = reader.GetInt32(0),
                                    empID = Convert.ToInt32(reader["EmpID"]),
                                    date = reader.GetDateTime(2),
                                    otHours = reader.IsDBNull(3) ? 0 : reader.GetDouble(3),
                                    status = reader["Status"]?.ToString(),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                }
                            );
                        }
                    }
                }
            }

            return Ok(results);
        }
    }
}
