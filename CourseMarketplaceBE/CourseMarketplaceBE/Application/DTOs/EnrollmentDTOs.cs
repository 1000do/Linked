namespace CourseMarketplaceBE.Application.DTOs;

public class ProgressResponse
{
    public int EnrollmentId { get; set; }
    public int LearnedMaterialCount { get; set; }
    public int TotalMaterialCount { get; set; }
    public double ProgressPercentage => TotalMaterialCount > 0 
        ? (double)LearnedMaterialCount / TotalMaterialCount * 100 
        : 0;
}

public class UpdateProgressRequest
{
    public int CourseId { get; set; }
    public int MaterialId { get; set; } // For future proofing even if we just use count now
}
