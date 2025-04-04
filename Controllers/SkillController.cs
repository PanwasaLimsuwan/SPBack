// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class SkillController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public SkillController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/Skill
//         [HttpGet]
//         public async Task<IActionResult> GetSkills()
//         {
//             var skills = await _context.Skill.ToListAsync();
//             return Ok(skills);
//         }

//         // GET: api/Skill/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetSkillById(int id)
//         {
//             var skill = await _context.Skill.FindAsync(id);
//             if (skill == null)
//             {
//                 return NotFound("Skill not found");
//             }
//             return Ok(skill);
//         }

//         // POST: api/Skill
//         [HttpPost]
//         public async Task<IActionResult> CreateSkill([FromBody] Skill skill)
//         {
//             if (skill == null)
//             {
//                 return BadRequest("Skill is null");
//             }

//             _context.Skill.Add(skill);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetSkillById), new { id = skill.EmpID }, skill);
//         }

//         // PUT: api/Skill/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateSkill(int id, [FromBody] Skill skill)
//         {
//             if (id != skill.EmpID)
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
//                 if (!_context.Skill.Any(e => e.EmpID == id))
//                 {
//                     return NotFound("Skill not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/Skill/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteSkill(int id)
//         {
//             var skill = await _context.Skill.FindAsync(id);
//             if (skill == null)
//             {
//                 return NotFound("Skill not found");
//             }

//             _context.Skill.Remove(skill);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }

// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Threading.Tasks;
// using System.Linq;
// using Api.Services;
// using Api.Responses;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class SkillController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public SkillController()
//         {
//             _db = new LibQueryController("Default");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetAll()
//         {
//             var data = _db.Query("SELECT * FROM Skill");
//             return _res.Success(data);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetById(int id)
//         {
//             var data = _db.Query($"SELECT * FROM Skill WHERE EmpID = {id}");
//             if (data.Rows.Count == 0)
//                 return _res.NotFound("Skill not found");

//             return _res.Success(data);
//         }

//         [HttpPost]
//         public async Task<JsonResult> Create([FromBody] Skill skill)
//         {
//             if (skill == null)
//                 return _res.BadRequest("Skill is null");

//             var query = $"INSERT INTO Skill (EmpID, SkillName, SkillLevel) VALUES ('{skill.EmpID}', '{skill.SkillName}', '{skill.SkillLevel}')";
//             var result = _db.QueryPOST(query);
//             return _res.Created(result);
//         }

//         [HttpPut("{id}")]
//         public async Task<JsonResult> Update(int id, [FromBody] Skill skill)
//         {
//             if (id != skill.EmpID)
//                 return _res.BadRequest("ID mismatch");

//             var exists = _db.QueryDT($"SELECT 1 FROM Skill WHERE EmpID = {id}");
//             if (exists.Rows.Count == 0)
//                 return _res.NotFound("Skill not found");

//             var query = $"UPDATE Skill SET SkillName = '{skill.SkillName}', SkillLevel = '{skill.SkillLevel}' WHERE EmpID = {id}";
//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> Delete(int id)
//         {
//             var exists = _db.QueryDT($"SELECT 1 FROM Skill WHERE EmpID = {id}");
//             if (exists.Rows.Count == 0)
//                 return _res.NotFound("Skill not found");

//             var query = $"DELETE FROM Skill WHERE EmpID = {id}";
//             var result = _db.QueryPOST(query);
//             return _res.Success(result);
//         }
//     }
// }