using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EICCControlController : ControllerBase
    {
        private readonly string _connectionString;

        public EICCControlController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var results = new List<EICC_Control>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM EICC_Control", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    results.Add(new EICC_Control
                    {
                        ControlID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        WeekID = reader.GetInt32(2),
                        TotalHours = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                        DaysWorked = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        Status = reader.GetString(5)
                    });
                }
            }

            return Ok(results);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            EICC_Control result = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM EICC_Control WHERE ControlID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    result = new EICC_Control
                    {
                        ControlID = reader.GetInt32(0),
                        EmpID = reader.GetString(1),
                        WeekID = reader.GetInt32(2),
                        TotalHours = reader.IsDBNull(3) ? null : reader.GetFloat(3),
                        DaysWorked = reader.IsDBNull(4) ? null : reader.GetInt32(4),
                        Status = reader.GetString(5)
                    };
                }
            }

            if (result == null)
                return NotFound();

            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> Create(EICC_Control control)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO EICC_Control (EmpID, WeekID, TotalHours, DaysWorked, Status)
                    VALUES (@EmpID, @WeekID, @TotalHours, @DaysWorked, @Status)", conn);

                cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                cmd.Parameters.AddWithValue("@TotalHours", (object)control.TotalHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DaysWorked", (object)control.DaysWorked ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", control.Status);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, EICC_Control control)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE EICC_Control SET
                        EmpID = @EmpID,
                        WeekID = @WeekID,
                        TotalHours = @TotalHours,
                        DaysWorked = @DaysWorked,
                        Status = @Status
                    WHERE ControlID = @ControlID", conn);

                cmd.Parameters.AddWithValue("@ControlID", id);
                cmd.Parameters.AddWithValue("@EmpID", control.EmpID);
                cmd.Parameters.AddWithValue("@WeekID", control.WeekID);
                cmd.Parameters.AddWithValue("@TotalHours", (object)control.TotalHours ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@DaysWorked", (object)control.DaysWorked ?? DBNull.Value);
                cmd.Parameters.AddWithValue("@Status", control.Status);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM EICC_Control WHERE ControlID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
