using System.IO;

var filePath = @"C:\Users\binh4\source\repos\Linked\CourseMarketplaceBE\CourseMarketplaceBE.Tests\Application\Services\GiftCheckoutServiceTests.cs";
var content = File.ReadAllText(filePath);

// 1. Add Reflection using
content = content.Replace("using Xunit;", "using Xunit;\r\nusing System.Reflection;");

// 2. Replace InlineData
var oldInlineData = @"        [Theory]
        [InlineData(""GB"", ""GBP"")]
        [InlineData(""CA"", ""CAD"")]
        [InlineData(""CH"", ""CHF"")]
        [InlineData(""AT"", ""EUR"")]
        [InlineData(""FR"", ""EUR"")]
        [InlineData(""DE"", ""EUR"")]
        [InlineData(""IT"", ""EUR"")]
        [InlineData(""ES"", ""EUR"")]
        [InlineData(""BG"", ""BGN"")]
        [InlineData(""HR"", ""EUR"")]
        [InlineData(""CZ"", ""CZK"")]
        [InlineData(""DK"", ""DKK"")]
        [InlineData(""HU"", ""HUF"")]
        [InlineData(""IS"", ""ISK"")]
        [InlineData(""NO"", ""NOK"")]
        [InlineData(""PL"", ""PLN"")]
        [InlineData(""RO"", ""RON"")]
        [InlineData(""SE"", ""SEK"")]
        [InlineData(""AU"", ""AUD"")]
        [InlineData(""VN"", ""VND"")]
        [InlineData(""US"", ""USD"")]
        [InlineData(""JP"", ""USD"")]
        [InlineData("""", ""USD"")]
        [InlineData(null, ""USD"")]
        public async Task GetCurrencyFromCountry_ValidCountry_MapsCorrectly";

var newInlineData = @"        [Theory]
        [InlineData(""GB"", ""GBP"")]
        [InlineData(""CA"", ""CAD"")]
        [InlineData(""CH"", ""CHF"")]
        [InlineData(""AT"", ""EUR"")]
        [InlineData(""BE"", ""EUR"")]
        [InlineData(""CY"", ""EUR"")]
        [InlineData(""EE"", ""EUR"")]
        [InlineData(""FI"", ""EUR"")]
        [InlineData(""FR"", ""EUR"")]
        [InlineData(""DE"", ""EUR"")]
        [InlineData(""GR"", ""EUR"")]
        [InlineData(""IE"", ""EUR"")]
        [InlineData(""IT"", ""EUR"")]
        [InlineData(""LV"", ""EUR"")]
        [InlineData(""LT"", ""EUR"")]
        [InlineData(""LU"", ""EUR"")]
        [InlineData(""MT"", ""EUR"")]
        [InlineData(""NL"", ""EUR"")]
        [InlineData(""PT"", ""EUR"")]
        [InlineData(""SK"", ""EUR"")]
        [InlineData(""SI"", ""EUR"")]
        [InlineData(""ES"", ""EUR"")]
        [InlineData(""BG"", ""BGN"")]
        [InlineData(""HR"", ""EUR"")]
        [InlineData(""CZ"", ""CZK"")]
        [InlineData(""DK"", ""DKK"")]
        [InlineData(""HU"", ""HUF"")]
        [InlineData(""IS"", ""ISK"")]
        [InlineData(""NO"", ""NOK"")]
        [InlineData(""PL"", ""PLN"")]
        [InlineData(""RO"", ""RON"")]
        [InlineData(""SE"", ""SEK"")]
        [InlineData(""AU"", ""AUD"")]
        [InlineData(""VN"", ""VND"")]
        [InlineData(""US"", ""USD"")]
        [InlineData(""JP"", ""USD"")]
        [InlineData("""", ""USD"")]
        [InlineData(null, ""USD"")]
        public async Task GetCurrencyFromCountry_ValidCountry_MapsCorrectly";

content = content.Replace(oldInlineData, newInlineData);

// 3. Add new tests
var oldEnd = @"            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), expectedCurrency, Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<Dictionary<string, string>>());
        }
    }
}";

