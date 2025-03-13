using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FullySkillController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public FullySkillController(IConfiguration configuration)
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
                { "3", 0 },
                { "Other", 0 }
            };

            try
            {
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();  // เปิดการเชื่อมต่อ

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        while (await reader.ReadAsync())
                        {
                            // ค่าทั้งหมดจากฐานข้อมูล
                            int material = (int)reader["Material"];
                            int operation = (int)reader["Operation"];
                            int machineSAB1 = (int)reader["Machine:SAB#1"];
                            int machineSAB2 = (int)reader["Machine:SAB#2"];
                            int machineSAB3 = (int)reader["Machine:SAB#3"];
                            int inspection = (int)reader["Inspection"];

                            // เช็คว่าทุกทักษะมีค่าเท่ากับ 3 หรือไม่
                            if (material == 3 && operation == 3 && machineSAB1 == 3 && machineSAB2 == 3 && machineSAB3 == 3 && inspection == 3)
                            {
                                totalCounts["3"]++; // นับค่าที่เท่ากับ 3
                            }
                            else
                            {
                                totalCounts["Other"]++; // ค่าที่ไม่เป็น 3
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            // ส่งผลลัพธ์ในรูปแบบ JSON
            return Ok(totalCounts);
        }
    }
}
