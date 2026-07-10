using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Enums;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;

namespace CourseMarketplaceBE.Application.Services;

public class QuizService : IQuizService
{
    private readonly IQuizRepository _quizRepo;
    private readonly IEnrollmentRepository _enrollmentRepo;
    private readonly IQuestionBankRepository _questionBankRepository;
    private readonly ICourseRepository _courseRepository;
    private readonly IRedisService _redisService;

    public QuizService(IQuizRepository quizRepo, IEnrollmentRepository enrollmentRepo, IQuestionBankRepository questionBankRepository, ICourseRepository courseRepository, IRedisService redisService)
    {
        _quizRepo = quizRepo;
        _enrollmentRepo = enrollmentRepo;
        _questionBankRepository = questionBankRepository;
        _courseRepository = courseRepository;
        _redisService = redisService;
    }

    // ── Instructor: Quản lý Quiz ──────────────────────────────────────────────

    public async Task<QuizDetailResponse> CreateQuizAsync(QuizCreateRequest request, int instructorId)
    {
        if (!await _quizRepo.IsTitleUniqueAsync(request.Title, instructorId))
            throw new ArgumentException("You already have a quiz with this title. Please choose a unique title.");
            
        if (request.TotalQuestions <= 0)
            throw new ArgumentException("Total questions must be greater than 0.");
            
        if (request.Distributions != null && request.Distributions.Any())
        {
            if (request.Distributions.Sum(d => d.QuestionCount) != request.TotalQuestions)
                throw new ArgumentException("Total questions distributed across lessons must equal the quiz's Total Questions.");

            foreach (var dist in request.Distributions)
            {
                var questionsInBank = await _questionBankRepository.GetQuestionsByLessonAsync(dist.LessonId);
                if (questionsInBank.Count < dist.QuestionCount)
                {
                    // get lesson name? We might not have lesson repo here easily, just throw error with ID.
                    throw new ArgumentException($"Lesson ID {dist.LessonId} only has {questionsInBank.Count} questions in Question Bank, but you requested {dist.QuestionCount} questions. Please reduce the number or add more questions to the lesson.");
                }
            }
        }

        var quiz = new Quiz
        {
            InstructorId = instructorId,
            Title = request.Title,
            Description = request.Description,
            TimeLimitMinutes = request.TimeLimitMinutes,
            PassingScore = request.PassingScore,
            CourseId = request.CourseId,
            TotalQuestions = request.TotalQuestions,
            QuizLessonDistributions = request.Distributions.Select(d => new QuizLessonDistribution
            {
                LessonId = d.LessonId,
                QuestionCount = d.QuestionCount
            }).ToList()
        };

        var created = await _quizRepo.CreateAsync(quiz);
        return MapToDetail(created);
    }

    public async Task<QuizDetailResponse> UpdateQuizSettingsAsync(int quizId, QuizUpdateRequest request, int instructorId)
    {
        if (!await _quizRepo.IsTitleUniqueAsync(request.Title, instructorId, quizId))
            throw new ArgumentException("You already have another quiz with this title. Please choose a unique title.");
            
        if (request.TotalQuestions <= 0)
            throw new ArgumentException("Total questions must be greater than 0.");
            
        if (request.Distributions != null && request.Distributions.Any())
        {
            if (request.Distributions.Sum(d => d.QuestionCount) != request.TotalQuestions)
                throw new ArgumentException("Total questions distributed across lessons must equal the quiz's Total Questions.");

            foreach (var dist in request.Distributions)
            {
                var questionsInBank = await _questionBankRepository.GetQuestionsByLessonAsync(dist.LessonId);
                if (questionsInBank.Count < dist.QuestionCount)
                {
                    throw new ArgumentException($"Lesson ID {dist.LessonId} only has {questionsInBank.Count} questions in Question Bank, but you requested {dist.QuestionCount} questions. Please reduce the number or add more questions to the lesson.");
                }
            }
        }

        var quiz = await _quizRepo.GetByIdWithQuestionsAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        
        if (await _quizRepo.HasActiveAttemptsAsync(quizId))
            throw new InvalidOperationException("Cannot update quiz because a student is currently taking it.");

        quiz.Title = request.Title;
        quiz.Description = request.Description;
        quiz.TimeLimitMinutes = request.TimeLimitMinutes;
        quiz.PassingScore = request.PassingScore;
        quiz.TotalQuestions = request.TotalQuestions;

        quiz.QuizLessonDistributions.Clear();
        foreach (var d in request.Distributions)
        {
            quiz.QuizLessonDistributions.Add(new QuizLessonDistribution
            {
                LessonId = d.LessonId,
                QuestionCount = d.QuestionCount
            });
        }

        await _quizRepo.UpdateAsync(quiz);
        return MapToDetail(quiz);
    }

