using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class QuizOption
{
    public int OptionId { get; set; }

    public int QuestionId { get; set; }

    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }

    public int OrderIndex { get; set; }

    // Navigation
    public virtual QuizQuestion? QuizQuestion { get; set; }

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
