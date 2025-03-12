using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecEmployeeController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Constructor ที่รับ IConfiguration เพื่อใช้ในการเข้าถึงการตั้งค่าฐานข้อมูล
        public RecEmployeeController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันที่ใช้ ADO.NET เพื่อดึงข้อมูลจาก EmployeeInfo, OJTandInspectionSkill, WorkTime
        [HttpGet]
        public async Task<IActionResult> GetEmployeeWithRec()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่จะใช้ในการ JOIN ตาราง EmployeeInfo, OJTandInspectionSkill และ WorkTime
            string query = @"
                SELECT 
                    e.EmpID, 
                    e.FirstName, 
                    e.LastName,   
                    w.WorkedHours,
                    o.SkillGroup
                FROM EmployeeInfo e
                INNER JOIN OJTandInspectionSkill o ON e.EmpID = CAST(o.EmpID AS INT)
                LEFT JOIN WorkTime w ON e.EmpID = CAST(w.EmpID AS INT);";

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
                                SkillGroup = reader["SkillGroup"],  // ดึงข้อมูล SkillGroup จาก OJTandInspectionSkill
                                WorkedHours = reader["WorkedHours"],  // ดึงข้อมูล WorkedHours จาก WorkTime
                            };
                            resultList.Add(result);  // เพิ่มข้อมูลลงในรายการ
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // หากเกิดข้อผิดพลาดในการเชื่อมต่อหรือดึงข้อมูล
                return StatusCode(500, "Internal server error: " + ex.Message);  // ส่งข้อผิดพลาดกลับไป
            }

            // ตรวจสอบว่าได้ข้อมูลหรือไม่
            if (resultList.Count == 0)
            {
                return NotFound("No data found.");  // ถ้าไม่มีข้อมูล, ส่ง HTTP 404
            }

            // ส่งข้อมูลกลับไปในรูปแบบ JSON
            return Ok(resultList);
        }
    }
}
