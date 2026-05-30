using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class RefundEligibilityDto
    {
        public int AccountFlagCount { get; set; }
        public int PastRefundedCountForCourse { get; set; }
        public int RefundRequestsLast14DaysCount { get; set; }
        public double CourseTotalDurationHours { get; set; }
        public double StudentProgressPercentage { get; set; }
        public double CompletedVideoDurationHours { get; set; }
    }
}
