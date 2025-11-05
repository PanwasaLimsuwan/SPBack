// using System.Data;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public AttendanceController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         // [HttpGet("ByDate")] ที่ดึงข้อมูลตามวันที่
//         [HttpGet("ByDate")]
//         public async Task<IActionResult> GetAllAttendanceByDate(
//             [FromQuery] DateTime? date,
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 DateTime selectedDate;

//                 if (date.HasValue)
//                 {
//                     selectedDate = date.Value;
//                 }
//                 else
//                 {
//                     var latestDateCmd = new SqlCommand(
//                         "SELECT TOP 1 EntryDateTime FROM GateEntry WHERE EntryDateTime IS NOT NULL ORDER BY EntryDateTime DESC",
//                         conn
//                     );
//                     var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

//                     if (latestDateObj == null || latestDateObj == DBNull.Value)
//                     {
//                         return Ok(result);
//                     }

//                     selectedDate = ((DateTime)latestDateObj).Date;
//                 }

//                 DateTime dateStart = selectedDate.Date;
//                 DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1);

//                 var query =
//                     @"
//                     SELECT
//                         a.AttendanceID, a.EmpID, a.Date, a.CheckInTime, a.CheckOutTime, a.Status,
//                         e.Division, e.Department, e.Section, e.Biz, e.Process, e.FirstName, e.LastName
//                     FROM Attendance a
//                     JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//                     WHERE a.Date BETWEEN @dateStart AND @dateEnd
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;
//                 cmd.Parameters.AddWithValue("@dateStart", dateStart);
//                 cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(
//                             new
//                             {
//                                 attendanceID = reader.GetInt32(0),
//                                 empID = reader.GetInt32(1),
//                                 date = reader.IsDBNull(2)
//                                     ? null
//                                     : reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                                 checkInTime = reader.IsDBNull(3)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(3),
//                                 checkOutTime = reader.IsDBNull(4)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(4),
//                                 status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                                 division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                                 department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                                 section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                                 biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                                 process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                                 firstName = reader.GetString(11),
//                                 lastName = reader.GetString(12),
//                             }
//                         );
//                     }
//                 }
//             }

//             return Ok(result);
//         }

