// using Microsoft.Extensions.Hosting;
// using Microsoft.Data.SqlClient;
// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;

// namespace Api.Services
// {
//     public class CalculatedAttendanceJob : BackgroundService
//     {
//         private readonly IConfiguration _configuration;

//         public CalculatedAttendanceJob(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//       protected override async Task ExecuteAsync(CancellationToken stoppingToken)
// {
//     while (!stoppingToken.IsCancellationRequested)
//     {
//         try
//         {
//             string connStr = _configuration.GetConnectionString("DefaultConnection");

//             using (var conn = new SqlConnection(connStr))
//             {
//                 await conn.OpenAsync();

//                 // ดึงข้อมูลจาก GateEntry ที่มี EntryDateTime (พนักงานที่เข้ามาแล้ว)
//                 var cmd = new SqlCommand(@"
//                     SELECT EmpID, EntryDateTime, ExitDateTime
//                     FROM GateEntry
//                     WHERE EntryDateTime IS NOT NULL", conn);

//                 var reader = await cmd.ExecuteReaderAsync();

//                 var attendanceRecords = new List<(int EmpID, DateTime EntryDateTime, DateTime? ExitDateTime)>();

//                 while (await reader.ReadAsync())
//                 {
//                     var empID = Convert.ToInt32(reader["EmpID"]);
//                     var entryDateTime = reader.GetDateTime(reader.GetOrdinal("EntryDateTime"));
//                     var exitDateTime = reader.IsDBNull(reader.GetOrdinal("ExitDateTime")) ? (DateTime?)null : reader.GetDateTime(reader.GetOrdinal("ExitDateTime"));
//                     attendanceRecords.Add((empID, entryDateTime, exitDateTime));
//                 }

//                 await reader.CloseAsync();

//                 // Insert attendance for employees with calculated status and Entry/Exit times
//                 foreach (var record in attendanceRecords)
//                 {
//                     var status = CalculateStatus(record.EntryDateTime);
//                     var currentDate = DateTime.Now.Date;

//                     // Pass both EntryDateTime and ExitDateTime to the InsertAttendance method
//                     await InsertAttendance(conn, record.EmpID, record.EntryDateTime, record.ExitDateTime, status);
//                 }

//                 // สำหรับพนักงานที่ขาดงาน (ไม่มีเวลาเข้ามา) ให้บันทึกสถานะ "Missing"
//                 var missingEmployees = new List<int>();
//                 var cmd2 = new SqlCommand(@"
//                     SELECT EmpID
//                     FROM GateEntry
//                     WHERE EntryDateTime IS NULL", conn);

//                 reader = await cmd2.ExecuteReaderAsync();
//                 while (await reader.ReadAsync())
//                 {
//                     missingEmployees.Add(Convert.ToInt32(reader["EmpID"]));
//                 }

//                 await reader.CloseAsync();

//                 foreach (var empID in missingEmployees)
//                 {
//                     var status = "Missing";
//                     var currentDate = DateTime.Now.Date;

//                     // For missing employees, pass null for both EntryDateTime and ExitDateTime
//                     await InsertAttendance(conn, empID, currentDate, null, status);
//                 }
//             }

//             // Console.WriteLine($"[CalculatedAttendanceJob] Updated at {DateTime.Now}");
//         }
//         catch (Exception ex)
//         {
//             // Console.WriteLine($"[CalculatedAttendanceJob] ERROR: {ex.Message}");
//         }

//         // รอ 10 วินาทีแล้วทำงานต่อ
//         await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//     }
// }


// private string CalculateStatus(DateTime entryDateTime)
// {
//     string status = "unknown"; // กำหนดค่าเริ่มต้นให้กับ status
//     TimeSpan entryTimeOfDay = entryDateTime.TimeOfDay;

//     // เช็คกะเช้า
//     if (entryTimeOfDay < TimeSpan.FromHours(7) && entryTimeOfDay < TimeSpan.FromHours(19))  // ก่อน 07:00
//     {
//         status = "normal";  // เข้างานก่อนเวลา
//     }
//     else if (entryTimeOfDay >= TimeSpan.FromHours(7) && entryTimeOfDay <= TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))  // 07:00 - 07:15
//     {
//         status = "normal";  // เข้าตรงเวลา กะเช้า
//     }
//     // เข้าช้าหลัง 07:15 จะถือว่า late กะเช้า
//     else if (entryTimeOfDay > TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)) && entryTimeOfDay <= TimeSpan.FromHours(19))
//     {
//         status = "late";  // เข้าช้า กะเช้า
//     }

