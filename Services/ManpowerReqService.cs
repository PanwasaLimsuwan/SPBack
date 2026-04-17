using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class ManpowerReqService
    {
        private readonly string _connectionString;
        private readonly IHubContext<AttendanceHub> _hub;

        public ManpowerReqService(IConfiguration configuration, IHubContext<AttendanceHub> hub)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _hub = hub;
        }

        // ============================
        // 🔥 คำนวณทุกวัน (Historical)
        // รันตอน startup ครั้งเดียว
        // ============================
        public async Task RecalculateAllAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            using var transaction = conn.BeginTransaction();

            try
            {
                var query = @"
TRUNCATE TABLE ManpowerReq;

WITH AllDates AS (
    SELECT DISTINCT Date FROM Attendance WHERE CheckInTime IS NOT NULL
),
PlanShift AS (
    SELECT DISTINCT CAST(Date AS DATE) AS Date, ShiftCode FROM ManpowerPlan
),
Employees AS (
    SELECT e.EmpID, e.Biz, e.Process, e.ShiftCode FROM EmployeeInfo e
),
EmployeeSkill AS (
    SELECT EmpID, SkillGroup FROM OJTandInspectionSkill WHERE Active >= 2
),
SkillBreakdown AS (
    SELECT 
        ad.Date,
        e.Biz, e.Process, s.SkillGroup,
        COUNT(DISTINCT e.EmpID) AS Require,
        COUNT(DISTINCT CASE WHEN pe.EmpID IS NOT NULL THEN e.EmpID END) AS Present
    FROM AllDates ad
    JOIN PlanShift ps ON ps.Date = ad.Date
    JOIN Employees e ON LTRIM(RTRIM(e.ShiftCode)) = LTRIM(RTRIM(ps.ShiftCode))
    JOIN EmployeeSkill s ON s.EmpID = e.EmpID
    LEFT JOIN Attendance pe 
        ON pe.EmpID = e.EmpID 
        AND pe.Date = ad.Date
        AND pe.CheckInTime IS NOT NULL
    GROUP BY ad.Date, e.Biz, e.Process, s.SkillGroup
)
INSERT INTO ManpowerReq (Date, Biz, Process, SkillGroup, Require, Present, Shortage, LastUpdateTime)
SELECT Date, Biz, Process, SkillGroup, Require, Present,
    Require - Present AS Shortage, GETDATE()
FROM SkillBreakdown
WHERE Require > 0;
";
                using var cmd = new SqlCommand(query, conn, transaction);
                cmd.CommandTimeout = 600; // 🔥 10 นาที เพราะข้อมูลเยอะ
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();

                Console.WriteLine("[ManpowerReq] ✅ RecalculateAll done");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            await _hub.Clients.All.SendAsync("ManpowerUpdated");
        }

        // ============================
        // 🔥 คำนวณแค่วันล่าสุด (Real-time)
        // รันทุก 5 นาทีใน Job
        // ============================
        public async Task RecalculateTodayAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"
DECLARE @WorkDate DATE = CASE
    WHEN CAST(GETDATE() AS TIME) < '07:00:00'
    THEN CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
    ELSE CAST(GETDATE() AS DATE)
END;

-- ✅ แก้เป็น — คำนวณเฉพาะ ShiftCode ที่ active ตอนนี้
WITH ActiveShift AS (
    SELECT CASE
        WHEN CAST(GETDATE() AS TIME) >= '07:00:00' 
             AND CAST(GETDATE() AS TIME) < '19:00:00' THEN 'DAY'
        ELSE 'NIGHT'
    END AS CurrentShift
),
PlanShift AS (
    SELECT DISTINCT CAST(Date AS DATE) AS Date, ShiftCode 
    FROM ManpowerPlan 
    WHERE CAST(Date AS DATE) = @WorkDate
      AND Shift = (SELECT CurrentShift FROM ActiveShift)  -- ✅ filter เฉพาะกะนี้
),
Employees AS (
    SELECT e.EmpID, e.Biz, e.Process, e.ShiftCode FROM EmployeeInfo e
),
EmployeeSkill AS (
    SELECT EmpID, SkillGroup FROM OJTandInspectionSkill WHERE Active >= 2
),
SkillBreakdown AS (
    SELECT 
        @WorkDate AS Date,
        e.Biz, e.Process, s.SkillGroup,
        COUNT(DISTINCT e.EmpID) AS Require,
        COUNT(DISTINCT CASE WHEN pe.EmpID IS NOT NULL THEN e.EmpID END) AS Present
    FROM PlanShift ps
    JOIN Employees e ON LTRIM(RTRIM(e.ShiftCode)) = LTRIM(RTRIM(ps.ShiftCode))
    JOIN EmployeeSkill s ON s.EmpID = e.EmpID
    LEFT JOIN Attendance pe 
    ON pe.EmpID = e.EmpID 
    AND pe.Date = @WorkDate
    AND pe.Status != 'Absent'  -- มาทำงาน = ไม่ขาด
    GROUP BY e.Biz, e.Process, s.SkillGroup
)
MERGE ManpowerReq AS target
USING SkillBreakdown AS source
ON target.Date       = source.Date
   AND target.Biz        = source.Biz
   AND target.Process    = source.Process
   AND target.SkillGroup = source.SkillGroup

WHEN MATCHED THEN
    UPDATE SET
        Require        = source.Require,
        Present        = source.Present,
        Shortage       = source.Require - source.Present,
        LastUpdateTime = GETDATE()

WHEN NOT MATCHED THEN
    INSERT (Date, Biz, Process, SkillGroup, Require, Present, Shortage, LastUpdateTime)
    VALUES (
        source.Date, source.Biz, source.Process, source.SkillGroup,
        source.Require, source.Present,
        source.Require - source.Present,
        GETDATE()
    );
";
            using var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 300;
            await cmd.ExecuteNonQueryAsync();

            Console.WriteLine("[ManpowerReq] ✅ RecalculateToday done");
            await _hub.Clients.All.SendAsync("ManpowerUpdated");
        }
    }
}