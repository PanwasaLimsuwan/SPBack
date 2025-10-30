using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MimeKit;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly string _connectionString;

        public AssignmentController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Assignment?status=Active&toBiz=...&toProcess=...
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? toBiz,
            [FromQuery] string? toProcess
        )
        {
            try
            {
                var results = new List<object>();

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sb = new StringBuilder(
                    @"
SELECT AssignmentID, EmpID, FromBiz, FromProcess, ToBiz, ToProcess, 
       SkillGroup, StartAt, EndAt, Status
FROM Assignment
WHERE 1=1"
                );

                using var cmd = new SqlCommand();
                cmd.Connection = conn;

                if (!string.IsNullOrWhiteSpace(status))
                {
                    sb.Append(" AND Status = @status");
                    cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = status;
                }
                if (!string.IsNullOrWhiteSpace(toBiz))
                {
                    sb.Append(" AND ToBiz = @toBiz");
                    cmd.Parameters.Add("@toBiz", SqlDbType.NVarChar, 100).Value = toBiz;
                }
                if (!string.IsNullOrWhiteSpace(toProcess))
                {
                    sb.Append(" AND ToProcess = @toProcess");
                    cmd.Parameters.Add("@toProcess", SqlDbType.NVarChar, 100).Value = toProcess;
                }

                cmd.CommandText = sb.ToString();

                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    results.Add(
                        new
                        {
                            assignmentID = reader.GetInt32(0),
                            empID = reader.GetInt32(1),
                            fromBiz = reader.IsDBNull(2) ? null : reader.GetString(2),
                            fromProcess = reader.IsDBNull(3) ? null : reader.GetString(3),
                            toBiz = reader.IsDBNull(4) ? null : reader.GetString(4),
                            toProcess = reader.IsDBNull(5) ? null : reader.GetString(5),
                            skillGroup = reader.IsDBNull(6) ? null : reader.GetString(6),
                            startAt = reader.GetDateTime(7),
                            endAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                            status = reader.IsDBNull(9) ? null : reader.GetString(9),
                        }
                    );
                }

                return Ok(results);
            }
            catch (Exception ex)
            {
                return Problem(title: "GetAll failed", detail: ex.Message, statusCode: 500);
            }
        }

        // POST: api/Assignment
        // สร้าง Assignment ใหม่ (AssignmentID สร้างที่ DB ด้วย SEQUENCE/DEFAULT)
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Assignment assignment)
        {
            try
            {
                if (assignment == null)
                    return BadRequest("Invalid payload.");

                // validate ฟิลด์จำเป็น
                var errors = new List<string>();
                if (assignment.EmpID <= 0)
                    errors.Add("EmpID is required.");
                if (string.IsNullOrWhiteSpace(assignment.ToBiz))
                    errors.Add("ToBiz is required.");
                if (string.IsNullOrWhiteSpace(assignment.ToProcess))
                    errors.Add("ToProcess is required.");
                if (string.IsNullOrWhiteSpace(assignment.SkillGroup))
                    errors.Add("SkillGroup is required.");
                if (errors.Count > 0)
                    return BadRequest(string.Join(" ", errors));

                var startAt = assignment.StartAt == default ? DateTime.UtcNow : assignment.StartAt;

                // ✅ ให้ EndAt เป็น NULL เมื่อไม่ส่งมา (ตรงความต้องการ)
                object endAt = (assignment.EndAt == default) ? DBNull.Value : assignment.EndAt;

                // var status = string.IsNullOrWhiteSpace(assignment.Status)
                //     ? "Active"
                //     : assignment.Status;
                // Default Status to 'Pending'
        var status = string.IsNullOrWhiteSpace(assignment.Status) ? "Pending" : assignment.Status;

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql =
                    @"
INSERT INTO Assignment
  (EmpID, FromBiz, FromProcess, ToBiz, ToProcess, SkillGroup, StartAt, EndAt, Status)
OUTPUT INSERTED.AssignmentID
VALUES
  (@EmpID, @FromBiz, @FromProcess, @ToBiz, @ToProcess, @SkillGroup, @StartAt, @EndAt, @Status);";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@EmpID", SqlDbType.Int).Value = assignment.EmpID;
                cmd.Parameters.Add("@FromBiz", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(assignment.FromBiz)
                        ? (object)DBNull.Value
                        : assignment.FromBiz;
                cmd.Parameters.Add("@FromProcess", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(assignment.FromProcess)
                        ? (object)DBNull.Value
                        : assignment.FromProcess;
                cmd.Parameters.Add("@ToBiz", SqlDbType.NVarChar, 100).Value = assignment.ToBiz;
                cmd.Parameters.Add("@ToProcess", SqlDbType.NVarChar, 100).Value =
                    assignment.ToProcess;
                cmd.Parameters.Add("@SkillGroup", SqlDbType.NVarChar, 100).Value =
                    assignment.SkillGroup;
                cmd.Parameters.Add("@StartAt", SqlDbType.DateTime2).Value = startAt;
                cmd.Parameters.Add("@EndAt", SqlDbType.DateTime2).Value = endAt;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;

                var newIdObj = await cmd.ExecuteScalarAsync();
                var newId = Convert.ToInt32(newIdObj);

                return Ok(
                    new { assignmentID = newId, message = "Assignment created successfully." }
                );
            }
            catch (SqlException ex)
            {
                return Problem(title: "SQL error", detail: ex.Message, statusCode: 500);
            }
            catch (Exception ex)
            {
                return Problem(title: "Create failed", detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/Assignment/{id}/status?status=Completed
        [HttpPut("{id:int}/status")]
        public async Task<IActionResult> UpdateStatus([FromRoute] int id, [FromQuery] string status)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(status))
                    return BadRequest("Status is required.");

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql = @"UPDATE Assignment SET Status = @status WHERE AssignmentID = @id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = status;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound("Assignment not found.");
                return Ok("Status updated successfully.");
            }
            catch (Exception ex)
            {
                return Problem(title: "UpdateStatus failed", detail: ex.Message, statusCode: 500);
            }
        }

        // DTO สำหรับปิดงาน
        public class UpdateEndAtDto
        {
            public DateTime? EndAt { get; set; }
            public string? Status { get; set; }
        }

        // PUT: api/Assignment/{empID}  -> ปิดงาน Active ของพนักงาน (EndAt + Status)
        [HttpPut("{empID:int}")]
        public async Task<IActionResult> UpdateEndAt(
            [FromRoute] int empID,
            [FromBody] UpdateEndAtDto model
        )
        {
            try
            {
                if (model == null)
                    return BadRequest("Invalid payload.");

                var endAt = model.EndAt ?? DateTime.UtcNow;
                var status = string.IsNullOrWhiteSpace(model.Status) ? "Completed" : model.Status;

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql =
                    @"
UPDATE Assignment
SET EndAt = @EndAt,
    Status = @Status
WHERE EmpID = @empID AND Status = 'Active'";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@EndAt", SqlDbType.DateTime2).Value = endAt;
                cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;
                cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound("Assignment not found or not Active.");

                return Ok("EndAt updated successfully.");
            }
            catch (Exception ex)
            {
                return Problem(title: "UpdateEndAt failed", detail: ex.Message, statusCode: 500);
            }
        }

        // DELETE: api/Assignment/emp/{empID}
        // ลบ Assignment ทั้งหมดของ EmpID นั้น และหักชั่วโมงออกจาก EICC_Control (เดือนปัจจุบัน)
        [HttpDelete("emp/{empID:int}")]
        public async Task<IActionResult> DeleteByEmpID([FromRoute] int empID)
        {
            SqlTransaction? tx = null;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                tx = conn.BeginTransaction();

                // รวมชั่วโมงก่อนลบ
                var hoursSql =
                    @"
SELECT ISNULL(SUM(DATEDIFF(HOUR, StartAt, EndAt)), 0)
FROM Assignment
WHERE EmpID = @empID";

                using var hoursCmd = new SqlCommand(hoursSql, conn, tx);
                hoursCmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                var totalHoursObj = await hoursCmd.ExecuteScalarAsync();
                var totalHours = Convert.ToInt32(totalHoursObj);

                // ลบ assignment ทั้งหมด
                var delSql = "DELETE FROM Assignment WHERE EmpID = @empID";
                using var delCmd = new SqlCommand(delSql, conn, tx);
                delCmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                var rowsAffected = await delCmd.ExecuteNonQueryAsync();

                if (rowsAffected == 0)
                {
                    tx.Rollback();
                    return NotFound("No assignments found for this employee.");
                }

                // อัปเดต EICC_Control ด้วยจำนวนชั่วโมงที่คำนวณไว้
                var updSql =
                    @"
UPDATE EICC_Control
SET TotalHours = ISNULL(TotalHours,0) - @totalHours
WHERE EmpID = @empID AND MonthYear = FORMAT(GETDATE(),'yyyyMM')";

                using var updCmd = new SqlCommand(updSql, conn, tx);
                updCmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                updCmd.Parameters.Add("@totalHours", SqlDbType.Int).Value = totalHours;
                await updCmd.ExecuteNonQueryAsync();

                tx.Commit();
                return Ok("All assignments deleted and EICC updated.");
            }
            catch (Exception ex)
            {
                try
                {
                    tx?.Rollback();
                }
                catch
                { /* ignore */
                }
                return Problem(title: "DeleteByEmpID failed", detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/Assignment/{id}/approve
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> ApproveAssignment([FromRoute] int id)
        {
            try
            {
                // ค้นหางานที่ต้องการยืนยัน
                var assignment = await GetAssignmentById(id); // ฟังก์ชันนี้จะดึงข้อมูลของ Assignment ตาม id
                if (assignment == null)
                    return NotFound("Assignment not found.");

                // ตรวจสอบสถานะ Assignment ว่าเป็น Pending
                if (assignment.Status != "Pending")
                    return BadRequest("Assignment is not in 'Pending' status.");

                // อัปเดตสถานะให้เป็น Active
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();

                var sql =
                    @"UPDATE Assignment
            SET Status = 'Active'
            WHERE AssignmentID = @id AND Status = 'Pending'";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound("Assignment not found or already approved.");

                // ส่งการแจ้งเตือนผ่านอีเมล
                var supervisorEmail = await GetSupervisorEmailForEmployee(assignment.EmpID);
                if (!string.IsNullOrEmpty(supervisorEmail))
                {
                    var subject = "Assignment Approved";
                    var body = $"Assignment ID {id} has been approved and is now Active.";
                    await SendNotification(
                        new NotificationRequest
                        {
                            EmpID = assignment.EmpID,
                            ToProcess = assignment.ToProcess,
                            ToBiz = assignment.ToBiz,
                        }
                    );
                }

                return Ok("Assignment approved successfully.");
            }
            catch (Exception ex)
            {
                return Problem(title: "Approval failed", detail: ex.Message, statusCode: 500);
            }
        }

        [HttpPost("notify")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                var supervisorEmail = "panwasalimsuwan@gmail.com"; // หรือใช้ฟังก์ชัน GetSupervisorEmailForProcess() ถ้าคุณต้องการดึงอีเมลจากฐานข้อมูล
                var subject = "Employee Assignment Notification";
                var body =
                    $"พนักงาน {request.EmpID} ได้ย้ายไปยังแผนก {request.ToProcess} ที่ธุรกิจ {request.ToBiz}. กรุณาตรวจสอบการย้ายงาน.";

                // ส่งอีเมลแจ้งเตือน
                EmailService.SendEmail(supervisorEmail, subject, body);

                return Ok("Email sent successfully.");
            }
            catch (Exception ex)
            {
                return Problem(title: "Email sending failed", detail: ex.Message, statusCode: 500);
            }
        }

        private async Task<Assignment> GetAssignmentById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql =
                @"SELECT AssignmentID, EmpID, FromBiz, FromProcess, ToBiz, ToProcess, 
               SkillGroup, StartAt, EndAt, Status
        FROM Assignment
        WHERE AssignmentID = @id";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new Assignment
                {
                    AssignmentID = reader.GetInt32(0),
                    EmpID = reader.GetInt32(1),
                    FromBiz = reader.IsDBNull(2) ? null : reader.GetString(2),
                    FromProcess = reader.IsDBNull(3) ? null : reader.GetString(3),
                    ToBiz = reader.IsDBNull(4) ? null : reader.GetString(4),
                    ToProcess = reader.IsDBNull(5) ? null : reader.GetString(5),
                    SkillGroup = reader.IsDBNull(6) ? null : reader.GetString(6),
                    StartAt = reader.GetDateTime(7),
                    EndAt = reader.IsDBNull(8) ? (DateTime?)null : reader.GetDateTime(8),
                    Status = reader.IsDBNull(9) ? null : reader.GetString(9),
                };
            }

            return null;
        }

        private async Task<string> GetSupervisorEmailForProcess(string process)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            // ปรับ query ให้ใช้ตาราง EmployeeInfo
            var query =
                @"
        SELECT Email
        FROM EmployeeInfo
        WHERE Process = @process"; // ใช้ Process จาก EmployeeInfo

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@process", SqlDbType.NVarChar, 100).Value = process;

            try
            {
                var result = await cmd.ExecuteScalarAsync();
                if (result != null)
                {
                    return result.ToString(); // ถ้ามีผลลัพธ์ก็คืนค่าอีเมล
                }
                else
                {
                    // ถ้าไม่พบข้อมูลที่ตรงกับ process
                    return null; // หรือสามารถส่งข้อความอื่นได้ เช่น "No supervisor found for this process"
                }
            }
            catch (Exception ex)
            {
                // ในกรณีที่เกิดข้อผิดพลาด
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null; // หรือสามารถส่งข้อความแสดงข้อผิดพลาดที่ต้องการ
            }
        }

        private async Task<string> GetSupervisorEmailForEmployee(int empID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var query =
                @"SELECT Email
          FROM EmployeeInfo
          WHERE EmpID = @empID";

            using var cmd = new SqlCommand(query, conn);
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;

            try
            {
                var result = await cmd.ExecuteScalarAsync();
                return result?.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred: {ex.Message}");
                return null;
            }
        }

        //         public async Task<IActionResult> AssignEmployeeToProcess([FromBody] Assignment assignment)
        // {
        //     try
        //     {
        //         // ตรวจสอบหัวหน้างานของ ToProcess
        //         var supervisorEmail = await GetSupervisorEmailForProcess(assignment.ToProcess);
        //         if (supervisorEmail != null)
        //         {
        //             var subject = "Employee Assignment Notification";
        //             var body = $"พนักงาน {assignment.EmpID} ได้ย้ายไปยังแผนก {assignment.ToProcess}. กรุณาตรวจสอบการย้ายงาน.";

        //             // ส่งอีเมลแจ้งเตือนหัวหน้างาน
        //             EmailService.SendEmail(supervisorEmail, subject, body);
        //         }

        //         // ดำเนินการสร้าง Assignment ต่อไป
        //         return await Create(assignment);
        //     }
        //     catch (Exception ex)
        //     {
        //         return Problem(title: "Error assigning employee", detail: ex.Message, statusCode: 500);
        //     }
        // }

        public async Task<IActionResult> AssignEmployeeToProcess([FromBody] Assignment assignment)
        {
            try
            {
                // ส่งอีเมลแจ้งเตือนไปที่ panwasalimsuwan@gmail.com
                var supervisorEmail = "panwasalimsuwan@gmail.com"; // เปลี่ยนเป็นอีเมลที่ต้องการ
                var subject = "Employee Assignment Notification";
                var body =
                    $"พนักงาน {assignment.EmpID} ได้ย้ายไปยังแผนก {assignment.ToProcess}. กรุณาตรวจสอบการย้ายงาน.";

                // Debug log เพื่อดูว่าโค้ดเข้าถึงจุดนี้หรือไม่
                Console.WriteLine("Sending email to: " + supervisorEmail);

                // ส่งอีเมลแจ้งเตือนหัวหน้างาน
                EmailService.SendEmail(supervisorEmail, subject, body);

                // Debug log หลังจากส่งอีเมล
                Console.WriteLine("Email sent successfully!");

                // ดำเนินการสร้าง Assignment ต่อไป
                return await Create(assignment);
            }
            catch (Exception ex)
            {
                return Problem(
                    title: "Error assigning employee",
                    detail: ex.Message,
                    statusCode: 500
                );
            }
        }

        public class EmailService
        {
            public static void SendEmail(string toEmail, string subject, string body)
            {
                try
                {
                    // สร้าง MimeMessage สำหรับส่งอีเมล
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Your Company", "your-email@example.com"));
                    message.To.Add(new MailboxAddress("Recipient", toEmail));
                    message.Subject = subject;

                    // สร้างเนื้อหาอีเมล
                    var bodyBuilder = new BodyBuilder { TextBody = body };

                    message.Body = bodyBuilder.ToMessageBody();

                    // เชื่อมต่อและส่งอีเมลผ่าน SMTP
                    using (var client = new SmtpClient())
                    {
                        client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                        // ใช้ App Password
                        client.Authenticate("panwasalimsuwan@gmail.com", "xemtsrhrnzpddwjl"); // ใช้ App Password แทนรหัสผ่านปกติ

                        client.Send(message);
                        client.Disconnect(true);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error sending email: " + ex.Message);
                    throw new Exception("Error sending email: " + ex.Message);
                }
            }
        }
    }
}
