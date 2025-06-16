namespace ShiftWiseAI.Server.Models
{
    public class Shift
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }

        public string ShiftType { get; set; } // Morning, Evening, Night, etc.

        public ICollection<ShiftAssignment> Assignments { get; set; }
    }
}
