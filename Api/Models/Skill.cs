using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Skill
    {
        [Key]
        public int EmpID { get; set; }          // รหัสพนักงาน
        public string FirstName { get; set; }   // ชื่อพนักงาน
        public string LastName { get; set; }    // นามสกุลพนักงาน
        // public string Division { get; set; }    // แผนกที่พนักงานสังกัด
        // public string Department { get; set; }  // หน่วยงานที่พนักงานสังกัด
        // public string Position { get; set; }    // ตำแหน่งพนักงาน
        // public string Email { get; set; }       // อีเมลของพนักงาน
        // public string CourseNo { get; set; }    // หมายเลขหลักสูตร
        // public string CourseGroup { get; set; } // กลุ่มหลักสูตร
        // public string Biz { get; set; }         // ธุรกิจ
        // public string Process { get; set; }     // กระบวนการ
        public string SkillGroup { get; set; }  // กลุ่มทักษะ

        public int Material { get; set; }       // ระดับทักษะใน Material
        public int Operation { get; set; }      // ระดับทักษะใน Operation
        public int MachineSAB1 { get; set; }    // ระดับทักษะใน Machine:SAB#1
        public int MachineSAB2 { get; set; }    // ระดับทักษะใน Machine:SAB#2
        public int MachineSAB3 { get; set; }    // ระดับทักษะใน Machine:SAB#3
        public int Inspection { get; set; }     // ระดับทักษะในการตรวจสอบ
    }
}
