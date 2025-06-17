namespace ShiftWiseAI.Server.DTOs
{
    public class UpdateEmployeeRequest
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string Phone { get; set; }
        public int MaxWeeklyHours { get; set; }
        public bool IsActive { get; set; }

    }
}
