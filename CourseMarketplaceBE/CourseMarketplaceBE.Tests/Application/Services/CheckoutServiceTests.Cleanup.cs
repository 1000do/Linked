using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using CourseMarketplaceBE.Domain.Entities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public partial class CheckoutServiceTests
    {
        // ═══════════════════════════════════════════════════════════════════════════
        // CleanupAbandonedPendingOrdersAsync (Private Method via Reflection)
        // ═══════════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task CleanupAbandonedPendingOrdersAsync_NoAbandonedOrders_DoesNothing()
        {
            //Arrange 1
            int userId = 1;
            var abandonedOrders = new List<OrderInfo>();
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService)
                .GetMethod("CleanupAbandonedPendingOrdersAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            //Arrange 2
            _repoMock.GetPendingOrdersByUserAsync(userId).Returns(abandonedOrders);

            //Act
            var task = (Task?)methodInfo?.Invoke(_sut, new object[] { userId });
            if (task != null)
            {
                await task;
            }

            //Assert
            await _repoMock.Received(1).GetPendingOrdersByUserAsync(userId);
            await _repoMock.DidNotReceive().DeleteOrderAsync(Arg.Any<OrderInfo>());
        }

        [Fact]
        public async Task CleanupAbandonedPendingOrdersAsync_HasAbandonedOrders_DeletesOrders()
        {
            //Arrange 1
            int userId = 1;
            var abandonedOrders = new List<OrderInfo>
            {
                new OrderInfo { OrderId = 101 },
                new OrderInfo { OrderId = 102 }
            };
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService)
                .GetMethod("CleanupAbandonedPendingOrdersAsync", BindingFlags.NonPublic | BindingFlags.Instance);

            //Arrange 2
            _repoMock.GetPendingOrdersByUserAsync(userId).Returns(abandonedOrders);
            _repoMock.GetTransactionsByOrderIdAsync(Arg.Any<int>()).Returns(new List<Transaction>());

            //Act
            var task = (Task?)methodInfo?.Invoke(_sut, new object[] { userId });
            if (task != null)
            {
                await task;
            }

            //Assert
            await _repoMock.Received(1).GetPendingOrdersByUserAsync(userId);
            await _repoMock.Received(1).DeleteOrderAsync(Arg.Is<OrderInfo>(o => o.OrderId == 101));
            await _repoMock.Received(1).DeleteOrderAsync(Arg.Is<OrderInfo>(o => o.OrderId == 102));
        }

        [Fact]
        public async Task CleanupAbandonedPendingOrdersAsync_HasTransactions_DeletesTransactions()
        {
            // Arrange 1
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService).GetMethod("CleanupAbandonedPendingOrdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int userId = 1;
            var pendingOrders = new List<OrderInfo> { new OrderInfo { OrderId = 100 } };
            var transactions = new List<Transaction> { new Transaction { TransactionId = 200 } };

            // Arrange 2
            _repoMock.GetPendingOrdersByUserAsync(userId).Returns(pendingOrders);
            _repoMock.GetTransactionsByOrderIdAsync(100).Returns(transactions);

            // Act
            var task = (Task?)methodInfo?.Invoke(_sut, new object[] { userId });
            if (task != null) await task;

            // Assert
            await _repoMock.Received(1).DeleteTransactionsAsync(transactions);
            await _repoMock.Received(1).DeleteOrderAsync(pendingOrders[0]);
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task CleanupAbandonedPendingOrdersAsync_ExceptionThrown_LogsError()
        {
            // Arrange 1
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService).GetMethod("CleanupAbandonedPendingOrdersAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            int userId = 1;

            // Arrange 2
            _repoMock.GetPendingOrdersByUserAsync(userId).Returns(Task.FromException<List<OrderInfo>>(new Exception("DB Error")));

            // Act
            var task = (Task?)methodInfo?.Invoke(_sut, new object[] { userId });
            if (task != null) await task;

            // Assert
            // (Logger should log the error without throwing it up)
            await _repoMock.Received(1).GetPendingOrdersByUserAsync(userId);
            await _repoMock.DidNotReceive().DeleteOrderAsync(Arg.Any<OrderInfo>());
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // GetCurrencyFromCountry (Private Method via Reflection)
        // ═══════════════════════════════════════════════════════════════════════════
        [Theory]
        [InlineData("US", "USD")]
        [InlineData("GB", "GBP")]
        [InlineData("CA", "CAD")]
        [InlineData("CH", "CHF")]
        [InlineData("FR", "EUR")]
        [InlineData("DE", "EUR")]
        [InlineData("HR", "EUR")]
        [InlineData("BG", "BGN")]
        [InlineData("CZ", "CZK")]
        [InlineData("DK", "DKK")]
        [InlineData("HU", "HUF")]
        [InlineData("IS", "ISK")]
        [InlineData("NO", "NOK")]
        [InlineData("PL", "PLN")]
        [InlineData("RO", "RON")]
        [InlineData("SE", "SEK")]
        [InlineData("AU", "AUD")]
        [InlineData("VN", "VND")]
        [InlineData("", "USD")]
        [InlineData(null, "USD")]
        [InlineData("UNKNOWN", "USD")]
        public void GetCurrencyFromCountry_ReturnsExpectedCurrency(string? countryCode, string expectedCurrency)
        {
            //Arrange 1
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService)
                .GetMethod("GetCurrencyFromCountry", BindingFlags.NonPublic | BindingFlags.Instance);

            //Act
            var result = (string?)methodInfo?.Invoke(_sut, new object?[] { countryCode });

            //Assert
            result.Should().Be(expectedCurrency);
        }

        [Theory]
        [InlineData("15,20,30", 15)]
        [InlineData("30, 20", 30)] // Tests daysInNextMonth limit depending on month
        [InlineData("invalid", 15)]
        [InlineData("-5", 15)]
        [InlineData(null, 15)]
        public async Task CalculatePayoutDateAsync_VariousConfigs_ReturnsExpectedDate(string? config, int expectedDay)
        {
            // Arrange 1
            var methodInfo = typeof(CourseMarketplaceBE.Application.Services.CheckoutService).GetMethod("CalculatePayoutDateAsync", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var transactionDate = new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            
            // Arrange 2
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(config);

            // Act
            var task = (Task<DateTime>)methodInfo.Invoke(_sut, new object[] { transactionDate });
            var result = await task;

            // Assert
            var expectedMonth = transactionDate.AddMonths(1);
            var daysInMonth = DateTime.DaysInMonth(expectedMonth.Year, expectedMonth.Month);
            var expectedFinalDay = Math.Min(expectedDay, daysInMonth);

            result.Should().Be(new DateTime(expectedMonth.Year, expectedMonth.Month, expectedFinalDay, 0, 0, 0, DateTimeKind.Utc));
        }
    }
}
