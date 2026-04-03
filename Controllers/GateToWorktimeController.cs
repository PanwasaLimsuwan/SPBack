// using System;
// using System.Collections.Generic;
// using System.Data.SqlClient;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;

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
//             try
//             {
//                 using (var conn = new SqlConnection(_connectionString))
//                 {
//                     await conn.OpenAsync();

//                     var selectCmd = new SqlCommand(
//                         @"
//                         SELECT EmpID, EntryDateTime, ExitDateTime
//                         FROM GateEntry
//                         WHERE EntryDateTime IS NOT NULL",
//                         conn
//                     );

//                     var reader = await selectCmd.ExecuteReaderAsync();

//                     var worktimeData =
//                         new List<(
//                             string EmpID,
//                             DateTime Date,
//                             double WorkedHours,
//                             double OTHours,
//                             string Status
//                         )>();

//                     while (await reader.ReadAsync())
//                     {
//                         var empId = reader["EmpID"].ToString();
//                         var entry = (DateTime)reader["EntryDateTime"];

//                         var exitValue = reader["ExitDateTime"];
//                         DateTime? exit =
//                             exitValue == DBNull.Value ? (DateTime?)null : (DateTime)exitValue;

//                         var workedHours = exit.HasValue ? (exit.Value - entry).TotalHours : 0;

//                         // ✅ คำนวณ OT : ถ้า worked hours > 8 ชั่วโมง
//                         var otHours = workedHours > 8 ? workedHours - 8 : 0;

//                         var status = exit == null ? "Active" : "Finished";

//                         worktimeData.Add((empId, entry.Date, workedHours, otHours, status));
//                     }

//                     await reader.CloseAsync();

//                     int inserted = 0;
//                     int updated = 0;

//                     // ✅ ดึงรายการ Worktime ที่มีอยู่แล้ว
//                     var existingRecords =
//                         new Dictionary<
//                             string,
//                             (double WorkedHours, double OTHours, string Status)
//                         >();
//                     var existingCmd = new SqlCommand(
//                         "SELECT EmpID, Date, WorkedHours, OTHours, Status FROM Worktime",
//                         conn
//                     );
//                     using (var existingReader = await existingCmd.ExecuteReaderAsync())
//                     {
//                         while (await existingReader.ReadAsync())
//                         {
//                             // var empId = existingReader["EmpID"].ToString();
//                             var empId = Convert.ToInt32(existingReader["EmpID"]);
//                             var date = Convert.ToDateTime(existingReader["Date"]).Date;
//                             var workedHours = Convert.ToDouble(existingReader["WorkedHours"]);
//                             var otHours = Convert.ToDouble(existingReader["OTHours"]);
//                             var status = existingReader["Status"].ToString();

//                             existingRecords[$"{empId}_{date:yyyyMMdd}"] = (
//                                 workedHours,
//                                 otHours,
//                                 status
//                             );
//                         }
//                     }

//                     // ✅ Insert / Update
//                     foreach (var item in worktimeData)
//                     {
//                         var key = $"{item.EmpID}_{item.Date:yyyyMMdd}";

//                         if (!existingRecords.ContainsKey(key))
//                         {
//                             var insertCmd = new SqlCommand(
//                                 @"
//                                 INSERT INTO Worktime (EmpID, Date, WorkedHours, OTHours, Status)
//                                 VALUES (@EmpID, @Date, @WorkedHours, @OTHours, @Status)",
//                                 conn
//                             );

//                             insertCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//                             insertCmd.Parameters.AddWithValue("@Date", item.Date);
//                             insertCmd.Parameters.AddWithValue("@WorkedHours", item.WorkedHours);
//                             insertCmd.Parameters.AddWithValue("@OTHours", item.OTHours);
//                             insertCmd.Parameters.AddWithValue("@Status", item.Status);

//                             await insertCmd.ExecuteNonQueryAsync();
//                             inserted++;
//                         }
//                         else
//                         {
//                             var existing = existingRecords[key];

//                             if (
//                                 existing.Status != item.Status
//                                 || Math.Abs(existing.WorkedHours - item.WorkedHours) > 0.01
//                                 || Math.Abs(existing.OTHours - item.OTHours) > 0.01
//                             )
//                             {
//                                 var updateCmd = new SqlCommand(
//                                     @"
//                                     UPDATE Worktime
//                                     SET WorkedHours = @WorkedHours, OTHours = @OTHours, Status = @Status
//                                     WHERE EmpID = @EmpID AND Date = @Date",
//                                     conn
//                                 );

//                                 updateCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//                                 updateCmd.Parameters.AddWithValue("@Date", item.Date);
//                                 updateCmd.Parameters.AddWithValue("@WorkedHours", item.WorkedHours);
//                                 updateCmd.Parameters.AddWithValue("@OTHours", item.OTHours);
//                                 updateCmd.Parameters.AddWithValue("@Status", item.Status);

