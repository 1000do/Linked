using System.ComponentModel.DataAnnotations;

namespace CourseMarketplaceBE.Domain.Entities;

public class QuizLessonDistribution
{
    [Key]
    public int Id { get; set; }

    public int QuizId { get; set; }

    public int LessonId { get; set; }

    /// <summary>
    /// Phần trăm số lượng câu hỏi từ Lesson này trong tổng số câu hỏi của Quiz.
    /// </summary>
    public int QuestionCount { get; set; }

    // Navigation properties
    public virtual Quiz? Quiz { get; set; }
    
    public virtual Lesson? Lesson { get; set; }
}
