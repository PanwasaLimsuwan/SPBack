using System.Data;  // ใช้สำหรับ IDbConnection
using Microsoft.Data.SqlClient;  // สำหรับการเชื่อมต่อ SQL Server
using Dapper;  // ใช้ Dapper ในการ query ฐานข้อมูล
using System.Collections.Generic;
using System.Threading.Tasks;

public class AnalyticsRepository
{
    private readonly string _conn;
    public AnalyticsRepository(IConfiguration cfg) => _conn = cfg.GetConnectionString("DefaultConnection")!;

    IDbConnection Conn() => new SqlConnection(_conn);

    // ฟังก์ชันสำหรับ SkillGapByProcess
    public async Task<IEnumerable<dynamic>> SkillGapByProcess(InsightRequest r)
    {
        var sql = @"
        SELECT ei.Process, s.SkillName,
               SUM(CASE WHEN o.Level IS NULL OR o.Level < s.RequiredLevel THEN 1 ELSE 0 END) AS GapCount,
               COUNT(*) AS EmpCount
        FROM EmployeeInfo ei
        LEFT JOIN OJTandInspectionSkill o ON o.EmpID = ei.EmpID
        LEFT JOIN Skill s ON s.SkillID = o.SkillID
        WHERE COALESCE(@Division, 'ALL') = 'ALL' OR ei.Division = @Division
          AND COALESCE(@Department, 'ALL') = 'ALL' OR ei.Department = @Department
          AND COALESCE(@Section, 'ALL') = 'ALL' OR ei.Section = @Section
          AND COALESCE(@Biz, 'ALL') = 'ALL' OR ei.Biz = @Biz
          AND COALESCE(@Process, 'ALL') = 'ALL' OR ei.Process = @Process
        GROUP BY ei.Process, s.SkillName
        ORDER BY GapCount DESC;
        ";

        using var db = Conn();
        return await db.QueryAsync(sql, new {
            Division = r.Division ?? "ALL", 
            Department = r.Department ?? "ALL",
            Section = r.Section ?? "ALL", 
            Biz = r.Biz ?? "ALL", 
            Process = r.Process ?? "ALL"
        });
    }

    // ฟังก์ชัน OT_Hotspots
    public async Task<IEnumerable<dynamic>> OT_Hotspots(InsightRequest r)
    {
        var sql = @"
        -- SQL สำหรับ OT Hotspots
        ";
        using var db = Conn();
        return await db.QueryAsync(sql, new { /* พารามิเตอร์ */ });
    }

    // ฟังก์ชัน HeadcountVsPlanVariance
    public async Task<IEnumerable<dynamic>> HeadcountVsPlanVariance(InsightRequest r)
    {
        var sql = @"
        -- SQL สำหรับ Headcount vs Plan
        ";
        using var db = Conn();
        return await db.QueryAsync(sql, new { /* พารามิเตอร์ */ });
    }

    // ฟังก์ชัน AbsenceStreaks
    public async Task<IEnumerable<dynamic>> AbsenceStreaks(InsightRequest r)
    {
        var sql = @"
        -- SQL สำหรับ Absence Streaks
        ";
        using var db = Conn();
        return await db.QueryAsync(sql, new { /* พารามิเตอร์ */ });
    }

    // ฟังก์ชัน EICC_Risk
    public async Task<IEnumerable<dynamic>> EICC_Risk(InsightRequest r)
    {
        var sql = @"
        -- SQL สำหรับ EICC Risk
        ";
        using var db = Conn();
        return await db.QueryAsync(sql, new { /* พารามิเตอร์ */ });
    }
}
