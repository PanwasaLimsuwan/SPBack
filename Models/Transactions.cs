using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Transactions
    {
        [Key]
        public int TransacID { get; set; }  // ใช้เป็น Primary Key
        
        [Required]
        public DateTime Timestamp { get; set; }  // คอลัมน์สำหรับเวลาที่เข้า/ออก

        [Required]
        public int CameraID { get; set; }  // คอลัมน์สำหรับ CameraID

        [Required]
        public int EmpID { get; set; }  // คอลัมน์สำหรับ EmpID

        // คุณสามารถเพิ่มคอลัมน์เพิ่มเติมตามที่ต้องการ เช่น ExitDateTime หรือรายละเอียดอื่นๆ
    }
}
