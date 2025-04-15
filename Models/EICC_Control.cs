using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class EICC_Control
    {
        [Key]
        public int ControlID { get; set; }
        public int EmpID { get; set; }
        public int WeekID { get; set; }
        public float? TotalHours { get; set; }
        public int? DaysWorked { get; set; }
        public float? TotalOT { get; set; } // ✅ เพิ่มตรงนี้!!
        public string Status { get; set; }
    }
}
