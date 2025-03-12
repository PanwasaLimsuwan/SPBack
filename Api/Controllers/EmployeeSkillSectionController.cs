using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeSkillSectionController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public EmployeeSkillSectionController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันเพื่อดึงข้อมูลพนักงานและทักษะของพนักงาน
        [HttpGet("{empId}")]
        public async Task<IActionResult> GetEmployeeSkill(int empId)
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่จะใช้ในการดึงข้อมูลจากฐานข้อมูล "EmployeeInfo" และ "OJTandInspectionSkill"
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
                LEFT JOIN OJTandInspectionSkill o ON e.EmpID = o.EmpID
                WHERE e.EmpID = @EmpID
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
                        cmd.Parameters.AddWithValue("@EmpID", empId);
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        // อ่านข้อมูลจากฐานข้อมูล
                        if (await reader.ReadAsync())
                        {
                            var result = new
                            {
                                EmpID = reader["EmpID"],
                                FirstName = reader["FirstName"],
                                LastName = reader["LastName"],
                                Material = reader["Material"],
                                Operation = reader["Operation"],
                                Machine_SAB1 = reader["Machine:SAB#1"],
                                Machine_SAB2 = reader["Machine:SAB#2"],
                                Machine_SAB3 = reader["Machine:SAB#3"],
                                Inspection = reader["Inspection"]
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
            return Ok(resultList[0]);
        }
    }
}
