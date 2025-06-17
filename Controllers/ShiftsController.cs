using Microsoft.AspNetCore.Mvc;
using ShiftWiseAI.Server.Models;
using ShiftWiseAI.Server.Data;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ShiftWiseAI.Server.DTOs;
using Microsoft.AspNetCore.Authorization;

namespace ShiftWiseAI.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ShiftsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Create Shifts
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateShift([FromBody] CreateShiftRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var orgId = user.OrganizationId;

            int createdShifts = 0;
            int offset = 0;

            while (createdShifts < request.DaysCount)
            {
                var currentDate = request.ShiftDate.AddDays(offset);

                if (request.SkipWeekends &&
                    (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday))
                {
                    offset++;
                    continue;
                }

                var shift = new Shift
                {
                    OrganizationId = orgId,
                    ShiftDate = currentDate,
                    StartTime = request.StartTime,
                    EndTime = request.EndTime,
                    ShiftType = request.ShiftType
                };

                _context.Shifts.Add(shift);

                createdShifts++;
                offset++;
            }

            await _context.SaveChangesAsync();
            return Ok("Shifts created successfully.");
        }

        // Get all shifts for the organization
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetShifts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            var shifts = await _context.Shifts
                .Where(s => s.OrganizationId == user.OrganizationId)
                .OrderBy(s => s.ShiftDate)
                .ToListAsync();

            return Ok(shifts);
        }

        // Assign employees for the shift
        [HttpPost("{id}/assign")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> AssignEmployeesToShift(Guid id, [FromBody] AssignEmployeesRequest request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.OrganizationId == null)
                return Unauthorized("No organization found for user.");

            // Get the shift and verify it belongs to the user’s organization
            var shift = await _context.Shifts
                .Include(s => s.Assignments)
                .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == user.OrganizationId);

            if (shift == null)
                return NotFound("Shift not found.");

            // Get valid employees for this org
            var employees = await _context.Employees
                .Where(e => request.EmployeeIds.Contains(e.Id) && e.OrganizationId == user.OrganizationId)
                .ToListAsync();

            foreach (var emp in employees)
            {
                // Avoid duplicate assignments
                if (!shift.Assignments.Any(a => a.EmployeeId == emp.Id))
                {
                    var assignment = new ShiftAssignment
                    {
                        Id = Guid.NewGuid(),
                        EmployeeId = emp.Id,
                        ShiftId = shift.Id,
                    };

                    _context.ShiftAssignments.Add(assignment);
                }
            }

            await _context.SaveChangesAsync();
            return Ok("Employees assigned to shift.");
        }

        // Confirm shift
        [HttpPatch("{shiftId}/confirm")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ConfirmShift(Guid shiftId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.OrganizationId == Guid.Empty)
                return Unauthorized("Invalid user or organization.");

            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == shiftId && s.OrganizationId == user.OrganizationId);

            if (shift == null)
                return NotFound("Shift not found.");

            if (shift.IsConfirmed)
                return BadRequest("Shift is already confirmed.");

            shift.IsConfirmed = true;
            await _context.SaveChangesAsync();

            return Ok("Shift confirmed successfully.");
        }

        // Delete a shift if it is not confirmed and delete assigned employees
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteShift(Guid id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.OrganizationId == Guid.Empty)
                return Unauthorized("Invalid user.");

            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == user.OrganizationId);

            if (shift == null)
                return NotFound("Shift not found.");

            if (shift.IsConfirmed)
                return BadRequest("Confirmed shifts cannot be deleted.");

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return Ok("Shift deleted successfully.");
        }

        // Update Shift main details if it is not confirmed
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateShift(Guid id, [FromBody] ShiftUpdate updatedShift)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);

            if (user == null || user.OrganizationId == Guid.Empty)
                return Unauthorized("Invalid user.");

            var shift = await _context.Shifts
                .FirstOrDefaultAsync(s => s.Id == id && s.OrganizationId == user.OrganizationId);

            if (shift == null)
                return NotFound("Shift not found.");

            if (shift.IsConfirmed)
                return BadRequest("Confirmed shifts cannot be edited.");

            // Only update allowed properties
            shift.ShiftDate = updatedShift.ShiftDate;
            shift.StartTime = updatedShift.StartTime;
            shift.EndTime = updatedShift.EndTime;
            shift.ShiftType = updatedShift.ShiftType;

            await _context.SaveChangesAsync();

            return Ok("Shift updated successfully.");
        }

        //Remove assign employees from a shift
        [HttpDelete("{shiftId}/unassign/{employeeId}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UnassignEmployee(Guid shiftId, Guid employeeId)
        {
            var assignment = await _context.ShiftAssignments
                .FirstOrDefaultAsync(sa => sa.ShiftId == shiftId && sa.EmployeeId == employeeId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            _context.ShiftAssignments.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok("Employee unassigned from shift.");
        }

        // Get assigned employees for a shift
        [HttpGet("{id}/assignments")]
        [Authorize(Roles ="Admin,Manager")]
        public async Task<IActionResult> GetAssignedEmployees(Guid id)
        {
            var assignedEmployees = await _context.ShiftAssignments
                .Where(sa => sa.ShiftId == id)
                .Include(sa => sa.Employee)
                .Select(sa => new AssignedEmployee
                {
                    Id = sa.Employee.Id,
                    FullName = sa.Employee.FullName,
                    Email = sa.Employee.Email,
                    Phone = sa.Employee.Phone,
                    Role = sa.Employee.Role
                })
                .ToListAsync();

            return Ok(assignedEmployees);
        }


    }
}
