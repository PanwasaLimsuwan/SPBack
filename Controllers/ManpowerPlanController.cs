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
    public class ManpowerPlanController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerPlanController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: api/ManpowerPlan (พร้อม filters)
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process)
        {
            var plans = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        mp.PlanID,
                        mp.Date,
                        mp.ShiftCode,
                        mp.Shift,
                        mp.PlannedHeadcount,
                        mp.ActualHeadcount,
                        ei.Division,
                        ei.Department,
                        ei.Section,
                        ei.Biz,
                        ei.Process
                    FROM ManpowerPlan mp
                    LEFT JOIN EmployeeInfo ei ON mp.EmpID = ei.EmpID
                    WHERE (@division IS NULL OR ei.Division = @division)
                      AND (@department IS NULL OR ei.Department = @department)
                      AND (@section IS NULL OR ei.Section = @section)
                      AND (@biz IS NULL OR ei.Biz = @biz)
                      AND (@process IS NULL OR ei.Process = @process)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@division", string.IsNullOrEmpty(division) || division == "ALL" ? DBNull.Value : division);
                    cmd.Parameters.AddWithValue("@department", string.IsNullOrEmpty(department) || department == "ALL" ? DBNull.Value : department);
                    cmd.Parameters.AddWithValue("@section", string.IsNullOrEmpty(section) || section == "ALL" ? DBNull.Value : section);
                    cmd.Parameters.AddWithValue("@biz", string.IsNullOrEmpty(biz) || biz == "ALL" ? DBNull.Value : biz);
                    cmd.Parameters.AddWithValue("@process", string.IsNullOrEmpty(process) || process == "ALL" ? DBNull.Value : process);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            plans.Add(new
                            {
                                planID = reader["PlanID"],
                                date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                                shiftCode = reader["ShiftCode"]?.ToString(),
                                shift = reader["Shift"]?.ToString(),
                                plannedHeadcount = reader["PlannedHeadcount"] == DBNull.Value ? null : (int?)reader["PlannedHeadcount"],
                                actualHeadcount = reader["ActualHeadcount"] == DBNull.Value ? null : (int?)reader["ActualHeadcount"],
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString()
                            });
                        }
                    }
                }
            }

            return Ok(plans);
        }
    }
}
