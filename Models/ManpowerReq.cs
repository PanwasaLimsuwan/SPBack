using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ManpowerReq
{
    [Key]
    public int MPRID { get; set; }
    public DateTime? Date { get; set; }
    public string? Biz { get; set; }
    public string? Process { get; set; }
    public int? Require { get; set; }
    public string? SkillGroup { get; set; }
}
}