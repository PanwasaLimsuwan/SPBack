using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;

namespace Api.Jobs
{
    public class CalculatedOTJob : BackgroundService
    {
        private readonly IConfiguration _configuration;

        public CalculatedOTJob(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    string connStr = _configuration.GetConnectionString("DefaultConnection");

                    using (var conn = new SqlConnection(connStr))
                    {
                        await conn.OpenAsync();

                        var cmd = new SqlCommand(@"
                            SELECT 
                                EmpID,
                                DATEPART(YEAR, Date) AS Year,
                                DATEPART(WEEK, Date) AS WeekID,
                                SUM(WorkedHours) AS TotalHours,
                                SUM(OTHours) AS TotalOT,
                                COUNT(DISTINCT CAST(Date AS DATE)) AS DaysWorked
                            FROM CalculatedWorktime
                            GROUP BY EmpID, DATEPART(YEAR, Date), DATEPART(WEEK, Date)", conn);

                        var reader = await cmd.ExecuteReaderAsync();

                        var summaryList = new List<(string EmpID, int Year, int WeekID, double TotalHours, double TotalOT, int DaysWorked)>();

                        while (await reader.ReadAsync())
                        {
                            summaryList.Add((
                                reader["EmpID"].ToString(),
                                Convert.ToInt32(reader["Year"]),
                                Convert.ToInt32(reader["WeekID"]),
                                Convert.ToDouble(reader["TotalHours"]),
                                Convert.ToDouble(reader["TotalOT"]),
                                Convert.ToInt32(reader["DaysWorked"])
                            ));
                        }

                        await reader.CloseAsync();

                        foreach (var summary in summaryList)
                        {
                            var checkCmd = new SqlCommand(@"
                                SELECT COUNT(*) FROM EICC_Control
                                WHERE EmpID = @EmpID AND WeekID = @WeekID", conn);

                            checkCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                            checkCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);

                            var count = (int)await checkCmd.ExecuteScalarAsync();

                            if (count > 0)
                            {
                                var updateCmd = new SqlCommand(@"
                                    UPDATE EICC_Control
                                    SET TotalHours = @TotalHours, DaysWorked = @DaysWorked, TotalOT = @TotalOT, Status = @Status
                                    WHERE EmpID = @EmpID AND WeekID = @WeekID", conn);

                                updateCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                                updateCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);
                                updateCmd.Parameters.AddWithValue("@TotalHours", summary.TotalHours);
                                updateCmd.Parameters.AddWithValue("@DaysWorked", summary.DaysWorked);
                                updateCmd.Parameters.AddWithValue("@TotalOT", summary.TotalOT);
                                updateCmd.Parameters.AddWithValue("@Status", "Active");

                                await updateCmd.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                var insertCmd = new SqlCommand(@"
                                    INSERT INTO EICC_Control (EmpID, WeekID, TotalHours, DaysWorked, TotalOT, Status)
                                    VALUES (@EmpID, @WeekID, @TotalHours, @DaysWorked, @TotalOT, @Status)", conn);

                                insertCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                                insertCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);
                                insertCmd.Parameters.AddWithValue("@TotalHours", summary.TotalHours);
                                insertCmd.Parameters.AddWithValue("@DaysWorked", summary.DaysWorked);
                                insertCmd.Parameters.AddWithValue("@TotalOT", summary.TotalOT);
                                insertCmd.Parameters.AddWithValue("@Status", "Active");

                                await insertCmd.ExecuteNonQueryAsync();
                            }
                        }
                    }

                    Console.WriteLine($"[CalculatedOTJob] Updated at {DateTime.Now}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[CalculatedOTJob] ERROR: {ex.Message}");
                }

                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
