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
    public partial class QuizServiceTests
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
        public async Task UpdateQuizSettingsAsync_TotalQuestionsZeroOrLess_ThrowsArgumentException()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest { Title = "Unique", TotalQuestions = 0, Distributions = new List<QuizLessonDistributionRequest>() };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuizSettingsAsync(1, req, 2);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>().WithMessage("Total questions must be greater than 0.");
        }

        [Fact]
        public async Task UpdateQuizSettingsAsync_ValidRequest_UpdatesAndReturnsQuiz()
        {
            // 1. Arrange
            var req = new QuizUpdateRequest 
            { 
                Title = "Unique", 
                TotalQuestions = 5, 
                Distributions = new List<QuizLessonDistributionRequest> 
                { 
                    new QuizLessonDistributionRequest { LessonId = 1, QuestionCount = 5 } 
                } 
            };
            _quizRepoMock.IsTitleUniqueAsync(req.Title, 2, 1).Returns(true);
            
            _questionBankRepoMock.GetQuestionsByLessonAsync(1).Returns(new List<QuizQuestion> { new(), new(), new(), new(), new() });

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
    
        #region Instructor Methods

        [Fact]
        public async Task GetMyQuizzesAsync_ReturnsQuizzes()
        {
            var quizzes = new List<Quiz>
            {
                new Quiz { QuizId = 1, Title = "Quiz 1", Course = new Course { Title = "Course 1" }, TotalQuestions = 10 },
                new Quiz { QuizId = 2, Title = "Quiz 2" }
            };
            _quizRepoMock.GetByInstructorAsync(1).Returns(quizzes);

            var result = await _sut.GetMyQuizzesAsync(1);

            result.Should().HaveCount(2);
            result[0].CourseTitle.Should().Be("Course 1");
            result[1].CourseTitle.Should().Be("Unknown");
        }

        [Fact]
        public async Task GetQuizDetailAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.GetQuizDetailAsync(1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetQuizDetailAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.GetQuizDetailAsync(1, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetQuizDetailAsync_Valid_ReturnsDetail()
        {
            var quiz = new Quiz 
            { 
                QuizId = 1, InstructorId = 2, Title = "Title", Course = new Course { Title = "CourseTitle" },
                QuizLessonDistributions = new List<QuizLessonDistribution> { new QuizLessonDistribution { LessonId = 10, QuestionCount = 5 } }
            };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);

            var result = await _sut.GetQuizDetailAsync(1, 2);

            result.Title.Should().Be("Title");
            result.CourseTitle.Should().Be("CourseTitle");
            result.Distributions.Should().HaveCount(1);
        }

        [Fact]
        public async Task GetQuizDetailAsync_Valid_DistributionsNull_ReturnsEmpty()
        {
            var quiz = new Quiz 
            { 
                QuizId = 1, InstructorId = 2, Title = "Title", Course = null,
                QuizLessonDistributions = null! // testing fallback to empty list
            };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);

            var result = await _sut.GetQuizDetailAsync(1, 2);

            result.CourseTitle.Should().Be("Unknown");
            result.Distributions.Should().BeEmpty();
        }

        [Fact]
        public async Task GetQuizQuestionPoolAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.GetQuizQuestionPoolAsync(1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetQuizQuestionPoolAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.GetQuizQuestionPoolAsync(1, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetQuizQuestionPoolAsync_NoDistributions_LoadsFromCourse()
        {
            var quiz = new Quiz { QuizId = 1, InstructorId = 2, CourseId = 10, QuizLessonDistributions = new List<QuizLessonDistribution>() };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);
            
            var courseQuestions = new List<QuizQuestion> 
            { 
                new QuizQuestion { QuestionId = 1, QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 1 } } } 
            };
            _questionBankRepoMock.GetQuestionsByCourseAsync(10).Returns(courseQuestions);

            var result = await _sut.GetQuizQuestionPoolAsync(1, 2);

            result.Should().HaveCount(1);
            result[0].QuestionId.Should().Be(1);
            result[0].Options.Should().HaveCount(1);
            await _questionBankRepoMock.DidNotReceive().GetQuestionsByLessonAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetQuizQuestionPoolAsync_WithDistributions_LoadsFromLessons()
        {
            var quiz = new Quiz 
            { 
                QuizId = 1, InstructorId = 2, CourseId = 10, 
                QuizLessonDistributions = new List<QuizLessonDistribution> { new QuizLessonDistribution { LessonId = 20 } }
            };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);
            
            var lessonQuestions = new List<QuizQuestion> { new QuizQuestion { QuestionId = 2, QuizOptions = new List<QuizOption>() } };
            _questionBankRepoMock.GetQuestionsByLessonAsync(20).Returns(lessonQuestions);

            var result = await _sut.GetQuizQuestionPoolAsync(1, 2);

            result.Should().HaveCount(1);
            await _questionBankRepoMock.DidNotReceive().GetQuestionsByCourseAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task SoftDeleteQuizAsync_HasActiveAttempts_ThrowsInvalidOperationException()
        {
            var quiz = new Quiz { QuizId = 1, InstructorId = 2 };
            _quizRepoMock.GetByIdAsync(1).Returns(quiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(true);

            Func<Task> act = async () => await _sut.SoftDeleteQuizAsync(1, 2);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete quiz because a student is currently taking it.");
        }

        [Fact]
        public async Task SoftDeleteQuizAsync_InEnrolledCourse_ThrowsInvalidOperationException()
        {
            var quiz = new Quiz { QuizId = 1, InstructorId = 2 };
            _quizRepoMock.GetByIdAsync(1).Returns(quiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);
            _quizRepoMock.IsQuizInEnrolledCourseAsync(1).Returns(true);

            Func<Task> act = async () => await _sut.SoftDeleteQuizAsync(1, 2);

            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot delete this quiz because it belongs to a course that has enrolled students.");
        }

        [Fact]
        public async Task SetQuizHiddenAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetByIdAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.SetQuizHiddenAsync(1, true, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SetQuizHiddenAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.SetQuizHiddenAsync(1, true, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SetQuizHiddenAsync_HasActiveAttempts_ThrowsInvalidOperationException()
        {
            var quiz = new Quiz { QuizId = 1, InstructorId = 2 };
            _quizRepoMock.GetByIdAsync(1).Returns(quiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(true);

            Func<Task> act = async () => await _sut.SetQuizHiddenAsync(1, true, 2);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SetQuizHiddenAsync_Valid_SetsHidden()
        {
            var quiz = new Quiz { QuizId = 1, InstructorId = 2 };
            _quizRepoMock.GetByIdAsync(1).Returns(quiz);
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);

            await _sut.SetQuizHiddenAsync(1, true, 2);

            await _quizRepoMock.Received(1).SetHiddenAsync(1, true);
        }

        [Fact]
        public async Task AddQuizToCourseAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns((Quiz?)null);

            Func<Task> act = async () => await _sut.AddQuizToCourseAsync(req, 2);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task AddQuizToCourseAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 99 });

            Func<Task> act = async () => await _sut.AddQuizToCourseAsync(req, 2);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task AddQuizToCourseAsync_AlreadyAdded_ThrowsInvalidOperationException()
        {
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.GetCourseQuizAsync(10, 1).Returns(new CourseQuiz());

            Func<Task> act = async () => await _sut.AddQuizToCourseAsync(req, 2);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }
        
        [Fact]
        public async Task AddQuizToCourseAsync_CourseIsPublished_ChangesToDraft()
        {
            var req = new AddQuizToCourseRequest { CourseId = 10, QuizId = 1 };
            var course = new Course { CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
            _courseRepoMock.GetByIdAsync(10).Returns(course);
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.GetCourseQuizAsync(10, 1).Returns((CourseQuiz?)null);
            _quizRepoMock.AddToCourseAsync(Arg.Any<CourseQuiz>()).Returns(callInfo => callInfo.Arg<CourseQuiz>());

            await _sut.AddQuizToCourseAsync(req, 2);

            course.CourseStatus.Should().Be(CourseStatus.Draft.ToValue());
            _courseRepoMock.Received(1).Update(course);
            await _courseRepoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveQuizFromCourseAsync_CoursePending_ThrowsInvalidOperationException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Pending.ToValue() });
            Func<Task> act = async () => await _sut.RemoveQuizFromCourseAsync(10, 1, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RemoveQuizFromCourseAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.RemoveQuizFromCourseAsync(10, 1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task RemoveQuizFromCourseAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.RemoveQuizFromCourseAsync(10, 1, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task RemoveQuizFromCourseAsync_HasActiveAttempts_ThrowsInvalidOperationException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(true);
            Func<Task> act = async () => await _sut.RemoveQuizFromCourseAsync(10, 1, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task RemoveQuizFromCourseAsync_Valid_RemovesAndCaches()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Published.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);

            await _sut.RemoveQuizFromCourseAsync(10, 1, 2);

            await _quizRepoMock.Received(1).RemoveFromCourseAsync(10, 1);
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(10));
        }

        [Fact]
        public async Task SetCourseQuizHiddenAsync_CoursePending_ThrowsInvalidOperationException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Pending.ToValue() });
            Func<Task> act = async () => await _sut.SetCourseQuizHiddenAsync(10, 1, true, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SetCourseQuizHiddenAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.SetCourseQuizHiddenAsync(10, 1, true, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SetCourseQuizHiddenAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.SetCourseQuizHiddenAsync(10, 1, true, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SetCourseQuizHiddenAsync_HasActiveAttempts_ThrowsInvalidOperationException()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(true);
            Func<Task> act = async () => await _sut.SetCourseQuizHiddenAsync(10, 1, true, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SetCourseQuizHiddenAsync_Valid_UpdatesAndCaches()
        {
            _courseRepoMock.GetByIdAsync(10).Returns(new Course { CourseStatus = CourseStatus.Draft.ToValue() });
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 2 });
            _quizRepoMock.HasActiveAttemptsAsync(1).Returns(false);

            await _sut.SetCourseQuizHiddenAsync(10, 1, true, 2);

            await _quizRepoMock.Received(1).SetCourseQuizHiddenAsync(10, 1, true);
            await _redisServiceMock.Received(1).RemoveCacheAsync(CacheKeys.CourseDetail.GetKey(10));
        }

        [Fact]
        public async Task GetCourseQuizzesAsync_ReturnsQuizzes()
        {
            var courseQuizzes = new List<CourseQuiz>
            {
                new CourseQuiz { CourseId = 10, QuizId = 1, Quiz = new Quiz { Title = "Q" } }
            };
            _quizRepoMock.GetCourseQuizzesAsync(10).Returns(courseQuizzes);

            var result = await _sut.GetCourseQuizzesAsync(10, 2);

            result.Should().HaveCount(1);
            result[0].Title.Should().Be("Q");
        }

        [Fact]
        public async Task GetMyQuizAttemptsAsync_ReturnsPagedResult()
        {
            var req = new PagedRequestDto { Page = 1, PageSize = 10 };
            var attempts = new List<QuizAttempt>
            {
                new QuizAttempt { AttemptId = 1, Quiz = new Quiz { Title = "Q" }, User = new User { FullName = "U" } }
            };
            _quizRepoMock.GetAttemptsByQuizAndUserAsync(1, 2, 1, 10).Returns((attempts, 1));

            var result = await _sut.GetMyQuizAttemptsAsync(1, 2, req);

            result.TotalCount.Should().Be(1);
            result.Items.First().QuizTitle.Should().Be("Q");
            result.Items.First().UserFullName.Should().Be("U");
        }
        
        [Fact]
        public async Task GetMyQuizAttemptsAsync_NullNavigationProperties_HandlesGracefully()
        {
            var req = new PagedRequestDto { Page = 1, PageSize = 10 };
            var attempts = new List<QuizAttempt>
            {
                new QuizAttempt { AttemptId = 1, Quiz = null, User = null }
            };
            _quizRepoMock.GetAttemptsByQuizAndUserAsync(1, 2, 1, 10).Returns((attempts, 1));

            var result = await _sut.GetMyQuizAttemptsAsync(1, 2, req);

            var resultList = result.Items.ToList();
            resultList[0].QuizTitle.Should().Be(string.Empty);
            resultList[0].UserFullName.Should().Be(string.Empty);
        }

        [Fact]
        public async Task GetStudentQuizAttemptsAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            var req = new PagedRequestDto { Page = 1, PageSize = 10 };
            _quizRepoMock.GetByIdAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.GetStudentQuizAttemptsAsync(1, 2, req);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetStudentQuizAttemptsAsync_NotOwner_ThrowsUnauthorizedAccessException()
        {
            var req = new PagedRequestDto { Page = 1, PageSize = 10 };
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 99 });
            Func<Task> act = async () => await _sut.GetStudentQuizAttemptsAsync(1, 2, req);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetStudentQuizAttemptsAsync_Valid_ReturnsPagedResult()
        {
            var req = new PagedRequestDto { Page = 1, PageSize = 10 };
            _quizRepoMock.GetByIdAsync(1).Returns(new Quiz { InstructorId = 2, Title = "Q" });
            var attempts = new List<QuizAttempt>
            {
                new QuizAttempt { AttemptId = 1, User = null }
            };
            _quizRepoMock.GetAttemptsByQuizAsync(1, 1, 10).Returns((attempts, 1));

            var result = await _sut.GetStudentQuizAttemptsAsync(1, 2, req);

            result.TotalCount.Should().Be(1);
            result.Items.First().QuizTitle.Should().Be("Q");
        }

        #endregion
    

        #region Student Methods

        [Fact]
        public async Task GetQuizForStudentAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.GetQuizForStudentAsync(1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetQuizForStudentAsync_HiddenOrRemoved_ThrowsInvalidOperationException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { IsHidden = true });
            Func<Task> act = async () => await _sut.GetQuizForStudentAsync(1, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();

            _quizRepoMock.GetByIdWithQuestionsAsync(2).Returns(new Quiz { IsRemoved = true });
            Func<Task> act2 = async () => await _sut.GetQuizForStudentAsync(2, 2);
            await act2.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task GetQuizForStudentAsync_NotEnrolledAndNotInstructor_ThrowsUnauthorizedAccessException()
        {
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(new Quiz { InstructorId = 99 });
            _enrollmentRepoMock.IsUserEnrolledInAnyCoursWithQuizAsync(2, 1).Returns(false);

            Func<Task> act = async () => await _sut.GetQuizForStudentAsync(1, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetQuizForStudentAsync_Valid_ReturnsQuizForStudent()
        {
            var quiz = new Quiz 
            { 
                QuizId = 1, InstructorId = 99, TotalQuestions = 2, CourseId = 10,
                QuizLessonDistributions = new List<QuizLessonDistribution>
                {
                    new QuizLessonDistribution { LessonId = 5, QuestionCount = 1 }
                }
            };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);
            _enrollmentRepoMock.IsUserEnrolledInAnyCoursWithQuizAsync(2, 1).Returns(true);

            var lessonQuestions = new List<QuizQuestion> 
            { 
                new QuizQuestion 
                { 
                    QuestionId = 101, 
                    QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 1, OptionText = "A" } } 
                },
                new QuizQuestion 
                { 
                    QuestionId = 102, 
                    QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 2, OptionText = "B" } } 
                }
            };
            _questionBankRepoMock.GetQuestionsByLessonAsync(5).Returns(lessonQuestions);

            var courseQuestions = new List<QuizQuestion>
            {
                new QuizQuestion { QuestionId = 103, QuizOptions = new List<QuizOption>() }
            };
            _questionBankRepoMock.GetQuestionsByCourseAsync(10).Returns(courseQuestions);

            _quizRepoMock.SaveAttemptAsync(Arg.Any<QuizAttempt>()).Returns(callInfo => callInfo.Arg<QuizAttempt>());

            var result = await _sut.GetQuizForStudentAsync(1, 2);

            result.Questions.Should().HaveCount(2);
            await _quizRepoMock.Received(1).SaveAttemptAsync(Arg.Any<QuizAttempt>());
        }
        
        [Fact]
        public async Task GetQuizForStudentAsync_IsInstructor_AllowsAccess()
        {
            var quiz = new Quiz 
            { 
                QuizId = 1, InstructorId = 2, TotalQuestions = 0,
                QuizLessonDistributions = new List<QuizLessonDistribution>()
            };
            _quizRepoMock.GetByIdWithQuestionsAsync(1).Returns(quiz);
            _enrollmentRepoMock.IsUserEnrolledInAnyCoursWithQuizAsync(2, 1).Returns(false);
            _quizRepoMock.SaveAttemptAsync(Arg.Any<QuizAttempt>()).Returns(callInfo => callInfo.Arg<QuizAttempt>());

            var result = await _sut.GetQuizForStudentAsync(1, 2);

            result.Questions.Should().BeEmpty();
        }

        [Fact]
        public async Task SubmitAttemptAsync_AttemptNotFound_ThrowsKeyNotFoundException()
        {
            var req = new QuizAttemptSubmitRequest { AttemptId = 1 };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns((QuizAttempt?)null);
            Func<Task> act = async () => await _sut.SubmitAttemptAsync(req, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task SubmitAttemptAsync_WrongUser_ThrowsUnauthorizedAccessException()
        {
            var req = new QuizAttemptSubmitRequest { AttemptId = 1 };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(new QuizAttempt { UserId = 99 });
            Func<Task> act = async () => await _sut.SubmitAttemptAsync(req, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task SubmitAttemptAsync_AlreadySubmitted_ThrowsInvalidOperationException()
        {
            var req = new QuizAttemptSubmitRequest { AttemptId = 1 };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(new QuizAttempt { UserId = 2, SubmittedAt = DateTime.UtcNow });
            Func<Task> act = async () => await _sut.SubmitAttemptAsync(req, 2);
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task SubmitAttemptAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            var req = new QuizAttemptSubmitRequest { AttemptId = 1 };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(new QuizAttempt { UserId = 2, QuizId = 10 });
            _quizRepoMock.GetByIdAsync(10).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.SubmitAttemptAsync(req, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }
        
        [Fact]
        public async Task SubmitAttemptAsync_CalculatesScoreCorrectly_WithMissingAnswersAndIncorrect()
        {
            var req = new QuizAttemptSubmitRequest 
            { 
                AttemptId = 1,
                Answers = new List<AttemptAnswerRequest>
                {
                    new AttemptAnswerRequest { QuestionId = 101, SelectedOptionId = 1 }, // Correct
                    new AttemptAnswerRequest { QuestionId = 102, SelectedOptionId = 2 }, // Incorrect
                    new AttemptAnswerRequest { QuestionId = 103, SelectedOptionId = null } // Skipped but sent
                }
            };
            
            var attempt = new QuizAttempt 
            { 
                UserId = 2, QuizId = 10,
                QuizAttemptQuestions = new List<QuizAttemptQuestion>
                {
                    new QuizAttemptQuestion 
                    { 
                        QuestionId = 101, 
                        Question = new QuizQuestion { QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 1, IsCorrect = true } } }
                    },
                    new QuizAttemptQuestion 
                    { 
                        QuestionId = 102, 
                        Question = new QuizQuestion { QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 2, IsCorrect = false } } }
                    },
                    new QuizAttemptQuestion 
                    { 
                        QuestionId = 103, 
                        Question = new QuizQuestion { QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 3, IsCorrect = true } } }
                    },
                    new QuizAttemptQuestion // not in answers
                    {
                        QuestionId = 104,
                        Question = new QuizQuestion { QuizOptions = new List<QuizOption>() }
                    }
                },
                QuizAttemptAnswers = new List<QuizAttemptAnswer>()
            };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(attempt);
            _quizRepoMock.GetByIdAsync(10).Returns(new Quiz { PassingScore = 50 });

            var result = await _sut.SubmitAttemptAsync(req, 2);

            // 1 correct out of 4 total questions => 25%
            result.Score.Should().Be(25);
            result.IsPassed.Should().BeFalse();
            result.CorrectCount.Should().Be(1);
        }

        [Fact]
        public async Task SubmitAttemptAsync_ZeroQuestions_ScoreIsZero()
        {
            var req = new QuizAttemptSubmitRequest { AttemptId = 1, Answers = new List<AttemptAnswerRequest>() };
            var attempt = new QuizAttempt { UserId = 2, QuizId = 10, QuizAttemptQuestions = new List<QuizAttemptQuestion>(), QuizAttemptAnswers = new List<QuizAttemptAnswer>() };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(attempt);
            _quizRepoMock.GetByIdAsync(10).Returns(new Quiz { PassingScore = 50 });

            var result = await _sut.SubmitAttemptAsync(req, 2);

            result.Score.Should().Be(0);
        }

        [Fact]
        public async Task GetAttemptDetailAsync_AttemptNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetAttemptByIdAsync(1).Returns((QuizAttempt?)null);
            Func<Task> act = async () => await _sut.GetAttemptDetailAsync(1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetAttemptDetailAsync_WrongUser_ThrowsUnauthorizedAccessException()
        {
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(new QuizAttempt { UserId = 99 });
            Func<Task> act = async () => await _sut.GetAttemptDetailAsync(1, 2);
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetAttemptDetailAsync_QuizNotFound_ThrowsKeyNotFoundException()
        {
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(new QuizAttempt { UserId = 2, QuizId = 10 });
            _quizRepoMock.GetByIdAsync(10).Returns((Quiz?)null);
            Func<Task> act = async () => await _sut.GetAttemptDetailAsync(1, 2);
            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        [Fact]
        public async Task GetAttemptDetailAsync_Valid_ReturnsDetail()
        {
            var attempt = new QuizAttempt 
            { 
                AttemptId = 1, UserId = 2, QuizId = 10, Score = 100, IsPassed = true,
                QuizAttemptQuestions = new List<QuizAttemptQuestion>
                {
                    new QuizAttemptQuestion 
                    { 
                        OrderIndex = 1,
                        Question = new QuizQuestion 
                        { 
                            QuestionId = 101, 
                            QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 1, IsCorrect = true, OrderIndex = 1 } }
                        }
                    },
                    new QuizAttemptQuestion 
                    { 
                        OrderIndex = 2,
                        Question = new QuizQuestion 
                        { 
                            QuestionId = 102, 
                            QuizOptions = new List<QuizOption> { new QuizOption { OptionId = 2, IsCorrect = true, OrderIndex = 1 } }
                        }
                    }
                },
                QuizAttemptAnswers = new List<QuizAttemptAnswer>
                {
                    new QuizAttemptAnswer { QuestionId = 101, SelectedOptionId = 1 }, // Correct
                    new QuizAttemptAnswer { QuestionId = 102, SelectedOptionId = 3 }  // Wrong/Not found
                }
            };
            _quizRepoMock.GetAttemptByIdAsync(1).Returns(attempt);
            _quizRepoMock.GetByIdAsync(10).Returns(new Quiz { Title = "Q" });

            var result = await _sut.GetAttemptDetailAsync(1, 2);

            result.CorrectCount.Should().Be(1);
            result.Questions[0].IsCorrect.Should().BeTrue();
            result.Questions[1].IsCorrect.Should().BeFalse();
            result.TotalQuestions.Should().Be(2);
        }

        #endregion
    
    }
}
