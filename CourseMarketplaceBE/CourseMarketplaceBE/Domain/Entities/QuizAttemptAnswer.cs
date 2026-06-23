namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>Câu trả lời của học viên cho mỗi câu hỏi trong một lần làm quiz.</summary>
public partial class QuizAttemptAnswer
{
    public int AnswerId { get; set; }

    public int AttemptId { get; set; }

    public int QuestionId { get; set; }

    /// <summary>Null nếu học viên bỏ qua câu hỏi này.</summary>
    public int? SelectedOptionId { get; set; }

    // Navigation
    public virtual QuizAttempt? QuizAttempt { get; set; }

    public virtual QuizQuestion? QuizQuestion { get; set; }

    public virtual QuizOption? SelectedOption { get; set; }
}
