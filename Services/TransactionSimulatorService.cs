using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

public class TransactionSimulatorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public TransactionSimulatorService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
                var connStr = config.GetConnectionString("DefaultConnection");
                await SimulateTodayAsync(connStr);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[TransactionSimulator] Error: {ex.Message}");
            }
            await Task.Delay(TimeSpan.FromMinutes(5), ct);
        }
    }

    public async Task RunOnceAsync(string connStr)
    {
        await SimulateTodayAsync(connStr);
    }

    private async Task SimulateTodayAsync(string connStr)
    {
        var now = DateTime.Now;
        var tasks = new List<(string shift, DateTime workDate)>();

        if (now.Hour >= 7 && now.Hour < 19)
        {
            tasks.Add(("DAY", DateTime.Today));
        }
        else if (now.Hour >= 19)
        {
            tasks.Add(("NIGHT", DateTime.Today));
            if (now.Hour < 21)
                tasks.Add(("DAY", DateTime.Today));
        }
        else
        {
            tasks.Add(("NIGHT", DateTime.Today.AddDays(-1)));
        }

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        foreach (var (activeShift, workDate) in tasks)
        {
            await ProcessShiftAsync(conn, activeShift, workDate, now);
        }
    }

    private async Task ProcessShiftAsync(
        SqlConnection conn,
        string activeShift,
        DateTime workDate,
        DateTime now
    )
    {
        var empList = new List<(int EmpID, string Shift)>();
        var selectSql =
            @"
        SELECT e.EmpID, m.Shift
        FROM EmployeeInfo e
        JOIN ManpowerPlan m
            ON CAST(m.Date AS DATE) = @workDate
            AND LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
        WHERE m.Shift = @activeShift";

        using (var cmd = new SqlCommand(selectSql, conn))
        {
            cmd.Parameters.AddWithValue("@workDate", workDate);
            cmd.Parameters.AddWithValue("@activeShift", activeShift);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                empList.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        foreach (var (empID, shift) in empList)
        {
            // ✅ ใช้ helper เดียวกัน → baseTime และ rng state ต่อเนื่อง
            var (baseTime, shouldSkip, seededRng) = CalculateBaseTime(empID, workDate, shift);

            if (shouldSkip || baseTime > now)
                continue;

            var existingTimestamps = await GetExistingTimestamps(conn, empID, workDate, shift);

            await InsertIfNotExists(conn, empID, baseTime, 1, existingTimestamps);

            var t = baseTime.AddMinutes(2 + seededRng.Next(0, 3));
            await InsertIfNotExists(conn, empID, t, 2, existingTimestamps);

            t = t.AddMinutes(3 + seededRng.Next(0, 3));
            await InsertIfNotExists(conn, empID, t, 3, existingTimestamps);

            int maxLoop = 8 + seededRng.Next(0, 4);
            for (int loop = 1; loop <= maxLoop; loop++)
            {
                int offset = (60 * loop) + seededRng.Next(0, 30);
                var loopTime = baseTime.AddMinutes(offset);
                if (loopTime > now)
                    break;

                await InsertIfNotExists(conn, empID, loopTime, 2, existingTimestamps);

                int breakMin = 10 + seededRng.Next(0, 10);
                var breakTime = loopTime.AddMinutes(breakMin);
                if (breakTime > now)
                    break;

                await InsertIfNotExists(conn, empID, breakTime, 3, existingTimestamps);
            }

            // ✅ แก้ตรงนี้: ยึด shift end time แทน baseTime + 12h
            DateTime shiftEnd =
                activeShift == "DAY" ? workDate.AddHours(19) : workDate.AddDays(1).AddHours(7);

            // endWork = shiftEnd ± random (แต่ไม่เกิน shiftEnd + 90 นาที)
            var endWork = shiftEnd;
            if (seededRng.Next(100) < 20)
                endWork = endWork.AddMinutes(30 + seededRng.Next(0, 90)); // OT
            if (seededRng.Next(100) < 5)
                endWork = endWork.AddMinutes(-(30 + seededRng.Next(0, 60))); // ออกก่อน

            var exitCleanroom = endWork.AddMinutes(-5);
            if (exitCleanroom <= now)
                await InsertIfNotExists(conn, empID, exitCleanroom, 2, existingTimestamps);
            if (endWork <= now)
                await InsertIfNotExists(conn, empID, endWork, 1, existingTimestamps);
        }
    }

    public async Task SimulateEndShiftAsync(string connStr, string shift)
    {
        var now = DateTime.Now;

        // NIGHT shift ที่กำลังจบตอนเช้า (00:00-12:00) = กะเริ่มเมื่อวาน
        var workDate =
            (shift == "NIGHT" && now.Hour < 12) ? DateTime.Today.AddDays(-1) : DateTime.Today;

        using var conn = new SqlConnection(connStr);
        await conn.OpenAsync();

        var empList = new List<(int EmpID, string Shift)>();
        var selectSql =
            @"
            SELECT e.EmpID, m.Shift
            FROM EmployeeInfo e
            JOIN ManpowerPlan m
                ON CAST(m.Date AS DATE) = @workDate
                AND LTRIM(RTRIM(m.ShiftCode)) = LTRIM(RTRIM(e.ShiftCode))
            WHERE m.Shift = @shift";

        using (var cmd = new SqlCommand(selectSql, conn))
        {
            cmd.Parameters.AddWithValue("@workDate", workDate);
            cmd.Parameters.AddWithValue("@shift", shift);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                empList.Add((reader.GetInt32(0), reader.GetString(1)));
        }

        foreach (var (empID, _) in empList)
        {
            // ✅ ใช้ helper เดียวกัน → baseTime ตรงกับ ProcessShiftAsync เสมอ
            var (baseTime, shouldSkip, _) = CalculateBaseTime(empID, workDate, shift);

            if (shouldSkip)
                continue;

            var existingTimestamps = await GetExistingTimestamps(conn, empID, workDate, shift);

            // Insert ตอนเข้างาน ถ้ายังไม่มี
            await InsertIfNotExists(conn, empID, baseTime, 1, existingTimestamps);
            await InsertIfNotExists(conn, empID, baseTime.AddMinutes(2), 2, existingTimestamps);
            await InsertIfNotExists(conn, empID, baseTime.AddMinutes(5), 3, existingTimestamps);

            // Insert ตอนออกงาน ใช้เวลาปัจจุบัน
            var exitCleanroom = now.AddMinutes(-5);
            await InsertIfNotExists(conn, empID, exitCleanroom, 2, existingTimestamps);
            await InsertIfNotExists(conn, empID, now, 1, existingTimestamps);
        }
    }

    // ✅ Helper: คำนวณ baseTime และ return rng ที่ consume แล้ว
    //    ทั้ง ProcessShiftAsync และ SimulateEndShiftAsync เรียก method นี้
    //    → ได้ baseTime ตัวเดียวกันเสมอ ไม่มีโอกาส duplicate
    private (DateTime baseTime, bool shouldSkip, Random rng) CalculateBaseTime(
        int empID,
        DateTime workDate,
        string shift
    )
    {
        var rng = new Random(empID + workDate.DayOfYear);
        DateTime baseTime = shift == "DAY" ? workDate.AddHours(7) : workDate.AddHours(19);

        baseTime = baseTime.AddMinutes(rng.Next(0, 15));
        if (rng.Next(100) < 15)
            baseTime = baseTime.AddMinutes(5 + rng.Next(0, 25));
        bool shouldSkip = rng.Next(100) < 5;

        // ✅ return rng ที่ consume state ผ่าน CalculateBaseTime แล้ว
        //    ProcessShiftAsync ใช้ต่อได้เลย โดย state ต่อเนื่อง
        return (baseTime, shouldSkip, rng);
    }

    // ดึง timestamps ที่มีอยู่แล้วใน DB สำหรับ shift window นั้น
    private async Task<HashSet<string>> GetExistingTimestamps(
        SqlConnection conn,
        int empID,
        DateTime workDate,
        string shift
    )
    {
        var set = new HashSet<string>();

        DateTime startTime,
            endTime;
        if (shift == "DAY")
        {
            startTime = workDate.AddHours(7);
            endTime = workDate.AddHours(21); // ✅ +2h เผื่อ OT สูงสุด 90 นาที
        }
        else
        {
            startTime = workDate.AddHours(19);
            endTime = workDate.AddDays(1).AddHours(9); // ✅ +2h เผื่อ OT
        }

        var sql =
            @"
        SELECT CONVERT(VARCHAR, Timestamp, 120), CameraID 
        FROM Transactions 
        WHERE EmpID = @empID 
          AND Timestamp BETWEEN @start AND @end";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@empID", empID);
        cmd.Parameters.AddWithValue("@start", startTime);
        cmd.Parameters.AddWithValue("@end", endTime);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
            set.Add($"{reader.GetString(0)}_{reader.GetInt32(1)}");

        return set;
    }

    // Insert เฉพาะถ้ายังไม่มี event นี้ใน DB
    private async Task InsertIfNotExists(
        SqlConnection conn,
        int empID,
        DateTime timestamp,
        int cameraID,
        HashSet<string> existing
    )
    {
        var key = $"{timestamp:yyyy-MM-dd HH:mm:ss}_{cameraID}";
        if (existing.Contains(key))
            return;

        // ✅ เปลี่ยนแค่ตรงนี้
        var sql =
            @"
        IF NOT EXISTS (
            SELECT 1 FROM Transactions 
            WHERE EmpID = @empID 
              AND Timestamp = @ts 
              AND CameraID = @cam
        )
        INSERT INTO Transactions (EmpID, Timestamp, CameraID) 
        VALUES (@empID, @ts, @cam)";

        using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@empID", empID);
        cmd.Parameters.AddWithValue("@ts", timestamp);
        cmd.Parameters.AddWithValue("@cam", cameraID);
        await cmd.ExecuteNonQueryAsync();

        existing.Add(key);
    }
}
