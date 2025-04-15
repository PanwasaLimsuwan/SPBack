using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatedAttendance : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CalculatedAttendance(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpPost("GenerateAttendance")]
        public async Task<IActionResult> GenerateAttendance([FromQuery] DateTime? date)
        {
            using (var conn = new SqlConnection(_configuration.GetConnectionString("DefaultConnection")))
            {
                await conn.OpenAsync();

                DateTime selectedDate = date?.Date ?? DateTime.Now.Date;
                DateTime dateStart = selectedDate;
                DateTime dateEnd = selectedDate.AddDays(1).AddSeconds(-1);

                async Task InsertAttendance(string status, string query)
                {
                    var empIDs = new List<int>();

                    using (var cmd = new SqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@dateStart", dateStart);
                        cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

                        using (var reader = await cmd.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                empIDs.Add(Convert.ToInt32(reader["EmpID"]));
                            }
                        }
                    }

                    foreach (var empID in empIDs)
                    {
                        // ตรวจสอบว่าใน Attendance มีข้อมูลที่ตรงกับ EmpID, Date และ Status หรือไม่
                        var checkCmd = new SqlCommand(@"
                            SELECT COUNT(1) FROM Attendance 
                            WHERE EmpID = @empID AND Date = @date AND Status = @status
                        ", conn);
                        checkCmd.Parameters.AddWithValue("@empID", empID);
                        checkCmd.Parameters.AddWithValue("@date", selectedDate);
                        checkCmd.Parameters.AddWithValue("@status", status);

                        var count = (int)await checkCmd.ExecuteScalarAsync();

                        // เพิ่มการ log เพื่อตรวจสอบผลลัพธ์
                        Console.WriteLine($"EmpID: {empID}, Status: {status}, Count: {count}");

                        // ถ้าจำนวนแถวที่พบเป็น 0 ให้ทำการ insert
                        if (count == 0)
                        {
                            var insertCmd = new SqlCommand(@"
                                INSERT INTO Attendance (EmpID, Date, Status)
                                VALUES (@empID, @date, @status)
                            ", conn);
                            insertCmd.Parameters.AddWithValue("@empID", empID);
                            insertCmd.Parameters.AddWithValue("@date", selectedDate);
                            insertCmd.Parameters.AddWithValue("@status", status);

                            await insertCmd.ExecuteNonQueryAsync();
                            // เพิ่มการ log หลังจาก insert ข้อมูล
                            Console.WriteLine($"Inserted Attendance: EmpID = {empID}, Status = {status}");
                        }
                    }
                }

                // Query สำหรับการสร้างสถานะต่าง ๆ

                // Generate absent
                await InsertAttendance("absent", @"
                    SELECT e.EmpID
                    FROM EmployeeInfo e
                    WHERE NOT EXISTS (
                        SELECT 1 FROM GateEntry g
                        WHERE g.EmpID = e.EmpID
                        AND g.EntryDateTime BETWEEN @dateStart AND @dateEnd
                    )
                ");

                // Generate late (หลัง 07:15 หรือ 19:15)
                await InsertAttendance("late", @"
                    SELECT DISTINCT g.EmpID
                    FROM GateEntry g
                    WHERE g.EntryDateTime BETWEEN @dateStart AND @dateEnd
                    AND (
                        CAST(g.EntryDateTime AS TIME) > '07:15:00' OR 
                        CAST(g.EntryDateTime AS TIME) > '19:15:00'
                    )
                ");

                // Generate normal (ก่อนเวลา 07:15 หรือ 19:15)
                await InsertAttendance("normal", @"
                    SELECT DISTINCT g.EmpID
                    FROM GateEntry g
                    WHERE g.EntryDateTime BETWEEN @dateStart AND @dateEnd
                    AND (
                        CAST(g.EntryDateTime AS TIME) <= '07:15:00' OR 
                        CAST(g.EntryDateTime AS TIME) <= '19:15:00'
                    )
                ");

                // Generate status-missing สำหรับข้อมูลที่ไม่มี EntryDateTime
                await InsertAttendance("status-missing", @"
                    SELECT e.EmpID
                    FROM EmployeeInfo e
                    WHERE NOT EXISTS (
                        SELECT 1 FROM GateEntry g
                        WHERE g.EmpID = e.EmpID
                        AND g.EntryDateTime BETWEEN @dateStart AND @dateEnd
                    )
                ");
            }

            return Ok(new { message = "Attendance generation completed." });
        }
    }
}
