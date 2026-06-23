using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;

namespace CourseMarketplaceBE.Domain.IRepositories;

public interface IQuestionBankRepository
{
    Task<QuizQuestion> AddQuestionAsync(QuizQuestion question);
    Task<QuizQuestion> UpdateQuestionAsync(QuizQuestion question);
    Task<QuizQuestion?> GetQuestionByIdAsync(int questionId);
    Task DeleteQuestionAsync(int questionId);
    
    Task<List<QuizQuestion>> GetQuestionsByCourseAsync(int courseId);
    Task<List<QuizQuestion>> GetQuestionsByLessonAsync(int lessonId);
}
