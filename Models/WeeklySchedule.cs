using System.ComponentModel.DataAnnotations;

namespace Api.Models
{

public class WeeklySchedule
{
    [Key]
    public int WeekID { get; set; }
    public int Year { get; set; }
    public string Month { get; set; }
    public int Week { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public int? AbsentCount { get; set; }
}
}