//         // [HttpGet] ที่ดึงข้อมูลทั้งหมด
//         [HttpGet]
//         public async Task<IActionResult> GetAllAttendance(
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 var query =
//                     @"
//                     SELECT
//                         a.AttendanceID,
//                         a.EmpID,
//                         CAST(a.Date AS DATE) AS Date,
//                         a.CheckInTime,
//                         a.CheckOutTime,
//                         a.Status,
//                         e.Division,
//                         e.Department,
//                         e.Section,
//                         e.Biz,
//                         e.Process,
//                         e.FirstName,
//                         e.LastName
//                     FROM Attendance a
//                     JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//                     WHERE 1 = 1
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(
//                             new
//                             {
//                                 attendanceID = reader.GetInt32(0),
//                                 empID = reader.GetInt32(1),
//                                 date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                                 checkInTime = reader.IsDBNull(3)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(3),
//                                 checkOutTime = reader.IsDBNull(4)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(4),
//                                 status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                                 division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                                 department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                                 section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                                 biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                                 process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                                 firstName = reader.GetString(11),
//                                 lastName = reader.GetString(12),
//                             }
//                         );
//                     }
//                 }
//             }

//             return Ok(result);
//         }
//     }
// }

// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public AttendanceController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         // ✅ [HttpPost("UpdateCheckOutTimes")] - อัปเดต CheckOutTime จาก Transactions ลงในตาราง Attendance
//         [HttpPost("UpdateCheckOutTimes")]
//         public async Task<IActionResult> UpdateCheckOutTimes([FromQuery] DateTime? date)
//         {
//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 var updateQuery = @"
//                     UPDATE a
//                     SET a.CheckOutTime = (
//                         SELECT CAST(MAX(t.Timestamp) AS TIME) 
//                         FROM Transactions t 
//                         WHERE t.EmpID = a.EmpID 
//                           AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                           AND t.CameraID IN (1, 2)
//                     )
//                     FROM Attendance a
//                     WHERE a.CheckOutTime IS NULL
//                       AND EXISTS (
//                           SELECT 1 
//                           FROM Transactions t 
//                           WHERE t.EmpID = a.EmpID 
//                             AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                             AND t.CameraID IN (1, 2)
//                       )
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;

//                 if (date.HasValue)
//                 {
//                     updateQuery += " AND CAST(a.Date AS DATE) = @date";
//                     cmd.Parameters.AddWithValue("@date", date.Value.Date);
//                 }

//                 cmd.CommandText = updateQuery;
//                 cmd.CommandTimeout = 300;

//                 int rowsAffected = await cmd.ExecuteNonQueryAsync();

//                 return Ok(new { 
//                     success = true,
//                     message = $"อัปเดต CheckOutTime สำเร็จ {rowsAffected} รายการ", 
//                     rowsAffected = rowsAffected,
//                     date = date?.ToString("yyyy-MM-dd") ?? "ทุกวัน"
//                 });
//             }
//         }

//         // [HttpGet("ByDate")] ที่ดึงข้อมูลตามวันที่
//         [HttpGet("ByDate")]
//         public async Task<IActionResult> GetAllAttendanceByDate(
//             [FromQuery] DateTime? date,
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 DateTime selectedDate;
//                 if (date.HasValue)
//                 {
//                     selectedDate = date.Value;
//                 }
//                 else
//                 {
//                     var latestDateCmd = new SqlCommand(
//                         "SELECT TOP 1 Timestamp FROM Transactions WHERE Timestamp IS NOT NULL ORDER BY Timestamp DESC",
//                         conn
//                     );
//                     var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

//                     if (latestDateObj == null || latestDateObj == DBNull.Value)
//                     {
//                         return Ok(result);
//                     }

//                     selectedDate = ((DateTime)latestDateObj).Date;
//                 }

//                 DateTime dateStart = selectedDate.Date;
//                 DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1);

//                 // ✅ แก้ไข Query เพื่อดึง CheckOutTime จาก Transactions
//                 var query =
//                     @"
//             SELECT 
//                 a.AttendanceID, 
//                 a.EmpID, 
//                 a.Date, 
//                 a.CheckInTime, 
//                 COALESCE(
//                     a.CheckOutTime,
//                     (SELECT CAST(MAX(t.Timestamp) AS TIME) 
//                      FROM Transactions t 
//                      WHERE t.EmpID = a.EmpID 
//                        AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                        AND t.CameraID IN (1, 2))
//                 ) AS CheckOutTime,
//                 a.Status,
//                 e.Division, 
//                 e.Department, 
//                 e.Section, 
//                 e.Biz, 
//                 e.Process, 
//                 e.FirstName, 
//                 e.LastName
//             FROM Attendance a
//             JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//             WHERE a.Date BETWEEN @dateStart AND @dateEnd
//         ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;
//                 cmd.Parameters.AddWithValue("@dateStart", dateStart);
//                 cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(
//                             new
//                             {
//                                 attendanceID = reader.GetInt32(0),
//                                 empID = reader.GetInt32(1),
//                                 date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                                 checkInTime = reader.IsDBNull(3)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(3),
//                                 checkOutTime = reader.IsDBNull(4)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(4), // ✅ จะได้ค่าจาก Transactions ถ้า Attendance เป็น NULL
//                                 status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                                 division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                                 department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                                 section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                                 biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                                 process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                                 firstName = reader.GetString(11),
//                                 lastName = reader.GetString(12),
//                             }
//                         );
//                     }
//                 }
//             }

//             return Ok(result);
//         }

//         // [HttpGet] ที่ดึงข้อมูลทั้งหมด
//         [HttpGet]
//         public async Task<IActionResult> GetAllAttendance(
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 var query =
//                     @"
//                     SELECT 
//                         a.AttendanceID, 
//                         a.EmpID, 
//                         CAST(a.Date AS DATE) AS Date, 
//                         a.CheckInTime, 
//                 COALESCE(
//                             a.CheckOutTime,
//                             (SELECT CAST(MAX(t.Timestamp) AS TIME) 
//                              FROM Transactions t 
//                              WHERE t.EmpID = a.EmpID 
//                                AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                                AND t.CameraID IN (1, 2))
//                         ) AS CheckOutTime,
//                         a.Status,
//                         e.Division, 
//                         e.Department, 
//                         e.Section, 
//                         e.Biz, 
//                         e.Process, 
//                         e.FirstName, 
//                         e.LastName
//                     FROM Attendance a
//                     JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//                     WHERE 1 = 1
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(
//                             new
//                             {
//                                 attendanceID = reader.GetInt32(0),
//                                 empID = reader.GetInt32(1),
//                                 date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                                 checkInTime = reader.IsDBNull(3)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(3),
//                                 checkOutTime = reader.IsDBNull(4)
//                                     ? (TimeSpan?)null
//                                     : reader.GetTimeSpan(4),
//                                 status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                                 division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                                 department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                                 section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                                 biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                                 process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                                 firstName = reader.GetString(11),
//                                 lastName = reader.GetString(12),
//                             }
//                         );
//                     }
//                 }
//             }

//             return Ok(result);
//         }
//     }
// }

// using System;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AttendanceController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public AttendanceController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         // ✅ Method สำหรับอัปเดต CheckOutTime อัตโนมัติ
//         // ✅ Method สำหรับอัปเดต CheckOutTime อัตโนมัติ (แก้ไขแล้ว)
//         private async Task AutoUpdateCheckOutTimes(SqlConnection conn, DateTime? date = null)
//         {
//             var updateQuery = @"
//         UPDATE a
//         SET a.CheckOutTime = (
//             SELECT CAST(MAX(t.Timestamp) AS TIME) 
//             FROM Transactions t 
//             WHERE t.EmpID = a.EmpID 
//               AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//         )
//         FROM Attendance a
//         WHERE EXISTS (
//               SELECT 1 
//               FROM Transactions t 
//               WHERE t.EmpID = a.EmpID 
//                 AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//           )
//     ";

//             var cmd = new SqlCommand();
//             cmd.Connection = conn;

//             if (date.HasValue)
//             {
//                 updateQuery += " AND CAST(a.Date AS DATE) = @date";
//                 cmd.Parameters.AddWithValue("@date", date.Value.Date);
//             }

//             cmd.CommandText = updateQuery;
//             cmd.CommandTimeout = 300;

//             int rowsAffected = await cmd.ExecuteNonQueryAsync();

//             Console.WriteLine($"✅ อัปเดต CheckOutTime สำเร็จ {rowsAffected} รายการ");
//         }

//         // [HttpPost("UpdateCheckOutTimes")] - อัปเดต CheckOutTime แบบ Manual
//         [HttpPost("UpdateCheckOutTimes")]
//         public async Task<IActionResult> UpdateCheckOutTimes([FromQuery] DateTime? date)
//         {
//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();
                
//                 var updateQuery = @"
//                     UPDATE a
//                     SET a.CheckOutTime = (
//                         SELECT CAST(MAX(t.Timestamp) AS TIME) 
//                         FROM Transactions t 
//                         WHERE t.EmpID = a.EmpID 
//                           AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                     )
//                     FROM Attendance a
//                     WHERE a.CheckOutTime IS NULL
//                       AND EXISTS (
//                           SELECT 1 
//                           FROM Transactions t 
//                           WHERE t.EmpID = a.EmpID 
//                             AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
//                       )
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;

//                 if (date.HasValue)
//                 {
//                     updateQuery += " AND CAST(a.Date AS DATE) = @date";
//                     cmd.Parameters.AddWithValue("@date", date.Value.Date);
//                 }

//                 cmd.CommandText = updateQuery;
//                 cmd.CommandTimeout = 300;

//                 int rowsAffected = await cmd.ExecuteNonQueryAsync();

//                 return Ok(new { 
//                     success = true,
//                     message = $"อัปเดต CheckOutTime สำเร็จ {rowsAffected} รายการ", 
//                     rowsAffected = rowsAffected,
//                     date = date?.ToString("yyyy-MM-dd") ?? "ทุกวัน"
//                 });
//             }
//         }

//         // [HttpGet("ByDate")] ที่ดึงข้อมูลตามวันที่ + อัปเดตอัตโนมัติ
//         [HttpGet("ByDate")]
//         public async Task<IActionResult> GetAllAttendanceByDate(
//             [FromQuery] DateTime? date,
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 DateTime selectedDate;
//                 if (date.HasValue)
//                 {
//                     selectedDate = date.Value;
//                 }
//                 else
//                 {
//                     var latestDateCmd = new SqlCommand(
//                         "SELECT TOP 1 Timestamp FROM Transactions WHERE Timestamp IS NOT NULL ORDER BY Timestamp DESC",
//                         conn
//                     );
//                     var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

//                     if (latestDateObj == null || latestDateObj == DBNull.Value)
//                     {
//                         return Ok(result);
//                     }

//                     selectedDate = ((DateTime)latestDateObj).Date;
//                 }

//                 // ✅ อัปเดต CheckOutTime อัตโนมัติก่อน SELECT
//                 await AutoUpdateCheckOutTimes(conn, selectedDate);

//                 DateTime dateStart = selectedDate.Date;
//                 DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1);

//                 var query = @"
//                     SELECT 
//                         a.AttendanceID, 
//                         a.EmpID, 
//                         a.Date, 
//                         a.CheckInTime, 
//                         a.CheckOutTime,
//                         a.Status,
//                         e.Division, 
//                         e.Department, 
//                         e.Section, 
//                         e.Biz, 
//                         e.Process, 
//                         e.FirstName, 
//                         e.LastName
//                     FROM Attendance a
//                     JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//                     WHERE a.Date BETWEEN @dateStart AND @dateEnd
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;
//                 cmd.Parameters.AddWithValue("@dateStart", dateStart);
//                 cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(new
//                         {
//                             attendanceID = reader.GetInt32(0),
//                             empID = reader.GetInt32(1),
//                             date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                             checkInTime = reader.IsDBNull(3)
//                                 ? (TimeSpan?)null
//                                 : reader.GetTimeSpan(3),
//                             checkOutTime = reader.IsDBNull(4)
//                                 ? (TimeSpan?)null
//                                 : reader.GetTimeSpan(4),
//                             status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                             division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                             department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                             section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                             biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                             process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                             firstName = reader.GetString(11),
//                             lastName = reader.GetString(12),
//                         });
//                     }
//                 }
//             }

//             return Ok(result);
//         }

//         // [HttpGet] ที่ดึงข้อมูลทั้งหมด + อัปเดตอัตโนมัติ
//         [HttpGet]
//         public async Task<IActionResult> GetAllAttendance(
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 // ✅ อัปเดต CheckOutTime อัตโนมัติก่อน SELECT (ทุกวัน)
//                 await AutoUpdateCheckOutTimes(conn);

//                 var query = @"
//                     SELECT 
//                         a.AttendanceID, 
//                         a.EmpID, 
//                         CAST(a.Date AS DATE) AS Date, 
//                         a.CheckInTime, 
//                         a.CheckOutTime,
//                         a.Status,
//                         e.Division, 
//                         e.Department, 
//                         e.Section, 
//                         e.Biz, 
//                         e.Process, 
//                         e.FirstName, 
//                         e.LastName
//                     FROM Attendance a
//                     JOIN EmployeeInfo e ON a.EmpID = e.EmpID
//                     WHERE 1 = 1
//                 ";

//                 var cmd = new SqlCommand();
//                 cmd.Connection = conn;

//                 if (!string.IsNullOrEmpty(division))
//                 {
//                     query += " AND e.Division = @division";
//                     cmd.Parameters.AddWithValue("@division", division);
//                 }

//                 if (!string.IsNullOrEmpty(department))
//                 {
//                     query += " AND e.Department = @department";
//                     cmd.Parameters.AddWithValue("@department", department);
//                 }

//                 if (!string.IsNullOrEmpty(section))
//                 {
//                     query += " AND e.Section = @section";
//                     cmd.Parameters.AddWithValue("@section", section);
//                 }

//                 if (!string.IsNullOrEmpty(biz))
//                 {
//                     query += " AND e.Biz = @biz";
//                     cmd.Parameters.AddWithValue("@biz", biz);
//                 }

//                 if (!string.IsNullOrEmpty(process))
//                 {
//                     query += " AND e.Process = @process";
//                     cmd.Parameters.AddWithValue("@process", process);
//                 }

//                 query += " ORDER BY CAST(a.Date AS DATE)";
//                 cmd.CommandText = query;
//                 cmd.CommandTimeout = 300;

//                 using (var reader = await cmd.ExecuteReaderAsync())
//                 {
//                     while (await reader.ReadAsync())
//                     {
//                         result.Add(new
//                         {
//                             attendanceID = reader.GetInt32(0),
//                             empID = reader.GetInt32(1),
//                             date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
//                             checkInTime = reader.IsDBNull(3)
//                                 ? (TimeSpan?)null
//                                 : reader.GetTimeSpan(3),
//                             checkOutTime = reader.IsDBNull(4)
//                                 ? (TimeSpan?)null
//                                 : reader.GetTimeSpan(4),
//                             status = reader.IsDBNull(5) ? null : reader.GetString(5),
//                             division = reader.IsDBNull(6) ? null : reader.GetString(6),
//                             department = reader.IsDBNull(7) ? null : reader.GetString(7),
//                             section = reader.IsDBNull(8) ? null : reader.GetString(8),
//                             biz = reader.IsDBNull(9) ? null : reader.GetString(9),
//                             process = reader.IsDBNull(10) ? null : reader.GetString(10),
//                             firstName = reader.GetString(11),
//                             lastName = reader.GetString(12),
//                         });
//                     }
//                 }
//             }

//             return Ok(result);
//         }
//     }
// }

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        // [HttpPost("UpdateCheckOutTimes")] - อัปเดต CheckOutTime แบบ Manual
        [HttpPost("UpdateCheckOutTimes")]
        public async Task<IActionResult> UpdateCheckOutTimes([FromQuery] DateTime? date)
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();
                    
                    var updateQuery = @"
                        UPDATE a
                        SET a.CheckOutTime = (
                            SELECT CAST(MAX(t.Timestamp) AS TIME) 
                            FROM Transactions t 
                            WHERE t.EmpID = a.EmpID 
                              AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
                        )
                        FROM Attendance a
                        WHERE EXISTS (
                            SELECT 1 
                            FROM Transactions t 
                            WHERE t.EmpID = a.EmpID 
                              AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
                              AND t.Timestamp > CAST(CAST(a.Date AS DATE) AS DATETIME) + CAST(a.CheckInTime AS DATETIME)
                        )
                    ";

                    var cmd = new SqlCommand();
                    cmd.Connection = conn;

                    if (date.HasValue)
                    {
                        updateQuery += " AND CAST(a.Date AS DATE) = @date";
                        cmd.Parameters.AddWithValue("@date", date.Value.Date);
                    }

                    cmd.CommandText = updateQuery;
                    cmd.CommandTimeout = 300;

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();

                    return Ok(new { 
                        success = true,
                        message = $"อัปเดต CheckOutTime สำเร็จ {rowsAffected} รายการ", 
                        rowsAffected = rowsAffected,
                        date = date?.ToString("yyyy-MM-dd") ?? "ทุกวัน"
                    });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error updating CheckOutTime",
                    detail = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // [HttpGet("ByDate")] ดึงข้อมูลตามวันที่ พร้อม CheckOutTime จาก Transactions
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

            try
            {
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
                            "SELECT TOP 1 Timestamp FROM Transactions WHERE Timestamp IS NOT NULL ORDER BY Timestamp DESC",
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

                    // ✅ Query ที่ดึง CheckOutTime จาก Transactions โดยตรง
                    var query = @"
                        SELECT 
                            a.AttendanceID, 
                            a.EmpID, 
                            a.Date, 
                            a.CheckInTime,
                            COALESCE(
                                a.CheckOutTime,
                                (SELECT CAST(MAX(t.Timestamp) AS TIME) 
                                 FROM Transactions t 
                                 WHERE t.EmpID = a.EmpID 
                                   AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
                                   AND t.Timestamp > CAST(CAST(a.Date AS DATE) AS DATETIME) + CAST(a.CheckInTime AS DATETIME))
                            ) AS CheckOutTime,
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

                    query += " ORDER BY CAST(a.Date AS DATE), a.EmpID";
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 300;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                attendanceID = reader.GetInt32(0),
                                empID = reader.GetInt32(1),
                                date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                checkInTime = reader.IsDBNull(3)
                                    ? null
                                    : reader.GetTimeSpan(3).ToString(@"hh\:mm\:ss"),
                                checkOutTime = reader.IsDBNull(4)
                                    ? null
                                    : reader.GetTimeSpan(4).ToString(@"hh\:mm\:ss"),
                                status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                division = reader.IsDBNull(6) ? null : reader.GetString(6),
                                department = reader.IsDBNull(7) ? null : reader.GetString(7),
                                section = reader.IsDBNull(8) ? null : reader.GetString(8),
                                biz = reader.IsDBNull(9) ? null : reader.GetString(9),
                                process = reader.IsDBNull(10) ? null : reader.GetString(10),
                                firstName = reader.GetString(11),
                                lastName = reader.GetString(12),
                            });
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error fetching attendance data",
                    detail = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // [HttpGet] ดึงข้อมูลทั้งหมด
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

            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var query = @"
                        SELECT 
                            a.AttendanceID, 
                            a.EmpID, 
                            CAST(a.Date AS DATE) AS Date, 
                            a.CheckInTime,
                            COALESCE(
                                a.CheckOutTime,
                                (SELECT CAST(MAX(t.Timestamp) AS TIME) 
                                 FROM Transactions t 
                                 WHERE t.EmpID = a.EmpID 
                                   AND CAST(t.Timestamp AS DATE) = CAST(a.Date AS DATE)
                                   AND t.Timestamp > CAST(CAST(a.Date AS DATE) AS DATETIME) + CAST(a.CheckInTime AS DATETIME))
                            ) AS CheckOutTime,
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

                    query += " ORDER BY CAST(a.Date AS DATE), a.EmpID";
                    cmd.CommandText = query;
                    cmd.CommandTimeout = 300;

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(new
                            {
                                attendanceID = reader.GetInt32(0),
                                empID = reader.GetInt32(1),
                                date = reader.GetDateTime(2).ToString("yyyy-MM-dd"),
                                checkInTime = reader.IsDBNull(3)
                                    ? null
                                    : reader.GetTimeSpan(3).ToString(@"hh\:mm\:ss"),
                                checkOutTime = reader.IsDBNull(4)
                                    ? null
                                    : reader.GetTimeSpan(4).ToString(@"hh\:mm\:ss"),
                                status = reader.IsDBNull(5) ? null : reader.GetString(5),
                                division = reader.IsDBNull(6) ? null : reader.GetString(6),
                                department = reader.IsDBNull(7) ? null : reader.GetString(7),
                                section = reader.IsDBNull(8) ? null : reader.GetString(8),
                                biz = reader.IsDBNull(9) ? null : reader.GetString(9),
                                process = reader.IsDBNull(10) ? null : reader.GetString(10),
                                firstName = reader.GetString(11),
                                lastName = reader.GetString(12),
                            });
                        }
                    }
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Error fetching all attendance data",
                    detail = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }
    }
}