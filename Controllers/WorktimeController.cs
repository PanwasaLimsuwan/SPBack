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
    public class WorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public WorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ Get All
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = new List<Worktime>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var cmd = new SqlCommand("SELECT WorktimeID, EmpID, Date, WorkedHours, OTHours, Status FROM Worktime", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new Worktime
                    {
                        WorktimeID = reader.GetInt32(0),
                        EmpID = reader.GetInt32(1),
                        Date = reader.GetDateTime(2),
                        WorkedHours = reader.GetDouble(3),
                        OTHours = reader.GetDouble(4),
                        Status = reader.GetString(5)
                    });
                }
            }

            return Ok(results);
        }
    }
}