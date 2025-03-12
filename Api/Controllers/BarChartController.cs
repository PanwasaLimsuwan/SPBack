using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BarChartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public BarChartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันเพื่อดึงข้อมูลจากฐานข้อมูล
        [HttpGet]
        public async Task<IActionResult> GetBarChartData()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่ใช้ดึงข้อมูลจากฐานข้อมูล "ManpowerReg"
            string query = @"
                SELECT 
                    e.Process, 
                    e.SkillGroup, 
                    COUNT(e.MPRID) AS EmployeeCount
                FROM ManpowerReq e
                GROUP BY e.Process, e.SkillGroup
                ORDER BY e.Process, e.SkillGroup;
            ";

            List<object> resultList = new List<object>();

            try
            {
                // เปิดการเชื่อมต่อฐานข้อมูล
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        // อ่านข้อมูลที่ได้รับ
                        while (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                Process = reader["Process"],
                                SkillGroup = reader["SkillGroup"],
                                EmployeeCount = reader["EmployeeCount"]
                            };
                            resultList.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // หากเกิดข้อผิดพลาดในการเชื่อมต่อหรือดึงข้อมูล
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            // หากไม่มีข้อมูล
            if (resultList.Count == 0)
            {
                return NotFound("No data found.");
            }

            // ส่งข้อมูลในรูปแบบ JSON
            return Ok(resultList);
        }
    }
}
