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
using NSubstitute.ExceptionExtensions;

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
        public async Task GetInstructorCourseRevenuesByInstructorAsync_ValidInstructor_ReturnsMappedResponses()
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

        // =========================================================================
        // LÄ‚â€Ă¢â‚¬ÂÄ‚â€Ă‚Â¡Ă„â€Ă¢â‚¬ÂÄ‚â€Ă‚ÂºĂ„â€Ă¢â‚¬ÂÄ‚â€Ă‚Â§n 1: Test cĂ„â€Ă¢â‚¬ÂÄ‚Â¢Ă¢â€Â¬Ă‚ÂĂ„â€Ă¢â‚¬ÂÄ‚â€Ă‚Â¡c hĂ„â€Ă¢â‚¬ÂÄ‚Â¢Ă¢â€Â¬Ă‚ÂĂ„â€Ă¢â‚¬ÂÄ‚â€Ă‚Â m 1 - 5
        // =========================================================================

        [Fact]
        public async Task SetTransferRateAsync_RateLessThan30_ThrowsInvalidOperationException()
        {
            //Arrange 1
            decimal rate = 29m;

            //Arrange 2
            
            //Act
            System.Func<Task> act = async () => await _financeService.SetTransferRateAsync(rate);

            //Assert
            var ex = await act.Should().ThrowAsync<System.InvalidOperationException>();
            ex.WithMessage("The revenue share rate must be between 30% and 95%.");
        }

        [Fact]
        public async Task SetTransferRateAsync_RateGreaterThan95_ThrowsInvalidOperationException()
        {
            //Arrange 1
            decimal rate = 96m;

            //Arrange 2
            
            //Act
            System.Func<Task> act = async () => await _financeService.SetTransferRateAsync(rate);

            //Assert
            var ex = await act.Should().ThrowAsync<System.InvalidOperationException>();
            ex.WithMessage("The revenue share rate must be between 30% and 95%.");
        }

        [Fact]
        public async Task SetTransferRateAsync_ValidRateWithInstructors_UpsertsConfigAndSendsNotifications()
        {
            //Arrange 1
            decimal rate = 75m;
            var instructors = new List<Instructor>
            {
                new Instructor { InstructorId = 1, StripeAccountId = "acct_123" },
                new Instructor { InstructorId = 2, StripeAccountId = "acct_456" }
            };

            //Arrange 2
            _mockConfigRepo.UpsertConfigAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));
            _mockInstructorRepo.GetInstructorsWithStripeAsync()
                .Returns(Task.FromResult(instructors));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(true));

            //Act
            await _financeService.SetTransferRateAsync(rate);

            //Assert
            await _mockConfigRepo.Received(1).UpsertConfigAsync(
                "TransferRate",
                "75.00",
                "Instructor share rate: 75%, Platform share rate: 25%"
            );
            await _mockInstructorRepo.Received(1).GetInstructorsWithStripeAsync();
            await _mockNotiService.Received(1).SendNotificationAsync(
                1,
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            );
            await _mockNotiService.Received(1).SendNotificationAsync(
                2,
                Arg.Any<string>(),
                Arg.Any<string>(),
                Arg.Any<string>()
            );
        }

        [Fact]
        public async Task SetTransferRateAsync_ValidRateNoInstructors_UpsertsConfigAndSkipsNotifications()
        {
            //Arrange 1
            decimal rate = 80m;
            var instructors = new List<Instructor>();

            //Arrange 2
            _mockConfigRepo.UpsertConfigAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));
            _mockInstructorRepo.GetInstructorsWithStripeAsync()
                .Returns(Task.FromResult(instructors));

            //Act
            await _financeService.SetTransferRateAsync(rate);

            //Assert
            await _mockConfigRepo.Received(1).UpsertConfigAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
            await _mockInstructorRepo.Received(1).GetInstructorsWithStripeAsync();
            await _mockNotiService.DidNotReceiveWithAnyArgs().SendNotificationAsync(default, default!, default!, default!);
        }

        [Fact]
        public async Task SetTransferRateAsync_NotificationThrowsException_SwallowsExceptionAndCompletes()
        {
            //Arrange 1
            decimal rate = 80m;
            var instructors = new List<Instructor>
            {
                new Instructor { InstructorId = 1, StripeAccountId = "acct_123" }
            };

            //Arrange 2
            _mockConfigRepo.UpsertConfigAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(Task.FromResult(1));
            _mockInstructorRepo.GetInstructorsWithStripeAsync()
                .Returns(Task.FromResult(instructors));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>())
                .ThrowsAsync(new System.Exception("Notification failed"));

            //Act
            System.Func<Task> act = async () => await _financeService.SetTransferRateAsync(rate);

            //Assert
            await act.Should().NotThrowAsync();
            await _mockConfigRepo.Received(1).UpsertConfigAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task GetFinancialSummaryAsync_ValidDate_ReturnsCorrectCalculationsAndSums()
        {
            //Arrange 1
            int? year = 2026;
            int? month = 5;
            decimal grossRevenue = 1000m;
            decimal totalPaidOut = 500m;
            decimal pendingEscrow = 200m;
            decimal maturedEscrow = 100m;
            int totalTransactions = 50;
            decimal totalRefunded = 50m;
            decimal currentRate = 70m;

            var payoutDetails = new List<PayoutDetailProjection>
            {
                new PayoutDetailProjection
                {
                    TotalAmount = 100m,
                    InstructorReceived = 70m,
                    PayoutStatus = "paid"
                },
                new PayoutDetailProjection
                {
                    TotalAmount = 200m,
                    InstructorReceived = 140m,
                    PayoutStatus = "refunded" 
                },
                new PayoutDetailProjection
                {
                    TotalAmount = 50m,
                    InstructorReceived = 35m,
                    PayoutStatus = "pending"
                }
            };

            //Arrange 2
            _mockRepo.GetGrossRevenueAsync(year, month).Returns(Task.FromResult(grossRevenue));
            _mockRepo.GetTotalPaidOutAsync(year, month).Returns(Task.FromResult(totalPaidOut));
            _mockRepo.GetPendingEscrowAsync(year, month).Returns(Task.FromResult(pendingEscrow));
            _mockRepo.GetMaturedEscrowAsync(year, month).Returns(Task.FromResult(maturedEscrow));
            _mockRepo.GetSucceededTransactionCountAsync(year, month).Returns(Task.FromResult(totalTransactions));
            _mockRepo.GetTotalRefundedAsync(year, month).Returns(Task.FromResult(totalRefunded));
            _mockConfigRepo.GetValueAsync("TransferRate").Returns(Task.FromResult(currentRate.ToString()));
            _mockRepo.GetPayoutDetailsAsync(year, month, 1, int.MaxValue).Returns(Task.FromResult((payoutDetails, payoutDetails.Count)));

            //Act
            var result = await _financeService.GetFinancialSummaryAsync(year, month);

            //Assert
            // Stripe fee = 4.95
            result.GrossRevenue.Should().Be(1000m - 4.95m);
            result.TotalPaidOut.Should().Be(500m);
            result.PendingEscrow.Should().Be(200m);
            result.MaturedEscrow.Should().Be(100m);
            result.PlatformNetProfit.Should().Be(40.05m);
            result.CurrentTransferRate.Should().Be(70m);
            result.TotalTransactions.Should().Be(50);
            result.TotalRefunded.Should().Be(50m);
        }

        [Fact]
        public async Task GetInstructorPayoutsAsync_NormalPayouts_CalculatesPlatformReceivedCorrectly()
        {
            //Arrange 1
            int? year = 2026;
            int? month = 5;
            int page = 1;
            int pageSize = 10;

            var projections = new List<PayoutDetailProjection>
            {
                new PayoutDetailProjection
                {
                    PayoutId = 1,
                    TotalAmount = 100m,
                    InstructorReceived = 70m,
                    PayoutStatus = "paid"
                }
            };

            //Arrange 2
            _mockRepo.GetPayoutDetailsAsync(year, month, page, pageSize).Returns(Task.FromResult((projections, 1)));

            //Act
            var result = await _financeService.GetInstructorPayoutsAsync(year, month, page, pageSize);

            //Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().PlatformReceived.Should().Be(26.8m); // 100 - 3.2 - 70
        }

        [Fact]
        public async Task GetInstructorPayoutsAsync_RefundedPayout_CalculatesNegativePlatformReceived()
        {
            //Arrange 1
            var projections = new List<PayoutDetailProjection>
            {
                new PayoutDetailProjection
                {
                    PayoutId = 2,
                    TotalAmount = -100m,
                    InstructorReceived = -70m,
                    PayoutStatus = "refunded"
                }
            };

            //Arrange 2
            _mockRepo.GetPayoutDetailsAsync(null, null, 1, 10).Returns(Task.FromResult((projections, 1)));

            //Act
            var result = await _financeService.GetInstructorPayoutsAsync();

            //Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().PlatformReceived.Should().Be(-26.8m);
        }

        [Fact]
        public async Task MarkPayoutAsPaidAsync_PayoutNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int payoutId = 1;

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(null));

            //Act
            System.Func<Task> act = async () => await _financeService.MarkPayoutAsPaidAsync(payoutId);

            //Assert
            var ex = await act.Should().ThrowAsync<System.InvalidOperationException>();
            ex.WithMessage("This payment was not found.");
        }

        [Fact]
        public async Task MarkPayoutAsPaidAsync_AlreadyPaid_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int payoutId = 1;
            var payout = new InstructorPayout { PayoutId = payoutId, IsPaid = true };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult(payout));

            //Act
            System.Func<Task> act = async () => await _financeService.MarkPayoutAsPaidAsync(payoutId);

            //Assert
            var ex = await act.Should().ThrowAsync<System.InvalidOperationException>();
            ex.WithMessage("This payment has already been marked as Paid.");
        }

        [Fact]
        public async Task MarkPayoutAsPaidAsync_ValidPendingPayout_UpdatesStatusAndBroadcastsSignalR()
        {
            //Arrange 1
            int payoutId = 1;
            var payout = new InstructorPayout { PayoutId = payoutId, IsPaid = false, InstructorId = 5 };

            var mockClients = Substitute.For<IHubClients>();
            var mockGroup = Substitute.For<IClientProxy>();
            mockClients.Group(Arg.Any<string>()).Returns(mockGroup);

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult(payout));
            _mockRepo.SaveChangesAsync().Returns(Task.FromResult(1));
            _mockHubContext.Clients.Returns(mockClients);

            //Act
            await _financeService.MarkPayoutAsPaidAsync(payoutId);

            //Assert
            payout.IsPaid.Should().BeTrue();
            payout.PayoutStatus.Should().Be("paid");
            payout.PayoutDate.Should().NotBe(default(System.DateTime));
            await _mockRepo.Received(1).SaveChangesAsync();
            
            mockClients.Received(1).Group("AdminFinance");
            mockClients.Received(1).Group("InstructorFinance_5");
        }

        [Fact]
        public async Task MarkPayoutAsPaidAsync_SignalRThrowsException_SwallowsExceptionAndCompletes()
        {
            //Arrange 1
            int payoutId = 1;
            var payout = new InstructorPayout { PayoutId = payoutId, IsPaid = false, InstructorId = 5 };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult(payout));
            _mockRepo.SaveChangesAsync().Returns(Task.FromResult(1));
            
            _mockHubContext.Clients.Throws(new System.Exception("SignalR failed"));

            //Act
            System.Func<Task> act = async () => await _financeService.MarkPayoutAsPaidAsync(payoutId);

            //Assert
            await act.Should().NotThrowAsync();
            payout.IsPaid.Should().BeTrue();
        }

        [Fact]
        public async Task GetCurrentTransferRateAsync_ConfigExists_ReturnsConfiguredRate()
        {
            //Arrange 1
            string storedRate = "72.5";

            //Arrange 2
            _mockConfigRepo.GetValueAsync("TransferRate").Returns(Task.FromResult(storedRate));

            //Act
            var result = await _financeService.GetCurrentTransferRateAsync();

            //Assert
            result.Should().Be(72.5m);
        }

        [Fact]
        public async Task GetCurrentTransferRateAsync_ConfigEmptyOrInvalid_ReturnsDefaultRate()
        {
            //Arrange 1
            //Arrange 2
            _mockConfigRepo.GetValueAsync("TransferRate").Returns(Task.FromResult<string>(null));

            //Act
            var result = await _financeService.GetCurrentTransferRateAsync();

            //Assert
            result.Should().Be(70.00m);
        }
            #region Phase 2 Tests

        [Fact]
        public async Task GetPayoutDaysConfigAsync_NullConfig_ReturnsDefault15()
        {
            //Arrange 1
            _mockConfigRepo.GetValueAsync("PayoutDays").Returns(Task.FromResult<string?>(null));

            //Act
            var result = await _financeService.GetPayoutDaysConfigAsync();

            //Assert
            result.Should().Be("15");
        }

        [Fact]
        public async Task GetPayoutDaysConfigAsync_HasConfig_ReturnsConfigValue()
        {
            //Arrange 1
            _mockConfigRepo.GetValueAsync("PayoutDays").Returns(Task.FromResult<string?>("15,20"));

            //Act
            var result = await _financeService.GetPayoutDaysConfigAsync();

            //Assert
            result.Should().Be("15,20");
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task SetPayoutDaysConfigAsync_EmptyInput_ThrowsInvalidOperationException(string input)
        {
            //Arrange 1
            
            //Act
            System.Func<Task> act = async () => await _financeService.SetPayoutDaysConfigAsync(input);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Payout days configuration cannot be empty.");
        }

        [Theory]
        [InlineData("abc")]
        [InlineData("10")]
        [InlineData("21")]
        [InlineData("15,22")]
        public async Task SetPayoutDaysConfigAsync_InvalidFormat_ThrowsInvalidOperationException(string input)
        {
            //Arrange 1
            
            //Act
            System.Func<Task> act = async () => await _financeService.SetPayoutDaysConfigAsync(input);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Payout days must be a comma-separated list of integers between 15 and 20 (e.g., '15' or '17, 20').");
        }

        [Fact]
        public async Task SetPayoutDaysConfigAsync_ValidInput_UpsertsConfig()
        {
            //Arrange 1
            string input = "15, 20";

            //Arrange 2
            _mockConfigRepo.UpsertConfigAsync("PayoutDays", input, Arg.Any<string>()).Returns(Task.FromResult(1));

            //Act
            await _financeService.SetPayoutDaysConfigAsync(input);

            //Assert
            await _mockConfigRepo.Received(1).UpsertConfigAsync("PayoutDays", input, Arg.Any<string>());
        }

        [Fact]
        public async Task PerformStripeTransferAsync_PayoutNotFound_ThrowsException()
        {
            //Arrange 1
            int payoutId = 1;

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(null));

            //Act
            System.Func<Task> act = async () => await _financeService.PerformStripeTransferAsync(payoutId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Payment not found.");
        }

        [Fact]
        public async Task PerformStripeTransferAsync_AlreadyPaid_ThrowsException()
        {
            //Arrange 1
            int payoutId = 1;
            var payout = new InstructorPayout { PayoutId = payoutId, IsPaid = true };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(payout));

            //Act
            System.Func<Task> act = async () => await _financeService.PerformStripeTransferAsync(payoutId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This payment was previously paid.");
        }

        [Fact]
        public async Task PerformStripeTransferAsync_NoStripeAccount_ThrowsException()
        {
            //Arrange 1
            int payoutId = 1;
            var payout = new InstructorPayout { PayoutId = payoutId, IsPaid = false, Instructor = new Instructor { StripeAccountId = null } };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(payout));

            //Act
            System.Func<Task> act = async () => await _financeService.PerformStripeTransferAsync(payoutId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("This instructor has not set up a Stripe Connect account.");
        }

        [Fact]
        public async Task PerformStripeTransferAsync_Valid_TransfersAndUpdates()
        {
            //Arrange 1
            int payoutId = 1;
            var instructor = new Instructor { StripeAccountId = "acct_123" };
            var payout = new InstructorPayout 
            { 
                PayoutId = payoutId, 
                IsPaid = false, 
                InstructorId = 5,
                Instructor = instructor,
                PayoutAmount = 100m,
                Transaction = new Transaction { Currency = "usd" }
            };
            var transferResult = new StripeTransferResponseDto { DestinationAmount = 98m, DestinationPaymentId = "py_123", Id = "tr_123" };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(payout));
            _mockStripeConnect.CreateConnectTransferAsync(100m, "usd", "acct_123", Arg.Any<string>(), payoutId)
                .Returns(Task.FromResult(transferResult));

            //Act
            var result = await _financeService.PerformStripeTransferAsync(payoutId);

            //Assert
            result.Should().Be("tr_123");
            payout.IsPaid.Should().BeTrue();
            payout.PayoutStatus.Should().Be("transferred");
            payout.PayoutAmount.Should().Be(98m);
            payout.StripeTransferId.Should().Be("py_123");
            payout.PayoutDate.Should().NotBe(default(System.DateTime));
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task PerformStripeTransferAsync_StripeError_SetsFailedAndThrows()
        {
            //Arrange 1
            int payoutId = 1;
            var instructor = new Instructor { StripeAccountId = "acct_123" };
            var payout = new InstructorPayout 
            { 
                PayoutId = payoutId, 
                IsPaid = false, 
                InstructorId = 5,
                Instructor = instructor,
                PayoutAmount = 100m,
                Transaction = new Transaction { Currency = "usd" }
            };

            //Arrange 2
            _mockRepo.GetPayoutByIdAsync(payoutId).Returns(Task.FromResult<InstructorPayout?>(payout));
            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>())
                .Throws(new Exception("Stripe error"));

            //Act
            System.Func<Task> act = async () => await _financeService.PerformStripeTransferAsync(payoutId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stripe Transfer error: Stripe error");
            payout.PayoutStatus.Should().Be("failed");
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task BulkPayAllViaStripeAsync_ValidData_FiltersAndProcessesCorrectly()
        {
            //Arrange 1
            var now = DateTime.UtcNow;
            var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var refundLimitDate = now.AddDays(-14);

            var pendingPayouts = new List<PayoutDetailProjection>
            {
                // Should process: unpaid, old month, > 14 days old
                new PayoutDetailProjection { PayoutId = 1, IsPaid = false, TransactionDate = firstDayOfCurrentMonth.AddDays(-20) },
                // Should ignore: paid
                new PayoutDetailProjection { PayoutId = 2, IsPaid = true, TransactionDate = firstDayOfCurrentMonth.AddDays(-20) },
                // Should ignore: missing transaction date
                new PayoutDetailProjection { PayoutId = 3, IsPaid = false, TransactionDate = null },
                // Should ignore: current month
                new PayoutDetailProjection { PayoutId = 4, IsPaid = false, TransactionDate = firstDayOfCurrentMonth.AddDays(1) },
                // Should process: unpaid, old month, > 14 days old
                new PayoutDetailProjection { PayoutId = 5, IsPaid = false, TransactionDate = firstDayOfCurrentMonth.AddDays(-16) },
                // Throws error
                new PayoutDetailProjection { PayoutId = 6, IsPaid = false, TransactionDate = firstDayOfCurrentMonth.AddDays(-20), InstructorName = "Error" }
            };

            var payout1 = new InstructorPayout { PayoutId = 1, IsPaid = false, Instructor = new Instructor { StripeAccountId = "acc" } };
            var payout5 = new InstructorPayout { PayoutId = 5, IsPaid = false, Instructor = new Instructor { StripeAccountId = "acc" } };
            var payout6 = new InstructorPayout { PayoutId = 6, IsPaid = false, Instructor = new Instructor { StripeAccountId = "acc" } };
            
            var transferResult = new StripeTransferResponseDto { DestinationAmount = 10, DestinationPaymentId = "py", Id = "tr" };

            //Arrange 2
            _mockRepo.GetPayoutDetailsAsync(null, null, 1, 10000).Returns(Task.FromResult((pendingPayouts, pendingPayouts.Count)));
            
            _mockRepo.GetPayoutByIdAsync(1).Returns(Task.FromResult<InstructorPayout?>(payout1));
            _mockRepo.GetPayoutByIdAsync(5).Returns(Task.FromResult<InstructorPayout?>(payout5));
            _mockRepo.GetPayoutByIdAsync(6).Returns(Task.FromResult<InstructorPayout?>(payout6));

            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(s => s.Contains("#1")), 1)
                .Returns(Task.FromResult(transferResult));
            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(s => s.Contains("#5")), 5)
                .Returns(Task.FromResult(transferResult));
            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Is<string>(s => s.Contains("#6")), 6)
                .Throws(new Exception("simulated error"));

            //Act
            var result = await _financeService.BulkPayAllViaStripeAsync();

            //Assert
            result.TotalProcessed.Should().Be(3);
            result.SuccessCount.Should().Be(2);
            result.FailCount.Should().Be(1);
            result.Errors.Should().Contain(e => e.Contains("Payout #6 (Instructor: Error): Stripe Transfer error: simulated error"));
        }
        #endregion
        #region Phase 3 Tests

        [Fact]
        public async Task GetPlatformBalanceAsync_ValidState_ReturnsCorrectMappedData()
        {
            //Arrange 1
            var stripeBalance = new StripePlatformBalanceDto 
            { 
                Available = 1000m, 
                Incoming = 500m, 
                Currency = "usd",
                PayoutScheduleInterval = "manual",
                PayoutScheduleAnchor = null
            };

            //Arrange 2
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(stripeBalance));

            //Act
            var result = await _financeService.GetPlatformBalanceAsync();

            //Assert
            result.Available.Should().Be(1000m);
            result.Incoming.Should().Be(500m);
            result.Total.Should().Be(1500m);
            result.Currency.Should().Be("usd");
            result.PayoutScheduleInterval.Should().Be("manual");
        }

        [Fact]
        public async Task CreateWithdrawalAsync_AmountLessThanMin_ThrowsException()
        {
            //Arrange 1
            var request = new WithdrawRequest { Amount = 0.49m };
            var stripeBalance = new StripePlatformBalanceDto { Available = 1000m };

            //Arrange 2
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(stripeBalance));

            //Act
            System.Func<Task> act = async () => await _financeService.CreateWithdrawalAsync(request, 1);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("The withdrawal amount must be at least $0.50.");
        }

        [Fact]
        public async Task CreateWithdrawalAsync_AmountGreaterThanAvailable_ThrowsException()
        {
            //Arrange 1
            var request = new WithdrawRequest { Amount = 1500m };
            var stripeBalance = new StripePlatformBalanceDto { Available = 1000m };

            //Arrange 2
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(stripeBalance));

            //Act
            System.Func<Task> act = async () => await _financeService.CreateWithdrawalAsync(request, 1);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("Insufficient balance. Available: $1000.00, Requested: $1500.00");
        }

        [Fact]
        public async Task CreateWithdrawalAsync_ValidRequest_CreatesWithdrawal()
        {
            //Arrange 1
            var request = new WithdrawRequest { Amount = 100m, Description = "Test" };
            var stripeBalance = new StripePlatformBalanceDto { Available = 1000m };
            var stripePayout = new StripeWithdrawalResponseDto { Id = "po_123", Status = "pending" };
            
            var mockClients = Substitute.For<Microsoft.AspNetCore.SignalR.IHubClients>();
            var mockGroup = Substitute.For<Microsoft.AspNetCore.SignalR.IClientProxy>();
            mockClients.Group(Arg.Any<string>()).Returns(mockGroup);

            //Arrange 2
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(stripeBalance));
            _mockStripeConnect.CreatePlatformWithdrawalAsync(100m, "Test", 1).Returns(Task.FromResult(stripePayout));
            _mockRepo.AddWithdrawalAsync(Arg.Any<PlatformWithdrawal>()).Returns(Task.FromResult(1));
            _mockHubContext.Clients.Returns(mockClients);

            //Act
            var result = await _financeService.CreateWithdrawalAsync(request, 1);

            //Assert
            result.StripePayoutId.Should().Be("po_123");
            result.Amount.Should().Be(100m);
            result.Status.Should().Be("pending");
            await _mockRepo.Received(1).AddWithdrawalAsync(Arg.Is<PlatformWithdrawal>(w => w.Amount == 100m && w.StripePayoutId == "po_123"));
            mockClients.Received(1).Group("AdminFinance");
        }

        [Fact]
        public async Task GetWithdrawalHistoryAsync_NoChanges_ReturnsData()
        {
            //Arrange 1
            var withdrawals = new List<PlatformWithdrawal>
            {
                new PlatformWithdrawal { WithdrawalId = 1, Status = "paid", StripePayoutId = "po_1", Amount = 100m, Manager = new Manager { DisplayName = "AB" } }
            };

            //Arrange 2
            _mockRepo.GetWithdrawalsAsync(null, null, 1, 10).Returns(Task.FromResult((withdrawals, 1)));

            //Act
            var result = await _financeService.GetWithdrawalHistoryAsync(null, null, 1, 10);

            //Assert
            result.Items.Should().HaveCount(1);
            result.Items.First().Status.Should().Be("paid");
            await _mockStripeConnect.DidNotReceive().GetPlatformPayoutStatusAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task GetWithdrawalHistoryAsync_HasPending_SyncsStatusAndSaves()
        {
            //Arrange 1
            var withdrawals = new List<PlatformWithdrawal>
            {
                new PlatformWithdrawal { WithdrawalId = 1, Status = "pending", StripePayoutId = "po_1", Amount = 100m }
            };
            var syncResult = new StripeWithdrawalResponseDto { Status = "paid", ArrivalDate = DateTime.UtcNow };

            //Arrange 2
            _mockRepo.GetWithdrawalsAsync(null, null, 1, 10).Returns(Task.FromResult((withdrawals, 1)));
            _mockStripeConnect.GetPlatformPayoutStatusAsync("po_1").Returns(Task.FromResult(syncResult));

            //Act
            var result = await _financeService.GetWithdrawalHistoryAsync(null, null, 1, 10);

            //Assert
            result.Items.First().Status.Should().Be("paid");
            await _mockStripeConnect.Received(1).GetPlatformPayoutStatusAsync("po_1");
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RefundTransactionAsync_TransactionNotFound_ThrowsException()
        {
            //Arrange 1
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(null));

            //Act
            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction #1 not found.");
        }

        [Fact]
        public async Task RefundTransactionAsync_AlreadyRefunded_ThrowsException()
        {
            //Arrange 1
            var txn = new Transaction { TransactionsStatus = "refunded" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This transaction was previously refunded.");
        }

        [Fact]
        public async Task RefundTransactionAsync_NotSucceeded_ThrowsException()
        {
            //Arrange 1
            var txn = new Transaction { TransactionsStatus = "failed" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Only successful transactions can be refunded. Current status: failed.");
        }

        [Fact]
        public async Task RefundTransactionAsync_NoPaymentIntent_ThrowsException()
        {
            //Arrange 1
            var txn = new Transaction { TransactionsStatus = "succeeded", StripePaymentintentId = null };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This transaction does not have a PaymentIntent ID * cannot refund via Stripe.");
        }

        [Fact]
        public async Task RefundTransactionAsync_HasTransfer_ReversesTransferAndRefunds()
        {
            //Arrange 1
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = true, StripeTransferId = "py_123" };
            var txn = new Transaction 
            { 
                TransactionId = 1,
                TransactionsStatus = "succeeded", 
                StripePaymentintentId = "pi_123",
                Amount = 100m,
                InstructorPayouts = new List<InstructorPayout> { payout }
            };

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetStripeTransferIdByDestinationPaymentAsync("py_123").Returns(Task.FromResult<string?>("tr_123"));
            _mockPaymentGateway.ReverseTransferAsync("tr_123").Returns(Task.FromResult("re_123"));
            _mockPaymentGateway.RefundAsync("pi_123", 100m, "Test").Returns(Task.FromResult("ref_123"));

            //Act
            var result = await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            result.RefundedAmount.Should().Be(100m);
            txn.TransactionsStatus.Should().Be("refunded");
            payout.PayoutStatus.Should().Be("refunded");
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RefundTransactionAsync_ReversalFails_ThrowsException()
        {
            //Arrange 1
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = true, StripeTransferId = "py_123" };
            var txn = new Transaction 
            { 
                TransactionId = 1,
                TransactionsStatus = "succeeded", 
                StripePaymentintentId = "pi_123",
                Amount = 100m,
                InstructorPayouts = new List<InstructorPayout> { payout }
            };

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetStripeTransferIdByDestinationPaymentAsync("py_123").Returns(Task.FromResult<string?>("tr_123"));
            _mockPaymentGateway.ReverseTransferAsync("tr_123").Throws(new Exception("Reverse failed"));

            //Act
            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Test");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot claw back money from the instructor: Reverse failed. The instructor may have already withdrawn all funds from Stripe.");
        }

        #endregion
        #region Phase 4 Tests

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_NoInstructors_ReturnsEarly()
        {
            //Arrange 1
            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(new List<Instructor>()));

            //Act
            await _financeService.SyncAllPayoutsWithStripeAsync();

            //Assert
            await _mockStripeConnect.DidNotReceive().ListPayoutsAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_HasInstructors_SyncsSuccessfully()
        {
            //Arrange 1
            var instructors = new List<Instructor>
            {
                new Instructor { InstructorId = 1, StripeAccountId = "acct_1" },
                new Instructor { InstructorId = 2, StripeAccountId = "" } // Should skip
            };

            var stripePayouts = new List<StripePayoutDto>
            {
                new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow }
            };

            var dbPayouts = new List<InstructorPayout>(); // Simulate no existing payouts mapped to po_1

            var balanceTransactions = new List<StripeBalanceTransactionDto>
            {
                new StripeBalanceTransactionDto { Type = "transfer", SourceId = "tr_1" },
                new StripeBalanceTransactionDto { Type = "charge", SourceId = "ch_1" } // Should skip
            };

            var localPayout = new InstructorPayout { PayoutId = 1, StripeTransferId = "tr_1", PayoutStatus = "pending" };

            //Arrange 2
            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromResult(dbPayouts));
            _mockStripeConnect.ListBalanceTransactionsAsync("acct_1", "po_1").Returns(Task.FromResult(balanceTransactions));
            _mockRepo.GetPayoutByTransferIdAsync("tr_1").Returns(Task.FromResult<InstructorPayout?>(localPayout));

            //Act
            await _financeService.SyncAllPayoutsWithStripeAsync();

            //Assert
            localPayout.StripePayoutId.Should().Be("po_1");
            localPayout.PayoutStatus.Should().Be("paid");
            localPayout.IsPaid.Should().BeTrue();
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RequestRefundAsync_NotSucceeded_ThrowsException()
        {
            //Arrange 1
            var txn = new Transaction { TransactionsStatus = "failed", AccountFrom = 1 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Refunds are only allowed for successful transactions. Current status: failed");
        }

        [Fact]
        public async Task RequestRefundAsync_ValidRequest_AutoRejected()
        {
            //Arrange 1
            var txn = new Transaction 
            { 
                TransactionId = 1, 
                TransactionsStatus = "succeeded", 
                AccountFrom = 1,
                TransactionCreatedAt = DateTime.UtcNow,
                OrderItem = new OrderItem { CourseId = 1 }
            };
            var metrics = new RefundEligibilityDto { AccountFlagCount = 3 }; // Should auto reject

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));

            //Act
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");

            //Assert
            result.IsAutoRejected.Should().BeTrue();
            result.RejectReason.Should().Be("your account having multiple flags");
            txn.TransactionExt.Should().NotBeNull();
            txn.TransactionExt!.RefundAdminNote.Should().Be("Auto-rejected: your account having multiple flags");
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task RequestRefundAsync_ValidRequest_PendingApproval()
        {
            //Arrange 1
            var txn = new Transaction 
            { 
                TransactionId = 1, 
                TransactionsStatus = "succeeded", 
                TransactionType = "gift_purchase",
                AccountFrom = 1,
                TransactionCreatedAt = DateTime.UtcNow,
                OrderItemId = 5,
                OrderItem = new OrderItem { CourseId = 1 }
            };
            var metrics = new RefundEligibilityDto { AccountFlagCount = 0 };

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(new Gift { IsClaimed = false }));

            //Act
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");

            //Assert
            result.IsAutoRejected.Should().BeFalse();
            txn.TransactionsStatus.Should().Be("refund_pending");
            txn.TransactionExt.Should().NotBeNull();
            await _mockRepo.Received(1).SaveChangesAsync();
            await _mockNotiService.Received(1).SendNotificationAsync(1, "New Refund Request", Arg.Any<string>(), "/AdminFinance/Refunds");
        }

        [Fact]
        public async Task GetPendingRefundRequestsAsync_ValidState_ReturnsData()
        {
            //Arrange 1
            var list = new List<TransactionListDto> { new TransactionListDto { TransactionId = 1 } };

            //Arrange 2
            _mockRepo.GetPendingRefundRequestsAsync(1, 10).Returns(Task.FromResult((list, 1)));

            //Act
            var result = await _financeService.GetPendingRefundRequestsAsync(1, 10);

            //Assert
            result.Items.Should().HaveCount(1);
            result.TotalCount.Should().Be(1);
        }

        [Fact]
        public async Task ApproveRefundAsync_NotPending_ThrowsException()
        {
            //Arrange 1
            var txn = new Transaction { TransactionsStatus = "succeeded" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction is not in pending refund approval status.");
        }

        [Fact]
        public async Task ApproveRefundAsync_Valid_ApprovesAndRefunds()
        {
            //Arrange 1
            var txn = new Transaction 
            { 
                TransactionId = 1, 
                TransactionsStatus = "refund_pending",
                StripePaymentintentId = "pi_1",
                Amount = 100m,
                AccountFrom = 2,
                TransactionExt = new TransactionExt { RefundReason = "Test" }
            };

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));

            //Act
            await _financeService.ApproveRefundAsync(1, "Approved Note");

            //Assert
            txn.TransactionsStatus.Should().Be("refunded");
            txn.TransactionExt!.RefundAdminNote.Should().Be("Approved Note");
            await _mockNotiService.Received(1).SendNotificationAsync(2, "Refund Request APPROVED", Arg.Any<string>(), "/Transaction/History");
        }

        [Fact]
        public async Task RejectRefundAsync_Valid_Rejects()
        {
            //Arrange 1
            var txn = new Transaction 
            { 
                TransactionId = 1, 
                TransactionsStatus = "refund_pending",
                AccountFrom = 2
            };

            //Arrange 2
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));

            //Act
            await _financeService.RejectRefundAsync(1, "Rejected Note");

            //Assert
            txn.TransactionsStatus.Should().Be("succeeded");
            txn.TransactionExt!.RefundAdminNote.Should().Be("Rejected Note");
            await _mockRepo.Received(1).SaveChangesAsync();
            await _mockNotiService.Received(1).SendNotificationAsync(2, "Refund Request REJECTED", Arg.Any<string>(), "/Transaction/History");
        }

        [Fact]
        public async Task GetInstructorCourseRevenuesAsync_ValidRequest_ReturnsData()
        {
            //Arrange 1
            var list = new List<InstructorCourseRevenueProjection>
            {
                new InstructorCourseRevenueProjection { CourseId = 1, CourseTitle = "C1", InstructorId = 1 }
            };

            //Arrange 2
            _mockRepo.GetInstructorCourseRevenuesAsync(2023, 1).Returns(Task.FromResult(list));

            //Act
            var result = await _financeService.GetInstructorCourseRevenuesAsync(2023, 1);

            //Assert
            result.Should().HaveCount(1);
            result.First().CourseTitle.Should().Be("C1");
        }

        [Fact]
        public async Task GetStripeCountriesAsync_NoConfig_ReturnsEmptyList()
        {
            //Arrange 1
            _mockConfigRepo.GetValueAsync("StripeCountries").Returns(Task.FromResult<string?>(null));

            //Act
            var result = await _financeService.GetStripeCountriesAsync();

            //Assert
            result.Should().BeEmpty();
        }

        [Fact]
        public async Task GetStripeCountriesAsync_HasConfig_ReturnsList()
        {
            //Arrange 1
            var json = "[{\"id\":\"US\",\"name\":\"United States\"}]";
            _mockConfigRepo.GetValueAsync("StripeCountries").Returns(Task.FromResult<string?>(json));

            //Act
            var result = await _financeService.GetStripeCountriesAsync();

            //Assert
            result.Should().HaveCount(1);
        }

        #endregion
        #region Phase 5 Tests (100% Coverage)

        // --- RequestRefundAsync ---
        [Fact]
        public async Task RequestRefundAsync_TransactionNotFound_ThrowsException()
        {
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(null));
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction not found.");
        }

        [Fact]
        public async Task RequestRefundAsync_WrongStudent_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 2 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You do not own this transaction.");
        }

        [Fact]
        public async Task RequestRefundAsync_PendingRefund_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 1, TransactionsStatus = "refund_pending" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This transaction is currently pending refund approval.");
        }

        [Fact]
        public async Task RequestRefundAsync_AlreadyRefunded_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 1, TransactionsStatus = "refunded" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This transaction has already been refunded.");
        }

        [Fact]
        public async Task RequestRefundAsync_Exceeds14Days_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow.AddDays(-15) };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("The transaction has exceeded the 14-day refund period required by platform rules.");
        }

        [Fact]
        public async Task RequestRefundAsync_GiftClaimed_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItemId = 5 };
            var gift = new Gift { IsClaimed = true };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));
            
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This gift has already been claimed, refund is not allowed.");
        }

        [Fact]
        public async Task RequestRefundAsync_NoCourseId_ThrowsException()
        {
            var txn = new Transaction { AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = null } };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            
            System.Func<Task> act = async () => await _financeService.RequestRefundAsync(1, 1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Course not found for this transaction.");
        }

        [Fact]
        public async Task RequestRefundAsync_AutoReject_TooManyRefunds()
        {
            var txn = new Transaction { TransactionId = 1, AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = 1 } };
            var metrics = new RefundEligibilityDto { RefundRequestsLast14DaysCount = 3 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");
            result.RejectReason.Should().Be("having requested too many refunds within the refund period");
        }

        [Fact]
        public async Task RequestRefundAsync_AutoReject_PastRefunded()
        {
            var txn = new Transaction { TransactionId = 1, AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = 1 }, TransactionExt = new TransactionExt() };
            var metrics = new RefundEligibilityDto { PastRefundedCountForCourse = 1 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");
            result.RejectReason.Should().Be("previous refund history for this course");
        }

        [Fact]
        public async Task RequestRefundAsync_AutoReject_ShortCourseExceedsProgress()
        {
            var txn = new Transaction { TransactionId = 1, AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = 1 } };
            var metrics = new RefundEligibilityDto { CourseTotalDurationHours = 3.0, StudentProgressPercentage = 16.0 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");
            result.RejectReason.Should().Be("learning progress exceeding the limit for short courses");
        }

        [Fact]
        public async Task RequestRefundAsync_AutoReject_LongCourseExceedsWatchTime()
        {
            var txn = new Transaction { TransactionId = 1, AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = 1 } };
            var metrics = new RefundEligibilityDto { CourseTotalDurationHours = 5.0, CompletedVideoDurationHours = 2.0 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");
            result.RejectReason.Should().Be("video watch time exceeding the limit allowed for long courses");
        }

        [Fact]
        public async Task RequestRefundAsync_ValidRequest_WithExistingExt()
        {
            var txn = new Transaction { TransactionId = 1, AccountFrom = 1, TransactionsStatus = "succeeded", TransactionCreatedAt = DateTime.UtcNow, OrderItem = new OrderItem { CourseId = 1 }, TransactionExt = new TransactionExt() };
            var metrics = new RefundEligibilityDto { };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetRefundEligibilityMetricsAsync(1, 1, 1).Returns(Task.FromResult(metrics));
            _mockNotiService.SendNotificationAsync(1, Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Fail"))); // Cover catch block
            
            var result = await _financeService.RequestRefundAsync(1, 1, "Reason");
            result.IsAutoRejected.Should().BeFalse();
            txn.TransactionExt.RefundAdminNote.Should().BeNull();
        }

        // --- ApproveRefundAsync ---
        [Fact]
        public async Task ApproveRefundAsync_TransactionNotFound_ThrowsException()
        {
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(null));
            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction not found.");
        }

        [Fact]
        public async Task ApproveRefundAsync_GiftClaimed_RejectsAndThrowsException()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", OrderItemId = 5, AccountFrom = 2 };
            var gift = new Gift { IsClaimed = true };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Fail")));

            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("This gift has already been claimed, refund is not allowed.");
            txn.TransactionsStatus.Should().Be("succeeded");
            txn.TransactionExt.Should().NotBeNull();
        }

        [Fact]
        public async Task ApproveRefundAsync_GiftClaimed_WithExistingExt()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", OrderItemId = 5, AccountFrom = 2, TransactionExt = new TransactionExt() };
            var gift = new Gift { IsClaimed = true };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));

            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>();
            txn.TransactionExt.RefundAdminNote.Should().Be("This gift has already been claimed, refund is not allowed.");
        }

        [Fact]
        public async Task ApproveRefundAsync_Valid_WithExistingExtAndFailingNoti()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", AccountFrom = 2, TransactionExt = new TransactionExt(), StripePaymentintentId = "pi_1" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", Arg.Any<decimal>(), Arg.Any<string>()).Returns(Task.FromResult("re_1"));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Fail")));

            await _financeService.ApproveRefundAsync(1, "Note");
            txn.TransactionExt.RefundAdminNote.Should().Be("Note");
        }

        // --- RejectRefundAsync ---
        [Fact]
        public async Task RejectRefundAsync_TransactionNotFound_ThrowsException()
        {
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(null));
            System.Func<Task> act = async () => await _financeService.RejectRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction not found.");
        }

        [Fact]
        public async Task RejectRefundAsync_NotPending_ThrowsException()
        {
            var txn = new Transaction { TransactionsStatus = "succeeded" };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            System.Func<Task> act = async () => await _financeService.RejectRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Transaction is not in pending refund approval status.");
        }

        [Fact]
        public async Task RejectRefundAsync_Valid_WithExistingExtAndFailingNoti()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", AccountFrom = 2, TransactionExt = new TransactionExt() };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromException<bool>(new Exception("Fail")));

            await _financeService.RejectRefundAsync(1, "Note");
            txn.TransactionExt.RefundAdminNote.Should().Be("Note");
        }

        // --- RefundTransactionAsync (Missing Branches) ---
        [Fact]
        public async Task RefundTransactionAsync_WithExistingExt_ProcessesRefund()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = false };
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout> { payout }, TransactionExt = new TransactionExt() };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));

            var result = await _financeService.RefundTransactionAsync(1, "Test");
            txn.TransactionExt.RefundReason.Should().Be("Test");
            txn.TransactionExt.RefundRequestedAt.Should().NotBeNull();
        }

        [Fact]
        public async Task RefundTransactionAsync_NoInstructorPayout_NoTransfer()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout>() };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));

            var result = await _financeService.RefundTransactionAsync(1, "Test");
            result.RefundedAmount.Should().Be(100m);
        }

        [Fact]
        public async Task RefundTransactionAsync_InstructorPayoutPaid_WithNoTransferId_SkipsReversal()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = true, StripeTransferId = null };
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout> { payout } };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));

            var result = await _financeService.RefundTransactionAsync(1, "Test");
            payout.PayoutStatus.Should().Be("refunded");
        }

        // --- SyncAllPayoutsWithStripeAsync (Missing Branches) ---
        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_GetPayoutsThrows_LogsAndContinues()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromException<List<StripePayoutDto>>(new Exception("API Error")));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            // Should not throw
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_ExistingPayout_Refunded_DoesNotUpdate()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            var stripePayouts = new List<StripePayoutDto> { new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow } };
            var dbPayouts = new List<InstructorPayout> { new InstructorPayout { StripePayoutId = "po_1", PayoutStatus = "refunded" } };

            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromResult(dbPayouts));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            dbPayouts[0].PayoutStatus.Should().Be("refunded"); // Status should not change
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_StatusInTransit_MarksInTransit()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            var stripePayouts = new List<StripePayoutDto> { new StripePayoutDto { Id = "po_1", Status = "in_transit" } };
            var dbPayouts = new List<InstructorPayout> { new InstructorPayout { StripePayoutId = "po_1", PayoutStatus = "pending" } };

            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromResult(dbPayouts));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            dbPayouts[0].PayoutStatus.Should().Be("in_transit");
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_StatusFailed_MarksFailed()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            var stripePayouts = new List<StripePayoutDto> { new StripePayoutDto { Id = "po_1", Status = "failed" } };
            var dbPayouts = new List<InstructorPayout> { new InstructorPayout { StripePayoutId = "po_1", PayoutStatus = "pending", IsPaid = true } };

            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromResult(dbPayouts));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            dbPayouts[0].PayoutStatus.Should().Be("failed");
            dbPayouts[0].IsPaid.Should().BeFalse();
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_StatusUnknown_DoesNotUpdate()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            var stripePayouts = new List<StripePayoutDto> { new StripePayoutDto { Id = "po_1", Status = "unknown" } };
            var dbPayouts = new List<InstructorPayout> { new InstructorPayout { StripePayoutId = "po_1", PayoutStatus = "pending" } };

            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromResult(dbPayouts));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            dbPayouts[0].PayoutStatus.Should().Be("pending"); // Unknown status has no if branch
        }
        
        #endregion
        #region Phase 6 Tests (Final 100% Coverage)

        // --- RefundTransactionAsync: Revoke Enrollments & Gifts ---
        [Fact]
        public async Task RefundTransactionAsync_WithGiftAndRecipientEnrollment_RevokesBoth()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", TransactionType = "gift_purchase", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout> { new InstructorPayout { InstructorId = 1 } }, OrderItemId = 5, OrderItem = new OrderItem { CourseId = 10 } };
            var gift = new Gift { ClaimedByUserId = 2, DeliveryStatus = "delivered", IsClaimed = false };
            var enrollment = new Enrollment { EnrollmentStatus = "active" };

            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));
            _mockCourseRepo.GetActiveEnrollmentAsync(2, 10).Returns(Task.FromResult<Enrollment?>(enrollment));
            
            await _financeService.RefundTransactionAsync(1, "Test");

            gift.DeliveryStatus.Should().Be("refunded");
            enrollment.EnrollmentStatus.Should().Be("revoked");
        }

        [Fact]
        public async Task RefundTransactionAsync_WithBuyerEnrollment_RevokesEnrollment()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout>(), AccountFrom = 3, OrderItem = new OrderItem { CourseId = 10 }, AccountFromNavigation = new Account { User = new User { UserId = 3 } } };
            var enrollment = new Enrollment { EnrollmentStatus = "active" };

            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));
            _mockCourseRepo.GetActiveEnrollmentAsync(3, 10).Returns(Task.FromResult<Enrollment?>(enrollment));
            
            await _financeService.RefundTransactionAsync(1, "Test");

            enrollment.EnrollmentStatus.Should().Be("revoked");
        }

        [Fact]
        public async Task RefundTransactionAsync_SignalRFails_ContinuesWithoutThrowing()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = false, InstructorId = 1 };
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", Amount = 100m, InstructorPayouts = new List<InstructorPayout> { payout }, AccountFrom = 3, OrderItem = new OrderItem { CourseId = 10 } };
            
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockPaymentGateway.RefundAsync("pi_1", 100m, "Test").Returns(Task.FromResult("re_1"));
            _mockHubContext.Clients.Group(Arg.Any<string>()).Returns(x => throw new Exception("SignalR Error"));

            await _financeService.RefundTransactionAsync(1, "Test");
            // Should not throw
            payout.PayoutStatus.Should().Be("refunded");
        }

        // --- ApproveRefundAsync: Gift Claimed no existing TransactionExt ---
        [Fact]
        public async Task ApproveRefundAsync_GiftClaimed_NoExt_CreatesExtAndRejects()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", OrderItemId = 5, AccountFrom = 2 };
            var gift = new Gift { IsClaimed = true };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));

            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>();
            txn.TransactionExt.Should().NotBeNull();
            txn.TransactionExt!.RefundAdminNote.Should().Be("This gift has already been claimed, refund is not allowed.");
        }

        // --- SyncAllPayoutsWithStripeAsync: Empty Catch Block ---
        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_GetPayoutsByStripePayoutIdThrows_Caught()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            var stripePayouts = new List<StripePayoutDto> { new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow } };

            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(stripePayouts));
            _mockRepo.GetPayoutsByStripePayoutIdAsync("po_1").Returns(Task.FromException<List<InstructorPayout>>(new Exception("DB Error")));

            await _financeService.SyncAllPayoutsWithStripeAsync();
            // Should not throw, catch block consumes it
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        #endregion
        #region Phase 7 Tests (The Final Stretch)

        // --- Config Methods: SystemConfig returns null ---
        [Fact]
        public async Task GetCurrentTransferRateAsync_ConfigNotFound_CreatesDefault()
        {
            _mockConfigRepo.GetValueAsync("PlatformTransferRate").Returns(Task.FromResult<string?>(null));
            
            var rate = await _financeService.GetCurrentTransferRateAsync();
            rate.Should().Be(70.00m); // DefaultTransferRate
        }

        [Fact]
        public async Task GetPayoutDaysConfigAsync_ConfigNotFound_CreatesDefault()
        {
            _mockConfigRepo.GetValueAsync("PayoutDays").Returns(Task.FromResult<string?>(null));
            
            var days = await _financeService.GetPayoutDaysConfigAsync();
            days.Should().Be("15"); // DefaultPayoutProcessingDays
        }

        // --- CreateWithdrawalAsync: Exceptions & SignalR ---
        [Fact]
        public async Task CreateWithdrawalAsync_StripeError_ThrowsException()
        {
            var req = new WithdrawRequest { Amount = 10m };
            _mockConfigRepo.GetValueAsync("PlatformBalanceUsd").Returns(Task.FromResult<string?>("100.00"));
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(new StripePlatformBalanceDto { Available = 100m, Incoming = 0m, Currency = "usd" }));
            _mockStripeConnect.CreatePlatformWithdrawalAsync(10m, Arg.Any<string>(), 1).Returns(Task.FromException<StripeWithdrawalResponseDto>(new Exception("Stripe down")));

            System.Func<Task> act = async () => await _financeService.CreateWithdrawalAsync(req, 1);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stripe Payout error: Stripe down");
        }

        [Fact]
        public async Task CreateWithdrawalAsync_SignalRError_CatchesException()
        {
            var req = new WithdrawRequest { Amount = 10m };
            _mockConfigRepo.GetValueAsync("PlatformBalanceUsd").Returns(Task.FromResult<string?>("100.00"));
            _mockStripeConnect.GetPlatformBalanceAsync().Returns(Task.FromResult(new StripePlatformBalanceDto { Available = 100m, Incoming = 0m, Currency = "usd" }));
            _mockStripeConnect.CreatePlatformWithdrawalAsync(10m, Arg.Any<string>(), 1).Returns(Task.FromResult(new StripeWithdrawalResponseDto { Id = "wd_1", Status = "pending" }));
            _mockHubContext.Clients.Group(Arg.Any<string>()).Returns(x => throw new Exception("SignalR Down"));

            var result = await _financeService.CreateWithdrawalAsync(req, 1);
            result.StripePayoutId.Should().Be("wd_1");
            // Should not throw
        }

        // --- PerformStripeTransferAsync: Exceptions & SignalR ---
        [Fact]
        public async Task PerformStripeTransferAsync_SignalRError_CatchesException()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = false, Instructor = new Instructor { StripeAccountId = "acct_1" }, Transaction = new Transaction() };
            _mockRepo.GetPayoutByIdAsync(1).Returns(Task.FromResult<InstructorPayout?>(payout));
            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), "acct_1", Arg.Any<string>(), 1).Returns(Task.FromResult(new StripeTransferResponseDto { Id = "py_1", DestinationAmount = 10m, DestinationPaymentId = "py_1" }));
            _mockHubContext.Clients.Group(Arg.Any<string>()).Returns(x => throw new Exception("SignalR Down"));

            var result = await _financeService.PerformStripeTransferAsync(1);
            result.Should().Be("py_1");
            // Should not throw
        }

        [Fact]
        public async Task PerformStripeTransferAsync_StripeErrorAndSignalRError_CatchesSignalRAndThrowsStripeError()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = false, Instructor = new Instructor { StripeAccountId = "acct_1" }, Transaction = new Transaction() };
            _mockRepo.GetPayoutByIdAsync(1).Returns(Task.FromResult<InstructorPayout?>(payout));
            _mockStripeConnect.CreateConnectTransferAsync(Arg.Any<decimal>(), Arg.Any<string>(), "acct_1", Arg.Any<string>(), 1).Returns(Task.FromException<StripeTransferResponseDto>(new Exception("Stripe Failed")));
            _mockHubContext.Clients.Group(Arg.Any<string>()).Returns(x => throw new Exception("SignalR Down"));

            System.Func<Task> act = async () => await _financeService.PerformStripeTransferAsync(1);
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stripe Transfer error: Stripe Failed");
        }

        // --- RefundTransactionAsync: Transfer ID null & Stripe Error ---
        [Fact]
        public async Task RefundTransactionAsync_StripeTransferIdNull_ThrowsException()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = true, StripeTransferId = "py_1" };
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", InstructorPayouts = new List<InstructorPayout> { payout } };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetStripeTransferIdByDestinationPaymentAsync("py_1").Returns(Task.FromResult<string?>(null));

            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot find original Stripe Transfer ID for DestPaymentId=*");
        }

        [Fact]
        public async Task RefundTransactionAsync_RefundStripeError_ThrowsException()
        {
            var payout = new InstructorPayout { PayoutId = 1, IsPaid = true, StripeTransferId = "py_1" };
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "succeeded", StripePaymentintentId = "pi_1", InstructorPayouts = new List<InstructorPayout> { payout } };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockRepo.GetStripeTransferIdByDestinationPaymentAsync("py_1").Returns(Task.FromResult("tr_1"));
            _mockPaymentGateway.ReverseTransferAsync("tr_1").Returns(Task.FromResult("re_1"));
            _mockPaymentGateway.RefundAsync("pi_1", Arg.Any<decimal>(), Arg.Any<string>()).Returns(Task.FromException<string>(new Exception("Refund Failed")));

            System.Func<Task> act = async () => await _financeService.RefundTransactionAsync(1, "Reason");
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Stripe refund error: Refund Failed");
        }

        // --- ApproveRefundAsync: No Ext & Catch SignalR ---
        [Fact]
        public async Task ApproveRefundAsync_Valid_NoExistingExt_CreatesNewExt()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", TransactionType = "gift_purchase", OrderItemId = 5, AccountFrom = 2, StripePaymentintentId = "pi_1" }; // TransactionExt is null
            var gift = new Gift { IsClaimed = false };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));
            _mockPaymentGateway.RefundAsync("pi_1", Arg.Any<decimal>(), Arg.Any<string>()).Returns(Task.FromResult("re_1"));
            _mockNotiService.SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>()).Returns(Task.FromResult(true));
            _mockHubContext.Clients.All.Returns(x => throw new Exception("SignalR Down")); 

            await _financeService.ApproveRefundAsync(1, "Note");
            txn.TransactionExt.Should().NotBeNull();
            txn.TransactionExt!.RefundAdminNote.Should().Be("Note");
        }

        [Fact]
        public async Task ApproveRefundAsync_GiftClaimed_WithSignalRError_Catches()
        {
            var txn = new Transaction { TransactionId = 1, TransactionsStatus = "refund_pending", OrderItemId = 5, AccountFrom = 2 };
            var gift = new Gift { IsClaimed = true, ClaimedByUserId = 2 };
            _mockRepo.GetTransactionWithFullGraphAsync(1).Returns(Task.FromResult<Transaction?>(txn));
            _mockGiftRepo.GetByOrderItemIdAsync(5).Returns(Task.FromResult<Gift?>(gift));
            _mockHubContext.Clients.User(Arg.Any<string>()).Returns(x => throw new Exception("SignalR"));

            System.Func<Task> act = async () => await _financeService.ApproveRefundAsync(1, "Note");
            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        // --- SyncAllPayoutsWithStripeAsync: GetPlatformWithdrawalAsync Error ---
        [Fact]
        public async Task GetWithdrawalHistoryAsync_StripeError_CatchesException()
        {
            var withdrawals = new List<Domain.Entities.PlatformWithdrawal> { new Domain.Entities.PlatformWithdrawal { StripePayoutId = "wd_1", Status = "pending" } };
            _mockRepo.GetWithdrawalsAsync(null, null, 1, 10).Returns(Task.FromResult((withdrawals, 1)));
            _mockStripeConnect.GetPlatformPayoutStatusAsync("wd_1").Returns(Task.FromException<StripeWithdrawalResponseDto>(new Exception("Stripe API down")));

            var result = await _financeService.GetWithdrawalHistoryAsync();
            // Should ignore the exception and keep old status
            withdrawals[0].Status.Should().Be("pending");
        }

        [Fact]
        public async Task SyncAllPayoutsWithStripeAsync_SignalRError_CaughtAndIgnored()
        {
            var instructors = new List<Instructor> { new Instructor { InstructorId = 1, StripeAccountId = "acct_1" } };
            _mockInstructorRepo.GetInstructorsWithStripeAsync().Returns(Task.FromResult(instructors));
            _mockStripeConnect.ListPayoutsAsync("acct_1").Returns(Task.FromResult(new List<StripePayoutDto>()));
            _mockHubContext.Clients.Group(Arg.Any<string>()).Returns(x => throw new Exception("SignalR Down")); // Covers line 803

            await _financeService.SyncAllPayoutsWithStripeAsync();
            // Should not throw
            await _mockRepo.Received(1).SaveChangesAsync();
        }

        #endregion


}
}
