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

        // ✅ GET: api/ManpowerPlan (แบบ Join EmployeeInfo)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        mp.PlanID,
                        mp.Date,
                        mp.EmpID,
                        mp.Attendance,
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
                    LEFT JOIN EmployeeInfo ei ON mp.EmpID = ei.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        plans.Add(new
                        {
                            planID = reader["PlanID"],
                            date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                            empID = reader["EmpID"]?.ToString(),
                            attendance = reader["Attendance"]?.ToString(),
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

            return Ok(plans);
        }

        // ✅ GET: api/ManpowerPlan/{id} (แบบ Join EmployeeInfo)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            object plan = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        mp.PlanID,
                        mp.Date,
                        mp.EmpID,
                        mp.Attendance,
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
                    WHERE mp.PlanID = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            plan = new
                            {
                                planID = reader["PlanID"],
                                date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                                empID = reader["EmpID"]?.ToString(),
                                attendance = reader["Attendance"]?.ToString(),
                                shiftCode = reader["ShiftCode"]?.ToString(),
                                shift = reader["Shift"]?.ToString(),
                                plannedHeadcount = reader["PlannedHeadcount"] == DBNull.Value ? null : (int?)reader["PlannedHeadcount"],
                                actualHeadcount = reader["ActualHeadcount"] == DBNull.Value ? null : (int?)reader["ActualHeadcount"],
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (plan == null)
                return NotFound();

            return Ok(plan);
        }

        // POST: api/ManpowerPlan
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManpowerPlan plan)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO ManpowerPlan 
                    (Date, EmpID, Attendance, ShiftCode, Shift, PlannedHeadcount, ActualHeadcount)
                    VALUES 
                    (@Date, @EmpID, @Attendance, @ShiftCode, @Shift, @PlannedHeadcount, @ActualHeadcount)", conn);

                cmd.Parameters.AddWithValue("@Date", (object?)plan.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)plan.EmpID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Attendance", (object?)plan.Attendance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ShiftCode", (object?)plan.ShiftCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Shift", (object?)plan.Shift ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PlannedHeadcount", (object?)plan.PlannedHeadcount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActualHeadcount", (object?)plan.ActualHeadcount ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/ManpowerPlan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ManpowerPlan plan)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE ManpowerPlan SET 
                        Date = @Date,
                        EmpID = @EmpID,
                        Attendance = @Attendance,
                        ShiftCode = @ShiftCode,
                        Shift = @Shift,
                        PlannedHeadcount = @PlannedHeadcount,
                        ActualHeadcount = @ActualHeadcount
                    WHERE PlanID = @PlanID", conn);

                cmd.Parameters.AddWithValue("@PlanID", id);
                cmd.Parameters.AddWithValue("@Date", (object?)plan.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)plan.EmpID ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Attendance", (object?)plan.Attendance ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ShiftCode", (object?)plan.ShiftCode ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Shift", (object?)plan.Shift ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@PlannedHeadcount", (object?)plan.PlannedHeadcount ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ActualHeadcount", (object?)plan.ActualHeadcount ?? DBNull.Value);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/ManpowerPlan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM ManpowerPlan WHERE PlanID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
