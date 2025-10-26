// using System;
// using System.ComponentModel.DataAnnotations;
// // using System.ComponentModel.DataAnnotations.Schema;
// // using System.Text.Json.Serialization;

// namespace Api.Models
// {
//     public class Admin
//     {
//         public int user_id { get; set; }
//         public string Username { get; set; }
//         // public string Password { get; set; } // รหัสผ่านที่ไม่แฮช
//         public string PasswordHash { get; set; } // รหัสผ่านที่แฮชแล้ว
//         // public string Role { get; set; } // เช่น 'superadmin', 'admin'

//         // [NotMapped] // ไม่สร้าง column ใน DB
//         // [JsonPropertyName("password")]
//         // public string Password { get; set; }  // ใช้รับ input ชั่วคราว
//     }
// }

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("Admin")] // กันเหนียวว่าแมพไปที่ตารางชื่อ Admin แน่ๆ
    public class Admin
    {
        [Key]
        [Column("user_id")]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int user_id { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string PasswordHash { get; set; }
    }
}
