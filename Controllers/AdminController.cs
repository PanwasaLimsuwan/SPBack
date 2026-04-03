using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using BCrypt.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit;

namespace API_ProductionQuality.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AdminController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        // ตรวจสอบว่า EmpID ใน EmployeeInfo เป็น Supervisor และยังไม่ได้ลงทะเบียนใน Admin หรือไม่
        //         [HttpGet("check-registration-leader")]
        // public async Task<IActionResult> CheckRegistrationForLeader([FromQuery] string empID)
        // {
        //     if (string.IsNullOrEmpty(empID))
        //     {
        //         return BadRequest("EmpID cannot be null or empty.");
        //     }

        //     int empIDInt;
        //     if (!int.TryParse(empID, out empIDInt)) // ตรวจสอบการแปลง EmpID เป็น int
        //     {
        //         return BadRequest("Invalid EmpID format.");
        //     }

        //     try
        //     {
        //         var employee = await _context.EmployeeInfo.FirstOrDefaultAsync(e =>
        //             e.EmpID == empIDInt && e.Position == "Supervisor"
        //         );

        //         if (employee != null)
        //         {
        //             return Ok(new { isRegistered = true });
        //         }

        //         Console.WriteLine($"EmpID {empIDInt} not found or not a Supervisor");

        //         return Ok(new { isRegistered = false });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error occurred while checking registration: {ex.Message}");
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        // แก้ไขเส้นทางใน WidgetController
        [Route("api/admin")] // เพิ่มเส้นทางนี้เพื่อให้สอดคล้องกับ /api/admin
        [HttpGet("get-user-id")]
        public IActionResult GetUserIdByEmail(string email)
        {
            var admin = _context.Admin.FirstOrDefault(a => a.Email == email);
            if (admin == null)
            {
                return NotFound("User not found");
            }

            return Ok(new { user_id = admin.user_id });
        }

        // ตรวจสอบว่า EmpID ใน EmployeeInfo เป็น Supervisor และยังไม่ได้ลงทะเบียนใน Admin หรือไม่
        // [HttpGet("check-registration-leader")]
        // public async Task<IActionResult> CheckRegistrationForLeader([FromQuery] string empID)
        // {
        //     if (string.IsNullOrEmpty(empID))
        //     {
        //         return BadRequest("EmpID cannot be null or empty.");
        //     }

        //     int empIDInt;
        //     if (!int.TryParse(empID, out empIDInt)) // ตรวจสอบการแปลง EmpID เป็น int
        //     {
        //         return BadRequest("Invalid EmpID format.");
        //     }

        //     try
        //     {
        //         // ตรวจสอบว่า EmpID เป็นตัวใหญ่ในฐานข้อมูลและตำแหน่งเป็น "Leader"
        //         var employee = await _context.Admin.FirstOrDefaultAsync(e =>
        //             e.EmpID == empIDInt && e.Role == "Leader"
        //         ); // ใช้ EmpID แทน empID

        //         if (employee != null)
        //         {
        //             return Ok(new { isRegistered = true });
        //         }

        //         Console.WriteLine($"EmpID {empIDInt} not found or not a Leader");

        //         return Ok(new { isRegistered = false });
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine($"Error occurred while checking registration: {ex.Message}");
        //         return StatusCode(500, "Internal server error");
        //     }
        // }
        [HttpGet("check-registration-leader")]
        public async Task<IActionResult> CheckRegistrationForLeader([FromQuery] string empID)
        {
            if (string.IsNullOrEmpty(empID))
            {
                return BadRequest("EmpID cannot be null or empty.");
            }

            if (!int.TryParse(empID, out int empIDInt))
            {
                return BadRequest("Invalid EmpID format.");
            }

            try
            {
                // ✅ เช็คทั้ง LeaderMFG และ LeaderHR
                var employee = await _context.Admin.FirstOrDefaultAsync(e =>
                    e.EmpID == empIDInt && (e.Role == "LeaderMFG" || e.Role == "LeaderHR")
                );

                if (employee != null)
                {
                    return Ok(new { isRegistered = true });
                }

                Console.WriteLine($"EmpID {empIDInt} not found or not a Leader");

                return Ok(new { isRegistered = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while checking registration: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("check-registration-admin")]
        public async Task<IActionResult> CheckRegistrationForAdmin([FromQuery] string empID)
        {
            if (string.IsNullOrEmpty(empID))
            {
                return BadRequest("EmpID cannot be null or empty.");
            }

            int empIDInt;
            if (!int.TryParse(empID, out empIDInt)) // ตรวจสอบการแปลง EmpID เป็น int
            {
                return BadRequest("Invalid EmpID format.");
            }

            try
            {
                // ตรวจสอบว่า EmpID เป็นตัวใหญ่ในฐานข้อมูลและตำแหน่งเป็น "Admin"
                var employee = await _context.Admin.FirstOrDefaultAsync(e =>
                    e.EmpID == empIDInt && e.Role == "Admin"
                ); // ใช้ EmpID แทน empID

                if (employee != null)
                {
                    return Ok(new { isRegistered = true });
                }

                Console.WriteLine($"EmpID {empIDInt} not found or not a Admin");

                return Ok(new { isRegistered = false });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while checking registration: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // // POST: api/admin/register
        // [HttpPost("register")]
        // public async Task<IActionResult> RegisterNewEmployee([FromBody] Admin admin)
        // {
        //     if (admin == null) return BadRequest("Invalid data.");

        //     // ตรวจสอบว่า EmpID ใน Admin มีค่าถูกต้องหรือไม่
        //     var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
        //     if (existingAdmin != null) return Conflict("Admin already exists.");

        //     // ตรวจสอบ PasswordHash
        //     if (string.IsNullOrEmpty(admin.PasswordHash))
        //     {
        //         return BadRequest("Password is required.");
        //     }

        //     // แปลงรหัสผ่านเป็น Hash ก่อนที่จะเก็บในฐานข้อมูล
        //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

        //     // เพิ่มข้อมูล Admin ใหม่ในฐานข้อมูล
        //     var newAdmin = new Admin
        //     {
        //         EmpID = admin.EmpID,
        //         FirstName = admin.FirstName,
        //         LastName = admin.LastName,
        //         Email = admin.Email,
        //         PasswordHash = hashedPassword,  // เก็บรหัสผ่านที่แปลงเป็น Hash แล้ว
        //         // Role = admin.Role,
        //         Role = "Leader"
        //     };

        //     try
        //     {
        //         _context.Admin.Add(newAdmin);
        //         await _context.SaveChangesAsync();
        //         return Ok(new { Message = "Admin registered successfully." });
        //     }
        //     catch (Exception ex)
        //     {
        //         // ถ้ามีข้อผิดพลาดให้แสดงข้อความข้อผิดพลาด
        //         Console.WriteLine("Error during registration: " + ex.Message);
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        // [HttpPost("register-leader")]
        // public async Task<IActionResult> RegisterNewEmployeeForLeader([FromBody] Admin admin)
        // {
        //     if (admin == null)
        //         return BadRequest("Invalid data.");

        //     // ตรวจสอบข้อมูลที่สำคัญ (เช่น Email, Password)
        //     if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
        //     {
        //         return BadRequest("Email or Password is missing.");
        //     }

        //     // เช็คว่า admin มีอยู่ในระบบหรือไม่
        //     var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a =>
        //         a.EmpID == admin.EmpID
        //     );
        //     if (existingAdmin != null)
        //         return Conflict("Admin already exists.");

        //     // แปลงรหัสผ่านเป็น Hash
        //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

        //     // สร้าง Admin ใหม่
        //     var newAdmin = new Admin
        //     {
        //         EmpID = admin.EmpID,
        //         FirstName = admin.FirstName,
        //         LastName = admin.LastName,
        //         Email = admin.Email,
        //         PasswordHash = hashedPassword,
        //         Role = "Leader", // Set default role
        //     };

        //     try
        //     {
        //         // เพิ่ม Admin ลงในฐานข้อมูล
        //         _context.Admin.Add(newAdmin);
        //         await _context.SaveChangesAsync();

        //         // ส่งอีเมลแจ้งเตือนหลังจากการลงทะเบียน
        //         SendVerificationEmail(newAdmin, admin.PasswordHash);

        //         return Ok(new { Message = "Admin registered successfully." });
        //     }
        //     catch (DbUpdateException dbEx)
        //     {
        //         Console.WriteLine("Database update error: " + dbEx.Message);
        //         return StatusCode(500, "Database update error");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("Error during registration: " + ex.Message);
        //         return StatusCode(500, "Internal server error");
        //     }
        // }
        [HttpPost("register-leader")]
        public async Task<IActionResult> RegisterNewEmployeeForLeader([FromBody] Admin admin)
        {
            if (admin == null)
                return BadRequest("Invalid data.");

            if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
            {
                return BadRequest("Email or Password is missing.");
            }

            var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a =>
                a.EmpID == admin.EmpID
            );
            if (existingAdmin != null)
                return Conflict("Admin already exists.");

            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

            // ✅ ใช้ Role ที่ส่งมาจาก Frontend (LeaderMFG หรือ LeaderHR)
            var newAdmin = new Admin
            {
                EmpID = admin.EmpID,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Email = admin.Email,
                PasswordHash = hashedPassword,
                Role = admin.Role, // ✅ ใช้ Role ที่ส่งมา ไม่ใช่ค่าคงที่ "Leader"
            };

            try
            {
                _context.Admin.Add(newAdmin);
                await _context.SaveChangesAsync();

                SendVerificationEmail(newAdmin, admin.PasswordHash);

                return Ok(new { Message = $"Admin registered successfully as {admin.Role}." });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine("Database update error: " + dbEx.Message);
                return StatusCode(500, "Database update error");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during registration: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterNewEmployeeForAdmin([FromBody] Admin admin)
        {
            if (admin == null)
                return BadRequest("Invalid data.");

            // ตรวจสอบข้อมูลที่สำคัญ (เช่น Email, Password)
            if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
            {
                return BadRequest("Email or Password is missing.");
            }

            // เช็คว่า admin มีอยู่ในระบบหรือไม่
            var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a =>
                a.EmpID == admin.EmpID
            );
            if (existingAdmin != null)
                return Conflict("Admin already exists.");

            // แปลงรหัสผ่านเป็น Hash
            string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

            // สร้าง Admin ใหม่
            var newAdmin = new Admin
            {
                EmpID = admin.EmpID,
                FirstName = admin.FirstName,
                LastName = admin.LastName,
                Email = admin.Email,
                PasswordHash = hashedPassword,
                Role = "Admin", // Set default role
            };

            try
            {
                // เพิ่ม Admin ลงในฐานข้อมูล
                _context.Admin.Add(newAdmin);
                await _context.SaveChangesAsync();

                // ส่งอีเมลแจ้งเตือนหลังจากการลงทะเบียน
                SendVerificationEmail(newAdmin, admin.PasswordHash);

                return Ok(new { Message = "Admin registered successfully." });
            }
            catch (DbUpdateException dbEx)
            {
                Console.WriteLine("Database update error: " + dbEx.Message);
                return StatusCode(500, "Database update error");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during registration: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // [HttpPost("register-admin")]
        // public async Task<IActionResult> RegisterAdmin([FromBody] Admin admin)
        // {
        //     if (admin == null)
        //         return BadRequest("Invalid data.");

        //     // ตรวจสอบข้อมูลที่สำคัญ (เช่น Email, Password)
        //     if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
        //     {
        //         return BadRequest("Email or Password is missing.");
        //     }

        //     // เช็คว่า admin มีอยู่ในระบบหรือไม่
        //     var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
        //     if (existingAdmin != null)
        //         return Conflict("Admin already exists.");

        //     // แปลงรหัสผ่านเป็น Hash
        //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

        //     // สร้าง Admin ใหม่
        //     var newAdmin = new Admin
        //     {
        //         EmpID = admin.EmpID,
        //         FirstName = admin.FirstName,
        //         LastName = admin.LastName,
        //         Email = admin.Email,
        //         PasswordHash = hashedPassword,
        //         Role = "Admin"  // Role สำหรับ Admin
        //     };

        //     try
        //     {
        //         // เพิ่ม Admin ลงในฐานข้อมูล
        //         _context.Admin.Add(newAdmin);
        //         await _context.SaveChangesAsync();

        //         // ส่งอีเมลแจ้งเตือนหลังจากการลงทะเบียน
        //         SendVerificationEmail(newAdmin, admin.PasswordHash);

        //         return Ok(new { Message = "Admin registered successfully." });
        //     }
        //     catch (DbUpdateException dbEx)
        //     {
        //         Console.WriteLine("Database update error: " + dbEx.Message);
        //         return StatusCode(500, "Database update error");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("Error during registration: " + ex.Message);
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        // [HttpPost("register-leader")]
        // public async Task<IActionResult> RegisterLeader([FromBody] Admin admin)
        // {
        //     if (admin == null)
        //         return BadRequest("Invalid data.");

        //     // ตรวจสอบข้อมูลที่สำคัญ (เช่น Email, Password)
        //     if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
        //     {
        //         return BadRequest("Email or Password is missing.");
        //     }

        //     // เช็คว่า EmpID เป็น Supervisor หรือไม่
        //     var employee = await _context.EmployeeInfo
        //         .FirstOrDefaultAsync(e => e.EmpID == admin.EmpID && e.Position == "Supervisor");

        //     if (employee == null)
        //     {
        //         return BadRequest("EmpID must be a Supervisor to register as a Leader.");
        //     }

        //     // เช็คว่า admin มีอยู่ในระบบหรือไม่
        //     var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
        //     if (existingAdmin != null)
        //         return Conflict("Leader already exists.");

        //     // แปลงรหัสผ่านเป็น Hash
        //     string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

        //     // สร้าง Leader ใหม่
        //     var newLeader = new Admin
        //     {
        //         EmpID = admin.EmpID,
        //         FirstName = admin.FirstName,
        //         LastName = admin.LastName,
        //         Email = admin.Email,
        //         PasswordHash = hashedPassword,
        //         Role = "Leader"  // Role สำหรับ Leader
        //     };

        //     try
        //     {
        //         // เพิ่ม Leader ลงในฐานข้อมูล
        //         _context.Admin.Add(newLeader);
        //         await _context.SaveChangesAsync();

        //         // ส่งอีเมลแจ้งเตือนหลังจากการลงทะเบียน
        //         SendVerificationEmail(newLeader, admin.PasswordHash);

        //         return Ok(new { Message = "Leader registered successfully." });
        //     }
        //     catch (DbUpdateException dbEx)
        //     {
        //         Console.WriteLine("Database update error: " + dbEx.Message);
        //         return StatusCode(500, "Database update error");
        //     }
        //     catch (Exception ex)
        //     {
        //         Console.WriteLine("Error during registration: " + ex.Message);
        //         return StatusCode(500, "Internal server error");
        //     }
        // }

        // เปลี่ยนจาก GenerateJwtToken(admin) เป็น GenerateJwtToken(admin, employee)
        // และดึง EmployeeInfo มาด้วย

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
        {
            if (
                loginDto == null
                || string.IsNullOrEmpty(loginDto.Email)
                || string.IsNullOrEmpty(loginDto.Password)
            )
            {
                return BadRequest("Email and Password are required.");
            }

            try
            {
                var admin = await _context.Admin.FirstOrDefaultAsync(a =>
                    a.Email == loginDto.Email
                );
                if (admin == null)
                    return Unauthorized("Invalid email or password.");

                if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
                    return Unauthorized("Invalid email or password.");

                // ✅ ดึงข้อมูล Biz / Process / Position จาก EmployeeInfo
                var employee = await _context.EmployeeInfo.FirstOrDefaultAsync(e =>
                    e.EmpID == admin.EmpID
                );

                // ✅ ส่ง employee เข้า GenerateJwtToken ด้วย
                var token = GenerateJwtToken(admin, employee);

                return Ok(
                    new
                    {
                        token,
                        role = admin.Role,
                        biz = employee?.Biz ?? "",
                        process = employee?.Process ?? "",
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error during login: " + ex.Message);
                return StatusCode(500, "Internal server error");
            }
        }

        // ฟังก์ชันการแก้ไขพนักงาน
        [HttpPut("edit/{empID}")]
        public async Task<IActionResult> EditEmployee(int empID, [FromBody] Admin admin)
        {
            if (admin == null || empID != admin.EmpID)
            {
                return BadRequest("Invalid data.");
            }

            try
            {
                var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == empID);
                if (existingAdmin == null)
                {
                    return NotFound("Admin not found.");
                }

                // อัปเดตเฉพาะฟิลด์ที่มีการเปลี่ยนแปลง
                if (!string.IsNullOrEmpty(admin.FirstName))
                {
                    existingAdmin.FirstName = admin.FirstName;
                }
                if (!string.IsNullOrEmpty(admin.LastName))
                {
                    existingAdmin.LastName = admin.LastName;
                }
                if (!string.IsNullOrEmpty(admin.Email))
                {
                    existingAdmin.Email = admin.Email;
                }

                // ไม่ต้องบังคับการอัปเดต Role และ PasswordHash
                // คงค่าเดิมในฐานข้อมูล

                // ถ้าไม่มีการอัปเดตในฟิลด์ใด ๆ ให้ส่งกลับเป็นข้อความ "No changes detected"
                if (
                    string.IsNullOrEmpty(existingAdmin.FirstName)
                    && string.IsNullOrEmpty(existingAdmin.LastName)
                    && string.IsNullOrEmpty(existingAdmin.Email)
                )
                {
                    return BadRequest("One or more fields are missing or invalid.");
                }

                await _context.SaveChangesAsync();
                return Ok(new { Message = "Employee updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error during edit: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpDelete("delete/{empID}")]
        public async Task<IActionResult> DeleteEmployee(int empID)
        {
            // เพิ่มบรรทัดนี้เพื่อดูว่า empID ที่ส่งมาถูกต้องหรือไม่
            Console.WriteLine($"Attempting to delete Admin with EmpID: {empID}");

            var admin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == empID);
            if (admin == null)
            {
                // ถ้าไม่พบ Admin ให้ log ออก
                Console.WriteLine("Admin not found in the database.");
                return NotFound("Admin not found.");
            }

            _context.Admin.Remove(admin);
            await _context.SaveChangesAsync();
            return Ok(new { Message = "Employee deleted successfully." });
        }

        [HttpGet("get-user-process")]
        public async Task<IActionResult> GetUserProcess([FromQuery] string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest("Email is required");
            }

            try
            {
                var admin = await _context.Admin.FirstOrDefaultAsync(a => a.Email == email);

                if (admin == null)
                {
                    return NotFound("User not found");
                }

                // ดึงข้อมูล Process จาก EmployeeInfo
                var employee = await _context.EmployeeInfo.FirstOrDefaultAsync(e =>
                    e.EmpID == admin.EmpID
                );

                return Ok(
                    new
                    {
                        user_id = admin.user_id,
                        empID = admin.EmpID,
                        firstName = admin.FirstName,
                        lastName = admin.LastName,
                        role = admin.Role,
                        email = admin.Email,
                        process = employee?.Process, // ส่ง process กลับไปด้วย
                    }
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting user process: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // ฟังก์ชันส่งอีเมลให้กับ Leader
        private void SendVerificationEmail(Admin admin, string password)
        {
            try
            {
                // สร้าง MimeMessage สำหรับส่งอีเมล
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("Your Company", "your-email@example.com"));
                // message.From.Add(new MailboxAddress("Your Company", "panwasalimsuwan@gmail.com"));
                // เปลี่ยนผู้รับเป็น panwasalimsuwan@gmail.com
                message.To.Add(new MailboxAddress("Panwa Salimsuwan", "panwasalimsuwan@gmail.com"));
                // message.To.Add(new MailboxAddress(admin.FirstName + " " + admin.LastName, admin.Email));
                message.Subject = "Your Registration Details";

                // สร้างเนื้อหาอีเมล
                var bodyBuilder = new BodyBuilder
                {
                    TextBody =
                        $"Hello {admin.FirstName} {admin.LastName},\n\n"
                        + $"Your account has been created successfully. Your default password is: {password}\n"
                        + "Please log in and change your password as soon as possible.\n\n"
                        + "Best regards,\nYour Company",
                };

                message.Body = bodyBuilder.ToMessageBody();

                // เชื่อมต่อและส่งอีเมลผ่าน SMTP
                using (var client = new SmtpClient())
                {
                    client.Connect(
                        "smtp.gmail.com",
                        587,
                        MailKit.Security.SecureSocketOptions.StartTls
                    );

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

        // รับ employee เพิ่ม แล้วใส่ biz / process / position ใน claims

        private string GenerateJwtToken(Admin admin, EmployeeInfo? employee = null)
        {
            var key = _configuration["Jwt:Key"];
            var creds = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256
            );

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, admin.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("role", admin.Role ?? ""),
                // ✅ เพิ่ม 3 claims นี้
                new Claim("empID", admin.EmpID.ToString()),
                new Claim("biz", employee?.Biz ?? ""),
                new Claim("process", employee?.Process ?? ""),
                new Claim("position", employee?.Position ?? ""),
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // ✅ เพิ่มเป็น 8h (เหมาะกับ shift งาน)
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ฟังก์ชันยืนยันอีเมล
        // [HttpGet("verify-email")]
        // public async Task<IActionResult> VerifyEmail([FromQuery] string token)
        // {
        //     try
        //     {
        //         var tokenHandler = new JwtSecurityTokenHandler();
        //         var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);

        //         var principal = ValidateToken(token, key);

        //         if (principal == null)
        //             return Unauthorized("Invalid or expired token");

        //         var userEmail = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);  // ใช้ "sub" แทน "ClaimTypes.Name"
        //         var admin = await _context.Admin.FirstOrDefaultAsync(a => a.Email == userEmail);

        //         if (admin == null)
        //             return NotFound("Admin not found");

        //         admin.IsVerified = true;
        //         await _context.SaveChangesAsync();

        //         return Ok("Email verified successfully");
        //     }
        //     catch
        //     {
        //         return BadRequest("Invalid or expired verification link");
        //     }
        // }

        // private ClaimsPrincipal ValidateToken(string token, byte[] key)
        // {
        //     var tokenHandler = new JwtSecurityTokenHandler();
        //     try
        //     {
        //         var validationParameters = new TokenValidationParameters
        //         {
        //             ValidateIssuer = true,
        //             ValidateAudience = true,
        //             ValidateLifetime = true,
        //             ValidIssuer = _configuration["Jwt:Issuer"],
        //             ValidAudience = _configuration["Jwt:Audience"],
        //             IssuerSigningKey = new SymmetricSecurityKey(key)
        //         };

        //         var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

        //         if (validatedToken is JwtSecurityToken jwtToken && jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.OrdinalIgnoreCase))
        //         {
        //             return principal;
        //         }
        //     }
        //     catch
        //     {
        //         return null;
        //     }

        //     return null;
        // }

        // private string GenerateJwtToken(Admin admin)
        // {
        //     var key = _configuration["Jwt:Key"];
        //     var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

        //     var token = new JwtSecurityToken(
        //         issuer: _configuration["Jwt:Issuer"],
        //         audience: _configuration["Jwt:Audience"],
        //         claims: new List<Claim>
        //         {
        //             new Claim(JwtRegisteredClaimNames.Sub, admin.Username),
        //             new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        //         },
        //         expires: DateTime.UtcNow.AddHours(1),
        //         signingCredentials: creds
        //     );

        //     return new JwtSecurityTokenHandler().WriteToken(token);
        // }

        // // ฟังก์ชันส่งอีเมลยืนยัน
        // private void SendVerificationEmail(Admin admin)
        // {
        //     var verificationToken = GenerateJwtToken(admin);
        //     var verifyUrl = $"http://localhost:5000/api/admin/verify-email?token={verificationToken}";

        //     var mailMessage = new MailMessage("your-email@example.com", admin.Email)
        //     {
        //         Subject = "Please verify your email address",
        //         Body = $"Please click the following link to verify your email: {verifyUrl}",
        //         IsBodyHtml = true
        //     };

        //     var smtpClient = new SmtpClient("smtp.gmail.com")
        //     {
        //         Port = 587,
        //         Credentials = new System.Net.NetworkCredential("your-email@example.com", "your-email-password"),
        //         EnableSsl = true,
        //     };

        //     smtpClient.Send(mailMessage);
        // }
    }
}
