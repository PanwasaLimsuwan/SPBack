using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class HeadcountTransition
{
    [Key]
    public DateTime DateTime { get; set; }
    public string? TransType { get; set; }
    public string? EmpID { get; set; }
}
}