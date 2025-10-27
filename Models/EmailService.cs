// using System.Net;
// using System.Net.Mail;
// using Microsoft.AspNetCore.Mvc;
// using Microsoft.Extensions.Configuration;
// using System;
// using System.Collections.Generic;
// using System.Data;
// using System.Data.SqlClient;
// using System.Text;
// using System.Threading.Tasks;
// using Api.Models;

// namespace Api.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AssignmentController : ControllerBase
//     {
//         private readonly string _connectionString;

//         public AssignmentController(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("DefaultConnection");
//         }

//         [HttpPost]
//         public async Task<IActionResult> Create([FromBody] Assignment assignment)
//         {
//             try
//             {
//                 if (assignment == null)
//                     return BadRequest("Invalid payload.");

//                 // validate ฟิลด์จำเป็น
//                 var errors = new List<string>();
//                 if (assignment.EmpID <= 0) errors.Add("EmpID is required.");
//                 if (string.IsNullOrWhiteSpace(assignment.ToBiz)) errors.Add("ToBiz is required.");
//                 if (string.IsNullOrWhiteSpace(assignment.ToProcess)) errors.Add("ToProcess is required.");
//                 if (string.IsNullOrWhiteSpace(assignment.SkillGroup)) errors.Add("SkillGroup is required.");
//                 if (errors.Count > 0) return BadRequest(string.Join(" ", errors));

//                 var startAt = assignment.StartAt == default ? DateTime.UtcNow : assignment.StartAt;

//                 object endAt = (assignment.EndAt == default) ? DBNull.Value : assignment.EndAt;

//                 var status = string.IsNullOrWhiteSpace(assignment.Status) ? "Active" : assignment.Status;

//                 using var conn = new SqlConnection(_connectionString);
//                 await conn.OpenAsync();

//                 var sql = @"
//                 INSERT INTO Assignment
//                   (EmpID, FromBiz, FromProcess, ToBiz, ToProcess, SkillGroup, StartAt, EndAt, Status)
//                 OUTPUT INSERTED.AssignmentID
//                 VALUES
//                   (@EmpID, @FromBiz, @FromProcess, @ToBiz, @ToProcess, @SkillGroup, @StartAt, @EndAt, @Status);";

//                 using var cmd = new SqlCommand(sql, conn);
//                 cmd.Parameters.Add("@EmpID", SqlDbType.Int).Value = assignment.EmpID;
//                 cmd.Parameters.Add("@FromBiz", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(assignment.FromBiz) ? (object)DBNull.Value : assignment.FromBiz;
//                 cmd.Parameters.Add("@FromProcess", SqlDbType.NVarChar, 100).Value = string.IsNullOrWhiteSpace(assignment.FromProcess) ? (object)DBNull.Value : assignment.FromProcess;
//                 cmd.Parameters.Add("@ToBiz", SqlDbType.NVarChar, 100).Value = assignment.ToBiz;
//                 cmd.Parameters.Add("@ToProcess", SqlDbType.NVarChar, 100).Value = assignment.ToProcess;
//                 cmd.Parameters.Add("@SkillGroup", SqlDbType.NVarChar, 100).Value = assignment.SkillGroup;
//                 cmd.Parameters.Add("@StartAt", SqlDbType.DateTime2).Value = startAt;
//                 cmd.Parameters.Add("@EndAt", SqlDbType.DateTime2).Value = endAt;
//                 cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;

//                 var newIdObj = await cmd.ExecuteScalarAsync();
//                 var newId = Convert.ToInt32(newIdObj);

//                 // รับข้อมูลพนักงานและอีเมลของ supervisor
//                 var employee = await _context.EmployeeInfo.FindAsync(assignment.EmpID);
//                 if (employee != null)
//                 {
//                     var supervisorEmail = employee.Email; // หรือหากต้องการอีเมล supervisor สามารถใช้วิธีค้นหา
//                     string subject = $"Employee {employee.FirstName} {employee.LastName} has been moved";
//                     string body = $"Dear Supervisor, <br><br>The employee <strong>{employee.FirstName} {employee.LastName}</strong> has been moved to the new section and process.<br><br>Details:<br>Biz: {assignment.ToBiz}<br>Process: {assignment.ToProcess}<br><br>Regards,<br>Your System";

//                     // ส่งอีเมลแจ้งเตือน
//                     EmailService.SendEmail(supervisorEmail, subject, body);
//                 }

//                 return Ok(new { assignmentID = newId, message = "Assignment created successfully." });
//             }
//             catch (SqlException ex)
//             {
//                 return Problem(title: "SQL error", detail: ex.Message, statusCode: 500);
//             }
//             catch (Exception ex)
//             {
//                 return Problem(title: "Create failed", detail: ex.Message, statusCode: 500);
//             }
//         }
//     }
// }
