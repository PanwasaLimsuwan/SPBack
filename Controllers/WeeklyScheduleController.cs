// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class WeeklyScheduleController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public WeeklyScheduleController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetWeeklySchedules()
//         {
//             var schedules = await _context.WeeklySchedule.ToListAsync();
//             return Ok(schedules);
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetWeeklyScheduleById(int id)
//         {
//             var schedule = await _context.WeeklySchedule.FindAsync(id);
//             if (schedule == null)
//             {
//                 return NotFound("Weekly schedule not found");
//             }
//             return Ok(schedule);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateWeeklySchedule([FromBody] WeeklySchedule schedule)
//         {
//             if (schedule == null)
//             {
//                 return BadRequest("Weekly schedule is null");
//             }

//             _context.WeeklySchedule.Add(schedule);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetWeeklyScheduleById), new { id = schedule.WeekID }, schedule);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateWeeklySchedule(int id, [FromBody] WeeklySchedule schedule)
//         {
//             if (id != schedule.WeekID)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(schedule).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.WeeklySchedule.Any(e => e.WeekID == id))
//                 {
//                     return NotFound("Weekly schedule not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteWeeklySchedule(int id)
//         {
//             var schedule = await _context.WeeklySchedule.FindAsync(id);
//             if (schedule == null)
//             {
//                 return NotFound("Weekly schedule not found");
//             }

//             _context.WeeklySchedule.Remove(schedule);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

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
            var list = new List<WeeklySchedule>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM WeeklySchedule";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new WeeklySchedule
                        {
                            WeekID = Convert.ToInt32(reader["WeekID"]),
                            Year = Convert.ToInt32(reader["Year"]),
                            Month = reader["Month"]?.ToString(),
                            Week = Convert.ToInt32(reader["Week"]),
                            StartDate = reader["StartDate"] as DateTime?,
                            EndDate = reader["EndDate"] as DateTime?,
                            AbsentCount = reader["AbsentCount"] as int?
                        });
                    }
                }
            }

            return Ok(list);
        }

        // GET: api/WeeklySchedule/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            WeeklySchedule schedule = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM WeeklySchedule WHERE WeekID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            schedule = new WeeklySchedule
                            {
                                WeekID = Convert.ToInt32(reader["WeekID"]),
                                Year = Convert.ToInt32(reader["Year"]),
                                Month = reader["Month"]?.ToString(),
                                Week = Convert.ToInt32(reader["Week"]),
                                StartDate = reader["StartDate"] as DateTime?,
                                EndDate = reader["EndDate"] as DateTime?,
                                AbsentCount = reader["AbsentCount"] as int?
                            };
                        }
                    }
                }
            }

            if (schedule == null)
                return NotFound("Weekly schedule not found");

            return Ok(schedule);
        }

        // POST: api/WeeklySchedule
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] WeeklySchedule model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"INSERT INTO WeeklySchedule (Year, Month, Week, StartDate, EndDate, AbsentCount)
                              VALUES (@Year, @Month, @Week, @StartDate, @EndDate, @AbsentCount)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Year", model.Year);
                    cmd.Parameters.AddWithValue("@Month", model.Month ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Week", model.Week);
                    cmd.Parameters.AddWithValue("@StartDate", (object)model.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object)model.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AbsentCount", (object)model.AbsentCount ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Weekly schedule created successfully");
        }

        // PUT: api/WeeklySchedule/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] WeeklySchedule model)
        {
            if (id != model.WeekID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"UPDATE WeeklySchedule SET Year = @Year, Month = @Month, Week = @Week,
                              StartDate = @StartDate, EndDate = @EndDate, AbsentCount = @AbsentCount
                              WHERE WeekID = @WeekID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@WeekID", model.WeekID);
                    cmd.Parameters.AddWithValue("@Year", model.Year);
                    cmd.Parameters.AddWithValue("@Month", model.Month ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Week", model.Week);
                    cmd.Parameters.AddWithValue("@StartDate", (object)model.StartDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EndDate", (object)model.EndDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@AbsentCount", (object)model.AbsentCount ?? DBNull.Value);

                    var affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("Weekly schedule not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/WeeklySchedule/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "DELETE FROM WeeklySchedule WHERE WeekID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var affected = await cmd.ExecuteNonQueryAsync();

                    if (affected == 0)
                        return NotFound("Weekly schedule not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Newtonsoft.Json;
// using Api.Services;
// using System.Data;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class WeeklyScheduleController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public WeeklyScheduleController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public IActionResult GetAll()
//         {
//             var query = "SELECT * FROM WeeklySchedule";
//             var data = _db.Query(query);
//             return _res.Success(data);
//         }

//         [HttpGet("{id}")]
//         public IActionResult GetById(int id)
//         {
//             var query = $"SELECT * FROM WeeklySchedule WHERE WeekID = {id}";
//             var data = _db.Query(query);
//             if (data.Rows.Count == 0)
//                 return _res.NotFound("Weekly schedule not found");
//             return _res.Success(data);
//         }

//         [HttpPost]
//         public IActionResult Create([FromBody] WeeklySchedule schedule)
//         {
//             if (schedule == null)
//                 return _res.BadRequest("Weekly schedule is null");

//             var query = $@"
//                 INSERT INTO WeeklySchedule (WeekStart, WeekEnd, Description)
//                 VALUES ('{schedule.WeekStart:yyyy-MM-dd}', '{schedule.WeekEnd:yyyy-MM-dd}', '{schedule.Description}')";
//             var result = _db.QueryPOST(query);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);
//             return response.code == "201" ? _res.Created(null) : _res.InternalServerError(response.message);
//         }

//         [HttpPut("{id}")]
//         public IActionResult Update(int id, [FromBody] WeeklySchedule schedule)
//         {
//             if (id != schedule.WeekID)
//                 return _res.BadRequest("ID mismatch");

//             var query = $@"
//                 UPDATE WeeklySchedule SET 
//                     WeekStart = '{schedule.WeekStart:yyyy-MM-dd}',
//                     WeekEnd = '{schedule.WeekEnd:yyyy-MM-dd}',
//                     Description = '{schedule.Description}'
//                 WHERE WeekID = {id}";
//             var result = _db.QueryPOST(query);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);
//             return response.code == "201" ? _res.Success(null) : _res.InternalServerError(response.message);
//         }

//         [HttpDelete("{id}")]
//         public IActionResult Delete(int id)
//         {
//             var query = $"DELETE FROM WeeklySchedule WHERE WeekID = {id}";
//             var result = _db.QueryPOST(query);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);
//             return response.code == "201" ? _res.Success(null) : _res.InternalServerError(response.message);
//         }
//     }
// }
