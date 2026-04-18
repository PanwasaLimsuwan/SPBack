// using Api.Hubs;
// using Microsoft.AspNetCore.SignalR;
// using Microsoft.Data.SqlClient;

// namespace Api.Services
// {
//     public class AttendanceService
//     {
//         private readonly string _connectionString;
//         private readonly IHubContext<AttendanceHub> _hub;

//         public AttendanceService(IConfiguration configuration, IHubContext<AttendanceHub> hub)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//             _hub = hub;
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
// WITH Mapped AS (
//     SELECT
//         t.EmpID,
//         CASE
//             WHEN m.Shift = 'NIGHT'
//                  AND CAST(t.Timestamp AS TIME) < '07:00:00'
//             THEN CAST(DATEADD(DAY, -1, t.Timestamp) AS DATE)
//             ELSE m.Date
//         END AS WorkDate,
//         m.Shift,
//         t.Timestamp,
//         t.CameraID
//     FROM Transactions t
//     JOIN EmployeeInfo e ON e.EmpID = t.EmpID
//     JOIN ManpowerPlan m
//         ON LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
//         AND t.Timestamp >=
//             CASE
//                 WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 7,  CAST(m.Date AS DATETIME))
//                 ELSE                       DATEADD(HOUR, 19, CAST(m.Date AS DATETIME))
//             END
//         AND t.Timestamp <
//             CASE
//                 WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 31, CAST(m.Date AS DATETIME))
//                 ELSE                       DATEADD(HOUR, 38, CAST(m.Date AS DATETIME))
//             END
// ),
// Dedup AS (
//     SELECT *,
//         ROW_NUMBER() OVER (
//             PARTITION BY EmpID, Timestamp, CameraID
//             ORDER BY Timestamp
//         ) AS rn
//     FROM Mapped
// ),
// Final AS (
//     SELECT
//         EmpID, WorkDate,
//         MIN(CASE WHEN CameraID = 1 THEN Timestamp END) AS CheckInTime,
//         MAX(CASE WHEN CameraID = 1 THEN Timestamp END) AS CheckOutTime,
//         MAX(Shift) AS Shift
//     FROM Dedup
//     WHERE rn = 1
//     GROUP BY EmpID, WorkDate
// ),
// FinalCleaned AS (
//     SELECT
//         EmpID, WorkDate, CheckInTime, Shift,
//         CASE
//             WHEN CheckOutTime = CheckInTime THEN NULL
//             WHEN CheckOutTime >= LEAD(CheckInTime) OVER (
//                 PARTITION BY EmpID ORDER BY WorkDate
//             ) THEN NULL
//             ELSE CheckOutTime
//         END AS CheckOutTime
//     FROM Final
// ),
// -- 🔥 ดึงคนทุกคนที่ควรมาทำงาน
// AllEmployees AS (
//     SELECT
//         e.EmpID,
//         CAST(m.Date AS DATE) AS WorkDate,
//         m.Shift
//     FROM EmployeeInfo e
//     JOIN ManpowerPlan m
//         ON LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
// ),
// -- 🔥 รวม คนมา + คนขาดงาน
// FinalWithAbsent AS (
//     SELECT
//         ae.EmpID,
//         ae.WorkDate,
//         ae.Shift,
//         fc.CheckInTime,
//         fc.CheckOutTime
//     FROM AllEmployees ae
//     LEFT JOIN FinalCleaned fc
//         ON fc.EmpID = ae.EmpID
//         AND fc.WorkDate = ae.WorkDate
// )

// MERGE Attendance AS target
// USING FinalWithAbsent AS source
// ON target.EmpID = source.EmpID
//    AND target.Date = source.WorkDate

// WHEN MATCHED THEN
//     UPDATE SET
//         CheckInTime  = source.CheckInTime,
//         CheckOutTime = source.CheckOutTime,
//         Status = CASE
//             WHEN source.CheckInTime IS NULL THEN 'Absent'
//             WHEN source.CheckOutTime IS NULL THEN 'Missing Checkout'
//             WHEN source.Shift = 'DAY'
//                  AND CAST(source.CheckInTime AS TIME) <= '07:15:00' THEN 'Normal'
//             WHEN source.Shift = 'NIGHT'
//                  AND CAST(source.CheckInTime AS TIME) >= '19:00:00'
//                  AND CAST(source.CheckInTime AS TIME) <= '19:15:00' THEN 'Normal'
//             ELSE 'Late'
//         END

