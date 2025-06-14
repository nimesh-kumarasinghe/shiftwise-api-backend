namespace ShiftWiseAI.Server.Models
{
    public class Shift
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
        public string ShiftType { get; set; } // e.g., "Morning", "Evening"
    }
}
