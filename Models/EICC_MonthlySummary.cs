using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class EICC_MonthlySummary
    {
        [Key]
        public int SummaryID { get; set; }
        public string EmpID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public int OT_Total { get; set; }           // OT ทั้งหมด
        public float EICC_Hours { get; set; }       // เช่น 240 hrs
        public float TotalHours { get; set; }       // เวลารวมในเดือนนั้น
    }
}
