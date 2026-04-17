// using Microsoft.Data.SqlClient;

// namespace Api.Services
// {
//     public class CleanroomEntryService
//     {
//         private readonly string _connectionString;

//         public CleanroomEntryService(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         public async Task ProcessAsync()
//         {
//             using var conn = new SqlConnection(_connectionString);
//             await conn.OpenAsync();

//             using var transaction = conn.BeginTransaction();

//             try
//             {
//                 var cmd = new SqlCommand(
//                     @"
// WITH Dedup AS (
//     SELECT EmpID, Timestamp, CameraID,
//         ROW_NUMBER() OVER (
//             PARTITION BY EmpID, Timestamp, CameraID
//             ORDER BY Timestamp
//         ) AS rn
//     FROM Transactions
//     WHERE EmpID IS NOT NULL
// ),
// Cleaned AS (
//     SELECT EmpID, Timestamp, CameraID
//     FROM Dedup WHERE rn = 1
// ),
// OrderedLogs AS (
//     SELECT
//         EmpID, Timestamp, CameraID,
//         LAG(CameraID)  OVER (PARTITION BY EmpID ORDER BY Timestamp) AS PrevCamera,
//         -- 🔥 หา Cam2 ถัดไปด้วย LEAD แทน Subquery
//         LEAD(CASE WHEN CameraID = 2 THEN Timestamp END)
//             IGNORE NULLS OVER (PARTITION BY EmpID ORDER BY Timestamp) AS NextCam2Time
//     FROM Cleaned
// ),
// Cleanroom AS (
//     SELECT
//         EmpID,
//         Timestamp AS CheckInDateTime,
//         NextCam2Time AS CheckOutDateTime,
//         CASE
//             WHEN NextCam2Time IS NOT NULL THEN 'Out'
//             ELSE 'In'
//         END AS Status
//     FROM OrderedLogs
//     WHERE CameraID = 3
//       AND (PrevCamera IS NULL OR PrevCamera <> 3)
// )
// MERGE CleanroomEntry AS target
// USING Cleanroom AS source
// ON target.EmpID = source.EmpID
//    AND target.CheckInDateTime = source.CheckInDateTime
// WHEN MATCHED THEN
//     UPDATE SET
//         CheckOutDateTime = source.CheckOutDateTime,
//         CStatus = source.Status
// WHEN NOT MATCHED THEN
//     INSERT (EmpID, CheckInDateTime, CheckOutDateTime, CStatus)
//     VALUES (source.EmpID, source.CheckInDateTime, source.CheckOutDateTime, source.Status);
// ",
//                     conn,
//                     transaction
//                 );

//                 cmd.CommandTimeout = 180; // 🔥 เพิ่ม timeout จาก 120 → 180
//                 await cmd.ExecuteNonQueryAsync();

//                 await transaction.CommitAsync();
//             }
//             catch
//             {
//                 await transaction.RollbackAsync();
//                 throw;
//             }
//         }
//     }
// }

using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class CleanroomEntryService
    {
        private readonly string _connectionString;

        public CleanroomEntryService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        public async Task ProcessAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ เช็คก่อนว่ามี transaction ใหม่ไหม
            var checkCmd = new SqlCommand(
                @"
        DECLARE @WorkDate DATE = CASE
    WHEN CAST(GETDATE() AS TIME) < '07:00:00'
    THEN CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
    ELSE CAST(GETDATE() AS DATE)
END;

SELECT COUNT(1) FROM Transactions
WHERE CAST(Timestamp AS DATE) = @WorkDate
  AND EmpID IS NOT NULL
  AND CameraID IN (2, 3)
    ",
                conn
            );
            checkCmd.CommandTimeout = 10;
            var count = (int)await checkCmd.ExecuteScalarAsync();
            if (count == 0)
                return; // ✅ ออกเลย

            using var transaction = conn.BeginTransaction();
            try
            {
                var cmd = new SqlCommand(
                    @"
DECLARE @WorkDate DATE = CASE
    WHEN CAST(GETDATE() AS TIME) < '07:00:00'
    THEN CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
    ELSE CAST(GETDATE() AS DATE)
END;
DECLARE @From DATETIME = CAST(@WorkDate AS DATETIME);

WITH AffectedEmps AS (
    SELECT DISTINCT EmpID
    FROM Transactions
    WHERE Timestamp >= @From
      AND EmpID IS NOT NULL
      AND CameraID IN (2, 3)
),
Cleaned AS (
    SELECT t.EmpID, t.Timestamp, t.CameraID,
        ROW_NUMBER() OVER (
            PARTITION BY t.EmpID, t.Timestamp, t.CameraID
            ORDER BY t.Timestamp
        ) AS rn
    FROM Transactions t
    WHERE t.EmpID IS NOT NULL
      AND EXISTS (SELECT 1 FROM AffectedEmps a WHERE a.EmpID = t.EmpID)
),
OrderedLogs AS (
    SELECT 
        EmpID, Timestamp, CameraID,
        LAG(CameraID) OVER (PARTITION BY EmpID ORDER BY Timestamp) AS PrevCamera,
        LEAD(CASE WHEN CameraID = 2 THEN Timestamp END)
            IGNORE NULLS OVER (PARTITION BY EmpID ORDER BY Timestamp) AS NextCam2Time
    FROM Cleaned
    WHERE rn = 1
),
Cleanroom AS (
    SELECT 
        EmpID,
        Timestamp AS CheckInDateTime,
        NextCam2Time AS CheckOutDateTime,
        CASE 
            WHEN NextCam2Time IS NOT NULL THEN 'Out'
            ELSE 'In'
        END AS Status
    FROM OrderedLogs
    WHERE CameraID = 3
      AND (PrevCamera IS NULL OR PrevCamera <> 3)
)
MERGE CleanroomEntry AS target
USING Cleanroom AS source
ON target.EmpID = source.EmpID AND target.CheckInDateTime = source.CheckInDateTime
WHEN MATCHED THEN
    UPDATE SET 
        CheckOutDateTime = source.CheckOutDateTime,
        CStatus = source.Status
WHEN NOT MATCHED THEN
    INSERT (EmpID, CheckInDateTime, CheckOutDateTime, CStatus)
    VALUES (source.EmpID, source.CheckInDateTime, source.CheckOutDateTime, source.Status);
",
                    conn,
                    transaction
                );

                cmd.CommandTimeout = 60;
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
