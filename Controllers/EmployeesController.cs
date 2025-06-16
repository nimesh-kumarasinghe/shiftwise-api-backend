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

        [HttpPost]
        public async Task<IActionResult> CreateEmployee(CreateEmployeeRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var employee = new Employee
            {
                FullName = request.FullName,
                Email = request.Email,
                Phone = request.Phone,
                Role = request.Role,
                AvailabilityNotes = request.AvailabilityNotes,
                OrganizationId = user.OrganizationId
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee added successfully." });
        }

        [HttpGet]
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
                    AvailabilityNotes = e.AvailabilityNotes
                })
                .ToListAsync();

            return Ok(employees);
        }
    }
}
