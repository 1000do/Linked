using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Enums;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class QuizServiceTests
    {
        private readonly IQuizRepository _quizRepoMock;
        private readonly IEnrollmentRepository _enrollmentRepoMock;
        private readonly IQuestionBankRepository _questionBankRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly IRedisService _redisServiceMock;
        private readonly QuizService _sut;

        public QuizServiceTests()
        {
            _quizRepoMock = Substitute.For<IQuizRepository>();
            _enrollmentRepoMock = Substitute.For<IEnrollmentRepository>();
            _questionBankRepoMock = Substitute.For<IQuestionBankRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _redisServiceMock = Substitute.For<IRedisService>();

            _sut = new QuizService(
                _quizRepoMock,
                _enrollmentRepoMock,
                _questionBankRepoMock,
                _courseRepoMock,
                _redisServiceMock
            );
        }

        #region CreateQuizAsync

        [Fact]
        public async Task CreateQuizAsync_TitleNotUnique_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizCreateRequest { Title = "Duplicate", TotalQuestions = 10, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 1).Returns(false);

            // 2. Act
            Func<Task> act = async () => await _sut.CreateQuizAsync(req, 1);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("You already have a quiz with this title. Please choose a unique title.");
        }

        [Fact]
        public async Task CreateQuizAsync_TotalQuestionsZeroOrLess_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizCreateRequest { Title = "Unique", TotalQuestions = 0, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 1).Returns(true);

            // 2. Act
            Func<Task> act = async () => await _sut.CreateQuizAsync(req, 1);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Total questions must be greater than 0.");
        }

        [Fact]
        public async Task CreateQuizAsync_DistributionsMismatchTotalQuestions_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizCreateRequest 
            { 
                Title = "Unique", 
                TotalQuestions = 10,
                Distributions = new List<QuizLessonDistributionRequest>
                {
                    new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 5 }
                }
            };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 1).Returns(true);

            // 2. Act
            Func<Task> act = async () => await _sut.CreateQuizAsync(req, 1);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Total questions distributed across lessons must equal the quiz's Total Questions.");
        }

        [Fact]
        public async Task CreateQuizAsync_DistributionsExceedBankCount_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizCreateRequest 
            { 
                Title = "Unique", 
                TotalQuestions = 5,
                Distributions = new List<QuizLessonDistributionRequest>
                {
                    new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 5 }
                }
            };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 1).Returns(true);
            
            // Mock bank returning fewer questions
            var bankQuestions = new List<QuizQuestion> { new QuizQuestion(), new QuizQuestion(), new QuizQuestion() };
            _questionBankRepoMock.GetQuestionsByLessonAsync(1).Returns(bankQuestions); // count is 3

            // 2. Act
            Func<Task> act = async () => await _sut.CreateQuizAsync(req, 1);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Lesson ID 1 only has 3 questions in Question Bank, but you requested 5 questions. Please reduce the number or add more questions to the lesson.");
        }

        [Fact]
        public async Task CreateQuizAsync_ValidRequest_CreatesAndReturnsQuiz()
        {
            // 1. Arrange
            var req = new QuizCreateRequest 
            { 
                Title = "Unique", 
                TotalQuestions = 5,
                Distributions = new List<QuizLessonDistributionRequest>
                {
                    new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 5 }
                }
            };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 1).Returns(true);
            
            var bankQuestions = new List<QuizQuestion> { new QuizQuestion(), new QuizQuestion(), new QuizQuestion(), new QuizQuestion(), new QuizQuestion(), new QuizQuestion() };
            _questionBankRepoMock.GetQuestionsByLessonAsync(1).Returns(bankQuestions); // count is 6
            
            _quizRepoMock.CreateAsync(Arg.Any<Quiz>()).Returns(callInfo => 
            {
                var q = callInfo.Arg<Quiz>();
                q.QuizId = 99;
                return q;
            });

            // 2. Act
            var result = await _sut.CreateQuizAsync(req, 1);

            // 3. Assert
            result.Should().NotBeNull();
            result.QuizId.Should().Be(99);
            result.Title.Should().Be("Unique");

            // 4. Verify
            await _quizRepoMock.Received(1).CreateAsync(Arg.Any<Quiz>());
        }

        #endregion

        #region UpdateQuizSettingsAsync

        [Fact]
        public async Task UpdateQuizSettingsAsync_TitleNotUnique_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Duplicate" };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(false);

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateQuizSettingsAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Unique", TotalQuestions = 5, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(Task.FromResult<Quiz?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task UpdateQuizSettingsAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Unique", TotalQuestions = 5, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 100 }); // Not 2

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateQuizSettingsAsync_HasActiveAttempts_ThrowsInvalidOperationException()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Unique", TotalQuestions = 5, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(true);

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot update quiz because a student is currently taking it.");
        }

        [Fact]
        public async Task UpdateQuizSettingsAsync_ValidRequest_UpdatesAndReturnsQuiz()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Unique", TotalQuestions = 5, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);
            
            var existingQuiz = new Quiz { QuizId = 1, InstructorId = 2, QuizLessonDistributions = new List<QuizLessonDistribution>() };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(existingQuiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);
            
            // Mock Update returns nothing but we assert on existingQuiz

            // 2. Act
            var result = await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            result.Title.Should().Be("Unique");
            
            // 4. Verify
            await _quizRepoMock.Received(1).UpdateAsync(existingQuiz);
        }

        #endregion

        #region SoftDeleteQuizAsync

        [Fact]
        public async Task SoftDeleteQuizAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            // 1. Arrange
            _quizRepoMock.GetByIdAsync(1).Returns(Task.FromResult<Quiz?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.SoftDeleteQuizAsync(1, 2);

            // 3. Assert
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SoftDeleteQuizAsync_ValidRequest_SoftDeletes()
        {
            // 1. Arrange
            var quiz = new Quiz { QuizId = 1, InstructorId = 2, IsRemoved = false };
            _quizRepoMock.GetByIdAsync(1).Returns(quiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);
            _quizRepoMock.IsQuizInEnrolledCourseAsync(1).Returns(false);

            // 2. Act
            await _sut.SoftDeleteQuizAsync(1, 2);

            // 3. Assert (Does not throw)
            
            // 4. Verify
            await _quizRepoMock.Received(1).SoftDeleteAsync(1);
        }

        #endregion

        #region AddQuizToCourseAsync

        [Fact]
        public async Task AddQuizToCourseAsync_CourseIsPending_ThrowsInvalidOperationException()
        {
            // 1. Arrange
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Pending.ToValue() });

            // 2. Act
            Func<Task> act = async () => await _sut.AddQuizToCourseAsync(req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        
        [Fact]
        public async Task AddQuizToCourseAsync_ValidRequest_AddsQuizAndInvalidatesCache()
        {
            // 1. Arrange
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            var course = new Course { CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            _courseRepoMock.GetByIdAsync(10).Returns(course);
            
            var quiz = new Quiz { InstructorId = 2 };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);
            
            _quizRepoMock.GetCourseQuizAsync(10, 1).Returns(Task.FromResult<CourseQuiz?>(null));
            _quizRepoMock.AddToCourseAsync(Arg.Any<CourseQuiz>()).Returns(callInfo => callInfo.Arg<CourseQuiz>());

            // 2. Act
            await _sut.AddQuizToCourseAsync(req, 2);

            // 3. Assert (does not throw)
            
            // 4. Verify
            await _quizRepoMock.Received(1).AddToCourseAsync(Arg.Any<CourseQuiz>());
        }

        #endregion
        
        #region SubmitAttemptAsync
        
        [Fact]
        public async Task SubmitAttemptAsync_ValidRequest_CalculatesScoreAndSaves()
        {
            // 1. Arrange
            var attemptId = 1;
            var userId = 2;
            var req = new QuizAttemptSubmitRequest
            {
                AttemptId = attemptId,
                Answers = new List<AttemptAnswerRequest>
                {
                    new AttemptAnswerRequest { QuestionId = 10, SelectedOptionId = 101 }
                }
            };
            
            var attempt = new QuizAttempt 
            { 
                AttemptId = attemptId, UserId = userId, QuizId = 99, 
                QuizAttemptQuestions = new List<QuizAttemptQuestion>
                {
                    new QuizAttemptQuestion 
                    { 
                        QuestionId = 10, 
                        Question = new QuizQuestion 
                        { 
                            QuestionId = 10, QuestionType = QuizQuestionType.SingleChoice,
                            QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 101, IsCorrect = true } } 
                        } 
                    }
                },
                QuizAttemptAnswers = new List<QuizAttemptAnswer>()
            };
            _quizRepoMock.GetAttemptByIdAsync(attemptId).Returns(attempt);
            
            var quiz = new Quiz { QuizId = 99, PassingScore = 80 };
            _quizRepoMock.GetByIdAsync(99).Returns(quiz);
            
            // 2. Act
            var result = await _sut.SubmitAttemptAsync(req, userId);

            // 3. Assert
            result.Score.Should().Be(100);
            result.IsPassed.Should().BeTrue();
            
            // 4. Verify
            await _quizRepoMock.Received(1).SaveAttemptAsync(attempt);
        }

        #endregion
    }
}
