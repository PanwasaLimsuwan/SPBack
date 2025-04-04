using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class HeadcountTransition
{
    [Key] // ใช้ ID เป็น Primary Key
    public int Id { get; set; }  

    public DateTime DateTime { get; set; } 
    public string? TransType { get; set; } 
    public string? EmpID { get; set; } 
}

}