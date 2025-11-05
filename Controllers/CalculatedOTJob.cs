using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Api.Jobs
{
    public class CalculatedOTJob : BackgroundService
    {
        private readonly IConfiguration _configuration;

        public CalculatedOTJob(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันหลักที่ใช้ในการทำงานใน Background Service
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // เชื่อมต่อกับฐานข้อมูลโดยใช้ connection string
                    string connStr = _configuration.GetConnectionString("DefaultConnection");

                    using (var conn = new SqlConnection(connStr))
                    {
                        await conn.OpenAsync();

                        // คำสั่ง SQL ที่ใช้ดึงข้อมูลการทำ OT
                        var cmd = new SqlCommand(
                            @"
                            SELECT 
                                EmpID,
                                FORMAT(Date, 'yyyy-MM') AS MonthYear,
                                DATEPART(YEAR, Date) AS Year,
                                DATEPART(WEEK, Date) AS WeekID,
                                SUM(WorkedHours) AS TotalHours,
                                SUM(OTHours) AS TotalOT,
                                COUNT(DISTINCT CAST(Date AS DATE)) AS DaysWorked
                            FROM CalculatedWorktime
                            GROUP BY EmpID, FORMAT(Date, 'yyyy-MM'), DATEPART(YEAR, Date), DATEPART(WEEK, Date)",
                            conn
                        );

                        var reader = await cmd.ExecuteReaderAsync();

                        // เก็บข้อมูลที่ดึงมาจาก SQL
                        var summaryList =
                            new List<(
                                int EmpID,
                                string MonthYear,
                                int Year,
                                int WeekID,
                                double TotalHours,
                                double TotalOT,
                                int DaysWorked
                            )>();

                        // อ่านข้อมูลจาก SQL
                        while (await reader.ReadAsync())
                        {
                            summaryList.Add(
                                (
                                    Convert.ToInt32(reader["EmpID"]),
                                    reader["MonthYear"].ToString(),
                                    Convert.ToInt32(reader["Year"]),
                                    Convert.ToInt32(reader["WeekID"]),
                                    Convert.ToDouble(reader["TotalHours"]),
                                    Convert.ToDouble(reader["TotalOT"]),
                                    Convert.ToInt32(reader["DaysWorked"])
                                )
                            );
                        }

                        await reader.CloseAsync();

                        // อัปเดตหรือแทรกข้อมูลใน EICC_Control
                        // ✅ แก้ไขส่วนนี้ใน CalculatedOTJob.cs
                        foreach (var summary in summaryList)
                        {
                            var checkCmd = new SqlCommand(
                                @"
        SELECT COUNT(*) FROM EICC_Control
        WHERE EmpID = @EmpID AND WeekID = @WeekID",
                                conn
                            );

                            checkCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                            checkCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);

                            var count = (int)await checkCmd.ExecuteScalarAsync();

                            // ✅ ตรวจสอบว่าพนักงานยังทำงานอยู่หรือไม่
                            var statusCheckCmd = new SqlCommand(
                                @"
        SELECT TOP 1 Status 
        FROM CalculatedWorktime 
        WHERE EmpID = @EmpID 
        ORDER BY Date DESC",
                                conn
                            );
                            statusCheckCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                            var latestStatus =
                                (await statusCheckCmd.ExecuteScalarAsync())?.ToString() ?? "Active";

                            // ✅ กำหนด status ตามสถานะล่าสุด
                            var eiccStatus = latestStatus == "Finished" ? "Complete" : "Active";

                            if (count > 0)
                            {
                                var updateCmd = new SqlCommand(
                                    @"
            UPDATE EICC_Control
            SET MonthYear = @MonthYear, TotalHours = @TotalHours, 
                DaysWorked = @DaysWorked, TotalOT = @TotalOT, Status = @Status
            WHERE EmpID = @EmpID AND WeekID = @WeekID",
                                    conn
                                );

                                updateCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                                updateCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);
                                updateCmd.Parameters.AddWithValue("@MonthYear", summary.MonthYear);
                                updateCmd.Parameters.AddWithValue(
                                    "@TotalHours",
                                    summary.TotalHours
                                );
                                updateCmd.Parameters.AddWithValue(
                                    "@DaysWorked",
                                    summary.DaysWorked
                                );
                                updateCmd.Parameters.AddWithValue("@TotalOT", summary.TotalOT);
                                updateCmd.Parameters.AddWithValue("@Status", eiccStatus); // ✅ ใช้ status ที่คำนวณแล้ว

                                await updateCmd.ExecuteNonQueryAsync();
                            }
                            else
                            {
                                var insertCmd = new SqlCommand(
                                    @"
            INSERT INTO EICC_Control (EmpID, WeekID, MonthYear, TotalHours, DaysWorked, TotalOT, Status)
            VALUES (@EmpID, @WeekID, @MonthYear, @TotalHours, @DaysWorked, @TotalOT, @Status)",
                                    conn
                                );

                                insertCmd.Parameters.AddWithValue("@EmpID", summary.EmpID);
                                insertCmd.Parameters.AddWithValue("@WeekID", summary.WeekID);
                                insertCmd.Parameters.AddWithValue("@MonthYear", summary.MonthYear);
                                insertCmd.Parameters.AddWithValue(
                                    "@TotalHours",
                                    summary.TotalHours
                                );
                                insertCmd.Parameters.AddWithValue(
                                    "@DaysWorked",
                                    summary.DaysWorked
                                );
                                insertCmd.Parameters.AddWithValue("@TotalOT", summary.TotalOT);
                                insertCmd.Parameters.AddWithValue("@Status", eiccStatus); // ✅ ใช้ status ที่คำนวณแล้ว

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

                // หน่วงเวลา 5 วินาทีก่อนที่จะทำงานใหม่
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
        }
    }
}
