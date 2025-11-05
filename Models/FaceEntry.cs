using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Api.Models
{
    [Table("FaceVectors")]
    public class FaceEntry
    {
        [Key]  // ⭐⭐⭐ เพิ่มบรรทัดนี้ - สำคัญมาก!
        public int FaceVectorID { get; set; }
        
        public int EmpID { get; set; }
        public string Vector { get; set; }
        public DateTime Timestamp { get; set; }
    }
}