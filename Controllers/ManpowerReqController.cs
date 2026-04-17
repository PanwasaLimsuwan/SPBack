using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerReqController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerReqController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var result = new List<object>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"
                SELECT DISTINCT
                    m.MPRID,
                    m.Date,
                    m.Biz,
                    m.Process,
                    m.Require,
                    m.SkillGroup,
                    m.Present,
                    m.Shortage,
                    m.LastUpdateTime,
                    (SELECT TOP 1 Division   FROM EmployeeInfo WHERE Biz = m.Biz AND Process = m.Process) AS Division,
                    (SELECT TOP 1 Department FROM EmployeeInfo WHERE Biz = m.Biz AND Process = m.Process) AS Department,
                    (SELECT TOP 1 Section    FROM EmployeeInfo WHERE Biz = m.Biz AND Process = m.Process) AS Section
                FROM ManpowerReq m
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(biz))
            {
                query += " AND m.Biz = @Biz";
                parameters.Add(new SqlParameter("@Biz", biz));
            }
            if (!string.IsNullOrEmpty(process))
            {
                query += " AND m.Process = @Process";
                parameters.Add(new SqlParameter("@Process", process));
            }

            using var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 30;
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    mprID        = reader.GetInt32(reader.GetOrdinal("MPRID")),
                    date         = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                    biz          = reader["Biz"]?.ToString(),
                    process      = reader["Process"]?.ToString(),
                    require      = reader["Require"]      == DBNull.Value ? null : (int?)reader["Require"],
                    skillGroup   = reader["SkillGroup"]?.ToString(),
                    present      = reader["Present"]      == DBNull.Value ? null : (int?)reader["Present"],
                    shortage     = reader["Shortage"]     == DBNull.Value ? null : (int?)reader["Shortage"],
                    lastUpdateTime = reader["LastUpdateTime"] == DBNull.Value ? null : (DateTime?)reader["LastUpdateTime"],
                    division     = reader["Division"]?.ToString(),
                    department   = reader["Department"]?.ToString(),
                    section      = reader["Section"]?.ToString(),
                });
            }

            return Ok(result);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatest(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? workDate,
            [FromQuery] string? shiftCode
        )
        {
            var result = new List<object>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query = @"
WITH LatestDate AS (
    SELECT CAST(
        CASE 
            WHEN CAST(GETDATE() AS TIME) < '07:00:00'
            THEN DATEADD(DAY, -1, GETDATE())
            ELSE GETDATE()
        END 
    AS DATE) AS MaxDate
),
ActiveShift AS (
    SELECT CASE
        WHEN CAST(GETDATE() AS TIME) >= '07:00:00' 
             AND CAST(GETDATE() AS TIME) < '19:00:00' THEN 'DAY'
        ELSE 'NIGHT'
    END AS CurrentShift
),
AbsentCount AS (
    SELECT ei.Biz, ei.Process, COUNT(*) AS AbsentTotal
    FROM Attendance a
    JOIN EmployeeInfo ei ON a.EmpID = ei.EmpID
    JOIN ManpowerPlan mp 
        ON LTRIM(RTRIM(mp.ShiftCode)) = LTRIM(RTRIM(ei.ShiftCode))
        AND CAST(mp.Date AS DATE) = (SELECT MaxDate FROM LatestDate)
        AND mp.Shift = (SELECT CurrentShift FROM ActiveShift)
    WHERE CAST(a.Date AS DATE) = (SELECT MaxDate FROM LatestDate)
    AND a.Status = 'Absent'
    GROUP BY ei.Biz, ei.Process
),
AssignedCount AS (
    SELECT
        ToProcess AS Process,
        ToBiz     AS Biz,
        COUNT(*)  AS AssignedTotal
    FROM Assignment
    WHERE Status IN ('Active', 'Returning')
    GROUP BY ToProcess, ToBiz
)
SELECT DISTINCT
    m.Date        AS WorkDate,
    m.Biz,
    m.Process,
    m.SkillGroup,
    m.Require     AS Required,
    m.Present,
    m.Shortage,
    m.LastUpdateTime,
    e.Division,
    e.Department,
    e.Section,
    CASE
        WHEN ISNULL(ac.AbsentTotal, 0) - ISNULL(asgn.AssignedTotal, 0) < 0 THEN 0
        ELSE ISNULL(ac.AbsentTotal, 0) - ISNULL(asgn.AssignedTotal, 0)
    END AS HeadcountShortage
FROM ManpowerReq m
JOIN LatestDate ld ON m.Date = ld.MaxDate
LEFT JOIN (
    SELECT DISTINCT Biz, Process, Division, Department, Section
    FROM EmployeeInfo
) e ON m.Biz = e.Biz AND m.Process = e.Process
LEFT JOIN AbsentCount ac
    ON m.Biz = ac.Biz AND m.Process = ac.Process
LEFT JOIN AssignedCount asgn
    ON m.Biz = asgn.Biz AND m.Process = asgn.Process
WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(biz))
            {
                query += " AND m.Biz = @Biz";
                parameters.Add(new SqlParameter("@Biz", biz));
            }
            if (!string.IsNullOrEmpty(process))
            {
                query += " AND m.Process = @Process";
                parameters.Add(new SqlParameter("@Process", process));
            }
            if (!string.IsNullOrEmpty(division))
            {
                query += " AND e.Division = @Division";
                parameters.Add(new SqlParameter("@Division", division));
            }
            if (!string.IsNullOrEmpty(department))
            {
                query += " AND e.Department = @Department";
                parameters.Add(new SqlParameter("@Department", department));
            }
            if (!string.IsNullOrEmpty(section))
            {
                query += " AND e.Section = @Section";
                parameters.Add(new SqlParameter("@Section", section));
            }

            query += " ORDER BY m.Process, m.SkillGroup";

            using var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 60;
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(new
                {
                    workDate = reader["WorkDate"] == DBNull.Value
                        ? null : (DateTime?)reader["WorkDate"],
                    biz           = reader["Biz"]?.ToString(),
                    process       = reader["Process"]?.ToString(),
                    skillGroup    = reader["SkillGroup"]?.ToString(),
                    required = reader["Required"] == DBNull.Value
                        ? 0 : Convert.ToInt32(reader["Required"]),
                    present = reader["Present"] == DBNull.Value
                        ? 0 : Convert.ToInt32(reader["Present"]),
                    shortage = reader["Shortage"] == DBNull.Value
                        ? 0 : Convert.ToInt32(reader["Shortage"]),
                    headcountShortage = reader["HeadcountShortage"] == DBNull.Value
                        ? 0 : Convert.ToInt32(reader["HeadcountShortage"]),
                    lastUpdateTime = reader["LastUpdateTime"] == DBNull.Value
                        ? null : (DateTime?)reader["LastUpdateTime"],
                    division   = reader["Division"]?.ToString(),
                    department = reader["Department"]?.ToString(),
                    section    = reader["Section"]?.ToString(),
                });
            }

            return Ok(result);
        }
    }
}