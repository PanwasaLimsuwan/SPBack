using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CalculatedWorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public CalculatedWorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = new List<CalculatedWorktime>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT WorktimeID, EmpID, Date, WorkedHours, OTHours, Status FROM CalculatedWorktime", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new CalculatedWorktime
                    {
                        WorktimeID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        WorkedHours = reader.GetDouble(3),
                        OTHours = reader.GetDouble(4),
                        Status = reader.GetString(5)
                    });
                }
            }

            return Ok(results);
        }

        // ✅ Get by EmpID
        [HttpGet("{empId}")]
        public async Task<IActionResult> GetByEmpId(string empId)
        {
            var results = new List<CalculatedWorktime>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT WorktimeID, EmpID, Date, WorkedHours, OTHours, Status FROM CalculatedWorktime WHERE EmpID = @EmpID", conn);
                cmd.Parameters.AddWithValue("@EmpID", empId);

                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new CalculatedWorktime
                    {
                        WorktimeID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        Date = reader.GetDateTime(2),
                        WorkedHours = reader.GetDouble(3),
                        OTHours = reader.GetDouble(4),
                        Status = reader.GetString(5)
                    });
                }
            }

            if (results.Count == 0)
                return NotFound($"No records found for EmpID: {empId}");

            return Ok(results);
        }
    }
}
