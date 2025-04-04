// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class OJTandInspectionSkillController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public OJTandInspectionSkillController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // ดึงข้อมูลจากทั้ง EmployeeInfo และ OJTandInspectionSkill โดยใช้ EmpID
//         [HttpGet]
//         [HttpGet]
// public async Task<IActionResult> GetOJTandInspectionSkills()
// {
//     var skills = await _context.OJTandInspectionSkill.ToListAsync();
//     return Ok(skills);
// }

//         // public async Task<IActionResult> GetOJTandInspectionSkills()
//         // {
//         //     var combinedData = await (from employee in _context.EmployeeInfo
//         //                               join skill in _context.OJTandInspectionSkill
//         //                               on employee.EmpID.ToString() equals skill.EmpID
//         //                               select new 
//         //                               {
//         //                                   EmpID = employee.EmpID,
//         //                                   FirstName = employee.FirstName,
//         //                                   LastName = employee.LastName,
//         //                                   Division = employee.Division,
//         //                                   Department = employee.Department,
//         //                                   Position = employee.Position,
//         //                                   Email = employee.Email,
//         //                                   CourseNo = skill.CourseNo,
//         //                                   CourseGroup = skill.CourseGroup,
//         //                                   Biz = skill.Biz,
//         //                                   Process = skill.Process,
//         //                                   SkillGroup = skill.SkillGroup,
//         //                                   Active = skill.Active
//         //                               }).ToListAsync();

//         //     return Ok(combinedData);
//         // }

//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetOJTandInspectionSkillById(string id)
//         {
//             var skill = await _context.OJTandInspectionSkill.FindAsync(id);
//             if (skill == null)
//             {
//                 return NotFound("OJT and inspection skill not found");
//             }
//             return Ok(skill);
//         }

//         [HttpPost]
//         public async Task<IActionResult> CreateOJTandInspectionSkill([FromBody] OJTandInspectionSkill skill)
//         {
//             if (skill == null)
//             {
//                 return BadRequest("OJT and inspection skill is null");
//             }

//             _context.OJTandInspectionSkill.Add(skill);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetOJTandInspectionSkillById), new { id = skill.CourseNo }, skill);
//         }

//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateOJTandInspectionSkill(string id, [FromBody] OJTandInspectionSkill skill)
//         {
//             if (id != skill.CourseNo)
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(skill).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.OJTandInspectionSkill.Any(e => e.CourseNo == id))
//                 {
//                     return NotFound("OJT and inspection skill not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteOJTandInspectionSkill(string id)
//         {
//             var skill = await _context.OJTandInspectionSkill.FindAsync(id);
//             if (skill == null)
//             {
//                 return NotFound("OJT and inspection skill not found");
//             }

