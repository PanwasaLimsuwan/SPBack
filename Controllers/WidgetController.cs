using System.Linq;
using Api.Models; // ใช้ Model ที่สร้างขึ้น
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // สำหรับ DbUpdateException

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WidgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public WidgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/widget/get-all-widget-settings
        [HttpGet("get-all-widget-settings")]
        public IActionResult GetAllWidgetSettings()
        {
            try
            {
                // ดึงข้อมูลทั้งหมดจากตาราง Widget
                var allSettings = _context.Widget.ToList();

                // ถ้าไม่พบข้อมูลใด ๆ ในตาราง ให้คืนค่า NotFound
                if (allSettings == null || !allSettings.Any())
                {
                    return NotFound("No widget settings found");
                }

                // คืนค่าการตั้งค่าทั้งหมดในรูปแบบ JSON
                return Ok(allSettings);
            }
            catch (Exception ex)
            {
                // ถ้ามีข้อผิดพลาดเกิดขึ้น ให้คืนค่า 500
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        [HttpPost("save-widget-settings")]
        public IActionResult SaveWidget([FromBody] WidgetDto settingsDto)
        {
            if (settingsDto == null)
            {
                return BadRequest("Invalid settings data");
            }

            try
            {
                // ตรวจสอบว่า user_id มีอยู่ในตาราง Admin หรือไม่
                var admin = _context.Admin.FirstOrDefault(a => a.user_id == settingsDto.user_id);
                if (admin == null)
                {
                    return BadRequest("User ID not found in Admin table");
                }

                // ตรวจสอบว่า UserId ในฐานข้อมูลมีข้อมูล Widget หรือไม่
                var existingSettings = _context.Widget.FirstOrDefault(ws =>
                    ws.user_id == settingsDto.user_id
                );

                if (existingSettings != null)
                {
                    // ถ้ามีข้อมูลการตั้งค่าอยู่แล้ว ให้ทำการอัปเดต
                    existingSettings.settings = settingsDto.Settings;
                    _context.Widget.Update(existingSettings);
                }
                else
                {
                    // ถ้ายังไม่มีข้อมูล ให้สร้างข้อมูลใหม่
                    var newSettings = new Widget
                    {
                        user_id = settingsDto.user_id,
                        settings = settingsDto.Settings,
                    };
                    _context.Widget.Add(newSettings);
                }

                // บันทึกการเปลี่ยนแปลง
                _context.SaveChanges();
                return Ok("Widget settings saved successfully");
            }
            catch (DbUpdateException ex)
            {
                return StatusCode(
                    500,
                    $"Database update failed: {ex.InnerException?.Message ?? ex.Message}"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"An unexpected error occurred: {ex.Message}");
            }
        }

        // GET: api/widget/get-widget-settings/{userId}
        [HttpGet("get-widget-settings/{user_id}")]
        public IActionResult GetWidget(int user_id)
        {
            var settings = _context.Widget.FirstOrDefault(ws => ws.user_id == user_id);

            if (settings == null)
            {
                return NotFound("Widget settings not found");
            }

            // ตรวจสอบว่า settings เป็น string ที่เป็น JSON หรือไม่
            return Ok(settings.settings); // ส่งกลับเป็น string ที่เป็น JSON
        }

        [HttpGet("get-user-id")]
        public IActionResult GetUserIdByEmail(string email)
        {
            // ค้นหาผู้ใช้จากอีเมล
            var admin = _context.Admin.FirstOrDefault(a => a.Email == email);
            if (admin == null)
            {
                return NotFound("User not found");
            }

            // ส่งค่า user_id ที่ตรงกับ admin โดยใช้ email
            return Ok(new { user_id = admin.user_id }); // ถ้าใช้ user_id ในฐานข้อมูล
        }
    }
}
