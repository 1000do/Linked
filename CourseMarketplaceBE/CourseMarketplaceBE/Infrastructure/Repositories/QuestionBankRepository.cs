using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class QuestionBankRepository : IQuestionBankRepository
{
    private readonly AppDbContext _context;

    public QuestionBankRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<QuizQuestion> AddQuestionAsync(QuizQuestion question)
    {
        _context.QuizQuestions.Add(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<QuizQuestion> UpdateQuestionAsync(QuizQuestion question)
    {
        _context.QuizQuestions.Update(question);
        await _context.SaveChangesAsync();
        return question;
    }

    public async Task<QuizQuestion?> GetQuestionByIdAsync(int questionId)
    {
        return await _context.QuizQuestions
            .Include(q => q.QuizOptions.OrderBy(o => o.OrderIndex))
            .Include(q => q.Course)
            .Include(q => q.Lesson)
            .FirstOrDefaultAsync(q => q.QuestionId == questionId);
    }

    public async Task DeleteQuestionAsync(int questionId)
    {
        var question = await _context.QuizQuestions.FindAsync(questionId);
        if (question != null)
        {
            _context.QuizQuestions.Remove(question);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<QuizQuestion>> GetQuestionsByCourseAsync(int courseId)
    {
        return await _context.QuizQuestions
            .Include(q => q.QuizOptions.OrderBy(o => o.OrderIndex))
            .Where(q => q.CourseId == courseId)
            .ToListAsync();
    }

    public async Task<List<QuizQuestion>> GetQuestionsByLessonAsync(int lessonId)
    {
        return await _context.QuizQuestions
            .Include(q => q.QuizOptions.OrderBy(o => o.OrderIndex))
            .Where(q => q.LessonId == lessonId)
            .ToListAsync();
    }
}
