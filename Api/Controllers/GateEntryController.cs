using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateEntryController : ControllerBase
    {
        private readonly string _connectionString;

        public GateEntryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/GateEntry
        // ดึงข้อมูลจากทั้ง GateEntry, EmployeeInfo, OJTandInspectionSkill, CleanroomEntry โดยใช้ ADO.NET
        [HttpGet]
        public async Task<IActionResult> GetGateEntry()
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"
                    SELECT 
                        g.EmpID, e.FirstName, e.LastName, e.Division, e.Department, 
                        e.Position, e.Email, e.ShiftCode, e.Section,
                        g.EntryDateTime, g.ExitDateTime, g.GateNo, g.GateStatus,
                        o.Biz, o.Process, o.CourseGroup, o.SkillGroup,
                        c.CStatus, c.CheckInDateTime, c.CheckOutDateTime
                    FROM GateEntry g
                    JOIN EmployeeInfo e ON g.EmpID = e.EmpID
                    JOIN OJTandInspectionSkill o ON g.EmpID = o.EmpID
                    JOIN CleanroomEntry c ON g.EmpID = c.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var gateStatus = reader["GateStatus"]?.ToString();
                        var cStatus = reader["CStatus"]?.ToString();

                        string status = (cStatus == "OUT" && gateStatus == "OUT") ? "status-missing"
                                     : (cStatus == "OUT" && gateStatus == "IN") ? "status-out-cleanroom"
                                     : (cStatus == "IN" && gateStatus == "IN") ? "status-in-cleanroom"
                                     : "status-unknown";

                        result.Add(new
                        {
                            empID = reader["EmpID"]?.ToString(),
                            firstName = reader["FirstName"]?.ToString(),
                            lastName = reader["LastName"]?.ToString(),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            position = reader["Position"]?.ToString(),
                            email = reader["Email"]?.ToString(),
                            shiftCode = reader["ShiftCode"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            entryDateTime = reader["EntryDateTime"] == DBNull.Value ? null : ((DateTime)reader["EntryDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            exitDateTime = reader["ExitDateTime"] == DBNull.Value ? null : ((DateTime)reader["ExitDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            gateNo = reader["GateNo"]?.ToString(),
                            gateStatus = gateStatus,
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            courseGroup = reader["CourseGroup"]?.ToString(),
                            workGroup = reader["SkillGroup"]?.ToString(),
                            cStatus = cStatus,
                            checkInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : ((DateTime)reader["CheckInDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            checkOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : ((DateTime)reader["CheckOutDateTime"]).ToString("yyyy-MM-dd HH:mm:ss"),
                            status = status
                        });
                    }
                }
            }

            return Ok(result);
        }

        // GET: api/GateEntry/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            GateEntry result = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM GateEntry WHERE GateEntryID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new GateEntry
                    {
                        GateEntryID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        EntryDateTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                        ExitDateTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                        GateNo = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Room = reader.IsDBNull(5) ? null : reader.GetString(5),
                        GateStatus = reader.IsDBNull(6) ? null : reader.GetString(6),
                    };
                }
            }

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        // POST: api/GateEntry
        [HttpPost]
        public async Task<IActionResult> Create(GateEntry gate)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO GateEntry (EmpID, EntryDateTime, ExitDateTime, GateNo, Room, GateStatus)
                    VALUES (@EmpID, @EntryDateTime, @ExitDateTime, @GateNo, @Room, @GateStatus)", conn);

                cmd.Parameters.AddWithValue("@EmpID", gate.EmpID);
                cmd.Parameters.AddWithValue("@EntryDateTime", (object?)gate.EntryDateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ExitDateTime", (object?)gate.ExitDateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GateNo", (object?)gate.GateNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Room", (object?)gate.Room ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GateStatus", (object?)gate.GateStatus ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/GateEntry/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, GateEntry gate)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE GateEntry SET 
                        EmpID = @EmpID,
                        EntryDateTime = @EntryDateTime,
                        ExitDateTime = @ExitDateTime,
                        GateNo = @GateNo,
                        Room = @Room,
                        GateStatus = @GateStatus
                    WHERE GateEntryID = @GateEntryID", conn);

                cmd.Parameters.AddWithValue("@GateEntryID", id);
                cmd.Parameters.AddWithValue("@EmpID", gate.EmpID);
                cmd.Parameters.AddWithValue("@EntryDateTime", (object?)gate.EntryDateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@ExitDateTime", (object?)gate.ExitDateTime ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GateNo", (object?)gate.GateNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Room", (object?)gate.Room ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@GateStatus", (object?)gate.GateStatus ?? DBNull.Value);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/GateEntry/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM GateEntry WHERE GateEntryID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
