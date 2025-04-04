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
    public class OJTandInspectionSkillController : ControllerBase
    {
        private readonly string _connectionString;

        public OJTandInspectionSkillController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/OJTandInspectionSkill
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<OJTandInspectionSkill>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM OJTandInspectionSkill", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    result.Add(new OJTandInspectionSkill
                    {
                        CourseNo = reader.GetString(0),
                        CourseGroup = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Biz = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Process = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CerNo = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Active = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                        SkillGroup = reader.IsDBNull(6) ? null : reader.GetString(6),
                        EmpID = reader.IsDBNull(7) ? null : reader.GetString(7)
                    });
                }
            }

            return Ok(result);
        }

        // GET: api/OJTandInspectionSkill/{courseNo}
        [HttpGet("{courseNo}")]
        public async Task<IActionResult> GetByCourseNo(string courseNo)
        {
            OJTandInspectionSkill skill = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM OJTandInspectionSkill WHERE CourseNo = @courseNo", conn);
                cmd.Parameters.AddWithValue("@courseNo", courseNo);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    skill = new OJTandInspectionSkill
                    {
                        CourseNo = reader.GetString(0),
                        CourseGroup = reader.IsDBNull(1) ? null : reader.GetString(1),
                        Biz = reader.IsDBNull(2) ? null : reader.GetString(2),
                        Process = reader.IsDBNull(3) ? null : reader.GetString(3),
                        CerNo = reader.IsDBNull(4) ? null : reader.GetString(4),
                        Active = reader.IsDBNull(5) ? null : reader.GetInt32(5),
                        SkillGroup = reader.IsDBNull(6) ? null : reader.GetString(6),
                        EmpID = reader.IsDBNull(7) ? null : reader.GetString(7)
                    };
                }
            }

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        // POST: api/OJTandInspectionSkill
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] OJTandInspectionSkill skill)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO OJTandInspectionSkill 
                        (CourseNo, CourseGroup, Biz, Process, CerNo, Active, SkillGroup, EmpID)
                    VALUES 
                        (@CourseNo, @CourseGroup, @Biz, @Process, @CerNo, @Active, @SkillGroup, @EmpID)", conn);

                cmd.Parameters.AddWithValue("@CourseNo", skill.CourseNo);
                cmd.Parameters.AddWithValue("@CourseGroup", (object?)skill.CourseGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biz", (object?)skill.Biz ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Process", (object?)skill.Process ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CerNo", (object?)skill.CerNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Active", (object?)skill.Active ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SkillGroup", (object?)skill.SkillGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)skill.EmpID ?? DBNull.Value);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/OJTandInspectionSkill/{courseNo}
        [HttpPut("{courseNo}")]
        public async Task<IActionResult> Update(string courseNo, [FromBody] OJTandInspectionSkill skill)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE OJTandInspectionSkill SET
                        CourseGroup = @CourseGroup,
                        Biz = @Biz,
                        Process = @Process,
                        CerNo = @CerNo,
                        Active = @Active,
                        SkillGroup = @SkillGroup,
                        EmpID = @EmpID
                    WHERE CourseNo = @CourseNo", conn);

                cmd.Parameters.AddWithValue("@CourseNo", courseNo);
                cmd.Parameters.AddWithValue("@CourseGroup", (object?)skill.CourseGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Biz", (object?)skill.Biz ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Process", (object?)skill.Process ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@CerNo", (object?)skill.CerNo ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Active", (object?)skill.Active ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@SkillGroup", (object?)skill.SkillGroup ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@EmpID", (object?)skill.EmpID ?? DBNull.Value);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/OJTandInspectionSkill/{courseNo}
        [HttpDelete("{courseNo}")]
        public async Task<IActionResult> Delete(string courseNo)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM OJTandInspectionSkill WHERE CourseNo = @courseNo", conn);
                cmd.Parameters.AddWithValue("@courseNo", courseNo);

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
