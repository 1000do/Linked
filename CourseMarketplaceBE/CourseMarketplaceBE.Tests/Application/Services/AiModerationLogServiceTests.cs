using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Exceptions;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Exceptions;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class AiModerationLogServiceTests
{
    private readonly ICourseAiUsageLogRepository _courseLogRepoMock;
    private readonly ICourseReviewModerationLogRepository _courseReviewLogRepoMock;
    private readonly ILessonReviewModerationLogRepository _lessonReviewLogRepoMock;
    private readonly IMapper _mapperMock;
    private readonly AiModerationLogService _sut;

    public AiModerationLogServiceTests()
    {
        _courseLogRepoMock = Substitute.For<ICourseAiUsageLogRepository>();
        _courseReviewLogRepoMock = Substitute.For<ICourseReviewModerationLogRepository>();
        _lessonReviewLogRepoMock = Substitute.For<ILessonReviewModerationLogRepository>();
        _mapperMock = Substitute.For<IMapper>();

        _sut = new AiModerationLogService(
            _courseLogRepoMock,
            _courseReviewLogRepoMock,
            _lessonReviewLogRepoMock,
            _mapperMock
        );
    }

    // --- GetCourseModerationLogsAsync ---
    [Fact]
    public async Task GetCourseModerationLogsAsync_PageAndPageSizeValid_ReturnsPagedResult()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<CourseAiUsageLog> { new CourseAiUsageLog() };
        var totalCount = 10;
        var dtos = new List<CourseModerationLogAdminDto> { new CourseModerationLogAdminDto() };
        
        //Arrange 2
        _courseLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<CourseModerationLogAdminDto>>(items).Returns(dtos);

        //Act
        var result = await _sut.GetCourseModerationLogsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        await _courseLogRepoMock.Received(1).GetPagedAdminAsync(2, 5);
        _mapperMock.Received(1).Map<List<CourseModerationLogAdminDto>>(items);
    }

    [Fact]
    public async Task GetCourseModerationLogsAsync_PageIsZero_DefaultsToPageOne()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 0, PageSize = 5 };
        var items = new List<CourseAiUsageLog> { new CourseAiUsageLog() };
        var totalCount = 10;
        
        //Arrange 2
        _courseLogRepoMock.GetPagedAdminAsync(1, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<CourseModerationLogAdminDto>>(items).Returns(new List<CourseModerationLogAdminDto>());

        //Act
        var result = await _sut.GetCourseModerationLogsAsync(req);

        //Assert
        result.Page.Should().Be(1);
        await _courseLogRepoMock.Received(1).GetPagedAdminAsync(1, 5);
    }

    [Fact]
    public async Task GetCourseModerationLogsAsync_PageSizeIsZero_DefaultsToPageSizeTen()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 0 };
        var items = new List<CourseAiUsageLog> { new CourseAiUsageLog() };
        var totalCount = 10;
        
        //Arrange 2
        _courseLogRepoMock.GetPagedAdminAsync(req.Page, 10).Returns((items, totalCount));
        _mapperMock.Map<List<CourseModerationLogAdminDto>>(items).Returns(new List<CourseModerationLogAdminDto>());

        //Act
        var result = await _sut.GetCourseModerationLogsAsync(req);

        //Assert
        result.PageSize.Should().Be(10);
        await _courseLogRepoMock.Received(1).GetPagedAdminAsync(2, 10);
    }

    [Fact]
    public async Task GetCourseModerationLogsAsync_ItemsIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        
        //Arrange 2
        _courseLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns(((List<CourseAiUsageLog>)null!, 0));

        //Act
        Func<Task> act = async () => await _sut.GetCourseModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No course moderation logs found.");
    }

    [Fact]
    public async Task GetCourseModerationLogsAsync_ItemsIsEmpty_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<CourseAiUsageLog>();
        
        //Arrange 2
        _courseLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, 0));

        //Act
        Func<Task> act = async () => await _sut.GetCourseModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No course moderation logs found.");
    }

    // --- GetCourseModerationLogDetailAsync ---
    [Fact]
    public async Task GetCourseModerationLogDetailAsync_EntityExists_ReturnsMappedDto()
    {
        //Arrange 1
        int logId = 5;
        var entity = new CourseAiUsageLog();
        var dto = new CourseModerationLogAdminDto();

        //Arrange 2
        _courseLogRepoMock.GetAdminDetailByIdAsync(logId).Returns(entity);
        _mapperMock.Map<CourseModerationLogAdminDto>(entity).Returns(dto);

        //Act
        var result = await _sut.GetCourseModerationLogDetailAsync(logId);

        //Assert
        result.Should().Be(dto);
        await _courseLogRepoMock.Received(1).GetAdminDetailByIdAsync(logId);
        _mapperMock.Received(1).Map<CourseModerationLogAdminDto>(entity);
    }

    [Fact]
    public async Task GetCourseModerationLogDetailAsync_EntityIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int logId = 5;

        //Arrange 2
        _courseLogRepoMock.GetAdminDetailByIdAsync(logId).Returns((CourseAiUsageLog)null!);

        //Act
        Func<Task> act = async () => await _sut.GetCourseModerationLogDetailAsync(logId);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("Log not found.");
    }

    // --- GetCourseReviewModerationLogsAsync ---
    [Fact]
    public async Task GetCourseReviewModerationLogsAsync_PageAndPageSizeValid_ReturnsPagedResult()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<CourseReviewModerationLog> { new CourseReviewModerationLog() };
        var totalCount = 10;
        var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto() };
        
        //Arrange 2
        _courseReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(dtos);

        //Act
        var result = await _sut.GetCourseReviewModerationLogsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(2, 5);
        _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(items);
    }

    [Fact]
    public async Task GetCourseReviewModerationLogsAsync_PageIsZero_DefaultsToPageOne()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 0, PageSize = 5 };
        var items = new List<CourseReviewModerationLog> { new CourseReviewModerationLog() };
        var totalCount = 10;
        
        //Arrange 2
        _courseReviewLogRepoMock.GetPagedAdminAsync(1, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(new List<ReviewModerationLogAdminDto>());

        //Act
        var result = await _sut.GetCourseReviewModerationLogsAsync(req);

        //Assert
        result.Page.Should().Be(1);
        await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(1, 5);
    }

    [Fact]
    public async Task GetCourseReviewModerationLogsAsync_PageSizeIsZero_DefaultsToPageSizeTen()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 0 };
        var items = new List<CourseReviewModerationLog> { new CourseReviewModerationLog() };
        var totalCount = 10;
        
        //Arrange 2
        _courseReviewLogRepoMock.GetPagedAdminAsync(req.Page, 10).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(new List<ReviewModerationLogAdminDto>());

        //Act
        var result = await _sut.GetCourseReviewModerationLogsAsync(req);

        //Assert
        result.PageSize.Should().Be(10);
        await _courseReviewLogRepoMock.Received(1).GetPagedAdminAsync(2, 10);
    }

    [Fact]
    public async Task GetCourseReviewModerationLogsAsync_ItemsIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        
        //Arrange 2
        _courseReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns(((List<CourseReviewModerationLog>)null!, 0));

        //Act
        Func<Task> act = async () => await _sut.GetCourseReviewModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No course review moderation logs found.");
    }

    [Fact]
    public async Task GetCourseReviewModerationLogsAsync_ItemsIsEmpty_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<CourseReviewModerationLog>();
        
        //Arrange 2
        _courseReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, 0));

        //Act
        Func<Task> act = async () => await _sut.GetCourseReviewModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No course review moderation logs found.");
    }

    // --- GetCourseReviewModerationLogDetailAsync ---
    [Fact]
    public async Task GetCourseReviewModerationLogDetailAsync_EntityExists_ReturnsMappedDto()
    {
        //Arrange 1
        int logId = 5;
        var entity = new CourseReviewModerationLog();
        var dto = new ReviewModerationLogAdminDto();

        //Arrange 2
        _courseReviewLogRepoMock.GetAdminDetailByIdAsync(logId).Returns(entity);
        _mapperMock.Map<ReviewModerationLogAdminDto>(entity).Returns(dto);

        //Act
        var result = await _sut.GetCourseReviewModerationLogDetailAsync(logId);

        //Assert
        result.Should().Be(dto);
        await _courseReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(logId);
        _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(entity);
    }

    [Fact]
    public async Task GetCourseReviewModerationLogDetailAsync_EntityIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int logId = 5;

        //Arrange 2
        _courseReviewLogRepoMock.GetAdminDetailByIdAsync(logId).Returns((CourseReviewModerationLog)null!);

        //Act
        Func<Task> act = async () => await _sut.GetCourseReviewModerationLogDetailAsync(logId);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("Log not found.");
    }

    // --- GetLessonReviewModerationLogsAsync ---
    [Fact]
    public async Task GetLessonReviewModerationLogsAsync_PageAndPageSizeValid_ReturnsPagedResult()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<LessonReviewModerationLog> { new LessonReviewModerationLog() };
        var totalCount = 10;
        var dtos = new List<ReviewModerationLogAdminDto> { new ReviewModerationLogAdminDto() };
        
        //Arrange 2
        _lessonReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(dtos);

        //Act
        var result = await _sut.GetLessonReviewModerationLogsAsync(req);

        //Assert
        result.Items.Should().BeEquivalentTo(dtos);
        result.TotalCount.Should().Be(totalCount);
        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(2, 5);
        _mapperMock.Received(1).Map<List<ReviewModerationLogAdminDto>>(items);
    }

    [Fact]
    public async Task GetLessonReviewModerationLogsAsync_PageIsZero_DefaultsToPageOne()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 0, PageSize = 5 };
        var items = new List<LessonReviewModerationLog> { new LessonReviewModerationLog() };
        var totalCount = 10;
        
        //Arrange 2
        _lessonReviewLogRepoMock.GetPagedAdminAsync(1, req.PageSize).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(new List<ReviewModerationLogAdminDto>());

        //Act
        var result = await _sut.GetLessonReviewModerationLogsAsync(req);

        //Assert
        result.Page.Should().Be(1);
        await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(1, 5);
    }

    [Fact]
    public async Task GetLessonReviewModerationLogsAsync_PageSizeIsZero_DefaultsToPageSizeTen()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 0 };
        var items = new List<LessonReviewModerationLog> { new LessonReviewModerationLog() };
        var totalCount = 10;
        
        //Arrange 2
        _lessonReviewLogRepoMock.GetPagedAdminAsync(req.Page, 10).Returns((items, totalCount));
        _mapperMock.Map<List<ReviewModerationLogAdminDto>>(items).Returns(new List<ReviewModerationLogAdminDto>());

        //Act
        var result = await _sut.GetLessonReviewModerationLogsAsync(req);

        //Assert
        result.PageSize.Should().Be(10);
        await _lessonReviewLogRepoMock.Received(1).GetPagedAdminAsync(2, 10);
    }

    [Fact]
    public async Task GetLessonReviewModerationLogsAsync_ItemsIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        
        //Arrange 2
        _lessonReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns(((List<LessonReviewModerationLog>)null!, 0));

        //Act
        Func<Task> act = async () => await _sut.GetLessonReviewModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No lesson review moderation logs found.");
    }

    [Fact]
    public async Task GetLessonReviewModerationLogsAsync_ItemsIsEmpty_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        var req = new PagedRequestDto { Page = 2, PageSize = 5 };
        var items = new List<LessonReviewModerationLog>();
        
        //Arrange 2
        _lessonReviewLogRepoMock.GetPagedAdminAsync(req.Page, req.PageSize).Returns((items, 0));

        //Act
        Func<Task> act = async () => await _sut.GetLessonReviewModerationLogsAsync(req);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("No lesson review moderation logs found.");
    }

    // --- GetLessonReviewModerationLogDetailAsync ---
    [Fact]
    public async Task GetLessonReviewModerationLogDetailAsync_EntityExists_ReturnsMappedDto()
    {
        //Arrange 1
        int logId = 5;
        var entity = new LessonReviewModerationLog();
        var dto = new ReviewModerationLogAdminDto();

        //Arrange 2
        _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(logId).Returns(entity);
        _mapperMock.Map<ReviewModerationLogAdminDto>(entity).Returns(dto);

        //Act
        var result = await _sut.GetLessonReviewModerationLogDetailAsync(logId);

        //Assert
        result.Should().Be(dto);
        await _lessonReviewLogRepoMock.Received(1).GetAdminDetailByIdAsync(logId);
        _mapperMock.Received(1).Map<ReviewModerationLogAdminDto>(entity);
    }

    [Fact]
    public async Task GetLessonReviewModerationLogDetailAsync_EntityIsNull_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int logId = 5;

        //Arrange 2
        _lessonReviewLogRepoMock.GetAdminDetailByIdAsync(logId).Returns((LessonReviewModerationLog)null!);

        //Act
        Func<Task> act = async () => await _sut.GetLessonReviewModerationLogDetailAsync(logId);

        //Assert
        var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
        ex.WithMessage("Log not found.");
    }

    // --- SaveCourseAiUsageLog ---
    [Fact]
    public async Task SaveCourseAiUsageLog_ValidCommand_SavesAndReturnsRowsAffected()
    {
        //Arrange 1
        var cmd = new SaveCourseAiUsageLogCommand
        {
            IntegrationId = 1,
            InteractionType = "type",
            InputJson = "{}",
            OutputJson = "{}",
            LatencyMs = 100,
            TokenUsage = 50,
            ErrorMessage = "none"
        };
        int expectedRows = 1;

        //Arrange 2
        _courseLogRepoMock.SaveChangesAsync().Returns(expectedRows);

        //Act
        var result = await _sut.SaveCourseAiUsageLog(cmd);

        //Assert
        result.Should().Be(expectedRows);
        await _courseLogRepoMock.Received(1).AddAsync(Arg.Is<CourseAiUsageLog>(x => 
            x.IntegrationId == cmd.IntegrationId &&
            x.InteractionType == cmd.InteractionType &&
            x.InputJson == cmd.InputJson &&
            x.OutputJson == cmd.OutputJson &&
            x.LatencyMs == cmd.LatencyMs &&
            x.TokenUsage == cmd.TokenUsage &&
            x.ErrorMessage == cmd.ErrorMessage
        ));
        await _courseLogRepoMock.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task SaveCourseAiUsageLog_SaveThrowsCourseAiUsageLogException_ThrowsBadRequestException()
    {
        //Arrange 1
        var cmd = new SaveCourseAiUsageLogCommand();
        var exMsg = "save error";

        //Arrange 2
        _courseLogRepoMock.SaveChangesAsync().Throws(new CourseAiUsageLogException(exMsg));

        //Act
        Func<Task> act = async () => await _sut.SaveCourseAiUsageLog(cmd);

        //Assert
        var ex = await act.Should().ThrowAsync<BadRequestException>();
        ex.WithMessage(exMsg);
        await _courseLogRepoMock.Received(1).AddAsync(Arg.Any<CourseAiUsageLog>());
        await _courseLogRepoMock.Received(1).SaveChangesAsync();
    }
}
