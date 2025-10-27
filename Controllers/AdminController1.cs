// using Microsoft.AspNetCore.Mvc;
// using Microsoft.EntityFrameworkCore;
// using Api.Models;
// using Microsoft.Extensions.Configuration;
// using System.Threading.Tasks;
// using System.Linq;
// using System.Text; // สำหรับการใช้ Encoding
// using System.IdentityModel.Tokens.Jwt; // สำหรับ JWT Token เช่น JwtSecurityToken, JwtSecurityTokenHandler
// using Microsoft.IdentityModel.Tokens; // สำหรับการใช้ SymmetricSecurityKey, SigningCredentials, SecurityAlgorithms
// using System.Security.Claims; // สำหรับการใช้ Claim, JwtRegisteredClaimNames
// using System;

// namespace API_ProductionQuality.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     public class AdminController : ControllerBase
//     {
//         private readonly ApplicationDbContext _context;
//         private readonly IConfiguration _configuration;

//         public AdminController(ApplicationDbContext context, IConfiguration configuration)
//         {
//             _context = context;
//             _configuration = configuration;
//         }

//         // POST: api/admin/register
//         [HttpPost("register")]
//         public async Task<IActionResult> RegisterAdmin([FromBody] Admin admin)
//         {
//             if (admin == null)
//             {
//                 return BadRequest("Invalid data.");
//             }

//             var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == admin.Username);
//             if (existingAdmin != null)
//             {
//                 return Conflict("Admin already exists.");
//             }

//             admin.PasswordHash = BCrypt.Net.BCrypt.HashPassword(admin.Password);

//             _context.Admins.Add(admin);
//             await _context.SaveChangesAsync();

//             return Ok(new { Message = "Admin registered successfully!" });
//         }

//         // POST: api/admin/login
//         [HttpPost("login")]
//         public async Task<IActionResult> LoginAdmin([FromBody] Admin admin)
//         {
//             if (admin == null)
//             {
//                 return BadRequest("Invalid data.");
//             }

//             var existingAdmin = await _context.Admins.FirstOrDefaultAsync(a => a.Username == admin.Username);
//             if (existingAdmin == null || !BCrypt.Net.BCrypt.Verify(admin.Password, existingAdmin.PasswordHash))
//             {
//                 return Unauthorized("Invalid credentials.");
//             }

//             var token = GenerateJwtToken(existingAdmin);

//             return Ok(new { Token = token });
//         }

//         private string GenerateJwtToken(Admin admin)
//         {
//             var claims = new List<Claim>
//             {
//                 new Claim(JwtRegisteredClaimNames.Sub, admin.Username),
//                 new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
//             };

//             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
//             var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
//             var expires = DateTime.UtcNow.AddHours(1);

//             var token = new JwtSecurityToken(
//                 issuer: _configuration["Jwt:Issuer"],
//                 audience: _configuration["Jwt:Audience"],
//                 claims: claims,
//                 expires: expires,
//                 signingCredentials: creds
//             );

//             return new JwtSecurityTokenHandler().WriteToken(token);
//         }
//     }
// }