var newTests = @"            await _paymentGatewayMock.Received(1).CreateCheckoutSessionAsync(Arg.Any<List<PaymentLineItem>>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), expectedCurrency, Arg.Any<string>(), Arg.Any<decimal?>(), Arg.Any<Dictionary<string, string>>());
        }

        // ═══════════════════════════════════════════════════════════════════════════
        // Missing Branches Implementation
        // ═══════════════════════════════════════════════════════════════════════════

        [Fact]
        public async Task ProcessPaymentSuccessAsync_ExistingTransactionNotSucceeded_ReturnsFalseAndProceeds()
        {
            //Arrange 1
            string sessionId = ""sess_1"";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = ""pending"", InstructorPayouts = new List<InstructorPayout> { new InstructorPayout() } } 
            };
            var metadata = new Dictionary<string, string> { { ""userId"", ""1"" }, { ""courseIds"", ""2"" }, { ""recipientEmail"", ""rec@test.com"" } };
            var course = new Course { Title = ""Test"", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            
            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns(""pi_1"");
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns(course);
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""15"");
            _userRepoMock.GetAccountByEmailAsync(""rec@test.com"").Returns(new Account { AccountId = 10, Email = ""rec@test.com"" });

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().NotThrowAsync<InvalidOperationException>();
            await _paymentGatewayMock.Received(1).GetSessionMetadataAsync(sessionId);
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_ExistingTransactionNotPayoutCreated_ReturnsFalseAndProceeds()
        {
            //Arrange 1
            string sessionId = ""sess_1"";
            var transactions = new List<Transaction> 
            { 
                new Transaction { TransactionsStatus = ""succeeded"", InstructorPayouts = new List<InstructorPayout>() } 
            };
            var metadata = new Dictionary<string, string> { { ""userId"", ""1"" }, { ""courseIds"", ""2"" }, { ""recipientEmail"", ""rec@test.com"" } };
            var course = new Course { Title = ""Test"", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = 3 };
            
            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(transactions);
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Returns(metadata);
            _paymentGatewayMock.GetPaymentReferenceAsync(sessionId).Returns(""pi_1"");
            var dbTransactionMock = Substitute.For<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
            _repoMock.BeginTransactionAsync().Returns(dbTransactionMock);
            _courseRepoMock.GetCourseWithInstructorAsync(2).Returns(course);
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""15"");
            _userRepoMock.GetAccountByEmailAsync(""rec@test.com"").Returns(new Account { AccountId = 10, Email = ""rec@test.com"" });

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().NotThrowAsync<InvalidOperationException>();
            await _paymentGatewayMock.Received(1).GetSessionMetadataAsync(sessionId);
        }

        [Fact]
        public async Task InitiateGiftCheckoutAsync_InstructorIdNull_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = ""rec@test.com"", SuccessUrl = ""success"", CancelUrl = ""cancel"" };
            var course = new Course { Title = ""Test"", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = null };
            
            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetInstructorStripeAccountIdAsync(0).Returns("""");

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftCheckoutAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(""Instructor has not connected a Stripe payment account."");
            await _userRepoMock.Received(1).GetInstructorStripeAccountIdAsync(0);
        }

        [Fact]
        public async Task InitiateGiftPaymentIntentAsync_InstructorIdNull_ThrowsInvalidOperationException()
        {
            //Arrange 1
            int userId = 1;
            var request = new GiftCheckoutRequest { CourseId = 1, RecipientEmail = ""rec@test.com"", SuccessUrl = ""success"", CancelUrl = ""cancel"" };
            var course = new Course { Title = ""Test"", CourseStatus = CourseMarketplaceBE.Domain.Constants.CourseStatus.Published.ToValue(), InstructorId = null };
            
            //Arrange 2
            _courseRepoMock.GetCourseWithInstructorAsync(request.CourseId).Returns(course);
            _userRepoMock.GetInstructorStripeAccountIdAsync(0).Returns("""");

            //Act
            Func<Task> act = async () => await _sut.InitiateGiftPaymentIntentAsync(userId, request);

            //Assert
            await act.Should().ThrowAsync<InvalidOperationException>().WithMessage(""Instructor has not connected a Stripe payment account."");
        }

        [Fact]
        public async Task ProcessPaymentSuccessAsync_PaymentGatewayGetSessionMetadataThrows_ThrowsException()
        {
            //Arrange 1
            string sessionId = ""sess_1"";
            var expectedException = new Exception(""Stripe API error"");
            
            //Arrange 2
            _repoMock.GetTransactionsBySessionIdAsync(sessionId).Returns(new List<Transaction>());
            _paymentGatewayMock.GetSessionMetadataAsync(sessionId).Throws(expectedException);

            //Act
            Func<Task> act = async () => await _sut.ProcessPaymentSuccessAsync(sessionId);

            //Assert
            await act.Should().ThrowAsync<Exception>().WithMessage(""Stripe API error"");
        }

        [Fact]
        public async Task CalculatePayoutDateAsync_VariousConfigs_ReturnsExpectedDate()
        {
            //Arrange 1
            var method = typeof(GiftCheckoutService).GetMethod(""CalculatePayoutDateAsync"", BindingFlags.NonPublic | BindingFlags.Instance);
            var transactionDate = new DateTime(2023, 1, 15, 0, 0, 0, DateTimeKind.Utc);
            
            //Arrange 2
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""""); // Empty config defaults to 15
            
            //Act
            var result1 = await (Task<DateTime>)method.Invoke(_sut, new object[] { transactionDate })!;
            
            //Arrange 2 - Invalid Config
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""abc""); // TryParse fails, returns 0, defaults to 15
            
            //Act
            var result2 = await (Task<DateTime>)method.Invoke(_sut, new object[] { transactionDate })!;
            
            //Arrange 2 - Clamping (Next month is Feb, days = 28, config = 31)
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""31""); 
            
            //Act
            var result3 = await (Task<DateTime>)method.Invoke(_sut, new object[] { transactionDate })!;
            
            //Arrange 2 - Zero or negative config
            _adminFinanceServiceMock.GetPayoutDaysConfigAsync().Returns(""-1, 0""); // Where(d => d > 0) is empty
            
            //Act
            var result4 = await (Task<DateTime>)method.Invoke(_sut, new object[] { transactionDate })!;

            //Assert
            result1.Should().Be(new DateTime(2023, 2, 15, 0, 0, 0, DateTimeKind.Utc));
            result2.Should().Be(new DateTime(2023, 2, 15, 0, 0, 0, DateTimeKind.Utc));
            result3.Should().Be(new DateTime(2023, 2, 28, 0, 0, 0, DateTimeKind.Utc));
            result4.Should().Be(new DateTime(2023, 2, 15, 0, 0, 0, DateTimeKind.Utc));
        }

        [Fact]
        public async Task ProcessGiftFulfillmentAsync_MissingMetadataAndSender_UsesDefaults()
        {
            //Arrange 1
            var method = typeof(GiftCheckoutService).GetMethod(""ProcessGiftFulfillmentAsync"", BindingFlags.NonPublic | BindingFlags.Instance);
            var metadata = new Dictionary<string, string>(); // Missing recipientName, giftMessage, cardTheme, feBaseUrl
            var course = new Course { Title = ""Test Course"" };
            int userId = 1;
            int orderItemId = 2;
            
            //Arrange 2
            _configurationMock.GetSection(""FrontendBaseUrl"").Value.Returns((string?)null); // Fallback to localhost:5208
            _userRepoMock.GetAccountByIdAsync(userId).Returns((Account?)null); // Fallback to ""A Friend""
            
            //Act
            await (Task)method.Invoke(_sut, new object[] { userId, orderItemId, course, metadata })!;
            
            //Assert
            await _giftRepoMock.Received(1).AddAsync(Arg.Is<Gift>(g => g.RecipientName == null && g.GiftMessage == null && g.CardTheme == ""classic""));
            await _emailServiceMock.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains(""A Friend"")), Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessGiftFulfillmentAsync_SenderNameFallbacks_UsesUsernameThenEmail()
        {
            //Arrange 1
            var method = typeof(GiftCheckoutService).GetMethod(""ProcessGiftFulfillmentAsync"", BindingFlags.NonPublic | BindingFlags.Instance);
            var metadata = new Dictionary<string, string>();
            var course = new Course { Title = ""Test Course"" };
            int userId = 1;
            int orderItemId = 2;
            var account1 = new Account { AccountId = 1, User = new User { FullName = null }, Username = ""testuser"" };
            var account2 = new Account { AccountId = 2, User = new User { FullName = null }, Username = null, Email = ""test@test.com"" };
            var account3 = new Account { AccountId = 3, User = null, Username = null, Email = null };
            
            //Arrange 2
            _configurationMock.GetSection(""FrontendBaseUrl"").Value.Returns((string?)null);
            
            //Act 1 - Username
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account1);
            await (Task)method.Invoke(_sut, new object[] { userId, orderItemId, course, metadata })!;
            
            //Act 2 - Email
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account2);
            await (Task)method.Invoke(_sut, new object[] { userId, orderItemId, course, metadata })!;
            
            //Act 3 - Null user props fallback to A Friend
            _userRepoMock.GetAccountByIdAsync(userId).Returns(account3);
            await (Task)method.Invoke(_sut, new object[] { userId, orderItemId, course, metadata })!;

            //Assert
            await _emailServiceMock.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains(""testuser"")), Arg.Any<string>());
            await _emailServiceMock.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains(""test@test.com"")), Arg.Any<string>());
            await _emailServiceMock.Received(1).SendEmailAsync(Arg.Any<string>(), Arg.Is<string>(s => s.Contains(""A Friend"")), Arg.Any<string>());
        }

        [Fact]
        public async Task ProcessPayoutAndNotificationAsync_InstructorIdOrTitleNull_HandlesCorrectly()
        {
            //Arrange 1
            var method = typeof(GiftCheckoutService).GetMethod(""ProcessPayoutAndNotificationAsync"", BindingFlags.NonPublic | BindingFlags.Instance);
            var transaction = new Transaction { TransactionId = 1 };
            decimal purchasePrice = 100m;
            decimal currentTransferRate = 80m;
            
            //Act 1 - InstructorId is null
            await (Task)method.Invoke(_sut, new object?[] { transaction, null, ""Course Title"", purchasePrice, currentTransferRate })!;
            
            //Act 2 - Title is null
            await (Task)method.Invoke(_sut, new object?[] { transaction, 1, null, purchasePrice, currentTransferRate })!;

            //Assert
            await _notificationServiceMock.Received(1).SendNotificationAsync(1, ""You have a new order"", Arg.Is<string>(s => s.Contains(""'your course'"")), Arg.Any<string>());
        }

        [Fact]
        public void BuildGiftMetadata_FeBaseUrlConfigNull_UsesLocalhost()
        {
            //Arrange 1
            var method = typeof(GiftCheckoutService).GetMethod(""BuildGiftMetadata"", BindingFlags.NonPublic | BindingFlags.Instance);
            int userId = 1;
            var request = new GiftCheckoutRequest { SuccessUrl = ""http://localhost/other"", RecipientEmail = ""r@t.com"" };
            int courseId = 2;
            
            //Arrange 2
            _configurationMock.GetSection(""FrontendBaseUrl"").Value.Returns((string?)null);

            //Act
            var result = (Dictionary<string, string>)method.Invoke(_sut, new object[] { userId, request, courseId })!;

            //Assert
            result[""feBaseUrl""].Should().Be(""http://localhost:5208"");
        }
    }
}";
content = content.Replace(oldEnd, newTests);

File.WriteAllText(filePath, content);
Console.WriteLine("Successfully updated tests.");
