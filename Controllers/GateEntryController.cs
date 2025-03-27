using Microsoft.AspNetCore.Mvc;
using Api.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateEntryController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public GateEntryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // ดึงข้อมูลจากทั้ง GateEntry, EmployeeInfo, OJTandInspectionSkill, CleanroomEntry โดยใช้ EmpID
        [HttpGet]
        public async Task<IActionResult> GetGateEntry()
        {
            var combinedData = await (from gate in _context.GateEntry
                                      join employee in _context.EmployeeInfo
                                      on gate.EmpID.ToString() equals employee.EmpID.ToString() // เชื่อมต่อ EmpID ของ GateEntry และ EmployeeInfo
                                      join ojt in _context.OJTandInspectionSkill
                                      on gate.EmpID equals ojt.EmpID // เชื่อมต่อ EmpID ของ GateEntry และ OJTandInspectionSkill
                                      join cleanroom in _context.CleanroomEntry
                                      on gate.EmpID equals cleanroom.EmpID // เชื่อมต่อ EmpID ของ GateEntry และ CleanroomEntry
                                      select new 
                                      {
                                          EmpID = gate.EmpID,
                                          FirstName = employee.FirstName,
                                          LastName = employee.LastName,
                                          Division = employee.Division,
                                          Department = employee.Department,
                                          Position = employee.Position,
                                          Email = employee.Email,
                                          ShiftCode = employee.ShiftCode,
                                            Section = employee.Section,
                                          EntryDateTime = gate.EntryDateTime,
                                          ExitDateTime = gate.ExitDateTime,
                                          GateNo = gate.GateNo,
                                          GateStatus = gate.GateStatus,
                                          Biz = ojt.Biz, // ข้อมูลจาก OJTandInspectionSkill
                                          Process = ojt.Process, // ข้อมูลจาก OJTandInspectionSkill
                                          CourseGroup = ojt.CourseGroup, // ข้อมูลจาก OJTandInspectionSkill
                                          WorkGroup = ojt.SkillGroup, // ข้อมูลจาก OJTandInspectionSkill
                                          CStatus = cleanroom.CStatus, // ข้อมูลจาก CleanroomEntry
                                          CheckInDateTime = cleanroom.CheckInDateTime, // ข้อมูลจาก CleanroomEntry
                                          CheckOutDateTime = cleanroom.CheckOutDateTime, // ข้อมูลจาก CleanroomEntry
                                          Status = (cleanroom.CStatus == "OUT" && gate.GateStatus == "OUT") ? "status-missing" :
                                           (cleanroom.CStatus == "OUT" && gate.GateStatus == "IN") ? "status-out-cleanroom" :
                                           (cleanroom.CStatus == "IN" && gate.GateStatus == "IN") ? "status-in-cleanroom" :
                                           "status-unknown", // กรณีอื่นๆ

                                      }).ToListAsync();

            // ส่งข้อมูลกลับไปยัง Frontend ในรูปแบบ JSON
            return Ok(combinedData);
        }
    }
}
