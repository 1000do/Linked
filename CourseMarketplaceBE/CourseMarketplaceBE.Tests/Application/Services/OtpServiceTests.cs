using System;
using System.Collections;
using System.Reflection;
using CourseMarketplaceBE.Application.Services;
using FluentAssertions;
using Xunit;

namespace CourseMarketplaceBE.Tests.Application.Services
{
    public class OtpServiceTests
    {
        private readonly OtpService _sut;

        public OtpServiceTests()
        {
            _sut = new OtpService();
        }

        private void InjectOtp(string email, string purpose, string otp, DateTime expireAt)
        {
            var key = $"{email}_{purpose}";
            var serviceType = typeof(OtpService);
            var storeField = serviceType.GetField("_store", BindingFlags.NonPublic | BindingFlags.Static);
            var store = (IDictionary)storeField!.GetValue(null)!;

            var otpInfoType = serviceType.GetNestedType("OtpInfo", BindingFlags.NonPublic);
            var otpInfoInstance = Activator.CreateInstance(otpInfoType!);
            
            var codeProp = otpInfoType!.GetProperty("Code");
            var expireAtProp = otpInfoType.GetProperty("ExpireAt");

            codeProp!.SetValue(otpInfoInstance, otp);
            expireAtProp!.SetValue(otpInfoInstance, expireAt);

            store[key] = otpInfoInstance;
        }

        private bool IsInStore(string email, string purpose)
        {
            var key = $"{email}_{purpose}";
            var serviceType = typeof(OtpService);
            var storeField = serviceType.GetField("_store", BindingFlags.NonPublic | BindingFlags.Static);
            var store = (IDictionary)storeField!.GetValue(null)!;
            return store.Contains(key);
        }

        [Fact]
        public void GenerateOtp_ValidInputs_GeneratesAndStoresOtpAndReturnsCode()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";

            //Arrange 2
            // No dependencies to mock

            //Act
            var result = _sut.GenerateOtp(email, purpose);

            //Assert
            result.Should().NotBeNullOrEmpty();
            result.Length.Should().Be(6);
            IsInStore(email, purpose).Should().BeTrue();
        }

        [Fact]
        public void ValidateOtp_KeyNotFound_ReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var otp = "123456";

            //Arrange 2
            // No dependencies to mock

            //Act
            var result = _sut.ValidateOtp(email, otp, purpose);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ValidateOtp_ExpiredOtp_RemovesKeyAndReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var otp = "123456";
            var expiredTime = DateTime.UtcNow.AddMinutes(-5);

            //Arrange 2
            InjectOtp(email, purpose, otp, expiredTime);

            //Act
            var result = _sut.ValidateOtp(email, otp, purpose);

            //Assert
            result.Should().BeFalse();
            IsInStore(email, purpose).Should().BeFalse();
        }

        [Fact]
        public void ValidateOtp_ValidKeyButWrongOtp_ReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var correctOtp = "123456";
            var wrongOtp = "654321";
            var validTime = DateTime.UtcNow.AddMinutes(5);

            //Arrange 2
            InjectOtp(email, purpose, correctOtp, validTime);

            //Act
            var result = _sut.ValidateOtp(email, wrongOtp, purpose);

            //Assert
            result.Should().BeFalse();
            IsInStore(email, purpose).Should().BeTrue();
        }

        [Fact]
        public void ValidateOtp_ValidKeyAndCorrectOtp_ReturnsTrue()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var correctOtp = "123456";
            var validTime = DateTime.UtcNow.AddMinutes(5);

            //Arrange 2
            InjectOtp(email, purpose, correctOtp, validTime);

            //Act
            var result = _sut.ValidateOtp(email, correctOtp, purpose);

            //Assert
            result.Should().BeTrue();
            IsInStore(email, purpose).Should().BeTrue(); // Validate keeps it in store
        }

        [Fact]
        public void ConsumeOtp_KeyNotFound_ReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var otp = "123456";

            //Arrange 2
            // No dependencies to mock

            //Act
            var result = _sut.ConsumeOtp(email, otp, purpose);

            //Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void ConsumeOtp_ExpiredOtp_RemovesKeyAndReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var otp = "123456";
            var expiredTime = DateTime.UtcNow.AddMinutes(-5);

            //Arrange 2
            InjectOtp(email, purpose, otp, expiredTime);

            //Act
            var result = _sut.ConsumeOtp(email, otp, purpose);

            //Assert
            result.Should().BeFalse();
            IsInStore(email, purpose).Should().BeFalse();
        }

        [Fact]
        public void ConsumeOtp_ValidKeyButWrongOtp_ReturnsFalse()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var correctOtp = "123456";
            var wrongOtp = "654321";
            var validTime = DateTime.UtcNow.AddMinutes(5);

            //Arrange 2
            InjectOtp(email, purpose, correctOtp, validTime);

            //Act
            var result = _sut.ConsumeOtp(email, wrongOtp, purpose);

            //Assert
            result.Should().BeFalse();
            IsInStore(email, purpose).Should().BeTrue(); // Wrong OTP should not remove it
        }

        [Fact]
        public void ConsumeOtp_ValidKeyAndCorrectOtp_RemovesKeyAndReturnsTrue()
        {
            //Arrange 1
            var email = Guid.NewGuid().ToString();
            var purpose = "TestPurpose";
            var correctOtp = "123456";
            var validTime = DateTime.UtcNow.AddMinutes(5);

            //Arrange 2
            InjectOtp(email, purpose, correctOtp, validTime);

            //Act
            var result = _sut.ConsumeOtp(email, correctOtp, purpose);

            //Assert
            result.Should().BeTrue();
            IsInStore(email, purpose).Should().BeFalse(); // Consume removes it from store
        }
    }
}
