namespace ShiftWiseAI.Server.DTOs
{
    public class ShiftUpdate
    {
        public DateTime ShiftDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShiftType { get; set; }
    }

    public class ShiftDto
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public DateTime ShiftDate { get; set; }
        public int DaysCount { get; set; }
        public bool SkipWeekends { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public string ShiftType { get; set; }
        public bool IsConfirmed { get; set; }
        public bool IsInformed { get; set; }
        public List<AssignedEmployeeDto> Assignments { get; set; }
    }

    public class AssignedEmployeeDto
    {
        public Guid Id { get; set; }
        public Guid EmployeeId { get; set; }
        public string FullName { get; set; } // Or whatever properties you want
        public string Email { get; set; }
    }

}
