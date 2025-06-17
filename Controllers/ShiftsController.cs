using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ShiftWiseAI.Server.Data;
using ShiftWiseAI.Server.DTOs;
using ShiftWiseAI.Server.Models;
using ShiftWiseAI.Server.Services;
using System.Security.Claims;

namespace ShiftWiseAI.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ShiftsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PdfService _pdfService;

        public ShiftsController(ApplicationDbContext context, PdfService pdfService)
        {
            _context = context;
            _pdfService = pdfService;
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

        // Export shift to PDF
        [HttpGet("export")]
        public async Task<IActionResult> ExportShiftsToPdf(DateTime from, DateTime to)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.Include(u => u.Organization).FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null || user.OrganizationId == Guid.Empty)
                return Unauthorized();

            var shifts = await _context.Shifts
                .Include(s => s.Assignments)
                    .ThenInclude(sa => sa.Employee)
                .Where(s => s.OrganizationId == user.OrganizationId && s.ShiftDate >= from && s.ShiftDate <= to && s.IsConfirmed == true)
                .ToListAsync();

            var organization = await _context.Organizations
                .FirstOrDefaultAsync(o => o.Id == user.OrganizationId);

            if (!shifts.Any())
                return NotFound("No shifts found in the selected range.");

            var html = await _pdfService.GenerateShiftHtmlAsync(shifts, organization.Name, from, to);
            var pdf = _pdfService.ConvertHtmlToPdf(html);

            return File(pdf, "application/pdf", $"Shifts_{from:yyyyMMdd}_to_{to:yyyyMMdd}.pdf");
        }

        // Export one sepecific shift as PDF
        [HttpGet("export/{shiftId}")]
        public async Task<IActionResult> ExportShiftById(Guid shiftId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return Unauthorized();

            var shift = await _context.Shifts
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Employee)
                .FirstOrDefaultAsync(s => s.Id == shiftId && s.OrganizationId == user.OrganizationId);

            if (shift == null)
                return NotFound("Shift not found.");

            if (!shift.Assignments.Any() || !shift.Assignments.All(a => a.ShiftId == shiftId))
                return BadRequest("Shift is not assigned properly.");

            if (!shift.Assignments.All(a => a.Shift.IsConfirmed))
                return BadRequest("Shift must be confirmed before export.");

            var orgName = await _context.Organizations
                .Where(o => o.Id == user.OrganizationId)
                .Select(o => o.Name)
                .FirstOrDefaultAsync();

            var html = await _pdfService.GenerateShiftHtmlAsync(new List<Shift> { shift }, orgName, shift.ShiftDate, shift.ShiftDate);
            var pdfBytes = _pdfService.ConvertHtmlToPdf(html);

            return File(pdfBytes, "application/pdf", $"Shift_{shift.ShiftDate:yyyyMMdd}.pdf");
        }



    }
}
