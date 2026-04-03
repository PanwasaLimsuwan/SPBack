using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class OTService
    {
        private readonly string _connectionString;
        private readonly IHubContext<NotificationHub> _hub;

        public OTService(IConfiguration configuration, IHubContext<NotificationHub> hub)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _hub = hub;
        }

        public async Task ProcessAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 🔥 STEP 1: สร้าง Index ถ้ายังไม่มี (รันครั้งแรกครั้งเดียว)
            var indexCmd = new SqlCommand(
                @"
        IF NOT EXISTS (
            SELECT 1 FROM sys.indexes 
            WHERE name = 'IX_Worktime_EmpID_Date' 
            AND object_id = OBJECT_ID('Worktime')
        )
        CREATE INDEX IX_Worktime_EmpID_Date ON Worktime (EmpID, Date)
        INCLUDE (WorkedHours, OTHours);
    ",
                conn
            );
            indexCmd.CommandTimeout = 120;
            await indexCmd.ExecuteNonQueryAsync();

            // 🔥 STEP 2: MERGE ปกติ
            var cmd = new SqlCommand(
                @"
MERGE EICC_Control AS target
USING (
    SELECT 
        EmpID,
        DATEPART(YEAR, Date)  AS Year,
        DATEPART(WEEK, Date)  AS WeekID,
        SUM(WorkedHours)      AS TotalHours,
        SUM(OTHours)          AS TotalOT,
        COUNT(DISTINCT Date)  AS DaysWorked,
        -- 🔥 คำนวณ WeekStart/End ใน source
        DATEADD(DAY, 1 - DATEPART(WEEKDAY, MIN(Date)), MIN(Date)) AS WeekStartDate,
        DATEADD(DAY, 7 - DATEPART(WEEKDAY, MIN(Date)), MIN(Date)) AS WeekEndDate
    FROM Worktime WITH (INDEX(IX_Worktime_EmpID_Date))
    GROUP BY 
        EmpID,
        DATEPART(YEAR, Date),
        DATEPART(WEEK, Date)
) AS source

ON target.EmpID   = source.EmpID
   AND target.WeekID = source.WeekID
   AND target.Year   = source.Year

WHEN MATCHED THEN
    UPDATE SET 
        TotalHours    = source.TotalHours,
        DaysWorked    = source.DaysWorked,
        TotalOT       = source.TotalOT,
        WeekStartDate = source.WeekStartDate,  -- 🔥 เพิ่ม
        WeekEndDate   = source.WeekEndDate,    -- 🔥 เพิ่ม
        Status        = 'Complete'

WHEN NOT MATCHED THEN
    INSERT (EmpID, WeekID, Year, TotalHours, DaysWorked, TotalOT, WeekStartDate, WeekEndDate, Status)
    VALUES (source.EmpID, source.WeekID, source.Year,
            source.TotalHours, source.DaysWorked, source.TotalOT,
            source.WeekStartDate, source.WeekEndDate,  -- 🔥 เพิ่ม
            'Complete');
",
                conn
            );

            cmd.CommandTimeout = 300; // 🔥 เพิ่มเป็น 5 นาที
            await cmd.ExecuteNonQueryAsync();
            await _hub.Clients.All.SendAsync("EICCUpdated");
        }
    }
}
