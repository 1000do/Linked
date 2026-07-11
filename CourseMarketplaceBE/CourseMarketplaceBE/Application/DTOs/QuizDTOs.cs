using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CourseMarketplaceBE.Domain.Enums;

namespace CourseMarketplaceBE.Application.DTOs;

// ==============================================================================
// REQUESTS
// ==============================================================================

public class QuestionBankLessonSummaryResponse
{
    public int LessonId { get; set; }
    public string Title { get; set; } = null!;
    public int AvailableQuestionCount { get; set; }
}

public class QuizCreateRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    /// <summary>Giới hạn thời gian (phút). Null = không giới hạn.</summary>
    public int? TimeLimitMinutes { get; set; }

    /// <summary>Điểm tối thiểu để pass (0–100). Mặc định 70.</summary>
    [Range(0, 100)]
    public int PassingScore { get; set; } = 70;

    [Required]
    public int CourseId { get; set; }

    [Range(1, 1000)]
    public int TotalQuestions { get; set; } = 30;

    public List<QuizLessonDistributionRequest> Distributions { get; set; } = new();
}

public class QuizUpdateRequest
{
    [Required]
    [MaxLength(255)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public int? TimeLimitMinutes { get; set; }

    [Range(0, 100)]
    public int PassingScore { get; set; } = 70;

    [Range(1, 1000)]
    public int TotalQuestions { get; set; } = 30;

    public List<QuizLessonDistributionRequest> Distributions { get; set; } = new();
}

public class QuizLessonDistributionRequest
{
    [Required]
    public int LessonId { get; set; }

    [Range(0, 1000)]
    public int QuestionCount { get; set; }
}

public class QuizAddQuestionRequest
{
    [Required]
    public int LessonId { get; set; }

    [Required]
    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    public QuizQuestionType QuestionType { get; set; } = QuizQuestionType.SingleChoice;

    [Required]
    [MinLength(2)]
    public List<QuizOptionRequest> Options { get; set; } = new();
}

public class QuizOptionRequest
{
    [Required]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }
}

public class QuizUpdateQuestionRequest
{
    [Required]
    public int LessonId { get; set; }

    [Required]
    public string QuestionText { get; set; } = null!;

    public string? Explanation { get; set; }

    public QuizQuestionType QuestionType { get; set; } = QuizQuestionType.SingleChoice;

    [Required]
    [MinLength(2)]
    public List<QuizUpdateOptionRequest> Options { get; set; } = new();
}

public class QuizUpdateOptionRequest
{
    public int? OptionId { get; set; }

    [Required]
    public string OptionText { get; set; } = null!;

    public bool IsCorrect { get; set; }
}

public class AddQuizToCourseRequest
{
    [Required]
    public int CourseId { get; set; }

    [Required]
    public int QuizId { get; set; }

    public int OrderIndex { get; set; } = 0;
}

public class QuizAttemptSubmitRequest
{
    [Required]
    public int AttemptId { get; set; }

    public List<AttemptAnswerRequest> Answers { get; set; } = new();
}

public class AttemptAnswerRequest
{
    public int QuestionId { get; set; }

    /// <summary>Null nếu bỏ qua câu hỏi.</summary>
    public int? SelectedOptionId { get; set; }
}

// ==============================================================================
// RESPONSES
// ==============================================================================

public class QuizSummaryResponse
{
    public int QuizId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool IsHidden { get; set; }
    public bool IsRemoved { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class QuizDetailResponse : QuizSummaryResponse
{
    public List<QuizQuestionResponse> Questions { get; set; } = new();
    public List<QuizLessonDistributionResponse> Distributions { get; set; } = new();
}

public class QuizLessonDistributionResponse
{
    public int LessonId { get; set; }
    public int QuestionCount { get; set; }
}

public class QuizQuestionResponse
{
    public int QuestionId { get; set; }
    public int CourseId { get; set; }
    public int? LessonId { get; set; }
    public string QuestionText { get; set; } = null!;
    public string? Explanation { get; set; }
    public QuizQuestionType QuestionType { get; set; }
    public List<QuizOptionResponse> Options { get; set; } = new();
}

public class QuizOptionResponse
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

public class CourseQuizResponse
{
    public int CourseQuizId { get; set; }
    public int CourseId { get; set; }
    public int QuizId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public bool IsHidden { get; set; }
    public int OrderIndex { get; set; }
    public DateTime? AddedAt { get; set; }
}

public class QuizAttemptResultResponse
{
    public int AttemptId { get; set; }
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
}

/// <summary>Dùng cho student xem câu hỏi — không trả về IsCorrect.</summary>
public class QuizForStudentResponse
{
    public int QuizId { get; set; }
    public int AttemptId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public int PassingScore { get; set; }
    public List<QuizQuestionForStudentResponse> Questions { get; set; } = new();
}

public class QuizQuestionForStudentResponse
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public QuizQuestionType QuestionType { get; set; }
    public List<QuizOptionForStudentResponse> Options { get; set; } = new();
}

public class QuizOptionForStudentResponse
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = null!;
    public int OrderIndex { get; set; }
    // IsCorrect KHÔNG được trả về cho student
}

public class QuizAttemptDetailResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = null!;
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public int TotalQuestions { get; set; }
    public int CorrectCount { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public List<QuizAttemptQuestionDetailResponse> Questions { get; set; } = new();
}

public class QuizAttemptQuestionDetailResponse
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = null!;
    public QuizQuestionType QuestionType { get; set; }
    public string? Explanation { get; set; }
    
    public int? SelectedOptionId { get; set; }
    public bool IsCorrect { get; set; }

    public List<QuizOptionDetailResponse> Options { get; set; } = new();
}

public class QuizOptionDetailResponse
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public int OrderIndex { get; set; }
}

public class QuizAttemptSummaryResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = null!;
    public int UserId { get; set; }
    public string UserFullName { get; set; } = null!;
    public int Score { get; set; }
    public bool IsPassed { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
