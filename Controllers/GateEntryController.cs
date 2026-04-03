// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Data.SqlClient;

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

//             using var conn = new SqlConnection(
//                 _configuration.GetConnectionString("DefaultConnection")
//             );

//             await conn.OpenAsync();

//             // ✅ WorkDate (รองรับ Night Shift)
//             DateTime now = DateTime.Now;

//             DateTime workDate = date.HasValue
//                 ? date.Value.Date
//                 : (now.Hour < 6 ? now.Date.AddDays(-1) : now.Date);

//             DateTime dateStart = workDate.AddHours(6);
//             DateTime dateEnd = workDate.AddDays(1).AddHours(6).AddSeconds(-1);

//             // ✅ ใช้ Camera ล่าสุด
//             var query = @"
//                 WITH LatestTrans AS (
//                     SELECT 
//                         t.EmpID,
//                         t.CameraID,
//                         t.Timestamp,
//                         ROW_NUMBER() OVER (
//                             PARTITION BY t.EmpID
//                             ORDER BY t.Timestamp DESC
//                         ) AS rn
//                     FROM Transactions t
//                     WHERE t.Timestamp BETWEEN @dateStart AND @dateEnd
//                 )

//                 SELECT 
//                     e.EmpID,
//                     e.FirstName,
//                     e.LastName,
//                     e.Division,
//                     e.Department,
//                     e.Section,
//                     e.Biz,
//                     e.Process,
//                     lt.CameraID,
//                     lt.Timestamp
//                 FROM EmployeeInfo e
//                 LEFT JOIN LatestTrans lt 
//                     ON e.EmpID = lt.EmpID AND lt.rn = 1

//                 WHERE 1=1
//                 AND (@division IS NULL OR e.Division = @division)
//                 AND (@department IS NULL OR e.Department = @department)
//                 AND (@section IS NULL OR e.Section = @section)
//                 AND (@biz IS NULL OR e.Biz = @biz)
//                 AND (@process IS NULL OR e.Process = @process)
//             ";

//             using var cmd = new SqlCommand(query, conn);

//             cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
//             cmd.Parameters.AddWithValue("@dateStart", dateStart);
//             cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

//             using var reader = await cmd.ExecuteReaderAsync();

//             while (await reader.ReadAsync())
//             {
//                 int? cameraID = reader["CameraID"] as int?;

//                 string status;

//                 if (cameraID == null)
//                 {
//                     status = "status-missing";
//                 }
//                 else if (cameraID == 3)
//                 {
//                     status = "status-in-cleanroom";
//                 }
//                 else if (cameraID == 1 || cameraID == 2)
//                 {
//                     status = "status-out-cleanroom";
//                 }
//                 else
//                 {
//                     status = "status-missing";
//                 }

//                 result.Add(new
//                 {
//                     workDate = workDate.ToString("yyyy-MM-dd"),

//                     empID = reader["EmpID"],
//                     firstName = reader["FirstName"],
//                     lastName = reader["LastName"],

//                     division = reader["Division"],
//                     department = reader["Department"],
//                     section = reader["Section"],
//                     biz = reader["Biz"],
//                     process = reader["Process"],

//                     cameraID,
//                     status
//                 });
//             }

//             return Ok(new
//             {
//                 workDate = workDate.ToString("yyyy-MM-dd"),
//                 data = result
//             });
//         }
//     }
// }