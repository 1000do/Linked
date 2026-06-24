using System;

namespace CourseMarketplaceBE.Domain.Entities;

/// <summary>Bảng nối: Quiz ↔ Course. Một quiz có thể được thêm vào nhiều course.</summary>
public partial class CourseQuiz
{
    public int CourseQuizId { get; set; }

    public int CourseId { get; set; }

    public int QuizId { get; set; }

    public int OrderIndex { get; set; }

    /// <summary>Ẩn quiz trong course này — không ảnh hưởng tới quiz gốc hay các course khác.</summary>
    public bool IsHidden { get; set; }

    public DateTime? AddedAt { get; set; }

    // Navigation
    public virtual Course? Course { get; set; }

    public virtual Quiz? Quiz { get; set; }
}
