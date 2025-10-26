using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

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

        // [HttpGet("ByDate")] ที่ดึงข้อมูลตามวันที่
        [HttpGet("ByDate")]
        public async Task<IActionResult> GetAllAttendanceByDate(
            [FromQuery] DateTime? date,
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                DateTime selectedDate;

                if (date.HasValue)
                {
                    selectedDate = date.Value;
                }
                else
                {
                    var latestDateCmd = new SqlCommand(
                        "SELECT TOP 1 EntryDateTime FROM GateEntry WHERE EntryDateTime IS NOT NULL ORDER BY EntryDateTime DESC",
                        conn
                    );
                    var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

                    if (latestDateObj == null || latestDateObj == DBNull.Value)
                    {
                        return Ok(result);
                    }

                    selectedDate = ((DateTime)latestDateObj).Date;
                }

                DateTime dateStart = selectedDate.Date;
                DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1);

                var query =
                    @"
                    SELECT 
                        a.AttendanceID, a.EmpID, a.Date, a.CheckInTime, a.CheckOutTime, a.Status,
                        e.Division, e.Department, e.Section, e.Biz, e.Process, e.FirstName, e.LastName
                    FROM Attendance a
                    JOIN EmployeeInfo e ON a.EmpID = e.EmpID
                    WHERE a.Date BETWEEN @dateStart AND @dateEnd
                ";

                var cmd = new SqlCommand();
                cmd.Connection = conn;
                cmd.Parameters.AddWithValue("@dateStart", dateStart);
                cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

                if (!string.IsNullOrEmpty(division))
                {
                    query += " AND e.Division = @division";
                    cmd.Parameters.AddWithValue("@division", division);
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query += " AND e.Department = @department";
                    cmd.Parameters.AddWithValue("@department", department);
                }

                if (!string.IsNullOrEmpty(section))
                {
                    query += " AND e.Section = @section";
                    cmd.Parameters.AddWithValue("@section", section);
                }

                if (!string.IsNullOrEmpty(biz))
                {
                    query += " AND e.Biz = @biz";
                    cmd.Parameters.AddWithValue("@biz", biz);
                }

                if (!string.IsNullOrEmpty(process))
                {
                    query += " AND e.Process = @process";
                    cmd.Parameters.AddWithValue("@process", process);
                }

                query += " ORDER BY CAST(a.Date AS DATE)";
                cmd.CommandText = query;
                cmd.CommandTimeout = 300;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(
                            new
                            {
                                attendanceID = reader.GetInt32(0),
                                empID = reader.GetInt32(1),
                                date = reader.IsDBNull(2)
                                    ? null
                                    : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                checkInTime = reader.IsDBNull(3)
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(3),
                                checkOutTime = reader.IsDBNull(4)
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(4),
                                status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                division = reader.IsDBNull(6) ? null : reader.GetString(6),
                                department = reader.IsDBNull(7) ? null : reader.GetString(7),
                                section = reader.IsDBNull(8) ? null : reader.GetString(8),
                                biz = reader.IsDBNull(9) ? null : reader.GetString(9),
                                process = reader.IsDBNull(10) ? null : reader.GetString(10),
                                firstName = reader.GetString(11),
                                lastName = reader.GetString(12),
                            }
                        );
                    }
                }
            }

            return Ok(result);
        }

        // [HttpGet] ที่ดึงข้อมูลทั้งหมด
        [HttpGet]
        public async Task<IActionResult> GetAllAttendance(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query =
                    @"
                    SELECT 
                        a.AttendanceID, 
                        a.EmpID, 
                        CAST(a.Date AS DATE) AS Date, 
                        a.CheckInTime, 
                        a.CheckOutTime, 
                        a.Status,
                        e.Division, 
                        e.Department, 
                        e.Section, 
                        e.Biz, 
                        e.Process, 
                        e.FirstName, 
                        e.LastName
                    FROM Attendance a
                    JOIN EmployeeInfo e ON a.EmpID = e.EmpID
                    WHERE 1 = 1
                ";

                var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrEmpty(division))
                {
                    query += " AND e.Division = @division";
                    cmd.Parameters.AddWithValue("@division", division);
                }

                if (!string.IsNullOrEmpty(department))
                {
                    query += " AND e.Department = @department";
                    cmd.Parameters.AddWithValue("@department", department);
                }

                if (!string.IsNullOrEmpty(section))
                {
                    query += " AND e.Section = @section";
                    cmd.Parameters.AddWithValue("@section", section);
                }

                if (!string.IsNullOrEmpty(biz))
                {
                    query += " AND e.Biz = @biz";
                    cmd.Parameters.AddWithValue("@biz", biz);
                }

                if (!string.IsNullOrEmpty(process))
                {
                    query += " AND e.Process = @process";
                    cmd.Parameters.AddWithValue("@process", process);
                }

                query += " ORDER BY CAST(a.Date AS DATE)";
                cmd.CommandText = query;
                cmd.CommandTimeout = 300;

                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(
                            new
                            {
                                attendanceID = reader.GetInt32(0),
                                empID = reader.GetInt32(1),
                                date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                checkInTime = reader.IsDBNull(3)
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(3),
                                checkOutTime = reader.IsDBNull(4)
                                    ? (TimeSpan?)null
                                    : reader.GetTimeSpan(4),
                                status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                division = reader.IsDBNull(6) ? null : reader.GetString(6),
                                department = reader.IsDBNull(7) ? null : reader.GetString(7),
                                section = reader.IsDBNull(8) ? null : reader.GetString(8),
                                biz = reader.IsDBNull(9) ? null : reader.GetString(9),
                                process = reader.IsDBNull(10) ? null : reader.GetString(10),
                                firstName = reader.GetString(11),
                                lastName = reader.GetString(12),
                            }
                        );
                    }
                }
            }

            return Ok(result);
        }
    }
}
