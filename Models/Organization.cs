namespace ShiftWiseAI.Server.Models
{
    public class Organization
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CountryCode { get; set; }
        public string TimeZone { get; set; }
        
        public ICollection<ApplicationUser> Users { get; set; }
        public ICollection<Employee> Employees { get; set; }
    }
}
