using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class EmployeeInfo
    {
        [Key]
        public int EmpID { get; set; }   // Primary Key
        public long? GID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Division { get; set; }
        public string? Department { get; set; }
        public string? Section { get; set; }
        public string? JobGrade { get; set; }
        public int? BossID { get; set; }
        public long? BossGID { get; set; }
        public string? CostCenter { get; set; }
        public string? ShiftCode { get; set; }
        public string? Position { get; set; }
        public string Email { get; set; }
        public int PlanID { get; set; }
        public ManpowerPlan ManpowerPlan { get; set; }

        public ICollection<GateEntry> GateEntry { get; set; }
        public ICollection<CleanroomEntry> CleanroomEntry { get; set; }
        public ICollection<OJTandInspectionSkill> OJTandInspectionSkill { get; set; }
        public ICollection<HeadcountTransition> HeadcountTransition { get; set; }
        public ICollection<Worktime> Worktime { get; set; } 
        public ICollection<EICC_Control> EICC_Control { get; set; }
        public ICollection<Attendance> Attendance { get; set; }
    }
}
