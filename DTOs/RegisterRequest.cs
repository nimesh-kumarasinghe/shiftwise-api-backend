namespace ShiftWiseAI.Server.DTOs
{
    public class RegisterRequest
    {
        public string OrganizationName { get; set; }
        public string CountryCode { get; set; }
        public string TimeZone { get; set; }

        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } = "Admin";
    }
}
