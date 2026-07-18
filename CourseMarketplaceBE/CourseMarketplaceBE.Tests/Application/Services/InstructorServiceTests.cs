using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.DTOs.Common;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using NSubstitute;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class InstructorServiceTests
    {
        private readonly IInstructorRepository _repoMock;
        private readonly IFileUploadService _uploadServiceMock;
        private readonly IAdminFinanceRepository _financeRepoMock;
        private readonly IUserRepository _userRepoMock;
        private readonly IStripeConnectService _stripeConnectMock;
        private readonly ICourseRepository _courseRepoMock;
        private readonly InstructorService _sut;

        public InstructorServiceTests()
        {
            _repoMock = Substitute.For<IInstructorRepository>();
            _uploadServiceMock = Substitute.For<IFileUploadService>();
            _financeRepoMock = Substitute.For<IAdminFinanceRepository>();
            _userRepoMock = Substitute.For<IUserRepository>();
            _stripeConnectMock = Substitute.For<IStripeConnectService>();
            _courseRepoMock = Substitute.For<ICourseRepository>();

            _sut = new InstructorService(
                _repoMock,
                _uploadServiceMock,
                _financeRepoMock,
                _userRepoMock,
                _stripeConnectMock,
                _courseRepoMock
            );
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SubmitApplicationAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task SubmitApplicationAsync_AccountNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest();

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns((Account?)null);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Account not found.");
            await _userRepoMock.Received(1).GetAccountByIdAsync(userId);
        }

        [Fact]
        public async Task SubmitApplicationAsync_AccountNotVerified_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest();
            var account = new Account { IsVerified = false };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("EMAIL_NOT_VERIFIED");
        }

        [Fact]
        public async Task SubmitApplicationAsync_TotalFilesLessThanOne_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest { DocumentFiles = new List<IFormFile>(), RetainedDocumentUrls = new List<string>() };
            var account = new Account { IsVerified = true };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Please upload at least 1 document/certificate file.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_TotalFilesGreaterThanThree_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new InstructorApplicationRequest 
            { 
                DocumentFiles = new List<IFormFile> { mockFile, mockFile, mockFile, mockFile } 
            };
            var account = new Account { IsVerified = true };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You can upload a maximum of 3 document/certificate files.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_ExistingInstructorNotRejected_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new InstructorApplicationRequest 
            { 
                DocumentFiles = new List<IFormFile> { mockFile } 
            };
            var account = new Account { IsVerified = true };
            var existing = new Instructor { ApprovalStatus = InstructorApprovalStatus.Pending.ToValue() };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns(existing);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have already submitted an instructor application.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_ResubmitWithLessThanOneValidUrl_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest 
            { 
                RetainedDocumentUrls = new List<string> { "url1" } // But existing has no url
            };
            var account = new Account { IsVerified = true };
            var existing = new Instructor { ApprovalStatus = InstructorApprovalStatus.Rejected.ToValue() };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns(existing);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Please upload at least 1 document/certificate file.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_ResubmitWithMoreThanThreeValidUrls_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest 
            { 
                RetainedDocumentUrls = new List<string> { "url1", "url2", "url3", "url4" } 
            };
            var account = new Account { IsVerified = true };
            var existing = new Instructor { 
                ApprovalStatus = InstructorApprovalStatus.Rejected.ToValue(),
                DocumentUrl = "url1;url2;url3;url4"
            };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns(existing);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You can upload a maximum of 3 document/certificate files.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_ResubmitValid_UpdatesAndReturnsSuccessMessage()
        {
            //Arrange 1
            int userId = 1;
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new InstructorApplicationRequest 
            { 
                DocumentFiles = new List<IFormFile> { mockFile },
                RetainedDocumentUrls = new List<string> { "url1" },
                StripeCountry = "us"
            };
            var account = new Account { IsVerified = true };
            var existing = new Instructor { 
                ApprovalStatus = InstructorApprovalStatus.Rejected.ToValue(),
                DocumentUrl = "url1;old2"
            };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns(existing);
            _uploadServiceMock.UploadImageAsync(mockFile).Returns("newUrl");
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            result.Should().Be("Your application has been resubmitted. Please wait for admin approval.");
            existing.DocumentUrl.Should().Be("url1;newUrl");
            existing.StripeCountry.Should().Be("US");
            existing.ApprovalStatus.Should().Be(InstructorApprovalStatus.Pending.ToValue());
            await _uploadServiceMock.Received(1).UploadImageAsync(mockFile);
            await _repoMock.Received(1).SaveChangesAsync();
        }
        
        [Fact]
        public async Task SubmitApplicationAsync_NewApplicationWithUploadFailures_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new InstructorApplicationRequest 
            { 
                DocumentFile = mockFile, // Using DocumentFile instead of DocumentFiles to cover that branch
                StripeCountry = "us"
            };
            var account = new Account { IsVerified = true };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);
            _uploadServiceMock.UploadImageAsync(mockFile).Returns(""); // Fails

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Failed to upload document files.");
        }

        [Fact]
        public async Task SubmitApplicationAsync_NewApplicationValid_AddsAndReturnsSuccessMessage()
        {
            //Arrange 1
            int userId = 1;
            var mockFile = Substitute.For<IFormFile>();
            mockFile.Length.Returns(100);
            var request = new InstructorApplicationRequest 
            { 
                DocumentFiles = new List<IFormFile> { mockFile },
                StripeCountry = "us",
                ProfessionalTitle = "Pro",
                ExpertiseCategories = "Cat"
            };
            var account = new Account { IsVerified = true };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account);
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);
            _uploadServiceMock.UploadImageAsync(mockFile).Returns("url1");
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            result.Should().Be("Your application has been submitted. Please wait for admin approval.");
            await _repoMock.Received(1).AddAsync(Arg.Is<Instructor>(i => 
                i.InstructorId == userId && 
                i.DocumentUrl == "url1" &&
                i.StripeCountry == "US" &&
                i.ApprovalStatus == InstructorApprovalStatus.Pending.ToValue()
            ));
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ApproveApplicationAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ApproveApplicationAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.ApproveApplicationAsync(instructorId, "Approved");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Application not found.");
        }

        [Fact]
        public async Task ApproveApplicationAsync_InvalidStatus_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;
            var existing = new Instructor();

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(existing);

            //Act
            Func<Task> act = async () => await _sut.ApproveApplicationAsync(instructorId, "InvalidStatus");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid status. Only 'Approved' or 'Rejected' are allowed.");
        }

        [Fact]
        public async Task ApproveApplicationAsync_ValidApproved_UpdatesAndReturnsTrue()
        {
            //Arrange 1
            int instructorId = 1;
            var existing = new Instructor();

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(existing);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveApplicationAsync(instructorId, InstructorApprovalStatus.Approved.ToValue());

            //Assert
            result.Should().BeTrue();
            existing.ApprovalStatus.Should().Be(InstructorApprovalStatus.Approved.ToValue());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ApproveApplicationAsync_ValidRejected_UpdatesAndReturnsTrue()
        {
            //Arrange 1
            int instructorId = 1;
            var existing = new Instructor();

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(existing);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveApplicationAsync(instructorId, InstructorApprovalStatus.Rejected.ToValue());

            //Assert
            result.Should().BeTrue();
            existing.ApprovalStatus.Should().Be(InstructorApprovalStatus.Rejected.ToValue());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SetupStripePayoutAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task SetupStripePayoutAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(userId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.SetupStripePayoutAsync(userId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have not submitted an instructor application.");
        }

        [Fact]
        public async Task SetupStripePayoutAsync_NotApproved_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { ApprovalStatus = InstructorApprovalStatus.Pending.ToValue() };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(userId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.SetupStripePayoutAsync(userId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Application is not approved yet. Please wait for admin approval.");
        }

        [Fact]
        public async Task SetupStripePayoutAsync_SameStripeAccountId_ReturnsResponseWithoutSaving()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor 
            { 
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                StripeAccountId = "acct_123",
                StripeCountry = "US",
                InstructorNavigation = new User { UserNavigation = new Account { Email = "test@test.com" } }
            };
            var setupResult = new StripeConnectSetupResponse { StripeAccountId = "acct_123", OnboardingUrl = "http://url" };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(userId).Returns(instructor);
            _stripeConnectMock.SetupExpressAccountAsync(userId, "test@test.com", "US", Arg.Any<string>(), Arg.Any<string>(), "acct_123").Returns(setupResult);

            //Act
            var result = await _sut.SetupStripePayoutAsync(userId);

            //Assert
            result.StripeAccountId.Should().Be("acct_123");
            result.OnboardingUrl.Should().Be("http://url");
            await _repoMock.DidNotReceive().SaveChangesAsync();
        }

        [Fact]
        public async Task SetupStripePayoutAsync_NewStripeAccountId_SavesAndReturnsResponse()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor 
            { 
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                StripeAccountId = null, // Empty
                StripeCountry = null, // Will use SG fallback
                InstructorNavigation = new User { UserNavigation = new Account { Email = "test@test.com" } }
            };
            var setupResult = new StripeConnectSetupResponse { StripeAccountId = "acct_123", OnboardingUrl = "http://url" };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(userId).Returns(instructor);
            _stripeConnectMock.SetupExpressAccountAsync(userId, "test@test.com", "SG", Arg.Any<string>(), Arg.Any<string>(), null).Returns(setupResult);

            //Act
            var result = await _sut.SetupStripePayoutAsync(userId);

            //Assert
            result.StripeAccountId.Should().Be("acct_123");
            result.OnboardingUrl.Should().Be("http://url");
            instructor.StripeAccountId.Should().Be("acct_123");
            instructor.StripeOnboardingStatus.Should().Be(StripeOnboardingStatus.Pending.ToValue());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // VerifyStripeOnboardingAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task VerifyStripeOnboardingAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.VerifyStripeOnboardingAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor not found.");
        }

        [Fact]
        public async Task VerifyStripeOnboardingAsync_NoStripeAccount_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = null };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.VerifyStripeOnboardingAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor does not have a Stripe account.");
        }

        [Fact]
        public async Task VerifyStripeOnboardingAsync_AlreadyActive_ReturnsActiveDirectly()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct", StripeOnboardingStatus = StripeOnboardingStatus.Active.ToValue() };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);

            //Act
            var result = await _sut.VerifyStripeOnboardingAsync(instructorId);

            //Assert
            result.Should().Be(StripeOnboardingStatus.Active.ToValue());
            await _stripeConnectMock.DidNotReceive().GetAccountStatusAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task VerifyStripeOnboardingAsync_DetailsSubmittedTrue_UpdatesAndReturnsActive()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct", StripeOnboardingStatus = StripeOnboardingStatus.Pending.ToValue() };
            var statusDto = new StripeAccountStatusDto { DetailsSubmitted = true };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);
            _stripeConnectMock.GetAccountStatusAsync("acct").Returns(statusDto);

            //Act
            var result = await _sut.VerifyStripeOnboardingAsync(instructorId);

            //Assert
            result.Should().Be(StripeOnboardingStatus.Active.ToValue());
            instructor.PayoutsEnabled.Should().BeTrue();
            instructor.ChargesEnabled.Should().BeTrue();
            instructor.StripeOnboardingStatus.Should().Be(StripeOnboardingStatus.Active.ToValue());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task VerifyStripeOnboardingAsync_DetailsSubmittedFalse_UpdatesAndReturnsPending()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct", StripeOnboardingStatus = StripeOnboardingStatus.Pending.ToValue() };
            var statusDto = new StripeAccountStatusDto { DetailsSubmitted = false, PayoutsEnabled = false, ChargesEnabled = false };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);
            _stripeConnectMock.GetAccountStatusAsync("acct").Returns(statusDto);

            //Act
            var result = await _sut.VerifyStripeOnboardingAsync(instructorId);

            //Assert
            result.Should().Be(StripeOnboardingStatus.Pending.ToValue());
            instructor.PayoutsEnabled.Should().BeFalse();
            instructor.ChargesEnabled.Should().BeFalse();
            instructor.StripeOnboardingStatus.Should().Be(StripeOnboardingStatus.Pending.ToValue());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetAllApplicationsAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetAllApplicationsAsync_ReturnsApplications()
        {
            //Arrange 1
            var list = new List<InstructorDashboardDto> { new InstructorDashboardDto() };

            //Arrange 2
            _repoMock.GetAllApplicationsDtoAsync().Returns(list);

            //Act
            var result = await _sut.GetAllApplicationsAsync();

            //Assert
            result.Should().BeEquivalentTo(list);
            await _repoMock.Received(1).GetAllApplicationsDtoAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetInstructorDashboardAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetInstructorDashboardAsync_DtoNull_ReturnsNull()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetDashboardDtoAsync(userId).Returns((InstructorDashboardDto?)null);

            //Act
            var result = await _sut.GetInstructorDashboardAsync(userId);

            //Assert
            result.Should().BeNull();
        }

        [Fact]
        public async Task GetInstructorDashboardAsync_StatsNull_ReturnsDtoWithDefaultStats()
        {
            //Arrange 1
            int userId = 1;
            var dto = new InstructorDashboardDto();
            var dict = new Dictionary<string, int> { { CourseStatus.Published.ToValue(), 5 } };

            //Arrange 2
            _repoMock.GetDashboardDtoAsync(userId).Returns(dto);
            _repoMock.GetStatsAsync(userId).Returns((InstructorStats?)null);
            _courseRepoMock.CountCoursesByStatusAsync(userId).Returns(dict);
            _repoMock.GetEnrollmentGrowthAsync(userId).Returns(10.0);
            _repoMock.GetInstructorRankingPercentageAsync(userId).Returns(5);

            //Act
            var result = await _sut.GetInstructorDashboardAsync(userId);

            //Assert
            result.Should().NotBeNull();
            result!.ActiveCoursesCount.Should().Be(5);
            result.PendingCoursesCount.Should().Be(0);
            result.DraftCoursesCount.Should().Be(0);
            result.EnrollmentGrowthPercentage.Should().Be(10.0);
            result.InstructorRankPercentage.Should().Be(5);
        }

        [Fact]
        public async Task GetInstructorDashboardAsync_Valid_ReturnsPopulatedDto()
        {
            //Arrange 1
            int userId = 1;
            var dto = new InstructorDashboardDto();
            var stats = new InstructorStats { TotalStudentsCount = 10, InstructorRating = 4.5, TotalRevenue = 100m };
            var dict = new Dictionary<string, int> { { CourseStatus.Pending.ToValue(), 2 }, { CourseStatus.Draft.ToValue(), 3 } };

            //Arrange 2
            _repoMock.GetDashboardDtoAsync(userId).Returns(dto);
            _repoMock.GetStatsAsync(userId).Returns(stats);
            _courseRepoMock.CountCoursesByStatusAsync(userId).Returns(dict);

            //Act
            var result = await _sut.GetInstructorDashboardAsync(userId);

            //Assert
            result.Should().NotBeNull();
            result!.TotalStudents.Should().Be(10);
            result.AverageRating.Should().Be(4.5m);
            result.TotalRevenue.Should().Be(100m);
            result.PendingCoursesCount.Should().Be(2);
            result.DraftCoursesCount.Should().Be(3);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetRejectedApplicationInfoAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetRejectedApplicationInfoAsync_ReturnsResult()
        {
            //Arrange 1
            int userId = 1;
            var dto = new InstructorDashboardDto();

            //Arrange 2
            _repoMock.GetRejectedApplicationDtoAsync(userId).Returns(dto);

            //Act
            var result = await _sut.GetRejectedApplicationInfoAsync(userId);

            //Assert
            result.Should().BeEquivalentTo(dto);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // ResetStripeAccountAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task ResetStripeAccountAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.ResetStripeAccountAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor not found.");
        }

        [Fact]
        public async Task ResetStripeAccountAsync_NoStripeAccount_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = null };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.ResetStripeAccountAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor does not have a Stripe account.");
        }

        [Fact]
        public async Task ResetStripeAccountAsync_DeleteAccountFails_IgnoresErrorAndResetsDb()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);
            _stripeConnectMock.DeleteAccountAsync("acct").Returns(x => throw new Exception("Failed"));

            //Act
            var result = await _sut.ResetStripeAccountAsync(instructorId);

            //Assert
            result.Should().Be($"Stripe account for instructor {instructorId} has been reset. The instructor needs to set up Stripe again.");
            instructor.StripeAccountId.Should().BeNull();
            instructor.StripeOnboardingStatus.Should().BeNull();
            instructor.PayoutsEnabled.Should().BeFalse();
            instructor.ChargesEnabled.Should().BeFalse();
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task ResetStripeAccountAsync_DeleteAccountSucceeds_ResetsDb()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);
            _stripeConnectMock.DeleteAccountAsync("acct").Returns(Task.CompletedTask);

            //Act
            var result = await _sut.ResetStripeAccountAsync(instructorId);

            //Assert
            result.Should().Be($"Stripe account for instructor {instructorId} has been reset. The instructor needs to set up Stripe again.");
            instructor.StripeAccountId.Should().BeNull();
            instructor.StripeOnboardingStatus.Should().BeNull();
            instructor.PayoutsEnabled.Should().BeFalse();
            instructor.ChargesEnabled.Should().BeFalse();
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetStripeLoginLinkAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetStripeLoginLinkAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.GetStripeLoginLinkAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Instructor not found.");
        }

        [Fact]
        public async Task GetStripeLoginLinkAsync_NoStripeAccount_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = null };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.GetStripeLoginLinkAsync(instructorId);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You do not have an active Stripe account. Please connect to Stripe first.");
        }

        [Fact]
        public async Task GetStripeLoginLinkAsync_Valid_ReturnsLoginLink()
        {
            //Arrange 1
            int instructorId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };

            //Arrange 2
            _repoMock.GetByIdAsync(instructorId).Returns(instructor);
            _stripeConnectMock.GetLoginLinkAsync("acct").Returns("link");

            //Act
            var result = await _sut.GetStripeLoginLinkAsync(instructorId);

            //Assert
            result.Should().Be("link");
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SetStripeCountryAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task SetStripeCountryAsync_InvalidCountry_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            // No mock needed

            //Act
            Func<Task> act = async () => await _sut.SetStripeCountryAsync(userId, "USA");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid country code (must be exactly 2 ISO characters, e.g. SG, AU, US).");
        }

        [Fact]
        public async Task SetStripeCountryAsync_InstructorNotFound_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.SetStripeCountryAsync(userId, "US");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("You have not submitted an instructor application.");
        }

        [Fact]
        public async Task SetStripeCountryAsync_HasStripeAccount_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);

            //Act
            Func<Task> act = async () => await _sut.SetStripeCountryAsync(userId, "US");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Cannot change country once a Stripe account exists. Please reset Stripe first.");
        }

        [Fact]
        public async Task SetStripeCountryAsync_Valid_UpdatesCountry()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = null };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            await _sut.SetStripeCountryAsync(userId, "sg");

            //Assert
            instructor.StripeCountry.Should().Be("SG");
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetPayoutsAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetPayoutsAsync_ReturnsPagedResult()
        {
            //Arrange 1
            int userId = 1;
            var pagedResult = new PagedResult<InstructorPayoutDto>();

            //Arrange 2
            _repoMock.GetPayoutsAsync(userId, 1, 10, null, "date_desc", null, null, null).Returns(pagedResult);

            //Act
            var result = await _sut.GetPayoutsAsync(userId);

            //Assert
            result.Should().Be(pagedResult);
        }

        // ═══════════════════════════════════════════════════════════════════════
        // SyncPayoutsWithStripeAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task SyncPayoutsWithStripeAsync_InstructorNotFound_ReturnsWithoutDoingAnything()
        {
            //Arrange 1
            int userId = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            await _stripeConnectMock.DidNotReceive().ListPayoutsAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_NoStripePayouts_ReturnsWithoutDoingAnything()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto>());

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            await _financeRepoMock.DidNotReceive().GetPayoutsByStripePayoutIdAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_PayoutNotInDbAndNoValidTransfers_Skips()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow };
            var bt = new StripeBalanceTransactionDto { Type = "charge", SourceId = "ch_1" }; // invalid type

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout>());
            _stripeConnectMock.ListBalanceTransactionsAsync("acct", "po_1").Returns(new List<StripeBalanceTransactionDto> { bt });

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            await _financeRepoMock.DidNotReceive().GetPayoutByTransferIdAsync(Arg.Any<string>());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_PayoutNotInDbAndValidTransfer_UpdatesLocalPayout()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow };
            var bt = new StripeBalanceTransactionDto { Type = "transfer", SourceId = "tr_1" };
            var localPayout = new InstructorPayout { PayoutStatus = PayoutStatus.Pending.ToValue() };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout>());
            _stripeConnectMock.ListBalanceTransactionsAsync("acct", "po_1").Returns(new List<StripeBalanceTransactionDto> { bt });
            _financeRepoMock.GetPayoutByTransferIdAsync("tr_1").Returns(localPayout);

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            localPayout.StripePayoutId.Should().Be("po_1");
            localPayout.PayoutStatus.Should().Be(PayoutStatus.Paid.ToValue());
            localPayout.IsPaid.Should().BeTrue();
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_PayoutInDb_UpdatesAllLocalPayouts()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "in_transit", ArrivalDate = DateTime.UtcNow };
            var localPayout = new InstructorPayout { PayoutStatus = PayoutStatus.Pending.ToValue() };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout> { localPayout });

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            localPayout.PayoutStatus.Should().Be(PayoutStatus.InTransit.ToValue());
            await _stripeConnectMock.DidNotReceive().ListBalanceTransactionsAsync(Arg.Any<string>(), Arg.Any<string>());
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_StatusFailed_UpdatesStatusAndIsPaidToFalse()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "failed", ArrivalDate = DateTime.UtcNow };
            var localPayout = new InstructorPayout { PayoutStatus = PayoutStatus.Paid.ToValue(), IsPaid = true };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout> { localPayout });

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            localPayout.PayoutStatus.Should().Be(PayoutStatus.Failed.ToValue());
            localPayout.IsPaid.Should().BeFalse();
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_StatusRefunded_Ignores()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "paid", ArrivalDate = DateTime.UtcNow };
            var localPayout = new InstructorPayout { PayoutStatus = PayoutStatus.Refunded.ToValue() }; // Should be ignored

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout> { localPayout });

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            localPayout.PayoutStatus.Should().Be(PayoutStatus.Refunded.ToValue()); // Unchanged
            await _repoMock.Received(1).SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════
        // GetPublicProfileAsync
        // ═══════════════════════════════════════════════════════════════════════
        [Fact]
        public async Task GetPublicProfileAsync_ReturnsProfile_WhenInstructorExists()
        {
            //Arrange 1
            int instructorId = 2;
            var user = new User { UserId = instructorId, FullName = "John Doe", Bio = "Expert", UserNavigation = new Account { AvatarUrl = "img.png" } };
            
            var courses = new List<Course>
            {
                new Course 
                { 
                    CourseId = 1, 
                    CourseStatus = CourseStatus.Published.ToValue(), 
                    Title = "Course 1", 
                    Enrollments = new List<Enrollment> { new Enrollment(), new Enrollment() } 
                },
                new Course 
                { 
                    CourseId = 2, 
                    CourseStatus = CourseStatus.Draft.ToValue(), 
                    Title = "Course 2" 
                }
            };

            var instructor = new Instructor 
            { 
                InstructorId = instructorId, 
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                InstructorNavigation = user,
                Courses = courses
            };

            var stats = new InstructorStats { TotalStudentsCount = 100, InstructorRating = 4.8 };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(instructorId).Returns(instructor);
            _repoMock.GetStatsAsync(instructorId).Returns(stats);
            int activeCoursesCount = 1;
            _repoMock.CountActiveCoursesAsync(instructorId).Returns(activeCoursesCount);
            _repoMock.CountInstructorReviewsAsync(instructorId).Returns(50);

            //Act
            var result = await _sut.GetPublicProfileAsync(instructorId);

            //Assert
            result.Should().NotBeNull();
            result!.InstructorId.Should().Be(instructorId);
            result.FullName.Should().Be("John Doe");
            result.Bio.Should().Be("Expert");
            result.AvatarUrl.Should().Be("img.png");
            result.TotalStudents.Should().Be(100);
            result.AverageRating.Should().Be(4.8m);
            result.TotalCourses.Should().Be(1);
            result.TotalReviews.Should().Be(50);
            
            result.Courses.Should().HaveCount(1);
            result.Courses.First().Title.Should().Be("Course 1");
            result.Courses.First().TotalStudents.Should().Be(2);

            await _repoMock.Received(1).GetByIdWithNavigationAsync(instructorId);
            await _repoMock.Received(1).GetStatsAsync(instructorId);
            await _repoMock.Received(1).CountActiveCoursesAsync(instructorId);
            await _repoMock.Received(1).CountInstructorReviewsAsync(instructorId);
        }

        [Fact]
        public async Task GetPublicProfileAsync_ReturnsNull_WhenInstructorDoesNotExistOrNotApproved()
        {
            //Arrange 1
            int instructorId = 99;

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(instructorId).Returns((Instructor?)null);

            //Act
            var result = await _sut.GetPublicProfileAsync(instructorId);

            //Assert
            result.Should().BeNull();

            await _repoMock.Received(1).GetByIdWithNavigationAsync(instructorId);
            await _repoMock.DidNotReceive().GetStatsAsync(Arg.Any<int>());
        }
        [Fact]
        public async Task SubmitApplicationAsync_WithNullDocumentFile_SkipsAndUsesValidRetained()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest
            {
                DocumentFiles = new List<Microsoft.AspNetCore.Http.IFormFile?> { null },
                RetainedDocumentUrls = new List<string> { "a.png" },
                StripeCountry = "SG"
            };
            var existing = new Instructor { ApprovalStatus = InstructorApprovalStatus.Rejected.ToValue(), DocumentUrl = "a.png;b.png" };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(new Account { IsVerified = true });
            _repoMock.GetByIdAsync(userId).Returns(existing);

            //Act
            var result = await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            result.Should().Be("Your application has been resubmitted. Please wait for admin approval.");
            existing.DocumentUrl.Should().Be("a.png");
        }

        [Fact]
        public async Task SubmitApplicationAsync_NoValidRetainedUrlsAndNoFiles_ThrowsExceptionForNewApp()
        {
            //Arrange 1
            int userId = 1;
            var request = new InstructorApplicationRequest
            {
                RetainedDocumentUrls = new List<string> { "invalid.png" } // Will be ignored because existing == null
            };

            //Arrange 2
            _userRepoMock.GetAccountByIdAsync(userId).Returns(new Account { IsVerified = true });
            _repoMock.GetByIdAsync(userId).Returns((Instructor?)null);

            //Act
            Func<Task> act = async () => await _sut.SubmitApplicationAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Please upload at least 1 document/certificate file.");
        }

        [Fact]
        public async Task SetupStripePayoutAsync_InstructorNavigationNull_UsesEmptyEmail()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor
            {
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                InstructorNavigation = null, // Null to trigger email fallback
                StripeAccountId = "acct_old"
            };
            var setupResult = new StripeConnectSetupResponse { StripeAccountId = "acct_new", OnboardingUrl = "http://url" };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(userId).Returns(instructor);
            _stripeConnectMock.SetupExpressAccountAsync(userId, "", "SG", Arg.Any<string>(), Arg.Any<string>(), "acct_old").Returns(setupResult);

            //Act
            var result = await _sut.SetupStripePayoutAsync(userId);

            //Assert
            result.StripeAccountId.Should().Be("acct_new");
            instructor.StripeAccountId.Should().Be("acct_new");
        }

        [Fact]
        public async Task SetStripeCountryAsync_EmptyCountry_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;

            //Act
            Func<Task> act = async () => await _sut.SetStripeCountryAsync(userId, " ");

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("Invalid country code (must be exactly 2 ISO characters, e.g. SG, AU, US).");
        }

        [Fact]
        public async Task SyncPayoutsWithStripeAsync_StatusCanceled_UpdatesStatusAndIsPaidToFalse()
        {
            //Arrange 1
            int userId = 1;
            var instructor = new Instructor { StripeAccountId = "acct" };
            var payout = new StripePayoutDto { Id = "po_1", Status = "canceled", ArrivalDate = DateTime.UtcNow };
            var localPayout = new InstructorPayout { PayoutStatus = PayoutStatus.Pending.ToValue(), IsPaid = true };

            //Arrange 2
            _repoMock.GetByIdAsync(userId).Returns(instructor);
            _stripeConnectMock.ListPayoutsAsync("acct").Returns(new List<StripePayoutDto> { payout });
            _financeRepoMock.GetPayoutsByStripePayoutIdAsync("po_1").Returns(new List<InstructorPayout> { localPayout });

            //Act
            await _sut.SyncPayoutsWithStripeAsync(userId);

            //Assert
            localPayout.PayoutStatus.Should().Be(PayoutStatus.Failed.ToValue());
            localPayout.IsPaid.Should().BeFalse();
            await _repoMock.Received(1).SaveChangesAsync();
        }

        [Fact]
        public async Task GetPublicProfileAsync_NullUserAndStats_HandlesNulls()
        {
            //Arrange 1
            int instructorId = 2;
            var instructor = new Instructor 
            { 
                InstructorId = instructorId, 
                ApprovalStatus = InstructorApprovalStatus.Approved.ToValue(),
                InstructorNavigation = null, // Trigger nulls
                Courses = new List<Course>() // Empty courses
            };

            //Arrange 2
            _repoMock.GetByIdWithNavigationAsync(instructorId).Returns(instructor);
            _repoMock.GetStatsAsync(instructorId).Returns((InstructorStats?)null);
            _repoMock.CountActiveCoursesAsync(instructorId).Returns(0);
            _repoMock.CountInstructorReviewsAsync(instructorId).Returns(0);

            //Act
            var result = await _sut.GetPublicProfileAsync(instructorId);

            //Assert
            result.Should().NotBeNull();
            result!.FullName.Should().BeNull();
            result.AvatarUrl.Should().BeNull();
            result.Bio.Should().BeNull();
            result.TotalStudents.Should().Be(0);
            result.AverageRating.Should().Be(0);
            result.Courses.Should().BeEmpty();
        }
    }
}