    public async Task<List<QuizSummaryResponse>> GetMyQuizzesAsync(int instructorId)
    {
        var quizzes = await _quizRepo.GetByInstructorAsync(instructorId);
        return quizzes.Select(MapToSummary).ToList();
    }

    public async Task<QuizDetailResponse> GetQuizDetailAsync(int quizId, int instructorId)
    {
        var quiz = await _quizRepo.GetByIdWithQuestionsAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        return MapToDetail(quiz);
    }

    public async Task<List<QuizQuestionResponse>> GetQuizQuestionPoolAsync(int quizId, int instructorId)
    {
        var quiz = await _quizRepo.GetByIdWithQuestionsAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);

        var pool = new List<QuizQuestion>();
        foreach (var dist in quiz.QuizLessonDistributions)
        {
            var lessonQuestions = await _questionBankRepository.GetQuestionsByLessonAsync(dist.LessonId);
            pool.AddRange(lessonQuestions);
        }

        // Nếu phân bổ không đủ số lượng câu hỏi hoặc do làm tròn, bù thêm random từ course.
        // Wait, for Question Pool view, we just show the questions from the designated lessons.
        // If there are no distributions or they don't cover everything, we show course questions?
        // Let's just show lesson questions from distributions. If they want course questions, it's better to force them to use distributions.
        if (!quiz.QuizLessonDistributions.Any())
        {
            var courseQuestions = await _questionBankRepository.GetQuestionsByCourseAsync(quiz.CourseId);
            pool.AddRange(courseQuestions);
        }

        // Map to Response. Wait, QuizQuestionResponse is in another namespace?
        // QuizService doesn't have MapToResponse for QuizQuestion. Let's create a local mapping or use QuestionBankService's mapper?
        // Wait, I can just return List<QuizQuestionResponse> by mapping it here.
        return pool.DistinctBy(q => q.QuestionId).Select(q => new QuizQuestionResponse
        {
            QuestionId = q.QuestionId,
            CourseId = q.CourseId,
            LessonId = q.LessonId,
            QuestionText = q.QuestionText,
            Explanation = q.Explanation,
            QuestionType = q.QuestionType,
            Options = q.QuizOptions.Select(o => new QuizOptionResponse
            {
                OptionId = o.OptionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
        }).ToList();
    }

    public async Task SoftDeleteQuizAsync(int quizId, int instructorId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        
        if (await _quizRepo.HasActiveAttemptsAsync(quizId))
            throw new InvalidOperationException("Cannot delete quiz because a student is currently taking it.");

        if (await _quizRepo.IsQuizInEnrolledCourseAsync(quizId))
            throw new InvalidOperationException("Cannot delete this quiz because it belongs to a course that has enrolled students.");

        await _quizRepo.SoftDeleteAsync(quizId);
    }

    public async Task SetQuizHiddenAsync(int quizId, bool isHidden, int instructorId)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        
        if (await _quizRepo.HasActiveAttemptsAsync(quizId))
            throw new InvalidOperationException("Cannot toggle quiz visibility because a student is currently taking the quiz.");

        await _quizRepo.SetHiddenAsync(quizId, isHidden);
    }

    // ── Instructor: Gán Quiz vào Course ───────────────────────────────────────

