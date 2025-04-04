// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ManpowerPlanController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public ManpowerPlanController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/ManpowerPlan
//         [HttpGet]
//         public async Task<IActionResult> GetManpowerPlans()
//         {
//             var manpowerPlans = await _context.ManpowerPlan.ToListAsync();
//             return Ok(manpowerPlans);
//         }

//         // GET: api/ManpowerPlan/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetManpowerPlanById(int id)
//         {
//             var manpowerPlan = await _context.ManpowerPlan.FindAsync(id);
//             if (manpowerPlan == null)
//             {
//                 return NotFound("Manpower plan not found");
//             }
//             return Ok(manpowerPlan);
//         }

//         // POST: api/ManpowerPlan
//         [HttpPost]
//         public async Task<IActionResult> CreateManpowerPlan([FromBody] ManpowerPlan plan)
//         {
//             if (plan == null)
//             {
//                 return BadRequest("Manpower plan is null");
//             }

//             _context.ManpowerPlan.Add(plan);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetManpowerPlanById), new { id = plan.PlanID }, plan); // Use PlanID instead of Id
//         }

//         // PUT: api/ManpowerPlan/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateManpowerPlan(int id, [FromBody] ManpowerPlan plan)
//         {
//             if (id != plan.PlanID) // Use PlanID instead of Id
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(plan).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.ManpowerPlan.Any(e => e.PlanID == id)) // Use PlanID instead of Id
//                 {
//                     return NotFound("Manpower plan not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/ManpowerPlan/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteManpowerPlan(int id)
//         {
//             var plan = await _context.ManpowerPlan.FindAsync(id);
//             if (plan == null)
//             {
//                 return NotFound("Manpower plan not found");
//             }

//             _context.ManpowerPlan.Remove(plan);
//             await _context.SaveChangesAsync();

