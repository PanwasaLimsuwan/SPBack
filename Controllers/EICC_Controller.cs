// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class EICC_ControlController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public EICC_ControlController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/EICC_Control
//         [HttpGet]
//         public async Task<IActionResult> GetEICCControls()
//         {
//             var controls = await _context.EICC_Control.ToListAsync();
//             return Ok(controls);
//         }

//         // GET: api/EICC_Control/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetEICCControlById(int id)
//         {
//             var control = await _context.EICC_Control.FindAsync(id);
//             if (control == null)
//             {
//                 return NotFound("EICC control not found");
//             }
//             return Ok(control);
//         }

//         // POST: api/EICC_Control
//         [HttpPost]
//         public async Task<IActionResult> CreateEICCControl([FromBody] EICC_Control control)
//         {
//             if (control == null)
//             {
//                 return BadRequest("EICC control is null");
//             }

//             _context.EICC_Control.Add(control);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetEICCControlById), new { id = control.ControlID }, control); // Use ControlID instead of Id
//         }

//         // PUT: api/EICC_Control/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateEICCControl(int id, [FromBody] EICC_Control control)
//         {
//             if (id != control.ControlID) // Use ControlID instead of Id
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(control).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.EICC_Control.Any(e => e.ControlID == id)) // Use ControlID instead of Id
//                 {
//                     return NotFound("EICC control not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/EICC_Control/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteEICCControl(int id)
//         {
//             var control = await _context.EICC_Control.FindAsync(id);
//             if (control == null)
//             {
//                 return NotFound("EICC control not found");
//             }

//             _context.EICC_Control.Remove(control);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Threading.Tasks;
using Api.Models;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EICC_ControlController : ControllerBase
    {
        private readonly string _connectionString;

        public EICC_ControlController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/EICC_Control
        [HttpGet]
        public async Task<IActionResult> GetEICCControls()
        {
            var result = new List<EICC_Control>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM EICC_Control", conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new EICC_Control
                        {
                            ControlID = reader.GetInt32(reader.GetOrdinal("ControlID")),
                            EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                            WeekID = reader.GetInt32(reader.GetOrdinal("WeekID")),
                            TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetFloat(reader.GetOrdinal("TotalHours")),
                            DaysWorked = reader.IsDBNull(reader.GetOrdinal("DaysWorked")) ? null : reader.GetInt32(reader.GetOrdinal("DaysWorked")),
                            Status = reader["Status"]?.ToString()
                        });
                    }
                }
            }

            return Ok(result);
        }

        // GET: api/EICC_Control/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEICCControlById(int id)
        {
            EICC_Control control = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("SELECT * FROM EICC_Control WHERE ControlID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            control = new EICC_Control
                            {
                                ControlID = reader.GetInt32(reader.GetOrdinal("ControlID")),
                                EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                                WeekID = reader.GetInt32(reader.GetOrdinal("WeekID")),
                                TotalHours = reader.IsDBNull(reader.GetOrdinal("TotalHours")) ? null : reader.GetFloat(reader.GetOrdinal("TotalHours")),
                                DaysWorked = reader.IsDBNull(reader.GetOrdinal("DaysWorked")) ? null : reader.GetInt32(reader.GetOrdinal("DaysWorked")),
                                Status = reader["Status"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (control == null)
                return NotFound("EICC control not found");

            return Ok(control);
        }

        // POST: api/EICC_Control
        [HttpPost]
        public async Task<IActionResult> CreateEICCControl([FromBody] EICC_Control control)
        {
            if (control == null)
                return BadRequest("EICC control is null");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(@"
                    INSERT INTO EICC_Control (EmpID, WeekID, TotalHours, DaysWorked, Status)
                    VALUES (@EmpID, @WeekID, @TotalHours, @DaysWorked, @Status);
                    SELECT SCOPE_IDENTITY();
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                    cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                    cmd.Parameters.AddWithValue("@TotalHours", (object?)control.TotalHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DaysWorked", (object?)control.DaysWorked ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)control.Status ?? DBNull.Value);

                    var insertedId = (decimal)await cmd.ExecuteScalarAsync();
                    control.ControlID = (int)insertedId;
                }
            }

            return CreatedAtAction(nameof(GetEICCControlById), new { id = control.ControlID }, control);
        }

        // PUT: api/EICC_Control/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateEICCControl(int id, [FromBody] EICC_Control control)
        {
            if (id != control.ControlID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand(@"
                    UPDATE EICC_Control
                    SET EmpID = @EmpID,
                        WeekID = @WeekID,
                        TotalHours = @TotalHours,
                        DaysWorked = @DaysWorked,
                        Status = @Status
                    WHERE ControlID = @ControlID
                ", conn))
                {
                    cmd.Parameters.AddWithValue("@ControlID", control.ControlID);
                    cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                    cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                    cmd.Parameters.AddWithValue("@TotalHours", (object?)control.TotalHours ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@DaysWorked", (object?)control.DaysWorked ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Status", (object?)control.Status ?? DBNull.Value);

                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("EICC control not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/EICC_Control/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEICCControl(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (SqlCommand cmd = new SqlCommand("DELETE FROM EICC_Control WHERE ControlID = @id", conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);
                    int rowsAffected = await cmd.ExecuteNonQueryAsync();
                    if (rowsAffected == 0)
                        return NotFound("EICC control not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;
// using API_ProductionQuality.Models;
// using System.Threading.Tasks;
// using System.Linq;
// using System.Data;

// namespace API_ProductionQuality.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class EICC_ControlController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public EICC_ControlController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetAll()
//         {
//             var query = _db.Query("SELECT * FROM EICC_Control");
//             return _res.Success(query);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetById(int id)
//         {
//             var query = _db.QueryDT("SELECT * FROM EICC_Control WHERE ControlID = " + id);
//             if (query.Rows.Count == 0)
//             {
//                 return _res.NotFound();
//             }
//             return _res.Success(query);
//         }

//         [HttpPost]
//         public async Task<JsonResult> Create([FromBody] EICC_Control control)
//         {
//             if (control == null)
//             {
//                 return _res.BadRequest("EICC control is null");
//             }

//             string insert = $@"
//                 INSERT INTO EICC_Control (ControlName, ControlValue, Description)
//                 VALUES (N'{control.ControlName}', N'{control.ControlValue}', N'{control.Description}')";

//             var result = _db.QueryPOST(insert);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);

//             if (response.code != "201")
//                 return _res.InternalServerError(response.message);

//             return _res.Created(null);
//         }

//         [HttpPut("{id}")]
//         public async Task<JsonResult> Update(int id, [FromBody] EICC_Control control)
//         {
//             if (id != control.ControlID)
//             {
//                 return _res.BadRequest("ID mismatch");
//             }

//             string update = $@"
//                 UPDATE EICC_Control SET 
//                     ControlName = N'{control.ControlName}',
//                     ControlValue = N'{control.ControlValue}',
//                     Description = N'{control.Description}'
//                 WHERE ControlID = {id}";

//             var result = _db.QueryPOST(update);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);

//             if (response.code != "201")
//                 return _res.InternalServerError(response.message);

//             return _res.Success(null);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> Delete(int id)
//         {
//             string delete = $"DELETE FROM EICC_Control WHERE ControlID = {id}";

//             var result = _db.QueryPOST(delete);
//             string json = _db.ConvertJsonResultToString(result);
//             var response = JsonConvert.DeserializeObject<Response>(json);

//             if (response.code != "201")
//                 return _res.InternalServerError(response.message);

//             return _res.Success(null);
//         }
//     }
// }
