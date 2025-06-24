using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using ShiftWiseAI.Server.Data;
using ShiftWiseAI.Server.DTOs;
using ShiftWiseAI.Server.Models;
using ShiftWiseAI.Server.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ShiftWiseAI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IConfiguration _config;
        private readonly ApplicationDbContext _db;
        private readonly EmailService _emailService;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IConfiguration config,
            ApplicationDbContext db,
            EmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _config = config;
            _db = db;
            _emailService = emailService; 
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest request)
        {
            // Create new organization
            var org = new Organization
            {
                Id = Guid.NewGuid(),
                Name = request.OrganizationName,
                CountryCode = request.CountryCode,
                TimeZone = request.TimeZone
            };

            await _db.Organizations.AddAsync(org);
            await _db.SaveChangesAsync();

            var user = new ApplicationUser
            {
                UserName = request.Email,
                Email = request.Email,
                OrganizationId = org.Id,
                Role = request.Role
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, request.Role);

            return Ok("User and organization created successfully");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest request)
        {
            var user = await _userManager.Users
                .Include(u => u.Organization)
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
                return Unauthorized("Invalid credentials");

            var valid = await _userManager.CheckPasswordAsync(user, request.Password);
            if (!valid)
                return Unauthorized("Invalid credentials");

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(ApplicationUser user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            //new Claim(JwtRegisteredClaimNames.Sub, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim("orgId", user.OrganizationId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        [HttpPost("request-reset")]
        public async Task<IActionResult> RequestPasswordReset([FromBody] EmailRequestDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return NotFound("User not found");

            var token = new Random().Next(100000, 999999).ToString();
            var expiry = DateTime.UtcNow.AddMinutes(10);

            user.PasswordResetToken = token;
            user.PasswordResetExpiry = expiry;
            await _userManager.UpdateAsync(user);

            var subject = "ShiftWise Password Reset Code";
            var body = $"Your 6-digit code is {token}. It expires in 10 minutes.";
            await _emailService.SendAsync(model.Email, subject, body);

            return Ok("Reset code sent to email");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.PasswordResetToken != model.Otp || user.PasswordResetExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");

            return Ok(new { valid = true });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null || user.PasswordResetToken != model.Otp || user.PasswordResetExpiry < DateTime.UtcNow)
                return BadRequest("Invalid or expired token");

            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, resetToken, model.NewPassword);

            if (!result.Succeeded)
                return BadRequest("Failed to reset password");

            user.PasswordResetToken = null;
            user.PasswordResetExpiry = null;
            await _userManager.UpdateAsync(user);

            return Ok("Password has been reset successfully");
        }
    }
}
