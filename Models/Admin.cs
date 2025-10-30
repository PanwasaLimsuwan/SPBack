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

        // เปลี่ยนให้เป็น Nullable
    public string? Role { get; set; } // หรือใช้ string? ถ้าคุณต้องการให้เป็น null ได้
    public string? PasswordHash { get; set; } // หรือใช้ string? ถ้าคุณต้องการให้เป็น null ได้
    }
}
