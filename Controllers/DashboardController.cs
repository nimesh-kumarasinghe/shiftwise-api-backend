using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ShiftWiseAI.Server.Data;
using ShiftWiseAI.Server.DTOs;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace ShiftWiseAI.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin,Manager")]
    public class DashboardController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DashboardController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetSummary()
        {
            // Get current user's orgId
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null || user.OrganizationId == Guid.Empty)
                return Unauthorized("Invalid user or organization.");

            var orgId = user.OrganizationId;
            var today = DateTime.Today;
            var weekFromNow = today.AddDays(7);

            var employees = await _context.Employees
                .Where(e => e.OrganizationId == orgId)
                .ToListAsync();

            var shifts = await _context.Shifts
                .Where(s => s.OrganizationId == orgId)
                .Include(s => s.Assignments)
                .ThenInclude(a => a.Employee)
                .ToListAsync();

            var upcomingShifts = shifts
                .Where(s => s.ShiftDate >= today && s.ShiftDate <= weekFromNow && s.IsConfirmed)
                .OrderBy(s => s.ShiftDate)
                .Take(10)
                .Select(s => new UpcomingShiftDto
                {
                    ShiftDate = s.ShiftDate,
                    ShiftType = s.ShiftType,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    AssignedEmployeeCount = s.Assignments.Count
                })
                .ToList();

            var weeklyLoad = shifts
                .Where(s => s.ShiftDate >= today && s.ShiftDate <= weekFromNow)
                .OrderBy(s => s.ShiftDate)
                .Select(s => new ShiftLoadDto
                {
                    Date = s.ShiftDate,
                    AssignedCount = s.Assignments.Count,
                    ShiftType = s.ShiftType,
                    StartTime = s.StartTime.ToString(@"hh\:mm"),
                    Confirmation = s.IsConfirmed

                })
                
                .ToList();


            var assignedThisWeek = shifts
                .Where(s => s.ShiftDate >= today && s.ShiftDate <= weekFromNow)
                .SelectMany(s => s.Assignments.Select(a => a.EmployeeId))
                .Distinct()
                .ToHashSet();

            var idleEmployees = employees
                .Where(e => !assignedThisWeek.Contains(e.Id))
                .Select(e => new IdleEmployeeDto
                {
                    Id = e.Id,
                    FullName = e.FullName,
                    Role = e.Role
                })
                .ToList();

            var summary = new DashboardSummaryDto
            {
                TotalEmployees = employees.Count,
                ActiveEmployees = employees.Count(e => e.IsActive),
                TotalShifts = shifts.Count,
                ConfirmedShifts = shifts.Count(s => s.IsConfirmed),
                UnconfirmedShifts = shifts.Count(s => !s.IsConfirmed),
                UpcomingShifts = upcomingShifts,
                WeeklyShiftLoad = weeklyLoad,
                IdleEmployees = idleEmployees
            };

            return Ok(summary);
        }
    }
}
