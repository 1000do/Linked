using CourseMarketplaceBE.Application.Services;
using CourseMarketplaceBE.Domain.IRepositories;
using CourseMarketplaceBE.Domain.Entities;
using CourseMarketplaceBE.Domain.Constants;
using CourseMarketplaceBE.Application.DTOs;
using NSubstitute;
using FluentAssertions;
using Xunit;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;

namespace CourseMarketplaceBE.Tests.Application.Services;

public class CartServiceTests
{
    private readonly ICartRepository _cartRepo;
    private readonly ICourseRepository _courseRepo;
    private readonly ICouponRepository _couponRepo;
    private readonly CartService _sut;

    public CartServiceTests()
    {
        _cartRepo = Substitute.For<ICartRepository>();
        _courseRepo = Substitute.For<ICourseRepository>();
        _couponRepo = Substitute.For<ICouponRepository>();
        _sut = new CartService(_cartRepo, _courseRepo, _couponRepo);
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // AddToCartAsync
    // ═══════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task AddToCartAsync_CourseNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns((Course)null);
        
        //Act
        Func<Task> act = async () => await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course does not exist or is not published.");
        await _cartRepo.DidNotReceive().AddCartItemAsync(Arg.Any<CartItem>());
    }
    
    [Fact]
    public async Task AddToCartAsync_CourseNotPublished_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var course = new Course { CourseId = courseId, CourseStatus = "Draft" };
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns(course);
        
