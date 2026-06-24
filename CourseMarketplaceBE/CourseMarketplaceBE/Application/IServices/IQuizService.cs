using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IQuizService
{
    // ── Instructor: Quản lý Quiz ──────────────────────────────────────────────
    Task<QuizDetailResponse> CreateQuizAsync(QuizCreateRequest request, int instructorId);
    Task<QuizDetailResponse> UpdateQuizSettingsAsync(int quizId, QuizUpdateRequest request, int instructorId);
    Task<List<QuizSummaryResponse>> GetMyQuizzesAsync(int instructorId);
    Task<QuizDetailResponse> GetQuizDetailAsync(int quizId, int instructorId);
    Task<List<QuizQuestionResponse>> GetQuizQuestionPoolAsync(int quizId, int instructorId);
    Task SoftDeleteQuizAsync(int quizId, int instructorId);
    Task SetQuizHiddenAsync(int quizId, bool isHidden, int instructorId);

    // ── Instructor: Gán Quiz vào Course ───────────────────────────────────────
    Task<CourseQuizResponse> AddQuizToCourseAsync(AddQuizToCourseRequest request, int instructorId);
    Task RemoveQuizFromCourseAsync(int courseId, int quizId, int instructorId);
    Task SetCourseQuizHiddenAsync(int courseId, int quizId, bool isHidden, int instructorId);
    Task<List<CourseQuizResponse>> GetCourseQuizzesAsync(int courseId, int instructorId);

    // ── Student: Làm Quiz ─────────────────────────────────────────────────────
    Task<QuizForStudentResponse> GetQuizForStudentAsync(int quizId, int userId);
    Task<QuizAttemptResultResponse> SubmitAttemptAsync(QuizAttemptSubmitRequest request, int userId);
    Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId, int userId);
}
