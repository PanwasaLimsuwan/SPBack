using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Worktime
{
    [Key]
    public int WorkTimeID { get; set; }
    public int EmpID { get; set; }
    public DateTime? Date { get; set; }
    public float? WorkedHours { get; set; }
    public float? OT_Hours { get; set; }
    public float? EICC_Hours { get; set; }
    // public float? OverloadHours { get; set; }
    public string Status { get; set; }
}
}