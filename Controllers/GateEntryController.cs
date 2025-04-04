// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class GateEntryController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public GateEntryController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ดึงข้อมูลจากทั้ง GateEntry, EmployeeInfo, OJTandInspectionSkill, CleanroomEntry โดยใช้ EmpID
//         [HttpGet]
//         public async Task<IActionResult> GetGateEntry()
//         {
//             var combinedData = await (from gate in _context.GateEntry
//                                       join employee in _context.EmployeeInfo
//                                       on gate.EmpID.ToString() equals employee.EmpID.ToString() // เชื่อมต่อ EmpID ของ GateEntry และ EmployeeInfo
//                                       join ojt in _context.OJTandInspectionSkill
//                                       on gate.EmpID equals ojt.EmpID // เชื่อมต่อ EmpID ของ GateEntry และ OJTandInspectionSkill
//                                       join cleanroom in _context.CleanroomEntry
//                                       on gate.EmpID equals cleanroom.EmpID // เชื่อมต่อ EmpID ของ GateEntry และ CleanroomEntry
//                                       select new 
//                                       {
//                                           EmpID = gate.EmpID,
//                                           FirstName = employee.FirstName,
//                                           LastName = employee.LastName,
//                                           Division = employee.Division,
//                                           Department = employee.Department,
//                                           Position = employee.Position,
//                                           Email = employee.Email,
//                                           ShiftCode = employee.ShiftCode,
//                                             Section = employee.Section,
//                                           EntryDateTime = gate.EntryDateTime,
//                                           ExitDateTime = gate.ExitDateTime,
//                                           GateNo = gate.GateNo,
//                                           GateStatus = gate.GateStatus,
//                                           Biz = ojt.Biz, // ข้อมูลจาก OJTandInspectionSkill
//                                           Process = ojt.Process, // ข้อมูลจาก OJTandInspectionSkill
//                                           CourseGroup = ojt.CourseGroup, // ข้อมูลจาก OJTandInspectionSkill
//                                           WorkGroup = ojt.SkillGroup, // ข้อมูลจาก OJTandInspectionSkill
//                                           CStatus = cleanroom.CStatus, // ข้อมูลจาก CleanroomEntry
//                                           CheckInDateTime = cleanroom.CheckInDateTime, // ข้อมูลจาก CleanroomEntry
//                                           CheckOutDateTime = cleanroom.CheckOutDateTime, // ข้อมูลจาก CleanroomEntry
//                                           Status = (cleanroom.CStatus == "OUT" && gate.GateStatus == "OUT") ? "status-missing" :
//                                            (cleanroom.CStatus == "OUT" && gate.GateStatus == "IN") ? "status-out-cleanroom" :
//                                            (cleanroom.CStatus == "IN" && gate.GateStatus == "IN") ? "status-in-cleanroom" :
//                                            "status-unknown", // กรณีอื่นๆ

//                                       }).ToListAsync();

