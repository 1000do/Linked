using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.IServices;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using FluentAssertions;
using NSubstitute;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class InstructorApprovalServiceTests
    {
        private readonly IInstructorRepository _repoMock;
        private readonly INotificationService _notiServiceMock;
        private readonly InstructorApprovalService _sut;

        public InstructorApprovalServiceTests()
        {
            _repoMock = Substitute.For<IInstructorRepository>();
            _notiServiceMock = Substitute.For<INotificationService>();
            _sut = new InstructorApprovalService(_repoMock, _notiServiceMock);
        }

        [Fact]
        public async Task GetPendingListAsync_InstructorsExist_ReturnsPagedResultWithDtos()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var instructors = new List<Instructor>
            {
                new Instructor
                {
                    InstructorId = 1,
                    ProfessionalTitle = "Pro",
                    DocumentUrl = "doc.pdf",
                    LinkedinUrl = "linkedin",
                    YoutubeUrl = "youtube",
                    FacebookUrl = "facebook",
                    ApprovalStatus = "Pending",
                    ExpertiseCategories = "IT",
                    InstructorNavigation = new User
                    {
                        FullName = "John Doe",
                        UserNavigation = new Account
                        {
                            Email = "john@test.com",
                            AvatarUrl = "avatar.png"
                        }
                    }
                }
            };
            int totalCount = 1;

            //Arrange 2
            _repoMock.GetPendingInstructorsAsync(page, pageSize).Returns((instructors, totalCount));

            //Act
            var result = await _sut.GetPendingListAsync(page, pageSize);

            //Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(totalCount);
            result.Items.Should().HaveCount(1);
            
            var dto = result.Items.First();
            dto.InstructorId.Should().Be(1);
            dto.FullName.Should().Be("John Doe");
            dto.Email.Should().Be("john@test.com");
            dto.AvatarUrl.Should().Be("avatar.png");
            dto.ProfessionalTitle.Should().Be("Pro");
            dto.DocumentUrl.Should().Be("doc.pdf");
            dto.LinkedInUrl.Should().Be("linkedin");
            dto.YoutubeUrl.Should().Be("youtube");
            dto.FacebookUrl.Should().Be("facebook");
            dto.ApprovalStatus.Should().Be("Pending");
            dto.ExpertiseCategories.Should().Be("IT");

            await _repoMock.Received(1).GetPendingInstructorsAsync(page, pageSize);
        }

        [Fact]
        public async Task GetPendingListAsync_InstructorsEmpty_ReturnsEmptyPagedResult()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var instructors = new List<Instructor>();
            int totalCount = 0;

            //Arrange 2
            _repoMock.GetPendingInstructorsAsync(page, pageSize).Returns((instructors, totalCount));

            //Act
            var result = await _sut.GetPendingListAsync(page, pageSize);

            //Assert
            result.Should().NotBeNull();
            result.TotalCount.Should().Be(0);
            result.Items.Should().BeEmpty();

            await _repoMock.Received(1).GetPendingInstructorsAsync(page, pageSize);
        }

        [Fact]
        public async Task GetPendingListAsync_NullNavigationProperties_MapsToDefaultValues()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var instructors = new List<Instructor>
            {
                new Instructor
                {
                    InstructorId = 2,
                    InstructorNavigation = null // Null navigation
                }
            };
            int totalCount = 1;

            //Arrange 2
            _repoMock.GetPendingInstructorsAsync(page, pageSize).Returns((instructors, totalCount));

            //Act
            var result = await _sut.GetPendingListAsync(page, pageSize);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            
            var dto = result.Items.First();
            dto.FullName.Should().Be("N/A");
            dto.Email.Should().Be("N/A");
            dto.AvatarUrl.Should().BeNull();

            await _repoMock.Received(1).GetPendingInstructorsAsync(page, pageSize);
        }
        
        [Fact]
        public async Task GetPendingListAsync_UserNavigationNull_MapsToDefaultValues()
        {
            //Arrange 1
            int page = 1;
            int pageSize = 10;
            var instructors = new List<Instructor>
            {
                new Instructor
                {
                    InstructorId = 2,
                    InstructorNavigation = new User
                    {
                        FullName = "John",
                        UserNavigation = null
                    }
                }
            };
            int totalCount = 1;

            //Arrange 2
            _repoMock.GetPendingInstructorsAsync(page, pageSize).Returns((instructors, totalCount));

            //Act
            var result = await _sut.GetPendingListAsync(page, pageSize);

            //Assert
            result.Should().NotBeNull();
            result.Items.Should().HaveCount(1);
            
            var dto = result.Items.First();
            dto.FullName.Should().Be("John");
            dto.Email.Should().Be("N/A");
            dto.AvatarUrl.Should().BeNull();

            await _repoMock.Received(1).GetPendingInstructorsAsync(page, pageSize);
        }

        [Fact]
        public async Task ApproveOrRejectAsync_InstructorNotFound_ReturnsFalse()
        {
            //Arrange 1
            var dto = new UpdateApprovalStatusDto { InstructorId = 1, Status = InstructorApprovalStatus.Approved.ToValue() };

            //Arrange 2
            _repoMock.GetByIdAsync(dto.InstructorId).Returns((Instructor)null!);

            //Act
            var result = await _sut.ApproveOrRejectAsync(dto);

            //Assert
            result.Should().BeFalse();
            await _repoMock.Received(1).GetByIdAsync(dto.InstructorId);
            _repoMock.DidNotReceive().Update(Arg.Any<Instructor>());
            await _repoMock.DidNotReceive().SaveChangesAsync();
            await _notiServiceMock.DidNotReceive().SendNotificationAsync(Arg.Any<int>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>());
        }

        [Fact]
        public async Task ApproveOrRejectAsync_StatusRejected_SetsReasonSendsRejectionNotification()
        {
            //Arrange 1
            var dto = new UpdateApprovalStatusDto 
            { 
                InstructorId = 1, 
                Status = InstructorApprovalStatus.Rejected.ToValue(),
                Reason = "Not qualified"
            };
            var instructor = new Instructor { InstructorId = 1, ApprovalStatus = "Pending" };

            //Arrange 2
            _repoMock.GetByIdAsync(dto.InstructorId).Returns(instructor);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveOrRejectAsync(dto);

            //Assert
            result.Should().BeTrue();
            instructor.ApprovalStatus.Should().Be(dto.Status);
            instructor.RejectionReason.Should().Be(dto.Reason);

            await _repoMock.Received(1).GetByIdAsync(dto.InstructorId);
            _repoMock.Received(1).Update(instructor);
            await _repoMock.Received(1).SaveChangesAsync();
            
            string expectedMsg = $"Unfortunately, your instructor application was not accepted. Reason: {dto.Reason}";
            await _notiServiceMock.Received(1).SendNotificationAsync(instructor.InstructorId, "Application Result", expectedMsg, "/Instructor/ApplicationStatus");
        }

        [Fact]
        public async Task ApproveOrRejectAsync_StatusApproved_ClearsReasonSendsApprovalNotification()
        {
            //Arrange 1
            var dto = new UpdateApprovalStatusDto 
            { 
                InstructorId = 1, 
                Status = InstructorApprovalStatus.Approved.ToValue()
            };
            var instructor = new Instructor { InstructorId = 1, ApprovalStatus = "Pending", RejectionReason = "Old reason" };

            //Arrange 2
            _repoMock.GetByIdAsync(dto.InstructorId).Returns(instructor);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveOrRejectAsync(dto);

            //Assert
            result.Should().BeTrue();
            instructor.ApprovalStatus.Should().Be(dto.Status);
            instructor.RejectionReason.Should().BeNull();

            await _repoMock.Received(1).GetByIdAsync(dto.InstructorId);
            _repoMock.Received(1).Update(instructor);
            await _repoMock.Received(1).SaveChangesAsync();
            
            string expectedMsg = "Congratulations! Your instructor application has been approved.";
            await _notiServiceMock.Received(1).SendNotificationAsync(instructor.InstructorId, "Application Result", expectedMsg, "/Instructor/ApplicationStatus");
        }
        
        [Fact]
        public async Task ApproveOrRejectAsync_OtherStatus_DoesNotChangeReasonSendsNotification()
        {
            //Arrange 1
            var dto = new UpdateApprovalStatusDto 
            { 
                InstructorId = 1, 
                Status = "OtherStatus",
                Reason = "Some reason"
            };
            var instructor = new Instructor { InstructorId = 1, ApprovalStatus = "Pending", RejectionReason = "Old reason" };

            //Arrange 2
            _repoMock.GetByIdAsync(dto.InstructorId).Returns(instructor);
            _repoMock.SaveChangesAsync().Returns(1);

            //Act
            var result = await _sut.ApproveOrRejectAsync(dto);

            //Assert
            result.Should().BeTrue();
            instructor.ApprovalStatus.Should().Be(dto.Status);
            instructor.RejectionReason.Should().Be("Old reason"); // Unchanged

            await _repoMock.Received(1).GetByIdAsync(dto.InstructorId);
            _repoMock.Received(1).Update(instructor);
            await _repoMock.Received(1).SaveChangesAsync();
            
            // The default message is the rejection one since status != Approved
            string expectedMsg = $"Unfortunately, your instructor application was not accepted. Reason: {dto.Reason}";
            await _notiServiceMock.Received(1).SendNotificationAsync(instructor.InstructorId, "Application Result", expectedMsg, "/Instructor/ApplicationStatus");
        }

        [Fact]
        public async Task GetDetailAsync_InstructorNotFound_ReturnsNull()
        {
            //Arrange 1
            int id = 1;

            //Arrange 2
            _repoMock.GetByIdAsync(id).Returns((Instructor)null!);

            //Act
            var result = await _sut.GetDetailAsync(id);

            //Assert
            result.Should().BeNull();
            await _repoMock.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetDetailAsync_InstructorFound_ReturnsMappedDto()
        {
            //Arrange 1
            int id = 1;
            var instructor = new Instructor
            {
                InstructorId = id,
                ProfessionalTitle = "Pro",
                DocumentUrl = "doc.pdf",
                LinkedinUrl = "linkedin",
                YoutubeUrl = "youtube",
                FacebookUrl = "facebook",
                ApprovalStatus = "Pending",
                ExpertiseCategories = "IT",
                InstructorNavigation = new User
                {
                    FullName = "John Doe",
                    UserNavigation = new Account
                    {
                        Email = "john@test.com",
                        AvatarUrl = "avatar.png"
                    }
                }
            };

            //Arrange 2
            _repoMock.GetByIdAsync(id).Returns(instructor);

            //Act
            var result = await _sut.GetDetailAsync(id);

            //Assert
            result.Should().NotBeNull();
            result.InstructorId.Should().Be(id);
            result.FullName.Should().Be("John Doe");
            result.Email.Should().Be("john@test.com");
            result.AvatarUrl.Should().Be("avatar.png");
            result.ProfessionalTitle.Should().Be("Pro");
            result.DocumentUrl.Should().Be("doc.pdf");
            result.LinkedInUrl.Should().Be("linkedin");
            result.YoutubeUrl.Should().Be("youtube");
            result.FacebookUrl.Should().Be("facebook");
            result.ApprovalStatus.Should().Be("Pending");
            result.ExpertiseCategories.Should().Be("IT");

            await _repoMock.Received(1).GetByIdAsync(id);
        }

        [Fact]
        public async Task GetDetailAsync_InstructorFoundWithNullNavigations_MapsToDefaultValues()
        {
            //Arrange 1
            int id = 1;
            var instructor = new Instructor
            {
                InstructorId = id,
                InstructorNavigation = null
            };

            //Arrange 2
            _repoMock.GetByIdAsync(id).Returns(instructor);

            //Act
            var result = await _sut.GetDetailAsync(id);

            //Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("N/A");
            result.Email.Should().Be("N/A");
            result.AvatarUrl.Should().BeNull();

            await _repoMock.Received(1).GetByIdAsync(id);
        }
        
        [Fact]
        public async Task GetDetailAsync_InstructorFoundWithUserNavigationNull_MapsToDefaultValues()
        {
            //Arrange 1
            int id = 1;
            var instructor = new Instructor
            {
                InstructorId = id,
                InstructorNavigation = new User
                {
                    FullName = "John",
                    UserNavigation = null
                }
            };

            //Arrange 2
            _repoMock.GetByIdAsync(id).Returns(instructor);

            //Act
            var result = await _sut.GetDetailAsync(id);

            //Assert
            result.Should().NotBeNull();
            result.FullName.Should().Be("John");
            result.Email.Should().Be("N/A");
            result.AvatarUrl.Should().BeNull();

            await _repoMock.Received(1).GetByIdAsync(id);
        }
    }
}
