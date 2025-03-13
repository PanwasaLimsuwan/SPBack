using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillLevelPiChartController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public SkillLevelPiChartController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันเพื่อดึงข้อมูลทักษะของพนักงานจากฐานข้อมูลและนับจำนวนค่าที่เป็น 0, 1, 2, 3
        [HttpGet]
        public async Task<IActionResult> GetEmployeeSkills()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่จะใช้ในการดึงข้อมูลจากฐานข้อมูล "OJTandInspectionSkill"
            string query = @"
                SELECT 
                    e.EmpID, 
                    e.FirstName, 
                    e.LastName, 
                    MAX(CASE WHEN o.SkillGroup = 'Material' THEN o.Active ELSE 0 END) AS Material,
                    MAX(CASE WHEN o.SkillGroup = 'Operation' THEN o.Active ELSE 0 END) AS Operation,
                    MAX(CASE WHEN o.SkillGroup = 'Machine:SAB#1' THEN o.Active ELSE 0 END) AS 'Machine:SAB#1',
                    MAX(CASE WHEN o.SkillGroup = 'Machine:SAB#2' THEN o.Active ELSE 0 END) AS 'Machine:SAB#2',
                    MAX(CASE WHEN o.SkillGroup = 'Machine:SAB#3' THEN o.Active ELSE 0 END) AS 'Machine:SAB#3',
                    MAX(CASE WHEN o.SkillGroup = 'Inspection' THEN o.Active ELSE 0 END) AS Inspection
                FROM EmployeeInfo e
                INNER JOIN OJTandInspectionSkill o ON e.EmpID = o.EmpID
                GROUP BY e.EmpID, e.FirstName, e.LastName
                ORDER BY e.EmpID;
            ";

            // ตัวแปรเพื่อเก็บการนับค่ารวมทั้งหมด
            var totalCounts = new Dictionary<string, int>
            {
                { "0", 0 },
                { "1", 0 },
                { "2", 0 },
                { "3", 0 }
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();  // เปิดการเชื่อมต่อ

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        // อ่านข้อมูลจากฐานข้อมูล
                        while (await reader.ReadAsync())
                        {
                            // นับค่าของทักษะต่างๆ
                            CountAndUpdateTotals(totalCounts, (int)reader["Material"]);
                            CountAndUpdateTotals(totalCounts, (int)reader["Operation"]);
                            CountAndUpdateTotals(totalCounts, (int)reader["Machine:SAB#1"]);
                            CountAndUpdateTotals(totalCounts, (int)reader["Machine:SAB#2"]);
                            CountAndUpdateTotals(totalCounts, (int)reader["Machine:SAB#3"]);
                            CountAndUpdateTotals(totalCounts, (int)reader["Inspection"]);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // หากเกิดข้อผิดพลาดในการเชื่อมต่อหรือดึงข้อมูล
                return StatusCode(500, "Internal server error: " + ex.Message);  // ส่งข้อผิดพลาดกลับไป
            }

            // ส่งผลลัพธ์ที่นับรวม
            return Ok(totalCounts);
        }

        // ฟังก์ชันที่ใช้ในการนับและอัพเดทจำนวนค่าของแต่ละทักษะ
        private void CountAndUpdateTotals(Dictionary<string, int> totalCounts, int skillValue)
        {
            // เพิ่มจำนวนค่าที่เป็น 0, 1, 2, 3 ใน Dictionary
            if (skillValue == 0)
                totalCounts["0"]++;
            else if (skillValue == 1)
                totalCounts["1"]++;
            else if (skillValue == 2)
                totalCounts["2"]++;
            else if (skillValue == 3)
                totalCounts["3"]++;
        }
    }
}
