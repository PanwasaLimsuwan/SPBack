using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
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
            var entries = new List<CleanroomEntry>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM CleanroomEntry";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new CleanroomEntry
                        {
                            CEntryID = Convert.ToInt32(reader["CEntryID"]),
                            EmpID = reader["EmpID"].ToString(),
                            CheckInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckInDateTime"],
                            CheckOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckOutDateTime"],
                            LocationStatus = reader["LocationStatus"].ToString(),
                            HeadCountDate = reader["HeadCountDate"] == DBNull.Value ? null : (DateTime?)reader["HeadCountDate"],
                            CStatus = reader["CStatus"].ToString()
                        });
                    }
                }
            }

            return Ok(entries);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCleanroomEntryById(int id)
        {
            CleanroomEntry entry = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM CleanroomEntry WHERE CEntryID = @id";
                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            entry = new CleanroomEntry
                            {
                                CEntryID = Convert.ToInt32(reader["CEntryID"]),
                                EmpID = reader["EmpID"].ToString(),
                                CheckInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckInDateTime"],
                                CheckOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckOutDateTime"],
                                LocationStatus = reader["LocationStatus"].ToString(),
                                HeadCountDate = reader["HeadCountDate"] == DBNull.Value ? null : (DateTime?)reader["HeadCountDate"],
                                CStatus = reader["CStatus"].ToString()
                            };
                        }
                    }
                }
            }

            return entry == null ? NotFound() : Ok(entry);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCleanroomEntry([FromBody] CleanroomEntry entry)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO CleanroomEntry (EmpID, CheckInDateTime, CheckOutDateTime, LocationStatus, HeadCountDate, CStatus)
                                 VALUES (@EmpID, @CheckInDateTime, @CheckOutDateTime, @LocationStatus, @HeadCountDate, @CStatus)";
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
                string query = @"UPDATE CleanroomEntry 
                                 SET EmpID = @EmpID, CheckInDateTime = @CheckInDateTime, CheckOutDateTime = @CheckOutDateTime, 
                                     LocationStatus = @LocationStatus, HeadCountDate = @HeadCountDate, CStatus = @CStatus 
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