// WHEN NOT MATCHED THEN
//     INSERT (EmpID, Date, CheckInTime, CheckOutTime, Status)
//     VALUES (
//         source.EmpID, source.WorkDate,
//         source.CheckInTime, source.CheckOutTime,
//         CASE
//             WHEN source.CheckInTime IS NULL THEN 'Absent'
//             WHEN source.CheckOutTime IS NULL THEN 'Missing Checkout'
//             WHEN source.Shift = 'DAY'
//                  AND CAST(source.CheckInTime AS TIME) <= '07:15:00' THEN 'Normal'
//             WHEN source.Shift = 'NIGHT'
//                  AND CAST(source.CheckInTime AS TIME) >= '19:00:00'
//                  AND CAST(source.CheckInTime AS TIME) <= '19:15:00' THEN 'Normal'
//             ELSE 'Late'
//         END
//     );
// ",
//                     conn,
//                     transaction
//                 );

//                 cmd.CommandTimeout = 180;

//                 await cmd.ExecuteNonQueryAsync();
//                 await transaction.CommitAsync();

//                 await _hub.Clients.All.SendAsync("AttendanceUpdated");
//             }
//             catch
//             {
//                 await transaction.RollbackAsync();
//                 throw;
//             }
//         }
//     }
// }

using Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class AttendanceService
    {
        private readonly string _connectionString;
        private readonly IHubContext<AttendanceHub> _hub;

        public AttendanceService(IConfiguration configuration, IHubContext<AttendanceHub> hub)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _hub = hub;
        }

        public async Task ProcessAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ ขั้นที่ 1 — เช็คว่ามี transaction ของ workdate นี้ไหม
            var checkCmd = new SqlCommand(
                @"
                DECLARE @WorkDate DATE = CASE
                    WHEN CAST(GETDATE() AS TIME) < '07:00:00'
                    THEN CAST(DATEADD(DAY, -1, GETDATE()) AS DATE)
                    ELSE CAST(GETDATE() AS DATE)
                END;

                SELECT COUNT(1)
                FROM Transactions t
                JOIN EmployeeInfo e ON e.EmpID = t.EmpID
                JOIN ManpowerPlan m
                    ON LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
                    AND CAST(m.Date AS DATE) = @WorkDate
                WHERE CAST(t.Timestamp AS DATE) >= @WorkDate
                  AND t.EmpID IS NOT NULL
                  AND t.CameraID = 1
            ",
                conn
            );
            checkCmd.CommandTimeout = 10;
            var count = (int)await checkCmd.ExecuteScalarAsync();
            if (count == 0)
                return;

            // ✅ ขั้นที่ 2 — process
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

                    WITH RecentTx AS (
                        SELECT EmpID, Timestamp, CameraID
                        FROM Transactions
                        WHERE Timestamp >= @From
                          AND EmpID IS NOT NULL
                    ),
                    Mapped AS (
                        SELECT
                            t.EmpID,
                            CASE
                                WHEN m.Shift = 'NIGHT' AND CAST(t.Timestamp AS TIME) < '07:00:00'
                                THEN CAST(DATEADD(DAY, -1, t.Timestamp) AS DATE)
                                ELSE CAST(m.Date AS DATE)
                            END AS WorkDate,
                            m.Shift,
                            t.Timestamp,
                            t.CameraID
                        FROM RecentTx t
                        JOIN EmployeeInfo e ON e.EmpID = t.EmpID
                        JOIN ManpowerPlan m
                            ON m.ShiftCode = e.ShiftCode
                            AND t.Timestamp >= CASE
                                WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 7,  CAST(m.Date AS DATETIME))
                                ELSE                       DATEADD(HOUR, 19, CAST(m.Date AS DATETIME))
                            END
                            AND t.Timestamp < CASE
                                WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 31, CAST(m.Date AS DATETIME))
                                ELSE                       DATEADD(HOUR, 38, CAST(m.Date AS DATETIME))
                            END
                    ),
                    AffectedDays AS (
                        SELECT DISTINCT EmpID, WorkDate
                        FROM Mapped
                    ),
                    FullDayTx AS (
                        SELECT
                            t.EmpID,
                            CASE
                                WHEN m.Shift = 'NIGHT' AND CAST(t.Timestamp AS TIME) < '07:00:00'
                                THEN CAST(DATEADD(DAY, -1, t.Timestamp) AS DATE)
                                ELSE CAST(m.Date AS DATE)
                            END AS WorkDate,
                            m.Shift,
                            t.Timestamp,
                            t.CameraID
                        FROM Transactions t
                        JOIN EmployeeInfo e ON e.EmpID = t.EmpID
                        JOIN ManpowerPlan m
                            ON m.ShiftCode = e.ShiftCode
                            AND t.Timestamp >= CASE
                                WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 7,  CAST(m.Date AS DATETIME))
                                ELSE                       DATEADD(HOUR, 19, CAST(m.Date AS DATETIME))
                            END
                            AND t.Timestamp < CASE
                                WHEN m.Shift = 'DAY'  THEN DATEADD(HOUR, 31, CAST(m.Date AS DATETIME))
                                ELSE                       DATEADD(HOUR, 38, CAST(m.Date AS DATETIME))
                            END
                        WHERE EXISTS (
                            SELECT 1 FROM AffectedDays ad
                            WHERE ad.EmpID = t.EmpID
                              AND ad.WorkDate = CASE
                                  WHEN m.Shift = 'NIGHT' AND CAST(t.Timestamp AS TIME) < '07:00:00'
                                  THEN CAST(DATEADD(DAY, -1, t.Timestamp) AS DATE)
                                  ELSE CAST(m.Date AS DATE)
                              END
                        )
                    ),
                    Dedup AS (
                        SELECT *,
                            ROW_NUMBER() OVER (
                                PARTITION BY EmpID, Timestamp, CameraID ORDER BY Timestamp
                            ) AS rn
                        FROM FullDayTx
                    ),
                    Final AS (
    SELECT
        EmpID, WorkDate,
        MIN(CASE WHEN CameraID = 1 THEN Timestamp END) AS CheckInTime,
        MAX(CASE 
            WHEN CameraID = 1 
            AND DATEDIFF(MINUTE, Timestamp, GETDATE()) >= 30  -- ✅ เพิ่มตรงนี้
            THEN Timestamp 
        END) AS CheckOutTime,
        MAX(Shift) AS Shift
    FROM Dedup WHERE rn = 1
    GROUP BY EmpID, WorkDate
),
                    FinalCleaned AS (
                        SELECT
                            EmpID, WorkDate, CheckInTime, Shift,
                            CASE
                                WHEN CheckOutTime = CheckInTime THEN NULL
                                WHEN CheckOutTime >= LEAD(CheckInTime) OVER (
                                    PARTITION BY EmpID ORDER BY WorkDate
                                ) THEN NULL
                                ELSE CheckOutTime
                            END AS CheckOutTime
                        FROM Final
                    ),
                    -- ✅ รวมคนที่มาและคนที่ขาด (Absent) ไว้ใน source เดียวกัน
                    AllEmployees AS (
                        SELECT
                            e.EmpID,
                            CAST(m.Date AS DATE) AS WorkDate,
                            m.Shift
                        FROM EmployeeInfo e
                        JOIN ManpowerPlan m
                            ON LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
                        WHERE CAST(m.Date AS DATE) = @WorkDate
                    ),
                    MergeSource AS (
                        SELECT
                            ae.EmpID,
                            ae.WorkDate,
                            ae.Shift,
                            fc.CheckInTime,
                            fc.CheckOutTime
                        FROM AllEmployees ae
                        LEFT JOIN FinalCleaned fc
                            ON fc.EmpID = ae.EmpID
                            AND fc.WorkDate = ae.WorkDate
                    )

                    -- ✅ MERGE ครอบคลุมทั้งคนมาและคนขาด
                    MERGE Attendance AS target
                    USING MergeSource AS source
                    ON target.EmpID = source.EmpID AND target.Date = source.WorkDate

                    WHEN MATCHED THEN
                        UPDATE SET
                            CheckInTime  = source.CheckInTime,
                            CheckOutTime = source.CheckOutTime,
                            Status = CASE
                                -- ไม่มี CheckIn → Absent
                                WHEN source.CheckInTime IS NULL THEN 'Absent'

                                -- มี CheckIn + CheckOut → Get Off
                                WHEN source.CheckOutTime IS NOT NULL THEN 'Get Off'

                                -- CheckIn แต่กะยังไม่จบ → Normal / Late
                                WHEN source.CheckOutTime IS NULL AND (
                                    (source.Shift = 'DAY'   AND GETDATE() < DATEADD(HOUR, 19, CAST(source.WorkDate AS DATETIME)))
                                 OR (source.Shift = 'NIGHT' AND GETDATE() < DATEADD(HOUR, 7,  CAST(DATEADD(DAY, 1, source.WorkDate) AS DATETIME)))
                                ) THEN
                                    CASE
                                        WHEN source.Shift = 'DAY'   AND CAST(source.CheckInTime AS TIME) <= '07:15:00' THEN 'Normal'
                                        WHEN source.Shift = 'NIGHT' AND CAST(source.CheckInTime AS TIME) BETWEEN '19:00:00' AND '19:15:00' THEN 'Normal'
                                        ELSE 'Late'
                                    END

                                -- กะจบแล้ว ไม่มี CheckOut → Missing Checkout
                                ELSE 'Missing Checkout'
                            END

                    WHEN NOT MATCHED THEN
                        INSERT (EmpID, Date, CheckInTime, CheckOutTime, Status)
                        VALUES (
                            source.EmpID, source.WorkDate, source.CheckInTime, source.CheckOutTime,
                            CASE
                                -- ไม่มี CheckIn → Absent
                                WHEN source.CheckInTime IS NULL THEN 'Absent'

                                -- มี CheckIn + CheckOut → Get Off
                                WHEN source.CheckOutTime IS NOT NULL THEN 'Get Off'

                                -- CheckIn แต่กะยังไม่จบ → Normal / Late
                                WHEN source.CheckOutTime IS NULL AND (
                                    (source.Shift = 'DAY'   AND GETDATE() < DATEADD(HOUR, 19, CAST(source.WorkDate AS DATETIME)))
                                 OR (source.Shift = 'NIGHT' AND GETDATE() < DATEADD(HOUR, 7,  CAST(DATEADD(DAY, 1, source.WorkDate) AS DATETIME)))
                                ) THEN
                                    CASE
                                        WHEN source.Shift = 'DAY'   AND CAST(source.CheckInTime AS TIME) <= '07:15:00' THEN 'Normal'
                                        WHEN source.Shift = 'NIGHT' AND CAST(source.CheckInTime AS TIME) BETWEEN '19:00:00' AND '19:15:00' THEN 'Normal'
                                        ELSE 'Late'
                                    END

                                -- กะจบแล้ว ไม่มี CheckOut → Missing Checkout
                                ELSE 'Missing Checkout'
                            END
                        );
                ",
                    conn,
                    transaction
                );

                cmd.CommandTimeout = 60;
                await cmd.ExecuteNonQueryAsync();
                await transaction.CommitAsync();
                await _hub.Clients.All.SendAsync("AttendanceUpdated");
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
            // Final AS (
            //             SELECT
            //                 EmpID, WorkDate,
            //                 MIN(CASE WHEN CameraID = 1 THEN Timestamp END) AS CheckInTime,
            //                 MAX(CASE WHEN CameraID = 1 THEN Timestamp END) AS CheckOutTime,
            //                 MAX(Shift) AS Shift
            //             FROM Dedup WHERE rn = 1
            //             GROUP BY EmpID, WorkDate
            //         ),
        }
    }
}
