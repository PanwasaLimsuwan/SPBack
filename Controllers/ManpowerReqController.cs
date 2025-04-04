using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerReqController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerReqController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/ManpowerReq
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<ManpowerReq>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM ManpowerReq", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new ManpowerReq
                    {
                        MPRID = reader.GetString(0),
                        Date = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        Biz = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Process = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Require = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        SkillGroup = reader.IsDBNull(5) ? null : reader.GetString(5)
                    });
                }
            }

            return Ok(result);
        }

        // GET: api/ManpowerReq/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            ManpowerReq req = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM ManpowerReq WHERE MPRID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    req = new ManpowerReq
                    {
                        MPRID = reader.GetString(0),
                        Date = reader.IsDBNull(1) ? null : reader.GetDateTime(1),
                        Biz = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Process = reader.IsDBNull(3) ? null : reader.GetString(3),
                        Require = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        SkillGroup = reader.IsDBNull(5) ? null : reader.GetString(5)
                    };
                }
            }

            if (req == null)
                return NotFound();

            return Ok(req);
        }

        // POST: api/ManpowerReq
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ManpowerReq req)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO ManpowerReq (MPRID, Date, Biz, Process, Require, SkillGroup)
                    VALUES (@MPRID, @Date, @Biz, @Process, @Require, @SkillGroup)", conn);

                cmd.Parameters.AddWithValue("@MPRID", req.MPRID);
                cmd.Parameters.AddWithValue("@Date", (object?)req.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biz", (object?)req.Biz ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Process", (object?)req.Process ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Require", (object?)req.Require ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SkillGroup", (object?)req.SkillGroup ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/ManpowerReq/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(string id, [FromBody] ManpowerReq req)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE ManpowerReq SET 
                        Date = @Date,
                        Biz = @Biz,
                        Process = @Process,
                        Require = @Require,
                        SkillGroup = @SkillGroup
                    WHERE MPRID = @MPRID", conn);

                cmd.Parameters.AddWithValue("@MPRID", id);
                cmd.Parameters.AddWithValue("@Date", (object?)req.Date ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biz", (object?)req.Biz ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Process", (object?)req.Process ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Require", (object?)req.Require ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SkillGroup", (object?)req.SkillGroup ?? DBNull.Value);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/ManpowerReq/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM ManpowerReq WHERE MPRID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
