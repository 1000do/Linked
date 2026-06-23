using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using FluentAssertions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class QuizServiceTests
{
    private readonly IQuizRepository _quizRepoMock;
    private readonly IEnrollmentRepository _enrollmentRepoMock;
    private readonly IQuestionBankRepository _questionBankRepoMock;
    private readonly ICourseRepository _courseRepoMock;
    private readonly QuizService _sut;

    public QuizServiceTests()
    {
        _quizRepoMock = Substitute.For<IQuizRepository>();
        _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
        _questionBankRepoMock = Substitute.For<IQuestionBankRepository>();
        _courseRepoMock = Substitute.For<ICourseRepository>();
        _sut = new QuizService(_quizRepoMock, _enrollmentRepoMock, _questionBankRepoMock, _courseRepoMock);
    }

    [Fact]
    public async Task UpdateQuizSettingsAsync_TotalQuestionsIsZero_ThrowsArgumentException()
    {
        // Arrange 1
        var request = new QuizUpdateRequest
        {
            TotalQuestions = 0,
            Distributions = new List<QuizLessonDistributionRequest>
            {
                new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 10 }
            }
        };

        // Arrange 2
        // No mocks needed before exception

        // Act
        Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, request, 100);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Total questions must be greater than 0.");
            
        await _quizRepoMock.DidNotReceiveWithAnyArgs().GetByIdWithQuestionsAsync(default);
    }

    [Fact]
    public async Task UpdateQuizSettingsAsync_TotalDistributionNotEqualTotalQuestions_ThrowsArgumentException()
    {
        // Arrange 1
        var request = new QuizUpdateRequest
        {
            TotalQuestions = 10,
            Distributions = new List<QuizLessonDistributionRequest>
            {
                new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 5 },
                new QuizLessonDistributionRequest { LessonId = 2, QuestionCount = 4 }
            }
        };

        // Arrange 2
        // No mocks needed before exception

        // Act
        Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, request, 100);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("Total questions distributed across lessons must equal the quiz's Total Questions.");
            
        await _quizRepoMock.DidNotReceiveWithAnyArgs().GetByIdWithQuestionsAsync(default);
    }

    [Fact]
    public async Task GetQuizForStudentAsync_WhenLessonHasFewerQuestionsThanRequested_MathMinPreventsCrash()
    {
        // Arrange 1
        int quizId = 1;
        int userId = 100;
        
        var quiz = new Quiz
        {
            QuizId = quizId,
            TotalQuestions = 10,
            QuizLessonDistributions = new List<QuizLessonDistribution>
            {
                new QuizLessonDistribution { LessonId = 1, QuestionCount = 10 }
            }
        };
        
        // Setup 3 existing questions (fewer than the 10 requested by 100% of TotalQuestions)
        var lessonQuestions = new List<QuizQuestion>
        {
            new QuizQuestion { QuestionId = 1, LessonId = 1, QuizOptions = new List<QuizOption> { new QuizOption() } },
            new QuizQuestion { QuestionId = 2, LessonId = 1, QuizOptions = new List<QuizOption> { new QuizOption() } },
            new QuizQuestion { QuestionId = 3, LessonId = 1, QuizOptions = new List<QuizOption> { new QuizOption() } }
        };

        // Arrange 2
        _quizRepoMock.GetByIdWithQuestionsAsync(quizId).Returns(quiz);
        _quizRepoMock.GetCourseQuizzesAsync(0).Returns(new List<CourseQuiz>());
        _enrollmentRepoMock.IsUserEnrolledInAnyCoursWithQuizAsync(userId, quizId).Returns(true);
        _questionBankRepoMock.GetQuestionsByLessonAsync(1).Returns(lessonQuestions);
        _questionBankRepoMock.GetQuestionsByCourseAsync(Arg.Any<int>()).Returns(new List<QuizQuestion>());
        _quizRepoMock.SaveAttemptAsync(Arg.Any<QuizAttempt>()).Returns(new QuizAttempt { AttemptId = 1 });

        // Act
        var result = await _sut.GetQuizForStudentAsync(quizId, userId);

        // Assert
        result.Should().NotBeNull();
        result.Questions.Should().HaveCount(3); // Expect Math.Min(10, 3) to cap it at 3

        await _questionBankRepoMock.Received(1).GetQuestionsByLessonAsync(1);
        await _quizRepoMock.Received(1).SaveAttemptAsync(Arg.Is<QuizAttempt>(a => a.QuizAttemptQuestions.Count == 3));
    }

        [Fact]
    public async Task SubmitAttemptAsync_SelectedCorrectOption_CalculatesAsCorrect()
    {
        // Arrange 1
        int attemptId = 10;
        int userId = 100;
        
        var qbQuestion = new QuizQuestion
        {
            QuestionId = 1,
            QuestionType = Domain.Enums.QuizQuestionType.SingleChoice,
            QuizOptions = new List<QuizOption>
            {
                new QuizOption { OptionId = 101, IsCorrect = true },
                new QuizOption { OptionId = 102, IsCorrect = false }
            }
        };

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            UserId = userId,
            QuizId = 1,
            QuizAttemptQuestions = new List<QuizAttemptQuestion>
            {
                new QuizAttemptQuestion { QuestionId = 1, OrderIndex = 0, Question = qbQuestion }
            },
            QuizAttemptAnswers = new List<QuizAttemptAnswer>()
        };
        
        var quiz = new Quiz { QuizId = 1, PassingScore = 50 };
        
        var request = new QuizAttemptSubmitRequest
        {
            AttemptId = attemptId,
            Answers = new List<AttemptAnswerRequest>
            {
                new AttemptAnswerRequest 
                { 
                    QuestionId = 1, 
                    SelectedOptionId = 101 // Correct
                }
            }
        };

        // Arrange 2
        _quizRepoMock.GetAttemptByIdAsync(attemptId).Returns(attempt);
        _quizRepoMock.GetByIdAsync(1).Returns(quiz);

        // Act
        var result = await _sut.SubmitAttemptAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.CorrectCount.Should().Be(1);
        result.Score.Should().Be(100);
        result.IsPassed.Should().BeTrue();

        await _quizRepoMock.Received(1).SaveAttemptAsync(Arg.Is<QuizAttempt>(a => a.Score == 100));
    }
    
    [Fact]
    public async Task SubmitAttemptAsync_SelectedWrongOption_CalculatesAsIncorrect()
    {
        // Arrange 1
        int attemptId = 10;
        int userId = 100;
        
        var qbQuestion = new QuizQuestion
        {
            QuestionId = 1,
            QuestionType = Domain.Enums.QuizQuestionType.SingleChoice,
            QuizOptions = new List<QuizOption>
            {
                new QuizOption { OptionId = 101, IsCorrect = true },
                new QuizOption { OptionId = 102, IsCorrect = false }
            }
        };

        var attempt = new QuizAttempt
        {
            AttemptId = attemptId,
            UserId = userId,
            QuizId = 1,
            QuizAttemptQuestions = new List<QuizAttemptQuestion>
            {
                new QuizAttemptQuestion { QuestionId = 1, OrderIndex = 0, Question = qbQuestion }
            },
            QuizAttemptAnswers = new List<QuizAttemptAnswer>()
        };
        
        var quiz = new Quiz { QuizId = 1, PassingScore = 50 };
        
        var request = new QuizAttemptSubmitRequest
        {
            AttemptId = attemptId,
            Answers = new List<AttemptAnswerRequest>
            {
                new AttemptAnswerRequest 
                { 
                    QuestionId = 1, 
                    SelectedOptionId = 102 // Wrong
                }
            }
        };

        // Arrange 2
        _quizRepoMock.GetAttemptByIdAsync(attemptId).Returns(attempt);
        _quizRepoMock.GetByIdAsync(1).Returns(quiz);

        // Act
        var result = await _sut.SubmitAttemptAsync(request, userId);

        // Assert
        result.Should().NotBeNull();
        result.CorrectCount.Should().Be(0);
        result.Score.Should().Be(0);

        await _quizRepoMock.Received(1).SaveAttemptAsync(Arg.Is<QuizAttempt>(a => a.Score == 0));
    }
}
