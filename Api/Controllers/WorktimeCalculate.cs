// using Api.Models;
// using Microsoft.EntityFrameworkCore;
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Threading.Tasks;

// public class WorktimeCalculate
// {
//     private readonly ApplicationDbContext _context;

//     public WorktimeCalculate(ApplicationDbContext context)
//     {
//         _context = context;
//     }

//     // อัปเดตข้อมูล Worktime โดยคำนวณชั่วโมงการทำงานและ OT จาก GateEntry
//     public async Task<string> UpdateWorktimeAsync()
//     {
//         var gateEntries = await _context.GateEntry
//             .Where(ge => ge.GateStatus == "OUT" && ge.EntryDateTime != null && ge.ExitDateTime != null)
//             .ToListAsync();

//         foreach (var entry in gateEntries)
//         {
//             var workedHours = (entry.ExitDateTime - entry.EntryDateTime)?.TotalHours ?? 0;
//             var otHours = workedHours > 8 ? workedHours - 8 : 0;

//             var existingWorktime = await _context.Worktime
//                 .FirstOrDefaultAsync(w => w.EmpID == entry.EmpID && w.Date == entry.ExitDateTime?.Date);

//             if (existingWorktime != null)
//             {
//                 existingWorktime.WorkedHours = (float?)workedHours;
//                 existingWorktime.OT_Hours = (float?)otHours;
//                 existingWorktime.Status = "Active";
//                 _context.Worktime.Update(existingWorktime);
//             }
//             else
//             {
//                 var worktime = new Worktime
//                 {
//                     EmpID = entry.EmpID,
//                     Date = entry.ExitDateTime?.Date,
//                     WorkedHours = (float?)workedHours,
//                     OT_Hours = (float?)otHours,
//                     Status = "Active"
//                 };
//                 _context.Worktime.Add(worktime);
//             }
//         }

//         await _context.SaveChangesAsync();
//         return "Worktime updated successfully";
//     }

//     // เพิ่มข้อมูล Worktime ใหม่ โดยไม่ตรวจสอบซ้ำ
//     public async Task<string> InsertWorktimeAsync()
//     {
//         var gateEntries = await _context.GateEntry
//             .Where(ge => ge.GateStatus == "OUT" && ge.EntryDateTime != null && ge.ExitDateTime != null)
//             .ToListAsync();

//         foreach (var entry in gateEntries)
//         {
//             var workedHours = (entry.ExitDateTime - entry.EntryDateTime)?.TotalHours ?? 0;
//             var otHours = workedHours > 8 ? workedHours - 8 : 0;

//             var worktime = new Worktime
//             {
//                 EmpID = entry.EmpID,
//                 Date = entry.ExitDateTime?.Date,
//                 WorkedHours = (float?)workedHours,
//                 OT_Hours = (float?)otHours,
//                 Status = "Active"
//             };

//             _context.Worktime.Add(worktime);
//         }

//         await _context.SaveChangesAsync();
//         return "Worktime inserted successfully";
//     }
// }
