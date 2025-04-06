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

        // ✅ GET: api/HeadcountTransition (แบบ Join EmployeeInfo)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var transitions = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        ht.Id,
                        ht.DateTime,
                        ht.TransType,
                        ht.EmpID,
                        ei.Division,
                        ei.Department,
                        ei.Section,
                        ei.Biz,
                        ei.Process
                    FROM HeadcountTransition ht
                    LEFT JOIN EmployeeInfo ei ON ht.EmpID = ei.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        transitions.Add(new
                        {
                            id = reader["Id"],
                            dateTime = reader["DateTime"],
                            transType = reader["TransType"]?.ToString(),
                            empID = reader["EmpID"]?.ToString(),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString()
                        });
                    }
                }
            }

            return Ok(transitions);
        }

        // ✅ GET: api/HeadcountTransition/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            object transition = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        ht.Id,
                        ht.DateTime,
                        ht.TransType,
                        ht.EmpID,
                        ei.Division,
                        ei.Department,
                        ei.Section,
                        ei.Biz,
                        ei.Process
                    FROM HeadcountTransition ht
                    LEFT JOIN EmployeeInfo ei ON ht.EmpID = ei.EmpID
                    WHERE ht.Id = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            transition = new
                            {
                                id = reader["Id"],
                                dateTime = reader["DateTime"],
                                transType = reader["TransType"]?.ToString(),
                                empID = reader["EmpID"]?.ToString(),
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString()
                            };
                        }
                    }
                }
            }

            if (transition == null)
                return NotFound();

            return Ok(transition);
        }

        // ✅ POST: api/HeadcountTransition
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

        // ✅ PUT: api/HeadcountTransition/{id}
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

        // ✅ DELETE: api/HeadcountTransition/{id}
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
