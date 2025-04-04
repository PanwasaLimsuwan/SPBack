using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ManpowerPlan
{
    [Key]
    public int PlanID { get; set; }
    public DateTime? Date { get; set; }
    public string EmpID { get; set; }
    // public string Attendance { get; set; }
    public string ShiftCode { get; set; }
    public string Shift { get; set; }
    public int? PlannedHeadcount { get; set; }
    public int? ActualHeadcount { get; set; }

    public ICollection<ManpowerReq> ManpowerReq { get; set; }
    public ICollection<EmployeeInfo> EmployeeInfo { get; set; }
}
}