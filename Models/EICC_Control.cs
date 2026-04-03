using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class EICC_Control
    {
        [Key]
        public int ControlID { get; set; }
        public int EmpID { get; set; }
        public int WeekID { get; set; }

        public double? TotalHours { get; set; }
        public int? DaysWorked { get; set; }

        public double? OTHours { get; set; }   // 👈 เพิ่ม
        public double? TotalOT { get; set; }

        public int? Year { get; set; }         // 👈 เพิ่ม

        public string Status { get; set; }
    }
}