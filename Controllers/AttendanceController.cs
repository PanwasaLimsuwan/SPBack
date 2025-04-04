// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public AttendanceController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/Attendance
//         [HttpGet]
//         public async Task<IActionResult> GetAttendances()
//         {
//             var attendances = await _context.Attendance.ToListAsync();
//             return Ok(attendances);
//         }

//         // GET: api/Attendance/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetAttendanceById(int id)
//         {
//             var attendance = await _context.Attendance.FindAsync(id);
//             if (attendance == null)
//             {
//                 return NotFound("Attendance record not found");
//             }
//             return Ok(attendance);
//         }

//         // POST: api/Attendance
//         [HttpPost]
//         public async Task<IActionResult> CreateAttendance([FromBody] Attendance attendance)
//         {
//             if (attendance == null)
//             {
//                 return BadRequest("Attendance is null");
//             }

//             _context.Attendance.Add(attendance);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetAttendanceById), new { id = attendance.AttendanceID }, attendance);
//         }

//         // PUT: api/Attendance/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateAttendance(int id, [FromBody] Attendance attendance)
//         {
//             if (id != attendance.AttendanceID)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(attendance).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.Attendance.Any(e => e.AttendanceID == id))
//                 {
//                     return NotFound("Attendance record not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/Attendance/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteAttendance(int id)
//         {
//             var attendance = await _context.Attendance.FindAsync(id);
//             if (attendance == null)
//             {
//                 return NotFound("Attendance record not found");
//             }

//             _context.Attendance.Remove(attendance);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceController : ControllerBase
    {
        private readonly string _connectionString;

        public AttendanceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<Attendance> attendances = new();

            using (SqlConnection conn = new(_connectionString))
            {
                SqlCommand cmd = new("SELECT * FROM Attendance", conn);
                await conn.OpenAsync();
                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    attendances.Add(new Attendance
                    {
                        AttendanceID = reader.GetInt32(0),
                        EmpID = reader.GetInt32(1),
                        Date = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        CheckInTime = reader.IsDBNull(3) ? null : reader.GetTimeSpan(3),
                        CheckOutTime = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4),
                        Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                        WeekNumber = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                    });
                }
            }

            return Ok(attendances);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Attendance attendance = null;

            using (SqlConnection conn = new(_connectionString))
            {
                SqlCommand cmd = new("SELECT * FROM Attendance WHERE AttendanceID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);
                await conn.OpenAsync();

                using SqlDataReader reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    attendance = new Attendance
                    {
                        AttendanceID = reader.GetInt32(0),
                        EmpID = reader.GetInt32(1),
                        Date = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        CheckInTime = reader.IsDBNull(3) ? null : reader.GetTimeSpan(3),
                        CheckOutTime = reader.IsDBNull(4) ? null : reader.GetTimeSpan(4),
                        Status = reader.IsDBNull(5) ? null : reader.GetString(5),
                        WeekNumber = reader.IsDBNull(6) ? null : reader.GetInt32(6)
                    };
                }
            }

            return attendance == null ? NotFound() : Ok(attendance);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Attendance attendance)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new(@"INSERT INTO Attendance (EmpID, Date, CheckInTime, CheckOutTime, Status, WeekNumber) 
                                   VALUES (@EmpID, @Date, @CheckInTime, @CheckOutTime, @Status, @WeekNumber)", conn);

            cmd.Parameters.AddWithValue("@EmpID", attendance.EmpID);
            cmd.Parameters.AddWithValue("@Date", (object?)attendance.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckInTime", (object?)attendance.CheckInTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", (object?)attendance.CheckOutTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)attendance.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WeekNumber", (object?)attendance.WeekNumber ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return Ok("Attendance created successfully.");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Attendance attendance)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new(@"UPDATE Attendance 
                                   SET EmpID = @EmpID, Date = @Date, CheckInTime = @CheckInTime, 
                                       CheckOutTime = @CheckOutTime, Status = @Status, WeekNumber = @WeekNumber
                                   WHERE AttendanceID = @AttendanceID", conn);

            cmd.Parameters.AddWithValue("@AttendanceID", id);
            cmd.Parameters.AddWithValue("@EmpID", attendance.EmpID);
            cmd.Parameters.AddWithValue("@Date", (object?)attendance.Date ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckInTime", (object?)attendance.CheckInTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOutTime", (object?)attendance.CheckOutTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)attendance.Status ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@WeekNumber", (object?)attendance.WeekNumber ?? DBNull.Value);

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected == 0 ? NotFound("Attendance not found.") : Ok("Attendance updated.");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new("DELETE FROM Attendance WHERE AttendanceID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            int rowsAffected = await cmd.ExecuteNonQueryAsync();

            return rowsAffected == 0 ? NotFound("Attendance not found.") : Ok("Attendance deleted.");
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;
// using API_ProductionQuality.Models;
// using System.Threading.Tasks;
// using System;
// using System.Data;

// namespace API_ProductionQuality.Controllers
// {
//     [ApiController]
//     [Route("attendance/[action]")]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public AttendanceController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetAll()
//         {
//             try
//             {
//                 var data = _db.Query("SELECT * FROM Attendance");
//                 return _res.Success(data);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetById(int id)
//         {
//             try
//             {
//                 var data = _db.QueryDT($"SELECT * FROM Attendance WHERE AttendanceID = {id}");
//                 if (data.Rows.Count == 0)
//                     return _res.NotFound("Attendance record not found");
//                 return _res.Success(data);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpPost]
//         public async Task<JsonResult> Create([FromBody] Params param)
//         {
//             try
//             {
//                 var json = param.param1.ToString();
//                 DataTable dt = _db.ConvertJsonStringToDT(json);
//                 foreach (DataRow row in dt.Rows)
//                 {
//                     var query = $@"INSERT INTO Attendance (EmpID, Date, Status, Note)
//                                     VALUES ('{row["EmpID"]}', '{row["Date"]}', '{row["Status"]}', '{row["Note"]}')";
//                     var result = _db.QueryPOST(query);
//                     string jsonResult = _db.ConvertJsonResultToString(result);
//                     var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//                     if (response.code != "201")
//                         return _res.InternalServerError(response.message);
//                 }
//                 return _res.Created(null);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpPut]
//         public async Task<JsonResult> Update([FromBody] Params param)
//         {
//             try
//             {
//                 var json = param.param1.ToString();
//                 DataTable dt = _db.ConvertJsonStringToDT(json);
//                 foreach (DataRow row in dt.Rows)
//                 {
//                     var query = $@"UPDATE Attendance
//                                     SET EmpID = '{row["EmpID"]}',
//                                         Date = '{row["Date"]}',
//                                         Status = '{row["Status"]}',
//                                         Note = '{row["Note"]}'
//                                     WHERE AttendanceID = {row["AttendanceID"]}";
//                     var result = _db.QueryPOST(query);
//                     string jsonResult = _db.ConvertJsonResultToString(result);
//                     var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//                     if (response.code != "201")
//                         return _res.InternalServerError(response.message);
//                 }
//                 return _res.Success(null);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> Delete(int id)
//         {
//             try
//             {
//                 var query = $"DELETE FROM Attendance WHERE AttendanceID = {id}";
//                 var result = _db.QueryPOST(query);
//                 string jsonResult = _db.ConvertJsonResultToString(result);
//                 var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//                 if (response.code != "201")
//                     return _res.InternalServerError(response.message);
//                 return _res.Success(null);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }
//     }
// }
