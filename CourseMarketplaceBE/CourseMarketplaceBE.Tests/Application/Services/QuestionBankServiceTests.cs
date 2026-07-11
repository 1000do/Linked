using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Enums;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class QuestionBankServiceTests
    {
        private readonly IQuestionBankRepository _questionBankRepoMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly ILessonRepository _lessonRepoMock;
        private readonly QuestionBankService _sut;

        public QuestionBankServiceTests()
        {
            _questionBankRepoMock = Substitute.For<IQuestionBankRepository>();
            _courseRepoMock = Substitute.For<ICourseRepository>();
            _lessonRepoMock = Substitute.For<ILessonRepository>();

            _sut = new QuestionBankService(
                _questionBankRepoMock,
                _courseRepoMock,
                _lessonRepoMock
            );
        }

        #region AddQuestionAsync

        [Fact]
        public async Task AddQuestionAsync_OptionsNullOrLessThanTwo_ThrowsArgumentException()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            var request = new QuizAddQuestionRequest
            {
                Options = new List<QuizOptionRequest> { new QuizOptionRequest { IsCorrect = true } }
            };

            // 2. Act
            Func<Task> act = async () => await _sut.AddQuestionAsync(courseId, request, instructorId);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("A question must have at least 2 options.");
            
            // 4. Verify
            await _courseRepoMock.DidNotReceiveWithAnyArgs().GetByIdAsync(default);
        }

        [Fact]
        public async Task AddQuestionAsync_NoCorrectOption_ThrowsArgumentException()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            var request = new QuizAddQuestionRequest
            {
                Options = new List<QuizOptionRequest>
                {
                    new QuizOptionRequest { IsCorrect = false },
                    new QuizOptionRequest { IsCorrect = false }
                }
            };

            // 2. Act
            Func<Task> act = async () => await _sut.AddQuestionAsync(courseId, request, instructorId);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>()
                .WithMessage("A question must have at least 1 correct option.");
        }

        [Fact]
        public async Task AddQuestionAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            var request = new QuizAddQuestionRequest
            {
                Options = new List<QuizOptionRequest>
                {
                    new QuizOptionRequest { IsCorrect = true },
                    new QuizOptionRequest { IsCorrect = false }
                }
            };

            _courseRepoMock.GetByIdAsync(courseId).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.AddQuestionAsync(courseId, request, instructorId);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>()
                .WithMessage("You do not have permission to add questions to this course.");
        }

        [Fact]
        public async Task AddQuestionAsync_ValidRequest_AddsQuestionAndReturnsResponse()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            var request = new QuizAddQuestionRequest
            {
                LessonId = 10,
                QuestionText = "Test",
                Explanation = "Expl",
                QuestionType = QuizQuestionType.SingleChoice,
                Options = new List<QuizOptionRequest>
                {
                    new QuizOptionRequest { OptionText = "A", IsCorrect = true },
                    new QuizOptionRequest { OptionText = "B", IsCorrect = false }
                }
            };

            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, InstructorId = instructorId });
            
            _questionBankRepoMock.AddQuestionAsync(Arg.Any<QuizQuestion>()).Returns(callInfo =>
            {
                var q = callInfo.Arg<QuizQuestion>();
                q.QuestionId = 99;
                return q;
            });

            // 2. Act
            var result = await _sut.AddQuestionAsync(courseId, request, instructorId);

            // 3. Assert
            result.Should().NotBeNull();
            result.QuestionId.Should().Be(99);
            result.CourseId.Should().Be(courseId);
            result.Options.Should().HaveCount(2);
            result.Options[0].OrderIndex.Should().Be(0);

            // 4. Verify
            await _questionBankRepoMock.Received(1).AddQuestionAsync(Arg.Any<QuizQuestion>());
        }

        #endregion

        #region UpdateQuestionAsync

        [Fact]
        public async Task UpdateQuestionAsync_OptionsLessThanTwo_ThrowsArgumentException()
        {
            // 1. Arrange
            var request = new QuizUpdateQuestionRequest
            {
                Options = new List<QuizUpdateOptionRequest> { new QuizUpdateOptionRequest { IsCorrect = true } }
            };

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuestionAsync(1, request, 2);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateQuestionAsync_NoCorrectOption_ThrowsArgumentException()
        {
            // 1. Arrange
            var request = new QuizUpdateQuestionRequest
            {
                Options = new List<QuizUpdateOptionRequest>
                {
                    new QuizUpdateOptionRequest { IsCorrect = false },
                    new QuizUpdateOptionRequest { IsCorrect = false }
                }
            };

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuestionAsync(1, request, 2);

            // 3. Assert
            await act.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task UpdateQuestionAsync_QuestionNotFound_ThrowsKeyNotFoundException()
        {
            // 1. Arrange
            var questionId = 1;
            var request = new QuizUpdateQuestionRequest
            {
                Options = new List<QuizUpdateOptionRequest>
                {
                    new QuizUpdateOptionRequest { IsCorrect = true },
                    new QuizUpdateOptionRequest { IsCorrect = false }
                }
            };

            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(Task.FromResult<QuizQuestion?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuestionAsync(questionId, request, 2);

            // 3. Assert
            await act.Should().ThrowAsync<KeyNotFoundException>()
                .WithMessage("Question not found.");
        }

        [Fact]
        public async Task UpdateQuestionAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var questionId = 1;
            var request = new QuizUpdateQuestionRequest
            {
                Options = new List<QuizUpdateOptionRequest>
                {
                    new QuizUpdateOptionRequest { IsCorrect = true },
                    new QuizUpdateOptionRequest { IsCorrect = false }
                }
            };
            
            var existingQuestion = new QuizQuestion { CourseId = 100 };
            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(existingQuestion);
            
            _courseRepoMock.GetByIdAsync(100).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.UpdateQuestionAsync(questionId, request, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task UpdateQuestionAsync_ValidRequest_ClearsOldOptionsAndUpdates()
        {
            // 1. Arrange
            var questionId = 1;
            var instructorId = 2;
            var request = new QuizUpdateQuestionRequest
            {
                LessonId = 10,
                QuestionText = "New Text",
                Options = new List<QuizUpdateOptionRequest>
                {
                    new QuizUpdateOptionRequest { OptionText = "X", IsCorrect = true },
                    new QuizUpdateOptionRequest { OptionText = "Y", IsCorrect = false }
                }
            };
            
            var existingQuestion = new QuizQuestion 
            { 
                CourseId = 100, 
                QuizOptions = new List<QuizOption> { new QuizOption { OptionText = "Old" } } 
            };
            
            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(existingQuestion);
            _courseRepoMock.GetByIdAsync(100).Returns(new Course { CourseId = 100, InstructorId = instructorId });
            _questionBankRepoMock.UpdateQuestionAsync(Arg.Any<QuizQuestion>()).Returns(existingQuestion);

            // 2. Act
            var result = await _sut.UpdateQuestionAsync(questionId, request, instructorId);

            // 3. Assert
            result.QuestionText.Should().Be("New Text");
            result.Options.Should().HaveCount(2);
            result.Options.Should().Contain(o => o.OptionText == "X");

            // 4. Verify
            await _questionBankRepoMock.Received(1).UpdateQuestionAsync(existingQuestion);
        }

        #endregion

        #region DeleteQuestionAsync

        [Fact]
        public async Task DeleteQuestionAsync_QuestionNotFound_ReturnsSilently()
        {
            // 1. Arrange
            var questionId = 1;
            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(Task.FromResult<QuizQuestion?>(null));

            // 2. Act
            await _sut.DeleteQuestionAsync(questionId, 2);

            // 3. Assert (Does not throw)
            // 4. Verify
            await _questionBankRepoMock.DidNotReceive().DeleteQuestionAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task DeleteQuestionAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var questionId = 1;
            var existingQuestion = new QuizQuestion { CourseId = 100 };
            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(existingQuestion);
            _courseRepoMock.GetByIdAsync(100).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.DeleteQuestionAsync(questionId, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task DeleteQuestionAsync_ValidRequest_DeletesQuestion()
        {
            // 1. Arrange
            var questionId = 1;
            var instructorId = 2;
            var existingQuestion = new QuizQuestion { CourseId = 100 };
            _questionBankRepoMock.GetQuestionByIdAsync(questionId).Returns(existingQuestion);
            _courseRepoMock.GetByIdAsync(100).Returns(new Course { CourseId = 100, InstructorId = instructorId });

            // 2. Act
            await _sut.DeleteQuestionAsync(questionId, instructorId);

            // 3. Assert (Does not throw)
            // 4. Verify
            await _questionBankRepoMock.Received(1).DeleteQuestionAsync(questionId);
        }

        #endregion

        #region GetQuestionsByCourseAsync

        [Fact]
        public async Task GetQuestionsByCourseAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var courseId = 1;
            _courseRepoMock.GetByIdAsync(courseId).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.GetQuestionsByCourseAsync(courseId, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetQuestionsByCourseAsync_ValidRequest_ReturnsMappedQuestions()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, InstructorId = instructorId });
            
            var questions = new List<QuizQuestion> 
            { 
                new QuizQuestion { QuestionId = 10, QuizOptions = new List<QuizOption>() } 
            };
            _questionBankRepoMock.GetQuestionsByCourseAsync(courseId).Returns(questions);

            // 2. Act
            var result = await _sut.GetQuestionsByCourseAsync(courseId, instructorId);

            // 3. Assert
            result.Should().HaveCount(1);
            result.First().QuestionId.Should().Be(10);
        }

        #endregion

        #region GetQuestionsByLessonAsync

        [Fact]
        public async Task GetQuestionsByLessonAsync_EmptyQuestions_ReturnsEmptyList()
        {
            // 1. Arrange
            var lessonId = 1;
            _questionBankRepoMock.GetQuestionsByLessonAsync(lessonId).Returns(new List<QuizQuestion>());

            // 2. Act
            var result = await _sut.GetQuestionsByLessonAsync(lessonId, 2);

            // 3. Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetQuestionsByLessonAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var lessonId = 1;
            var questions = new List<QuizQuestion> { new QuizQuestion { CourseId = 100 } };
            _questionBankRepoMock.GetQuestionsByLessonAsync(lessonId).Returns(questions);
            _courseRepoMock.GetByIdAsync(100).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.GetQuestionsByLessonAsync(lessonId, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetQuestionsByLessonAsync_ValidRequest_ReturnsMappedQuestions()
        {
            // 1. Arrange
            var lessonId = 1;
            var instructorId = 2;
            var courseId = 100;
            var questions = new List<QuizQuestion> 
            { 
                new QuizQuestion { CourseId = courseId, QuizOptions = new List<QuizOption>() } 
            };
            _questionBankRepoMock.GetQuestionsByLessonAsync(lessonId).Returns(questions);
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, InstructorId = instructorId });

            // 2. Act
            var result = await _sut.GetQuestionsByLessonAsync(lessonId, instructorId);

            // 3. Assert
            result.Should().HaveCount(1);
        }

        #endregion

        #region GetLessonsSummaryByCourseAsync

        [Fact]
        public async Task GetLessonsSummaryByCourseAsync_CourseNotFoundOrNotOwner_ThrowsUnauthorizedAccessException()
        {
            // 1. Arrange
            var courseId = 1;
            _courseRepoMock.GetByIdAsync(courseId).Returns(Task.FromResult<Course?>(null));

            // 2. Act
            Func<Task> act = async () => await _sut.GetLessonsSummaryByCourseAsync(courseId, 2);

            // 3. Assert
            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task GetLessonsSummaryByCourseAsync_ValidRequest_FiltersRemovedLessonsAndReturnsSummary()
        {
            // 1. Arrange
            var courseId = 1;
            var instructorId = 2;
            _courseRepoMock.GetByIdAsync(courseId).Returns(new Course { CourseId = courseId, InstructorId = instructorId });

            var lessons = new List<Lesson>
            {
                new Lesson { LessonId = 10, Title = "L1", IsRemoved = false },
                new Lesson { LessonId = 11, Title = "L2", IsRemoved = true } // Should be skipped
            };
            _lessonRepoMock.GetByCourseIdAsync(courseId).Returns(lessons);

            _questionBankRepoMock.GetQuestionsByLessonAsync(10).Returns(new List<QuizQuestion> { new QuizQuestion(), new QuizQuestion() });

            // 2. Act
            var result = await _sut.GetLessonsSummaryByCourseAsync(courseId, instructorId);

            // 3. Assert
            result.Should().HaveCount(1);
            result.First().LessonId.Should().Be(10);
            result.First().AvailableQuestionCount.Should().Be(2);
        }

        #endregion
    }
}
