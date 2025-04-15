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
                        a.AttendanceID, a.EmpID, a.Date, a.CheckInTime, a.CheckOutTime, a.Status,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM Attendance a
                    JOIN EmployeeInfo e ON a.EmpID = e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.CommandTimeout = 300; // เพิ่มเวลา Timeout (ค่าเป็นวินาที)

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                attendanceID = reader.GetInt32(0),
                                empID = reader.GetInt32(1),
                                date = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                                checkInTime = reader.IsDBNull(3) ? (TimeSpan?)null : reader.GetTimeSpan(3),
                                checkOutTime = reader.IsDBNull(4) ? (TimeSpan?)null : reader.GetTimeSpan(4),
                                status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                division = reader.IsDBNull(6) ? null : reader.GetString(6),
                                department = reader.IsDBNull(7) ? null : reader.GetString(7),
                                section = reader.IsDBNull(8) ? null : reader.GetString(8),
                                biz = reader.IsDBNull(9) ? null : reader.GetString(9),
                                process = reader.IsDBNull(10) ? null : reader.GetString(10)
                            });
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}
