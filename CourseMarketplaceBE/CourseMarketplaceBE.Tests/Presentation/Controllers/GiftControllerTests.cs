using System;
using System.Security.Claims;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Presentation.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Presentation.Controllers;

public class GiftControllerTests
{
    private readonly IGiftService _giftServiceMock;
    private readonly ICourseQueryService _courseQueryServiceMock;
    private readonly GiftController _sut;

    public GiftControllerTests()
    {
        _giftServiceMock = Substitute.For<IGiftService>();
        _courseQueryServiceMock = Substitute.For<ICourseQueryService>();
        _sut = new GiftController(_giftServiceMock, _courseQueryServiceMock);
    }

    private void SetUserContext(int? userId, string role)
    {
        var claims = new System.Collections.Generic.List<Claim>();
        if (userId.HasValue)
            claims.Add(new Claim(ClaimTypes.NameIdentifier, userId.Value.ToString()));
        if (!string.IsNullOrEmpty(role))
            claims.Add(new Claim(ClaimTypes.Role, role));

        var user = new ClaimsPrincipal(new ClaimsIdentity(claims, "mock"));
        _sut.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetCourseDetails_CourseExists_ReturnsOkResult()
    {
        // Arrange
        int courseId = 1;
        SetUserContext(1, "student");
        var courseDetail = new CourseDetailResponse { CourseId = courseId, Title = "Test Course" };
        _courseQueryServiceMock.GetCourseWithDetailsAsync(courseId, 1, "student").Returns(courseDetail);

        // Act
        var result = await _sut.GetCourseDetails(courseId) as OkObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(200);
        var apiResponse = result.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Data.Should().BeEquivalentTo(courseDetail);
        apiResponse.Message.Should().Be("Retrieved course successfully.");
    }

    [Fact]
    public async Task GetCourseDetails_CourseNotFound_ReturnsNotFoundResult()
    {
        // Arrange
        int courseId = 1;
        SetUserContext(null, null);
        _courseQueryServiceMock
            .When(x => x.GetCourseWithDetailsAsync(courseId, null, null))
            .Throw(new System.Collections.Generic.KeyNotFoundException("Course not found."));

        // Act
        var result = await _sut.GetCourseDetails(courseId) as NotFoundObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(404);
        var apiResponse = result.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Message.Should().Be("Course not found.");
    }

    [Fact]
    public async Task GetCourseDetails_UnauthorizedAccess_ReturnsForbiddenResult()
    {
        // Arrange
        int courseId = 1;
        SetUserContext(1, "student");
        _courseQueryServiceMock
            .When(x => x.GetCourseWithDetailsAsync(courseId, 1, "student"))
            .Throw(new UnauthorizedAccessException("You do not have permission to view this course."));

        // Act
        var result = await _sut.GetCourseDetails(courseId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(403);
        var apiResponse = result.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Message.Should().Be("You do not have permission to view this course.");
    }

    [Fact]
    public async Task GetCourseDetails_Exception_ReturnsInternalServerErrorResult()
    {
        // Arrange
        int courseId = 1;
        SetUserContext(null, null);
        _courseQueryServiceMock
            .When(x => x.GetCourseWithDetailsAsync(courseId, null, null))
            .Throw(new Exception("Database error."));

        // Act
        var result = await _sut.GetCourseDetails(courseId) as ObjectResult;

        // Assert
        result.Should().NotBeNull();
        result!.StatusCode.Should().Be(500);
        var apiResponse = result.Value as ApiResponse<object>;
        apiResponse.Should().NotBeNull();
        apiResponse!.Message.Should().Be("Database error.");
    }
}
