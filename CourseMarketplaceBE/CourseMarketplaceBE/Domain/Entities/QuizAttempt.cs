using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>Lịch sử mỗi lần học viên làm một bộ quiz.</summary>
public partial class QuizAttempt
{
    public int AttemptId { get; set; }

    public int QuizId { get; set; }

    public int UserId { get; set; }

    /// <summary>Điểm đạt được (0–100). Null nếu chưa nộp bài.</summary>
    public int? Score { get; set; }

    /// <summary>Null nếu chưa nộp bài.</summary>
    public bool? IsPassed { get; set; }

    public DateTime? StartedAt { get; set; }

    /// <summary>Null nếu học viên chưa nộp bài.</summary>
    public DateTime? SubmittedAt { get; set; }

    // Navigation
    public virtual Quiz? Quiz { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<QuizAttemptAnswer> QuizAttemptAnswers { get; set; } = new List<QuizAttemptAnswer>();
    
    public virtual ICollection<QuizAttemptQuestion> QuizAttemptQuestions { get; set; } = new List<QuizAttemptQuestion>();
}
