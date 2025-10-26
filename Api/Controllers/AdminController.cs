using System;
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

        // POST: api/admin/register
        [HttpPost("register")]
public async Task<IActionResult> RegisterAdmin([FromBody] AdminRegisterDto adminDto)
{
    if (adminDto == null) return BadRequest("Invalid data.");

    var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.Username == adminDto.Username);
    if (existingAdmin != null) return Conflict("Admin already exists.");

    string password = adminDto.Password?.Trim(); // Trim ช่องว่าง
    var passwordHash = BCrypt.Net.BCrypt.HashPassword(password);

    var admin = new Admin
    {
        Username = adminDto.Username,
        PasswordHash = passwordHash
    };

    _context.Admin.Add(admin);
    await _context.SaveChangesAsync();

    // สร้าง JWT token หลัง register
    var token = GenerateJwtToken(admin);

    return Ok(new { Token = token, Message = "Admin registered successfully!" });
}

[HttpPost("login")]
public async Task<IActionResult> LoginAdmin([FromBody] AdminLoginDto adminDto)
{
    if (adminDto == null) return BadRequest("Invalid data.");

    var existingAdmin = await _context.Admin.FirstOrDefaultAsync(a => a.Username == adminDto.Username);
    if (existingAdmin == null) return Unauthorized("Invalid credentials.");

    bool isValidPassword = BCrypt.Net.BCrypt.Verify(adminDto.Password?.Trim(), existingAdmin.PasswordHash);
    if (!isValidPassword) return Unauthorized("Invalid credentials.");

    var token = GenerateJwtToken(existingAdmin);
    return Ok(new { Token = token });
}

private string GenerateJwtToken(Admin admin)
{
    var key = _configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(key))
        throw new Exception("JWT Key is missing"); // ตอนนี้ค่า key มีแล้วก็ไม่ error

    var creds = new SigningCredentials(
        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        SecurityAlgorithms.HmacSha256
    );

    var token = new JwtSecurityToken(
        issuer: _configuration["Jwt:Issuer"],
        audience: _configuration["Jwt:Audience"],
        claims: new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, admin.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        },
        expires: DateTime.UtcNow.AddHours(1),
        signingCredentials: creds
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
    }
}
