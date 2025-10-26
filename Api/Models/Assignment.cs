using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class Assignment
    {
        [Key]
        public int AssignmentID { get; set; }

        public int EmpID { get; set; }
        public string FromBiz { get; set; }
        public string FromProcess { get; set; }

        public string ToBiz { get; set; }
        public string ToProcess { get; set; }
        public string SkillGroup { get; set; }

        public DateTime StartAt { get; set; }
        public DateTime? EndAt { get; set; }

        public string Status { get; set; } = "Active"; // Active / Cancelled / Done
    }
}
