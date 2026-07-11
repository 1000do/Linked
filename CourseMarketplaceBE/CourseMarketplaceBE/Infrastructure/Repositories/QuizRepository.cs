using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CourseMarketplaceBE.Infrastructure.Repositories;

public class QuizRepository : IQuizRepository
{
    private readonly AppDbContext _context;

    public QuizRepository(AppDbContext context)
    {
        _context = context;
    }

    // ── Quiz CRUD ─────────────────────────────────────────────────────────────

    public async Task<List<Quiz>> GetByInstructorAsync(int instructorId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Where(q => q.InstructorId == instructorId && !q.IsRemoved)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }


    public async Task<Quiz?> GetByIdAsync(int quizId)
    {
        return await _context.Quizzes
            .FirstOrDefaultAsync(q => q.QuizId == quizId);
    }

    public async Task<Quiz?> GetByIdWithQuestionsAsync(int quizId)
    {
        return await _context.Quizzes
            .Include(q => q.Course)
            .Include(q => q.QuizLessonDistributions)
            .FirstOrDefaultAsync(q => q.QuizId == quizId);
    }

    public async Task<Quiz> CreateAsync(Quiz quiz)
    {
        quiz.CreatedAt = DateTime.UtcNow;
        quiz.UpdatedAt = DateTime.UtcNow;
        await _context.Quizzes.AddAsync(quiz);
        await _context.SaveChangesAsync();
        return quiz;
    }

    public async Task UpdateAsync(Quiz quiz)
    {
        quiz.UpdatedAt = DateTime.UtcNow;
        _context.Quizzes.Update(quiz);
        await _context.SaveChangesAsync();
    }

    public async Task SoftDeleteAsync(int quizId)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz is null) return;
        quiz.IsRemoved = true;
        quiz.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task SetHiddenAsync(int quizId, bool isHidden)
    {
        var quiz = await _context.Quizzes.FindAsync(quizId);
        if (quiz is null) return;
        quiz.IsHidden = isHidden;
        quiz.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }



    public async Task<bool> HasActiveAttemptsAsync(int quizId)
    {
        return await _context.QuizAttempts
            .AnyAsync(a => a.QuizId == quizId && a.SubmittedAt == null);
    }

    public async Task<bool> IsTitleUniqueAsync(string title, int instructorId, int? excludeQuizId = null)
    {
        var query = _context.Quizzes
            .Where(q => q.InstructorId == instructorId && q.Title.ToLower() == title.ToLower() && !q.IsRemoved);
            
        if (excludeQuizId.HasValue)
        {
            query = query.Where(q => q.QuizId != excludeQuizId.Value);
        }
        
        return !await query.AnyAsync();
    }

    public async Task<bool> IsQuizInEnrolledCourseAsync(int quizId)
    {
        // Kiểm tra xem quiz này có đang nằm trong bất kỳ Course nào đã có học viên Enrollment không
        var courseIdsWithQuiz = await _context.CourseQuizzes
            .Where(cq => cq.QuizId == quizId)
            .Select(cq => cq.CourseId)
            .ToListAsync();

        if (!courseIdsWithQuiz.Any()) return false;

        return await _context.Enrollments
            .AnyAsync(e => e.CourseId.HasValue && courseIdsWithQuiz.Contains(e.CourseId.Value));
    }

    // ── Quiz ↔ Course ─────────────────────────────────────────────────────────

    public async Task<List<CourseQuiz>> GetCourseQuizzesAsync(int courseId)
    {
        return await _context.CourseQuizzes
            .Include(cq => cq.Quiz)
            .Where(cq => cq.CourseId == courseId)
            .OrderBy(cq => cq.OrderIndex)
            .ToListAsync();
    }

    public async Task<CourseQuiz?> GetCourseQuizAsync(int courseId, int quizId)
    {
        return await _context.CourseQuizzes
            .FirstOrDefaultAsync(cq => cq.CourseId == courseId && cq.QuizId == quizId);
    }

    public async Task<CourseQuiz> AddToCourseAsync(CourseQuiz courseQuiz)
    {
        courseQuiz.AddedAt = DateTime.UtcNow;
        await _context.CourseQuizzes.AddAsync(courseQuiz);
        await _context.SaveChangesAsync();
        return courseQuiz;
    }

    public async Task RemoveFromCourseAsync(int courseId, int quizId)
    {
        var cq = await _context.CourseQuizzes
            .FirstOrDefaultAsync(cq => cq.CourseId == courseId && cq.QuizId == quizId);
        if (cq is null) return;
        _context.CourseQuizzes.Remove(cq);
        await _context.SaveChangesAsync();
    }

    public async Task SetCourseQuizHiddenAsync(int courseId, int quizId, bool isHidden)
    {
        var cq = await _context.CourseQuizzes
            .FirstOrDefaultAsync(cq => cq.CourseId == courseId && cq.QuizId == quizId);
        if (cq is null) return;
        cq.IsHidden = isHidden;
        await _context.SaveChangesAsync();
    }

    // ── Attempt ───────────────────────────────────────────────────────────────

    public async Task<QuizAttempt> SaveAttemptAsync(QuizAttempt attempt)
    {
        if (attempt.AttemptId == 0)
        {
            attempt.StartedAt = DateTime.UtcNow;
            await _context.QuizAttempts.AddAsync(attempt);
        }
        else
        {
            _context.QuizAttempts.Update(attempt);
        }
        await _context.SaveChangesAsync();
        return attempt;
    }

    public async Task<QuizAttempt?> GetAttemptByIdAsync(int attemptId)
    {
        return await _context.QuizAttempts
            .Include(a => a.QuizAttemptAnswers)
            .Include(a => a.QuizAttemptQuestions)
                .ThenInclude(aq => aq.Question)
                    .ThenInclude(q => q!.QuizOptions)
            .FirstOrDefaultAsync(a => a.AttemptId == attemptId);
    }
    public async Task<(List<QuizAttempt> Items, int TotalCount)> GetAttemptsByQuizAndUserAsync(int quizId, int userId, int page, int pageSize)
    {
        var query = _context.QuizAttempts
            .Include(a => a.User)
            .Where(a => a.QuizId == quizId && a.UserId == userId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, total);
    }

    public async Task<(List<QuizAttempt> Items, int TotalCount)> GetAttemptsByQuizAsync(int quizId, int page, int pageSize)
    {
        var query = _context.QuizAttempts
            .Include(a => a.User)
            .Where(a => a.QuizId == quizId);

        var total = await query.CountAsync();
        var items = await query
            .OrderByDescending(a => a.StartedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
            
        return (items, total);
    }
}
