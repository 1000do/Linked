using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IQuizRepository
{
    // ── Quiz CRUD ─────────────────────────────────────────────────────────────
    Task<List<Quiz>> GetByInstructorAsync(int instructorId);
    Task<Quiz?> GetByIdAsync(int quizId);
    Task<Quiz?> GetByIdWithQuestionsAsync(int quizId);
    Task<Quiz> CreateAsync(Quiz quiz);
    Task UpdateAsync(Quiz quiz);
    Task SoftDeleteAsync(int quizId);
    Task SetHiddenAsync(int quizId, bool isHidden);
    Task<bool> HasActiveAttemptsAsync(int quizId);
    Task<bool> IsQuizInEnrolledCourseAsync(int quizId);


    // ── Quiz ↔ Course ─────────────────────────────────────────────────────────
    Task<List<CourseQuiz>> GetCourseQuizzesAsync(int courseId);
    Task<CourseQuiz?> GetCourseQuizAsync(int courseId, int quizId);
    Task<CourseQuiz> AddToCourseAsync(CourseQuiz courseQuiz);
    Task RemoveFromCourseAsync(int courseId, int quizId);
    Task SetCourseQuizHiddenAsync(int courseId, int quizId, bool isHidden);

    // ── Attempt ───────────────────────────────────────────────────────────────
    Task<QuizAttempt> SaveAttemptAsync(QuizAttempt attempt);
    Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId);
}
