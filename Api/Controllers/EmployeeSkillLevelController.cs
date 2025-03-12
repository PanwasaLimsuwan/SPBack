using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeSkillController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeSkillController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันเพื่อดึงข้อมูลทักษะของพนักงานจากฐานข้อมูล
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

            List<object> resultList = new List<object>();

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
                            var result = new
                            {
                                EmpID = reader["EmpID"],
                                FirstName = reader["FirstName"],
                                LastName = reader["LastName"],
                                Material = reader["Material"],  // ค่า Material
                                Operation = reader["Operation"],  // ค่า Operation
                                Machine_SAB1 = reader["Machine:SAB#1"],  // ค่า Machine:SAB#1
                                Machine_SAB2 = reader["Machine:SAB#2"],  // ค่า Machine:SAB#2
                                Machine_SAB3 = reader["Machine:SAB#3"],  // ค่า Machine:SAB#3
                                Inspection = reader["Inspection"]  // ค่า Inspection
                            };
                            resultList.Add(result);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // หากเกิดข้อผิดพลาดในการเชื่อมต่อหรือดึงข้อมูล
                return StatusCode(500, "Internal server error: " + ex.Message);  // ส่งข้อผิดพลาดกลับไป
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