//     // เช็คกะดึก
//     else if (entryTimeOfDay >= TimeSpan.FromHours(19) && entryTimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))  // 19:00 - 19:15
//     {
//         status = "normal";  // เข้าตรงเวลา กะดึก
//     }
//     else if (entryTimeOfDay < TimeSpan.FromHours(19)) // เข้าก่อน 19:00
//     {
//         status = "normal";  // เข้าก่อนเวลา กะดึก
//     }
//     // ถ้าเข้าหลัง 19:15 ถือว่า late
//     else if (entryTimeOfDay > TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
//     {
//         status = "late";  // เข้าช้า กะดึก
//     }

//     return status;
// }

// private async Task InsertAttendance(SqlConnection conn, int empID, DateTime entryDateTime, DateTime? exitDateTime, string status)
// {
//     // ตรวจสอบข้อมูลซ้ำก่อนทำการ insert
//     var checkCmd = new SqlCommand(@"
//         SELECT COUNT(1)
//         FROM Attendance
//         WHERE EmpID = @empID AND Date = @date", conn);

//     checkCmd.Parameters.AddWithValue("@empID", empID);
//     checkCmd.Parameters.AddWithValue("@date", entryDateTime.Date);

//     var existingRecordCount = (int)await checkCmd.ExecuteScalarAsync();

//     // ถ้ามีข้อมูลแล้วให้ข้ามการ insert
//     if (existingRecordCount > 0)
//     {
//         // Console.WriteLine($"[InsertAttendance] Duplicate entry found for EmpID: {empID} on {entryDateTime.Date}. Skipping insert.");
//         return; // ข้ามการ insert
//     }
//     // If the status is "Missing", set CheckInTime to NULL
//     var checkInTime = status == "Missing" ? (object)DBNull.Value : (object)entryDateTime;

//     var insertCmd = new SqlCommand(@"
//         INSERT INTO Attendance (EmpID, Date, Status, CheckInTime, CheckOutTime)
//         VALUES (@empID, @date, @status, @checkInTime, @checkOutTime)", conn);

//     insertCmd.Parameters.AddWithValue("@empID", empID);
//     insertCmd.Parameters.AddWithValue("@date", entryDateTime.Date);
//     insertCmd.Parameters.AddWithValue("@status", status);
//     insertCmd.Parameters.AddWithValue("@checkInTime", checkInTime);  // Use NULL if status is "Missing"
//     insertCmd.Parameters.AddWithValue("@checkOutTime", exitDateTime.HasValue ? (object)exitDateTime.Value : DBNull.Value);

//     await insertCmd.ExecuteNonQueryAsync();
// }

//     }
// }

// using Microsoft.Extensions.Hosting;
// using Microsoft.Data.SqlClient;
// using System;
// using System.Collections.Generic;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;

// namespace Api.Services
// {
//     public class CalculatedAttendanceJob : BackgroundService
//     {
//         private readonly IConfiguration _configuration;

//         public CalculatedAttendanceJob(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     string connStr = _configuration.GetConnectionString("DefaultConnection");

//                     using (var conn = new SqlConnection(connStr))
//                     {
//                         await conn.OpenAsync();

//                         // ดึงข้อมูลจาก Transactions ที่มี Timestamp (พนักงานที่เข้ามาแล้ว)
//                         var cmd = new SqlCommand(@"
//                             SELECT EmpID, Timestamp
//                             FROM Transactions
//                             WHERE Timestamp IS NOT NULL", conn);

//                         var reader = await cmd.ExecuteReaderAsync();

//                         var attendanceRecords = new List<(int EmpID, DateTime Timestamp)>();

//                         while (await reader.ReadAsync())
//                         {
//                             var empID = Convert.ToInt32(reader["EmpID"]);
//                             var timestamp = reader.GetDateTime(reader.GetOrdinal("Timestamp"));
//                             attendanceRecords.Add((empID, timestamp));
//                         }

//                         await reader.CloseAsync();

//                         // Insert attendance for employees with calculated status and Timestamp
//                         foreach (var record in attendanceRecords)
//                         {
//                             var status = CalculateStatus(record.Timestamp);
//                             var currentDate = DateTime.Now.Date;

//                             // Pass both Timestamp to the InsertAttendance method
//                             await InsertAttendance(conn, record.EmpID, record.Timestamp, status);
//                         }

