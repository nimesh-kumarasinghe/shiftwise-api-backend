using Microsoft.AspNetCore.Identity;
namespace ShiftWiseAI.Server.Models
{
    public class ApplicationUser: IdentityUser
    {
        public Guid OrganizationId { get; set; }
        public Organization Organization { get; set; }

        public string Role { get; set; } // e.g., "Admin", "Manager", "Employee"
    }
}
