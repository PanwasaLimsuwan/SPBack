// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System.Linq;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class ManpowerReqController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public ManpowerReqController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/ManpowerReq
//         [HttpGet]
//         public async Task<IActionResult> GetManpowerReqs()
//         {
//             var manpowerReqs = await _context.ManpowerReq.ToListAsync();
//             return Ok(manpowerReqs);
//         }

//         // GET: api/ManpowerReq/{id}
//         [HttpGet("{id}")]
//         public async Task<IActionResult> GetManpowerReqById(string id)
//         {
//             var manpowerReq = await _context.ManpowerReq.FindAsync(id);
//             if (manpowerReq == null)
//             {
//                 return NotFound("Manpower request not found");
//             }
//             return Ok(manpowerReq);
//         }

//         // POST: api/ManpowerReq
//         [HttpPost]
//         public async Task<IActionResult> CreateManpowerReq([FromBody] ManpowerReq req)
//         {
//             if (req == null)
//             {
//                 return BadRequest("Manpower request is null");
//             }

//             _context.ManpowerReq.Add(req);
//             await _context.SaveChangesAsync();
//             return CreatedAtAction(nameof(GetManpowerReqById), new { id = req.MPRID }, req); // Use MPRID instead of Id
//         }

//         // PUT: api/ManpowerReq/{id}
//         [HttpPut("{id}")]
//         public async Task<IActionResult> UpdateManpowerReq(string id, [FromBody] ManpowerReq req)
//         {
//             if (id != req.MPRID) // Use MPRID instead of Id
//             {
//                 return BadRequest("ID mismatch");
//             }

//             _context.Entry(req).State = EntityState.Modified;

//             try
//             {
//                 await _context.SaveChangesAsync();
//             }
//             catch (DbUpdateConcurrencyException)
//             {
//                 if (!_context.ManpowerReq.Any(e => e.MPRID == id)) // Use MPRID instead of Id
//                 {
//                     return NotFound("Manpower request not found");
//                 }
//                 else
//                 {
//                     throw;
//                 }
//             }

//             return NoContent();
//         }

//         // DELETE: api/ManpowerReq/{id}
//         [HttpDelete("{id}")]
//         public async Task<IActionResult> DeleteManpowerReq(string id)
//         {
//             var req = await _context.ManpowerReq.FindAsync(id);
//             if (req == null)
//             {
//                 return NotFound("Manpower request not found");
//             }

//             _context.ManpowerReq.Remove(req);
//             await _context.SaveChangesAsync();

//             return NoContent();
//         }
//     }
// }



// using Microsoft.AspNetCore.Mvc;
// using Api.Models;
// using Newtonsoft.Json;
// using System.Threading.Tasks;
// using System.Linq;
// using API_ProductionQuality.Services;
// using API_ProductionQuality.Helper;
// using System.Data;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]/[action]")]
//     public class ManpowerReqController : ControllerBase
//     {
//         private readonly LibQueryController _db;
//         private readonly LibResponseController _res;

//         public ManpowerReqController()
//         {
//             _db = new LibQueryController("Deploy");
//             _res = new LibResponseController();
//         }

//         [HttpGet]
//         public async Task<JsonResult> GetAll()
//         {
//             var query = _db.Query("SELECT * FROM ManpowerReq");
//             return _res.Success(query);
//         }

//         [HttpGet("{id}")]
//         public async Task<JsonResult> GetById(string id)
//         {
//             var query = _db.Query($"SELECT * FROM ManpowerReq WHERE MPRID = '{id}'");
//             if (query.Rows.Count == 0)
//             {
//                 return _res.NotFound("Manpower request not found");
//             }
//             return _res.Success(query);
//         }

//         [HttpPost]
//         public async Task<JsonResult> Create([FromBody] Params param)
//         {
//             string json = param.param1.ToString();
//             DataTable dt = _db.ConvertJsonStringToDT(json);
//             foreach (DataRow row in dt.Rows)
//             {
//                 string insertQuery = $@"
//                     INSERT INTO ManpowerReq (
//                         MPRID, Biz, Division, Department, Process,
//                         CourseGroup, SkillGroup, Required, WeekNo, YearNo, CreateDate, CreateBy
//                     ) VALUES (
//                         '{row["MPRID"]}', '{row["Biz"]}', '{row["Division"]}', '{row["Department"]}', '{row["Process"]}',
//                         '{row["CourseGroup"]}', '{row["SkillGroup"]}', {row["Required"]}, {row["WeekNo"]}, {row["YearNo"]}, GETDATE(), 'system')";

//                 var result = _db.QueryPOST(insertQuery);
//                 var response = JsonConvert.DeserializeObject<Response>(_db.ConvertJsonResultToString(result));
//                 if (response.code != "201")
//                 {
//                     return _res.InternalServerError(response.message);
//                 }
//             }
//             return _res.Created(null);
//         }

//         [HttpPut]
//         public async Task<JsonResult> Update([FromBody] Params param)
//         {
//             string json = param.param1.ToString();
//             DataTable dt = _db.ConvertJsonStringToDT(json);
//             foreach (DataRow row in dt.Rows)
//             {
//                 string updateQuery = $@"
//                     UPDATE ManpowerReq SET
//                         Biz = '{row["Biz"]}',
//                         Division = '{row["Division"]}',
//                         Department = '{row["Department"]}',
//                         Process = '{row["Process"]}',
//                         CourseGroup = '{row["CourseGroup"]}',
//                         SkillGroup = '{row["SkillGroup"]}',
//                         Required = {row["Required"]},
//                         WeekNo = {row["WeekNo"]},
//                         YearNo = {row["YearNo"]},
//                         UpdateDate = GETDATE(),
//                         UpdateBy = 'system'
//                     WHERE MPRID = '{row["MPRID"]}'";

//                 var result = _db.QueryPOST(updateQuery);
//                 var response = JsonConvert.DeserializeObject<Response>(_db.ConvertJsonResultToString(result));
//                 if (response.code != "201")
//                 {
//                     return _res.InternalServerError(response.message);
//                 }
//             }
//             return _res.Success(null);
//         }

//         [HttpDelete("{id}")]
//         public async Task<JsonResult> Delete(string id)
//         {
//             string deleteQuery = $"DELETE FROM ManpowerReq WHERE MPRID = '{id}'";
//             var result = _db.QueryPOST(deleteQuery);
//             var response = JsonConvert.DeserializeObject<Response>(_db.ConvertJsonResultToString(result));
//             if (response.code != "201")
//             {
//                 return _res.InternalServerError(response.message);
//             }
//             return _res.Created(null);
//         }
//     }
// }