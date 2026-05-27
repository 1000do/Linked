namespace CourseMarketplaceBE.Domain.Entities;

public class Lockout
{
    public int LockoutId { get; set; }
    public int? AccountId { get; set; }
    public Account? Account { get; set; }
    public string? LockoutType { get; set; } // "account", "review", "instructor"
    public string? LockoutLevel { get; set; } // "moderate", "severe"
    public DateTime? LockoutStart { get; set; }
    public DateTime? LockoutEnd { get; set; }
}
