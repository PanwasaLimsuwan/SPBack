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
    public class AttendanceController : ControllerBase
    {
        private readonly string _connectionString;

        public AttendanceController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllAttendance()
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        a.AttendanceID, a.EmpID, a.Date, a.CheckInTime, a.CheckOutTime,
                        a.ScheduledStartTime, a.ScheduledEndTime, a.Status, a.WeekNumber,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM Attendance a
                    JOIN EmployeeInfo e ON a.EmpID = CAST(e.EmpID AS VARCHAR)";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new
                        {
                            attendanceID = Convert.ToInt32(reader["AttendanceID"]),
                            empID = reader["EmpID"].ToString(),
                            date = reader["Date"] as DateTime?,
                            checkInTime = reader["CheckInTime"] as TimeSpan?,
                            checkOutTime = reader["CheckOutTime"] as TimeSpan?,
                            scheduledStartTime = reader["ScheduledStartTime"] as TimeSpan?,
                            scheduledEndTime = reader["ScheduledEndTime"] as TimeSpan?,
                            status = reader["Status"]?.ToString(),
                            weekNumber = reader["WeekNumber"] as int?,
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString()
                        });
                    }
                }
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAttendanceById(int id)
        {
            object attendance = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        a.AttendanceID, a.EmpID, a.Date, a.CheckInTime, a.CheckOutTime,
                        a.ScheduledStartTime, a.ScheduledEndTime, a.Status, a.WeekNumber,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM Attendance a
                    JOIN EmployeeInfo e ON a.EmpID = CAST(e.EmpID AS VARCHAR)
                    WHERE a.AttendanceID = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            attendance = new
                            {
                                attendanceID = Convert.ToInt32(reader["AttendanceID"]),
                                empID = reader["EmpID"].ToString(),
                                date = reader["Date"] as DateTime?,
                                checkInTime = reader["CheckInTime"] as TimeSpan?,
                                checkOutTime = reader["CheckOutTime"] as TimeSpan?,
                                scheduledStartTime = reader["ScheduledStartTime"] as TimeSpan?,
                                scheduledEndTime = reader["ScheduledEndTime"] as TimeSpan?,
                                status = reader["Status"]?.ToString(),
                                weekNumber = reader["WeekNumber"] as int?,
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

            if (attendance == null)
                return NotFound("Attendance record not found");

            return Ok(attendance);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAttendance([FromBody] Attendance attendance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"
                    INSERT INTO Attendance (EmpID, Date, CheckInTime, CheckOutTime, ScheduledStartTime, ScheduledEndTime, Status, WeekNumber)
                    VALUES (@EmpID, @Date, @CheckInTime, @CheckOutTime, @ScheduledStartTime, @ScheduledEndTime, @Status, @WeekNumber)";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", attendance.EmpID);
                    cmd.Parameters.AddWithValue("@Date", (object?)attendance.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckInTime", (object?)attendance.CheckInTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckOutTime", (object?)attendance.CheckOutTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScheduledStartTime", (object?)attendance.ScheduledStartTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScheduledEndTime", (object?)attendance.ScheduledEndTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)attendance.Status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WeekNumber", (object?)attendance.WeekNumber ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Attendance record created successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAttendance(int id, [FromBody] Attendance attendance)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"
                    UPDATE Attendance SET 
                        EmpID = @EmpID,
                        Date = @Date,
                        CheckInTime = @CheckInTime,
                        CheckOutTime = @CheckOutTime,
                        ScheduledStartTime = @ScheduledStartTime,
                        ScheduledEndTime = @ScheduledEndTime,
                        Status = @Status,
                        WeekNumber = @WeekNumber
                    WHERE AttendanceID = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@EmpID", attendance.EmpID);
                    cmd.Parameters.AddWithValue("@Date", (object?)attendance.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckInTime", (object?)attendance.CheckInTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckOutTime", (object?)attendance.CheckOutTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScheduledStartTime", (object?)attendance.ScheduledStartTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ScheduledEndTime", (object?)attendance.ScheduledEndTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)attendance.Status ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WeekNumber", (object?)attendance.WeekNumber ?? DBNull.Value);

                    var affectedRows = await cmd.ExecuteNonQueryAsync();
                    if (affectedRows == 0) return NotFound("Attendance record not found");
                }
            }

            return Ok("Attendance record updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAttendance(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "DELETE FROM Attendance WHERE AttendanceID = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var affectedRows = await cmd.ExecuteNonQueryAsync();
                    if (affectedRows == 0) return NotFound("Attendance record not found");
                }
            }

            return Ok("Attendance record deleted successfully");
        }
    }
}
