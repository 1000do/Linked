using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;

namespace CourseMarketplaceBE.Application.IServices;

public interface IQuestionBankService
{
    Task<QuizQuestionResponse> AddQuestionAsync(int courseId, QuizAddQuestionRequest request, int instructorId);
    Task<QuizQuestionResponse> UpdateQuestionAsync(int questionId, QuizUpdateQuestionRequest request, int instructorId);
    Task DeleteQuestionAsync(int questionId, int instructorId);
    Task<List<QuizQuestionResponse>> GetQuestionsByCourseAsync(int courseId, int instructorId);
    Task<List<QuizQuestionResponse>> GetQuestionsByLessonAsync(int lessonId, int instructorId);
    Task<List<QuestionBankLessonSummaryResponse>> GetLessonsSummaryByCourseAsync(int courseId, int instructorId);
}
