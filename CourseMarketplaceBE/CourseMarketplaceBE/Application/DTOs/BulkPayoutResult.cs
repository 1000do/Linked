using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Application.DTOs;

public class BulkPayoutResult
{
    public int TotalProcessed { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new List<string>();
}
