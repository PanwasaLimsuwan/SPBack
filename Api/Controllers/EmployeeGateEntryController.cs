using Microsoft.AspNetCore.Mvc;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeGateEntryController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        // Constructor ที่รับ IConfiguration เพื่อใช้ในการเข้าถึงการตั้งค่าฐานข้อมูล
        public EmployeeGateEntryController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันที่ใช้ ADO.NET เพื่อดึงข้อมูลจาก EmployeeInfo, GateEntry, CleanroomEntry, และ OJTandInspectionSkill
        [HttpGet]
        public async Task<IActionResult> GetEmployeeWithGateEntry()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่จะใช้ในการ JOIN ตาราง EmployeeInfo, GateEntry, CleanroomEntry, OJTandInspectionSkill
            string query = @"
                SELECT 
                    e.EmpID, 
                    e.FirstName, 
                    e.LastName, 
                    g.GateNo, 
                    e.Division, 
                    e.Department, 
                    e.Position, 
                    c.CStatus, 
                    c.CheckInDateTime,
                    o.Biz, 
                    o.Process, 
                    STRING_AGG(o.CourseGroup, ',') AS CourseGroup
                FROM EmployeeInfo e
                INNER JOIN GateEntry g ON e.EmpID = CAST(g.EmpID AS INT)
                LEFT JOIN CleanroomEntry c ON e.EmpID = c.EmpID
                LEFT JOIN OJTandInspectionSkill o ON e.EmpID = o.EmpID
                GROUP BY e.EmpID, e.FirstName, e.LastName, g.GateNo, e.Division, e.Department, e.Position, c.CStatus, c.CheckInDateTime, o.Biz, o.Process;";

            List<object> resultList = new List<object>();

            try
            {
                // เปิดการเชื่อมต่อ
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

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
                                CheckInDateTime = reader["CheckInDateTime"],
                                CStatus = reader["CStatus"],
                                GateNo = reader["GateNo"],
                                Division = reader["Division"],
                                Department = reader["Department"],
                                Biz = reader["Biz"],
                                Process = reader["Process"],
                                Position = reader["Position"],
                                CourseGroup = reader["CourseGroup"] // รวมค่า CourseGroup
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
