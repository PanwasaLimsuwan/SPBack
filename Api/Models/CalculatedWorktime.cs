using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CalculatedWorktime
    {
        [Key]
        public int WorktimeID { get; set; }
        public int EmpID { get; set; }
        public DateTime Date { get; set; }
        public double WorkedHours { get; set; }
        public double OTHours { get; set; } // ✅ เพิ่มตรงนี้!
        public string Status { get; set; }
    }
}
