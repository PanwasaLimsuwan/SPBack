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
    public class ManpowerPlanController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerPlanController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: api/ManpowerPlan (พร้อม filters)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var plans = new List<object>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ✅ ไม่มี Biz/Process ใน table นี้ ดึงตรงๆ ไม่ต้อง JOIN
            var query =
                @"
        SELECT PlanID, Date, ShiftCode, Shift, PlannedHeadcount, ActualHeadcount
        FROM ManpowerPlan
        ORDER BY Date, Shift";

            using var cmd = new SqlCommand(query, conn);
            cmd.CommandTimeout = 30;
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                plans.Add(
                    new
                    {
                        planID = reader.GetInt32(0),
                        date = reader.GetDateTime(1).ToString("yyyy-MM-dd"),
                        shiftCode = reader.IsDBNull(2) ? null : reader.GetString(2),
                        shift = reader.IsDBNull(3) ? null : reader.GetString(3),
                        plannedHeadcount = reader.IsDBNull(4) ? 0 : reader.GetInt32(4),
                        actualHeadcount = reader.IsDBNull(5) ? 0 : reader.GetInt32(5),
                    }
                );
            }
            return Ok(plans);
        }
    }
}
