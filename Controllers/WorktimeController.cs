// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class WorktimeController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public WorktimeController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetWorktimes()
//         {
//             var worktimes = await _context.Worktime.ToListAsync();
//             return Ok(worktimes);
//         }

//         [HttpPost("update-worktime")]
//         public async Task<IActionResult> UpdateWorktime()
//         {
//             var gateEntries = await _context.GateEntry
//                 .Where(ge => ge.GateStatus == "OUT" && ge.EntryDateTime != null && ge.ExitDateTime != null)
//                 .ToListAsync();

//             foreach (var entry in gateEntries)
//             {
//                 var workedHours = (entry.ExitDateTime - entry.EntryDateTime)?.TotalHours;
//                 var targetDate = entry.ExitDateTime?.Date;

//                 var existingWorktime = await _context.Worktime
//                     .FirstOrDefaultAsync(w => w.EmpID == entry.EmpID && w.Date == targetDate);

//                 if (existingWorktime != null)
//                 {
//                     existingWorktime.WorkedHours = (float?)workedHours;
//                     existingWorktime.Status = "Active";
//                     _context.Worktime.Update(existingWorktime);
//                 }
//                 else
//                 {
//                     var worktime = new Worktime
//                     {
//                         EmpID = entry.EmpID,
//                         Date = targetDate,
//                         WorkedHours = (float?)workedHours,
//                         Status = "Active"
//                     };
//                     _context.Worktime.Add(worktime);
//                 }
//             }

//             await _context.SaveChangesAsync();
//             return Ok("Worktime updated successfully.");
//         }

//         [HttpPost("insert-worktime")]
//         public async Task<IActionResult> InsertWorktime()
//         {
//             var gateEntries = await _context.GateEntry
//                 .Where(ge => ge.GateStatus == "OUT" && ge.EntryDateTime != null && ge.ExitDateTime != null)
//                 .ToListAsync();

//             foreach (var entry in gateEntries)
//             {
//                 var workedHours = (entry.ExitDateTime - entry.EntryDateTime)?.TotalHours;

//                 var worktime = new Worktime
//                 {
//                     EmpID = entry.EmpID,
//                     Date = entry.ExitDateTime?.Date,
//                     WorkedHours = (float?)workedHours,
//                     Status = "Active"
//                 };

//                 _context.Worktime.Add(worktime); // เพิ่มข้อมูลใหม่
//             }

