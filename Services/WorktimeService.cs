using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class WorktimeService
    {
        private readonly string _connectionString;

        public WorktimeService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task CalculateAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"
    WITH Base AS (
        SELECT 
            EmpID,
            Date,
            -- 🔥 รวม Date + Time เป็น DATETIME ก่อน
            CAST(Date AS DATETIME) + CAST(CheckInTime AS DATETIME) AS CheckInDT,
            CASE 
                WHEN CheckOutTime < CheckInTime
                -- ข้ามวัน: CheckOut เป็นของวันถัดไป
                THEN CAST(DATEADD(DAY, 1, Date) AS DATETIME) + CAST(CheckOutTime AS DATETIME)
                ELSE CAST(Date AS DATETIME) + CAST(CheckOutTime AS DATETIME)
            END AS CheckOutDT,
            Status
        FROM Attendance
        WHERE CheckInTime IS NOT NULL
          AND CheckOutTime IS NOT NULL
    ),
    Calc AS (
        SELECT
            EmpID,
            Date,
            Status,
            DATEDIFF(MINUTE, CheckInDT, CheckOutDT) / 60.0 AS WorkedHours,
            CASE 
                WHEN DATEDIFF(MINUTE, CheckInDT, CheckOutDT) > 720
                THEN (DATEDIFF(MINUTE, CheckInDT, CheckOutDT) - 720) / 60.0
                ELSE 0
            END AS OTHours
        FROM Base
    )
    MERGE Worktime AS target
    USING Calc AS source
    ON target.EmpID = source.EmpID
       AND target.Date = source.Date
    WHEN MATCHED THEN
        UPDATE SET
            WorkedHours = source.WorkedHours,
            OTHours = source.OTHours,
            Status = source.Status
    WHEN NOT MATCHED THEN
        INSERT (EmpID, Date, WorkedHours, OTHours, Status)
        VALUES (source.EmpID, source.Date,
                source.WorkedHours,
                source.OTHours,
                source.Status);
",
                conn
            );

            cmd.CommandTimeout = 120;
            await cmd.ExecuteNonQueryAsync();
        }
    }
}
