using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateEntryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public GateEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        [HttpGet]
        public async Task<IActionResult> GetGateEntry(
            [FromQuery] string? division,
            [FromQuery] string? department,
            [FromQuery] string? section,
            [FromQuery] string? biz,
            [FromQuery] string? process,
            [FromQuery] DateTime? date
        )
        {
            var result = new List<object>();

            using (
                var conn = new SqlConnection(
                    _configuration.GetConnectionString("DefaultConnection")
                )
            )
            {
                await conn.OpenAsync();

                DateTime selectedDate;

                if (date.HasValue)
                {
                    selectedDate = date.Value;
                }
                else
                {
                    var latestDateCmd = new SqlCommand(
                        "SELECT TOP 1 EntryDateTime FROM GateEntry WHERE EntryDateTime IS NOT NULL ORDER BY EntryDateTime DESC",
                        conn
                    );
                    var latestDateObj = await latestDateCmd.ExecuteScalarAsync();

                    if (latestDateObj == null || latestDateObj == DBNull.Value)
                    {
                        return Ok(result);
                    }

                    selectedDate = ((DateTime)latestDateObj).Date;
                }

                DateTime dateStart = selectedDate.Date;
                // DateTime dateEnd = selectedDate.Date.AddDays(1).AddSeconds(-1);
                DateTime dateEnd = selectedDate.Date.AddDays(1).AddHours(7).AddSeconds(-1); // ครอบคลุมเวลาออกกะ 07:00 ของวันถัดไป

                var query =
                    @"
                    WITH RankedGateEntry AS (
                        SELECT 
                            g.EmpID,
                            e.FirstName, e.LastName, e.Division, e.Department, 
                            e.Position, e.Email, e.ShiftCode, e.Section,
                            e.Biz, e.Process,
                            g.EntryDateTime, g.ExitDateTime, g.GateNo, g.GateStatus,
                            c.CStatus, c.CheckInDateTime, c.CheckOutDateTime,
                            ROW_NUMBER() OVER (PARTITION BY g.EmpID ORDER BY g.EntryDateTime DESC) AS rn
                        FROM GateEntry g
                        JOIN EmployeeInfo e ON g.EmpID = e.EmpID
                        OUTER APPLY (
                            SELECT TOP 1 *
                            FROM CleanroomEntry c
                            WHERE c.EmpID = g.EmpID
                            ORDER BY c.CheckInDateTime DESC
                        ) c
                        WHERE (
                            g.EntryDateTime BETWEEN @dateStart AND @dateEnd
                            OR g.ExitDateTime BETWEEN @dateStart AND @dateEnd
                            OR (g.EntryDateTime <= @dateStart AND g.ExitDateTime >= @dateEnd)
                        )
                        AND (@division IS NULL OR e.Division = @division)
                        AND (@department IS NULL OR e.Department = @department)
                        AND (@section IS NULL OR e.Section = @section)
                        AND (@biz IS NULL OR COALESCE(e.Biz, '') = @biz)
                        AND (@process IS NULL OR COALESCE(e.Process, '') = @process)
                    )
                    SELECT * FROM RankedGateEntry WHERE rn = 1";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@dateStart", dateStart);
                    cmd.Parameters.AddWithValue("@dateEnd", dateEnd);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var empID = Convert.ToInt32(reader["EmpID"]);
                            var gateStatus =
                                reader["GateStatus"] == DBNull.Value
                                    ? null
                                    : reader["GateStatus"]?.ToString();
                            var cStatus =
                                reader["CStatus"] == DBNull.Value
                                    ? null
                                    : reader["CStatus"]?.ToString();

                            DateTime? entryDateTime =
                                reader["EntryDateTime"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["EntryDateTime"];
                            DateTime? exitDateTime =
                                reader["ExitDateTime"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["ExitDateTime"];
                            DateTime? checkInDateTime =
                                reader["CheckInDateTime"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["CheckInDateTime"];
                            DateTime? checkOutDateTime =
                                reader["CheckOutDateTime"] == DBNull.Value
                                    ? (DateTime?)null
                                    : (DateTime)reader["CheckOutDateTime"];

                            if (gateStatus == "IN")
                            {
                                exitDateTime = null;
                                checkOutDateTime = null;
                            }
                            else if (string.IsNullOrEmpty(gateStatus))
                            {
                                entryDateTime = null;
                                exitDateTime = null;
                                checkInDateTime = null;
                                checkOutDateTime = null;
                            }

                            string status = (cStatus, gateStatus) switch
                            {
                                (null, null) => "status-missing",
                                ("OUT", "IN") => "status-out-cleanroom",
                                ("IN", "IN") => "status-in-cleanroom",
                                ("OUT", "OUT") => "status-get-off",
                                _ => "status-unknown",
                            };

                            result.Add(
                                new
                                {
                                    EmpID = empID,
                                    firstName = reader["FirstName"]?.ToString(),
                                    lastName = reader["LastName"]?.ToString(),
                                    division = reader["Division"]?.ToString(),
                                    department = reader["Department"]?.ToString(),
                                    position = reader["Position"]?.ToString(),
                                    email = reader["Email"]?.ToString(),
                                    shiftCode = reader["ShiftCode"]?.ToString(),
                                    section = reader["Section"]?.ToString(),
                                    entryDateTime = entryDateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    exitDateTime = exitDateTime?.ToString("yyyy-MM-dd HH:mm:ss"),
                                    gateNo = reader["GateNo"]?.ToString(),
                                    gateStatus,
                                    biz = reader["Biz"]?.ToString(),
                                    process = reader["Process"]?.ToString(),
                                    cStatus,
                                    checkInDateTime = checkInDateTime?.ToString(
                                        "yyyy-MM-dd HH:mm:ss"
                                    ),
                                    checkOutDateTime = checkOutDateTime?.ToString(
                                        "yyyy-MM-dd HH:mm:ss"
                                    ),
                                    status,
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