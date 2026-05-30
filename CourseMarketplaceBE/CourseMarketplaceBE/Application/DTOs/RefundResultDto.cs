using System;

namespace CourseMarketplaceBE.Application.DTOs
{
    public class RefundResultDto
    {
        public bool IsAutoRejected { get; set; }
        public string? RejectReason { get; set; }
    }
}
