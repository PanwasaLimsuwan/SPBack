using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FaceEntryController : ControllerBase // 🔥 เปลี่ยนชื่อ
    {
        private readonly IConfiguration _configuration;

        public FaceEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetFaceEntry(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? date,
            [FromQuery] string? shiftOverride
        )
        {
            var result = new List<object>();

            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            await conn.OpenAsync();

            DateTime now = DateTime.Now;
            DateTime workDate = date.HasValue
                ? date.Value.Date
                : (now.Hour < 7 ? now.Date.AddDays(-1) : now.Date);

            // ✅ ขยาย window ให้กว้างขึ้น ครอบ early swipe
            DateTime dayStart,
                dayEnd;
            if (shiftOverride == "NIGHT")
            {
                dayStart = workDate.AddHours(18); // เผื่อ 1 ชม.
                dayEnd = workDate.AddDays(1).AddHours(8);
            }
            else
            {
                dayStart = workDate.AddHours(6); // ✅ เผื่อจาก 06:00
                dayEnd = workDate.AddHours(20); // ✅ เผื่อถึง 20:00
            }

            var query =
                @"
        WITH LatestTrans AS (
            SELECT 
                t.EmpID,
                t.CameraID,
                t.Timestamp,
                ROW_NUMBER() OVER (
                    PARTITION BY t.EmpID
                    ORDER BY t.Timestamp DESC
                ) AS rn
            FROM Transactions t
            WHERE t.Timestamp BETWEEN @dayStart AND @dayEnd
        )
        SELECT 
            e.EmpID,
            e.FirstName,
            e.LastName,
            e.Division,
            e.Department,
            e.Section,
            e.Biz,
            e.Process,
            lt.CameraID,
            lt.Timestamp AS LastSeen,
            (SELECT MIN(t2.Timestamp) 
             FROM Transactions t2 
             WHERE t2.EmpID = e.EmpID 
               AND t2.CameraID = 1
               AND t2.Timestamp BETWEEN @dayStart AND @dayEnd) AS CheckInTime,
            (SELECT MAX(t2.Timestamp) 
             FROM Transactions t2 
             WHERE t2.EmpID = e.EmpID 
               AND t2.CameraID = 1
               AND t2.Timestamp BETWEEN @dayStart AND @dayEnd) AS CheckOutTime
        FROM EmployeeInfo e
        JOIN ManpowerPlan mp
            ON LTRIM(RTRIM(mp.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
            AND CAST(mp.Date AS DATE) = @workDate
            -- ✅ ถ้า shiftOverride เป็น NULL ให้แสดงทุก shift
            AND (@shiftOverride IS NULL OR LTRIM(RTRIM(mp.Shift)) = LTRIM(RTRIM(@shiftOverride)))
        LEFT JOIN LatestTrans lt 
            ON e.EmpID = lt.EmpID AND lt.rn = 1
        WHERE 1=1
        AND (@division   IS NULL OR e.Division   = @division)
        AND (@department IS NULL OR e.Department = @department)
        AND (@section    IS NULL OR e.Section    = @section)
        AND (@biz        IS NULL OR e.Biz        = @biz)
        AND (@process    IS NULL OR e.Process    = @process)
    ";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@shiftOverride", (object?)shiftOverride ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@workDate", workDate);
            cmd.Parameters.AddWithValue("@dayStart", dayStart);
            cmd.Parameters.AddWithValue("@dayEnd", dayEnd);
            cmd.CommandTimeout = 60;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int? cameraID = reader["CameraID"] as int?;
                object? checkInObj = reader["CheckInTime"];
                object? checkOutObj = reader["CheckOutTime"];

                DateTime? checkIn = checkInObj == DBNull.Value ? null : (DateTime?)checkInObj;
                DateTime? checkOut = checkOutObj == DBNull.Value ? null : (DateTime?)checkOutObj;

                string status;
                if (cameraID == null)
                    status = "status-missing";
                else if (cameraID == 3)
                    status = "status-in-cleanroom";
                else if (cameraID == 2)
                    status = "status-out-cleanroom";
                else if (cameraID == 1)
                    // ✅ get-off = รูดออกแล้ว (checkIn และ checkOut ต่างกัน)
                    status =
                        (checkIn.HasValue && checkOut.HasValue && checkIn != checkOut)
                            ? "status-get-off"
                            : "status-out-cleanroom";
                else
                    status = "status-missing";

                result.Add(
                    new
                    {
                        workDate = workDate.ToString("yyyy-MM-dd"),
                        empID = reader["EmpID"],
                        firstName = reader["FirstName"]?.ToString(),
                        lastName = reader["LastName"]?.ToString(),
                        division = reader["Division"]?.ToString(),
                        department = reader["Department"]?.ToString(),
                        section = reader["Section"]?.ToString(),
                        biz = reader["Biz"]?.ToString(),
                        process = reader["Process"]?.ToString(),
                        cameraID,
                        status,
                        entryDateTime = checkIn?.ToString("yyyy-MM-dd HH:mm:ss"),
                        exitDateTime = status == "status-get-off"
                            ? checkOut?.ToString("yyyy-MM-dd HH:mm:ss")
                            : null,
                    }
                );
            }

            return Ok(new { workDate = workDate.ToString("yyyy-MM-dd"), data = result });
        }

        [HttpGet("latest")]
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

            using var conn = new SqlConnection(
                _configuration.GetConnectionString("DefaultConnection")
            );
            await conn.OpenAsync();

            // ✅ Step 1: หาวันล่าสุดจาก Transactions
            using var dateCmd = new SqlCommand(
                @"
        SELECT CAST(MAX(Timestamp) AS DATE) 
        FROM Transactions 
        WHERE Timestamp IS NOT NULL
    ",
                conn
            );

            var latestDateObj = await dateCmd.ExecuteScalarAsync();

            if (latestDateObj == null || latestDateObj == DBNull.Value)
                return Ok(new { workDate = (string?)null, data = result });

            DateTime workDate = Convert.ToDateTime(latestDateObj).Date;

            // ✅ Step 2: กำหนด time window ตาม shift
            DateTime dayStart,
                dayEnd;
            if (shiftOverride == "NIGHT")
            {
                dayStart = workDate.AddHours(18);
                dayEnd = workDate.AddDays(1).AddHours(8);
            }
            else
            {
                dayStart = workDate.AddHours(6);
                dayEnd = workDate.AddHours(20);
            }

            // ✅ Step 3: Query หลัก
            var query =
                @"
        WITH LatestTrans AS (
            SELECT 
                t.EmpID,
                t.CameraID,
                t.Timestamp,
                ROW_NUMBER() OVER (
                    PARTITION BY t.EmpID
                    ORDER BY t.Timestamp DESC
                ) AS rn
            FROM Transactions t
            WHERE t.Timestamp BETWEEN @dayStart AND @dayEnd
        )
        SELECT 
            e.EmpID,
            e.FirstName,
            e.LastName,
            e.Division,
            e.Department,
            e.Section,
            e.Biz,
            e.Process,
            lt.CameraID,
            lt.Timestamp AS LastSeen,
            (SELECT MIN(t2.Timestamp) 
             FROM Transactions t2 
             WHERE t2.EmpID = e.EmpID 
               AND t2.CameraID = 1
               AND t2.Timestamp BETWEEN @dayStart AND @dayEnd) AS CheckInTime,
            (SELECT MAX(t2.Timestamp) 
             FROM Transactions t2 
             WHERE t2.EmpID = e.EmpID 
               AND t2.CameraID = 1
               AND t2.Timestamp BETWEEN @dayStart AND @dayEnd) AS CheckOutTime
        FROM EmployeeInfo e
        JOIN ManpowerPlan mp
            ON LTRIM(RTRIM(mp.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
            AND CAST(mp.Date AS DATE) = @workDate
            AND (@shiftOverride IS NULL OR LTRIM(RTRIM(mp.Shift)) = LTRIM(RTRIM(@shiftOverride)))
        LEFT JOIN LatestTrans lt 
            ON e.EmpID = lt.EmpID AND lt.rn = 1
        WHERE 1=1
        AND (@division   IS NULL OR e.Division   = @division)
        AND (@department IS NULL OR e.Department = @department)
        AND (@section    IS NULL OR e.Section    = @section)
        AND (@biz        IS NULL OR e.Biz        = @biz)
        AND (@process    IS NULL OR e.Process    = @process)
    ";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@shiftOverride", (object?)shiftOverride ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@workDate", workDate);
            cmd.Parameters.AddWithValue("@dayStart", dayStart);
            cmd.Parameters.AddWithValue("@dayEnd", dayEnd);
            cmd.CommandTimeout = 60;

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                int? cameraID = reader["CameraID"] as int?;
                object? checkInObj = reader["CheckInTime"];
                object? checkOutObj = reader["CheckOutTime"];

                DateTime? checkIn = checkInObj == DBNull.Value ? null : (DateTime?)checkInObj;
                DateTime? checkOut = checkOutObj == DBNull.Value ? null : (DateTime?)checkOutObj;

                string status;
                if (cameraID == null)
                    status = "status-missing";
                else if (cameraID == 3)
                    status = "status-in-cleanroom";
                else if (cameraID == 2)
                    status = "status-out-cleanroom";
                else if (cameraID == 1)
                    status =
                        (checkIn.HasValue && checkOut.HasValue && checkIn != checkOut)
                            ? "status-get-off"
                            : "status-out-cleanroom";
                else
                    status = "status-missing";

                result.Add(
                    new
                    {
                        workDate = workDate.ToString("yyyy-MM-dd"),
                        empID = reader["EmpID"],
                        firstName = reader["FirstName"]?.ToString(),
                        lastName = reader["LastName"]?.ToString(),
                        division = reader["Division"]?.ToString(),
                        department = reader["Department"]?.ToString(),
                        section = reader["Section"]?.ToString(),
                        biz = reader["Biz"]?.ToString(),
                        process = reader["Process"]?.ToString(),
                        cameraID,
                        status,
                        entryDateTime = checkIn?.ToString("yyyy-MM-dd HH:mm:ss"),
                        exitDateTime = status == "status-get-off"
                            ? checkOut?.ToString("yyyy-MM-dd HH:mm:ss")
                            : null,
                    }
                );
            }

            return Ok(new { workDate = workDate.ToString("yyyy-MM-dd"), data = result });
        }
    }
}
