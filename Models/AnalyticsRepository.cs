// Data/AnalyticsRepository.cs
using Dapper;
using System.Data;
using Microsoft.Data.SqlClient;

public class AnalyticsRepository
{
    private readonly string _conn;
    public AnalyticsRepository(IConfiguration cfg) => _conn = cfg.GetConnectionString("DefaultConnection")!;

    IDbConnection Conn() => new SqlConnection(_conn);

    public async Task<IEnumerable<dynamic>> SkillGapByProcess(InsightRequest r)
    {
        // พารามิเตอร์แบบ choose/ALL
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
            Division = r.Division ?? "ALL", Department = r.Department ?? "ALL",
            Section = r.Section ?? "ALL", Biz = r.Biz ?? "ALL", Process = r.Process ?? "ALL"
        });
    }

    public async Task<IEnumerable<dynamic>> OT_Hotspots(InsightRequest r)
    {
        var sql = @"
-- จุดร้อน OT ช่วง N สัปดาห์ล่าสุด
WITH W AS (
  SELECT TOP (@Weeks * 7) w.* 
  FROM Worktime w
  ORDER BY w.Date DESC
)
SELECT ei.Process, ei.Section, ei.Biz,
       AVG(COALESCE(w.OT_Hours,0)) AS AvgOT,
       SUM(COALESCE(w.OT_Hours,0)) AS SumOT,
       COUNT(DISTINCT w.EmpID) AS EmpOT
FROM W w
JOIN EmployeeInfo ei ON ei.EmpID = w.EmpID
WHERE (@Division = 'ALL' OR ei.Division = @Division)
  AND (@Department = 'ALL' OR ei.Department = @Department)
  AND (@Section = 'ALL' OR ei.Section = @Section)
  AND (@Biz = 'ALL' OR ei.Biz = @Biz)
  AND (@Process = 'ALL' OR ei.Process = @Process)
GROUP BY ei.Process, ei.Section, ei.Biz
ORDER BY SumOT DESC;";
        using var db = Conn();
        return await db.QueryAsync(sql, new {
            Weeks = r.Weeks ?? 4,
            Division = r.Division ?? "ALL", Department = r.Department ?? "ALL",
            Section = r.Section ?? "ALL", Biz = r.Biz ?? "ALL", Process = r.Process ?? "ALL"
        });
    }

    public async Task<IEnumerable<dynamic>> HeadcountVsPlanVariance(InsightRequest r)
    {
        var sql = @"
-- แผน vs จริง (นับจาก EmployeeInfo + ManpowerPlan)
SELECT p.Year, p.Month, p.Process, 
       SUM(p.PlannedHeadcount) AS Planned,
       COUNT(ei.EmpID) AS Actual,
       (COUNT(ei.EmpID) - SUM(p.PlannedHeadcount)) AS Variance
FROM ManpowerPlan p
LEFT JOIN EmployeeInfo ei 
  ON ei.Process = p.Process
WHERE p.Year = @Year
  AND (@Process = 'ALL' OR p.Process = @Process)
GROUP BY p.Year, p.Month, p.Process
ORDER BY p.Year, p.Month, p.Process;";
        using var db = Conn();
        return await db.QueryAsync(sql, new {
            Year = r.Year ?? DateTime.Now.Year,
            Process = r.Process ?? "ALL"
        });
    }

    public async Task<IEnumerable<dynamic>> AbsenceStreaks(InsightRequest r)
    {
        var sql = @"
-- หาวันขาดงานต่อเนื่อง (ตัวอย่างง่าย)
SELECT ei.EmpID, ei.FirstName, ei.LastName, ei.Process,
       COUNT(*) AS AbsentDays
FROM Attendance a
JOIN EmployeeInfo ei ON ei.EmpID = a.EmpID
WHERE a.Status = 'Absent'
  AND YEAR(a.Date) = @Year
  AND MONTH(a.Date) = @Month
  AND (@Division = 'ALL' OR ei.Division = @Division)
  AND (@Department = 'ALL' OR ei.Department = @Department)
  AND (@Section = 'ALL' OR ei.Section = @Section)
  AND (@Biz = 'ALL' OR ei.Biz = @Biz)
  AND (@Process = 'ALL' OR ei.Process = @Process)
GROUP BY ei.EmpID, ei.FirstName, ei.LastName, ei.Process
HAVING COUNT(*) >= 2
ORDER BY AbsentDays DESC;";
        using var db = Conn();
        return await db.QueryAsync(sql, new {
            Year = r.Year ?? DateTime.Now.Year,
            Month = r.Month ?? DateTime.Now.Month,
            Division = r.Division ?? "ALL", Department = r.Department ?? "ALL",
            Section = r.Section ?? "ALL", Biz = r.Biz ?? "ALL", Process = r.Process ?? "ALL"
        });
    }

    public async Task<IEnumerable<dynamic>> EICC_Risk(InsightRequest r)
    {
        var sql = @"
-- สัญญาณเสี่ยง EICC (ชั่วโมงรวมสูง)
SELECT e.Process, e.Section, 
       SUM(COALESCE(w.EICC_Hours,0)) AS SumEICC,
       AVG(COALESCE(w.EICC_Hours,0)) AS AvgEICC
FROM Worktime w
JOIN EmployeeInfo e ON e.EmpID = w.EmpID
WHERE YEAR(w.Date) = @Year AND MONTH(w.Date) = @Month
  AND (@Division = 'ALL' OR e.Division = @Division)
  AND (@Department = 'ALL' OR e.Department = @Department)
  AND (@Section = 'ALL' OR e.Section = @Section)
  AND (@Biz = 'ALL' OR e.Biz = @Biz)
  AND (@Process = 'ALL' OR e.Process = @Process)
GROUP BY e.Process, e.Section
ORDER BY SumEICC DESC;";
        using var db = Conn();
        return await db.QueryAsync(sql, new {
            Year = r.Year ?? DateTime.Now.Year,
            Month = r.Month ?? DateTime.Now.Month,
            Division = r.Division ?? "ALL", Department = r.Department ?? "ALL",
            Section = r.Section ?? "ALL", Biz = r.Biz ?? "ALL", Process = r.Process ?? "ALL"
        });
    }
}
