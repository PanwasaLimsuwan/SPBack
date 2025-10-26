// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Api.Models;
// using System.Collections.Generic;
// using System.Threading.Tasks;

// namespace Api.Controllers
// {
//     [Route("api/[controller]")]
//     [ApiController]
//     public class CamerasController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;

//         public CamerasController(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         // GET: api/cameras  ✅ ดึงทั้งหมด (ให้ Vue ใช้)
//         [HttpGet]
//         public async Task<ActionResult<IEnumerable<Cameras>>> GetCameras()
//         {
//             var cameras = await _context.Cameras.ToListAsync();
//             return Ok(cameras);
//         }

//         // GET: api/cameras/CAM-ENTR-01  ✅ ดึงทีละตัวด้วย string id
//         [HttpGet("{id}")]
//         public async Task<ActionResult<Cameras>> GetCamera(string id)
//         {
//             var camera = await _context.Cameras.FirstOrDefaultAsync(c => c.CameraID == id);
//             if (camera == null) return NotFound();
//             return Ok(camera);
//         }
//     }
// }
