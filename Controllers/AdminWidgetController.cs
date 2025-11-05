using System;
using System.Collections.Generic;
using System.Linq;
using Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminWidgetController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminWidgetController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/AdminWidget
        [HttpGet]
        public IActionResult GetAllWidgets()
        {
            try
            {
                var widgets = _context.WidgetDefinition
                    .OrderBy(w => w.DisplayOrder)
                    .ToList();
                
                return Ok(widgets ?? new List<WidgetDefinition>());  // ✅ ป้องกัน null
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        // GET: api/AdminWidget/{id}
        [HttpGet("{id}")]
        public IActionResult GetWidgetById(string id)
        {
            try
            {
                var widget = _context.WidgetDefinition
                    .FirstOrDefault(w => w.WidgetId == id);
                
                if (widget == null)
                {
                    return NotFound($"Widget with ID '{id}' not found");
                }
                
                return Ok(widget);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // POST: api/AdminWidget
        [HttpPost]
        public IActionResult CreateWidget([FromBody] WidgetDefinitionDto widgetDto)
        {
            if (widgetDto == null)
            {
                return BadRequest("Widget data is required");
            }

            try
            {
                // ตรวจสอบว่า WidgetId ซ้ำหรือไม่
                var existing = _context.WidgetDefinition
                    .FirstOrDefault(w => w.WidgetId == widgetDto.WidgetId);
                
                if (existing != null)
                {
                    return BadRequest($"Widget with ID '{widgetDto.WidgetId}' already exists");
                }

                var newWidget = new WidgetDefinition
                {
                    WidgetId = widgetDto.WidgetId,
                    DisplayName = widgetDto.DisplayName,
                    ComponentName = widgetDto.ComponentName,
                    ComponentPath = widgetDto.ComponentPath,
                    IsActive = widgetDto.IsActive,
                    Span2 = widgetDto.Span2,
                    DisplayOrder = widgetDto.DisplayOrder ?? GetNextDisplayOrder(),
                    Description = widgetDto.Description,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.WidgetDefinition.Add(newWidget);
                _context.SaveChanges();

                return CreatedAtAction(nameof(GetWidgetById), 
                    new { id = newWidget.WidgetId }, newWidget);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = $"Error creating widget: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PUT: api/AdminWidget/{id}
        [HttpPut("{id}")]
        public IActionResult UpdateWidget(string id, [FromBody] WidgetDefinitionDto widgetDto)
        {
            if (widgetDto == null)
            {
                return BadRequest("Widget data is required");
            }

            try
            {
                var widget = _context.WidgetDefinition
                    .FirstOrDefault(w => w.WidgetId == id);
                
                if (widget == null)
                {
                    return NotFound($"Widget with ID '{id}' not found");
                }

                // อัปเดตข้อมูล
                widget.DisplayName = widgetDto.DisplayName ?? widget.DisplayName;
                widget.ComponentName = widgetDto.ComponentName ?? widget.ComponentName;
                widget.ComponentPath = widgetDto.ComponentPath ?? widget.ComponentPath;
                widget.IsActive = widgetDto.IsActive;
                widget.Span2 = widgetDto.Span2;
                widget.DisplayOrder = widgetDto.DisplayOrder ?? widget.DisplayOrder;
                widget.Description = widgetDto.Description;
                widget.UpdatedAt = DateTime.UtcNow;

                _context.WidgetDefinition.Update(widget);
                _context.SaveChanges();

                return Ok(widget);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = $"Error updating widget: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // DELETE: api/AdminWidget/{id}
        [HttpDelete("{id}")]
        public IActionResult DeleteWidget(string id)
        {
            try
            {
                var widget = _context.WidgetDefinition
                    .FirstOrDefault(w => w.WidgetId == id);
                
                if (widget == null)
                {
                    return NotFound($"Widget with ID '{id}' not found");
                }

                _context.WidgetDefinition.Remove(widget);
                _context.SaveChanges();

                return Ok(new { message = $"Widget '{id}' deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = $"Error deleting widget: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // PATCH: api/AdminWidget/{id}/toggle
        [HttpPatch("{id}/toggle")]
        public IActionResult ToggleWidgetStatus(string id)
        {
            try
            {
                var widget = _context.WidgetDefinition
                    .FirstOrDefault(w => w.WidgetId == id);
                
                if (widget == null)
                {
                    return NotFound($"Widget with ID '{id}' not found");
                }

                widget.IsActive = !widget.IsActive;
                widget.UpdatedAt = DateTime.UtcNow;

                _context.WidgetDefinition.Update(widget);
                _context.SaveChanges();

                return Ok(new { widgetId = widget.WidgetId, isActive = widget.IsActive });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new 
                { 
                    error = $"Error toggling widget status: {ex.Message}",
                    innerError = ex.InnerException?.Message
                });
            }
        }

        // ✅ แก้ไข Helper method นี้
        private int GetNextDisplayOrder()
        {
            try
            {
                // ถ้าไม่มีข้อมูลเลย ให้เริ่มที่ 1
                if (!_context.WidgetDefinition.Any())
                {
                    return 1;
                }
                
                // ถ้ามีข้อมูล ให้หา max + 1
                var maxOrder = _context.WidgetDefinition
                    .Max(w => (int?)w.DisplayOrder) ?? 0;
                
                return maxOrder + 1;
            }
            catch
            {
                return 1;  // ถ้า error อะไรก็ให้เริ่มที่ 1
            }
        }
    }

    // DTO
    public class WidgetDefinitionDto
    {
        public string WidgetId { get; set; }
        public string DisplayName { get; set; }
        public string ComponentName { get; set; }
        public string ComponentPath { get; set; }
        public bool IsActive { get; set; } = true;
        public bool Span2 { get; set; } = false;
        public int? DisplayOrder { get; set; }
        public string Description { get; set; }

        public string? DashboardType { get; set; }
    }
}