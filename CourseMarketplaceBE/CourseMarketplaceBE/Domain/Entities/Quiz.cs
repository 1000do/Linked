using System;
using System.Collections.Generic;

namespace CourseMarketplaceBE.Domain.Entities;

public partial class Quiz
{
    public int QuizId { get; set; }

    public int InstructorId { get; set; }

    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Giới hạn thời gian làm quiz (phút). Null = không giới hạn.</summary>
    public int? TimeLimitMinutes { get; set; }

    /// <summary>Điểm tối thiểu để pass (0–100). Mặc định 70.</summary>
    public int PassingScore { get; set; } = 70;

    /// <summary>Tổng số câu hỏi sẽ được sinh ra cho Quiz này.</summary>
    public int TotalQuestions { get; set; } = 10;

    /// <summary>Course chứa Quiz này.</summary>
    public int CourseId { get; set; }

    /// <summary>Ẩn quiz toàn cục — không hiện trong bất kỳ course nào.</summary>
    public bool IsHidden { get; set; }

    /// <summary>Xóa mềm.</summary>
    public bool IsRemoved { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public virtual Instructor? Instructor { get; set; }
    
    public virtual Course? Course { get; set; }

    public virtual ICollection<QuizLessonDistribution> QuizLessonDistributions { get; set; } = new List<QuizLessonDistribution>();

    public virtual ICollection<CourseQuiz> CourseQuizzes { get; set; } = new List<CourseQuiz>();

    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
