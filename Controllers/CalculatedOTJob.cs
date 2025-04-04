// using System;
// using System.Data.SqlClient;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Hosting;
// using Microsoft.Extensions.Configuration;

// public class CalculatedOTJob : BackgroundService
// {
//     private readonly IConfiguration _configuration;

//     public CalculatedOTJob(IConfiguration configuration)
//     {
//         _configuration = configuration;
//     }

//     protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//     {
//         while (!stoppingToken.IsCancellationRequested)
//         {
//             try
//             {
//                 string connStr = _configuration.GetConnectionString("DefaultConnection");

//                 using (var conn = new SqlConnection(connStr))
//                 {
//                     await conn.OpenAsync();

//                     var cmd = new SqlCommand(@"
//                         SELECT EmpID, 
//                                YEAR(Date) AS Year,
//                                MONTH(Date) AS Month,
//                                SUM(WorkedHours) AS TotalHours,
//                                SUM(CASE WHEN WorkedHours > 8 THEN WorkedHours - 8 ELSE 0 END) AS OT_Total
//                         FROM CalculatedWorktime
//                         GROUP BY EmpID, YEAR(Date), MONTH(Date)", conn);

//                     var reader = await cmd.ExecuteReaderAsync();

//                     var summaryList = new List<(string EmpID, int Year, int Month, int OT_Total, float TotalHours)>();

//                     while (await reader.ReadAsync())
//                     {
//                         var empID = reader["EmpID"].ToString();
//                         var year = (int)reader["Year"];
//                         var month = (int)reader["Month"];
//                         var totalHours = Convert.ToSingle(reader["TotalHours"]);
//                         var ot = Convert.ToInt32(reader["OT_Total"]);

//                         summaryList.Add((empID, year, month, ot, totalHours));
//                     }

//                     await reader.CloseAsync();

//                     foreach (var s in summaryList)
//                     {
//                         var checkCmd = new SqlCommand(@"
//                             SELECT COUNT(*) FROM EICC_MonthlySummary
//                             WHERE EmpID = @EmpID AND Year = @Year AND Month = @Month", conn);

//                         checkCmd.Parameters.AddWithValue("@EmpID", s.EmpID);
//                         checkCmd.Parameters.AddWithValue("@Year", s.Year);
//                         checkCmd.Parameters.AddWithValue("@Month", s.Month);

//                         var count = (int)await checkCmd.ExecuteScalarAsync();

//                         if (count > 0)
//                         {
//                             var updateCmd = new SqlCommand(@"
//                                 UPDATE EICC_MonthlySummary SET
//                                     OT_Total = @OT_Total,
//                                     TotalHours = @TotalHours,
//                                     EICC_Hours = @TotalHours
//                                 WHERE EmpID = @EmpID AND Year = @Year AND Month = @Month", conn);

//                             updateCmd.Parameters.AddWithValue("@EmpID", s.EmpID);
//                             updateCmd.Parameters.AddWithValue("@Year", s.Year);
//                             updateCmd.Parameters.AddWithValue("@Month", s.Month);
//                             updateCmd.Parameters.AddWithValue("@OT_Total", s.OT_Total);
//                             updateCmd.Parameters.AddWithValue("@TotalHours", s.TotalHours);

//                             await updateCmd.ExecuteNonQueryAsync();
//                         }
//                         else
//                         {
//                             var insertCmd = new SqlCommand(@"
//                                 INSERT INTO EICC_MonthlySummary (EmpID, Year, Month, OT_Total, EICC_Hours, TotalHours)
//                                 VALUES (@EmpID, @Year, @Month, @OT_Total, @TotalHours, @TotalHours)", conn);

//                             insertCmd.Parameters.AddWithValue("@EmpID", s.EmpID);
//                             insertCmd.Parameters.AddWithValue("@Year", s.Year);
//                             insertCmd.Parameters.AddWithValue("@Month", s.Month);
//                             insertCmd.Parameters.AddWithValue("@OT_Total", s.OT_Total);
//                             insertCmd.Parameters.AddWithValue("@TotalHours", s.TotalHours);

//                             await insertCmd.ExecuteNonQueryAsync();
//                         }
//                     }
//                 }

//                 Console.WriteLine($"[CalculatedOTJob] Updated at {DateTime.Now}");
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"[CalculatedOTJob] ERROR: {ex.Message}");
//             }

//             await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
//         }
//     }
// }