    public async Task<CourseQuizResponse> AddQuizToCourseAsync(AddQuizToCourseRequest request, int instructorId)
    {
        await EnsureCourseNotPendingAsync(request.CourseId);

        var quiz = await _quizRepo.GetByIdWithQuestionsAsync(request.QuizId)
            ?? throw new KeyNotFoundException($"Quiz {request.QuizId} does not exist.");

        EnsureOwnership(quiz, instructorId);

        // Kiểm tra đã tồn tại chưa
        var existing = await _quizRepo.GetCourseQuizAsync(request.CourseId, request.QuizId);
        if (existing is not null)
            throw new InvalidOperationException("This quiz has already been added to the course.");

        var courseQuiz = new CourseQuiz
        {
            CourseId = request.CourseId,
            QuizId = request.QuizId,
            OrderIndex = request.OrderIndex,
            IsHidden = quiz.IsHidden
        };

        var created = await _quizRepo.AddToCourseAsync(courseQuiz);
        
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(request.CourseId));
        
        // Update Course Status to Draft if Published
        var course = await _courseRepository.GetByIdAsync(request.CourseId);
        if (course != null && string.Equals(course.CourseStatus, CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            course.CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue();

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
        }

        return MapToCourseQuizResponse(created, quiz);
    }

    public async Task RemoveQuizFromCourseAsync(int courseId, int quizId, int instructorId)
    {
        await EnsureCourseNotPendingAsync(courseId);

        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        
        if (await _quizRepo.HasActiveAttemptsAsync(quizId))
            throw new InvalidOperationException("Cannot remove quiz from course because a student is currently taking the quiz.");

        await _quizRepo.RemoveFromCourseAsync(courseId, quizId);
        
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
        
        // Update Course Status to Draft if Published
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null && string.Equals(course.CourseStatus, CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            course.CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Draft.ToValue();

            _courseRepository.Update(course);
            await _courseRepository.SaveChangesAsync();
        }
    }

