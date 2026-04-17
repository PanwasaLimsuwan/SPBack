using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class ManpowerPlanService
    {
        private readonly string _connectionString;

        public ManpowerPlanService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ============================
        // 🔥 Generate Plan ONLY (ยังไม่ใช้ Actual)
        // ============================
        public async Task GeneratePlanAsync(DateTime startDate, DateTime endDate)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query =
                @"
WITH Dates AS (
    SELECT @StartDate AS d
    UNION ALL
    SELECT DATEADD(DAY,1,d) FROM Dates WHERE d < @EndDate
),
ShiftCycle AS (
    SELECT d, DATEDIFF(DAY, @StartDate, d) % 12 AS CycleDay
    FROM Dates
),
Shifts AS (
    SELECT 'A' AS ShiftCode, 0 AS Offset
    UNION ALL SELECT 'B', 4
    UNION ALL SELECT 'C', 8
),
Final AS (
    SELECT 
        sc.d AS Date,
        s.ShiftCode,
        (sc.CycleDay + s.Offset) % 12 AS ShiftDay
    FROM ShiftCycle sc
    CROSS JOIN Shifts s
),
Result AS (
    SELECT 
        Date,
        ShiftCode,
        CASE 
            WHEN ShiftDay IN (0,1,2,3) THEN 'NIGHT'
            WHEN ShiftDay IN (6,7,8,9) THEN 'DAY'
            ELSE NULL
        END AS ShiftType
    FROM Final
)
MERGE ManpowerPlan AS target
USING (
    SELECT Date, ShiftCode, ShiftType, 35 AS PlannedHeadcount
    FROM Result
    WHERE ShiftType IS NOT NULL
) AS source
ON target.Date = source.Date 
   AND target.ShiftCode = source.ShiftCode
   AND target.Shift = source.ShiftType
WHEN NOT MATCHED THEN
    INSERT (Date, ShiftCode, Shift, PlannedHeadcount)
    VALUES (source.Date, source.ShiftCode, source.ShiftType, source.PlannedHeadcount)
WHEN NOT MATCHED BY SOURCE 
     AND target.Date BETWEEN @StartDate AND @EndDate THEN
    DELETE
OPTION (MAXRECURSION 1000);
";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@StartDate", startDate);
            cmd.Parameters.AddWithValue("@EndDate", endDate);

            cmd.CommandTimeout = 180;

            var rows = await cmd.ExecuteNonQueryAsync();

            Console.WriteLine($"[GeneratePlan] Inserted rows: {rows}");
        }

        public async Task UpdateActualHeadcountAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query =
                @"
DECLARE @WorkDate DATE = CASE
    WHEN CAST(GETDATE() AS TIME) < '07:00:00'
    THEN CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
    ELSE CAST(GETDATE() AS DATE)
END;

-- 🔥 อัพเดตเฉพาะวันล่าสุดแทนทุกวัน
UPDATE ManpowerPlan
SET ActualHeadcount = (
    SELECT COUNT(DISTINCT a.EmpID)
    FROM Attendance a
    JOIN EmployeeInfo e ON e.EmpID = a.EmpID
    WHERE a.Date = @WorkDate
      AND a.CheckInTime IS NOT NULL
      AND LTRIM(RTRIM(e.ShiftCode)) = LTRIM(RTRIM(ManpowerPlan.ShiftCode))
)
WHERE CAST(ManpowerPlan.Date AS DATE) = @WorkDate;
";

            using var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 180; // 🔥 เพิ่ม timeout เพราะอัพเดตหลายวัน
            await cmd.ExecuteNonQueryAsync();

            Console.WriteLine("[UpdateActual] Updated ActualHeadcount for all dates");
        }
    }
}
