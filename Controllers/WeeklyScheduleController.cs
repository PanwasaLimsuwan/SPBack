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
    public class WeeklyScheduleController : ControllerBase
    {
        private readonly string _connectionString;

        public WeeklyScheduleController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/WeeklySchedule
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var schedules = new List<WeeklySchedule>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM WeeklySchedule", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    schedules.Add(new WeeklySchedule
                    {
                        WeekID = reader.GetInt32(0),
                        Year = reader.GetInt32(1),
                        Month = reader.GetString(2),
                        Week = reader.GetInt32(3),
                        StartDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        EndDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        AbsentCount = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                    });
                }
            }

            return Ok(schedules);
        }

        // GET: api/WeeklySchedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            WeeklySchedule result = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM WeeklySchedule WHERE WeekID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new WeeklySchedule
                    {
                        WeekID = reader.GetInt32(0),
                        Year = reader.GetInt32(1),
                        Month = reader.GetString(2),
                        Week = reader.GetInt32(3),
                        StartDate = reader.IsDBNull(4) ? null : reader.GetDateTime(4),
                        EndDate = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                        AbsentCount = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                    };
                }
            }

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/WeeklySchedule
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeeklySchedule schedule)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO WeeklySchedule 
                        (Year, Month, Week, StartDate, EndDate, AbsentCount)
                    VALUES 
                        (@Year, @Month, @Week, @StartDate, @EndDate, @AbsentCount)", conn);

                cmd.Parameters.AddWithValue("@Year", schedule.Year);
                cmd.Parameters.AddWithValue("@Month", schedule.Month);
                cmd.Parameters.AddWithValue("@Week", schedule.Week);
                cmd.Parameters.AddWithValue("@StartDate", (object)schedule.StartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object)schedule.EndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AbsentCount", (object)schedule.AbsentCount ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/WeeklySchedule/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeeklySchedule schedule)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE WeeklySchedule SET
                        Year = @Year,
                        Month = @Month,
                        Week = @Week,
                        StartDate = @StartDate,
                        EndDate = @EndDate,
                        AbsentCount = @AbsentCount
                    WHERE WeekID = @WeekID", conn);

                cmd.Parameters.AddWithValue("@WeekID", id);
                cmd.Parameters.AddWithValue("@Year", schedule.Year);
                cmd.Parameters.AddWithValue("@Month", schedule.Month);
                cmd.Parameters.AddWithValue("@Week", schedule.Week);
                cmd.Parameters.AddWithValue("@StartDate", (object)schedule.StartDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EndDate", (object)schedule.EndDate ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@AbsentCount", (object)schedule.AbsentCount ?? DBNull.Value);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/WeeklySchedule/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM WeeklySchedule WHERE WeekID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
