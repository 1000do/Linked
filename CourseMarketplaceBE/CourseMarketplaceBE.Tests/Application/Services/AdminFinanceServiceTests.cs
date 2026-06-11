using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class AdminFinanceServiceTests
    {
        private readonly IAdminFinanceRepository _mockRepo;
        private readonly IPaymentGatewayService _mockPaymentGateway;
        private readonly IInstructorRepository _mockInstructorRepo;
        private readonly INotificationService _mockNotiService;
        private readonly IHubContext<FinanceHub> _mockHubContext;
        private readonly ILogger<AdminFinanceService> _mockLogger;
        private readonly ISystemConfigRepository _mockConfigRepo;
        private readonly ICourseRepository _mockCourseRepo;
        private readonly IStripeConnectService _mockStripeConnect;
        private readonly IGiftRepository _mockGiftRepo;

        private readonly AdminFinanceService _financeService;

        public AdminFinanceServiceTests()
        {
            _mockRepo = Substitute.For<IAdminFinanceRepository>();
            _mockPaymentGateway = Substitute.For<IPaymentGatewayService>();
            _mockInstructorRepo = Substitute.For<IInstructorRepository>();
            _mockNotiService = Substitute.For<INotificationService>();
            _mockHubContext = Substitute.For<IHubContext<FinanceHub>>();
            _mockLogger = Substitute.For<ILogger<AdminFinanceService>>();
            _mockConfigRepo = Substitute.For<ISystemConfigRepository>();
            _mockCourseRepo = Substitute.For<ICourseRepository>();
            _mockStripeConnect = Substitute.For<IStripeConnectService>();
            _mockGiftRepo = Substitute.For<IGiftRepository>();

            _financeService = new AdminFinanceService(
                _mockRepo,
                _mockPaymentGateway,
                _mockInstructorRepo,
                _mockNotiService,
                _mockHubContext,
                _mockLogger,
                _mockConfigRepo,
                _mockCourseRepo,
                _mockStripeConnect,
                _mockGiftRepo
            );
        }

        [Fact]
        public async Task GetInstructorCourseRevenuesByInstructorAsync_ShouldReturnMappedResponses()
        {
            // Arrange
            int instructorId = 5;
            int year = 2026;
            int month = 5;

            var mockProjections = new List<InstructorCourseRevenueProjection>
            {
                new InstructorCourseRevenueProjection
                {
                    CourseId = 101,
                    CourseTitle = "Web Dev",
                    InstructorId = instructorId,
                    InstructorName = "John Doe",
                    SalesCount = 10,
                    MonthlyRevenue = 500000,
                    PreviousMonthRevenue = 400000,
                    YearlyRevenue = 3000000,
                    LifetimeRevenue = 10000000
                },
                new InstructorCourseRevenueProjection
                {
                    CourseId = 102,
                    CourseTitle = "Design Dev",
                    InstructorId = instructorId,
                    InstructorName = "John Doe",
                    SalesCount = 5,
                    MonthlyRevenue = 250000,
                    PreviousMonthRevenue = 300000,
                    YearlyRevenue = 1500000,
                    LifetimeRevenue = 5000000
                }
            };

            _mockRepo.GetInstructorCourseRevenuesByInstructorAsync(instructorId, year, month)
                .Returns(Task.FromResult(mockProjections));

            // Act
            var result = await _financeService.GetInstructorCourseRevenuesByInstructorAsync(instructorId, year, month);

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);

            result[0].CourseId.Should().Be(101);
            result[0].CourseTitle.Should().Be("Web Dev");
            result[0].InstructorId.Should().Be(instructorId);
            result[0].InstructorName.Should().Be("John Doe");
            result[0].SalesCount.Should().Be(10);
            result[0].MonthlyRevenue.Should().Be(500000);
            result[0].PreviousMonthRevenue.Should().Be(400000);
            result[0].YearlyRevenue.Should().Be(3000000);
            result[0].LifetimeRevenue.Should().Be(10000000);

            result[1].CourseId.Should().Be(102);
            result[1].CourseTitle.Should().Be("Design Dev");
            result[1].InstructorId.Should().Be(instructorId);
            result[1].InstructorName.Should().Be("John Doe");
            result[1].SalesCount.Should().Be(5);
            result[1].MonthlyRevenue.Should().Be(250000);
            result[1].PreviousMonthRevenue.Should().Be(300000);
            result[1].YearlyRevenue.Should().Be(1500000);
            result[1].LifetimeRevenue.Should().Be(5000000);

            await _mockRepo.Received(1).GetInstructorCourseRevenuesByInstructorAsync(instructorId, year, month);
        }
    }
}
