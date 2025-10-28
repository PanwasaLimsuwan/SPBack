using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MailKit.Net.Smtp;
using BCrypt.Net;
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
        [HttpGet("check-registration-leader")]
public async Task<IActionResult> CheckRegistrationForLeader([FromQuery] string empID)
{
    if (string.IsNullOrEmpty(empID))
    {
        return BadRequest("EmpID cannot be null or empty.");
    }

    int empIDInt;
    if (!int.TryParse(empID, out empIDInt))  // ตรวจสอบการแปลง EmpID เป็น int
    {
        return BadRequest("Invalid EmpID format.");
    }

    try
    {
        // ตรวจสอบว่า EmpID เป็นตัวใหญ่ในฐานข้อมูลและตำแหน่งเป็น "Supervisor"
        var employee = await _context.EmployeeInfo
            .FirstOrDefaultAsync(e => e.EmpID == empIDInt && e.Position == "Supervisor");  // ใช้ EmpID แทน empID

        if (employee != null)
        {
            return Ok(new { isRegistered = true });
        }

        Console.WriteLine($"EmpID {empIDInt} not found or not a Supervisor");

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
    if (!int.TryParse(empID, out empIDInt))  // ตรวจสอบการแปลง EmpID เป็น int
    {
        return BadRequest("Invalid EmpID format.");
    }

    try
    {
        // ตรวจสอบว่า EmpID เป็นตัวใหญ่ในฐานข้อมูลและตำแหน่งเป็น "Admin"
        var employee = await _context.EmployeeInfo
            .FirstOrDefaultAsync(e => e.EmpID == empIDInt && e.Position == "Admin");  // ใช้ EmpID แทน empID

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

[HttpPost("register-leader")]
public async Task<IActionResult> RegisterNewEmployeeForLeader([FromBody] Admin admin)
{
    if (admin == null) 
        return BadRequest("Invalid data.");

    // ตรวจสอบข้อมูลที่สำคัญ (เช่น Email, Password)
    if (string.IsNullOrEmpty(admin.Email) || string.IsNullOrEmpty(admin.PasswordHash))
    {
        return BadRequest("Email or Password is missing.");
    }

    // เช็คว่า admin มีอยู่ในระบบหรือไม่
    var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
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
        Role = "Leader", // Set default role
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
    var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
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


[HttpPost("login")]
public async Task<IActionResult> Login([FromBody] AdminLoginDto loginDto)
{
    if (loginDto == null || string.IsNullOrEmpty(loginDto.Email) || string.IsNullOrEmpty(loginDto.Password))
    {
        return BadRequest("Email and Password are required.");
    }

    try
    {
        // ค้นหา admin ตาม Email
        var admin = await _context.Admin
            .FirstOrDefaultAsync(a => a.Email == loginDto.Email);

        if (admin == null)
        {
            return Unauthorized("Invalid email or password.");
        }

        // ตรวจสอบว่า Password ที่กรอกมาถูกต้องหรือไม่
        if (!BCrypt.Net.BCrypt.Verify(loginDto.Password, admin.PasswordHash))
        {
            return Unauthorized("Invalid email or password.");
        }

        // สร้าง JWT Token
        var token = GenerateJwtToken(admin);

        // ส่ง token กลับไปให้ผู้ใช้
        return Ok(new { token });
    }
    catch (Exception ex)
    {
        Console.WriteLine("Error during login: " + ex.Message);
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
            TextBody = $"Hello {admin.FirstName} {admin.LastName},\n\n" +
                       $"Your account has been created successfully. Your default password is: {password}\n" +
                       "Please log in and change your password as soon as possible.\n\n" +
                       "Best regards,\nYour Company"
        };

        message.Body = bodyBuilder.ToMessageBody();

        // เชื่อมต่อและส่งอีเมลผ่าน SMTP
        using (var client = new SmtpClient())
        {
            client.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
            
            // ใช้ App Password
            client.Authenticate("panwasalimsuwan@gmail.com", "xemtsrhrnzpddwjl");  // ใช้ App Password แทนรหัสผ่านปกติ
            
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

private string GenerateJwtToken(Admin admin)
{
    var key = _configuration["Jwt:Key"];
    var creds = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)), SecurityAlgorithms.HmacSha256);

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Email),  // ใช้ Email เป็น Subject
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        },
        expires: DateTime.UtcNow.AddHours(1),  // ตั้งเวลาให้หมดอายุภายใน 1 ชั่วโมง
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);  // สร้างและแปลงเป็น JWT Token
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
