// using Microsoft.AspNetCore.Mvc;
// using System.Data.SqlClient;
// using Microsoft.Extensions.Configuration;
// using System.Threading.Tasks;
// using System.Collections.Generic;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class FullySkilledPieChartController : ControllerBase
//     {
//         private readonly IConfiguration _configuration;

//         public FullySkilledPieChartController(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetFullySkilledData()
//         {
//             string connectionString = _configuration.GetConnectionString("DefaultConnection");

//             // SQL Query เพื่อดึงข้อมูลจากฐานข้อมูล
//             string query = @"
//                 SELECT SkillGroup, COUNT(*) AS SkillCount
//                 FROM OJTandInspectionSkill
//                 WHERE Active = 3
//                 GROUP BY SkillGroup
//             ";

//             // ตัวแปรเพื่อเก็บข้อมูลที่ดึงมาจากฐานข้อมูล
//             Dictionary<string, int> skillData = new Dictionary<string, int>();

//             try
//             {
//                 using (SqlConnection conn = new SqlConnection(connectionString))
//                 {
//                     await conn.OpenAsync();  // เปิดการเชื่อมต่อ

//                     using (SqlCommand cmd = new SqlCommand(query, conn))
//                     {
//                         SqlDataReader reader = await cmd.ExecuteReaderAsync();

//                         // อ่านข้อมูลจากฐานข้อมูล
//                         while (await reader.ReadAsync())
//                         {
//                             string skillGroup = reader["SkillGroup"].ToString();
//                             int skillCount = (int)reader["SkillCount"];
//                             skillData[skillGroup] = skillCount;
//                         }
//                     }
//                 }
//             }
//             catch (Exception ex)
//             {
//                 // หากเกิดข้อผิดพลาดในการเชื่อมต่อหรือดึงข้อมูล
//                 return StatusCode(500, "Internal server error: " + ex.Message);  // ส่งข้อผิดพลาดกลับไป
//             }

//             // คำนวณข้อมูลที่ต้องการส่งกลับไป
//             var totalSkills = skillData.Values.Sum();
//             var fullySkilled = skillData.ContainsKey("Fully Skilled") ? skillData["Fully Skilled"] : 0;
//             var needTraining = totalSkills - fullySkilled;

//             var result = new
//             {
//                 values = new[] { fullySkilled, needTraining },
//                 labels = new[] { "Fully Skilled", "Need Training" }
//             };

//             return Ok(result);
//         }
//     }
// }