//             return NoContent();
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
    public class ManpowerPlanController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerPlanController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/ManpowerPlan
        [HttpGet]
        public async Task<IActionResult> GetAllPlans()
        {
            var plans = new List<ManpowerPlan>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM ManpowerPlan";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        plans.Add(new ManpowerPlan
                        {
                            PlanID = reader.GetInt32(reader.GetOrdinal("PlanID")),
                            Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("Date")),
                            EmpID = reader["EmpID"]?.ToString(),
                            ShiftCode = reader["ShiftCode"]?.ToString(),
                            Shift = reader["Shift"]?.ToString(),
                            PlannedHeadcount = reader.IsDBNull(reader.GetOrdinal("PlannedHeadcount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PlannedHeadcount")),
                            ActualHeadcount = reader.IsDBNull(reader.GetOrdinal("ActualHeadcount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("ActualHeadcount"))
                        });
                    }
                }
            }

            return Ok(plans);
        }

        // GET: api/ManpowerPlan/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPlanById(int id)
        {
            ManpowerPlan plan = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "SELECT * FROM ManpowerPlan WHERE PlanID = @PlanID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlanID", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            plan = new ManpowerPlan
                            {
                                PlanID = reader.GetInt32(reader.GetOrdinal("PlanID")),
                                Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("Date")),
                                EmpID = reader["EmpID"]?.ToString(),
                                ShiftCode = reader["ShiftCode"]?.ToString(),
                                Shift = reader["Shift"]?.ToString(),
                                PlannedHeadcount = reader.IsDBNull(reader.GetOrdinal("PlannedHeadcount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("PlannedHeadcount")),
                                ActualHeadcount = reader.IsDBNull(reader.GetOrdinal("ActualHeadcount")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("ActualHeadcount"))
                            };
                        }
                    }
                }
            }

            if (plan == null)
                return NotFound("ManpowerPlan not found");

            return Ok(plan);
        }

        // POST: api/ManpowerPlan
        [HttpPost]
        public async Task<IActionResult> CreatePlan([FromBody] ManpowerPlan plan)
        {
            if (plan == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"INSERT INTO ManpowerPlan (Date, EmpID, ShiftCode, Shift, PlannedHeadcount, ActualHeadcount)
                              VALUES (@Date, @EmpID, @ShiftCode, @Shift, @PlannedHeadcount, @ActualHeadcount)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Date", (object?)plan.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmpID", plan.EmpID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", plan.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Shift", plan.Shift ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PlannedHeadcount", (object?)plan.PlannedHeadcount ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActualHeadcount", (object?)plan.ActualHeadcount ?? DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Created successfully");
        }

        // PUT: api/ManpowerPlan/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdatePlan(int id, [FromBody] ManpowerPlan plan)
        {
            if (id != plan.PlanID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = @"UPDATE ManpowerPlan 
                              SET Date = @Date, EmpID = @EmpID, ShiftCode = @ShiftCode, Shift = @Shift,
                                  PlannedHeadcount = @PlannedHeadcount, ActualHeadcount = @ActualHeadcount
                              WHERE PlanID = @PlanID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlanID", plan.PlanID);
                    cmd.Parameters.AddWithValue("@Date", (object?)plan.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@EmpID", plan.EmpID ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@ShiftCode", plan.ShiftCode ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Shift", plan.Shift ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@PlannedHeadcount", (object?)plan.PlannedHeadcount ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@ActualHeadcount", (object?)plan.ActualHeadcount ?? DBNull.Value);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("ManpowerPlan not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/ManpowerPlan/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePlan(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var query = "DELETE FROM ManpowerPlan WHERE PlanID = @PlanID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@PlanID", id);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("ManpowerPlan not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Newtonsoft.Json;
// using API_ProductionQuality.Controllers;
// using System.Threading.Tasks;
// using System.Data;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ManpowerPlanController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public ManpowerPlanController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetManpowerPlans()
//         {
//             var query = _db.Query("SELECT * FROM ManpowerPlan");
//             return _res.Success(query);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetManpowerPlanById(int id)
//         {
//             var query = _db.Query($"SELECT * FROM ManpowerPlan WHERE PlanID = {id}");
//             if (query.Rows.Count == 0)
//             {
//                 return _res.NotFound("Manpower plan not found");
//             }
//             return _res.Success(query);
//         }

//         [HttpPost]
//         public async Task<JsonResult> CreateManpowerPlan([FromBody] Params param)
//         {
//             string json = param.param1.ToString();
//             DataTable dt = _db.ConvertJsonStringToDT(json);
//             foreach (DataRow row in dt.Rows)
//             {
//                 string insertQuery = $@"
//                     INSERT INTO ManpowerPlan (Division, Department, Process, RequiredHeadcount, Week, CreatedDate)
//                     VALUES (
//                         '{row["Division"]}',
//                         '{row["Department"]}',
//                         '{row["Process"]}',
//                         '{row["RequiredHeadcount"]}',
//                         '{row["Week"]}',
//                         GETDATE()
//                     )";

//                 var result = _db.QueryPOST(insertQuery);
//                 string jsonResult = _db.ConvertJsonResultToString(result);
//                 var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//                 if (response.code != "201")
//                 {
//                     return _res.InternalServerError(response.message);
//                 }
//             }
//             return _res.Created(null);
//         }

//         [HttpPut("{id}")]
//         public async Task<JsonResult> UpdateManpowerPlan(int id, [FromBody] Params param)
//         {
//             string json = param.param1.ToString();
//             DataTable dt = _db.ConvertJsonStringToDT(json);
//             foreach (DataRow row in dt.Rows)
//             {
//                 string updateQuery = $@"
//                     UPDATE ManpowerPlan SET
//                         Division = '{row["Division"]}',
//                         Department = '{row["Department"]}',
//                         Process = '{row["Process"]}',
//                         RequiredHeadcount = '{row["RequiredHeadcount"]}',
//                         Week = '{row["Week"]}'
//                     WHERE PlanID = {id}";

//                 var result = _db.QueryPOST(updateQuery);
//                 string jsonResult = _db.ConvertJsonResultToString(result);
//                 var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//                 if (response.code != "201")
//                 {
//                     return _res.InternalServerError(response.message);
//                 }
//             }
//             return _res.Success(null);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> DeleteManpowerPlan(int id)
//         {
//             var deleteQuery = $"DELETE FROM ManpowerPlan WHERE PlanID = {id}";
//             var result = _db.QueryPOST(deleteQuery);
//             string jsonResult = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(jsonResult);
//             if (response.code != "201")
//             {
//                 return _res.InternalServerError(response.message);
//             }
//             return _res.Created(null);
//         }
//     }
// }
