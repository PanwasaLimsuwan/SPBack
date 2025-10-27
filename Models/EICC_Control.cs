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
        public double? TotalOT { get; set; } // ✅ เพิ่มตรงนี้!!
        public string Status { get; set; }
    }
}