//                                 await updateCmd.ExecuteNonQueryAsync();
//                                 updated++;
//                             }
//                         }
//                     }

//                     return Ok(
//                         $"Worktime inserted: {inserted} records, updated: {updated} records."
//                     );
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, $"Internal server error: {ex.Message}");
//             }
//         }
//     }
// }

// using System;
// using System.Collections.Generic;
// using System.Data.SqlClient;
// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;

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
//             try
//             {
//                 using (var conn = new SqlConnection(_connectionString))
//                 {
//                     await conn.OpenAsync();

//                     // ✅ ดึงข้อมูล Transactions แยกตามวันและพนักงาน
//                     var selectCmd = new SqlCommand(
//                         @"
//                         SELECT 
//                             EmpID,
//                             CAST(Timestamp AS DATE) AS WorkDate,
//                             Timestamp,
//                             CameraID
//                         FROM Transactions
//                         WHERE Timestamp IS NOT NULL
//                         ORDER BY EmpID, Timestamp",
//                         conn
//                     );

//                     var reader = await selectCmd.ExecuteReaderAsync();

//                     // ✅ จัดกลุ่มข้อมูลตามพนักงานและวัน
//                     var transactionsByEmployee = new Dictionary<string, List<(DateTime Timestamp, int CameraID)>>();

//                     while (await reader.ReadAsync())
//                     {
//                         var empId = Convert.ToInt32(reader["EmpID"]);
//                         var workDate = Convert.ToDateTime(reader["WorkDate"]);
//                         var timestamp = Convert.ToDateTime(reader["Timestamp"]);
//                         var cameraId = Convert.ToInt32(reader["CameraID"]);

//                         var key = $"{empId}_{workDate:yyyyMMdd}";

//                         if (!transactionsByEmployee.ContainsKey(key))
//                         {
//                             transactionsByEmployee[key] = new List<(DateTime, int)>();
//                         }

//                         transactionsByEmployee[key].Add((timestamp, cameraId));
//                     }

//                     await reader.CloseAsync();

//                     var worktimeData = new List<(
//                         int EmpID,
//                         DateTime Date,
//                         double WorkedHours,
//                         double OTHours,
//                         string Status
//                     )>();

//                     // ✅ คำนวณชั่วโมงทำงานสำหรับแต่ละวัน
//                     foreach (var kvp in transactionsByEmployee)
//                     {
//                         var parts = kvp.Key.Split('_');
//                         var empId = int.Parse(parts[0]);
//                         var workDate = DateTime.ParseExact(parts[1], "yyyyMMdd", null);
//                         var transactions = kvp.Value.OrderBy(t => t.Timestamp).ToList();

//                         // ✅ คำนวณชั่วโมงทำงานจริง (จับคู่เข้า-ออก)
//                         double totalWorkedHours = 0;
//                         DateTime? entryTime = null;
//                         string status = "Active";

//                         for (int i = 0; i < transactions.Count; i++)
//                         {
                            
//                             bool isEntry = (transactions[i].CameraID == 1 || transactions[i].CameraID == 3);
                            
//                             if (isEntry && entryTime == null)
//                             {
//                                 // บันทึกเวลาเข้า
//                                 entryTime = transactions[i].Timestamp;
//                             }
//                             else if (!isEntry && entryTime.HasValue)
//                             {
//                                 // คำนวณชั่วโมงทำงานจากเข้า-ออก
//                                 var exitTime = transactions[i].Timestamp;
//                                 totalWorkedHours += (exitTime - entryTime.Value).TotalHours;
//                                 entryTime = null;
//                             }
//                         }

//                         // ✅ ถ้ายังไม่ได้ออก (entryTime ยังมีค่า) = สถานะ Active
//                         if (entryTime.HasValue)
//                         {
//                             // คำนวณชั่วโมงจนถึงปัจจุบัน
//                             totalWorkedHours += (DateTime.Now - entryTime.Value).TotalHours;
//                             status = "Active";
//                         }
//                         else
//                         {
//                             status = "Finished";
//                         }

//                         // ✅ คำนวณ OT: ถ้าทำงานเกิน 8 ชั่วโมง
//                         var otHours = totalWorkedHours > 8 ? totalWorkedHours - 8 : 0;

//                         worktimeData.Add((empId, workDate, totalWorkedHours, otHours, status));
//                     }

//                     // ✅ ดึงข้อมูล Attendance (ถ้ามี)
//                     var attendanceCmd = new SqlCommand(
//                         @"
//                         SELECT 
//                             EmpID,
//                             CAST(Date AS DATE) AS WorkDate,
//                             Status,
//                             Hours
//                         FROM Attendance
//                         WHERE Status IN ('Leave', 'Absent', 'Late')",
//                         conn
//                     );

//                     var attendanceReader = await attendanceCmd.ExecuteReaderAsync();

