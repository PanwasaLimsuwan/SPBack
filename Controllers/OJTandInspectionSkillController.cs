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

        // ✅ GET: api/OJTandInspectionSkill (With Join EmployeeInfo)
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = new List<object>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        o.CourseNo,
                        o.CourseGroup,
                        o.Biz,
                        o.Process,
                        o.CerNo,
                        o.Active,
                        o.SkillGroup,
                        o.EmpID,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz AS EmployeeBiz,
                        e.Process AS EmployeeProcess
                    FROM OJTandInspectionSkill o
                    LEFT JOIN EmployeeInfo e ON o.EmpID = e.EmpID";

                using (var cmd = new SqlCommand(query, conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        result.Add(new
                        {
                            courseNo = reader["CourseNo"]?.ToString(),
                            courseGroup = reader["CourseGroup"]?.ToString(),
                            biz = reader["Biz"]?.ToString(),
                            process = reader["Process"]?.ToString(),
                            cerNo = reader["CerNo"]?.ToString(),
                            active = reader["Active"] == DBNull.Value ? null : (int?)reader["Active"],
                            skillGroup = reader["SkillGroup"]?.ToString(),
                            empID = reader["EmpID"]?.ToString(),
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

        // ✅ GET: api/OJTandInspectionSkill/{courseNo} (With Join EmployeeInfo)
        [HttpGet("{courseNo}")]
        public async Task<IActionResult> GetByCourseNo(string courseNo)
        {
            object skill = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();

                var query = @"
                    SELECT 
                        o.CourseNo,
                        o.CourseGroup,
                        o.Biz,
                        o.Process,
                        o.CerNo,
                        o.Active,
                        o.SkillGroup,
                        o.EmpID,
                        e.Division,
                        e.Department,
                        e.Section,
                        e.Biz AS EmployeeBiz,
                        e.Process AS EmployeeProcess
                    FROM OJTandInspectionSkill o
                    LEFT JOIN EmployeeInfo e ON o.EmpID = e.EmpID
                    WHERE o.CourseNo = @CourseNo";

                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@CourseNo", courseNo);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            skill = new
                            {
                                courseNo = reader["CourseNo"]?.ToString(),
                                courseGroup = reader["CourseGroup"]?.ToString(),
                                biz = reader["Biz"]?.ToString(),
                                process = reader["Process"]?.ToString(),
                                cerNo = reader["CerNo"]?.ToString(),
                                active = reader["Active"] == DBNull.Value ? null : (int?)reader["Active"],
                                skillGroup = reader["SkillGroup"]?.ToString(),
                                empID = reader["EmpID"]?.ToString(),
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
