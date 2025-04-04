// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System;
// using System.Collections.Generic;
// using System.Data.SqlClient;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class GateToWorktimeController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public GateToWorktimeController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         [HttpPost]
//         public async Task<IActionResult> ConvertGateToWorktime()
//         {
//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 // ดึงข้อมูลจาก GateEntry ที่มี Entry/Exit
//                 var selectCmd = new SqlCommand(@"
//                     SELECT EmpID, EntryDateTime, ExitDateTime
//                     FROM GateEntry
//                     WHERE EntryDateTime IS NOT NULL AND ExitDateTime IS NOT NULL", conn);

//                 var reader = await selectCmd.ExecuteReaderAsync();

//                 var worktimeData = new List<(string EmpID, DateTime Date, double WorkedHours)>();

//                 while (await reader.ReadAsync())
//                 {
//                     var empId = reader["EmpID"].ToString();
//                     var entry = (DateTime)reader["EntryDateTime"];
//                     var exit = (DateTime)reader["ExitDateTime"];
//                     var hours = (exit - entry).TotalHours;

//                     worktimeData.Add((empId, entry.Date, hours));
//                 }

//                 await reader.CloseAsync();

//                 int inserted = 0;

//                 // เปลี่ยนเฉพาะบรรทัดใน foreach เท่านั้น
// foreach (var item in worktimeData)
// {
//     // เช็คก่อนว่ามีข้อมูลอยู่แล้วไหม
//     var checkCmd = new SqlCommand(@"
//         SELECT COUNT(*) FROM CalculatedWorktime
//         WHERE EmpID = @EmpID AND Date = @Date", conn);

//     checkCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//     checkCmd.Parameters.AddWithValue("@Date", item.Date);

//     var count = (int)await checkCmd.ExecuteScalarAsync();

//     if (count == 0) // ถ้ายังไม่มี ให้ Insert ได้
//     {
//         var insertCmd = new SqlCommand(@"
//             INSERT INTO CalculatedWorktime (EmpID, Date, WorkedHours, Status)
//             VALUES (@EmpID, @Date, @WorkedHours, @Status)", conn);

//         insertCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//         insertCmd.Parameters.AddWithValue("@Date", item.Date);
//         insertCmd.Parameters.AddWithValue("@WorkedHours", item.WorkedHours);
//         insertCmd.Parameters.AddWithValue("@Status", "auto");

//         await insertCmd.ExecuteNonQueryAsync();
//     }
// }



//                 return Ok($"Worktime inserted: {inserted} records.");
//             }
//         }
//     }
// }
