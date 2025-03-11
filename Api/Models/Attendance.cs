using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Attendance
{
    [Key]
    public int AttendanceID { get; set; }
    public string EmpID { get; set; }
    public DateTime? Date { get; set; }
    public TimeSpan? CheckInTime { get; set; }
    public TimeSpan? CheckOutTime { get; set; }
    public TimeSpan? ScheduledStartTime { get; set; }
    public TimeSpan? ScheduledEndTime { get; set; }
    public string? Status { get; set; }
    public int? WeekNumber { get; set; }
}
}