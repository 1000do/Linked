using CourseMarketplaceBE.Application.DTOs;
using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Application.IServices;
using FluentAssertions;
using NSubstitute;
using System.Reflection;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class CouponServiceTests
{
    private readonly ICouponRepository _mockRepo;
    private readonly ICourseRepository _mockCourseRepo;
    private readonly IRedisService _mockRedisService;
    private readonly ICartRepository _mockCartRepo;
    private readonly CouponService _sut;

    public CouponServiceTests()
    {
        _mockRepo = Substitute.For<ICouponRepository>();
        _mockCourseRepo = Substitute.For<ICourseRepository>();
        _mockRedisService = Substitute.For<IRedisService>();
        _mockCartRepo = Substitute.For<ICartRepository>();

        _sut = new CouponService(
            _mockRepo,
            _mockCourseRepo,
            _mockRedisService,
            _mockCartRepo
        );
    }

    [Fact]
    public async Task GetAll_IsAdminTrue_PassesNullFilterAndReturnsMapped()
    {
        //Arrange 1
        var coupons = new List<Coupon>
        {
            new Coupon { CouponId = 1, CouponCode = "TEST1" }
        };

        //Arrange 2
        _mockRepo.GetAllAsync(null, null, null, null).Returns(coupons);

        //Act
        var result = await _sut.GetAll(123, null, null, null, isAdmin: true);

        //Assert
        result.Should().HaveCount(1);
        result[0].CouponCode.Should().Be("TEST1");
        await _mockRepo.Received(1).GetAllAsync(null, null, null, null);
    }

    [Fact]
    public async Task GetAll_IsAdminFalse_PassesManagerIdAndReturnsMapped()
    {
        //Arrange 1
        int managerId = 123;
        var coupons = new List<Coupon>
        {
            new Coupon { CouponId = 1, CouponCode = "TEST1", ManagerId = managerId }
        };

        //Arrange 2
        _mockRepo.GetAllAsync(managerId, true, "fixed", "search").Returns(coupons);

        //Act
        var result = await _sut.GetAll(managerId, true, "fixed", "search", isAdmin: false);

        //Assert
        result.Should().HaveCount(1);
        await _mockRepo.Received(1).GetAllAsync(managerId, true, "fixed", "search");
    }

    [Fact]
    public async Task GetById_NotFound_ReturnsNull()
    {
        //Arrange 1
        int id = 1;
        int managerId = 123;

        //Arrange 2
        _mockRepo.GetByIdAsync(id, managerId).Returns((Coupon?)null);

        //Act
        var result = await _sut.GetById(id, managerId, isAdmin: false);

        //Assert
        result.Should().BeNull();
        await _mockRepo.Received(1).GetByIdAsync(id, managerId);
    }

    [Fact]
    public async Task GetById_Found_ReturnsMappedResponse()
    {
        //Arrange 1
        int id = 1;
        var coupon = new Coupon { CouponId = id, CouponCode = "TEST", CouponType = null }; // testing ?? "fixed" map

        //Arrange 2
        _mockRepo.GetByIdAsync(id, null).Returns(coupon);

        //Act
        var result = await _sut.GetById(id, 123, isAdmin: true);

        //Assert
        result.Should().NotBeNull();
        result!.CouponType.Should().Be("fixed");
        await _mockRepo.Received(1).GetByIdAsync(id, null);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("invalid")]
    public async Task Create_InvalidType_ThrowsArgumentException(string invalidType)
    {
        //Arrange 1
        var req = new CreateCouponRequest { CouponType = invalidType };

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public async Task Create_InvalidDiscountValue_ThrowsArgumentException(decimal invalidValue)
    {
        //Arrange 1
        var req = new CreateCouponRequest { CouponType = "fixed", DiscountValue = invalidValue };

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("DiscountValue must be greater than 0.");
    }

    [Fact]
    public async Task Create_PercentageGreaterThan100_ThrowsArgumentException()
    {
        //Arrange 1
        var req = new CreateCouponRequest { CouponType = "percent", DiscountValue = 101 };

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("Discount percentage cannot exceed 100%.");
    }

    [Fact]
    public async Task Create_InvalidDates_ThrowsArgumentException()
    {
        //Arrange 1
        var req = new CreateCouponRequest 
        { 
            CouponType = "fixed", 
            DiscountValue = 10,
            StartDate = DateTime.UtcNow.AddDays(2),
            EndDate = DateTime.UtcNow.AddDays(1)
        };

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("StartDate must be before EndDate.");
    }

    [Fact]
    public async Task Create_InvalidUsageLimit_ThrowsArgumentException()
    {
        //Arrange 1
        var req = new CreateCouponRequest 
        { 
            CouponType = "fixed", 
            DiscountValue = 10,
            UsageLimit = 0
        };

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("UsageLimit must be >= 1.");
    }

    [Fact]
    public async Task Create_CodeAlreadyExists_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var req = new CreateCouponRequest { CouponType = "fixed", DiscountValue = 10, CouponCode = "EXISTING" };

        //Arrange 2
        _mockRepo.GetByCodeAsync("EXISTING").Returns(new Coupon());

        //Act
        Func<Task> act = async () => await _sut.Create(req, 1);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Coupon code 'EXISTING' already exists.");
    }

    [Fact]
    public async Task Create_ValidRequestFixed_BuildsAndSavesCoupon()
    {
        //Arrange 1
        var req = new CreateCouponRequest 
        { 
            CouponType = "Amount", // tests normalization
            DiscountValue = 10, 
            CouponCode = " NEWCODE ",
            MinOrderValue = -5 // tests normalization to 0
        };
        int managerId = 123;

        //Arrange 2
        _mockRepo.GetByCodeAsync("NEWCODE").Returns((Coupon?)null);

        //Act
        await _sut.Create(req, managerId);

        //Assert
        await _mockRepo.Received(1).AddAsync(Arg.Is<Coupon>(c => 
            c.CouponCode == "NEWCODE" &&
            c.CouponType == "fixed" &&
            c.DiscountValue == 10 &&
            c.MinOrderValue == 0 &&
            c.UsedCount == 0 &&
            c.IsActive == true &&
            c.ManagerId == managerId
        ));
        await _mockRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Create_ValidRequestPercentage_BuildsAndSavesCoupon()
    {
        //Arrange 1
        var req = new CreateCouponRequest 
        { 
            CouponType = "percentage", 
            DiscountValue = 50, 
            CouponCode = "CODE2",
            MinOrderValue = 100
        };
        int managerId = 123;

        //Arrange 2
        _mockRepo.GetByCodeAsync("CODE2").Returns((Coupon?)null);

        //Act
        await _sut.Create(req, managerId);

        //Assert
        await _mockRepo.Received(1).AddAsync(Arg.Is<Coupon>(c => c.CouponType == "percentage" && c.DiscountValue == 50));
        await _mockRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task Update_NotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int id = 1;
        var req = new UpdateCouponRequest();

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns((Coupon?)null);

        //Act
        Func<Task> act = async () => await _sut.Update(id, req, 1, isAdmin: true);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task Update_EndDateBeforeStartDate_ThrowsArgumentException()
    {
        //Arrange 1
        int id = 1;
        var req = new UpdateCouponRequest { EndDate = DateTime.UtcNow };
        var coupon = new Coupon { StartDate = DateTime.UtcNow.AddDays(1) };

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.Update(id, req, 1, isAdmin: true);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("EndDate must be after StartDate.");
    }

    [Fact]
    public async Task Update_UsageLimitLessThanUsedCount_ThrowsArgumentException()
    {
        //Arrange 1
        int id = 1;
        var req = new UpdateCouponRequest { UsageLimit = 5 };
        var coupon = new Coupon { UsedCount = 10 };

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.Update(id, req, 1, isAdmin: true);

        //Assert
        var ex = await act.Should().ThrowAsync<ArgumentException>();
        ex.WithMessage("UsageLimit (5) cannot be less than the used count (10).");
    }

    [Fact]
    public async Task Update_ValidPartialRequest_UpdatesPropertiesAndSaves()
    {
        //Arrange 1
        int id = 1;
        var req = new UpdateCouponRequest { UsageLimit = 20, IsActive = false, EndDate = DateTime.UtcNow.AddDays(2) };
        var coupon = new Coupon { UsedCount = 10, StartDate = DateTime.UtcNow.AddDays(1) };

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns(coupon);

        //Act
        await _sut.Update(id, req, 1, isAdmin: true);

        //Assert
        coupon.UsageLimit.Should().Be(20);
        coupon.IsActive.Should().BeFalse();
        coupon.EndDate.Should().Be(req.EndDate);

        _mockRepo.Received(1).Update(coupon);
        await _mockRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task SoftDelete_NotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int id = 1;

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns((Coupon?)null);

        //Act
        Func<Task> act = async () => await _sut.SoftDelete(id, 1);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task SoftDelete_Found_SetsIsActiveFalseAndSaves()
    {
        //Arrange 1
        int id = 1;
        var coupon = new Coupon { IsActive = true, EndDate = DateTime.UtcNow.AddDays(10) };

        //Arrange 2
        _mockRepo.GetByIdAsync(id, Arg.Any<int?>()).Returns(coupon);

        //Act
        await _sut.SoftDelete(id, 1);

        //Assert
        coupon.IsActive.Should().BeFalse();
        coupon.EndDate.Should().BeBefore(DateTime.UtcNow);

        _mockRepo.Received(1).Update(coupon);
        await _mockRepo.Received(1).SaveChangesAsync();
    }

    [Fact]
    public async Task GetActivePlatformCouponsAsync_ReturnsMappedResponses()
    {
        //Arrange 1
        var coupons = new List<Coupon> { new Coupon { CouponId = 1, CouponCode = "A" } };

        //Arrange 2
        _mockRepo.GetActivePlatformCouponsAsync().Returns(coupons);

        //Act
        var result = await _sut.GetActivePlatformCouponsAsync();

        //Assert
        result.Should().HaveCount(1);
        result[0].CouponCode.Should().Be("A");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_CourseNotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int courseId = 1;

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns((Course?)null);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, 1, 1);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_NotOwner_ThrowsUnauthorizedAccessException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 99;
        var course = new Course { Instructor = new Instructor { InstructorId = 1 } };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, 1, instructorId);

        //Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>();
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_CourseInCart_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        var course = new Course { Instructor = new Instructor { InstructorId = instructorId } };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(true);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, 1, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Cannot apply coupon. This course is currently in a user's shopping cart.");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_FreeCourse_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        var course = new Course { Price = 0, Instructor = new Instructor { InstructorId = instructorId } };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, 1, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Coupons cannot be applied to free courses.");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_CouponNotFound_ThrowsKeyNotFoundException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { Price = 100, Instructor = new Instructor { InstructorId = instructorId } };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns((Coupon?)null);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_CouponNotActive_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { Price = 100, Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = false };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has been disabled.");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_CouponExpired_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { Price = 100, Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = true, EndDate = DateTime.UtcNow.AddDays(-1) };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has expired.");
    }
    
    [Fact]
    public async Task ApplyCouponToCourseAsync_CouponNotStarted_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { Price = 100, Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = true, StartDate = DateTime.UtcNow.AddDays(1) };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has not started yet.");
    }
    
    [Fact]
    public async Task ApplyCouponToCourseAsync_CouponUsageLimitReached_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { Price = 100, Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = true, UsageLimit = 10, UsedCount = 10 };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        Func<Task> act = async () => await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon usage limit has been reached.");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_ValidPublishedCourse_SetsDraftAndAppliesCoupon()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { CourseId = courseId, Price = 100, CourseStatus = "published", Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = true };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        course.CouponId.Should().Be(couponId);
        course.CourseStatus.Should().Be("draft");
        
        _mockCourseRepo.Received(1).Update(course);
        await _mockRepo.Received(1).SaveChangesAsync();
        await _mockRedisService.Received(1).RemoveCacheAsync($"course:detail:{courseId}");
    }

    [Fact]
    public async Task ApplyCouponToCourseAsync_ValidDraftCourse_AppliesCouponWithoutStatusChange()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        int couponId = 2;
        var course = new Course { CourseId = courseId, Price = 100, CourseStatus = "draft", Instructor = new Instructor { InstructorId = instructorId } };
        var coupon = new Coupon { IsActive = true };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);
        _mockRepo.GetByIdGlobalAsync(couponId).Returns(coupon);

        //Act
        await _sut.ApplyCouponToCourseAsync(courseId, couponId, instructorId);

        //Assert
        course.CouponId.Should().Be(couponId);
        course.CourseStatus.Should().Be("draft");
        
        _mockCourseRepo.Received(1).Update(course);
        await _mockRepo.Received(1).SaveChangesAsync();
        await _mockRedisService.Received(1).RemoveCacheAsync($"course:detail:{courseId}");
    }

    [Fact]
    public async Task RemoveCouponFromCourseAsync_ValidRequest_RemovesCouponAndClearsCache()
    {
        //Arrange 1
        int courseId = 1;
        int instructorId = 1;
        var course = new Course { CourseId = courseId, CouponId = 5, Instructor = new Instructor { InstructorId = instructorId } };

        //Arrange 2
        _mockCourseRepo.GetCourseWithInstructorAsync(courseId).Returns(course);
        _mockCartRepo.IsCourseInAnyCartAsync(courseId).Returns(false);

        //Act
        await _sut.RemoveCouponFromCourseAsync(courseId, instructorId);

        //Assert
        course.CouponId.Should().BeNull();
        _mockCourseRepo.Received(1).Update(course);
        await _mockRepo.Received(1).SaveChangesAsync();
        await _mockRedisService.Received(1).RemoveCacheAsync($"course:detail:{courseId}");
    }

    [Fact]
    public void ApplyCoupon_NotActive_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = false };

        //Act
        Action act = () => _sut.ApplyCoupon(coupon, 100);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Coupon is not active.");
    }

    [Fact]
    public void ApplyCoupon_NotStarted_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, StartDate = DateTime.UtcNow.AddDays(1) };

        //Act
        Action act = () => _sut.ApplyCoupon(coupon, 100);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Coupon has not started yet.");
    }

    [Fact]
    public void ApplyCoupon_Expired_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, EndDate = DateTime.UtcNow.AddDays(-1) };

        //Act
        Action act = () => _sut.ApplyCoupon(coupon, 100);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Coupon has expired.");
    }

    [Fact]
    public void ApplyCoupon_UsageLimitReached_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, UsageLimit = 5, UsedCount = 5 };

        //Act
        Action act = () => _sut.ApplyCoupon(coupon, 100);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Coupon usage limit has been reached.");
    }

    [Fact]
    public void ApplyCoupon_MinOrderValueNotMet_ThrowsInvalidOperationException()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, MinOrderValue = 150 };

        //Act
        Action act = () => _sut.ApplyCoupon(coupon, 100);

        //Assert
        act.Should().Throw<InvalidOperationException>().WithMessage("Minimum order value required to use this coupon is $150.00 USD.");
    }

    [Fact]
    public void ApplyCoupon_FixedDiscount_CalculatesFinalPrice()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, CouponType = "fixed", DiscountValue = 20 };

        //Act
        var result = _sut.ApplyCoupon(coupon, 100);

        //Assert
        result.Should().Be(80);
    }

    [Fact]
    public void ApplyCoupon_PercentageDiscount_CalculatesFinalPrice()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, CouponType = "percentage", DiscountValue = 25 };

        //Act
        var result = _sut.ApplyCoupon(coupon, 100);

        //Assert
        result.Should().Be(75);
    }

    [Fact]
    public void ApplyCoupon_DiscountGreaterThanPrice_ReturnsZero()
    {
        //Arrange 1
        var coupon = new Coupon { IsActive = true, CouponType = "fixed", DiscountValue = 150 };

        //Act
        var result = _sut.ApplyCoupon(coupon, 100);

        //Assert
        result.Should().Be(0);
    }
}
