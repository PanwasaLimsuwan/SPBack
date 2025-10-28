namespace Api.Models
{
    public class FaceEntry
    {
        public int FaceEntryID { get; set; }
        public int EmpID { get; set; }
        public string Vector { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
