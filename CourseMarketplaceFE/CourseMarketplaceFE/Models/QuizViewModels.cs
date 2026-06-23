using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceFE.Models
{
    public class QuizListViewModel
    {
        public List<QuizSummaryItem> Quizzes { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? FilterStatus { get; set; }
        public string? SearchTerm { get; set; }
        public List<CourseListViewModel> AvailableCourses { get; set; } = new();
    }

    public class QuizSummaryItem
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int QuestionCount { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int PassingScore { get; set; }
        public string CourseTitle { get; set; } = string.Empty;
        public bool IsHidden { get; set; }
        public bool IsRemoved { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class QuizCreateViewModel
    {
        [Required(ErrorMessage = "Quiz title is required")]
        [MaxLength(255, ErrorMessage = "Title cannot exceed 255 characters")]
        public string Title { get; set; } = null!;

        public string? Description { get; set; }

        [Range(1, 1000, ErrorMessage = "Time limit must be valid if provided")]
        public int? TimeLimitMinutes { get; set; }

        [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100")]
        public int PassingScore { get; set; } = 70;

        [Required(ErrorMessage = "Course is required")]
        public int CourseId { get; set; }

        [Range(1, 1000)]
        public int TotalQuestions { get; set; } = 30;

        public List<QuizLessonDistributionItem> Distributions { get; set; } = new();
    }

    public class QuizUpdateViewModel : QuizCreateViewModel
    {
        [Range(1, 1000)]
        public int TotalQuestions { get; set; } = 30;
        public List<QuizLessonDistributionItem> Distributions { get; set; } = new();
    }

    public class QuizEditorViewModel
    {
        public int QuizId { get; set; }
        public string Title { get; set; } = null!;
        public string? Description { get; set; }
        public int? TimeLimitMinutes { get; set; }
        public int PassingScore { get; set; }
        public int TotalQuestions { get; set; }
        public bool IsHidden { get; set; }
        public int CourseId { get; set; }
        public List<QuizLessonDistributionItem> Distributions { get; set; } = new();
        public List<CourseListViewModel> AvailableCourses { get; set; } = new();
    }

    public class QuizLessonDistributionItem
    {
        public int LessonId { get; set; }
        public int QuestionCount { get; set; }
    }

    public class QuizQuestionItem
    {
        public int QuestionId { get; set; }
        public string QuestionText { get; set; } = null!;
        public int QuestionType { get; set; } // enum map
        public int OrderIndex { get; set; }
        public List<OptionItem> Options { get; set; } = new();
    }

    public class OptionItem
    {
        public int OptionId { get; set; }
        public string OptionText { get; set; } = null!;
        public bool IsCorrect { get; set; }
        public int OrderIndex { get; set; }
    }
}