        //Act
        Func<Task> act = async () => await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course does not exist or is not published.");
        await _cartRepo.DidNotReceive().AddCartItemAsync(Arg.Any<CartItem>());
    }

    [Fact]
    public async Task AddToCartAsync_CourseBelongsToUser_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = userId };
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns(course);
        
        //Act
        Func<Task> act = async () => await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("You cannot add your own course to the cart.");
        await _cartRepo.DidNotReceive().AddCartItemAsync(Arg.Any<CartItem>());
    }
    
    [Fact]
    public async Task AddToCartAsync_UserAlreadyEnrolled_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns(course);
        _courseRepo.IsEnrolledAsync(userId, courseId).Returns(true);
        
        //Act
        Func<Task> act = async () => await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("You have already purchased this course.");
        await _cartRepo.DidNotReceive().AddCartItemAsync(Arg.Any<CartItem>());
    }

    [Fact]
    public async Task AddToCartAsync_CourseAlreadyInCart_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2 };
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns(course);
        _courseRepo.IsEnrolledAsync(userId, courseId).Returns(false);
        _cartRepo.IsCourseInCartAsync(userId, courseId).Returns(true);
        
        //Act
        Func<Task> act = async () => await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course is already in the cart.");
        await _cartRepo.DidNotReceive().AddCartItemAsync(Arg.Any<CartItem>());
    }
    
    [Fact]
    public async Task AddToCartAsync_ValidRequest_AddsToCartAndSaves()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var course = new Course { CourseId = courseId, CourseStatus = CourseStatus.Published.ToValue(), InstructorId = 2, Price = 50.0m };
        
        //Arrange 2
        _courseRepo.GetByIdAsync(courseId).Returns(course);
        _courseRepo.IsEnrolledAsync(userId, courseId).Returns(false);
        _cartRepo.IsCourseInCartAsync(userId, courseId).Returns(false);
        _cartRepo.SaveChangesAsync().Returns(1);
        
        //Act
        await _sut.AddToCartAsync(userId, courseId);
        
        //Assert
        await _cartRepo.Received(1).AddCartItemAsync(Arg.Is<CartItem>(c => c.UserId == userId && c.CourseId == courseId && c.Price == 50.0m));
        await _cartRepo.Received(1).SaveChangesAsync();
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // RemoveFromCartAsync
    // ═══════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task RemoveFromCartAsync_ItemNotFound_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        
        //Arrange 2
        _cartRepo.GetCartItemAsync(userId, courseId).Returns((CartItem)null);
        
        //Act
        Func<Task> act = async () => await _sut.RemoveFromCartAsync(userId, courseId);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("Course not found in the cart.");
        _cartRepo.DidNotReceive().RemoveCartItem(Arg.Any<CartItem>());
    }
    
    [Fact]
    public async Task RemoveFromCartAsync_ValidRequest_RemovesAndSaves()
    {
        //Arrange 1
        int userId = 1;
        int courseId = 100;
        var cartItem = new CartItem { UserId = userId, CourseId = courseId };
        
        //Arrange 2
        _cartRepo.GetCartItemAsync(userId, courseId).Returns(cartItem);
        _cartRepo.SaveChangesAsync().Returns(1);
        
        //Act
        await _sut.RemoveFromCartAsync(userId, courseId);
        
        //Assert
        _cartRepo.Received(1).RemoveCartItem(cartItem);
        await _cartRepo.Received(1).SaveChangesAsync();
    }
    
    // ═══════════════════════════════════════════════════════════════════════
    // GetCartSummaryAsync
    // ═══════════════════════════════════════════════════════════════════════
    [Fact]
    public async Task GetCartSummaryAsync_NoCoupon_ReturnsSummarySuccessfully()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = null;
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = null, Course = new Course { Title = "Course 1", Price = 10m } },
            new CartItem { CourseId = 2, Price = 20m, Course = new Course { Title = "Course 2", Price = 25m, Instructor = new Instructor { InstructorNavigation = new User { FullName = "Inst" } } } },
            new CartItem { CourseId = null, Price = null, Course = null }
        };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(new List<Coupon>());
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.Should().NotBeNull();
        result.SubTotal.Should().Be(30m); // 10 + 20 + 0
        result.Total.Should().Be(30m);
        result.DiscountAmount.Should().Be(0);
        result.AppliedCouponCode.Should().BeNull();
        result.CouponMessage.Should().BeNull();
        result.AvailableCoupons.Should().BeEmpty();
        result.Items.Count.Should().Be(3);
        result.Items[0].Title.Should().Be("Course 1");
        result.Items[2].Title.Should().Be("(Course not found)");
        result.Items[2].CourseId.Should().Be(0);
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_InvalidCoupon_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "INVALID";
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(new List<CartItem>());
        _couponRepo.GetByCodeAsync("INVALID").Returns((Coupon)null);
        
        //Act
        Func<Task> act = async () => await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon does not exist.");
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_CouponInactive_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "INACTIVE";
        var coupon = new Coupon { IsActive = false };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(new List<CartItem>());
        _couponRepo.GetByCodeAsync("INACTIVE").Returns(coupon);
        
        //Act
        Func<Task> act = async () => await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has expired or run out of usage.");
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_CouponBeforeStartDate_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "FUTURE";
        var coupon = new Coupon { IsActive = true, StartDate = DateTime.Now.AddDays(1) };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(new List<CartItem>());
        _couponRepo.GetByCodeAsync("FUTURE").Returns(coupon);
        
        //Act
        Func<Task> act = async () => await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has expired or run out of usage.");
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_CouponAfterEndDate_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "PAST";
        var coupon = new Coupon { IsActive = true, EndDate = DateTime.Now.AddDays(-1) };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(new List<CartItem>());
        _couponRepo.GetByCodeAsync("PAST").Returns(coupon);
        
        //Act
        Func<Task> act = async () => await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has expired or run out of usage.");
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_CouponUsageLimitReached_ThrowsInvalidOperationException()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "MAXED";
        var coupon = new Coupon { IsActive = true, UsageLimit = 10, UsedCount = 10 };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(new List<CartItem>());
        _couponRepo.GetByCodeAsync("MAXED").Returns(coupon);
        
        //Act
        Func<Task> act = async () => await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        var ex = await act.Should().ThrowAsync<InvalidOperationException>();
        ex.WithMessage("This coupon has expired or run out of usage.");
    }

    [Fact]
    public async Task GetCartSummaryAsync_CouponNotApplicableToAnyCourse_ReturnsNoValidCouponMessage()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "VALID";
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = 50m, Course = new Course { CouponId = 99 } }
        };
        var coupon = new Coupon { CouponId = 1, CouponCode = "VALID", IsActive = true, MinOrderValue = 10m, CouponType = "fixed", DiscountValue = 5m };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetByCodeAsync("VALID").Returns(coupon);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(new List<Coupon>());
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.DiscountAmount.Should().Be(0);
        result.Total.Should().Be(50m);
        result.CouponMessage.Should().Be("No valid coupon applied.");
        result.AppliedCouponCode.Should().BeNull();
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_PercentageCoupon_AppliesCorrectly()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "PERCENT";
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = 100m, Course = new Course { CouponId = 1, Price = 100m } }
        };
        var coupon = new Coupon { CouponId = 1, CouponCode = "PERCENT", IsActive = true, MinOrderValue = 50m, CouponType = "percentage", DiscountValue = 20m };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetByCodeAsync("PERCENT").Returns(coupon);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(new List<Coupon>());
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.DiscountAmount.Should().Be(20m);
        result.Total.Should().Be(80m);
        result.AppliedCouponCode.Should().Be("PERCENT");
        result.CouponMessage.Should().Contain("PERCENT");
        result.Items[0].DiscountAmount.Should().Be(20m);
        result.Items[0].DiscountedPrice.Should().Be(80m);
        result.Items[0].AppliedCouponCode.Should().Be("PERCENT");
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_FixedCoupon_AppliesCorrectly()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = "FIXED";
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = 50m, Course = new Course { CouponId = 1, Price = 50m } },
            new CartItem { CourseId = 2, Price = null, Course = new Course { CouponId = 1, Price = 20m } } // To cover c.Price ?? c.Course?.Price
        };
        var coupon = new Coupon { CouponId = 1, CouponCode = "FIXED", IsActive = true, MinOrderValue = 10m, CouponType = "fixed", DiscountValue = 100m }; // testing the Min(discount, eligibleSubTotal)
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetByCodeAsync("FIXED").Returns(coupon);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(new List<Coupon>());
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.SubTotal.Should().Be(70m);
        result.DiscountAmount.Should().Be(70m); // Capped at eligibleSubTotal 70m
        result.Total.Should().Be(0m);
        result.AppliedCouponCode.Should().Be("FIXED");
        result.Items[0].DiscountAmount.Should().Be(50m); // Capped at original price
        result.Items[0].DiscountedPrice.Should().Be(0m);
    }
    
    [Fact]
    public async Task GetCartSummaryAsync_MultipleCoupons_CalculatesCorrectly()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = " C1 , C2 ";
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = 50m, Course = new Course { CouponId = 1 } },
            new CartItem { CourseId = 2, Price = 100m, Course = new Course { CouponId = 2 } }
        };
        var coupon1 = new Coupon { CouponId = 1, CouponCode = "C1", IsActive = true, MinOrderValue = 10m, CouponType = "percentage", DiscountValue = 10m }; // 5m discount
        var coupon2 = new Coupon { CouponId = 2, CouponCode = "C2", IsActive = true, MinOrderValue = 10m, CouponType = "fixed", DiscountValue = 20m }; // 20m discount
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetByCodeAsync("C1").Returns(coupon1);
        _couponRepo.GetByCodeAsync("C2").Returns(coupon2);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(new List<Coupon>());
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.SubTotal.Should().Be(150m);
        result.DiscountAmount.Should().Be(25m);
        result.Total.Should().Be(125m);
        result.AppliedCouponCode.Should().Be("C1,C2");
        result.CouponMessage.Should().Contain("C1").And.Contain("C2");
    }

    [Fact]
    public async Task GetCartSummaryAsync_AvailableCouponsEligibility_CorrectlyEvaluated()
    {
        //Arrange 1
        int userId = 1;
        string couponCode = null;
        var cartItems = new List<CartItem>
        {
            new CartItem { CourseId = 1, Price = 20m, Course = new Course { CourseId = 1, CouponId = 1 } },
            new CartItem { CourseId = 2, Price = 30m, Course = new Course { CourseId = 2, CouponId = 2 } }
        };
        
        var availableCoupons = new List<Coupon>
        {
            new Coupon { CouponId = 99, CouponCode = "NA", CouponType = "fixed", DiscountValue = 5m, MinOrderValue = 10m, Courses = new List<Course>() }, // Not applicable (eligible 0)
            new Coupon { CouponId = 1, CouponCode = "UNDER", CouponType = "fixed", DiscountValue = 5m, MinOrderValue = 50m, Courses = new List<Course> { new Course { CourseId = 1 } } }, // Under min order
            new Coupon { CouponId = 2, CouponCode = "PERCENT_OK", CouponType = "percentage", DiscountValue = 10m, MinOrderValue = 20m, Courses = new List<Course>() }, // Eligible percentage
            new Coupon { CouponId = 2, CouponCode = "FIXED_OK", CouponType = null, DiscountValue = 5m, MinOrderValue = 20m, Courses = new List<Course>() } // Eligible fixed (null defaults to fixed)
        };
        
        //Arrange 2
        _cartRepo.GetCartItemsWithDetailsAsync(userId).Returns(cartItems);
        _couponRepo.GetActiveAvailableCouponsAsync(Arg.Any<DateTime>()).Returns(availableCoupons);
        
        //Act
        var result = await _sut.GetCartSummaryAsync(userId, couponCode);
        
        //Assert
        result.AvailableCoupons.Count.Should().Be(4);
        
        var naCoupon = result.AvailableCoupons.First(c => c.CouponCode == "NA");
        naCoupon.IsEligible.Should().BeFalse();
        naCoupon.ConditionMessage.Should().Be("Not applicable to any course in the cart");
        
        var underCoupon = result.AvailableCoupons.First(c => c.CouponCode == "UNDER");
        underCoupon.IsEligible.Should().BeFalse();
        underCoupon.ConditionMessage.Should().Be("Spend another $30.00 to use this coupon"); // 50 - 20 = 30
        
        var pctCoupon = result.AvailableCoupons.First(c => c.CouponCode == "PERCENT_OK");
        pctCoupon.IsEligible.Should().BeTrue();
        pctCoupon.ConditionMessage.Should().Be("Off 10%");
        
        var fixedCoupon = result.AvailableCoupons.First(c => c.CouponCode == "FIXED_OK");
        fixedCoupon.IsEligible.Should().BeTrue();
        fixedCoupon.ConditionMessage.Should().Be("Off $5.00");
    }
}
