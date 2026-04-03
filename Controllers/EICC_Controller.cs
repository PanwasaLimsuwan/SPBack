using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Data.SqlClient;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EICCControlController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IHubContext<NotificationHub> _hub;

        public EICCControlController(IConfiguration configuration, IHubContext<NotificationHub> hub)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _hub = hub;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] int? weekID
        )
        {
            var results = new List<object>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 🔥 แก้ SELECT ซ้อน + คำนวณ MonthYear จาก Year/WeekID
            var query =
                @"
                SELECT 
                    eicc.ControlID,
                    eicc.EmpID,
                    eicc.WeekID,
                    eicc.Year,
                    CONCAT(eicc.Year, '-W', RIGHT('00' + CAST(eicc.WeekID AS VARCHAR), 2)) AS MonthYear,
                    eicc.TotalHours,
                    eicc.DaysWorked,
                    eicc.TotalOT,
                    eicc.Status,
                    eicc.WeekStartDate,
                    eicc.WeekEndDate,
                    ei.Division,
                    ei.Department,
                    ei.Section,
                    ei.Biz,
                    ei.Process,
                    ei.FirstName,
                    ei.LastName
                FROM EICC_Control eicc
                JOIN EmployeeInfo ei ON eicc.EmpID = ei.EmpID
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(division))
            {
                query += " AND ei.Division = @division";
                parameters.Add(new SqlParameter("@division", division));
            }
            if (!string.IsNullOrEmpty(department))
            {
                query += " AND ei.Department = @department";
                parameters.Add(new SqlParameter("@department", department));
            }
            if (!string.IsNullOrEmpty(section))
            {
                query += " AND ei.Section = @section";
                parameters.Add(new SqlParameter("@section", section));
            }
            if (!string.IsNullOrEmpty(biz))
            {
                query += " AND ei.Biz = @biz";
                parameters.Add(new SqlParameter("@biz", biz));
            }
            if (!string.IsNullOrEmpty(process))
            {
                query += " AND ei.Process = @process";
                parameters.Add(new SqlParameter("@process", process));
            }
            if (weekID.HasValue)
            {
                query += " AND eicc.WeekID = @weekID";
                parameters.Add(new SqlParameter("@weekID", weekID.Value));
            }

            query += " ORDER BY eicc.Year, eicc.WeekID, ei.Biz, ei.Process";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.CommandTimeout = 60;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(
                    new
                    {
                        controlID = reader["ControlID"],
                        empID = reader["EmpID"],
                        weekID = reader["WeekID"],
                        year = reader["Year"],
                        monthYear = reader["MonthYear"]?.ToString(),
                        totalHours = reader["TotalHours"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["TotalHours"]),
                        daysWorked = reader["DaysWorked"] == DBNull.Value
                            ? 0
                            : Convert.ToInt32(reader["DaysWorked"]),
                        totalOT = reader["TotalOT"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["TotalOT"]),
                        status = reader["Status"]?.ToString(),
                        weekStart = reader["WeekStartDate"] == DBNull.Value
                            ? null
                            : (DateTime?)reader["WeekStartDate"],
                        weekEnd = reader["WeekEndDate"] == DBNull.Value
                            ? null
                            : (DateTime?)reader["WeekEndDate"],
                        division = reader["Division"]?.ToString(),
                        department = reader["Department"]?.ToString(),
                        section = reader["Section"]?.ToString(),
                        biz = reader["Biz"]?.ToString(),
                        process = reader["Process"]?.ToString(),
                        firstName = reader["FirstName"]?.ToString(),
                        lastName = reader["LastName"]?.ToString(),
                    }
                );
            }

            return Ok(results);
        }

        [HttpGet("MonthlySummary")]
        public async Task<IActionResult> GetMonthlySummary(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var results = new List<object>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // 🔥 คำนวณ MonthYear จาก Year/WeekID แทนที่จะใช้ column MonthYear
            var query =
                @"
                SELECT 
                    CONCAT(eicc.Year, '-W', RIGHT('00' + CAST(eicc.WeekID AS VARCHAR), 2)) AS MonthYear,
                    ISNULL(SUM(eicc.TotalOT), 0)    AS TotalOT,
                    ISNULL(SUM(eicc.TotalHours), 0) AS TotalHours,
                    COUNT(DISTINCT eicc.EmpID)       AS EmployeeCount
                FROM EICC_Control eicc
                JOIN EmployeeInfo ei ON eicc.EmpID = ei.EmpID
                WHERE 1=1";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(division))
            {
                query += " AND ei.Division = @division";
                parameters.Add(new SqlParameter("@division", division));
            }
            if (!string.IsNullOrEmpty(department))
            {
                query += " AND ei.Department = @department";
                parameters.Add(new SqlParameter("@department", department));
            }
            if (!string.IsNullOrEmpty(section))
            {
                query += " AND ei.Section = @section";
                parameters.Add(new SqlParameter("@section", section));
            }
            if (!string.IsNullOrEmpty(biz))
            {
                query += " AND ei.Biz = @biz";
                parameters.Add(new SqlParameter("@biz", biz));
            }
            if (!string.IsNullOrEmpty(process))
            {
                query += " AND ei.Process = @process";
                parameters.Add(new SqlParameter("@process", process));
            }

            query +=
                @"
                GROUP BY eicc.Year, eicc.WeekID
                ORDER BY eicc.Year, eicc.WeekID";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.CommandTimeout = 60;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(
                    new
                    {
                        month = reader["MonthYear"]?.ToString(),
                        totalOT = Math.Round(Convert.ToDouble(reader["TotalOT"]), 2),
                        totalHours = Math.Round(Convert.ToDouble(reader["TotalHours"]), 2),
                        employeeCount = Convert.ToInt32(reader["EmployeeCount"]),
                    }
                );
            }

            return Ok(results);
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestWeek(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process
        )
        {
            var results = new List<object>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query =
                @"
        WITH LatestWeek AS (
    SELECT TOP 1 WeekID, Year
    FROM EICC_Control
    ORDER BY Year DESC, WeekID DESC
)

SELECT 
    ei.EmpID,
    ei.FirstName,
    ei.LastName,
    lw.WeekID,
    lw.Year,
    ISNULL(eicc.TotalHours, 0) AS TotalHours
FROM EmployeeInfo ei
CROSS JOIN LatestWeek lw
LEFT JOIN EICC_Control eicc 
    ON eicc.EmpID = ei.EmpID
    AND eicc.WeekID = lw.WeekID
    AND eicc.Year = lw.Year
WHERE 1=1
    ";

            var parameters = new List<SqlParameter>();

            if (!string.IsNullOrEmpty(division))
            {
                query += " AND ei.Division = @division";
                parameters.Add(new SqlParameter("@division", division));
            }

            if (!string.IsNullOrEmpty(department))
            {
                query += " AND ei.Department = @department";
                parameters.Add(new SqlParameter("@department", department));
            }

            if (!string.IsNullOrEmpty(section))
            {
                query += " AND ei.Section = @section";
                parameters.Add(new SqlParameter("@section", section));
            }

            if (!string.IsNullOrEmpty(biz))
            {
                query += " AND ei.Biz = @biz";
                parameters.Add(new SqlParameter("@biz", biz));
            }

            if (!string.IsNullOrEmpty(process))
            {
                query += " AND ei.Process = @process";
                parameters.Add(new SqlParameter("@process", process));
            }

            query += " ORDER BY ei.Biz, ei.Process, eicc.EmpID";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddRange(parameters.ToArray());
            cmd.CommandTimeout = 60;

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(
                    new
                    {
                        empID = reader["EmpID"],
                        weekID = reader["WeekID"],
                        year = reader["Year"],
                        totalHours = reader["TotalHours"] == DBNull.Value
                            ? 0m
                            : Convert.ToDecimal(reader["TotalHours"]),
                        firstName = reader["FirstName"]?.ToString(),
                        lastName = reader["LastName"]?.ToString(),
                    }
                );
            }

            return Ok(results);
        }

        [HttpPost("test-update")]
        public async Task<IActionResult> TestRealtime()
        {
            await _hub.Clients.All.SendAsync("EICCUpdated");
            return Ok(new { message = "Realtime triggered" });
        }
    }
}
