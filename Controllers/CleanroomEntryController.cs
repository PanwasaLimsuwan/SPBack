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
    public class CleanroomEntryController : ControllerBase
    {
        private readonly string _connectionString;

        public CleanroomEntryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAllCleanroomEntries()
        {
            var entries = new List<object>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query =
                    @"
                    SELECT 
                        c.CEntryID, c.EmpID, c.CheckInDateTime, c.CheckOutDateTime, c.CStatus,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM CleanroomEntry c
                    JOIN EmployeeInfo e ON c.EmpID = e.EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(
                            new
                            {
                                cEntryID = Convert.ToInt32(reader["CEntryID"]),
                                EmpID = Convert.ToInt32(reader["EmpID"]),
                                checkInDateTime = reader["CheckInDateTime"] == DBNull.Value
                                    ? null
                                    : (DateTime?)reader["CheckInDateTime"],
                                checkOutDateTime = reader["CheckOutDateTime"] == DBNull.Value
                                    ? null
                                    : (DateTime?)reader["CheckOutDateTime"],
                                cStatus = reader["CStatus"]?.ToString(),
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString(),
                            }
                        );
                    }
                }
            }

            return Ok(entries);
        }

        // GET: api/CleanroomEntry/active
        // ดึงเฉพาะคนที่ยังอยู่ใน Cleanroom (ยังไม่ checkout)
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveInCleanroom()
        {
            var entries = new List<object>();

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ ดึงเฉพาะคนที่ยัง CheckOut ไม่ได้ = ยังอยู่ใน Cleanroom
            var cmd = new SqlCommand(
                @"
        SELECT 
            c.CEntryID,
            c.EmpID,
            c.CheckInDateTime,
            c.CStatus,
            e.ShiftCode,
            e.Division,
            e.Department,
            e.Section,
            e.Biz,
            e.Process
        FROM CleanroomEntry c
        JOIN EmployeeInfo e ON c.EmpID = e.EmpID
        WHERE c.CheckOutDateTime IS NULL
        AND c.CStatus NOT IN ('Out', 'Completed')
    ",
                conn
            );

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(
                    new
                    {
                        cEntryID = Convert.ToInt32(reader["CEntryID"]),
                        empID = Convert.ToInt32(reader["EmpID"]),
                        checkInDateTime = reader["CheckInDateTime"] == DBNull.Value
                            ? null
                            : (DateTime?)reader["CheckInDateTime"],
                        cStatus = reader["CStatus"]?.ToString(),
                        shiftCode = reader["ShiftCode"]?.ToString(),
                        division = reader["Division"]?.ToString(),
                        department = reader["Department"]?.ToString(),
                        section = reader["Section"]?.ToString(),
                        biz = reader["Biz"]?.ToString(),
                        process = reader["Process"]?.ToString(),
                    }
                );
            }

            return Ok(entries);
        }
    }
}
