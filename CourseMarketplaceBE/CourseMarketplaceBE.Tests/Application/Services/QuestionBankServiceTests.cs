using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using NSubstitute;
using FluentAssertions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class QuestionBankServiceTests
{
    private readonly IQuestionBankRepository _questionBankRepositoryMock;
    private readonly ICourseRepository _courseRepositoryMock;
    private readonly ILessonRepository _lessonRepositoryMock;
    private readonly QuestionBankService _sut;

    public QuestionBankServiceTests()
    {
        _questionBankRepositoryMock = Substitute.For<IQuestionBankRepository>();
        _courseRepositoryMock = Substitute.For<ICourseRepository>();
        _lessonRepositoryMock = Substitute.For<ILessonRepository>();
        _sut = new QuestionBankService(_questionBankRepositoryMock, _courseRepositoryMock, _lessonRepositoryMock);
    }

    [Fact]
    public async Task AddQuestionAsync_WithLessThanTwoOptions_ThrowsArgumentException()
    {
        // Arrange 1
        int courseId = 1;
        int instructorId = 100;
        var request = new QuizAddQuestionRequest
        {
            LessonId = 10,
            QuestionText = "Sample Question",
            Options = new List<QuizOptionRequest>
            {
                new QuizOptionRequest { OptionText = "Option A", IsCorrect = true }
            }
        };

        // Arrange 2
        // No mocks needed before exception is thrown

        // Act
        Func<Task> act = async () => await _sut.AddQuestionAsync(courseId, request, instructorId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A question must have at least 2 options.");
            
        await _courseRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdAsync(default);
    }

    [Fact]
    public async Task AddQuestionAsync_WithNoCorrectOption_ThrowsArgumentException()
    {
        // Arrange 1
        int courseId = 1;
        int instructorId = 100;
        var request = new QuizAddQuestionRequest
        {
            LessonId = 10,
            QuestionText = "Sample Question",
            Options = new List<QuizOptionRequest>
            {
                new QuizOptionRequest { OptionText = "Option A", IsCorrect = false },
                new QuizOptionRequest { OptionText = "Option B", IsCorrect = false }
            }
        };

        // Arrange 2
        // No mocks needed before exception is thrown

        // Act
        Func<Task> act = async () => await _sut.AddQuestionAsync(courseId, request, instructorId);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("A question must have at least 1 correct option.");

        await _courseRepositoryMock.DidNotReceiveWithAnyArgs().GetByIdAsync(default);
    }

    [Fact]
    public async Task AddQuestionAsync_WithValidOptions_SavesSuccessfully()
    {
        // Arrange 1
        int courseId = 1;
        int instructorId = 100;
        var request = new QuizAddQuestionRequest
        {
            LessonId = 10,
            QuestionText = "Sample Question",
            QuestionType = CourseMarketplaceBE.Domain.Enums.QuizQuestionType.MultiChoice,
            Options = new List<QuizOptionRequest>
            {
                new QuizOptionRequest { OptionText = "Option A", IsCorrect = true },
                new QuizOptionRequest { OptionText = "Option B", IsCorrect = false }
            }
        };
        
        var courseMock = new Course { CourseId = courseId, InstructorId = instructorId };
        var createdQuestion = new QuizQuestion { QuestionId = 50, QuestionText = "Sample Question" };

        // Arrange 2
        _courseRepositoryMock.GetByIdAsync(courseId).Returns(courseMock);
        _questionBankRepositoryMock.AddQuestionAsync(Arg.Any<QuizQuestion>()).Returns(createdQuestion);

        // Act
        var result = await _sut.AddQuestionAsync(courseId, request, instructorId);

        // Assert
        result.Should().NotBeNull();
        result.QuestionId.Should().Be(createdQuestion.QuestionId);
        result.QuestionText.Should().Be(createdQuestion.QuestionText);

        await _courseRepositoryMock.Received(1).GetByIdAsync(courseId);
        await _questionBankRepositoryMock.Received(1).AddQuestionAsync(Arg.Is<QuizQuestion>(q => 
            q.CourseId == courseId && 
            q.LessonId == request.LessonId && 
            q.QuestionText == request.QuestionText &&
            q.QuizOptions.Count == 2));
    }
}
