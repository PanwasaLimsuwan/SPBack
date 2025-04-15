using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    
public class OJTandInspectionSkill
{
    [Key]
    public string CourseNo { get; set; }
    public string CourseGroup { get; set; }
    public string Biz { get; set; }
    public string Process { get; set; }
    public string CerNo { get; set; }
    public int? Active { get; set; }
    public string SkillGroup { get; set; }
    public int EmpID { get; set; }
}
}