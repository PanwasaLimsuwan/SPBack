// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;
// using System.Collections.Generic;
// using System.Threading.Tasks;
// using System;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class GateEntryController : ControllerBase
//     {
//         private readonly IConfiguration _configuration;

//         public GateEntryController(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetGateEntry(
//             [FromQuery] string? division,
//             [FromQuery] string? department,
//             [FromQuery] string? section,
//             [FromQuery] string? biz,
//             [FromQuery] string? process,
//             [FromQuery] DateTime? date
//         )
//         {
//             var result = new List<object>();

//             using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
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
//                         "SELECT TOP 1 Timestamp FROM FaceVectors WHERE Timestamp IS NOT NULL ORDER BY Timestamp DESC",
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
//                 DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1);  // ครอบคลุมเวลาออกกะ 07:00 ของวันถัดไป

//                 var query =
//                     @"
//                     WITH RankedFaceVector AS (
//                         SELECT 
//                             f.EmpID,
//                             e.FirstName, e.LastName, e.Division, e.Department, 
//                             e.Position, e.Email, e.ShiftCode, e.Section,
//                             e.Biz, e.Process,
//                             f.Vector, f.Timestamp,
//                             ROW_NUMBER() OVER (PARTITION BY f.EmpID ORDER BY f.Timestamp DESC) AS rn
//                         FROM FaceVectors f
//                         JOIN EmployeeInfo e ON f.EmpID = e.EmpID
//                         WHERE f.Timestamp BETWEEN @dateStart AND @dateEnd
//                         AND (@division IS NULL OR e.Division = @division)
//                         AND (@department IS NULL OR e.Department = @department)
//                         AND (@section IS NULL OR e.Section = @section)
//                         AND (@biz IS NULL OR COALESCE(e.Biz, '') = @biz)
//                         AND (@process IS NULL OR COALESCE(e.Process, '') = @process)
//                     )
//                     SELECT * FROM RankedFaceVector WHERE rn = 1";

//                 using (var cmd = new SqlCommand(query, conn))
//                 {
//                     cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
//                     cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
//                     cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
//                     cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
//                     cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
//                     cmd.Parameters.AddWithValue("@dateStart", dateStart);
//                     cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

//                     using (var reader = await cmd.ExecuteReaderAsync())
//                     {
//                         while (await reader.ReadAsync())
//                         {
//                             var empID = Convert.ToInt32(reader["EmpID"]);
//                             var vector = reader["Vector"]?.ToString();
//                             var timestamp = reader["Timestamp"] == DBNull.Value ? (DateTime?)null : (DateTime)reader["Timestamp"];

//                             result.Add(
//                                 new
//                                 {
//                                     EmpID = empID,
//                                     firstName = reader["FirstName"]?.ToString(),
//                                     lastName = reader["LastName"]?.ToString(),
//                                     division = reader["Division"]?.ToString(),
//                                     department = reader["Department"]?.ToString(),
//                                     position = reader["Position"]?.ToString(),
//                                     email = reader["Email"]?.ToString(),
//                                     shiftCode = reader["ShiftCode"]?.ToString(),
//                                     section = reader["Section"]?.ToString(),
//                                     vector,
//                                     timestamp = timestamp?.ToString("yyyy-MM-dd HH:mm:ss"),
//                                     biz = reader["Biz"]?.ToString(),
//                                     process = reader["Process"]?.ToString(),
//                                 }
//                             );
//                         }
//                     }
//                 }
//             }

//             return Ok(result);
//         }
//     }
// }
