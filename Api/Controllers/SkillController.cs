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
    public class SkillController : ControllerBase
    {
        private readonly string _connectionString;

        public SkillController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // GET: api/Skill
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var skills = new List<Skill>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM Skill", conn);
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    skills.Add(new Skill
                    {
                        EmpID = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        SkillGroup = reader.GetString(3),
                        Material = reader.GetInt32(4),
                        Operation = reader.GetInt32(5),
                        MachineSAB1 = reader.GetInt32(6),
                        MachineSAB2 = reader.GetInt32(7),
                        MachineSAB3 = reader.GetInt32(8),
                        Inspection = reader.GetInt32(9)
                    });
                }
            }

            return Ok(skills);
        }

        // GET: api/Skill/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            Skill skill = null;

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("SELECT * FROM Skill WHERE EmpID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    skill = new Skill
                    {
                        EmpID = reader.GetInt32(0),
                        FirstName = reader.GetString(1),
                        LastName = reader.GetString(2),
                        SkillGroup = reader.GetString(3),
                        Material = reader.GetInt32(4),
                        Operation = reader.GetInt32(5),
                        MachineSAB1 = reader.GetInt32(6),
                        MachineSAB2 = reader.GetInt32(7),
                        MachineSAB3 = reader.GetInt32(8),
                        Inspection = reader.GetInt32(9)
                    };
                }
            }

            if (skill == null)
                return NotFound();

            return Ok(skill);
        }

        // POST: api/Skill
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] Skill skill)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    INSERT INTO Skill 
                        (EmpID, FirstName, LastName, SkillGroup, Material, Operation, MachineSAB1, MachineSAB2, MachineSAB3, Inspection)
                    VALUES 
                        (@EmpID, @FirstName, @LastName, @SkillGroup, @Material, @Operation, @MachineSAB1, @MachineSAB2, @MachineSAB3, @Inspection)", conn);

                cmd.Parameters.AddWithValue("@EmpID", skill.EmpID);
                cmd.Parameters.AddWithValue("@FirstName", skill.FirstName);
                cmd.Parameters.AddWithValue("@LastName", skill.LastName);
                cmd.Parameters.AddWithValue("@SkillGroup", skill.SkillGroup);
                cmd.Parameters.AddWithValue("@Material", skill.Material);
                cmd.Parameters.AddWithValue("@Operation", skill.Operation);
                cmd.Parameters.AddWithValue("@MachineSAB1", skill.MachineSAB1);
                cmd.Parameters.AddWithValue("@MachineSAB2", skill.MachineSAB2);
                cmd.Parameters.AddWithValue("@MachineSAB3", skill.MachineSAB3);
                cmd.Parameters.AddWithValue("@Inspection", skill.Inspection);

                await cmd.ExecuteNonQueryAsync();
            }

            return Ok("Created");
        }

        // PUT: api/Skill/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Skill skill)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"
                    UPDATE Skill SET 
                        FirstName = @FirstName,
                        LastName = @LastName,
                        SkillGroup = @SkillGroup,
                        Material = @Material,
                        Operation = @Operation,
                        MachineSAB1 = @MachineSAB1,
                        MachineSAB2 = @MachineSAB2,
                        MachineSAB3 = @MachineSAB3,
                        Inspection = @Inspection
                    WHERE EmpID = @EmpID", conn);

                cmd.Parameters.AddWithValue("@EmpID", id);
                cmd.Parameters.AddWithValue("@FirstName", skill.FirstName);
                cmd.Parameters.AddWithValue("@LastName", skill.LastName);
                cmd.Parameters.AddWithValue("@SkillGroup", skill.SkillGroup);
                cmd.Parameters.AddWithValue("@Material", skill.Material);
                cmd.Parameters.AddWithValue("@Operation", skill.Operation);
                cmd.Parameters.AddWithValue("@MachineSAB1", skill.MachineSAB1);
                cmd.Parameters.AddWithValue("@MachineSAB2", skill.MachineSAB2);
                cmd.Parameters.AddWithValue("@MachineSAB3", skill.MachineSAB3);
                cmd.Parameters.AddWithValue("@Inspection", skill.Inspection);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }

        // DELETE: api/Skill/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                var cmd = new SqlCommand("DELETE FROM Skill WHERE EmpID = @id", conn);
                cmd.Parameters.AddWithValue("@id", id);

                int rowsAffected = await cmd.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                    return NotFound();
            }

            return NoContent();
        }
    }
}
