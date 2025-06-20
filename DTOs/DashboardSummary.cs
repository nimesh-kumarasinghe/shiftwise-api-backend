namespace ShiftWiseAI.Server.DTOs
{
    public class DashboardSummaryDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }

        public int TotalShifts { get; set; }
        public int ConfirmedShifts { get; set; }
        public int UnconfirmedShifts { get; set; }

        public List<UpcomingShiftDto> UpcomingShifts { get; set; }
        public List<ShiftLoadDto> WeeklyShiftLoad { get; set; }
        public List<IdleEmployeeDto> IdleEmployees { get; set; }
    }

    public class IdleEmployeeDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
    }

    public class UpcomingShiftDto
    {
        public DateTime ShiftDate { get; set; }
        public string ShiftType { get; set; }
        public string StartTime { get; set; }
        public int AssignedEmployeeCount { get; set; }
    }

    public class ShiftLoadDto
    {
        public DateTime Date { get; set; }
        public int ShiftCount { get; set; }
    }
}