//             _context.OJTandInspectionSkill.Remove(skill);
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
    public class ManpowerReqController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerReqController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/ManpowerReq
        [HttpGet]
        public async Task<IActionResult> GetAllManpowerReqs()
        {
            var list = new List<ManpowerReq>();

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM ManpowerReq";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        list.Add(new ManpowerReq
                        {
                            MPRID = reader["MPRID"].ToString(),
                            Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("Date")),
                            Biz = reader["Biz"].ToString(),
                            Process = reader["Process"].ToString(),
                            Require = reader.IsDBNull(reader.GetOrdinal("Require")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Require")),
                            SkillGroup = reader["SkillGroup"].ToString()
                        });
                    }
                }
            }

            return Ok(list);
        }

        // GET: api/ManpowerReq/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetManpowerReqById(string id)
        {
            ManpowerReq req = null;

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "SELECT * FROM ManpowerReq WHERE MPRID = @MPRID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MPRID", id);

                    using (SqlDataReader reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            req = new ManpowerReq
                            {
                                MPRID = reader["MPRID"].ToString(),
                                Date = reader.IsDBNull(reader.GetOrdinal("Date")) ? null : (DateTime?)reader.GetDateTime(reader.GetOrdinal("Date")),
                                Biz = reader["Biz"].ToString(),
                                Process = reader["Process"].ToString(),
                                Require = reader.IsDBNull(reader.GetOrdinal("Require")) ? null : (int?)reader.GetInt32(reader.GetOrdinal("Require")),
                                SkillGroup = reader["SkillGroup"].ToString()
                            };
                        }
                    }
                }
            }

            if (req == null)
                return NotFound("ManpowerReq not found");

            return Ok(req);
        }

        // POST: api/ManpowerReq
        [HttpPost]
        public async Task<IActionResult> CreateManpowerReq([FromBody] ManpowerReq req)
        {
            if (req == null)
                return BadRequest("Invalid data");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"INSERT INTO ManpowerReq (MPRID, Date, Biz, Process, Require, SkillGroup) 
                                 VALUES (@MPRID, @Date, @Biz, @Process, @Require, @SkillGroup)";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MPRID", req.MPRID);
                    cmd.Parameters.AddWithValue("@Date", (object?)req.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Biz", req.Biz ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Process", req.Process ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Require", (object?)req.Require ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SkillGroup", req.SkillGroup ?? (object)DBNull.Value);

                    await cmd.ExecuteNonQueryAsync();
                }
            }

            return Ok("Created successfully");
        }

        // PUT: api/ManpowerReq/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateManpowerReq(string id, [FromBody] ManpowerReq req)
        {
            if (id != req.MPRID)
                return BadRequest("ID mismatch");

            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = @"UPDATE ManpowerReq 
                                 SET Date = @Date, Biz = @Biz, Process = @Process, Require = @Require, SkillGroup = @SkillGroup
                                 WHERE MPRID = @MPRID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MPRID", req.MPRID);
                    cmd.Parameters.AddWithValue("@Date", (object?)req.Date ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@Biz", req.Biz ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Process", req.Process ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@Require", (object?)req.Require ?? DBNull.Value);
                    cmd.Parameters.AddWithValue("@SkillGroup", req.SkillGroup ?? (object)DBNull.Value);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("ManpowerReq not found");
                }
            }

            return NoContent();
        }

        // DELETE: api/ManpowerReq/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteManpowerReq(string id)
        {
            using (SqlConnection conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string query = "DELETE FROM ManpowerReq WHERE MPRID = @MPRID";

                using (SqlCommand cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@MPRID", id);

                    int affected = await cmd.ExecuteNonQueryAsync();
                    if (affected == 0)
                        return NotFound("ManpowerReq not found");
                }
            }

            return NoContent();
        }
    }
}


// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;
// using API.Controllers.Lib;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class OJTandInspectionSkillController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public OJTandInspectionSkillController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetAll()
//         {
//             var data = _db.Query(@"
//                 SELECT 
//                     e.EmpID,
//                     e.FirstName,
//                     e.LastName,
//                     e.Division,
//                     e.Department,
//                     e.Position,
//                     e.Email,
//                     s.CourseNo,
//                     s.CourseGroup,
//                     s.Biz,
//                     s.Process,
//                     s.SkillGroup,
//                     s.Active
//                 FROM EmployeeInfo e
//                 INNER JOIN OJTandInspectionSkill s ON CAST(e.EmpID AS VARCHAR) = s.EmpID");

//             return _res.Success(data);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetById(string id)
//         {
//             var data = _db.Query($"SELECT * FROM OJTandInspectionSkill WHERE CourseNo = '{id}'");
//             if (data.Rows.Count == 0)
//                 return _res.NotFound("OJT and inspection skill not found");

//             return _res.Success(data);
//         }

//         [HttpPost]
//         public async Task<JsonResult> Create([FromBody] OJTandInspectionSkill skill)
//         {
//             if (skill == null)
//                 return _res.BadRequest("OJT and inspection skill is null");

//             string query = $@"
//                 INSERT INTO OJTandInspectionSkill 
//                 (CourseNo, EmpID, CourseGroup, Biz, Process, SkillGroup, Active)
//                 VALUES
//                 ('{skill.CourseNo}', '{skill.EmpID}', '{skill.CourseGroup}', '{skill.Biz}', '{skill.Process}', '{skill.SkillGroup}', '{skill.Active}')";

//             var result = _db.QueryPOST(query);
//             return _res.Created(result);
//         }

//         [HttpPut("{id}")]
//         public async Task<JsonResult> Update(string id, [FromBody] OJTandInspectionSkill skill)
//         {
//             if (id != skill.CourseNo)
//                 return _res.BadRequest("ID mismatch");

//             string query = $@"
//                 UPDATE OJTandInspectionSkill SET 
//                     EmpID = '{skill.EmpID}',
//                     CourseGroup = '{skill.CourseGroup}',
//                     Biz = '{skill.Biz}',
//                     Process = '{skill.Process}',
//                     SkillGroup = '{skill.SkillGroup}',
//                     Active = '{skill.Active}'
//                 WHERE CourseNo = '{id}'";

//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> Delete(string id)
//         {
//             var query = $"DELETE FROM OJTandInspectionSkill WHERE CourseNo = '{id}'";
//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }
//     }
// }