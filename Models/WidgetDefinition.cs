using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("WidgetDefinition")]
    public class WidgetDefinition
    {
        [Key]
        public string WidgetId { get; set; } // ต้องไม่เป็น null
        public string DisplayName { get; set; }
        public string ComponentName { get; set; }
        public string? ComponentPath { get; set; }
        public bool IsActive { get; set; } = true; // ✅ ต้องมีค่า default
        public bool Span2 { get; set; } = false; // ✅ ต้องมีค่า default
        public int DisplayOrder { get; set; }
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        
        public string? DashboardType { get; set; }
    }
}

// เพิ่มใน ApplicationDbContext.cs:
// public DbSet<WidgetDefinition> WidgetDefinition { get; set; }
