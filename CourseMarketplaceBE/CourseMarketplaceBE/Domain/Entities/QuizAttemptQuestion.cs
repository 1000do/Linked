using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Domain.Entities;

public class QuizAttemptQuestion
{
    [Key]
    public int Id { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    public int OrderIndex { get; set; }

    // Navigation properties
    public virtual QuizAttempt? Attempt { get; set; }
    
    public virtual QuizQuestion? Question { get; set; }
}