//                         // สำหรับพนักงานที่ขาดงาน (ไม่มี Timestamp) ให้บันทึกสถานะ "Missing"
//                         var missingEmployees = new List<int>();
//                         var cmd2 = new SqlCommand(@"
//                             SELECT EmpID
//                             FROM Transactions
//                             WHERE Timestamp IS NULL", conn);

//                         reader = await cmd2.ExecuteReaderAsync();
//                         while (await reader.ReadAsync())
//                         {
//                             missingEmployees.Add(Convert.ToInt32(reader["EmpID"]));
//                         }

//                         await reader.CloseAsync();

//                         foreach (var empID in missingEmployees)
//                         {
//                             var status = "Missing";
//                             var currentDate = DateTime.Now.Date;

//                             // For missing employees, pass null for Timestamp
//                             await InsertAttendance(conn, empID, currentDate, status);
//                         }
//                     }

//                     // รอ 10 วินาทีแล้วทำงานต่อ
//                     await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
//                 }
//                 catch (Exception ex)
//                 {
//                     // Console.WriteLine($"[CalculatedAttendanceJob] ERROR: {ex.Message}");
//                 }
//             }
//         }

//         private string CalculateStatus(DateTime timestamp)
//         {
//             string status = "unknown"; // กำหนดค่าเริ่มต้นให้กับ status
//             TimeSpan entryTimeOfDay = timestamp.TimeOfDay;

//             // เช็คกะเช้า
//             if (entryTimeOfDay < TimeSpan.FromHours(7) && entryTimeOfDay < TimeSpan.FromHours(19))  // ก่อน 07:00
//             {
//                 status = "normal";  // เข้างานก่อนเวลา
//             }
//             else if (entryTimeOfDay >= TimeSpan.FromHours(7) && entryTimeOfDay <= TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))  // 07:00 - 07:15
//             {
//                 status = "normal";  // เข้าตรงเวลา กะเช้า
//             }
//             // เข้าช้าหลัง 07:15 จะถือว่า late กะเช้า
//             else if (entryTimeOfDay > TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)) && entryTimeOfDay <= TimeSpan.FromHours(19))
//             {
//                 status = "late";  // เข้าช้า กะเช้า
//             }

//             // เช็คกะดึก
//             else if (entryTimeOfDay >= TimeSpan.FromHours(19) && entryTimeOfDay <= TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))  // 19:00 - 19:15
//             {
//                 status = "normal";  // เข้าตรงเวลา กะดึก
//             }
//             else if (entryTimeOfDay < TimeSpan.FromHours(19)) // เข้าก่อน 19:00
//             {
//                 status = "normal";  // เข้าก่อนเวลา กะดึก
//             }
//             // ถ้าเข้าหลัง 19:15 ถือว่า late
//             else if (entryTimeOfDay > TimeSpan.FromHours(19).Add(TimeSpan.FromMinutes(15)))
//             {
//                 status = "late";  // เข้าช้า กะดึก
//             }

//             return status;
//         }

//         private async Task InsertAttendance(SqlConnection conn, int empID, DateTime timestamp, string status)
//         {
//             // ตรวจสอบข้อมูลซ้ำก่อนทำการ insert
//             var checkCmd = new SqlCommand(@"
//                 SELECT COUNT(1)
//                 FROM Attendance
//                 WHERE EmpID = @empID AND Date = @date", conn);

//             checkCmd.Parameters.AddWithValue("@empID", empID);
//             checkCmd.Parameters.AddWithValue("@date", timestamp.Date);

//             var existingRecordCount = (int)await checkCmd.ExecuteScalarAsync();

//             // ถ้ามีข้อมูลแล้วให้ข้ามการ insert
//             if (existingRecordCount > 0)
//             {
//                 return; // ข้ามการ insert
//             }

//             // ถ้าสถานะเป็น "Missing" ให้ set CheckInTime เป็น NULL
//             var checkInTime = status == "Missing" ? (object)DBNull.Value : (object)timestamp;

//             var insertCmd = new SqlCommand(@"
//                 INSERT INTO Attendance (EmpID, Date, Status, CheckInTime)
//                 VALUES (@empID, @date, @status, @checkInTime)", conn);

//             insertCmd.Parameters.AddWithValue("@empID", empID);
//             insertCmd.Parameters.AddWithValue("@date", timestamp.Date);
//             insertCmd.Parameters.AddWithValue("@status", status);
//             insertCmd.Parameters.AddWithValue("@checkInTime", checkInTime);

