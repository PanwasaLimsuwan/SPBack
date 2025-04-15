using Microsoft.Extensions.Hosting;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace Api.Services
{
    public class CalculatedAttendanceJob : BackgroundService
    {
        private readonly IConfiguration _configuration;

        public CalculatedAttendanceJob(IConfiguration configuration)
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

                // ดึงข้อมูลจาก GateEntry ที่มี EntryDateTime (พนักงานที่เข้ามาแล้ว)
                var cmd = new SqlCommand(@"
                    SELECT EmpID, EntryDateTime, ExitDateTime
                    FROM GateEntry
                    WHERE EntryDateTime IS NOT NULL", conn);

                var reader = await cmd.ExecuteReaderAsync();

                var attendanceRecords = new List<(int EmpID, DateTime EntryDateTime, DateTime? ExitDateTime)>();

                while (await reader.ReadAsync())
                {
                    var empID = Convert.ToInt32(reader["EmpID"]);
                    var entryDateTime = reader.GetDateTime(reader.GetOrdinal("EntryDateTime"));
                    var exitDateTime = reader.IsDBNull(reader.GetOrdinal("ExitDateTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ExitDateTime"));
                    attendanceRecords.Add((empID, entryDateTime, exitDateTime));
                }

                await reader.CloseAsync();

                // Insert attendance for employees with calculated status and Entry/Exit times
                foreach (var record in attendanceRecords)
                {
                    var status = CalculateStatus(record.EntryDateTime);
                    var currentDate = DateTime.Now.Date;

                    // Pass both EntryDateTime and ExitDateTime to the InsertAttendance method
                    await InsertAttendance(conn, record.EmpID, record.EntryDateTime, record.ExitDateTime, status);
                }

                // สำหรับพนักงานที่ขาดงาน (ไม่มีเวลาเข้ามา) ให้บันทึกสถานะ "Missing"
                var missingEmployees = new List<int>();
                var cmd2 = new SqlCommand(@"
                    SELECT EmpID
                    FROM GateEntry
                    WHERE EntryDateTime IS NULL", conn);

                reader = await cmd2.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    missingEmployees.Add(Convert.ToInt32(reader["EmpID"]));
                }

                await reader.CloseAsync();

                foreach (var empID in missingEmployees)
                {
                    var status = "Missing";
                    var currentDate = DateTime.Now.Date;

                    // For missing employees, pass null for both EntryDateTime and ExitDateTime
                    await InsertAttendance(conn, empID, currentDate, null, status);
                }
            }

            Console.WriteLine($"[CalculatedAttendanceJob] Updated at {DateTime.Now}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[CalculatedAttendanceJob] ERROR: {ex.Message}");
        }

        // รอ 10 วินาทีแล้วทำงานต่อ
        await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
    }
}


private string CalculateStatus(DateTime entryDateTime)
{
    string status = "unknown"; // กำหนดค่าเริ่มต้นให้กับ status
    TimeSpan entryTimeOfDay = entryDateTime.TimeOfDay;

    // เช็คกะเช้า
    if (entryTimeOfDay < TimeSpan.FromHours(7) && entryTimeOfDay < TimeSpan.FromHours(19))  // ก่อน 07:00
    {
        status = "normal";  // เข้างานก่อนเวลา
    }
    else if (entryTimeOfDay >= TimeSpan.FromHours(7) && entryTimeOfDay <= TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))  // 07:00 - 07:15
    {
        status = "normal";  // เข้าตรงเวลา กะเช้า
    }
    // เข้าช้าหลัง 07:15 จะถือว่า late กะเช้า
    else if (entryTimeOfDay > TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)) && entryTimeOfDay <= TimeSpan.FromHours(19))
    {
        status = "late";  // เข้าช้า กะเช้า
    }

    // เช็คกะดึก
    else if (entryTimeOfDay >= TimeSpan.FromHours(19) && entryTimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))  // 19:00 - 19:15
    {
        status = "normal";  // เข้าตรงเวลา กะดึก
    }
    else if (entryTimeOfDay < TimeSpan.FromHours(19)) // เข้าก่อน 19:00
    {
        status = "normal";  // เข้าก่อนเวลา กะดึก
    }
    // ถ้าเข้าหลัง 19:15 ถือว่า late
    else if (entryTimeOfDay > TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
    {
        status = "late";  // เข้าช้า กะดึก
    }

    return status;
}

private async Task InsertAttendance(SqlConnection conn, int empID, DateTime entryDateTime, DateTime? exitDateTime, string status)
{
    // ตรวจสอบข้อมูลซ้ำก่อนทำการ insert
    var checkCmd = new SqlCommand(@"
        SELECT COUNT(1)
        FROM Attendance
        WHERE EmpID = @empID AND Date = @date", conn);

    checkCmd.Parameters.AddWithValue("@empID", empID);
    checkCmd.Parameters.AddWithValue("@date", entryDateTime.Date);

    var existingRecordCount = (int)await checkCmd.ExecuteScalarAsync();

    // ถ้ามีข้อมูลแล้วให้ข้ามการ insert
    if (existingRecordCount > 0)
    {
        Console.WriteLine($"[InsertAttendance] Duplicate entry found for EmpID: {empID} on {entryDateTime.Date}. Skipping insert.");
        return; // ข้ามการ insert
    }
    // If the status is "Missing", set CheckInTime to NULL
    var checkInTime = status == "Missing" ? (object)DBNull.Value : (object)entryDateTime;

    var insertCmd = new SqlCommand(@"
        INSERT INTO Attendance (EmpID, Date, Status, CheckInTime, CheckOutTime)
        VALUES (@empID, @date, @status, @checkInTime, @checkOutTime)", conn);

    insertCmd.Parameters.AddWithValue("@empID", empID);
    insertCmd.Parameters.AddWithValue("@date", entryDateTime.Date);
    insertCmd.Parameters.AddWithValue("@status", status);
    insertCmd.Parameters.AddWithValue("@checkInTime", checkInTime);  // Use NULL if status is "Missing"
    insertCmd.Parameters.AddWithValue("@checkOutTime", exitDateTime.HasValue ? (object)exitDateTime.Value : DBNull.Value);

    await insertCmd.ExecuteNonQueryAsync();
}

    }
}