//                     while (await attendanceReader.ReadAsync())
//                     {
//                         var empId = Convert.ToInt32(attendanceReader["EmpID"]);
//                         var workDate = Convert.ToDateTime(attendanceReader["WorkDate"]);
//                         var attendanceStatus = attendanceReader["Status"].ToString();
//                         var hours = attendanceReader["Hours"] != DBNull.Value 
//                             ? Convert.ToDouble(attendanceReader["Hours"]) 
//                             : 0;

//                         // ✅ เพิ่มข้อมูลจาก Attendance
//                         var key = $"{empId}_{workDate:yyyyMMdd}";
//                         var existing = worktimeData.FirstOrDefault(w => 
//                             w.EmpID == empId && w.Date == workDate);

//                         if (existing.EmpID == 0) // ถ้ายังไม่มีข้อมูลจาก Transactions
//                         {
//                             worktimeData.Add((empId, workDate, hours, 0, attendanceStatus));
//                         }
//                     }

//                     await attendanceReader.CloseAsync();

//                     // ✅ ดึงรายการ Worktime ที่มีอยู่แล้ว
//                     int inserted = 0;
//                     int updated = 0;

//                     var existingRecords = new Dictionary<string, (double WorkedHours, double OTHours, string Status)>();
//                     var existingCmd = new SqlCommand(
//                         "SELECT EmpID, Date, WorkedHours, OTHours, Status FROM Worktime",
//                         conn
//                     );
                    
//                     using (var existingReader = await existingCmd.ExecuteReaderAsync())
//                     {
//                         while (await existingReader.ReadAsync())
//                         {
//                             var empId = Convert.ToInt32(existingReader["EmpID"]);
//                             var date = Convert.ToDateTime(existingReader["Date"]).Date;
//                             var workedHours = Convert.ToDouble(existingReader["WorkedHours"]);
//                             var otHours = Convert.ToDouble(existingReader["OTHours"]);
//                             var status = existingReader["Status"].ToString();

//                             existingRecords[$"{empId}_{date:yyyyMMdd}"] = (workedHours, otHours, status);
//                         }
//                     }

//                     // ✅ Insert / Update
//                     foreach (var item in worktimeData)
//                     {
//                         var key = $"{item.EmpID}_{item.Date:yyyyMMdd}";

//                         if (!existingRecords.ContainsKey(key))
//                         {
//                             // Insert ข้อมูลใหม่
//                             var insertCmd = new SqlCommand(
//                                 @"
//                                 INSERT INTO Worktime (EmpID, Date, WorkedHours, OTHours, Status)
//                                 VALUES (@EmpID, @Date, @WorkedHours, @OTHours, @Status)",
//                                 conn
//                             );

//                             insertCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//                             insertCmd.Parameters.AddWithValue("@Date", item.Date);
//                             insertCmd.Parameters.AddWithValue("@WorkedHours", Math.Round(item.WorkedHours, 2));
//                             insertCmd.Parameters.AddWithValue("@OTHours", Math.Round(item.OTHours, 2));
//                             insertCmd.Parameters.AddWithValue("@Status", item.Status);

//                             await insertCmd.ExecuteNonQueryAsync();
//                             inserted++;
//                         }
//                         else
//                         {
//                             var existing = existingRecords[key];

//                             // Update ถ้ามีการเปลี่ยนแปลง
//                             if (
//                                 existing.Status != item.Status
//                                 || Math.Abs(existing.WorkedHours - item.WorkedHours) > 0.01
//                                 || Math.Abs(existing.OTHours - item.OTHours) > 0.01
//                             )
//                             {
//                                 var updateCmd = new SqlCommand(
//                                     @"
//                                     UPDATE Worktime
//                                     SET WorkedHours = @WorkedHours, OTHours = @OTHours, Status = @Status
//                                     WHERE EmpID = @EmpID AND Date = @Date",
//                                     conn
//                                 );

//                                 updateCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
//                                 updateCmd.Parameters.AddWithValue("@Date", item.Date);
//                                 updateCmd.Parameters.AddWithValue("@WorkedHours", Math.Round(item.WorkedHours, 2));
//                                 updateCmd.Parameters.AddWithValue("@OTHours", Math.Round(item.OTHours, 2));
//                                 updateCmd.Parameters.AddWithValue("@Status", item.Status);

//                                 await updateCmd.ExecuteNonQueryAsync();
//                                 updated++;
//                             }
//                         }
//                     }

//                     return Ok(new
//                     {
//                         message = "Worktime conversion completed from Transactions and Attendance.",
//                         inserted = inserted,
//                         updated = updated,
//                         total = worktimeData.Count,
//                         timestamp = DateTime.Now
//                     });
//                 }
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new 
//                 { 
//                     error = "Internal server error", 
//                     detail = ex.Message,
//                     stackTrace = ex.StackTrace 
//                 });
//             }
//         }
//     }
// }