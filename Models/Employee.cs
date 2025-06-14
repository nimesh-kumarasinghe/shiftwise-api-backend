namespace ShiftWiseAI.Server.Models
{
    public class Employee
    {
        public Guid Id { get; set; }
        public Guid OrganizationId { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public int MaxWeeklyHours { get; set; }
        public string PreferredShiftType { get; set; }
        public ICollection<Shift> AssignedShifts { get; set; }
    }
}
