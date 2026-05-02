namespace CourseMarketplaceBE.Application.DTOs;

public class PlatformStatsDto
{
    public int TotalStudents { get; set; }
    public int TotalCourses { get; set; }
    public int TotalInstructors { get; set; }
    public decimal SatisfactionRate { get; set; }
}
