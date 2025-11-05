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
        public async Task<IActionResult> GetFaceEntry(
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

                // หาวันที่ล่าสุดจาก Transactions
                DateTime selectedDate;
                var latestDateQuery =
                    "SELECT TOP 1 CAST(Timestamp AS DATE) AS LatestDate FROM Transactions ORDER BY Timestamp DESC";

                // กำหนดวันที่
                // DateTime selectedDate = date?.Date ?? DateTime.Now.Date;

                using (var cmd = new SqlCommand(latestDateQuery, conn))
                {
                    var latestDateResult = await cmd.ExecuteScalarAsync();
                    if (latestDateResult == DBNull.Value || latestDateResult == null)
                    {
                        return Ok(result); // ถ้าไม่พบข้อมูลในตาราง Transactions
                    }

                    selectedDate = (DateTime)latestDateResult; // ใช้วันที่ล่าสุด
                }

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
                       AND CAST(t2.Timestamp AS DATE) = @selectedDate
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
                    // เพิ่มพารามิเตอร์
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
                                status = "status-missing";
                            }
                            else if (recordCount == 1)
                            {
                                // มี 1 record = เพิ่งเข้ามา
                                // เพิ่มเงื่อนไข: ถ้ากล้องที่บันทึกไม่ใช่ CameraID 3
                                if (lastCameraID != 3)
                                {
                                    status = "status-in-cleanroom"; // เปลี่ยนเป็น status-in-cleanroom
                                }
                                else
                                {
                                    status = "status-out-cleanroom"; // ถ้าเป็น CameraID 3
                                }
                            }
                            else
                            {
                                // มีมากกว่า 1 record = ดูจาก CameraID ล่าสุด
                                status =
                                    (lastCameraID == 1 && recordCount % 2 == 0)
                                        ? "status-get-off"
                                        : "status-in-cleanroom";
                                // ถ้ามีหลายรายการ = ดูจาก CameraID ล่าสุด
                                if (lastCameraID == 2)
                                {
                                    status = "status-out-cleanroom"; // ถ้ากล้องที่บันทึกคือ CameraID = 2
                                }
                                else if (lastCameraID == 3)
                                {
                                    status = "status-in-cleanroom"; // ถ้ากล้องที่บันทึกคือ CameraID = 3
                                }
                                else
                                {
                                    status = "status-get-off"; // ถ้ากล้องเป็นค่าอื่นๆ
                                }
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
                       AND CAST(t2.Timestamp AS DATE) = @selectedDate
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
