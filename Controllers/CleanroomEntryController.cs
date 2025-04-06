using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

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

                string query = @"
                    SELECT 
                        c.CEntryID, c.EmpID, c.CheckInDateTime, c.CheckOutDateTime, 
                        c.LocationStatus, c.HeadCountDate, c.CStatus,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM CleanroomEntry c
                    JOIN EmployeeInfo e ON c.EmpID = CAST(e.EmpID AS VARCHAR)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new
                        {
                            cEntryID = Convert.ToInt32(reader["CEntryID"]),
                            empID = reader["EmpID"].ToString(),
                            checkInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckInDateTime"],
                            checkOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckOutDateTime"],
                            locationStatus = reader["LocationStatus"]?.ToString(),
                            headCountDate = reader["HeadCountDate"] == DBNull.Value ? null : (DateTime?)reader["HeadCountDate"],
                            cStatus = reader["CStatus"]?.ToString(),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString()
                        });
                    }
                }
            }

            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCleanroomEntryById(int id)
        {
            object entry = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    SELECT 
                        c.CEntryID, c.EmpID, c.CheckInDateTime, c.CheckOutDateTime, 
                        c.LocationStatus, c.HeadCountDate, c.CStatus,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM CleanroomEntry c
                    JOIN EmployeeInfo e ON c.EmpID = CAST(e.EmpID AS VARCHAR)
                    WHERE c.CEntryID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            entry = new
                            {
                                cEntryID = Convert.ToInt32(reader["CEntryID"]),
                                empID = reader["EmpID"].ToString(),
                                checkInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckInDateTime"],
                                checkOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckOutDateTime"],
                                locationStatus = reader["LocationStatus"]?.ToString(),
                                headCountDate = reader["HeadCountDate"] == DBNull.Value ? null : (DateTime?)reader["HeadCountDate"],
                                cStatus = reader["CStatus"]?.ToString(),
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (entry == null)
                return NotFound();

            return Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCleanroomEntry([FromBody] CleanroomEntry entry)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    INSERT INTO CleanroomEntry 
                        (EmpID, CheckInDateTime, CheckOutDateTime, LocationStatus, HeadCountDate, CStatus)
                    VALUES 
                        (@EmpID, @CheckInDateTime, @CheckOutDateTime, @LocationStatus, @HeadCountDate, @CStatus)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", entry.EmpID);
                    cmd.Parameters.AddWithValue("@CheckInDateTime", (object?)entry.CheckInDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckOutDateTime", (object?)entry.CheckOutDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationStatus", (object?)entry.LocationStatus ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HeadCountDate", (object?)entry.HeadCountDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CStatus", entry.CStatus);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Inserted successfully");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCleanroomEntry(int id, [FromBody] CleanroomEntry entry)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = @"
                    UPDATE CleanroomEntry 
                    SET 
                        EmpID = @EmpID, 
                        CheckInDateTime = @CheckInDateTime, 
                        CheckOutDateTime = @CheckOutDateTime, 
                        LocationStatus = @LocationStatus, 
                        HeadCountDate = @HeadCountDate, 
                        CStatus = @CStatus 
                    WHERE CEntryID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@EmpID", entry.EmpID);
                    cmd.Parameters.AddWithValue("@CheckInDateTime", (object?)entry.CheckInDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CheckOutDateTime", (object?)entry.CheckOutDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@LocationStatus", (object?)entry.LocationStatus ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@HeadCountDate", (object?)entry.HeadCountDate ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@CStatus", entry.CStatus);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Updated successfully");
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCleanroomEntry(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                string query = "DELETE FROM CleanroomEntry WHERE CEntryID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Deleted successfully");
        }
    }
}
