using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Api.Models;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ManpowerReqController : ControllerBase
    {
        private readonly string _connectionString;

        public ManpowerReqController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // ✅ GET: api/ManpowerReq (พร้อม Filter และ Join EmployeeInfo)
// ✅ GET: api/ManpowerReq (พร้อม Filter)
[HttpGet]
public async Task<IActionResult> GetAll(
    [FromQuery] string? division,
    [FromQuery] string? department,
    [FromQuery] string? section,
    [FromQuery] string? biz,
    [FromQuery] string? process
)
{
    var result = new List<object>();

    using (var conn = new SqlConnection(_connectionString))
    {
        await conn.OpenAsync();

        var query = @"
            SELECT 
                m.MPRID,
                m.Date,
                m.Biz,
                m.Process,
                m.Require,
                m.SkillGroup,
                e.Division,
                e.Department,
                e.Section,
                e.Biz AS EmployeeBiz,
                e.Process AS EmployeeProcess
            FROM ManpowerReq m
            LEFT JOIN EmployeeInfo e ON m.Biz = e.Biz AND m.Process = e.Process
            WHERE 1=1";

        var parameters = new List<SqlParameter>();

        if (!string.IsNullOrEmpty(biz))
        {
            query += " AND m.Biz = @Biz";
            parameters.Add(new SqlParameter("@Biz", biz));
        }

        if (!string.IsNullOrEmpty(process))
        {
            query += " AND m.Process = @Process";
            parameters.Add(new SqlParameter("@Process", process));
        }

        if (!string.IsNullOrEmpty(division))
        {
            query += " AND (e.Division = @Division OR e.Division IS NULL)";
            parameters.Add(new SqlParameter("@Division", division));
        }

        if (!string.IsNullOrEmpty(department))
        {
            query += " AND (e.Department = @Department OR e.Department IS NULL)";
            parameters.Add(new SqlParameter("@Department", department));
        }

        if (!string.IsNullOrEmpty(section))
        {
            query += " AND (e.Section = @Section OR e.Section IS NULL)";
            parameters.Add(new SqlParameter("@Section", section));
        }

        using (var cmd = new SqlCommand(query, conn))
        {
            cmd.Parameters.AddRange(parameters.ToArray());

            using (var reader = await cmd.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    result.Add(new
                    {
                        // mprID = reader["MPRID"]?.ToString(),
                        mprID = reader.GetInt32(0),
                        date = reader["Date"] == DBNull.Value ? null : (DateTime?)reader["Date"],
                        biz = reader["Biz"]?.ToString(),
                        process = reader["Process"]?.ToString(),
                        require = reader["Require"] == DBNull.Value ? null : (int?)reader["Require"],
                        skillGroup = reader["SkillGroup"]?.ToString(),
                        division = reader["Division"]?.ToString(),
                        department = reader["Department"]?.ToString(),
                        section = reader["Section"]?.ToString(),
                        employeeBiz = reader["EmployeeBiz"]?.ToString(),
                        employeeProcess = reader["EmployeeProcess"]?.ToString()
                    });
                }
            }
        }
    }

    return Ok(result);
}
    }
}
