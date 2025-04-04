// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System;
// using System.Data.SqlClient;
// using System.Threading.Tasks;
// using Api.Models;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class EICCMonthlySummaryController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public EICCMonthlySummaryController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         [HttpPost("upsert")]
//         public async Task<IActionResult> UpsertMonthlySummary([FromBody] EICC_MonthlySummary summary)
//         {
//             using (var conn = new SqlConnection(_connectionString))
//             {
//                 await conn.OpenAsync();

//                 // ตรวจสอบว่ามีอยู่แล้วหรือไม่ (อิงจาก EmpID + Year + Month)
//                 var checkCmd = new SqlCommand(@"
//                     SELECT COUNT(*) FROM EICC_MonthlySummary
//                     WHERE EmpID = @EmpID AND Year = @Year AND Month = @Month", conn);

//                 checkCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
//                 checkCmd.Parameters.AddWithValue("@Year", summary.Year);
//                 checkCmd.Parameters.AddWithValue("@Month", summary.Month);

//                 var count = (int)await checkCmd.ExecuteScalarAsync();

//                 if (count > 0)
//                 {
//                     // ถ้ามีแล้ว → UPDATE
//                     var updateCmd = new SqlCommand(@"
//                         UPDATE EICC_MonthlySummary SET
//                             OT_Total = @OT_Total,
//                             EICC_Hours = @EICC_Hours,
//                             TotalHours = @TotalHours
//                         WHERE EmpID = @EmpID AND Year = @Year AND Month = @Month", conn);

//                     updateCmd.Parameters.AddWithValue("@OT_Total", summary.OT_Total);
//                     updateCmd.Parameters.AddWithValue("@EICC_Hours", summary.EICC_Hours);
//                     updateCmd.Parameters.AddWithValue("@TotalHours", summary.TotalHours);
//                     updateCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
//                     updateCmd.Parameters.AddWithValue("@Year", summary.Year);
//                     updateCmd.Parameters.AddWithValue("@Month", summary.Month);

//                     await updateCmd.ExecuteNonQueryAsync();
//                     return Ok("Updated existing monthly summary.");
//                 }
//                 else
//                 {
//                     // ถ้ายังไม่มี → INSERT
//                     var insertCmd = new SqlCommand(@"
//                         INSERT INTO EICC_MonthlySummary (EmpID, Year, Month, OT_Total, EICC_Hours, TotalHours)
//                         VALUES (@EmpID, @Year, @Month, @OT_Total, @EICC_Hours, @TotalHours)", conn);

//                     insertCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
//                     insertCmd.Parameters.AddWithValue("@Year", summary.Year);
//                     insertCmd.Parameters.AddWithValue("@Month", summary.Month);
//                     insertCmd.Parameters.AddWithValue("@OT_Total", summary.OT_Total);
//                     insertCmd.Parameters.AddWithValue("@EICC_Hours", summary.EICC_Hours);
//                     insertCmd.Parameters.AddWithValue("@TotalHours", summary.TotalHours);

//                     await insertCmd.ExecuteNonQueryAsync();
//                     return Ok("Inserted new monthly summary.");
//                 }
//             }
//         }
//     }
// }
