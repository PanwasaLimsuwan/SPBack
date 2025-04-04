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
            var transitions = new List<HeadcountTransition>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM HeadcountTransition", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
{
    transitions.Add(new HeadcountTransition
    {
        Id = (int)reader["Id"],
        DateTime = (DateTime)reader["DateTime"],
        TransType = reader["TransType"]?.ToString(),
        EmpID = reader["EmpID"]?.ToString()
    });
}

            }

            return Ok(transitions);
        }

        // GET: api/HeadcountTransition/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            HeadcountTransition transition = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM HeadcountTransition WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    transition = new HeadcountTransition
                    {
                        Id = reader.GetInt32(0),
                        DateTime = reader.GetDateTime(1),
                        TransType = reader.IsDBNull(2) ? null : reader.GetString(2),
                        EmpID = reader.IsDBNull(3) ? null : reader.GetString(3)
                    };
                }
            }

            if (transition == null)
                return NotFound();

            return Ok(transition);
        }

        // POST: api/HeadcountTransition
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] HeadcountTransition transition)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO HeadcountTransition (DateTime, TransType, EmpID)
                    VALUES (@DateTime, @TransType, @EmpID)", conn);

                cmd.Parameters.AddWithValue("@DateTime", transition.DateTime);
                cmd.Parameters.AddWithValue("@TransType", (object?)transition.TransType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)transition.EmpID ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/HeadcountTransition/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] HeadcountTransition transition)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE HeadcountTransition SET
                        DateTime = @DateTime,
                        TransType = @TransType,
                        EmpID = @EmpID
                    WHERE Id = @Id", conn);

                cmd.Parameters.AddWithValue("@Id", id);
                cmd.Parameters.AddWithValue("@DateTime", transition.DateTime);
                cmd.Parameters.AddWithValue("@TransType", (object?)transition.TransType ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)transition.EmpID ?? DBNull.Value);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/HeadcountTransition/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM HeadcountTransition WHERE Id = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
