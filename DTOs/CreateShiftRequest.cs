namespace ShiftWiseAI.Server.DTOs
{
    public class CreateShiftRequest
    {
        public DateTime ShiftDate { get; set; }
        public int DaysCount { get; set; } = 1;
        public bool SkipWeekends { get; set; } = false;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShiftType { get; set; } // Morning, Evening, Night
    }
}
