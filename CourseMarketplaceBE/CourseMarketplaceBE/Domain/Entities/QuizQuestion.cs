using System;
using System.Collections.Generic;
using CourseMarketplaceBE.Domain.Enums;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class QuizQuestion
{
    public int QuestionId { get; set; }

    public int CourseId { get; set; }
    
    public int LessonId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    /// <summary>Loại câu hỏi: SingleChoice | MultiChoice | TrueFalse</summary>
    public QuizQuestionType QuestionType { get; set; } = QuizQuestionType.SingleChoice;

    public DateTime? CreatedAt { get; set; }

    // Navigation
    public virtual Course? Course { get; set; }
    
    public virtual Lesson? Lesson { get; set; }

    public virtual ICollection<QuizOption> QuizOptions { get; set; } = new List<QuizOption>();

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
}
