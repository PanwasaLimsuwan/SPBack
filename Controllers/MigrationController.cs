// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Api.Services;
// using Api.Models;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class MigrationController : ControllerBase
//     {
//         private readonly DataMigrationService _migrationService;
//         private readonly ApplicationDbContext _context;

//         public MigrationController(DataMigrationService migrationService, ApplicationDbContext context)
//         {
//             _migrationService = migrationService;
//             _context = context;
//         }

//         // ⭐ ใช้ method เดียวนี้เลย (ลบ migrate-entry-only ออก)
//         [HttpPost("migrate-with-exit")]
//         public async Task<IActionResult> MigrateWithExit()
//         {
//             try
//             {
//                 // เช็คข้อมูลก่อน migrate
//                 var gateCountBefore = await _context.GateEntry.CountAsync();
//                 var faceCountBefore = await _context.FaceVectors.CountAsync();
                
//                 Console.WriteLine($"=== ก่อน Migration ===");
//                 Console.WriteLine($"GateEntry: {gateCountBefore} records");
//                 Console.WriteLine($"FaceVectors: {faceCountBefore} records");

//                 // ทำ Migration
//                 await _migrationService.MigrateWithExitTime();
                
//                 // เช็คข้อมูลหลัง migrate
//                 var faceCountAfter = await _context.FaceVectors.CountAsync();
                
//                 Console.WriteLine($"=== หลัง Migration ===");
//                 Console.WriteLine($"FaceVectors: {faceCountAfter} records");
//                 Console.WriteLine($"เพิ่มขึ้น: {faceCountAfter - faceCountBefore} records");

//                 return Ok(new { 
//                     message = "Migration สำเร็จ",
//                     gateEntryTotal = gateCountBefore,
//                     faceVectorsBefore = faceCountBefore,
//                     faceVectorsAfter = faceCountAfter,
//                     added = faceCountAfter - faceCountBefore
//                 });
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ Error: {ex.Message}");
//                 Console.WriteLine($"StackTrace: {ex.StackTrace}");
                
//                 return StatusCode(500, new { 
//                     error = ex.Message,
//                     stackTrace = ex.StackTrace,
//                     innerException = ex.InnerException?.Message
//                 });
//             }
//         }

//         // ⭐ Endpoint ทดสอบการเชื่อมต่อ
//         [HttpGet("test-connection")]
//         public async Task<IActionResult> TestConnection()
//         {
//             try
//             {
//                 var gateCount = await _context.GateEntry.CountAsync();
//                 var faceCount = await _context.FaceVectors.CountAsync();
                
//                 // ดูข้อมูลตัวอย่าง
//                 var sampleGate = await _context.GateEntry
//                     .Where(g => g.EntryDateTime != null)
//                     .Take(5)
//                     .Select(g => new {
//                         g.GateEntryID,
//                         g.EmpID,
//                         g.EntryDateTime,
//                         g.ExitDateTime,
//                         g.GateNo
//                     })
//                     .ToListAsync();
                
//                 return Ok(new {
//                     message = "เชื่อมต่อฐานข้อมูลสำเร็จ",
//                     gateEntry = gateCount,
//                     faceVectors = faceCount,
//                     sampleData = sampleGate
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { 
//                     error = ex.Message,
//                     stackTrace = ex.StackTrace
//                 });
//             }
//         }

//         // ⭐ Endpoint ลบข้อมูล FaceVectors (ใช้เมื่อต้องการเริ่มใหม่)
//         [HttpDelete("clear-face-vectors")]
//         public async Task<IActionResult> ClearFaceVectors()
//         {
//             try
//             {
//                 var count = await _context.FaceVectors.CountAsync();
                
//                 if (count == 0)
//                 {
//                     return Ok(new { message = "FaceVectors ว่างอยู่แล้ว" });
//                 }

//                 // ลบข้อมูลทั้งหมด
//                 await _context.Database.ExecuteSqlRawAsync("DELETE FROM FaceVectors");
                
//                 return Ok(new { 
//                     message = "ลบข้อมูล FaceVectors สำเร็จ",
//                     deletedRecords = count
//                 });
//             }
//             catch (Exception ex)
//             {
//                 return StatusCode(500, new { error = ex.Message });
//             }
//         }
//     }
// }
