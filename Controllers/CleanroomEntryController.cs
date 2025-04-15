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
                        c.CEntryID, c.EmpID, c.CheckInDateTime, c.CheckOutDateTime, c.CStatus,
                        e.Division, e.Department, e.Section, e.Biz, e.Process
                    FROM CleanroomEntry c
                    JOIN EmployeeInfo e ON c.EmpID = e.EmpID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        entries.Add(new
                        {
                            cEntryID = Convert.ToInt32(reader["CEntryID"]),
                            EmpID = Convert.ToInt32(reader["EmpID"]),
                            checkInDateTime = reader["CheckInDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckInDateTime"],
                            checkOutDateTime = reader["CheckOutDateTime"] == DBNull.Value ? null : (DateTime?)reader["CheckOutDateTime"],
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
    }
}
