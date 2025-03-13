using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleanroomEntryCountController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public CleanroomEntryCountController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // ฟังก์ชันดึงข้อมูล In Cleanroom, Out Cleanroom, ขาดงาน, จำนวนพนักงานทั้งหมด และรวม Require จากตาราง ManpowerReq
        [HttpGet]
        public async Task<IActionResult> GetCleanroomStatus()
        {
            string connectionString = _configuration.GetConnectionString("DefaultConnection");

            // SQL Query ที่ใช้ในการดึงข้อมูล "IN", "OUT", "Absent" และนับจำนวนพนักงานทั้งหมดจากตาราง EmployeeInfo รวมถึงการรวม Require จาก ManpowerReq
            string query = @"
                SELECT 
                    CleanroomEntry.CStatus, 
                    COUNT(*) AS Count
                FROM 
                    CleanroomEntry
                LEFT JOIN Attendance ON CleanroomEntry.EmpID = Attendance.EmpID
                WHERE 
                    CleanroomEntry.CStatus IN ('IN', 'OUT')
                GROUP BY 
                    CleanroomEntry.CStatus
                UNION ALL
                SELECT 
                    'Absent' AS CStatus, 
                    COUNT(*) AS Count
                FROM 
                    Attendance
                WHERE 
                    Attendance.Status = 'Absent'
                UNION ALL
                -- จำนวนพนักงานทั้งหมดจาก EmployeeInfo
                SELECT 
                    'Total Employees' AS CStatus, 
                    COUNT(*) AS Count
                FROM 
                    EmployeeInfo
                UNION ALL
                -- รวม Require จาก ManpowerReq
                SELECT 
                    'Total Require' AS CStatus, 
                    SUM(Require) AS Count
                FROM 
                    ManpowerReq
            ";

            // เริ่มต้น dictionary สำหรับผลลัพธ์ที่ต้องการ
            Dictionary<string, int> result = new Dictionary<string, int>()
            {
                { "In Cleanroom", 0 },
                { "Out Cleanroom", 0 },
                { "Absent", 0 },
                { "Total Employees", 0 },
                { "Total Require", 0 }
            };

            try
            {
                // เปิดการเชื่อมต่อกับฐานข้อมูล
                using (SqlConnection conn = new SqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    using (SqlCommand cmd = new SqlCommand(query, conn))
                    {
                        SqlDataReader reader = await cmd.ExecuteReaderAsync();

                        // อ่านข้อมูลจากฐานข้อมูล
                        while (await reader.ReadAsync())
                        {
                            string status = reader["CStatus"].ToString();
                            int count = (int)reader["Count"];

                            // เก็บข้อมูลตาม CStatus
                            if (status == "IN")
                            {
                                result["In Cleanroom"] = count;
                            }
                            else if (status == "OUT")
                            {
                                result["Out Cleanroom"] = count;
                            }
                            else if (status == "Absent")
                            {
                                result["Absent"] = count;
                            }
                            else if (status == "Total Employees")
                            {
                                result["Total Employees"] = count;
                            }
                            else if (status == "Total Require")
                            {
                                result["Total Require"] = count;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }

            // ส่งผลลัพธ์กลับเป็น JSON
            return Ok(result);
        }
    }
}
