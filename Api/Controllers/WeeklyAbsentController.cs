using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeeklyAbsentController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public WeeklyAbsentController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ดึงข้อมูลจำนวนการขาดงานในแต่ละสัปดาห์จากฐานข้อมูล
        [HttpGet]
        public async Task<IActionResult> GetWeeklyAbsentData()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");
            string query = @"
                SELECT 
                    WeekNumber,
                    COUNT(*) AS Absences
                FROM Attendance
                WHERE Status = 'Absent'
                GROUP BY WeekNumber
                ORDER BY WeekNumber;
            ";

            List<object> resultList = new List<object>();

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                WeekNumber = reader["WeekNumber"],
                                Absences = reader["Absences"]
                            };
                            resultList.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            // เพิ่มข้อมูล { "weekNumber": 0, "absences": 0 } ที่ตำแหน่งแรก
            resultList.Insert(0, new { WeekNumber = 0, Absences = 0 });

            return Ok(resultList);
        }
    }
}
