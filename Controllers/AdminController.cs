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
using System.Net.Mail;
using BCrypt.Net;

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
        [HttpGet("check-registration")]
public async Task<IActionResult> CheckRegistration([FromQuery] string empID)
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


// POST: api/admin/register
[HttpPost("register")]
public async Task<IActionResult> RegisterNewEmployee([FromBody] Admin admin)
{
    if (admin == null) return BadRequest("Invalid data.");

    // ตรวจสอบว่า EmpID ใน Admin มีค่าถูกต้องหรือไม่
    var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.EmpID == admin.EmpID);
    if (existingAdmin != null) return Conflict("Admin already exists.");

    // ตรวจสอบ PasswordHash
    if (string.IsNullOrEmpty(admin.PasswordHash))
    {
        return BadRequest("Password is required.");
    }

    // แปลงรหัสผ่านเป็น Hash ก่อนที่จะเก็บในฐานข้อมูล
    string hashedPassword = BCrypt.Net.BCrypt.HashPassword(admin.PasswordHash);

    // เพิ่มข้อมูล Admin ใหม่ในฐานข้อมูล
    var newAdmin = new Admin
    {
        EmpID = admin.EmpID,
        FirstName = admin.FirstName,
        LastName = admin.LastName,
        Email = admin.Email,
        PasswordHash = hashedPassword,  // เก็บรหัสผ่านที่แปลงเป็น Hash แล้ว
        // Role = admin.Role,
        Role = "Leader"
    };

    try
    {
        _context.Admin.Add(newAdmin);
        await _context.SaveChangesAsync();
        return Ok(new { Message = "Admin registered successfully." });
    }
    catch (Exception ex)
    {
        // ถ้ามีข้อผิดพลาดให้แสดงข้อความข้อผิดพลาด
        Console.WriteLine("Error during registration: " + ex.Message);
        return StatusCode(500, "Internal server error");
    }
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
