namespace ShiftWiseAI.Server.DTOs
{
    public class EmployeeResponse
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; }
        public string AvailabilityNotes { get; set; }
        public int MaxWeeklyHours { get; set; }
    }
}