    public async Task SetCourseQuizHiddenAsync(int courseId, int quizId, bool isHidden, int instructorId)
    {
        await EnsureCourseNotPendingAsync(courseId);

        var quiz = await _quizRepo.GetByIdAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        EnsureOwnership(quiz, instructorId);
        
        if (await _quizRepo.HasActiveAttemptsAsync(quizId))
            throw new InvalidOperationException("Cannot toggle quiz visibility because a student is currently taking the quiz.");

        await _quizRepo.SetCourseQuizHiddenAsync(courseId, quizId, isHidden);
        
        await _redisService.RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(courseId));
    }

    public async Task<List<CourseQuizResponse>> GetCourseQuizzesAsync(int courseId, int instructorId)
    {
        var courseQuizzes = await _quizRepo.GetCourseQuizzesAsync(courseId);
        return courseQuizzes.Select(cq => MapToCourseQuizResponse(cq, cq.Quiz!)).ToList();
    }

    // ── Student: Làm Quiz ─────────────────────────────────────────────────────

    public async Task<QuizForStudentResponse> GetQuizForStudentAsync(int quizId, int userId)
    {
        var quiz = await _quizRepo.GetByIdWithQuestionsAsync(quizId)
            ?? throw new KeyNotFoundException($"Quiz {quizId} does not exist.");

        if (quiz.IsHidden || quiz.IsRemoved)
            throw new InvalidOperationException("This quiz is not available.");

        // Kiểm tra học viên đã enroll course có chứa quiz này chưa
        var courseQuizzes = await _quizRepo.GetCourseQuizzesAsync(0); // lấy tất cả course chứa quiz
        // Kiểm tra qua enrollment — có ít nhất 1 course chứa quiz mà user đã enrolled, hoặc user là instructor của quiz này
        var isEnrolled = await _enrollmentRepo.IsUserEnrolledInAnyCoursWithQuizAsync(userId, quizId);
        if (!isEnrolled && quiz.InstructorId != userId)
            throw new UnauthorizedAccessException("You have not enrolled in the course containing this quiz.");

        // 1. Sinh câu hỏi từ QuestionBank
        var selectedQuestions = new List<QuizQuestion>();
        var rand = new Random();

        foreach (var dist in quiz.QuizLessonDistributions)
        {
            var countToDraw = dist.QuestionCount;
            if (countToDraw > 0)
            {
                var lessonQuestions = await _questionBankRepository.GetQuestionsByLessonAsync(dist.LessonId);
                var drawn = lessonQuestions.OrderBy(x => rand.Next()).Take(countToDraw);
                selectedQuestions.AddRange(drawn);
            }
        }

        // Nếu phân bổ không đủ số lượng câu hỏi hoặc do làm tròn (countToDraw < TotalQuestions), bù thêm random từ course
        if (selectedQuestions.Count < quiz.TotalQuestions)
        {
            var remaining = quiz.TotalQuestions - selectedQuestions.Count;
            var courseQuestions = await _questionBankRepository.GetQuestionsByCourseAsync(quiz.CourseId);
            var alreadyDrawnIds = selectedQuestions.Select(q => q.QuestionId).ToHashSet();
            var drawnRemaining = courseQuestions.Where(q => !alreadyDrawnIds.Contains(q.QuestionId)).OrderBy(x => rand.Next()).Take(remaining);
            selectedQuestions.AddRange(drawnRemaining);
        }

        // Đảo lộn lại câu hỏi
        selectedQuestions = selectedQuestions.OrderBy(x => rand.Next()).ToList();

        // 2. Tạo Attempt
        var attempt = new QuizAttempt
        {
            QuizId = quizId,
            UserId = userId,
            StartedAt = DateTime.UtcNow,
            QuizAttemptQuestions = selectedQuestions.Select((q, i) => new QuizAttemptQuestion
            {
                QuestionId = q.QuestionId,
                OrderIndex = i
            }).ToList()
        };

        var createdAttempt = await _quizRepo.SaveAttemptAsync(attempt);

        return new QuizForStudentResponse
        {
            QuizId = quiz.QuizId,
            AttemptId = createdAttempt.AttemptId,
            Title = quiz.Title,
            Description = quiz.Description,
            TimeLimitMinutes = quiz.TimeLimitMinutes,
            PassingScore = quiz.PassingScore,
            Questions = selectedQuestions.Select((q, i) => new QuizQuestionForStudentResponse
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Options = q.QuizOptions.OrderBy(o => rand.Next()).Select(o => new QuizOptionForStudentResponse
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    OrderIndex = o.OrderIndex
                }).ToList()
            }).ToList()
        };
    }

    public async Task<QuizAttemptResultResponse> SubmitAttemptAsync(QuizAttemptSubmitRequest request, int userId)
    {
        var attempt = await _quizRepo.GetAttemptByIdAsync(request.AttemptId)
            ?? throw new KeyNotFoundException($"Attempt {request.AttemptId} does not exist.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to submit this attempt.");

        if (attempt.SubmittedAt.HasValue)
            throw new InvalidOperationException("This attempt has already been submitted.");

        var quiz = await _quizRepo.GetByIdAsync(attempt.QuizId)
            ?? throw new KeyNotFoundException($"Quiz does not exist.");

        int totalQuestions = attempt.QuizAttemptQuestions.Count;
        int correctCount = 0;

        foreach (var answerReq in request.Answers)
        {
            var attemptQuestion = attempt.QuizAttemptQuestions.FirstOrDefault(aq => aq.QuestionId == answerReq.QuestionId);
            if (attemptQuestion == null) continue; // Bỏ qua nếu câu hỏi này không nằm trong danh sách bốc được
            
            attempt.QuizAttemptAnswers.Add(new QuizAttemptAnswer
            {
                QuestionId = answerReq.QuestionId,
                SelectedOptionId = answerReq.SelectedOptionId
            });

            if (answerReq.SelectedOptionId.HasValue)
            {
                var option = attemptQuestion.Question?.QuizOptions.FirstOrDefault(o => o.OptionId == answerReq.SelectedOptionId);
                if (option != null && option.IsCorrect)
                {
                    correctCount++;
                }
            }
        }

        int score = totalQuestions > 0
            ? (int)Math.Round((double)correctCount / totalQuestions * 100)
            : 0;

        attempt.Score = score;
        attempt.IsPassed = score >= quiz.PassingScore;
        attempt.SubmittedAt = DateTime.UtcNow;

        await _quizRepo.SaveAttemptAsync(attempt);

        return new QuizAttemptResultResponse
        {
            AttemptId = attempt.AttemptId,
            Score = score,
            IsPassed = attempt.IsPassed.Value,
            TotalQuestions = totalQuestions,
            CorrectCount = correctCount,
            SubmittedAt = attempt.SubmittedAt
        };
    }

    public async Task<QuizAttemptDetailResponse> GetAttemptDetailAsync(int attemptId, int userId)
    {
        var attempt = await _quizRepo.GetAttemptByIdAsync(attemptId);
        if (attempt == null)
            throw new KeyNotFoundException("Attempt not found.");

        if (attempt.UserId != userId)
            throw new UnauthorizedAccessException("You do not have permission to view this attempt.");

        var quiz = await _quizRepo.GetByIdAsync(attempt.QuizId);
        if (quiz == null)
            throw new KeyNotFoundException("Quiz information not found.");

        var questions = attempt.QuizAttemptQuestions.OrderBy(aq => aq.OrderIndex).Select(aq =>
        {
            var q = aq.Question;
            var userAnswer = attempt.QuizAttemptAnswers.FirstOrDefault(a => a.QuestionId == q.QuestionId);
            var isCorrect = false;

            if (userAnswer != null && userAnswer.SelectedOptionId.HasValue)
            {
                var selectedOption = q.QuizOptions.FirstOrDefault(o => o.OptionId == userAnswer.SelectedOptionId.Value);
                if (selectedOption != null && selectedOption.IsCorrect)
                {
                    isCorrect = true;
                }
            }

            return new QuizAttemptQuestionDetailResponse
            {
                QuestionId = q.QuestionId,
                QuestionText = q.QuestionText,
                QuestionType = q.QuestionType,
                Explanation = q.Explanation,
                SelectedOptionId = userAnswer?.SelectedOptionId,
                IsCorrect = isCorrect,
                Options = q.QuizOptions.OrderBy(o => o.OrderIndex).Select(o => new QuizOptionDetailResponse
                {
                    OptionId = o.OptionId,
                    OptionText = o.OptionText,
                    IsCorrect = o.IsCorrect,
                    OrderIndex = o.OrderIndex
                }).ToList()
            };
        }).ToList();

        var response = new QuizAttemptDetailResponse
        {
            AttemptId = attempt.AttemptId,
            QuizId = attempt.QuizId,
            QuizTitle = quiz.Title,
            Score = attempt.Score ?? 0,
            IsPassed = attempt.IsPassed ?? false,
            TotalQuestions = attempt.QuizAttemptQuestions.Count,
            CorrectCount = questions.Count(q => q.IsCorrect),
            SubmittedAt = attempt.SubmittedAt,
            Questions = questions
        };

        return response;
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    public async Task<PagedResult<QuizAttemptSummaryResponse>> GetMyQuizAttemptsAsync(int quizId, int userId, PagedRequestDto request)
    {
        var (items, total) = await _quizRepo.GetAttemptsByQuizAndUserAsync(quizId, userId, request.Page, request.PageSize);
        var mappedItems = items.Select(a => new QuizAttemptSummaryResponse
        {
            AttemptId = a.AttemptId,
            QuizId = a.QuizId,
            QuizTitle = a.Quiz?.Title ?? string.Empty,
            UserId = a.UserId,
            UserFullName = a.User?.FullName ?? string.Empty,
            Score = a.Score ?? 0,
            IsPassed = a.IsPassed ?? false,
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt
        }).ToList();
        
        return new PagedResult<QuizAttemptSummaryResponse>(mappedItems, total, request.Page, request.PageSize);
    }

    public async Task<PagedResult<QuizAttemptSummaryResponse>> GetStudentQuizAttemptsAsync(int quizId, int instructorId, PagedRequestDto request)
    {
        var quiz = await _quizRepo.GetByIdAsync(quizId);
        if (quiz == null) throw new KeyNotFoundException("Quiz not found.");
        EnsureOwnership(quiz, instructorId);

        var (items, total) = await _quizRepo.GetAttemptsByQuizAsync(quizId, request.Page, request.PageSize);
        var mappedItems = items.Select(a => new QuizAttemptSummaryResponse
        {
            AttemptId = a.AttemptId,
            QuizId = a.QuizId,
            QuizTitle = quiz.Title,
            UserId = a.UserId,
            UserFullName = a.User?.FullName ?? string.Empty,
            Score = a.Score ?? 0,
            IsPassed = a.IsPassed ?? false,
            StartedAt = a.StartedAt,
            SubmittedAt = a.SubmittedAt
        }).ToList();
        
        return new PagedResult<QuizAttemptSummaryResponse>(mappedItems, total, request.Page, request.PageSize);
    }

    private void EnsureOwnership(Quiz quiz, int instructorId)
    {
        if (quiz.InstructorId != instructorId)
            throw new UnauthorizedAccessException("You are not the owner of this quiz.");
    }

    private async Task EnsureCourseNotPendingAsync(int courseId)
    {
        var course = await _courseRepository.GetByIdAsync(courseId);
        if (course != null && string.Equals(course.CourseStatus, CourseMarketplaceBE.Domain.Constants.CourseStatus.Pending.ToValue(), StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Cannot modify course quizzes while the course is under review.");
        }
    }

    private static QuizSummaryResponse MapToSummary(Quiz quiz) => new()
    {
        QuizId = quiz.QuizId,
        Title = quiz.Title,
        Description = quiz.Description,
        CourseId = quiz.CourseId,
        CourseTitle = quiz.Course?.Title ?? "Unknown",
        TotalQuestions = quiz.TotalQuestions,
        QuestionCount = quiz.TotalQuestions,
        TimeLimitMinutes = quiz.TimeLimitMinutes,
        PassingScore = quiz.PassingScore,
        IsHidden = quiz.IsHidden,
        IsRemoved = quiz.IsRemoved,
        CreatedAt = quiz.CreatedAt,
        UpdatedAt = quiz.UpdatedAt
    };

    private static QuizDetailResponse MapToDetail(Quiz quiz) => new()
    {
        QuizId = quiz.QuizId,
        Title = quiz.Title,
        Description = quiz.Description,
        CourseId = quiz.CourseId,
        CourseTitle = quiz.Course?.Title ?? "Unknown",
        TotalQuestions = quiz.TotalQuestions,
        QuestionCount = quiz.TotalQuestions,
        TimeLimitMinutes = quiz.TimeLimitMinutes,
        PassingScore = quiz.PassingScore,
        IsHidden = quiz.IsHidden,
        IsRemoved = quiz.IsRemoved,
        CreatedAt = quiz.CreatedAt,
        UpdatedAt = quiz.UpdatedAt,
        Questions = new List<QuizQuestionResponse>(),
        Distributions = quiz.QuizLessonDistributions.Select(d => new QuizLessonDistributionResponse
        {
            LessonId = d.LessonId,
            QuestionCount = d.QuestionCount
        }).ToList() ?? new List<QuizLessonDistributionResponse>()
    };

    private static QuizQuestionResponse MapToQuestionResponse(QuizQuestion q) => new()
    {
        QuestionId = q.QuestionId,
        QuestionText = q.QuestionText,
        QuestionType = q.QuestionType,
        CourseId = q.CourseId,
        LessonId = q.LessonId,
        Options = q.QuizOptions
            .OrderBy(o => o.OrderIndex)
            .Select(o => new QuizOptionResponse
            {
                OptionId = o.OptionId,
                OptionText = o.OptionText,
                IsCorrect = o.IsCorrect,
                OrderIndex = o.OrderIndex
            }).ToList()
    };

    private static CourseQuizResponse MapToCourseQuizResponse(CourseQuiz cq, Quiz quiz) => new()
    {
        CourseQuizId = cq.CourseQuizId,
        CourseId = cq.CourseId,
        QuizId = cq.QuizId,
        Title = quiz.Title,
        Description = quiz.Description,
        QuestionCount = quiz.TotalQuestions,
        TimeLimitMinutes = quiz.TimeLimitMinutes,
        PassingScore = quiz.PassingScore,
        IsHidden = cq.IsHidden,
        OrderIndex = cq.OrderIndex,
        AddedAt = cq.AddedAt
    };
}

