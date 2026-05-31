using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class InstructorCourseRevenueProjection
    {
        public int CourseId { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public int InstructorId { get; set; }
        public string InstructorName { get; set; } = string.Empty;
        public int SalesCount { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal PreviousMonthRevenue { get; set; }
        public decimal YearlyRevenue { get; set; }
        public decimal LifetimeRevenue { get; set; }
    }
}
