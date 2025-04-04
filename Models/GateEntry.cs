using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class GateEntry
    {
        [Key]
        public int GateEntryID { get; set; }
        public string EmpID { get; set; }
    public DateTime? EntryDateTime { get; set; }
    public DateTime? ExitDateTime { get; set; }
    public string GateNo { get; set; }
    public string Room { get; set; }
    public string GateStatus { get; set; }
    }
}