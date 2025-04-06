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

        // ✅ GET: api/ManpowerReq (พร้อม Join EmployeeInfo)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        m.MPRID,
                        m.Date,
                        m.Biz,
                        m.Process,
                        m.Require,
                        m.SkillGroup,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz AS EmployeeBiz,
                        e.Process AS EmployeeProcess
                    FROM ManpowerReq m
                    LEFT JOIN EmployeeInfo e ON m.Biz = e.Biz AND m.Process = e.Process";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new
                        {
                            mprID = reader["MPRID"]?.ToString(),
                            date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            require = reader["Require"] == DBNull.Value ? null : (int?)reader["Require"],
                            skillGroup = reader["SkillGroup"]?.ToString(),
                            division = reader["Division"]?.ToString(),
                            department = reader["Department"]?.ToString(),
                            section = reader["Section"]?.ToString(),
                            employeeBiz = reader["EmployeeBiz"]?.ToString(),
                            employeeProcess = reader["EmployeeProcess"]?.ToString()
                        });
                    }
                }
            }

            return Ok(result);
        }

        // ✅ GET: api/ManpowerReq/{id} (พร้อม Join EmployeeInfo)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(string id)
        {
            object req = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        m.MPRID,
                        m.Date,
                        m.Biz,
                        m.Process,
                        m.Require,
                        m.SkillGroup,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz AS EmployeeBiz,
                        e.Process AS EmployeeProcess
                    FROM ManpowerReq m
                    LEFT JOIN EmployeeInfo e ON m.Biz = e.Biz AND m.Process = e.Process
                    WHERE m.MPRID = @id";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", id);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            req = new
                            {
                                mprID = reader["MPRID"]?.ToString(),
                                date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString(),
                                require = reader["Require"] == DBNull.Value ? null : (int?)reader["Require"],
                                skillGroup = reader["SkillGroup"]?.ToString(),
                                division = reader["Division"]?.ToString(),
                                department = reader["Department"]?.ToString(),
                                section = reader["Section"]?.ToString(),
                                employeeBiz = reader["EmployeeBiz"]?.ToString(),
                                employeeProcess = reader["EmployeeProcess"]?.ToString()
                            };
                        }
                    }
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
