namespace ShiftWiseAI.Server.DTOs
{
    public class ShiftUpdate
    {
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShiftType { get; set; }
    }
}
