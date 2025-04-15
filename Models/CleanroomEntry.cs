using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CleanroomEntry
    {
        [Key]
        public int CEntryID { get; set; }
    public int EmpID { get; set; }
    public DateTime? CheckInDateTime { get; set; }
    public DateTime? CheckOutDateTime { get; set; }
    // public string? LocationStatus { get; set; }
    // public DateTime? HeadCountDate { get; set; }
    public string CStatus { get; set; }
    }
}