using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;

        public TransactionsController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
        }

        // GET: api/Transactions
        [HttpGet]
        public async Task<IActionResult> GetTransaction()
        {
            var transactions = await _context.Transactions.ToListAsync();
            return Ok(transactions);
        }

        // GET: api/Transactions/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTransaction(int id)
        {
            var transaction = await _context.Transactions.FindAsync(id);
            if (transaction == null)
            {
                return NotFound();
            }

            return Ok(transaction);
        }

        // GET: api/Transactions/GetFaceEntry (รวมคนขาดงานด้วย)
        [HttpGet("GetFaceEntry")]
        public async Task<IActionResult> GetFaceEntryLatest(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] string? shiftOverride
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                // ✅ Step 1: หา workDate และ activeShift จากเวลาปัจจุบัน
                //             DateTime now = DateTime.Now;
                //             DateTime selectedDate;
                //             string selectedShift;
                //             string shiftType;

                //             // กำหนด active shift จากเวลาปัจจุบัน
                //             // กะ A (DAY)   = 07:00 - 19:00
                //             // กะ B/C (NIGHT) = 19:00 - 07:00 ของวันถัดไป
                //             bool isDayShift = now.Hour >= 7 && now.Hour < 19;

                //             if (isDayShift)
                //             {
                //                 // กะกลางวัน
                //                 selectedDate = now.Date;
                //                 shiftType = "DAY";

                //                 // หา ShiftCode ของกะกลางวันจาก EmployeeInfo
                //                 // ที่มี Transaction วันนี้มากที่สุดในช่วง 07:00-19:00
                //                 using (
                //                     var cmd = new SqlCommand(
                //                         @"
                //     SELECT TOP 1 e.ShiftCode
                //     FROM Transactions t
                //     JOIN EmployeeInfo e ON t.EmpID = e.EmpID
                //     WHERE t.Timestamp BETWEEN @start AND @end
                //     GROUP BY e.ShiftCode
                //     ORDER BY COUNT(*) DESC
                // ",
                //                         conn
                //                     )
                //                 )
                //                 {
                //                     cmd.Parameters.AddWithValue("@start", now.Date.AddHours(7));
                //                     cmd.Parameters.AddWithValue("@end", now.Date.AddHours(19));
                //                     var r = await cmd.ExecuteScalarAsync();
                //                     selectedShift = r?.ToString() ?? "A";
                //                 }
                //             }
                //             else
                //             {
                //                 // กะดึก — workDate คือวันที่กะเริ่ม
                //                 // ถ้าตอนนี้ 19:00-23:59 → กะเริ่มวันนี้
                //                 // ถ้าตอนนี้ 00:00-06:59 → กะเริ่มเมื่อวาน
                //                 selectedDate = now.Hour >= 19 ? now.Date : now.Date.AddDays(-1);
                //                 shiftType = "NIGHT";

                //                 DateTime nightStart = selectedDate.AddHours(19);
                //                 DateTime nightEnd = selectedDate.AddDays(1).AddHours(7);

                //                 using (
                //                     var cmd = new SqlCommand(
                //                         @"
                //     SELECT TOP 1 e.ShiftCode
                //     FROM Transactions t
                //     JOIN EmployeeInfo e ON t.EmpID = e.EmpID
                //     WHERE t.Timestamp BETWEEN @start AND @end
                //     GROUP BY e.ShiftCode
                //     ORDER BY COUNT(*) DESC
                // ",
                //                         conn
                //                     )
                //                 )
                //                 {
                //                     cmd.Parameters.AddWithValue("@start", nightStart);
                //                     cmd.Parameters.AddWithValue("@end", nightEnd);
                //                     var r = await cmd.ExecuteScalarAsync();
                //                     selectedShift = r?.ToString() ?? "C";
                //                 }
                //             }

                //             // ✅ ถ้าส่ง shiftOverride มา ให้ override ทับ
                //             if (!string.IsNullOrEmpty(shiftOverride))
                //             {
                //                 selectedShift = shiftOverride;
                //                 // หา shiftType จาก ManpowerPlan
                //                 using (
                //                     var cmd = new SqlCommand(
                //                         @"
                //     SELECT TOP 1 Shift
                //     FROM ManpowerPlan
                //     WHERE CAST([Date] AS DATE) = @date AND ShiftCode = @code
                // ",
                //                         conn
                //                     )
                //                 )
                //                 {
                //                     cmd.Parameters.AddWithValue("@date", selectedDate);
                //                     cmd.Parameters.AddWithValue("@code", selectedShift);
                //                     var r = await cmd.ExecuteScalarAsync();
                //                     shiftType = r?.ToString() ?? shiftType;
                //                 }
                //             }

                //             // ✅ Step 2 (เดิม): คำนวณ time window — ใช้ shiftType ที่ได้มาแล้ว
                //             DateTime startTime,
                //                 endTime;
                //             if (shiftType == "DAY")
                //             {
                //                 startTime = selectedDate.AddHours(7);
                //                 endTime = selectedDate.AddHours(19);
                //             }
                //             else
                //             {
                //                 startTime = selectedDate.AddHours(19);
                //                 endTime = selectedDate.AddDays(1).AddHours(7);
                //             }

                // ✅ Step 1: หา Timestamp ล่าสุดจาก Transactions
                DateTime now = DateTime.Now;

                DateTime selectedDate;
                string shiftType;
                string selectedShift;

                // ✅ STEP 1: หา shiftType + workDate
                // if (now.Hour >= 7 && now.Hour < 19)
                // {
                //     selectedDate = now.Date;
                //     shiftType = "DAY";
                // }
                // else if (now.Hour >= 19)
                // {
                //     selectedDate = now.Date;
                //     shiftType = "NIGHT";
                // }
                // else
                // {
                //     selectedDate = now.Date.AddDays(-1);
                //     shiftType = "NIGHT";
                // }

                // ✅ ใหม่ — delay 15 นาที ก่อนเปลี่ยน shift
                // DAY  = 07:15 - 19:14
                // NIGHT = 19:15 - 07:14 ของวันถัดไป

                TimeSpan nowTime = now.TimeOfDay;
                TimeSpan dayStart = new TimeSpan(7, 15, 0); // 07:15
                TimeSpan nightStart = new TimeSpan(19, 15, 0); // 19:15

                if (nowTime >= dayStart && nowTime < nightStart)
                {
                    selectedDate = now.Date;
                    shiftType = "DAY";
                }
                else if (nowTime >= nightStart)
                {
                    selectedDate = now.Date;
                    shiftType = "NIGHT";
                }
                else // 00:00 - 07:14 → กะดึกที่เริ่มเมื่อวาน
                {
                    selectedDate = now.Date.AddDays(-1);
                    shiftType = "NIGHT";
                }

                // ✅ STEP 2: หา ShiftCode จาก ManpowerPlan (ทำครั้งเดียว!)
                using (
                    var cmd = new SqlCommand(
                        @"
    SELECT TOP 1 ShiftCode 
    FROM ManpowerPlan
    WHERE CAST(Date AS DATE) = @date
      AND Shift = @shift
",
                        conn
                    )
                )
                {
                    cmd.Parameters.AddWithValue("@date", selectedDate);
                    cmd.Parameters.AddWithValue("@shift", shiftType);

                    var r = await cmd.ExecuteScalarAsync();
                    selectedShift = r?.ToString() ?? "A";
                }

                // ✅ STEP 3: override (ถ้ามี)
                if (!string.IsNullOrEmpty(shiftOverride))
                {
                    selectedShift = shiftOverride;
                }

                // ✅ STEP 4: คำนวณเวลา (ห้ามลืม!!)
                DateTime startTime,
                    endTime;

                if (shiftType == "DAY")
                {
                    startTime = selectedDate.AddHours(7);
                    endTime = selectedDate.AddHours(19);
                }
                else
                {
                    startTime = selectedDate.AddHours(19);
                    endTime = selectedDate.AddDays(1).AddHours(7);
                }
                // ✅ Step 3: คำนวณ time window
                // DateTime startTime,
                //     endTime;
                // if (shiftType == "DAY")
                // {
                //     startTime = selectedDate.AddHours(7);
                //     endTime = selectedDate.AddHours(19);
                // }
                // else // NIGHT
                // {
                //     startTime = selectedDate.AddHours(19);
                //     endTime = selectedDate.AddDays(1).AddHours(7);
                // }

                // ✅ Step 4: Query พนักงาน
                var query =
                    @"
            SELECT 
                e.EmpID,
                e.FirstName, 
                e.LastName,
                e.Division,
                e.Department,
                e.Section,
                e.Biz,
                e.Process,
                MIN(t.Timestamp) AS EntryDateTime,
                MAX(t.Timestamp) AS ExitDateTime,
                COUNT(t.TransacID) AS RecordCount,
                (
                    SELECT TOP 1 t2.CameraID 
                    FROM Transactions t2 
                    WHERE t2.EmpID = e.EmpID 
                      AND t2.Timestamp BETWEEN @startTime AND @endTime
                    ORDER BY t2.Timestamp DESC
                ) AS LastCameraID
            FROM EmployeeInfo e
            LEFT JOIN Transactions t 
                ON e.EmpID = t.EmpID 
                AND t.Timestamp BETWEEN @startTime AND @endTime
            WHERE e.ShiftCode = @shift
                AND (@division   IS NULL OR e.Division   = @division)
                AND (@department IS NULL OR e.Department = @department)
                AND (@section    IS NULL OR e.Section    = @section)
                AND (@biz        IS NULL OR COALESCE(e.Biz, '')     = @biz)
                AND (@process    IS NULL OR COALESCE(e.Process, '') = @process)
            GROUP BY e.EmpID, e.FirstName, e.LastName,
                     e.Division, e.Department, e.Section, e.Biz, e.Process
            ORDER BY e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@startTime", startTime);
                    cmd.Parameters.AddWithValue("@endTime", endTime);
                    cmd.Parameters.AddWithValue("@shift", selectedShift);
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var recordCount = Convert.ToInt32(reader["RecordCount"]);
                            var lastCameraID =
                                reader["LastCameraID"] != DBNull.Value
                                    ? Convert.ToInt32(reader["LastCameraID"])
                                    : 0;

                            var entryDateTime =
                                reader["EntryDateTime"] == DBNull.Value
                                    ? null
                                    : ((DateTime)reader["EntryDateTime"]).ToString(
                                        "yyyy-MM-dd HH:mm:ss"
                                    );

                            DateTime? exitTime =
                                reader["ExitDateTime"] == DBNull.Value
                                    ? null
                                    : (DateTime?)reader["ExitDateTime"];

                            var exitDateTime =
                                recordCount > 1 && exitTime.HasValue
                                    ? exitTime.Value.ToString("yyyy-MM-dd HH:mm:ss")
                                    : null;

                            // ✅ Status logic
                            string status;
                            if (recordCount == 0)
                                status = "status-missing";
                            else if (lastCameraID == 3)
                                status = "status-in-cleanroom";
                            else if (lastCameraID == 2)
                                status = "status-out-cleanroom";
                            else if (lastCameraID == 1)
                                status = "status-get-off";
                            else
                                status = "status-missing";

                            result.Add(
                                new
                                {
                                    empID = reader["EmpID"],
                                    firstName = reader["FirstName"]?.ToString(),
                                    lastName = reader["LastName"]?.ToString(),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                    shift = selectedShift,
                                    entryDateTime,
                                    exitDateTime,
                                    recordCount,
                                    lastCameraID,
                                    status,
                                }
                            );
                        }
                    }
                }

                return Ok(
                    new
                    {
                        workDate = selectedDate.ToString("yyyy-MM-dd"), // ✅ ชื่อตรงกับ Frontend
                        selectedDate = selectedDate.ToString("yyyy-MM-dd"), // ✅ คงไว้กัน StatusTabMFG พัง
                        shift = selectedShift,
                        shiftType,
                        currentTime = DateTime.Now.ToString("HH:mm:ss"),
                        data = result,
                    }
                );
            }
        }

        // GET: api/Transactions/GetTransactions (รวมคนขาดงานด้วย)
        [HttpGet("GetTransactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? date
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                DateTime selectedDate;

                // ✅ ถ้าไม่ส่ง date มา ให้หาวันที่ล่าสุดในตาราง Transactions
                if (!date.HasValue)
                {
                    var latestDateQuery =
                        "SELECT TOP 1 CAST(Timestamp AS DATE) AS LatestDate FROM Transactions ORDER BY Timestamp DESC";
                    using (var latestCmd = new SqlCommand(latestDateQuery, conn))
                    {
                        var latestDateResult = await latestCmd.ExecuteScalarAsync();

                        if (latestDateResult == null || latestDateResult == DBNull.Value)
                        {
                            selectedDate = DateTime.Now.Date;
                        }
                        else
                        {
                            selectedDate = Convert.ToDateTime(latestDateResult);
                        }
                    }
                }
                else
                {
                    selectedDate = date.Value.Date;
                }

                // Console.WriteLine($"Selected Date: {selectedDate:yyyy-MM-dd}");

                // 🎯 Query ใหม่: LEFT JOIN จาก EmployeeInfo เพื่อแสดงทั้งคนมาและคนไม่มา
                var query =
                    @"
                SELECT 
                    e.EmpID,
                    e.FirstName, 
                    e.LastName, 
                    e.Division, 
                    e.Department, 
                    e.Position, 
                    e.Email, 
                    e.ShiftCode, 
                    e.Section,
                    e.Biz, 
                    e.Process,
                    MIN(t.Timestamp) AS EntryDateTime,
                    MAX(t.Timestamp) AS ExitDateTime,
                    COUNT(t.TransacID) AS RecordCount,
                    (SELECT TOP 1 t2.CameraID 
                     FROM Transactions t2 
                     WHERE t2.EmpID = e.EmpID 
                       AND (
    (
        e.ShiftCode = 'A'
        AND t.Timestamp BETWEEN 
            DATEADD(HOUR,-1, DATEADD(HOUR,7,@selectedDate))
            AND DATEADD(HOUR,19,@selectedDate)
    )
    OR
    (
        e.ShiftCode IN ('B','C')
        AND t.Timestamp BETWEEN 
            DATEADD(HOUR,-1, DATEADD(HOUR,19,DATEADD(DAY,-1,@selectedDate)))
            AND DATEADD(HOUR,7,@selectedDate)
    )
)
                     ORDER BY t2.Timestamp DESC) AS LastCameraID
                FROM EmployeeInfo e
                LEFT JOIN Transactions t ON e.EmpID = t.EmpID 
                    AND CAST(t.Timestamp AS DATE) = @selectedDate
                WHERE (@division IS NULL OR e.Division = @division)
                    AND (@department IS NULL OR e.Department = @department)
                    AND (@section IS NULL OR e.Section = @section)
                    AND (@biz IS NULL OR COALESCE(e.Biz, '') = @biz)
                    AND (@process IS NULL OR COALESCE(e.Process, '') = @process)
                GROUP BY 
                    e.EmpID, e.FirstName, e.LastName, e.Division, e.Department,
                    e.Position, e.Email, e.ShiftCode, e.Section, e.Biz, e.Process
                ORDER BY e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@selectedDate", selectedDate);
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var empID = Convert.ToInt32(reader["EmpID"]);
                            var recordCount = Convert.ToInt32(reader["RecordCount"]);
                            var lastCameraID =
                                reader["LastCameraID"] != DBNull.Value
                                    ? Convert.ToInt32(reader["LastCameraID"])
                                    : 0;

                            var entryDateTime =
                                reader["EntryDateTime"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["EntryDateTime"];

                            var exitDateTime =
                                recordCount > 1 && reader["ExitDateTime"] != DBNull.Value
                                    ? (DateTime?)reader["ExitDateTime"]
                                    : null;

                            // 🎯 กำหนด status
                            string status;
                            if (recordCount == 0 || !entryDateTime.HasValue)
                            {
                                // ไม่มี transaction เลย = ขาดงาน
                                status = "status-absent";
                            }
                            else if (recordCount == 1)
                            {
                                // มี 1 record = เพิ่งเข้ามา
                                status = "status-in";
                            }
                            else
                            {
                                // มีมากกว่า 1 record = ดูจาก CameraID ล่าสุด
                                status =
                                    (lastCameraID == 1 && recordCount % 2 == 0)
                                        ? "status-out"
                                        : "status-in";
                            }

                            result.Add(
                                new
                                {
                                    empID = empID,
                                    firstName = reader["FirstName"]?.ToString(),
                                    lastName = reader["LastName"]?.ToString(),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    position = reader["Position"]?.ToString(),
                                    email = reader["Email"]?.ToString(),
                                    shiftCode = reader["ShiftCode"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                    entryDateTime = entryDateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    exitDateTime = exitDateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    recordCount = recordCount,
                                    lastCameraID = lastCameraID,
                                    status = status,
                                }
                            );
                        }
                    }
                }
            }

            return Ok(result);
        }

        // 🆕 GET: api/Transactions/GetAbsentEmployees - ดึงรายชื่อพนักงานที่ไม่มาทำงาน
        [HttpGet("GetAbsentEmployees")]
        public async Task<IActionResult> GetAbsentEmployees(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? date
        )
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                DateTime selectedDate = date?.Date ?? DateTime.Now.Date;

                // Query หาพนักงานที่ไม่มี transaction ในวันที่เลือก
                var query =
                    @"
                SELECT 
                    e.EmpID,
                    e.FirstName, 
                    e.LastName, 
                    e.Division, 
                    e.Department, 
                    e.Position, 
                    e.Email, 
                    e.ShiftCode, 
                    e.Section,
                    e.Biz, 
                    e.Process
                FROM EmployeeInfo e
                WHERE NOT EXISTS (
                    SELECT 1 
                    FROM Transactions t 
                    WHERE t.EmpID = e.EmpID 
                      AND CAST(t.Timestamp AS DATE) = @selectedDate
                )
                AND (@division IS NULL OR e.Division = @division)
                AND (@department IS NULL OR e.Department = @department)
                AND (@section IS NULL OR e.Section = @section)
                AND (@biz IS NULL OR COALESCE(e.Biz, '') = @biz)
                AND (@process IS NULL OR COALESCE(e.Process, '') = @process)
                ORDER BY e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@selectedDate", selectedDate);
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            result.Add(
                                new
                                {
                                    empID = Convert.ToInt32(reader["EmpID"]),
                                    firstName = reader["FirstName"]?.ToString(),
                                    lastName = reader["LastName"]?.ToString(),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    position = reader["Position"]?.ToString(),
                                    email = reader["Email"]?.ToString(),
                                    shiftCode = reader["ShiftCode"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                    status = "status-absent",
                                }
                            );
                        }
                    }
                }
            }

            return Ok(result);
        }
    }
}
