// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class HeadcountTransitionController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public HeadcountTransitionController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetHeadcountTransitions()
//         {
//             var transitions = await _context.HeadcountTransition.ToListAsync();
//             return Ok(transitions);
//         }
// // [HttpGet]
// // public async Task<IActionResult> GetHeadcountTransitions()
// // {
// //     var transitions = await _context.HeadcountTransition
// //         .Where(t => t.dateTime >= DateTime.Now.AddMonths(-3)) // เอาข้อมูล 3 เดือนล่าสุด
// //         .OrderBy(t => t.dateTime)
// //         .ToListAsync();
// //     return Ok(transitions);
// // }




//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetHeadcountTransitionById(DateTime id)
//         {
//             var transition = await _context.HeadcountTransition.FindAsync(id);
//             if (transition == null)
//             {
//                 return NotFound("Headcount transition not found");
//             }
//             return Ok(transition);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateHeadcountTransition([FromBody] HeadcountTransition transition)
//         {
//             if (transition == null)
//             {
//                 return BadRequest("Headcount transition is null");
//             }

//             _context.HeadcountTransition.Add(transition);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetHeadcountTransitionById), new { id = transition.DateTime }, transition);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateHeadcountTransition(DateTime id, [FromBody] HeadcountTransition transition)
//         {
//             if (id != transition.DateTime)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(transition).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.HeadcountTransition.Any(e => e.DateTime == id))
//                 {
//                     return NotFound("Headcount transition not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteHeadcountTransition(DateTime id)
//         {
//             var transition = await _context.HeadcountTransition.FindAsync(id);
//             if (transition == null)
//             {
//                 return NotFound("Headcount transition not found");
//             }

//             _context.HeadcountTransition.Remove(transition);
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
    public class HeadcountTransitionController : ControllerBase
    {
        private readonly string _connectionString;

        public HeadcountTransitionController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/HeadcountTransition
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<HeadcountTransition>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM HeadcountTransition";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new HeadcountTransition
                        {
                            Id = reader.GetInt32(reader.GetOrdinal("Id")),
                            EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                            TransType = reader["TransType"]?.ToString(),
                            DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime"))
                        });
                    }
                }
            }

            return Ok(result);
        }

        // GET: api/HeadcountTransition/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            HeadcountTransition data = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM HeadcountTransition WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            data = new HeadcountTransition
                            {
                                Id = reader.GetInt32(reader.GetOrdinal("Id")),
                                EmpID = reader.GetInt32(reader.GetOrdinal("EmpID")),
                                TransType = reader["TransType"]?.ToString(),
                                DateTime = reader.GetDateTime(reader.GetOrdinal("DateTime"))
                            };
                        }
                    }
                }
            }

            if (data == null)
                return NotFound("HeadcountTransition not found");

            return Ok(data);
        }

        // POST: api/HeadcountTransition
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HeadcountTransition model)
        {
            if (model == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO HeadcountTransition (EmpID, DateTime, TransType) 
                                 VALUES (@EmpID, @DateTime, @TransType)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@EmpID", model.EmpID);
                    cmd.Parameters.AddWithValue("@DateTime", model.DateTime);
                    cmd.Parameters.AddWithValue("@TransType", model.TransType ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Created successfully");
        }

        // PUT: api/HeadcountTransition/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HeadcountTransition model)
        {
            if (id != model.Id)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"UPDATE HeadcountTransition 
                                 SET EmpID = @EmpID, DateTime = @DateTime, TransType = @TransType 
                                 WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", model.Id);
                    cmd.Parameters.AddWithValue("@EmpID", model.EmpID);
                    cmd.Parameters.AddWithValue("@DateTime", model.DateTime);
                    cmd.Parameters.AddWithValue("@TransType", model.TransType ?? (object)DBNull.Value);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("HeadcountTransition not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/HeadcountTransition/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "DELETE FROM HeadcountTransition WHERE Id = @Id";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Id", id);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("HeadcountTransition not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Threading.Tasks;
// using System;
// using System.Linq;
// using LibSystem.Controllers;
// using Newtonsoft.Json;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class HeadcountTransitionController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public HeadcountTransitionController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetHeadcountTransitions()
//         {
//             try
//             {
//                 var data = _db.Query("SELECT * FROM HeadcountTransition");
//                 return _res.Success(data);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpGet("recent")]
//         public async Task<IActionResult> GetRecentHeadcountTransitions()
//         {
//             try
//             {
//                 var query = @"SELECT * FROM HeadcountTransition WHERE DateTime >= DATEADD(MONTH, -3, GETDATE()) ORDER BY DateTime";
//                 var data = _db.Query(query);
//                 return _res.Success(data);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetHeadcountTransitionById(string id)
//         {
//             try
//             {
//                 var data = _db.Query($"SELECT * FROM HeadcountTransition WHERE CONVERT(varchar, DateTime, 120) = '{id}'");
//                 if (data.Rows.Count == 0)
//                     return _res.NotFound();
//                 return _res.Success(data);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateHeadcountTransition([FromBody] HeadcountTransition transition)
//         {
//             if (transition == null)
//                 return _res.BadRequest("Headcount transition is null");

//             try
//             {
//                 var json = JsonConvert.SerializeObject(transition);
//                 var result = _db.Insert("HeadcountTransition", json);
//                 return _res.Created(result);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateHeadcountTransition(string id, [FromBody] HeadcountTransition transition)
//         {
//             if (id != transition.DateTime.ToString("s"))
//                 return _res.BadRequest("ID mismatch");

//             try
//             {
//                 var json = JsonConvert.SerializeObject(transition);
//                 var result = _db.Update("HeadcountTransition", json, $"CONVERT(varchar, DateTime, 120) = '{id}'");
//                 return _res.Success(result);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteHeadcountTransition(string id)
//         {
//             try
//             {
//                 var result = _db.Delete("HeadcountTransition", $"CONVERT(varchar, DateTime, 120) = '{id}'");
//                 return _res.Success(result);
//             }
//             catch (Exception e)
//             {
//                 return _res.InternalServerError(e.ToString());
//             }
//         }
//     }
// }
