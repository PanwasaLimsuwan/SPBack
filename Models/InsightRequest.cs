// using System.Data; // เพิ่ม namespace นี้เพื่อใช้ IDbConnection
// using Microsoft.Data.SqlClient; // ใช้สำหรับ SQL Server
// using System.ComponentModel.DataAnnotations;


// Models/InsightRequest.cs
public class InsightRequest
{
    public string key { get; set; } = "skill-gaps";
    public int? Year { get; set; }
    public int? Month { get; set; }
    public int? Weeks { get; set; }
    public string? Division { get; set; }
    public string? Department { get; set; }
    public string? Section { get; set; }
    public string? Biz { get; set; }
    public string? Process { get; set; }
}
