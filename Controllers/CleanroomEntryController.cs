// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class CleanroomEntryController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public CleanroomEntryController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetCleanroomEntries()
//         {
//             var cleanroomEntries = await _context.CleanroomEntry.ToListAsync();
//             return Ok(cleanroomEntries);
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetCleanroomEntryById(int id)
//         {
//             var entry = await _context.CleanroomEntry.FindAsync(id);
//             if (entry == null)
//             {
//                 return NotFound("Cleanroom entry not found");
//             }
//             return Ok(entry);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateCleanroomEntry([FromBody] CleanroomEntry entry)
//         {
//             if (entry == null)
//             {
//                 return BadRequest("Cleanroom entry is null");
//             }

//             _context.CleanroomEntry.Add(entry);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetCleanroomEntryById), new { id = entry.CEntryID }, entry);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateCleanroomEntry(int id, [FromBody] CleanroomEntry entry)
//         {
//             if (id != entry.CEntryID)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(entry).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.CleanroomEntry.Any(e => e.CEntryID == id))
//                 {
//                     return NotFound("Cleanroom entry not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteCleanroomEntry(int id)
//         {
//             var entry = await _context.CleanroomEntry.FindAsync(id);
//             if (entry == null)
//             {
//                 return NotFound("Cleanroom entry not found");
//             }

//             _context.CleanroomEntry.Remove(entry);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;
using System;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleanroomEntryController : ControllerBase
    {
        private readonly string _connectionString;

        public CleanroomEntryController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/CleanroomEntry
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            List<CleanroomEntry> entries = new();

            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new("SELECT * FROM CleanroomEntry", conn);

            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                entries.Add(new CleanroomEntry
                {
                    CEntryID = reader.GetInt32(0),
                    EmpID = reader.GetInt32(1),
                    CheckInDateTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    CheckOutDateTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    CStatus = reader.IsDBNull(4) ? null : reader.GetString(4)
                });
            }

            return Ok(entries);
        }

        // GET: api/CleanroomEntry/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            CleanroomEntry entry = null;

            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new("SELECT * FROM CleanroomEntry WHERE CEntryID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            using SqlDataReader reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                entry = new CleanroomEntry
                {
                    CEntryID = reader.GetInt32(0),
                    EmpID = reader.GetInt32(1),
                    CheckInDateTime = reader.IsDBNull(2) ? null : reader.GetDateTime(2),
                    CheckOutDateTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3),
                    CStatus = reader.IsDBNull(4) ? null : reader.GetString(4)
                };
            }

            return entry == null ? NotFound("Record not found") : Ok(entry);
        }

        // POST: api/CleanroomEntry
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CleanroomEntry entry)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new(@"INSERT INTO CleanroomEntry (EmpID, CheckInDateTime, CheckOutDateTime, CStatus) 
                                   VALUES (@EmpID, @CheckIn, @CheckOut, @Status)", conn);

            cmd.Parameters.AddWithValue("@EmpID", entry.EmpID);
            cmd.Parameters.AddWithValue("@CheckIn", (object?)entry.CheckInDateTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOut", (object?)entry.CheckOutDateTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)entry.CStatus ?? DBNull.Value);

            await conn.OpenAsync();
            await cmd.ExecuteNonQueryAsync();

            return Ok("Created successfully");
        }

        // PUT: api/CleanroomEntry/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CleanroomEntry entry)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new(@"UPDATE CleanroomEntry 
                                   SET EmpID = @EmpID, CheckInDateTime = @CheckIn, 
                                       CheckOutDateTime = @CheckOut, CStatus = @Status
                                   WHERE CEntryID = @CEntryID", conn);

            cmd.Parameters.AddWithValue("@CEntryID", id);
            cmd.Parameters.AddWithValue("@EmpID", entry.EmpID);
            cmd.Parameters.AddWithValue("@CheckIn", (object?)entry.CheckInDateTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@CheckOut", (object?)entry.CheckOutDateTime ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Status", (object?)entry.CStatus ?? DBNull.Value);

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();

            return rows == 0 ? NotFound("Record not found") : Ok("Updated successfully");
        }

        // DELETE: api/CleanroomEntry/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using SqlConnection conn = new(_connectionString);
            SqlCommand cmd = new("DELETE FROM CleanroomEntry WHERE CEntryID = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);

            await conn.OpenAsync();
            int rows = await cmd.ExecuteNonQueryAsync();

            return rows == 0 ? NotFound("Record not found") : Ok("Deleted successfully");
        }
    }
}

// using Microsoft.AspNetCore.Mvc;
// using Newtonsoft.Json;
// using Api.Models;
// using System.Threading.Tasks;
// using System.Linq;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class CleanroomEntryController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public CleanroomEntryController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<IActionResult> GetAll()
//         {
//             var result = _db.Query("SELECT * FROM CleanroomEntry");
//             return _res.Success(result);
//         }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetById(int id)
//         {
//             var data = _db.Query($"SELECT * FROM CleanroomEntry WHERE CEntryID = {id}");
//             if (data.Rows.Count == 0)
//             {
//                 return _res.NotFound("Cleanroom entry not found");
//             }
//             return _res.Success(data);
//         }

//         [HttpPost]
//         public async Task<IActionResult> Create([FromBody] CleanroomEntry entry)
//         {
//             if (entry == null)
//             {
//                 return _res.BadRequest("Entry is null");
//             }

//             var query = $@"
//                 INSERT INTO CleanroomEntry (EmpID, CStatus, CheckInDateTime, CheckOutDateTime)
//                 VALUES ('{entry.EmpID}', '{entry.CStatus}', '{entry.CheckInDateTime:yyyy-MM-dd HH:mm:ss}', '{entry.CheckOutDateTime:yyyy-MM-dd HH:mm:ss}')";

//             var result = _db.QueryPOST(query);
//             return _res.Created(result);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> Update(int id, [FromBody] CleanroomEntry entry)
//         {
//             if (id != entry.CEntryID)
//             {
//                 return _res.BadRequest("ID mismatch");
//             }

//             var query = $@"
//                 UPDATE CleanroomEntry SET
//                     EmpID = '{entry.EmpID}',
//                     CStatus = '{entry.CStatus}',
//                     CheckInDateTime = '{entry.CheckInDateTime:yyyy-MM-dd HH:mm:ss}',
//                     CheckOutDateTime = '{entry.CheckOutDateTime:yyyy-MM-dd HH:mm:ss}'
//                 WHERE CEntryID = {id}";

//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> Delete(int id)
//         {
//             var query = $"DELETE FROM CleanroomEntry WHERE CEntryID = {id}";
//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }
//     }
// }
