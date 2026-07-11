using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Application.Services;

public class QuestionBankService : IQuestionBankService
{
    private readonly IQuestionBankRepository _questionBankRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly ILessonRepository _lessonRepository;

    public QuestionBankService(
        IQuestionBankRepository questionBankRepository,
        ICourseRepository courseRepository,
        ILessonRepository lessonRepository)
    {
        _questionBankRepository = questionBankRepository;
        _courseRepository = courseRepository;
        _lessonRepository = lessonRepository;
    }

    public async Task<QuizQuestionResponse> AddQuestionAsync(int courseId, QuizAddQuestionRequest request, int instructorId)
    {
        ValidateQuizOptions(request.Options);
        await EnsureInstructorOwnsCourseAsync(courseId, instructorId, "You do not have permission to add questions to this course.");

        var question = new QuizQuestion
        {
            CourseId = courseId,
            LessonId = request.LessonId,
            QuestionText = request.QuestionText,
            Explanation = request.Explanation,
            QuestionType = request.QuestionType,
            CreatedAt = DateTime.UtcNow,
            QuizOptions = request.Options.Select((o, i) => new QuizOption
            {
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = i
            }).ToList()
        };

        var created = await _questionBankRepository.AddQuestionAsync(question);
        return MapToResponse(created);
    }

    public async Task<QuizQuestionResponse> UpdateQuestionAsync(int questionId, QuizUpdateQuestionRequest request, int instructorId)
    {
        ValidateQuizOptions(request.Options);

        var question = await _questionBankRepository.GetQuestionByIdAsync(questionId);
        if (question == null)
            throw new KeyNotFoundException("Question not found.");

        await EnsureInstructorOwnsCourseAsync(question.CourseId, instructorId, "You do not have permission to edit this question.");

        question.LessonId = request.LessonId;
        question.QuestionText = request.QuestionText;
        question.Explanation = request.Explanation;
        question.QuestionType = request.QuestionType;
        
        question.QuizOptions.Clear();
        var idx = 0;
        foreach (var optReq in request.Options)
        {
            question.QuizOptions.Add(new QuizOption
            {
                OptionText = optReq.OptionText,
                IsCorrect = optReq.IsCorrect,
                OrderIndex = idx++
            });
        }

        var updated = await _questionBankRepository.UpdateQuestionAsync(question);
        return MapToResponse(updated);
    }

    public async Task DeleteQuestionAsync(int questionId, int instructorId)
    {
        var question = await _questionBankRepository.GetQuestionByIdAsync(questionId);
        if (question == null) return;

        await EnsureInstructorOwnsCourseAsync(question.CourseId, instructorId, "You do not have permission to delete this question.");

        await _questionBankRepository.DeleteQuestionAsync(questionId);
    }

    public async Task<List<QuizQuestionResponse>> GetQuestionsByCourseAsync(int courseId, int instructorId)
    {
        await EnsureInstructorOwnsCourseAsync(courseId, instructorId, "You do not have permission to access the question bank for this course.");

        var questions = await _questionBankRepository.GetQuestionsByCourseAsync(courseId);
        return questions.Select(MapToResponse).ToList();
    }

    public async Task<List<QuizQuestionResponse>> GetQuestionsByLessonAsync(int lessonId, int instructorId)
    {
        var questions = await _questionBankRepository.GetQuestionsByLessonAsync(lessonId);
        // Kiểm tra quyền
        if (questions.Any())
        {
            var courseId = questions.First().CourseId;
            await EnsureInstructorOwnsCourseAsync(courseId, instructorId, "You do not have permission to view these questions.");
        }
        return questions.Select(MapToResponse).ToList();
    }

    public async Task<List<QuestionBankLessonSummaryResponse>> GetLessonsSummaryByCourseAsync(int courseId, int instructorId)
    {
        await EnsureInstructorOwnsCourseAsync(courseId, instructorId, "You do not have permission to view this course's lessons.");

        var lessons = await _lessonRepository.GetByCourseIdAsync(courseId);
        var result = new List<QuestionBankLessonSummaryResponse>();

        foreach (var lesson in lessons.Where(l => !l.IsRemoved))
        {
            var questions = await _questionBankRepository.GetQuestionsByLessonAsync(lesson.LessonId);
            result.Add(new QuestionBankLessonSummaryResponse
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                AvailableQuestionCount = questions.Count
            });
        }

        return result;
    }

    private void ValidateQuizOptions(List<QuizOptionRequest> options)
    {
        if (options == null || options.Count < 2)
            throw new ArgumentException("A question must have at least 2 options.");
            
        if (!options.Any(o => o.IsCorrect))
            throw new ArgumentException("A question must have at least 1 correct option.");
    }

    private void ValidateQuizOptions(List<QuizUpdateOptionRequest> options)
    {
        if (options == null || options.Count < 2)
            throw new ArgumentException("A question must have at least 2 options.");
            
        if (!options.Any(o => o.IsCorrect))
            throw new ArgumentException("A question must have at least 1 correct option.");
    }

    private async Task EnsureInstructorOwnsCourseAsync(int courseId, int instructorId, string errorMessage)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course == null || course.InstructorId != instructorId)
            throw new UnauthorizedAccessException(errorMessage);
    }

    private static QuizQuestionResponse MapToResponse(QuizQuestion q)
    {
        return new QuizQuestionResponse
        {
            QuestionId = q.QuestionId,
            CourseId = q.CourseId,
            LessonId = q.LessonId,
            QuestionText = q.QuestionText,
            Explanation = q.Explanation,
            QuestionType = q.QuestionType,
            Options = q.QuizOptions.OrderBy(o => o.OrderIndex).Select(o => new QuizOptionResponse
            {
                OptionId = o.OptionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
        };
    }
}
