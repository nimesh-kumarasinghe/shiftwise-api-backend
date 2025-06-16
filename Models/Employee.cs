namespace ShiftWiseAI.Server.Models
{
    //public class Employee
    //{
    //    public Guid Id { get; set; }
    //    public Guid OrganizationId { get; set; }
    //    public string FullName { get; set; }
    //    public string Role { get; set; }
    //    public int MaxWeeklyHours { get; set; }
    //    public string PreferredShiftType { get; set; }
    //    public ICollection<Shift> AssignedShifts { get; set; }
    //}

    public class Employee
    {
        public int Id { get; set; }  // int or Guid is fine, but start simple
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Role { get; set; } // Cashier, Nurse, etc.
        public string AvailabilityNotes { get; set; }
        public int MaxWeeklyHours {  get; set; }

        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }
    }

}