//             await insertCmd.ExecuteNonQueryAsync();
//         }
//     }
// }

// using Microsoft.Extensions.Hosting;
// using Microsoft.Data.SqlClient;
// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using Microsoft.Extensions.Configuration;
// using Microsoft.AspNetCore.SignalR;
// using Api.Hubs;

// namespace Api.Services
// {
//     public class CalculatedAttendanceJob : BackgroundService
//     {
//         private readonly IConfiguration _configuration;
//         private readonly IHubContext<AttendanceHub> _hubContext;

//         public CalculatedAttendanceJob(
//             IConfiguration configuration,
//             IHubContext<AttendanceHub> hubContext)
//         {
//             _configuration = configuration;
//             _hubContext = hubContext;
//         }

//         protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//         {
//             while (!stoppingToken.IsCancellationRequested)
//             {
//                 try
//                 {
//                     using var conn = new SqlConnection(
//                         _configuration.GetConnectionString("DefaultConnection")
//                     );

//                     await conn.OpenAsync(stoppingToken);

//                     DateTime today = DateTime.Now.Date;

//                     var cmd = new SqlCommand(@"
//                         SELECT 
//                             EmpID,
//                             MIN(Timestamp) AS FirstScan,
//                             MAX(Timestamp) AS LastScan
//                         FROM Transactions
//                         WHERE CAST(Timestamp AS DATE) = @today
//                         GROUP BY EmpID
//                     ", conn);

//                     cmd.Parameters.AddWithValue("@today", today);

//                     using var reader = await cmd.ExecuteReaderAsync(stoppingToken);

//                     while (await reader.ReadAsync(stoppingToken))
//                     {
//                         int empID = Convert.ToInt32(reader["EmpID"]);
//                         DateTime firstScan = Convert.ToDateTime(reader["FirstScan"]);
//                         DateTime lastScan = Convert.ToDateTime(reader["LastScan"]);

//                         string status = CalculateStatus(firstScan);

//                         await UpsertAttendance(
//                             conn,
//                             empID,
//                             today,
//                             firstScan,
//                             lastScan,
//                             status
//                         );

//                         // 🔥 Push ไป frontend ทันที
//                         await _hubContext.Clients.All.SendAsync(
//                             "ReceiveAttendanceUpdate",
//                             new
//                             {
//                                 EmpID = empID,
//                                 Date = today,
//                                 Status = status,
//                                 CheckIn = firstScan,
//                                 CheckOut = lastScan
//                             }
//                         );
//                     }
//                 }
//                 catch (Exception ex)
//                 {
//                     Console.WriteLine("Realtime Engine Error: " + ex.Message);
//                 }

//                 await Task.Delay(5000, stoppingToken);
//             }
//         }

//         private string CalculateStatus(DateTime checkInTime)
//         {
//             var time = checkInTime.TimeOfDay;

//             if (time <= TimeSpan.FromHours(7).Add(TimeSpan.FromMinutes(15)))
//                 return "normal";

//             return "late";
//         }

//         private async Task UpsertAttendance(
//             SqlConnection conn,
//             int empID,
//             DateTime date,
//             DateTime checkIn,
//             DateTime checkOut,
//             string status)
//         {
//             var cmd = new SqlCommand(@"
//                 MERGE Attendance AS target
//                 USING (SELECT @empID AS EmpID, @date AS Date) AS source
//                 ON target.EmpID = source.EmpID AND target.Date = source.Date
//                 WHEN MATCHED THEN
//                     UPDATE SET 
//                         Status = @status,
//                         CheckInTime = @checkIn,
//                         CheckOutTime = @checkOut
//                 WHEN NOT MATCHED THEN
//                     INSERT (EmpID, Date, Status, CheckInTime, CheckOutTime)
//                     VALUES (@empID, @date, @status, @checkIn, @checkOut);
//             ", conn);

//             cmd.Parameters.AddWithValue("@empID", empID);
//             cmd.Parameters.AddWithValue("@date", date);
//             cmd.Parameters.AddWithValue("@status", status);
//             cmd.Parameters.AddWithValue("@checkIn", checkIn);
//             cmd.Parameters.AddWithValue("@checkOut", checkOut);

//             await cmd.ExecuteNonQueryAsync();
//         }
//     }
// }
