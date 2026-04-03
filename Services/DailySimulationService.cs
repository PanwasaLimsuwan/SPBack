using Microsoft.Data.SqlClient;

namespace Api.Services
{
    public class DailySimulationService
    {
        private readonly string _connectionString;
        private readonly Random _random = new Random();

        public DailySimulationService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // 🔥 เมธอดหลัก: สร้าง "วันถัดไปจากวันล่าสุด"
        public async Task RunNextDayAsync()
        {
            using var conn = new SqlConnection(_connectionString);
            await conn.OpenAsync();

            DateTime nextDate = await GetNextSimulationDateAsync(conn);

            // 🔥 ป้องกัน generate ซ้ำ
            if (await DayAlreadyExists(conn, nextDate))
                return;

            var employees = await GetEmployeesAsync(conn);

            foreach (var emp in employees)
            {
                // 10% ไม่มาทำงาน
                if (_random.NextDouble() < 0.1)
                    continue;

                DateTime checkIn = GenerateCheckIn(nextDate, emp.ShiftCode);

                await InsertTransaction(conn, emp.EmpID, checkIn, 1);

                DateTime expectedCheckout = GenerateCheckOut(checkIn, emp.ShiftCode);

                // 🔥 25% ยังไม่ออกงาน (ให้ดู realtime)
                if (_random.NextDouble() < 0.25)
                    continue;

                await InsertTransaction(conn, emp.EmpID, expectedCheckout, 2);
            }
        }

        // 🔥 หา "วันล่าสุด" แล้ว +1 วัน
        private async Task<DateTime> GetNextSimulationDateAsync(SqlConnection conn)
        {
            var cmd = new SqlCommand(
                @"
                SELECT MAX(CONVERT(date, Timestamp))
                FROM Transactions",
                conn
            );

            var result = await cmd.ExecuteScalarAsync();

            if (result == DBNull.Value || result == null)
                return new DateTime(2025, 8, 15); // วันเริ่มต้นระบบ

            DateTime lastDate = (DateTime)result;
            return lastDate.AddDays(1);
        }

        // 🔥 เช็คว่ามีข้อมูลวันนั้นแล้วหรือยัง
        private async Task<bool> DayAlreadyExists(SqlConnection conn, DateTime date)
        {
            var cmd = new SqlCommand(
                @"
                SELECT COUNT(*) 
                FROM Transactions
                WHERE CONVERT(date, Timestamp) = @Date",
                conn
            );

            cmd.Parameters.AddWithValue("@Date", date.Date);

            int count = (int)await cmd.ExecuteScalarAsync();
            return count > 0;
        }

        private DateTime GenerateCheckIn(DateTime date, string shift)
        {
            if (shift == "A")
                return date.Date.AddHours(7).AddMinutes(_random.Next(-30, 30));

            // B/C กะดึก
            return date.Date.AddHours(19).AddMinutes(_random.Next(-30, 30));
        }

        private DateTime GenerateCheckOut(DateTime checkIn, string shift)
        {
            if (shift == "A")
            {
                // กะเช้า 07:00 → 19:00
                return checkIn.Date.AddHours(19).AddMinutes(_random.Next(-30, 30));
            }
            else
            {
                // กะดึก 19:00 → 07:00 วันถัดไป
                return checkIn.Date.AddDays(1).AddHours(7).AddMinutes(_random.Next(-30, 30));
            }
        }

        private async Task InsertTransaction(
            SqlConnection conn,
            int empId,
            DateTime time,
            int cameraId
        )
        {
            var cmd = new SqlCommand(
                @"
                INSERT INTO Transactions (EmpID, Timestamp, CameraID)
                VALUES (@EmpID, @Timestamp, @CameraID)",
                conn
            );

            cmd.Parameters.AddWithValue("@EmpID", empId);
            cmd.Parameters.AddWithValue("@Timestamp", time);
            cmd.Parameters.AddWithValue("@CameraID", cameraId);

            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<List<(int EmpID, string ShiftCode)>> GetEmployeesAsync(
            SqlConnection conn
        )
        {
            var list = new List<(int, string)>();

            var cmd = new SqlCommand("SELECT EmpID, ShiftCode FROM EmployeeInfo", conn);
            var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                list.Add((reader.GetInt32(0), reader.GetString(1)));
            }

            await reader.CloseAsync();
            return list;
        }
    }
}
