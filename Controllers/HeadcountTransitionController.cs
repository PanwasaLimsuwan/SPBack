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
    public class HeadcountTransitionController : ControllerBase
    {
        private readonly string _connectionString;

        public HeadcountTransitionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: Get All with filters
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var transitions = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query =
                    @"
                    SELECT 
                        ROW_NUMBER() OVER (ORDER BY ht.EmpID) AS Id,
                        ht.DateTime,
                        ht.TransType,
                        ht.EmpID,
                        ei.Division,
                        ei.Department,
                        ei.Section,
                        ei.Biz,
                        ei.Process
                    FROM HeadcountTransition ht
                    LEFT JOIN EmployeeInfo ei ON ht.EmpID = ei.EmpID
                    WHERE (@division IS NULL OR ei.Division = @division)
                      AND (@department IS NULL OR ei.Department = @department)
                      AND (@section IS NULL OR ei.Section = @section)
                      AND (@biz IS NULL OR ei.Biz = @biz)
                      AND (@process IS NULL OR ei.Process = @process)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue(
                        "@division",
                        string.IsNullOrEmpty(division) || division == "ALL"
                            ? DBNull.Value
                            : division
                    );
                    cmd.Parameters.AddWithValue(
                        "@department",
                        string.IsNullOrEmpty(department) || department == "ALL"
                            ? DBNull.Value
                            : department
                    );
                    cmd.Parameters.AddWithValue(
                        "@section",
                        string.IsNullOrEmpty(section) || section == "ALL" ? DBNull.Value : section
                    );
                    cmd.Parameters.AddWithValue(
                        "@biz",
                        string.IsNullOrEmpty(biz) || biz == "ALL" ? DBNull.Value : biz
                    );
                    cmd.Parameters.AddWithValue(
                        "@process",
                        string.IsNullOrEmpty(process) || process == "ALL" ? DBNull.Value : process
                    );

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            transitions.Add(
                                new
                                {
                                    id = reader["Id"],
                                    dateTime = reader["DateTime"] == DBNull.Value
                                        ? null
                                        : ((DateTime)reader["DateTime"]).ToString(
                                            "yyyy-MM-ddTHH:mm:ss"
                                        ),
                                    transType = reader["TransType"]?.ToString(),
                                    EmpID = Convert.ToInt32(reader["EmpID"]),
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

            return Ok(transitions);
        }
    }
}
