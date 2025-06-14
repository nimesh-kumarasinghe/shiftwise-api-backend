namespace ShiftWiseAI.Server.Models
{
    public class Holiday
    {
        public Guid Id { get; set; }

        // The organization this holiday belongs to
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        // Date of the holiday (e.g., 2025-01-01)
        public DateTime Date { get; set; }

        // Name of the holiday (e.g., "New Year's Day")
        public string Name { get; set; }
    }
}
