using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class TransactionServiceTests
    {
        private readonly ITransactionRepository _repoMock;
        private readonly TransactionService _sut;

        public TransactionServiceTests()
        {
            _repoMock = Substitute.For<ITransactionRepository>();
            _sut = new TransactionService(_repoMock);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetTransactionsAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetTransactionsAsync_PageIsLessThanOne_SetsPageToOneAndCallsRepo()
        {
            //Arrange 1
            int invalidPage = 0;
            int pageSize = 20;
            int expectedPage = 1;
            var expectedItems = new List<TransactionListDto> { new TransactionListDto { TransactionId = 1 } };
            int expectedTotalCount = 1;

            //Arrange 2
            _repoMock.GetTransactionsAsync(expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetTransactionsAsync(page: invalidPage, pageSize: pageSize);

            //Assert
            result.Page.Should().Be(expectedPage);
            result.Items.Should().BeEquivalentTo(expectedItems);
            result.TotalCount.Should().Be(expectedTotalCount);

            await _repoMock.Received(1).GetTransactionsAsync(expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeIsLessThanOne_SetsPageSizeTo20AndCallsRepo()
        {
            //Arrange 1
            int page = 1;
            int invalidPageSize = 0;
            int expectedPageSize = 20;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetTransactionsAsync(page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetTransactionsAsync(page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(expectedPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);
            result.TotalCount.Should().Be(expectedTotalCount);

            await _repoMock.Received(1).GetTransactionsAsync(page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetTransactionsAsync_PageSizeIsGreaterThanMax_SetsPageSizeToMaxAndCallsRepo()
        {
            //Arrange 1
            int page = 1;
            int maxPageSize = 100;
            int invalidPageSize = 150;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetTransactionsAsync(page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetTransactionsAsync(page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(maxPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);
            result.TotalCount.Should().Be(expectedTotalCount);

            await _repoMock.Received(1).GetTransactionsAsync(page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetTransactionsAsync_ValidPagination_CallsRepoAndReturnsPagedResult()
        {
            //Arrange 1
            int page = 2;
            int pageSize = 15;
            string keyword = "test";
            string sortBy = "date_asc";
            string status = "Success";
            int year = 2023;
            int month = 5;
            var expectedItems = new List<TransactionListDto> { new TransactionListDto { TransactionId = 1 } };
            int expectedTotalCount = 50;

            //Arrange 2
            _repoMock.GetTransactionsAsync(page, pageSize, keyword, sortBy, status, year, month)
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetTransactionsAsync(page, pageSize, keyword, sortBy, status, year, month);

            //Assert
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(expectedTotalCount);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetTransactionsAsync(page, pageSize, keyword, sortBy, status, year, month);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetTransactionDetailAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetTransactionDetailAsync_TransactionIdIsZeroOrLess_ThrowsArgumentException()
        {
            //Arrange 1
            int invalidId = 0;

            //Arrange 2
            // No repo setup needed since it throws early

            //Act
            Func<Task> act = async () => await _sut.GetTransactionDetailAsync(invalidId);

            //Assert
            await act.Should().ThrowAsync<ArgumentException>()
                     .WithMessage("*Invalid Transaction ID.*");

            await _repoMock.DidNotReceive().GetTransactionDetailAsync(Arg.Any<int>());
        }

        [Fact]
        public async Task GetTransactionDetailAsync_TransactionNotFound_ReturnsNull()
        {
            //Arrange 1
            int transactionId = 1;

            //Arrange 2
            _repoMock.GetTransactionDetailAsync(transactionId).Returns((TransactionDetailDto?)null);

            //Act
            var result = await _sut.GetTransactionDetailAsync(transactionId);

            //Assert
            result.Should().BeNull();

            await _repoMock.Received(1).GetTransactionDetailAsync(transactionId);
        }

        [Fact]
        public async Task GetTransactionDetailAsync_ValidTransactionId_ReturnsTransactionDetail()
        {
            //Arrange 1
            int transactionId = 1;
            var expectedDetail = new TransactionDetailDto { TransactionId = transactionId };

            //Arrange 2
            _repoMock.GetTransactionDetailAsync(transactionId).Returns(expectedDetail);

            //Act
            var result = await _sut.GetTransactionDetailAsync(transactionId);

            //Assert
            result.Should().BeEquivalentTo(expectedDetail);

            await _repoMock.Received(1).GetTransactionDetailAsync(transactionId);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetInstructorTransactionsAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetInstructorTransactionsAsync_PageIsLessThanOne_SetsPageToOneAndCallsRepo()
        {
            //Arrange 1
            int instructorId = 1;
            int invalidPage = 0;
            int pageSize = 20;
            int expectedPage = 1;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetInstructorTransactionsAsync(instructorId, expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetInstructorTransactionsAsync(instructorId, page: invalidPage, pageSize: pageSize);

            //Assert
            result.Page.Should().Be(expectedPage);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetInstructorTransactionsAsync(instructorId, expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetInstructorTransactionsAsync_PageSizeIsLessThanOne_SetsPageSizeTo20AndCallsRepo()
        {
            //Arrange 1
            int instructorId = 1;
            int page = 1;
            int invalidPageSize = 0;
            int expectedPageSize = 20;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetInstructorTransactionsAsync(instructorId, page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetInstructorTransactionsAsync(instructorId, page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(expectedPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetInstructorTransactionsAsync(instructorId, page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetInstructorTransactionsAsync_PageSizeIsGreaterThanMax_SetsPageSizeToMaxAndCallsRepo()
        {
            //Arrange 1
            int instructorId = 1;
            int page = 1;
            int maxPageSize = 100;
            int invalidPageSize = 150;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetInstructorTransactionsAsync(instructorId, page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetInstructorTransactionsAsync(instructorId, page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(maxPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetInstructorTransactionsAsync(instructorId, page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<int?>(), Arg.Any<int?>());
        }

        [Fact]
        public async Task GetInstructorTransactionsAsync_ValidPagination_CallsRepoAndReturnsPagedResult()
        {
            //Arrange 1
            int instructorId = 1;
            int page = 2;
            int pageSize = 15;
            string keyword = "test";
            string sortBy = "date_asc";
            string status = "Success";
            int year = 2023;
            int month = 5;
            var expectedItems = new List<TransactionListDto> { new TransactionListDto { TransactionId = 2 } };
            int expectedTotalCount = 50;

            //Arrange 2
            _repoMock.GetInstructorTransactionsAsync(instructorId, page, pageSize, keyword, sortBy, status, year, month)
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetInstructorTransactionsAsync(instructorId, page, pageSize, keyword, sortBy, status, year, month);

            //Assert
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(expectedTotalCount);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetInstructorTransactionsAsync(instructorId, page, pageSize, keyword, sortBy, status, year, month);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetUserTransactionsAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetUserTransactionsAsync_PageIsLessThanOne_SetsPageToOneAndCallsRepo()
        {
            //Arrange 1
            int userId = 1;
            int invalidPage = 0;
            int pageSize = 20;
            int expectedPage = 1;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetUserTransactionsAsync(userId, expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetUserTransactionsAsync(userId, page: invalidPage, pageSize: pageSize);

            //Assert
            result.Page.Should().Be(expectedPage);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetUserTransactionsAsync(userId, expectedPage, pageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task GetUserTransactionsAsync_PageSizeIsLessThanOne_SetsPageSizeTo20AndCallsRepo()
        {
            //Arrange 1
            int userId = 1;
            int page = 1;
            int invalidPageSize = 0;
            int expectedPageSize = 20;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetUserTransactionsAsync(userId, page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetUserTransactionsAsync(userId, page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(expectedPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetUserTransactionsAsync(userId, page, expectedPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task GetUserTransactionsAsync_PageSizeIsGreaterThanMax_SetsPageSizeToMaxAndCallsRepo()
        {
            //Arrange 1
            int userId = 1;
            int page = 1;
            int maxPageSize = 100;
            int invalidPageSize = 150;
            var expectedItems = new List<TransactionListDto>();
            int expectedTotalCount = 0;

            //Arrange 2
            _repoMock.GetUserTransactionsAsync(userId, page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>())
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetUserTransactionsAsync(userId, page: page, pageSize: invalidPageSize);

            //Assert
            result.PageSize.Should().Be(maxPageSize);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetUserTransactionsAsync(userId, page, maxPageSize, Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<string?>());
        }

        [Fact]
        public async Task GetUserTransactionsAsync_ValidPagination_CallsRepoAndReturnsPagedResult()
        {
            //Arrange 1
            int userId = 1;
            int page = 2;
            int pageSize = 15;
            string keyword = "test";
            string sortBy = "date_asc";
            string status = "Success";
            var expectedItems = new List<TransactionListDto> { new TransactionListDto { TransactionId = 3 } };
            int expectedTotalCount = 50;

            //Arrange 2
            _repoMock.GetUserTransactionsAsync(userId, page, pageSize, keyword, sortBy, status)
                     .Returns((expectedItems, expectedTotalCount));

            //Act
            var result = await _sut.GetUserTransactionsAsync(userId, page, pageSize, keyword, sortBy, status);

            //Assert
            result.Page.Should().Be(page);
            result.PageSize.Should().Be(pageSize);
            result.TotalCount.Should().Be(expectedTotalCount);
            result.Items.Should().BeEquivalentTo(expectedItems);

            await _repoMock.Received(1).GetUserTransactionsAsync(userId, page, pageSize, keyword, sortBy, status);
        }
    }
}
