using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Hubs;
using CourseMarketplaceBE.Domain.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.SignalR;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class WishlistServiceTests
    {
        private readonly IWishlistRepository _wishlistRepositoryMock;
        private readonly IHubContext<NotificationHub> _hubContextMock;
        private readonly ICourseRepository _courseRepositoryMock;
        private readonly IHubClients _hubClientsMock;
        private readonly IClientProxy _clientProxyMock;
        private readonly WishlistService _sut;

        public WishlistServiceTests()
        {
            _wishlistRepositoryMock = Substitute.For<IWishlistRepository>();
            _hubContextMock = Substitute.For<IHubContext<NotificationHub>>();
            _courseRepositoryMock = Substitute.For<ICourseRepository>();

            _hubClientsMock = Substitute.For<IHubClients>();
            _clientProxyMock = Substitute.For<IClientProxy>();
            
            _hubContextMock.Clients.Returns(_hubClientsMock);
            _hubClientsMock.User(Arg.Any<string>()).Returns(_clientProxyMock);

            _sut = new WishlistService(_wishlistRepositoryMock, _hubContextMock, _courseRepositoryMock);
        }

        [Fact]
        public async Task GetWishlistAsync_NoItemsFound_ReturnsEmptyList()
        {
            //Arrange 1
            var userId = 1;
            var emptyList = new List<WishlistItem>();

            //Arrange 2
            _wishlistRepositoryMock.GetByUserIdAsync(userId).Returns(emptyList);

            //Act
            var result = await _sut.GetWishlistAsync(userId);

            //Assert
            result.Should().BeEmpty();
            await _wishlistRepositoryMock.Received(1).GetByUserIdAsync(userId);
            await _courseRepositoryMock.DidNotReceive().IsEnrolledAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task GetWishlistAsync_ItemsFoundAndStatsAreNull_MapsAndReturnsResponses()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var items = new List<WishlistItem>
            {
                new WishlistItem
                {
                    Id = 100,
                    CourseId = courseId,
                    AddedDate = DateTime.UtcNow,
                    Course = new Course 
                    { 
                        Title = "Test Course", 
                        Price = 10.5m,
                        Instructor = new Instructor { InstructorNavigation = new User { FullName = "John Doe" } }
                    }
                }
            };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserIdAsync(userId).Returns(items);
            _courseRepositoryMock.IsEnrolledAsync(userId, courseId).Returns(true);
            _courseRepositoryMock.GetCourseStatsAsync(courseId).Returns((CourseStats?)null); // Stats null

            //Act
            var result = await _sut.GetWishlistAsync(userId);

            //Assert
            result.Should().HaveCount(1);
            var response = result[0];
            response.RatingAverage.Should().Be(4.5m); // Fallback
            response.IsEnrolled.Should().BeTrue();
            response.Title.Should().Be("Test Course");
            
            await _wishlistRepositoryMock.Received(1).GetByUserIdAsync(userId);
            await _courseRepositoryMock.Received(1).IsEnrolledAsync(userId, courseId);
            await _courseRepositoryMock.Received(1).GetCourseStatsAsync(courseId);
        }

        [Fact]
        public async Task GetWishlistAsync_ItemsFoundAndStatsAreNotNull_MapsAndReturnsResponses()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var items = new List<WishlistItem>
            {
                new WishlistItem
                {
                    Id = 100,
                    CourseId = courseId,
                    Course = null // Testing null propagation in mapping
                }
            };
            var stats = new CourseStats { RatingAverage = 4.8, TotalStudents = 50 };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserIdAsync(userId).Returns(items);
            _courseRepositoryMock.IsEnrolledAsync(userId, courseId).Returns(false);
            _courseRepositoryMock.GetCourseStatsAsync(courseId).Returns(stats);

            //Act
            var result = await _sut.GetWishlistAsync(userId);

            //Assert
            result.Should().HaveCount(1);
            var response = result[0];
            response.RatingAverage.Should().Be(4.8m);
            response.TotalStudents.Should().Be(50);
            response.Title.Should().Be(""); // From null propagation fallback
            response.InstructorName.Should().Be("Unknown");
            
            await _wishlistRepositoryMock.Received(1).GetByUserIdAsync(userId);
        }

        [Fact]
        public async Task AddToWishlistAsync_CourseDoesNotExist_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;

            //Arrange 2
            _courseRepositoryMock.GetByIdAsync(courseId).Returns((Course?)null);

            //Act
            Func<Task> act = async () => await _sut.AddToWishlistAsync(userId, courseId);

            //Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.WithMessage("Course does not exist or is not published.");
            await _wishlistRepositoryMock.DidNotReceive().ExistsAsync(Arg.Any<int>(), Arg.Any<int>());
        }

        [Fact]
        public async Task AddToWishlistAsync_CourseIsNotPublished_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var course = new Course { CourseStatus = CourseStatus.Draft.ToValue() };

            //Arrange 2
            _courseRepositoryMock.GetByIdAsync(courseId).Returns(course);

            //Act
            Func<Task> act = async () => await _sut.AddToWishlistAsync(userId, courseId);

            //Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.WithMessage("Course does not exist or is not published.");
        }

        [Fact]
        public async Task AddToWishlistAsync_CourseAlreadyInWishlist_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var course = new Course { CourseStatus = CourseStatus.Published.ToValue() };

            //Arrange 2
            _courseRepositoryMock.GetByIdAsync(courseId).Returns(course);
            _wishlistRepositoryMock.ExistsAsync(userId, courseId).Returns(true);

            //Act
            Func<Task> act = async () => await _sut.AddToWishlistAsync(userId, courseId);

            //Assert
            var ex = await act.Should().ThrowAsync<InvalidOperationException>();
            ex.WithMessage("Course is already in your wishlist.");
            await _wishlistRepositoryMock.DidNotReceive().AddAsync(Arg.Any<WishlistItem>());
        }

        [Fact]
        public async Task AddToWishlistAsync_ValidCourseAndNotYetAdded_AddsToWishlist()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var course = new Course { CourseStatus = CourseStatus.Published.ToValue() };

            //Arrange 2
            _courseRepositoryMock.GetByIdAsync(courseId).Returns(course);
            _wishlistRepositoryMock.ExistsAsync(userId, courseId).Returns(false);

            //Act
            await _sut.AddToWishlistAsync(userId, courseId);

            //Assert
            await _wishlistRepositoryMock.Received(1).AddAsync(Arg.Is<WishlistItem>(x => x.UserId == userId && x.CourseId == courseId));
            await _wishlistRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_ItemNotFound_ThrowsKeyNotFoundException()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;

            //Arrange 2
            _wishlistRepositoryMock.GetByUserAndCourseAsync(userId, courseId).Returns((WishlistItem?)null);

            //Act
            Func<Task> act = async () => await _sut.RemoveFromWishlistAsync(userId, courseId);

            //Assert
            var ex = await act.Should().ThrowAsync<KeyNotFoundException>();
            ex.WithMessage("Course not found in your wishlist.");
            await _wishlistRepositoryMock.DidNotReceive().RemoveAsync(Arg.Any<WishlistItem>());
        }

        [Fact]
        public async Task RemoveFromWishlistAsync_ItemFound_RemovesFromWishlist()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var item = new WishlistItem { Id = 100 };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserAndCourseAsync(userId, courseId).Returns(item);

            //Act
            await _sut.RemoveFromWishlistAsync(userId, courseId);

            //Assert
            await _wishlistRepositoryMock.Received(1).RemoveAsync(item);
            await _wishlistRepositoryMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ToggleWishlistAsync_ItemExists_RemovesItemAndNotifiesAndReturnsFalse()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var item = new WishlistItem { Id = 100 };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserAndCourseAsync(userId, courseId).Returns(item);

            //Act
            var result = await _sut.ToggleWishlistAsync(userId, courseId);

            //Assert
            result.Should().BeFalse();
            await _wishlistRepositoryMock.Received(1).RemoveAsync(item);
            await _wishlistRepositoryMock.Received(1).SaveChangesAsync();
            
            _hubClientsMock.Received(1).User(userId.ToString());
            await _clientProxyMock.Received(1).SendCoreAsync("UpdateWishlistCount", Arg.Is<object[]>(x => x.Length == 0), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task ToggleWishlistAsync_ItemDoesNotExistAndCourseInvalid_ThrowsInvalidOperationException()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;

            //Arrange 2
            _wishlistRepositoryMock.GetByUserAndCourseAsync(userId, courseId).Returns((WishlistItem?)null);
            _courseRepositoryMock.GetByIdAsync(courseId).Returns((Course?)null); // Invalid course

            //Act
            Func<Task> act = async () => await _sut.ToggleWishlistAsync(userId, courseId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>();
            await _wishlistRepositoryMock.DidNotReceive().AddAsync(Arg.Any<WishlistItem>());
        }

        [Fact]
        public async Task ToggleWishlistAsync_ItemDoesNotExistAndCourseValid_AddsItemAndNotifiesAndReturnsTrue()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;
            var course = new Course { CourseStatus = CourseStatus.Published.ToValue() };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserAndCourseAsync(userId, courseId).Returns((WishlistItem?)null);
            _courseRepositoryMock.GetByIdAsync(courseId).Returns(course);

            //Act
            var result = await _sut.ToggleWishlistAsync(userId, courseId);

            //Assert
            result.Should().BeTrue();
            await _wishlistRepositoryMock.Received(1).AddAsync(Arg.Is<WishlistItem>(x => x.UserId == userId && x.CourseId == courseId));
            await _wishlistRepositoryMock.Received(1).SaveChangesAsync();

            _hubClientsMock.Received(1).User(userId.ToString());
            await _clientProxyMock.Received(1).SendCoreAsync("UpdateWishlistCount", Arg.Is<object[]>(x => x.Length == 0), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task IsInWishlistAsync_CallsRepository_ReturnsExistsResult()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 10;

            //Arrange 2
            _wishlistRepositoryMock.ExistsAsync(userId, courseId).Returns(true);

            //Act
            var result = await _sut.IsInWishlistAsync(userId, courseId);

            //Assert
            result.Should().BeTrue();
            await _wishlistRepositoryMock.Received(1).ExistsAsync(userId, courseId);
        }

        [Fact]
        public async Task GetWishlistCountAsync_CallsRepository_ReturnsCount()
        {
            //Arrange 1
            var userId = 1;
            var items = new List<WishlistItem> { new WishlistItem(), new WishlistItem() };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserIdAsync(userId).Returns(items);

            //Act
            var result = await _sut.GetWishlistCountAsync(userId);

            //Assert
            result.Should().Be(2);
            await _wishlistRepositoryMock.Received(1).GetByUserIdAsync(userId);
        }

        [Fact]
        public async Task GetWishlistAsync_ItemPropertiesAreNull_MapsWithFallbacks()
        {
            //Arrange 1
            var userId = 1;
            var courseId = 0; // null coalesced to 0
            var items = new List<WishlistItem>
            {
                new WishlistItem
                {
                    Id = 100,
                    CourseId = null, // Trigger item.CourseId ?? 0
                    AddedDate = null, // Trigger item.AddedDate ?? DateTime.UtcNow
                    Course = new Course 
                    { 
                        Title = null!, // Trigger item.Course?.Title ?? ""
                        CourseThumbnailUrl = null,
                        Instructor = new Instructor { InstructorNavigation = null! } // Trigger middle null path
                    }
                },
                new WishlistItem
                {
                    Id = 101,
                    CourseId = 10,
                    AddedDate = DateTime.UtcNow,
                    Course = new Course
                    {
                        Title = "Test 2",
                        Instructor = null // Trigger Instructor == null path
                    }
                }
            };

            //Arrange 2
            _wishlistRepositoryMock.GetByUserIdAsync(userId).Returns(items);
            _courseRepositoryMock.IsEnrolledAsync(userId, Arg.Any<int>()).Returns(false);
            _courseRepositoryMock.GetCourseStatsAsync(Arg.Any<int>()).Returns((CourseStats?)null);

            //Act
            var result = await _sut.GetWishlistAsync(userId);

            //Assert
            result.Should().HaveCount(2);
            result[0].CourseId.Should().Be(0);
            result[0].Title.Should().Be("");
            result[0].InstructorName.Should().Be("Unknown");
            result[1].InstructorName.Should().Be("Unknown");
        }
    }
}
