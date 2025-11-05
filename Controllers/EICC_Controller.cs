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
    public class EICCControlController : ControllerBase
    {
        private readonly string _connectionString;

        public EICCControlController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: ดึงข้อมูลทั้งหมด (รายสัปดาห์)
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

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query =
                    @"
            SELECT 
                eicc.ControlID, eicc.EmpID, eicc.WeekID, eicc.MonthYear,
                eicc.TotalHours, eicc.DaysWorked, eicc.TotalOT, eicc.Status,
                ei.Division, ei.Department, ei.Section, ei.Biz, ei.Process, ei.FirstName, ei.LastName
            FROM EICC_Control eicc
            JOIN EmployeeInfo ei ON eicc.EmpID = ei.EmpID
            WHERE 1=1";

                if (!string.IsNullOrEmpty(division))
                    query += " AND ei.Division = @division";
                if (!string.IsNullOrEmpty(department))
                    query += " AND ei.Department = @department";
                if (!string.IsNullOrEmpty(section))
                    query += " AND ei.Section = @section";
                if (!string.IsNullOrEmpty(biz))
                    query += " AND ei.Biz = @biz";
                if (!string.IsNullOrEmpty(process))
                    query += " AND ei.Process = @process";
                if (weekID.HasValue)
                    query += " AND eicc.WeekID = @weekID";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@division", (object?)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object?)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object?)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object?)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object?)process ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@weekID", (object?)weekID ?? DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            results.Add(
                                new
                                {
                                    controlID = reader.GetInt32(0),
                                    empID = reader.GetInt32(1),
                                    weekID = reader.GetInt32(2),
                                    monthYear = reader["MonthYear"]?.ToString(),
                                    totalHours = reader.IsDBNull(4)
                                        ? (decimal?)null
                                        : Convert.ToDecimal(reader.GetValue(4)),
                                    daysWorked = reader.IsDBNull(5)
                                        ? null
                                        : (int?)reader.GetInt32(5),
                                    totalOT = reader.IsDBNull(6)
                                        ? (decimal?)null
                                        : Convert.ToDecimal(reader.GetValue(6)),
                                    status = reader["Status"]?.ToString(),
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
                    }
                }
            }

            return Ok(results);
        }

        // ✅ GET: Summary รายเดือน (MonthlySummary)
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

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query =
                    @"
                    SELECT 
                        eicc.MonthYear,
                        ISNULL(SUM(eicc.TotalOT), 0) AS TotalOT,
                        ISNULL(SUM(eicc.TotalHours), 0) AS TotalHours,
                        COUNT(DISTINCT eicc.EmpID) AS EmployeeCount
                    FROM EICC_Control eicc
                    JOIN EmployeeInfo ei ON eicc.EmpID = ei.EmpID
                    WHERE 1=1";

                if (!string.IsNullOrEmpty(division))
                    query += " AND ei.Division = @division";
                if (!string.IsNullOrEmpty(department))
                    query += " AND ei.Department = @department";
                if (!string.IsNullOrEmpty(section))
                    query += " AND ei.Section = @section";
                if (!string.IsNullOrEmpty(biz))
                    query += " AND ei.Biz = @biz";
                if (!string.IsNullOrEmpty(process))
                    query += " AND ei.Process = @process";

                query +=
                    @"
                    GROUP BY eicc.MonthYear
                    ORDER BY eicc.MonthYear";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@division", (object)division ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@department", (object)department ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@section", (object)section ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@biz", (object)biz ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@process", (object)process ?? DBNull.Value);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var totalOT = reader.IsDBNull(1)
                                ? 0.0
                                : Convert.ToDouble(reader.GetValue(1));
                            var totalHours = reader.IsDBNull(2)
                                ? 0.0
                                : Convert.ToDouble(reader.GetValue(2));
                            results.Add(
                                new
                                {
                                    month = reader["MonthYear"].ToString(),
                                    // totalOT = reader.IsDBNull(reader.GetOrdinal("TotalOT"))
                                    //     ? 0
                                    //     : Convert.ToDouble(reader["TotalOT"]),
                                    // totalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours"))
                                    //     ? 0
                                    //     : Convert.ToDouble(reader["TotalHours"]),
                                    // totalOT = reader.IsDBNull(1) ? 0m : reader.GetDecimal(1),
                                    // totalHours = reader.IsDBNull(2) ? 0m : reader.GetDecimal(2),
                                    totalOT = Math.Round(totalOT, 2),
                                    totalHours = Math.Round(totalHours, 2),
                                    employeeCount = Convert.ToInt32(reader["EmployeeCount"]),
                                }
                            );
                        }
                    }
                }
            }

            return Ok(results);
        }
    }
}
