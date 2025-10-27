using System;
using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class EICC_MonthlySummary
    {
        [Key]
        public int SummaryID { get; set; }
        public int EmpID { get; set; }
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal OT_Total { get; set; }      // OT ทั้งหมด
    public decimal EICC_Hours { get; set; }    // เช่น 240.5 hrs
    public decimal TotalHours { get; set; }    // เวลารวมในเดือนนั้น
    }
}
