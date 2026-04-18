using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Text;
using System.Threading.Tasks;
using Api.Hubs;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : ControllerBase
    {
        private readonly string _connectionString;
        private readonly IConfiguration _configuration;
        private readonly IHubContext<AttendanceHub> _hub;

        public AssignmentController(IConfiguration configuration, IHubContext<AttendanceHub> hub)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
            _configuration = configuration;
            _hub = hub;
        }

        private static DateTime ThaiNow =>
            TimeZoneInfo.ConvertTimeFromUtc(
                DateTime.UtcNow,
                TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time")
            );

        // GET: api/Assignment
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] string? toBiz,
            [FromQuery] string? toProcess
        )
        {
            try
            {
                await AutoCloseAssignments(); // ✅ ตรงนี้

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
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Assignment assignment)
        {
            try
            {
                if (assignment == null)
                    return BadRequest("Invalid payload.");

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

                // var startAt = assignment.StartAt == default ? DateTime.UtcNow : assignment.StartAt;
                var startAt = assignment.StartAt == default ? ThaiNow : assignment.StartAt;
                object endAt =
                    (assignment.EndAt == default) ? DBNull.Value : (object)assignment.EndAt;
                var status = string.IsNullOrWhiteSpace(assignment.Status)
                    ? "Pending"
                    : assignment.Status;

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

                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
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

        // POST: api/Assignment/auto-assign
        [HttpPost("auto-assign")]
        public async Task<IActionResult> AutoAssign([FromBody] AutoAssignRequest req)
        {
            if (req == null || string.IsNullOrWhiteSpace(req.Process))
                return BadRequest("Process is required.");

            var candidates = await GetAutoAssignCandidates(req);
            if (candidates.Count == 0)
                return Ok(
                    new AutoAssignResult
                    {
                        Assigned = new(),
                        AssignedCount = 0,
                        RequestedCount = req.HeadcountNeed,
                        ShortfallCount = req.HeadcountNeed,
                        Message = "ไม่พบพนักงานที่ตรงเงื่อนไข",
                    }
                );

            var sorted = candidates
                .OrderByDescending(c => c.TotalSkill)
                .ThenBy(c => c.TotalTime)
                .ToList();
            var assigned = new List<AutoAssignedEmployee>();
            var take = Math.Min(req.HeadcountNeed, sorted.Count);

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            foreach (var emp in sorted.Take(take))
            {
                var sql =
                    @"
INSERT INTO Assignment
  (EmpID, FromBiz, FromProcess, ToBiz, ToProcess, SkillGroup, StartAt, EndAt, Status)
OUTPUT INSERTED.AssignmentID
VALUES (@EmpID,@FromBiz,@FromProcess,@ToBiz,@ToProcess,@SkillGroup,@StartAt,NULL,'Pending')";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@EmpID", SqlDbType.Int).Value = emp.EmpID;
                cmd.Parameters.Add("@FromBiz", SqlDbType.NVarChar, 100).Value =
                    (object?)emp.FromBiz ?? DBNull.Value;
                cmd.Parameters.Add("@FromProcess", SqlDbType.NVarChar, 100).Value =
                    (object?)emp.FromProcess ?? DBNull.Value;
                cmd.Parameters.Add("@ToBiz", SqlDbType.NVarChar, 100).Value =
                    string.IsNullOrWhiteSpace(req.ToBiz) ? (object)DBNull.Value : req.ToBiz;
                cmd.Parameters.Add("@ToProcess", SqlDbType.NVarChar, 100).Value = req.Process;
                cmd.Parameters.Add("@SkillGroup", SqlDbType.NVarChar, 100).Value =
                    req.SkillGroup ?? "General";
                // cmd.Parameters.Add("@StartAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@StartAt", SqlDbType.DateTime2).Value = ThaiNow;

                var newId = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                assigned.Add(
                    new AutoAssignedEmployee
                    {
                        AssignmentID = newId,
                        EmpID = emp.EmpID,
                        FirstName = emp.FirstName,
                        LastName = emp.LastName,
                        TotalSkill = emp.TotalSkill,
                        TotalTime = emp.TotalTime,
                    }
                );

                // ✅ แจ้งหัวหน้า FromProcess ให้ approve (ไม่ใช่แจ้งพนักงาน)
                var empCopy = emp;
                var reqCopy = req;
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var fromSupervisorEmail = await GetEmailFromAdminByProcess(
                            empCopy.FromBiz,
                            empCopy.FromProcess
                        );
                        if (string.IsNullOrEmpty(fromSupervisorEmail))
                            fromSupervisorEmail = "panwasalimsuwan@gmail.com";

                        // EmailService.SendEmail(fromSupervisorEmail,
                        new EmailService(_configuration).SendEmail(
                            "panwasalimsuwan@gmail.com",
                            $"[รออนุมัติ] Auto-Assign พนักงาน {empCopy.EmpID} → {reqCopy.Process}",
                            $"ระบบได้ Auto-Assign พนักงาน {empCopy.EmpID} {empCopy.FirstName} {empCopy.LastName}\n"
                                + $"ไปช่วยงาน Process: {reqCopy.Process} / Biz: {reqCopy.ToBiz}\n"
                                + $"Skill รวม: {empCopy.TotalSkill} | OT: {empCopy.TotalTime:F1}h\n"
                                + $"กรุณาเข้า Dashboard เพื่ออนุมัติ"
                        );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[AutoAssign] Email error: {ex.Message}");
                    }
                });
            }

            var shortfall = req.HeadcountNeed - assigned.Count;
            return Ok(
                new AutoAssignResult
                {
                    Assigned = assigned,
                    AssignedCount = assigned.Count,
                    RequestedCount = req.HeadcountNeed,
                    ShortfallCount = shortfall,
                    Message =
                        shortfall > 0
                            ? $"Assign ได้ {assigned.Count} คน ขาดอีก {shortfall} คน"
                            : $"Assign ครบ {assigned.Count} คน",
                }
            );
        }

        // helper: auto-assign candidates — ดึง FromBiz/FromProcess ด้วย
        private async Task<List<AutoCandidate>> GetAutoAssignCandidates(AutoAssignRequest req)
        {
            var list = new List<AutoCandidate>();
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql =
                @"
SELECT
    s.EmpID,
    s.FirstName,
    s.LastName,
    ISNULL(s.Material,0)+ISNULL(s.Operation,0)+
    ISNULL(s.MachineSAB1,0)+ISNULL(s.MachineSAB2,0)+
    ISNULL(s.MachineSAB3,0)+ISNULL(s.Inspection,0) AS TotalSkill,
    ISNULL(ot.TotalHours,0) AS TotalTime,
    e.Biz     AS FromBiz,
    e.Process AS FromProcess
FROM Skill s
INNER JOIN Attendance a
    ON  a.EmpID = s.EmpID
    AND CAST(a.Date AS DATE) = @workDate
    AND a.Status NOT IN ('Absent')
LEFT JOIN (
    SELECT EmpID, SUM(TotalHours) AS TotalHours
    FROM   EICC_Control
    WHERE  Year = YEAR(GETDATE())
    GROUP BY EmpID
) ot ON ot.EmpID = s.EmpID
INNER JOIN EmployeeInfo e ON e.EmpID = s.EmpID
WHERE e.Position NOT IN (
    'Supervisor','Foreman','Section Chief','Manager',
    'Senior Engineer','Executive Director','General Manager',
    'Assistant General Manager','Officer','Senior Officer'
)
AND NOT EXISTS (
    SELECT 1 FROM Assignment aa
    WHERE aa.EmpID = s.EmpID AND aa.Status IN ('Active','Pending') AND aa.Status IN ('Active','Pending','Returning')
)
AND (
    ISNULL(s.Material,0)+ISNULL(s.Operation,0)+
    ISNULL(s.MachineSAB1,0)+ISNULL(s.MachineSAB2,0)+
    ISNULL(s.MachineSAB3,0)+ISNULL(s.Inspection,0)
) > 0";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@workDate", SqlDbType.Date).Value =
                req.WorkDate == default ? DateTime.Today : req.WorkDate.Date;

            using var r = await cmd.ExecuteReaderAsync();
            while (await r.ReadAsync())
                list.Add(
                    new AutoCandidate
                    {
                        EmpID = r.GetInt32(0),
                        FirstName = r.IsDBNull(1) ? "" : r.GetString(1),
                        LastName = r.IsDBNull(2) ? "" : r.GetString(2),
                        TotalSkill = r.GetInt32(3),
                        TotalTime = r.GetDouble(4),
                        FromBiz = r.IsDBNull(5) ? null : r.GetString(5),
                        FromProcess = r.IsDBNull(6) ? null : r.GetString(6),
                    }
                );

            return list;
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

                // ✅ เซ็ต EndAt เมื่อ Completed หรือ Cancelled
                string sql =
                    (status == "Completed" || status == "Cancelled")
                        ? "UPDATE Assignment SET Status=@status, EndAt=@endAt WHERE AssignmentID=@id"
                        : "UPDATE Assignment SET Status=@status WHERE AssignmentID=@id";

                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = status;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                if (status == "Completed" || status == "Cancelled")
                    // cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                    cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = ThaiNow;

                var rows = await cmd.ExecuteNonQueryAsync();
                if (rows == 0)
                    return NotFound("Assignment not found.");
                await _hub.Clients.All.SendAsync("AssignmentUpdated"); // ✅ เพิ่มตรงนี้
                return Ok("Status updated successfully.");
            }
            catch (Exception ex)
            {
                return Problem(title: "UpdateStatus failed", detail: ex.Message, statusCode: 500);
            }
        }

        public class UpdateEndAtDto
        {
            public DateTime? EndAt { get; set; }
            public string? Status { get; set; }
        }

        // PUT: api/Assignment/{empID}
        [HttpPut("{empID:int}")]
        public async Task<IActionResult> UpdateEndAt(
            [FromRoute] int empID,
            [FromBody] UpdateEndAtDto model
        )
        {
            // var endAt = model?.EndAt ?? DateTime.UtcNow;
            var endAt = model?.EndAt ?? ThaiNow;
            var status = string.IsNullOrWhiteSpace(model?.Status) ? "Completed" : model.Status;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql =
                @"UPDATE Assignment
                        SET EndAt=@EndAt, Status=@Status
                        WHERE EmpID=@empID AND Status IN ('Active','Pending')";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@EndAt", SqlDbType.DateTime2).Value = endAt;
            cmd.Parameters.Add("@Status", SqlDbType.NVarChar, 50).Value = status;
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;

            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows == 0)
                return NotFound("Assignment not found or already closed.");
            return Ok("EndAt updated successfully.");
        }

        // DELETE: api/Assignment/emp/{empID}
        [HttpDelete("emp/{empID:int}")]
        public async Task<IActionResult> DeleteByEmpID([FromRoute] int empID)
        {
            SqlTransaction? tx = null;
            try
            {
                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync();
                tx = conn.BeginTransaction();

                var hoursSql =
                    "SELECT ISNULL(SUM(DATEDIFF(HOUR,StartAt,EndAt)),0) FROM Assignment WHERE EmpID=@empID";
                using var hoursCmd = new SqlCommand(hoursSql, conn, tx);
                hoursCmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                var totalHours = Convert.ToInt32(await hoursCmd.ExecuteScalarAsync());

                using var delCmd = new SqlCommand(
                    "DELETE FROM Assignment WHERE EmpID=@empID",
                    conn,
                    tx
                );
                delCmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
                var rowsAffected = await delCmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    tx.Rollback();
                    return NotFound("No assignments found.");
                }

                var updSql =
                    @"UPDATE EICC_Control SET TotalHours=ISNULL(TotalHours,0)-@totalHours
                               WHERE EmpID=@empID AND Year=YEAR(GETDATE())";
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
                catch { }
                return Problem(title: "DeleteByEmpID failed", detail: ex.Message, statusCode: 500);
            }
        }

        // PUT: api/Assignment/{id}/approve
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> ApproveAssignment([FromRoute] int id)
        {
            var assignment = await GetAssignmentById(id);
            if (assignment == null)
                return NotFound("Assignment not found.");
            if (assignment.Status != "Pending")
                return BadRequest("Assignment is not Pending.");

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql =
                "UPDATE Assignment SET Status='Active' WHERE AssignmentID=@id AND Status='Pending'";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            var rows = await cmd.ExecuteNonQueryAsync();
            if (rows == 0)
                return NotFound("Assignment not found or already approved.");

            await _hub.Clients.All.SendAsync("AssignmentUpdated"); // ✅ เพิ่มตรงนี้

            // ✅ ส่ง email หลัง approve
            _ = Task.Run(async () =>
            {
                try
                {
                    // 1. แจ้งพนักงาน (ดึง email จาก Admin table)
                    var empEmail = await GetEmailFromAdmin(assignment.EmpID);
                    if (!string.IsNullOrEmpty(empEmail))
                    {
                        // EmailService.SendEmail(empEmail,
                        new EmailService(_configuration).SendEmail(
                            "panwasalimsuwan@gmail.com",
                            $"[อนุมัติแล้ว] คุณได้รับมอบหมายงานที่ {assignment.ToProcess}",
                            $"Assignment ของคุณได้รับการอนุมัติแล้ว\n"
                                + $"กรุณาไปช่วยงาน Process: {assignment.ToProcess} / Biz: {assignment.ToBiz}\n"
                                + $"สถานะ: Active"
                        );
                    }

                    // 2. แจ้งหัวหน้า ToProcess ว่ามีคนมาช่วยแล้ว
                    var toLeaderEmail = await GetEmailFromAdminByProcess(
                        assignment.ToBiz,
                        assignment.ToProcess
                    );
                    if (!string.IsNullOrEmpty(toLeaderEmail))
                    {
                        // EmailService.SendEmail(toLeaderEmail,
                        new EmailService(_configuration).SendEmail(
                            "panwasalimsuwan@gmail.com",
                            $"[แจ้งเตือน] พนักงาน {assignment.EmpID} จะมาช่วยงาน {assignment.ToProcess}",
                            $"Assignment ได้รับการอนุมัติแล้ว\n"
                                + $"พนักงาน {assignment.EmpID} จะมาช่วยงาน {assignment.ToProcess} / {assignment.ToBiz}"
                        );
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email error: {ex.Message}");
                }
            });

            return Ok(new { message = "Assignment approved.", assignmentID = id });
        }

        // POST: api/Assignment/notify — แจ้งหัวหน้า FromProcess ให้ approve
        [HttpPost("notify")]
        public async Task<IActionResult> SendNotification([FromBody] NotificationRequest request)
        {
            try
            {
                // ✅ แจ้งหัวหน้าของ FromBiz/FromProcess (ต้นทาง) ให้ approve
                var fromLeaderEmail = await GetEmailFromAdminByProcess(
                    request.FromBiz,
                    request.FromProcess
                );

                if (string.IsNullOrEmpty(fromLeaderEmail))
                    fromLeaderEmail = "panwasalimsuwan@gmail.com"; // fallback

                var subject = $"[รออนุมัติ] คำขอย้ายพนักงาน {request.EmpID} → {request.ToProcess}";
                var body =
                    $"มีคำขอย้ายพนักงาน รหัส {request.EmpID}\n"
                    + $"จาก Process: {request.FromProcess} / Biz: {request.FromBiz}\n"
                    + $"ไปยัง Process: {request.ToProcess} / Biz: {request.ToBiz}\n"
                    + $"กรุณาเข้า Dashboard เพื่ออนุมัติ";

                // EmailService.SendEmail(fromLeaderEmail, subject, body);
                new EmailService(_configuration).SendEmail(
                    "panwasalimsuwan@gmail.com",
                    subject,
                    body
                );
                return Ok("Email sent to supervisor.");
            }
            // catch (Exception ex)
            // {
            //     return Problem(title: "Email sending failed", detail: ex.Message, statusCode: 500);
            // }
            catch (Exception ex)
            {
                Console.WriteLine("❌ Email error: " + ex.Message);
                // ❗ ไม่ต้อง return 500
            }

            return Ok("Email attempted");
        }

        // GET: api/Assignment/with-employees
        [HttpGet("with-employees")]
        public async Task<IActionResult> GetWithEmployees(
            [FromQuery] string? status,
            [FromQuery] string? toBiz,
            [FromQuery] string? toProcess,
            [FromQuery] string? fromBiz,
            [FromQuery] string? fromProcess
        )
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sb = new StringBuilder(
                @"
SELECT 
    a.AssignmentID, a.EmpID,
    e.FirstName, e.LastName, e.Position,
    a.FromBiz, a.FromProcess,
    a.ToBiz, a.ToProcess,
    a.SkillGroup, a.StartAt, a.EndAt, a.Status
FROM Assignment a
LEFT JOIN EmployeeInfo e ON e.EmpID = a.EmpID
WHERE 1=1"
            );

            using var cmd = new SqlCommand();
            cmd.Connection = conn;

            if (!string.IsNullOrWhiteSpace(status))
            {
                sb.Append(" AND a.Status = @status");
                cmd.Parameters.Add("@status", SqlDbType.NVarChar, 50).Value = status;
            }
            if (!string.IsNullOrWhiteSpace(toBiz))
            {
                sb.Append(" AND a.ToBiz = @toBiz");
                cmd.Parameters.Add("@toBiz", SqlDbType.NVarChar, 100).Value = toBiz;
            }
            if (!string.IsNullOrWhiteSpace(toProcess))
            {
                sb.Append(" AND a.ToProcess = @toProcess");
                cmd.Parameters.Add("@toProcess", SqlDbType.NVarChar, 100).Value = toProcess;
            }
            if (!string.IsNullOrWhiteSpace(fromBiz))
            {
                sb.Append(" AND a.FromBiz = @fromBiz");
                cmd.Parameters.Add("@fromBiz", SqlDbType.NVarChar, 100).Value = fromBiz;
            }
            if (!string.IsNullOrWhiteSpace(fromProcess))
            {
                sb.Append(" AND a.FromProcess = @fromProcess");
                cmd.Parameters.Add("@fromProcess", SqlDbType.NVarChar, 100).Value = fromProcess;
            }

            sb.Append(" ORDER BY a.StartAt DESC");
            cmd.CommandText = sb.ToString();

            var results = new List<object>();
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(
                    new
                    {
                        assignmentID = reader.GetInt32(0),
                        empID = reader.GetInt32(1),
                        firstName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                        lastName = reader.IsDBNull(3) ? "" : reader.GetString(3),
                        position = reader.IsDBNull(4) ? "" : reader.GetString(4),
                        fromBiz = reader.IsDBNull(5) ? null : reader.GetString(5),
                        fromProcess = reader.IsDBNull(6) ? null : reader.GetString(6),
                        toBiz = reader.IsDBNull(7) ? null : reader.GetString(7),
                        toProcess = reader.IsDBNull(8) ? null : reader.GetString(8),
                        skillGroup = reader.IsDBNull(9) ? null : reader.GetString(9),
                        startAt = reader.GetDateTime(10),
                        endAt = reader.IsDBNull(11) ? (DateTime?)null : reader.GetDateTime(11),
                        status = reader.IsDBNull(12) ? null : reader.GetString(12),
                    }
                );
            }
            return Ok(results);
        }

        // ─── Private helpers ───────────────────────────────────────

        private async Task<Assignment?> GetAssignmentById(int id)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql =
                @"SELECT AssignmentID,EmpID,FromBiz,FromProcess,ToBiz,ToProcess,
                               SkillGroup,StartAt,EndAt,Status
                        FROM Assignment WHERE AssignmentID=@id";
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

        // ✅ ดึง email พนักงานจาก Admin table (ใช้หลัง approve)
        private async Task<string?> GetEmailFromAdmin(int empID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT Email FROM Admin WHERE EmpID = @empID";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        // ✅ ดึง email หัวหน้าของ Biz+Process จาก Admin table
        private async Task<string?> GetEmailFromAdminByProcess(string? biz, string? process)
        {
            if (string.IsNullOrEmpty(biz) && string.IsNullOrEmpty(process))
                return null;

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql =
                @"
SELECT TOP 1 a.Email
FROM Admin a
JOIN EmployeeInfo e ON e.EmpID = a.EmpID
WHERE a.Role IN ('LeaderMFG','LeaderHR','Admin')
  AND (@biz     IS NULL OR e.Biz     = @biz)
  AND (@process IS NULL OR e.Process = @process)";

            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@biz", SqlDbType.NVarChar, 100).Value = string.IsNullOrEmpty(biz)
                ? (object)DBNull.Value
                : biz;
            cmd.Parameters.Add("@process", SqlDbType.NVarChar, 100).Value = string.IsNullOrEmpty(
                process
            )
                ? (object)DBNull.Value
                : process;

            var result = await cmd.ExecuteScalarAsync();
            return result?.ToString();
        }

        // deprecated helpers (ใช้ Admin table แทนแล้ว)
        private async Task<string?> GetSupervisorEmailForProcess(string process)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT TOP 1 Email FROM EmployeeInfo WHERE Process=@process";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@process", SqlDbType.NVarChar, 100).Value = process;
            return (await cmd.ExecuteScalarAsync())?.ToString();
        }

        private async Task<string?> GetSupervisorEmailForEmployee(int empID)
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();
            var sql = "SELECT Email FROM EmployeeInfo WHERE EmpID=@empID";
            using var cmd = new SqlCommand(sql, conn);
            cmd.Parameters.Add("@empID", SqlDbType.Int).Value = empID;
            return (await cmd.ExecuteScalarAsync())?.ToString();
        }

        [HttpPost("notify-dashboard")]
        public async Task<IActionResult> NotifyDashboard([FromBody] NotificationRequest request)
        {
            return Ok("acknowledged");
        }

        public class EmailService
        {
            private readonly IConfiguration _configuration;

            public EmailService(IConfiguration configuration)
            {
                _configuration = configuration;
            }

            public void SendEmail(string toEmail, string subject, string body)
            {
                try
                {
                    var ApiKey =
                        _configuration["SendGrid:ApiKey"]
                        ?? throw new InvalidOperationException("SendGrid:ApiKey is not configured");

                    var client = new SendGridClient(ApiKey);

                    var msg = MailHelper.CreateSingleEmail(
                        // from:             new EmailAddress("panwasalimsuwan@gmail.com", "MFG Dashboard"),
                        from: new EmailAddress(
                            "s6404062630465@email.kmutnb.ac.th",
                            "MFG Dashboard"
                        ),
                        to: new EmailAddress(toEmail),
                        subject: subject,
                        plainTextContent: body,
                        htmlContent: null
                    );

                    var response = client.SendEmailAsync(msg).GetAwaiter().GetResult();
                    Console.WriteLine($"SendGrid status: {response.StatusCode}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Email error: " + ex.Message);
                    // ไม่ throw เพราะ caller ส่วนใหญ่อยู่ใน Task.Run
                }
            }
        }

        // PUT: api/Assignment/{id}/return
        // ToProcess กด → Completed ทันที + แจ้ง FromProcess
        // FromProcess กด → Returning + แจ้ง ToProcess รอ Confirm
        [HttpPut("{id:int}/return")]
        public async Task<IActionResult> ReturnEmployee(
            [FromRoute] int id,
            [FromQuery] string callerSide
        ) // "from" หรือ "to"
        {
            var assignment = await GetAssignmentById(id);
            if (assignment == null)
                return NotFound("Assignment not found.");
            if (assignment.Status != "Active")
                return BadRequest("Assignment is not Active.");

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            if (callerSide == "to")
            {
                // ToProcess กด → Completed ทันที
                var sql =
                    "UPDATE Assignment SET Status='Completed', EndAt=@endAt WHERE AssignmentID=@id";
                using var cmd = new SqlCommand(sql, conn);
                // cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
                cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = ThaiNow;
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                await cmd.ExecuteNonQueryAsync();
                await _hub.Clients.All.SendAsync("AssignmentUpdated"); // ✅ เพิ่มตรงนี้

                // แจ้ง FromProcess ว่าพนักงานกลับมาแล้ว
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var fromEmail = await GetEmailFromAdminByProcess(
                            assignment.FromBiz,
                            assignment.FromProcess
                        );
                        if (!string.IsNullOrEmpty(fromEmail))
                            new EmailService(_configuration).SendEmail(
                                "panwasalimsuwan@gmail.com",
                                $"[พนักงานกลับมาแล้ว] {assignment.EmpID} คืนจาก {assignment.ToProcess}",
                                $"พนักงาน {assignment.EmpID} ได้กลับมายัง {assignment.FromProcess} / {assignment.FromBiz} แล้ว\n"
                                    + $"คืนโดย: {assignment.ToProcess}"
                            );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Email error: {ex.Message}");
                    }
                });

                return Ok(new { message = "Completed.", assignmentID = id });
            }
            else // callerSide == "from"
            {
                // FromProcess กด → Returning รอ ToProcess Confirm
                var sql = "UPDATE Assignment SET Status='Returning' WHERE AssignmentID=@id";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
                await cmd.ExecuteNonQueryAsync();
                await _hub.Clients.All.SendAsync("AssignmentUpdated"); // ✅ เพิ่มตรงนี้

                // แจ้ง ToProcess ว่าถูกขอพนักงานคืน รอยืนยัน
                _ = Task.Run(async () =>
                {
                    try
                    {
                        var toEmail = await GetEmailFromAdminByProcess(
                            assignment.ToBiz,
                            assignment.ToProcess
                        );
                        if (!string.IsNullOrEmpty(toEmail))
                            new EmailService(_configuration).SendEmail(
                                "panwasalimsuwan@gmail.com",
                                $"[ขอคืนพนักงาน] {assignment.EmpID} จาก {assignment.FromProcess}",
                                $"หัวหน้า {assignment.FromProcess} ขอคืนพนักงาน {assignment.EmpID}\n"
                                    + $"กรุณาเข้า Dashboard เพื่อยืนยันการคืนพนักงาน"
                            );
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Email error: {ex.Message}");
                    }
                });

                return Ok(
                    new
                    {
                        message = "Returning - waiting for ToProcess confirm.",
                        assignmentID = id,
                    }
                );
            }
        }

        // PUT: api/Assignment/{id}/confirm-return
        // ToProcess กด "ยืนยันรับทราบ" → Completed
        [HttpPut("{id:int}/confirm-return")]
        public async Task<IActionResult> ConfirmReturn([FromRoute] int id)
        {
            var assignment = await GetAssignmentById(id);
            if (assignment == null)
                return NotFound("Assignment not found.");
            if (assignment.Status != "Returning")
                return BadRequest("Assignment is not in Returning state.");

            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var sql =
                "UPDATE Assignment SET Status='Completed', EndAt=@endAt WHERE AssignmentID=@id";
            using var cmd = new SqlCommand(sql, conn);
            // cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = DateTime.UtcNow;
            cmd.Parameters.Add("@endAt", SqlDbType.DateTime2).Value = ThaiNow;
            cmd.Parameters.Add("@id", SqlDbType.Int).Value = id;
            await cmd.ExecuteNonQueryAsync();
            await _hub.Clients.All.SendAsync("AssignmentUpdated"); // ✅ เพิ่มตรงนี้

            // แจ้ง FromProcess ว่า ToProcess ยืนยันแล้ว
            _ = Task.Run(async () =>
            {
                try
                {
                    var fromEmail = await GetEmailFromAdminByProcess(
                        assignment.FromBiz,
                        assignment.FromProcess
                    );
                    if (!string.IsNullOrEmpty(fromEmail))
                        new EmailService(_configuration).SendEmail(
                            "panwasalimsuwan@gmail.com",
                            $"[ยืนยันแล้ว] พนักงาน {assignment.EmpID} กลับมายัง {assignment.FromProcess}",
                            $"{assignment.ToProcess} ยืนยันการคืนพนักงาน {assignment.EmpID} แล้ว\n"
                                + $"พนักงานพร้อมกลับมายัง {assignment.FromProcess} / {assignment.FromBiz}"
                        );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Email error: {ex.Message}");
                }
            });

            return Ok(new { message = "Return confirmed. Completed.", assignmentID = id });
        }

        private async Task AutoCloseAssignments()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            var cmd = new SqlCommand(
                @"
UPDATE A
SET A.Status = 'Completed',
    A.EndAt = T.Timestamp
FROM Assignment A
OUTER APPLY (
    SELECT TOP 1 T.Timestamp, T.CameraID
    FROM Transactions T
    WHERE T.EmpID = A.EmpID
      AND CAST(T.Timestamp AS DATE) = CAST(GETDATE() AS DATE)
    ORDER BY T.Timestamp DESC  -- ✅ เอาล่าสุดก่อน
) T
WHERE A.Status = 'Active'
  AND T.CameraID = 1           -- ✅ ล่าสุดต้องเป็น CameraID=1 ด้วย
  AND T.Timestamp IS NOT NULL
  AND DATEDIFF(MINUTE, T.Timestamp, GETDATE()) >= 30  -- ✅ ป้องกันออกแป๊บแล้วกลับ
",
                conn
            );

            await cmd.ExecuteNonQueryAsync();
        }

        // ─── DTOs ───────────────────────────────────────────────────
        public class AutoAssignRequest
        {
            public string Process { get; set; } = "";
            public string? ToBiz { get; set; }
            public string? SkillGroup { get; set; }
            public DateTime WorkDate { get; set; }
            public int HeadcountNeed { get; set; } = 1;
        }

        public class AutoAssignResult
        {
            public List<AutoAssignedEmployee> Assigned { get; set; } = new();
            public int AssignedCount { get; set; }
            public int RequestedCount { get; set; }
            public int ShortfallCount { get; set; }
            public string Message { get; set; } = "";
        }

        public class AutoAssignedEmployee
        {
            public int AssignmentID { get; set; }
            public int EmpID { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public int TotalSkill { get; set; }
            public double TotalTime { get; set; }
        }

        private class AutoCandidate
        {
            public int EmpID { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public int TotalSkill { get; set; }
            public double TotalTime { get; set; }
            public string? FromBiz { get; set; } // ✅ เพิ่ม
            public string? FromProcess { get; set; } // ✅ เพิ่ม
        }
    }
}