//             // ส่งข้อมูลกลับไปยัง Frontend ในรูปแบบ JSON
//             return Ok(combinedData);
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateEntryController : ControllerBase
    {
        private readonly string _connectionString;

        public GateEntryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/GateEntry
        [HttpGet]
        public async Task<IActionResult> GetAllGateEntries()
        {
            var gateEntries = new List<GateEntry>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM GateEntry";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        gateEntries.Add(new GateEntry
                        {
                            GateEntryID = reader.GetInt32(reader.GetOrdinal("GateEntryID")),
                            EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                            EntryDateTime = reader.IsDBNull(reader.GetOrdinal("EntryDateTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EntryDateTime")),
                            ExitDateTime = reader.IsDBNull(reader.GetOrdinal("ExitDateTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExitDateTime")),
                            GateNo = reader["GateNo"]?.ToString(),
                            Room = reader["Room"]?.ToString(),
                            GateStatus = reader["GateStatus"]?.ToString()
                        });
                    }
                }
            }

            return Ok(gateEntries);
        }

        // GET: api/GateEntry/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetGateEntryById(int id)
        {
            GateEntry gateEntry = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM GateEntry WHERE GateEntryID = @GateEntryID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GateEntryID", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            gateEntry = new GateEntry
                            {
                                GateEntryID = reader.GetInt32(reader.GetOrdinal("GateEntryID")),
                                EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                                EntryDateTime = reader.IsDBNull(reader.GetOrdinal("EntryDateTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("EntryDateTime")),
                                ExitDateTime = reader.IsDBNull(reader.GetOrdinal("ExitDateTime")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("ExitDateTime")),
                                GateNo = reader["GateNo"]?.ToString(),
                                Room = reader["Room"]?.ToString(),
                                GateStatus = reader["GateStatus"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (gateEntry == null)
                return NotFound("Gate entry not found");

            return Ok(gateEntry);
        }

        // POST: api/GateEntry
        [HttpPost]
        public async Task<IActionResult> CreateGateEntry([FromBody] GateEntry gateEntry)
        {
            if (gateEntry == null)
                return BadRequest("Invalid gate entry");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO GateEntry 
                                (EmpID, EntryDateTime, ExitDateTime, GateNo, Room, GateStatus)
                                 VALUES 
                                (@EmpID, @EntryDateTime, @ExitDateTime, @GateNo, @Room, @GateStatus)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", gateEntry.EmpID);
                    cmd.Parameters.AddWithValue("@EntryDateTime", (object?)gateEntry.EntryDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitDateTime", (object?)gateEntry.ExitDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GateNo", gateEntry.GateNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Room", gateEntry.Room ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GateStatus", gateEntry.GateStatus ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Gate entry created");
        }

        // PUT: api/GateEntry/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateGateEntry(int id, [FromBody] GateEntry gateEntry)
        {
            if (id != gateEntry.GateEntryID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"UPDATE GateEntry SET 
                                    EmpID = @EmpID,
                                    EntryDateTime = @EntryDateTime,
                                    ExitDateTime = @ExitDateTime,
                                    GateNo = @GateNo,
                                    Room = @Room,
                                    GateStatus = @GateStatus
                                WHERE GateEntryID = @GateEntryID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GateEntryID", gateEntry.GateEntryID);
                    cmd.Parameters.AddWithValue("@EmpID", gateEntry.EmpID);
                    cmd.Parameters.AddWithValue("@EntryDateTime", (object?)gateEntry.EntryDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ExitDateTime", (object?)gateEntry.ExitDateTime ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@GateNo", gateEntry.GateNo ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Room", gateEntry.Room ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@GateStatus", gateEntry.GateStatus ?? (object)DBNull.Value);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("Gate entry not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/GateEntry/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGateEntry(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "DELETE FROM GateEntry WHERE GateEntryID = @GateEntryID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@GateEntryID", id);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("Gate entry not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;
// using Newtonsoft.Json.Linq;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class GateEntryController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public GateEntryController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetGateEntry()
//         {
//             try
//             {
//                 var query = @"
//                     SELECT
//                         g.EmpID,
//                         e.FirstName,
//                         e.LastName,
//                         e.Division,
//                         e.Department,
//                         e.Position,
//                         e.Email,
//                         e.ShiftCode,
//                         e.Section,
//                         g.EntryDateTime,
//                         g.ExitDateTime,
//                         g.GateNo,
//                         g.GateStatus,
//                         o.Biz,
//                         o.Process,
//                         o.CourseGroup,
//                         o.SkillGroup as WorkGroup,
//                         c.CStatus,
//                         c.CheckInDateTime,
//                         c.CheckOutDateTime,
//                         CASE 
//                             WHEN c.CStatus = 'OUT' AND g.GateStatus = 'OUT' THEN 'status-missing'
//                             WHEN c.CStatus = 'OUT' AND g.GateStatus = 'IN' THEN 'status-out-cleanroom'
//                             WHEN c.CStatus = 'IN' AND g.GateStatus = 'IN' THEN 'status-in-cleanroom'
//                             ELSE 'status-unknown'
//                         END as Status
//                     FROM GateEntry g
//                     JOIN EmployeeInfo e ON CAST(g.EmpID AS NVARCHAR) = CAST(e.EmpID AS NVARCHAR)
//                     JOIN OJTandInspectionSkill o ON g.EmpID = o.EmpID
//                     JOIN CleanroomEntry c ON g.EmpID = c.EmpID
//                 ";

//                 var result = _db.Query(query);
//                 return _res.Success(result);
//             }
//             catch (System.Exception ex)
//             {
//                 return _res.InternalServerError(ex.ToString());
//             }
//         }
//     }
// }
