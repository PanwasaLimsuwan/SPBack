using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GateToWorktimeController : ControllerBase
    {
        private readonly string _connectionString;

        public GateToWorktimeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpPost]
        public async Task<IActionResult> ConvertGateToWorktime()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    await conn.OpenAsync();

                    var selectCmd = new SqlCommand(@"
                        SELECT EmpID, EntryDateTime, ExitDateTime
                        FROM GateEntry
                        WHERE EntryDateTime IS NOT NULL", conn);

                    var reader = await selectCmd.ExecuteReaderAsync();

                    var worktimeData = new List<(string EmpID, DateTime Date, double WorkedHours, double OTHours, string Status)>();

                    while (await reader.ReadAsync())
                    {
                        var empId = reader["EmpID"].ToString();
                        var entry = (DateTime)reader["EntryDateTime"];

                        var exitValue = reader["ExitDateTime"];
                        DateTime? exit = exitValue == DBNull.Value ? (DateTime?)null : (DateTime)exitValue;

                        var workedHours = exit.HasValue ? (exit.Value - entry).TotalHours : 0;

                        // ✅ คำนวณ OT : ถ้า worked hours > 8 ชั่วโมง
                        var otHours = workedHours > 8 ? workedHours - 8 : 0;

                        var status = exit == null ? "Active" : "Finished";

                        worktimeData.Add((empId, entry.Date, workedHours, otHours, status));
                    }

                    await reader.CloseAsync();

                    int inserted = 0;
                    int updated = 0;

                    // ✅ ดึงรายการ CalculatedWorktime ที่มีอยู่แล้ว
                    var existingRecords = new Dictionary<string, (double WorkedHours, double OTHours, string Status)>();
                    var existingCmd = new SqlCommand("SELECT EmpID, Date, WorkedHours, OTHours, Status FROM CalculatedWorktime", conn);
                    using (var existingReader = await existingCmd.ExecuteReaderAsync())
                    {
                        while (await existingReader.ReadAsync())
                        {
                            var empId = existingReader["EmpID"].ToString();
                            var date = Convert.ToDateTime(existingReader["Date"]).Date;
                            var workedHours = Convert.ToDouble(existingReader["WorkedHours"]);
                            var otHours = Convert.ToDouble(existingReader["OTHours"]);
                            var status = existingReader["Status"].ToString();

                            existingRecords[$"{empId}_{date:yyyyMMdd}"] = (workedHours, otHours, status);
                        }
                    }

                    // ✅ Insert / Update
                    foreach (var item in worktimeData)
                    {
                        var key = $"{item.EmpID}_{item.Date:yyyyMMdd}";

                        if (!existingRecords.ContainsKey(key))
                        {
                            var insertCmd = new SqlCommand(@"
                                INSERT INTO CalculatedWorktime (EmpID, Date, WorkedHours, OTHours, Status)
                                VALUES (@EmpID, @Date, @WorkedHours, @OTHours, @Status)", conn);

                            insertCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
                            insertCmd.Parameters.AddWithValue("@Date", item.Date);
                            insertCmd.Parameters.AddWithValue("@WorkedHours", item.WorkedHours);
                            insertCmd.Parameters.AddWithValue("@OTHours", item.OTHours);
                            insertCmd.Parameters.AddWithValue("@Status", item.Status);

                            await insertCmd.ExecuteNonQueryAsync();
                            inserted++;
                        }
                        else
                        {
                            var existing = existingRecords[key];

                            if (existing.Status != item.Status || 
                                Math.Abs(existing.WorkedHours - item.WorkedHours) > 0.01 ||
                                Math.Abs(existing.OTHours - item.OTHours) > 0.01)
                            {
                                var updateCmd = new SqlCommand(@"
                                    UPDATE CalculatedWorktime
                                    SET WorkedHours = @WorkedHours, OTHours = @OTHours, Status = @Status
                                    WHERE EmpID = @EmpID AND Date = @Date", conn);

                                updateCmd.Parameters.AddWithValue("@EmpID", item.EmpID);
                                updateCmd.Parameters.AddWithValue("@Date", item.Date);
                                updateCmd.Parameters.AddWithValue("@WorkedHours", item.WorkedHours);
                                updateCmd.Parameters.AddWithValue("@OTHours", item.OTHours);
                                updateCmd.Parameters.AddWithValue("@Status", item.Status);

                                await updateCmd.ExecuteNonQueryAsync();
                                updated++;
                            }
                        }
                    }

                    return Ok($"Worktime inserted: {inserted} records, updated: {updated} records.");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