//             await _context.SaveChangesAsync();
//             return Ok("Worktime inserted successfully.");
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public WorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Worktime
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = new List<Worktime>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM Worktime";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new Worktime
                        {
                            WorkTimeID = Convert.ToInt32(reader["WorkTimeID"]),
                            EmpID = Convert.ToInt32(reader["EmpID"]),
                            Date = reader["Date"] as DateTime?,
                            WorkedHours = reader["WorkedHours"] as float?,
                            OT_Hours = reader["OT_Hours"] as float?,
                            EICC_Hours = reader["EICC_Hours"] as float?,
                            OverloadHours = reader["OverloadHours"] as float?,
                            Status = reader["Status"]?.ToString()
                        });
                    }
                }
            }

            return Ok(list);
        }

        // GET: api/Worktime/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Worktime item = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM Worktime WHERE WorkTimeID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            item = new Worktime
                            {
                                WorkTimeID = Convert.ToInt32(reader["WorkTimeID"]),
                                EmpID = Convert.ToInt32(reader["EmpID"]),
                                Date = reader["Date"] as DateTime?,
                                WorkedHours = reader["WorkedHours"] as float?,
                                OT_Hours = reader["OT_Hours"] as float?,
                                EICC_Hours = reader["EICC_Hours"] as float?,
                                OverloadHours = reader["OverloadHours"] as float?,
                                Status = reader["Status"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (item == null)
                return NotFound("Worktime not found");

            return Ok(item);
        }

        // POST: api/Worktime
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Worktime model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"INSERT INTO Worktime (EmpID, Date, WorkedHours, OT_Hours, EICC_Hours, OverloadHours, Status)
                              VALUES (@EmpID, @Date, @WorkedHours, @OT_Hours, @EICC_Hours, @OverloadHours, @Status)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", model.EmpID);
                    cmd.Parameters.AddWithValue("@Date", (object)model.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkedHours", (object)model.WorkedHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OT_Hours", (object)model.OT_Hours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EICC_Hours", (object)model.EICC_Hours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OverloadHours", (object)model.OverloadHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "");

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Worktime created successfully");
        }

        // PUT: api/Worktime/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Worktime model)
        {
            if (id != model.WorkTimeID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"UPDATE Worktime SET EmpID = @EmpID, Date = @Date, WorkedHours = @WorkedHours, 
                              OT_Hours = @OT_Hours, EICC_Hours = @EICC_Hours, OverloadHours = @OverloadHours, 
                              Status = @Status WHERE WorkTimeID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    cmd.Parameters.AddWithValue("@EmpID", model.EmpID);
                    cmd.Parameters.AddWithValue("@Date", (object)model.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@WorkedHours", (object)model.WorkedHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OT_Hours", (object)model.OT_Hours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EICC_Hours", (object)model.EICC_Hours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@OverloadHours", (object)model.OverloadHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", model.Status ?? "");

                    var affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("Worktime not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/Worktime/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "DELETE FROM Worktime WHERE WorkTimeID = @id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    var affected = await cmd.ExecuteNonQueryAsync();

                    if (affected == 0)
                        return NotFound("Worktime not found");
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
// using Api.Helpers;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class WorktimeController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public WorktimeController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetWorktimes()
//         {
//             var query = "SELECT * FROM Worktime";
//             var data = _db.Query(query);
//             return _res.Success(data);
//         }

//         [HttpPost]
//         public async Task<JsonResult> UpdateWorktime()
//         {
//             var gateEntries = _db.QueryDT(@"SELECT * FROM GateEntry 
//                                              WHERE GateStatus = 'OUT' 
//                                                AND EntryDateTime IS NOT NULL 
//                                                AND ExitDateTime IS NOT NULL");

//             foreach (System.Data.DataRow entry in gateEntries.Rows)
//             {
//                 var empID = entry["EmpID"].ToString();
//                 var entryDate = System.Convert.ToDateTime(entry["EntryDateTime"]);
//                 var exitDate = System.Convert.ToDateTime(entry["ExitDateTime"]);
//                 var workedHours = (float)(exitDate - entryDate).TotalHours;
//                 var date = exitDate.Date;

//                 var exists = _db.QueryDT($"SELECT 1 FROM Worktime WHERE EmpID = '{empID}' AND Date = '{date:yyyy-MM-dd}'");
//                 if (exists.Rows.Count > 0)
//                 {
//                     _db.QueryPOST($@"UPDATE Worktime 
//                                     SET WorkedHours = {workedHours}, 
//                                         Status = 'Active' 
//                                     WHERE EmpID = '{empID}' AND Date = '{date:yyyy-MM-dd}'");
//                 }
//                 else
//                 {
//                     _db.QueryPOST($@"INSERT INTO Worktime (EmpID, Date, WorkedHours, Status)
//                                      VALUES ('{empID}', '{date:yyyy-MM-dd}', {workedHours}, 'Active')");
//                 }
//             }

//             return _res.Success("Worktime updated successfully.");
//         }

//         [HttpPost]
//         public async Task<JsonResult> InsertWorktime()
//         {
//             var gateEntries = _db.QueryDT(@"SELECT * FROM GateEntry 
//                                              WHERE GateStatus = 'OUT' 
//                                                AND EntryDateTime IS NOT NULL 
//                                                AND ExitDateTime IS NOT NULL");

//             foreach (System.Data.DataRow entry in gateEntries.Rows)
//             {
//                 var empID = entry["EmpID"].ToString();
//                 var entryDate = System.Convert.ToDateTime(entry["EntryDateTime"]);
//                 var exitDate = System.Convert.ToDateTime(entry["ExitDateTime"]);
//                 var workedHours = (float)(exitDate - entryDate).TotalHours;
//                 var date = exitDate.Date;

//                 _db.QueryPOST($@"INSERT INTO Worktime (EmpID, Date, WorkedHours, Status)
//                                  VALUES ('{empID}', '{date:yyyy-MM-dd}', {workedHours}, 'Active')");
//             }

//             return _res.Success("Worktime inserted successfully.");
//         }
//     }
// }
