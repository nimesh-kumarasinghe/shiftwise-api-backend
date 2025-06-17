namespace ShiftWiseAI.Server.Models
{
    public class ShiftAssignment
    {
        public Guid Id { get; set; }

        public Guid EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public Guid ShiftId { get; set; }
        public Shift Shift { get; set; }
    }
}
