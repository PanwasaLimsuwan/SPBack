// Models/ManpowerReq.cs
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ManpowerReq
    {
        [Key]
        public int MPRID { get; set; }
        public DateTime? Date { get; set; }
        public string? Biz { get; set; }
        public string? Process { get; set; }
        public int? Require { get; set; }
        public string? SkillGroup { get; set; }
        public int? Present { get; set; }       // ✅ เพิ่ม
        public int? Shortage { get; set; }      // ✅ เพิ่ม
        public DateTime? LastUpdateTime { get; set; } // ✅ เพิ่ม
    }
}