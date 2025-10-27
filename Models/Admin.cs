using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("Admin")] // กำหนดชื่อของตารางเป็น "Admin"
    public class Admin
    {
        [Key] // Primary Key
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] // ใช้ Auto Increment สำหรับ user_id
        [Column("user_id")]
        public int user_id { get; set; } // user_id จะเป็น PK และ Auto Increment

        [Required] // ฟิลด์ EmpID ที่จำเป็นต้องกรอก
        public int EmpID { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string Role { get; set; }

        public DateTime CreatedAt { get; set; } // เก็บวันที่สร้าง
    }
}
