// using System.Linq;
// using System.Threading.Tasks;
// using Microsoft.EntityFrameworkCore;
// using Api.Models;

// namespace Api.Services
// {
//     public class DataMigrationService
//     {
//         private readonly ApplicationDbContext _context;

//         public DataMigrationService(ApplicationDbContext context)
//         {
//             _context = context;
//         }

//         public async Task MigrateGateEntryToFaceEntry()
// {
//     try
//     {
//         // ดึงข้อมูล GateEntry ที่มี EntryDateTime ไม่เป็น NULL
//         var gateEntries = await _context.GateEntry
//             .Where(g => g.EntryDateTime.HasValue)  // ตรวจสอบว่า EntryDateTime มีค่าไม่เป็น NULL
//             .ToListAsync();

//         Console.WriteLine($"พบข้อมูล GateEntry ทั้งหมด {gateEntries.Count} รายการ");

//         foreach (var gate in gateEntries)
//         {
//             // สร้าง FaceEntry จากข้อมูล GateEntry
//             var faceEntry = new FaceEntry
//             {
//                 EmpID = gate.EmpID,
//                 Timestamp = gate.EntryDateTime.Value,  // ใช้ EntryDateTime จาก GateEntry
//                 Vector = "NULL" // เปลี่ยนค่าตามความต้องการของคุณ
//             };

//             // บันทึกข้อมูลลงในตาราง FaceEntry
//             await _context.FaceEntry.AddAsync(faceEntry);
//         }

//         // บันทึกการเปลี่ยนแปลงในฐานข้อมูล
//         int rowsAffected = await _context.SaveChangesAsync();
//         Console.WriteLine($"ย้ายข้อมูลเสร็จเรียบร้อยแล้ว! จำนวนแถวที่เพิ่ม: {rowsAffected}");
//     }
//     catch (Exception ex)
//     {
//         Console.WriteLine($"เกิดข้อผิดพลาด: {ex.Message}");
//     }
// }

//     }
// }
