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

        // GET: api/ManpowerPlan
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = new List<ManpowerPlan>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM ManpowerPlan", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    plans.Add(new ManpowerPlan
                    {
                        PlanID = reader.GetInt32(0),
                        Date = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        EmpID = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Attendance = reader.IsDBNull(3) ? null : reader.GetString(3),
                        ShiftCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Shift = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PlannedHeadcount = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        ActualHeadcount = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                    });
                }
            }

            return Ok(plans);
        }

        // GET: api/ManpowerPlan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            ManpowerPlan plan = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM ManpowerPlan WHERE PlanID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    plan = new ManpowerPlan
                    {
                        PlanID = reader.GetInt32(0),
                        Date = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        EmpID = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Attendance = reader.IsDBNull(3) ? null : reader.GetString(3),
                        ShiftCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Shift = reader.IsDBNull(5) ? null : reader.GetString(5),
                        PlannedHeadcount = reader.IsDBNull(6) ? null : reader.GetInt32(6),
                        ActualHeadcount = reader.IsDBNull(7) ? null : reader.GetInt32(7)
                    };
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
