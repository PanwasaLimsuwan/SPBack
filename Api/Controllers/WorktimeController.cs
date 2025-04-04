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
    public class WorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public WorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Worktime
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var worktimes = new List<Worktime>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM Worktime", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    worktimes.Add(new Worktime
                    {
                        WorkTimeID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        Date = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        WorkedHours = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                        OT_Hours = reader.IsDBNull(4) ? null : reader.GetFloat(4),
                        EICC_Hours = reader.IsDBNull(5) ? null : reader.GetFloat(5),
                        OverloadHours = reader.IsDBNull(6) ? null : reader.GetFloat(6),
                        Status = reader.GetString(7)
                    });
                }
            }

            return Ok(worktimes);
        }

        // GET: api/Worktime/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Worktime result = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM Worktime WHERE WorkTimeID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new Worktime
                    {
                        WorkTimeID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        Date = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        WorkedHours = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                        OT_Hours = reader.IsDBNull(4) ? null : reader.GetFloat(4),
                        EICC_Hours = reader.IsDBNull(5) ? null : reader.GetFloat(5),
                        OverloadHours = reader.IsDBNull(6) ? null : reader.GetFloat(6),
                        Status = reader.GetString(7)
                    };
                }
            }

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/Worktime
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Worktime worktime)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO Worktime (EmpID, Date, WorkedHours, OT_Hours, EICC_Hours, OverloadHours, Status)
                    VALUES (@EmpID, @Date, @WorkedHours, @OT_Hours, @EICC_Hours, @OverloadHours, @Status)", conn);

                cmd.Parameters.AddWithValue("@EmpID", worktime.EmpID);
                cmd.Parameters.AddWithValue("@Date", (object)worktime.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkedHours", (object)worktime.WorkedHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OT_Hours", (object)worktime.OT_Hours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EICC_Hours", (object)worktime.EICC_Hours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OverloadHours", (object)worktime.OverloadHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", worktime.Status ?? "");

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/Worktime/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Worktime worktime)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE Worktime SET 
                        EmpID = @EmpID,
                        Date = @Date,
                        WorkedHours = @WorkedHours,
                        OT_Hours = @OT_Hours,
                        EICC_Hours = @EICC_Hours,
                        OverloadHours = @OverloadHours,
                        Status = @Status
                    WHERE WorkTimeID = @WorkTimeID", conn);

                cmd.Parameters.AddWithValue("@WorkTimeID", id);
                cmd.Parameters.AddWithValue("@EmpID", worktime.EmpID);
                cmd.Parameters.AddWithValue("@Date", (object)worktime.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@WorkedHours", (object)worktime.WorkedHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OT_Hours", (object)worktime.OT_Hours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EICC_Hours", (object)worktime.EICC_Hours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@OverloadHours", (object)worktime.OverloadHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", worktime.Status ?? "");

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Worktime/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM Worktime WHERE WorkTimeID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
