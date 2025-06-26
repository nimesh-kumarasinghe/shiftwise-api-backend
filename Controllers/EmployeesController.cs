using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ShiftWiseAI.Server.Models;
using ShiftWiseAI.Server.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ShiftWiseAI.Server.DTOs;
using Microsoft.AspNetCore.Authorization;


namespace ShiftWiseAI.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin,Manager")]
    
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public EmployeesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Add an employee
        [HttpPost("bulk")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateEmployeesBulk([FromBody] List<CreateEmployeeRequest> requests)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var orgId = user.OrganizationId;

            // Get all existing emails in this org for validation
            var existingEmails = await _context.Employees
                .Where(e => e.OrganizationId == orgId)
                .Select(e => e.Email.ToLower())
                .ToListAsync();

            var newEmployees = new List<Employee>();
            var rejected = new List<string>();

            foreach (var req in requests)
            {
                if (string.IsNullOrWhiteSpace(req.Email)) continue;

                var email = req.Email.ToLower();
                if (existingEmails.Contains(email) || newEmployees.Any(e => e.Email.ToLower() == email))
                {
                    rejected.Add(email);
                    continue;
                }

                newEmployees.Add(new Employee
                {
                    FullName = req.FullName,
                    Email = req.Email,
                    Phone = req.Phone,
                    Role = req.Role,
                    AvailabilityNotes = req.AvailabilityNotes,
                    MaxWeeklyHours = req.MaxWeeklyHours,
                    OrganizationId = orgId
                });
            }

            if (newEmployees.Any())
            {
                _context.Employees.AddRange(newEmployees);
                await _context.SaveChangesAsync();
            }

            return Ok(new
            {
                message = $"Added {newEmployees.Count} employees.",
                rejectedEmails = rejected
            });
        }


        // Get all employees
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetMyEmployees()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _userManager.Users.Include(u => u.Organization)
                                               .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized();

            var employees = await _context.Employees
                .Where(e => e.OrganizationId == user.OrganizationId)
                .Select(e => new EmployeeResponse
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Email = e.Email,
                    Phone = e.Phone,
                    Role = e.Role,
                    AvailabilityNotes = e.AvailabilityNotes,
                    MaxWeeklyHours = e.MaxWeeklyHours,
                    IsActive = e.IsActive,
                })
                .ToListAsync();

            return Ok(employees);
        }

        // Update employee data
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateEmployee(Guid id, [FromBody] UpdateEmployeeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == user.OrganizationId);

            if (employee == null)
                return NotFound("Employee not found.");

            employee.FullName = request.FullName;
            employee.Email = request.Email;
            employee.Role = request.Role;
            employee.MaxWeeklyHours = request.MaxWeeklyHours;
            employee.Phone = request.Phone;
            employee.IsActive = request.IsActive;

            await _context.SaveChangesAsync();
            return Ok("Employee updated successfully.");
        }

        // Delete an employee
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteEmployee(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.Id == id && e.OrganizationId == user.OrganizationId);

            if (employee == null)
                return NotFound("Employee not found.");

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok("Employee deleted successfully.");
        }

    }
}
