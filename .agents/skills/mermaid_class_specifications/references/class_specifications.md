# Class Specifications

## Layer: Backend Controller
### 1. AdminAccountController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateStaff(request: CreateStaffRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Create Staff" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CreateStaffRequestDTO` as input parameters. |
| 2 | FlagAccount(id: int, request: FlagAccountRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Flag Account" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: FlagAccountRequestDTO` as input parameters. |
| 3 | GetAccountDetail(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Account Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 4 | GetAccountTransactions(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Account Transactions" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | GetAccounts(keyword: string?, role: string?, page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get Accounts" operation asynchronously, returning `Task<IActionResult>`. It accepts `keyword: string?, role: string?, page: int, pageSize: int` as input parameters. |
| 6 | ToggleBan(id: int) : Task&lt;IActionResult&gt; | Executes the "Toggle Ban" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 7 | UpdateStaff(id: int, request: UpdateStaffRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Update Staff" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: UpdateStaffRequestDTO` as input parameters. |
| 8 | _adminAccountService : IAdminAccountService | Provides the Admin Account Service dependency. |

### 2. AdminAiServiceController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddModel(req: CreateAiModelRequest) : Task&lt;IActionResult&gt; | Executes the "Add Model" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: CreateAiModelRequest` as input parameters. |
| 2 | GetAllModels() : Task&lt;IActionResult&gt; | Executes the "Get All Models" operation asynchronously, returning `Task<IActionResult>`. |
| 3 | GetCourseModerationLogs(req: PagedRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get Course Moderation Logs" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: PagedRequestDto` as input parameters. |
| 4 | GetCourseReviewModerationLogs(req: PagedRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get Course Review Moderation Logs" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: PagedRequestDto` as input parameters. |
| 5 | GetLessonReviewModerationLogs(req: PagedRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get Lesson Review Moderation Logs" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: PagedRequestDto` as input parameters. |
| 6 | GetPagedModels(request: PagedRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get Paged Models" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: PagedRequestDto` as input parameters. |
| 7 | ToggleModelStatus(id: int) : Task&lt;IActionResult&gt; | Executes the "Toggle Model Status" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 8 | UpdateModel(id: int, req: UpdateAiModelRequest) : Task&lt;IActionResult&gt; | Executes the "Update Model" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, req: UpdateAiModelRequest` as input parameters. |
| 9 | UpdateThresholds(req: UpdateThresholdsRequest) : Task&lt;IActionResult&gt; | Executes the "Update Thresholds" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: UpdateThresholdsRequest` as input parameters. |
| 10 | _aiConfigService : IAiConfigurationService | Provides the Ai Config Service dependency. |
| 11 | _aiLogService : IAiModerationLogService | Provides the Ai Log Service dependency. |
| 12 | _aiModelService : IAiModelManagementService | Provides the Ai Model Service dependency. |

### 3. AdminApprovalController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetDetail(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 2 | GetPending(page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get Pending" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int` as input parameters. |
| 3 | Process(dto: UpdateApprovalStatusDto) : Task&lt;IActionResult&gt; | Executes the "Process" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: UpdateApprovalStatusDto` as input parameters. |
| 4 | _service : IInstructorApprovalService | Provides the Service dependency. |

### 4. AdminFinanceController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveRefund(transactionId: int, request: RefundDecisionRequest) : Task&lt;IActionResult&gt; | Executes the "Approve Refund" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, request: RefundDecisionRequest` as input parameters. |
| 2 | GetFinancialSummary(year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Get Financial Summary" operation asynchronously, returning `Task<IActionResult>`. It accepts `year: int?, month: int?` as input parameters. |
| 3 | GetPayouts(year: int?, month: int?, page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get Payouts" operation asynchronously, returning `Task<IActionResult>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 4 | GetPendingRefundRequests(page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get Pending Refund Requests" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int` as input parameters. |
| 5 | GetPlatformBalance() : Task&lt;IActionResult&gt; | Executes the "Get Platform Balance" operation asynchronously, returning `Task<IActionResult>`. |
| 6 | GetWithdrawalHistory(year: int?, month: int?, page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get Withdrawal History" operation asynchronously, returning `Task<IActionResult>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 7 | RejectRefund(transactionId: int, request: RefundDecisionRequest) : Task&lt;IActionResult&gt; | Executes the "Reject Refund" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, request: RefundDecisionRequest` as input parameters. |
| 8 | SetPayoutDays(request: SetPayoutDaysRequest) : Task&lt;IActionResult&gt; | Executes the "Set Payout Days" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: SetPayoutDaysRequest` as input parameters. |
| 9 | SetTransferRate(request: SetTransferRateRequest) : Task&lt;IActionResult&gt; | Executes the "Set Transfer Rate" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: SetTransferRateRequest` as input parameters. |
| 10 | Withdraw(request: WithdrawRequest) : Task&lt;IActionResult&gt; | Executes the "Withdraw" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: WithdrawRequest` as input parameters. |
| 11 | _financeService : IAdminFinanceService | Provides the Finance Service dependency. |
| 12 | _instructorService : IInstructorService | Provides the Instructor Service dependency. |

### 5. AdminModerationController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveCourse(id: int, feedback: string?) : Task&lt;IActionResult&gt; | Executes the "Approve Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, feedback: string?` as input parameters. |
| 2 | FlagCourse(id: int, reason: string) : Task&lt;IActionResult&gt; | Executes the "Flag Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, reason: string` as input parameters. |
| 3 | GetAllCourseReports(request: PagedReportRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get All Course Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 4 | GetAllCourseReviewReports(request: PagedReportRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get All Course Review Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 5 | GetAllLessonReviewReports(request: PagedReportRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get All Lesson Review Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 6 | GetPendingCourses(filter: ModerationFilterDto) : Task&lt;IActionResult&gt; | Executes the "Get Pending Courses" operation asynchronously, returning `Task<IActionResult>`. It accepts `filter: ModerationFilterDto` as input parameters. |
| 7 | GetReportStats() : Task&lt;IActionResult&gt; | Executes the "Get Report Stats" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | RejectCourseDetailed(request: RejectCourseDetailedRequest) : Task&lt;IActionResult&gt; | Executes the "Reject Course Detailed" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: RejectCourseDetailedRequest` as input parameters. |
| 9 | ResolveCourseReport(reportId: int, request: ResolveReportRequest) : Task&lt;IActionResult&gt; | Executes the "Resolve Course Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, request: ResolveReportRequest` as input parameters. |
| 10 | ResolveCourseReviewReport(reportId: int, request: ResolveReportRequest) : Task&lt;IActionResult&gt; | Executes the "Resolve Course Review Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, request: ResolveReportRequest` as input parameters. |
| 11 | ResolveLessonReviewReport(reportId: int, request: ResolveReportRequest) : Task&lt;IActionResult&gt; | Executes the "Resolve Lesson Review Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, request: ResolveReportRequest` as input parameters. |
| 12 | _courseModerationService : ICourseModerationService | Provides the Course Moderation Service dependency. |
| 13 | _reportService : IReportModerationService | Provides the Report Service dependency. |

### 6. AdminProfileController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetProfile() : Task&lt;IActionResult&gt; | Executes the "Get Profile" operation asynchronously, returning `Task<IActionResult>`. |
| 2 | UpdateProfile(request: UpdateManagerProfileRequest) : Task&lt;IActionResult&gt; | Executes the "Update Profile" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: UpdateManagerProfileRequest` as input parameters. |
| 3 | _profileService : IManagerProfileService | Provides the Profile Service dependency. |

### 7. AuthController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ForgotPassword(request: ForgotPasswordRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Forgot Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: ForgotPasswordRequestDTO` as input parameters. |
| 2 | GoogleLogin(request: GoogleLoginRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Google Login" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: GoogleLoginRequestDTO` as input parameters. |
| 3 | Login(request: LoginRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Login" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: LoginRequestDTO` as input parameters. |
| 4 | Logout() : Task&lt;IActionResult&gt; | Executes the "Logout" operation asynchronously, returning `Task<IActionResult>`. |
| 5 | Register(request: RegisterRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Register" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: RegisterRequestDTO` as input parameters. |
| 6 | ResetPassword(request: ResetPasswordRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Reset Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: ResetPasswordRequestDTO` as input parameters. |
| 7 | VerifyEmail(request: VerifyEmailRequest) : Task&lt;IActionResult&gt; | Executes the "Verify Email" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: VerifyEmailRequest` as input parameters. |
| 8 | VerifyOtp(request: VerifyOtpRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Verify Otp" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: VerifyOtpRequestDTO` as input parameters. |
| 9 | _authService : IAuthService | Provides the Auth Service dependency. |

### 8. CartController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToCart(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Add To Cart" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 2 | GetCartSummary(couponCode: string?) : Task&lt;IActionResult&gt; | Executes the "Get Cart Summary" operation asynchronously, returning `Task<IActionResult>`. It accepts `couponCode: string?` as input parameters. |
| 3 | GetSummary(couponCode: string?) : Task&lt;IActionResult&gt; | Executes the "Get Summary" operation asynchronously, returning `Task<IActionResult>`. It accepts `couponCode: string?` as input parameters. |
| 4 | RemoveFromCart(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Remove From Cart" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 5 | _authService : IAuthService | Provides the Auth Service dependency. |
| 6 | _cartService : ICartService | Provides the Cart Service dependency. |

### 9. ChatAttachmentController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Upload(file: IFormFile) : Task&lt;IActionResult&gt; | Executes the "Upload" operation asynchronously, returning `Task<IActionResult>`. It accepts `file: IFormFile` as input parameters. |
| 2 | _uploadService : ICloudinaryUploadService | Provides the Upload Service dependency. |

### 10. ChatController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ClearChatHistory(chatId: int) : Task&lt;IActionResult&gt; | Executes the "Clear Chat History" operation asynchronously, returning `Task<IActionResult>`. It accepts `chatId: int` as input parameters. |
| 2 | CreateChat(dto: CreateChatDto) : Task&lt;IActionResult&gt; | Executes the "Create Chat" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: CreateChatDto` as input parameters. |
| 3 | GetChatHistory(roomId: int) : Task&lt;IActionResult&gt; | Executes the "Get Chat History" operation asynchronously, returning `Task<IActionResult>`. It accepts `roomId: int` as input parameters. |
| 4 | GetMyChats() : Task&lt;IActionResult&gt; | Executes the "Get My Chats" operation asynchronously, returning `Task<IActionResult>`. |
| 5 | RequestSupport(dto: SupportRequestDto) : Task&lt;IActionResult&gt; | Executes the "Request Support" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: SupportRequestDto` as input parameters. |
| 6 | SearchChats(q: string) : Task&lt;IActionResult&gt; | Executes the "Search Chats" operation asynchronously, returning `Task<IActionResult>`. It accepts `q: string` as input parameters. |
| 7 | _chatService : IChatService | Provides the Chat Service dependency. |
| 8 | _hubContext : IHubContext&lt;ChatHub&gt; | Provides the Hub Context dependency. |

### 11. CheckoutController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateIntent(userId: string, couponCode: string) : Task&lt;IActionResult&gt; | Executes the "Create Intent" operation asynchronously, returning `Task<IActionResult>`. It accepts `userId: string, couponCode: string` as input parameters. |
| 2 | GetCartSummary(couponCode: string) : Task&lt;IActionResult&gt; | Executes the "Get Cart Summary" operation asynchronously, returning `Task<IActionResult>`. It accepts `couponCode: string` as input parameters. |
| 3 | GetProfile() : Task&lt;IActionResult&gt; | Executes the "Get Profile" operation asynchronously, returning `Task<IActionResult>`. |
| 4 | GiftCreateIntent(request: GiftCheckoutRequest) : Task&lt;IActionResult&gt; | Executes the "Gift Create Intent" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: GiftCheckoutRequest` as input parameters. |
| 5 | GiftSuccessIntent(payment_intent_id: string) : Task&lt;IActionResult&gt; | Executes the "Gift Success Intent" operation asynchronously, returning `Task<IActionResult>`. It accepts `payment_intent_id: string` as input parameters. |
| 6 | SuccessIntent(payment_intent_id: string) : Task&lt;IActionResult&gt; | Executes the "Success Intent" operation asynchronously, returning `Task<IActionResult>`. It accepts `payment_intent_id: string` as input parameters. |
| 7 | _checkoutService : ICheckoutService | Provides the Checkout Service dependency. |
| 8 | _giftCheckoutService : IGiftCheckoutService | Provides the Gift Checkout Service dependency. |

### 12. CouponController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCoupon(request: CreateCouponRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Create Coupon" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CreateCouponRequestDTO` as input parameters. |
| 2 | DeleteCoupon(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete Coupon" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | GetCoupons() : Task&lt;IActionResult&gt; | Executes the "Get Coupons" operation asynchronously, returning `Task<IActionResult>`. |
| 4 | GetCoupons(isActive: bool?, type: string?) : Task&lt;IActionResult&gt; | Executes the "Get Coupons" operation asynchronously, returning `Task<IActionResult>`. It accepts `isActive: bool?, type: string?` as input parameters. |
| 5 | GetCoupons(isActive: bool?, type: string?, search: string?) : Task&lt;IActionResult&gt; | Executes the "Get Coupons" operation asynchronously, returning `Task<IActionResult>`. It accepts `isActive: bool?, type: string?, search: string?` as input parameters. |
| 6 | GetCoupons(search: string?) : Task&lt;IActionResult&gt; | Executes the "Get Coupons" operation asynchronously, returning `Task<IActionResult>`. It accepts `search: string?` as input parameters. |
| 7 | UpdateCoupon(id: int, request: UpdateCouponRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Update Coupon" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: UpdateCouponRequestDTO` as input parameters. |
| 8 | _couponService : ICouponService | Provides the Coupon Service dependency. |

### 13. CourseController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCourse(request: CourseCreateRequest) : Task&lt;IActionResult&gt; | Executes the "Create Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CourseCreateRequest` as input parameters. |
| 2 | DeleteCourse(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | GetInstructorId() : int | Executes the "Get Instructor Id" operation, returning `int`. |
| 4 | GetMyCourses(search: string?, status: string?, page: int?, pageSize: int?) : Task&lt;IActionResult&gt; | Executes the "Get My Courses" operation asynchronously, returning `Task<IActionResult>`. It accepts `search: string?, status: string?, page: int?, pageSize: int?` as input parameters. |
| 5 | ModerateCourse(request: CourseModerationRequest) : Task&lt;IActionResult&gt; | Executes the "Moderate Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CourseModerationRequest` as input parameters. |
| 6 | UpdateCourse(id: int, request: CourseUpdateRequest) : Task&lt;IActionResult&gt; | Executes the "Update Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: CourseUpdateRequest` as input parameters. |
| 7 | UpdateCourseStatus(id: int, request: UpdateStatusRequest) : Task&lt;IActionResult&gt; | Executes the "Update Course Status" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: UpdateStatusRequest` as input parameters. |
| 8 | _aiModerationService : ICourseAiModerationService | Provides the Ai Moderation Service dependency. |
| 9 | _courseAiModerationService : ICourseAiModerationService | Provides the Course Ai Moderation Service dependency. |
| 10 | _courseCommandService : ICourseCommandService | Provides the Course Command Service dependency. |
| 11 | _courseQueryService : ICourseQueryService | Provides the Course Query Service dependency. |

### 14. CoursePublicController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAllCourses(query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?) : Task&lt;IActionResult&gt; | Executes the "Get All Courses" operation asynchronously, returning `Task<IActionResult>`. It accepts `query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?` as input parameters. |
| 2 | GetCourseDetails(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Course Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | GetUserId() : int? | Executes the "Get User Id" operation, returning `int?`. |
| 4 | _courseQueryService : ICourseQueryService | Provides the Course Query Service dependency. |

### 15. CourseQuizController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuizToCourse(courseId: int, request: AddQuizToCourseRequest) : Task&lt;IActionResult&gt; | Executes the "Add Quiz To Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, request: AddQuizToCourseRequest` as input parameters. |
| 2 | GetInstructorId() : int | Executes the "Get Instructor Id" operation, returning `int`. |
| 3 | _quizService : IQuizService | Provides the Quiz Service dependency. |

### 16. EnrollmentController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | FreeEnroll(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Free Enroll" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 2 | GetMyCourses() : Task&lt;IActionResult&gt; | Executes the "Get My Courses" operation asynchronously, returning `Task<IActionResult>`. |
| 3 | GetProgress(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Get Progress" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 4 | GetUserId() : int? | Executes the "Get User Id" operation, returning `int?`. |
| 5 | _authService : IAuthService | Provides the Auth Service dependency. |
| 6 | _enrollmentService : IEnrollmentService | Provides the Enrollment Service dependency. |

### 17. GiftController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CheckRecipientEnrollment(email: string, courseId: int) : Task&lt;IActionResult&gt; | Executes the "Check Recipient Enrollment" operation asynchronously, returning `Task<IActionResult>`. It accepts `email: string, courseId: int` as input parameters. |
| 2 | ClaimGift(request: GiftClaimRequest) : Task&lt;IActionResult&gt; | Executes the "Claim Gift" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: GiftClaimRequest` as input parameters. |
| 3 | GetCourseDetails(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Course Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 4 | _courseQueryService : ICourseQueryService | Provides the Course Query Service dependency. |
| 5 | _giftService : IGiftService | Provides the Gift Service dependency. |

### 18. InstructorController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Apply(request: InstructorApplicationRequest) : Task&lt;IActionResult&gt; | Executes the "Apply" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: InstructorApplicationRequest` as input parameters. |
| 2 | GetDashboard() : Task&lt;IActionResult&gt; | Executes the "Get Dashboard" operation asynchronously, returning `Task<IActionResult>`. |
| 3 | GetPayoutDays(financeService: IAdminFinanceService) : Task&lt;IActionResult&gt; | Executes the "Get Payout Days" operation asynchronously, returning `Task<IActionResult>`. It accepts `financeService: IAdminFinanceService` as input parameters. |
| 4 | GetPayouts(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Get Payouts" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 5 | GetPublicProfile(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Public Profile" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 6 | GetUserId() : int? | Executes the "Get User Id" operation, returning `int?`. |
| 7 | SetupPayout() : Task&lt;IActionResult&gt; | Executes the "Setup Payout" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | StripeReturn(instructorId: int) : Task&lt;IActionResult&gt; | Executes the "Stripe Return" operation asynchronously, returning `Task<IActionResult>`. It accepts `instructorId: int` as input parameters. |
| 9 | _instructorService : IInstructorService | Provides the Instructor Service dependency. |

### 19. LessonController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddMaterial(lessonId: int, request: MaterialCreateRequest) : Task&lt;IActionResult&gt; | Executes the "Add Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `lessonId: int, request: MaterialCreateRequest` as input parameters. |
| 2 | CreateLesson(request: LessonCreateRequest) : Task&lt;IActionResult&gt; | Executes the "Create Lesson" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: LessonCreateRequest` as input parameters. |
| 3 | DeleteLesson(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete Lesson" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 4 | GetInstructorId() : int | Executes the "Get Instructor Id" operation, returning `int`. |
| 5 | GetTrashMaterials() : Task&lt;IActionResult&gt; | Executes the "Get Trash Materials" operation asynchronously, returning `Task<IActionResult>`. |
| 6 | PermanentDeleteMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Permanent Delete Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 7 | RemoveMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Remove Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 8 | RestoreMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Restore Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 9 | UpdateMaterialDetails(materialId: int, request: MaterialUpdateRequest) : Task&lt;IActionResult&gt; | Executes the "Update Material Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int, request: MaterialUpdateRequest` as input parameters. |
| 10 | _lessonService : ILessonService | Provides the Lesson Service dependency. |

### 20. NotificationController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Delete(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 2 | GetAllNotifications(page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get All Notifications" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int` as input parameters. |
| 3 | GetMyNotifications(page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Get My Notifications" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int` as input parameters. |
| 4 | GetNotificationDetail(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Notification Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | GetUnreadSummary() : Task&lt;IActionResult&gt; | Executes the "Get Unread Summary" operation asynchronously, returning `Task<IActionResult>`. |
| 6 | MarkAllAsRead() : Task&lt;IActionResult&gt; | Executes the "Mark All As Read" operation asynchronously, returning `Task<IActionResult>`. |
| 7 | MarkAsRead(id: int) : Task&lt;IActionResult&gt; | Executes the "Mark As Read" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 8 | SearchEmails(query: string) : Task&lt;IActionResult&gt; | Executes the "Search Emails" operation asynchronously, returning `Task<IActionResult>`. It accepts `query: string` as input parameters. |
| 9 | SendAdvanced(dto: NotificationAdvancedDto) : Task&lt;IActionResult&gt; | Executes the "Send Advanced" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: NotificationAdvancedDto` as input parameters. |
| 10 | SendTest(dto: NotificationSendDto) : Task&lt;IActionResult&gt; | Executes the "Send Test" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: NotificationSendDto` as input parameters. |
| 11 | _notiService : INotificationService | Provides the Noti Service dependency. |

### 21. ProfileController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChangePassword(request: ChangePasswordRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Change Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: ChangePasswordRequestDTO` as input parameters. |
| 2 | GetProfile() : Task&lt;IActionResult&gt; | Executes the "Get Profile" operation asynchronously, returning `Task<IActionResult>`. |
| 3 | UpdateProfile(request: UpdateProfileRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Update Profile" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: UpdateProfileRequestDTO` as input parameters. |
| 4 | _userProfileService : IUserProfileService | Provides the User Profile Service dependency. |

### 22. QuestionBankController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuestion(courseId: int, request: QuizAddQuestionRequest) : Task&lt;ActionResult&gt;QuizQuestionResponse&lt;&gt; | Executes the "Add Question" operation asynchronously, returning `Task<ActionResult>QuizQuestionResponse<>`. It accepts `courseId: int, request: QuizAddQuestionRequest` as input parameters. |
| 2 | DeleteQuestion(questionId: int) : Task&lt;IActionResult&gt; | Executes the "Delete Question" operation asynchronously, returning `Task<IActionResult>`. It accepts `questionId: int` as input parameters. |
| 3 | GetCurrentUserId() : int | Executes the "Get Current User Id" operation, returning `int`. |
| 4 | GetLessonsSummaryByCourse(courseId: int) : Task&lt;ActionResult&gt;List&lt;QuestionBankLessonSummaryResponse&gt;&lt;&gt; | Executes the "Get Lessons Summary By Course" operation asynchronously, returning `Task<ActionResult>List<QuestionBankLessonSummaryResponse><>`. It accepts `courseId: int` as input parameters. |
| 5 | GetQuestionsByLesson(lessonId: int) : Task&lt;ActionResult&gt;List&lt;QuizQuestionResponse&gt;&lt;&gt; | Executes the "Get Questions By Lesson" operation asynchronously, returning `Task<ActionResult>List<QuizQuestionResponse><>`. It accepts `lessonId: int` as input parameters. |
| 6 | UpdateQuestion(questionId: int, request: QuizUpdateQuestionRequest) : Task&lt;ActionResult&gt;QuizQuestionResponse&lt;&gt; | Executes the "Update Question" operation asynchronously, returning `Task<ActionResult>QuizQuestionResponse<>`. It accepts `questionId: int, request: QuizUpdateQuestionRequest` as input parameters. |
| 7 | _questionBankService : IQuestionBankService | Provides the Question Bank Service dependency. |

### 23. QuizAttemptController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAttemptDetail(attemptId: int) : Task&lt;IActionResult&gt; | Executes the "Get Attempt Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `attemptId: int` as input parameters. |
| 2 | GetMyQuizHistory(quizId: int, request: PagedRequestDto) : Task&lt;IActionResult&gt; | Executes the "Get My Quiz History" operation asynchronously, returning `Task<IActionResult>`. It accepts `quizId: int, request: PagedRequestDto` as input parameters. |
| 3 | GetQuizForStudent(quizId: int) : Task&lt;IActionResult&gt; | Executes the "Get Quiz For Student" operation asynchronously, returning `Task<IActionResult>`. It accepts `quizId: int` as input parameters. |
| 4 | SubmitAttempt(request: QuizAttemptSubmitRequest) : Task&lt;IActionResult&gt; | Executes the "Submit Attempt" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: QuizAttemptSubmitRequest` as input parameters. |
| 5 | _quizService : IQuizService | Provides the Quiz Service dependency. |

### 24. QuizController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateQuiz(request: QuizCreateRequest) : Task&lt;IActionResult&gt; | Executes the "Create Quiz" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: QuizCreateRequest` as input parameters. |
| 2 | GetInstructorId() : int | Executes the "Get Instructor Id" operation, returning `int`. |
| 3 | GetMyQuizzes() : Task&lt;IActionResult&gt; | Executes the "Get My Quizzes" operation asynchronously, returning `Task<IActionResult>`. |
| 4 | SoftDelete(id: int) : Task&lt;IActionResult&gt; | Executes the "Soft Delete" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | UpdateQuizSettings(id: int, request: QuizUpdateRequest) : Task&lt;IActionResult&gt; | Executes the "Update Quiz Settings" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, request: QuizUpdateRequest` as input parameters. |
| 6 | _quizService : IQuizService | Provides the Quiz Service dependency. |

### 25. ReportController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetUserId() : int? | Executes the "Get User Id" operation, returning `int?`. |
| 2 | ReportCourse(request: CreateCourseReportRequest) : Task&lt;IActionResult&gt; | Executes the "Report Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CreateCourseReportRequest` as input parameters. |
| 3 | ReportCourseReview(request: CreateCourseReviewReportRequest) : Task&lt;IActionResult&gt; | Executes the "Report Course Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CreateCourseReviewReportRequest` as input parameters. |
| 4 | ReportLessonReview(request: CreateLessonReviewReportRequest) : Task&lt;IActionResult&gt; | Executes the "Report Lesson Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: CreateLessonReviewReportRequest` as input parameters. |
| 5 | _reportService : IReportSubmissionService | Provides the Report Service dependency. |

### 26. ReviewController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteReview(reviewId: int, type: string) : Task&lt;IActionResult&gt; | Executes the "Delete Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `reviewId: int, type: string` as input parameters. |
| 2 | GetCourseReviews(courseId: int, page: int, pageSize: int, starFilter: int?) : Task&lt;IActionResult&gt; | Executes the "Get Course Reviews" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 3 | GetUserId() : int? | Executes the "Get User Id" operation, returning `int?`. |
| 4 | SubmitReview(request: ReviewRequest, source: string) : Task&lt;IActionResult&gt; | Executes the "Submit Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: ReviewRequest, source: string` as input parameters. |
| 5 | _reviewService : IReviewService | Provides the Review Service dependency. |

### 27. TransactionController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetInstructorCourseRevenues(year: int, month: int) : Task&lt;IActionResult&gt; | Executes the "Get Instructor Course Revenues" operation asynchronously, returning `Task<IActionResult>`. It accepts `year: int, month: int` as input parameters. |
| 2 | GetInstructorTransactions(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Get Instructor Transactions" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 3 | GetMyTransactions(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;IActionResult&gt; | Executes the "Get My Transactions" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |
| 4 | GetTransactionDetail(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Transaction Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | GetTransactions(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Get Transactions" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 6 | RequestRefund(transactionId: int, request: StudentRefundRequest) : Task&lt;IActionResult&gt; | Executes the "Request Refund" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, request: StudentRefundRequest` as input parameters. |
| 7 | _financeService : IAdminFinanceService | Provides the Finance Service dependency. |
| 8 | _transactionService : ITransactionService | Provides the Transaction Service dependency. |

### 28. WishlistController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToWishlist(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Add To Wishlist" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 2 | CheckWishlist(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Check Wishlist" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 3 | GetWishlist() : Task&lt;IActionResult&gt; | Executes the "Get Wishlist" operation asynchronously, returning `Task<IActionResult>`. |
| 4 | RemoveFromWishlist(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Remove From Wishlist" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 5 | _wishlistService : IWishlistService | Provides the Wishlist Service dependency. |

## Layer: Dbcontext
### 1. AppDbContext Class
| No | Method / Property | Description |
|---|---|---|
| 1 | OnConfiguring(optionsBuilder: DbContextOptionsBuilder) | Executes the "On Configuring" operation, returning `void`. It accepts `optionsBuilder: DbContextOptionsBuilder` as input parameters. |
| 2 | OnModelCreating(modelBuilder: ModelBuilder) | Executes the "On Model Creating" operation, returning `void`. It accepts `modelBuilder: ModelBuilder` as input parameters. |
| 3 | SaveChangesAsync() : Task&lt;int&gt; | Executes the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 4 | SaveChangesAsync(cancellationToken: CancellationToken) : Task&lt;int&gt; | Executes the "Save Changes" operation asynchronously, returning `Task<int>`. It accepts `cancellationToken: CancellationToken` as input parameters. |

## Layer: Dto
### 1. AccountDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountId : int | Represents the Account ID. |
| 2 | Balance : decimal? | Represents the Balance. |
| 3 | DefaultCurrency : string? | Represents the Default Currency. |
| 4 | IsActive : bool? | Indicates whether it is active. |
| 5 | User : UserDto? | Represents the User. |
| 6 | UserId : int? | Represents the User ID. |

### 2. AccountTransactionSummaryDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | TotalRevenue : decimal | Represents the Total Revenue. |
| 2 | TotalSpent : decimal | Represents the Total Spent. |
| 3 | TotalWithdrawn : decimal | Represents the Total Withdrawn. |

### 3. AddQuizToCourseRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | OrderIndex : int | Represents the Order Index. |
| 3 | QuizId : int | Represents the Quiz ID. |

### 4. AdminAccountDetailDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountId : int | Represents the Account ID. |
| 2 | Email : string | Represents the Email. |
| 3 | Role : string | Represents the Role. |

### 5. AdminAccountListDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountId : int | Represents the Account ID. |
| 2 | AccountStatus : string | Represents the Account Status. |
| 3 | Email : string | Represents the Email. |
| 4 | Role : string | Represents the Role. |
| 5 | Username : string | Represents the Username. |

### 6. AiModelAdminDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelId : int | Represents the Model ID. |
| 3 | ModelName : string | Represents the Model Name. |
| 4 | ModelPath : string | Represents the Model Path. |
| 5 | ModelProvider : string | Represents the Model Provider. |
| 6 | ModelStatus : string | Represents the Model Status. |
| 7 | ModelType : string | Represents the Model Type. |
| 8 | ModelUpdatedAt : DateTime? | Represents the Model Updated At. |
| 9 | ModelVersion : string | Represents the Model Version. |
| 10 | ProcessType : string | Represents the Process Type. |

### 7. AttachmentDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttachmentId : int | Represents the Attachment ID. |
| 2 | FileName : string? | Represents the File Name. |
| 3 | FileSize : long? | Represents the File Size. |
| 4 | FileType : string? | Represents the File Type. |
| 5 | FileUrl : string | Represents the File Url. |

### 8. AttachmentInputDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | FileName : string? | Represents the File Name. |
| 2 | FileSize : long? | Represents the File Size. |
| 3 | FileType : string? | Represents the File Type. |
| 4 | FileUrl : string | Represents the File Url. |

### 9. AttemptAnswerRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | QuestionId : int | Represents the Question ID. |
| 2 | SelectedOptionId : int? | Represents the Selected Option ID. |

### 10. AvailableCouponDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConditionMessage : string | Represents the Condition Message. |
| 2 | CouponCode : string | Represents the Coupon Code. |
| 3 | CouponType : string | Represents the Coupon Type. |
| 4 | CourseId : int? | Represents the Course ID. |
| 5 | DiscountValue : decimal | Represents the Discount Value. |
| 6 | EndDate : DateTime? | Represents the End Date. |
| 7 | IsEligible : bool | Indicates whether it is eligible. |
| 8 | MinOrderValue : decimal | Represents the Min Order Value. |

### 11. BaseAiLogAdminDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ErrorMessage : string? | Represents the Error Message. |
| 2 | InputJson : string? | Represents the Input Json. |
| 3 | LatencyMs : int | Represents the Latency Ms. |
| 4 | LogCreatedAt : DateTime | Represents the Log Created At. |
| 5 | LogId : int | Represents the Log ID. |
| 6 | ModelId : int | Represents the Model ID. |
| 7 | ModelName : string | Represents the Model Name. |
| 8 | OutputJson : string? | Represents the Output Json. |
| 9 | ResultStatus : string? | Represents the Result Status. |

### 12. CartItemDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | DiscountAmount : decimal | Represents the Discount Amount. |
| 4 | DiscountedPrice : decimal | Represents the Discounted Price. |
| 5 | InstructorName : string? | Represents the Instructor Name. |
| 6 | OriginalPrice : decimal | Represents the Original Price. |
| 7 | Price : decimal | Represents the Price. |
| 8 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 9 | Title : string | Represents the Title. |

### 13. CartSummaryResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string | Represents the Applied Coupon Code. |
| 2 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 3 | AvailableCoupons : List&lt;AvailableCouponDto&gt; | Represents the Available Coupons. |
| 4 | CouponMessage : string? | Represents the Coupon Message. |
| 5 | DiscountAmount : decimal | Represents the Discount Amount. |
| 6 | HasDiscount : bool | Indicates whether it has discount. |
| 7 | Items : List&lt;CartItemDto&gt; | Represents the Items. |
| 8 | Items : List&lt;CartItemViewModel&gt; | Represents the Items. |
| 9 | SubTotal : decimal | Represents the Sub Total. |
| 10 | Total : decimal | Represents the Total. |

### 14. CartSummaryResponseDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | AvailableCoupons : List&lt;CouponDto&gt; | Represents the Available Coupons. |
| 3 | DiscountAmount : decimal | Represents the Discount Amount. |
| 4 | Items : List&lt;CartItemDto&gt; | Represents the Items. |
| 5 | Subtotal : decimal | Represents the Subtotal. |
| 6 | TotalAmount : decimal | Represents the Total Amount. |

### 15. ChangePasswordRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmNewPassword : string | Represents the Confirm New Password. |
| 2 | CurrentPassword : string | Represents the Current Password. |
| 3 | NewPassword : string | Represents the New Password. |

### 16. ChatListDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChatId : int | Represents the Chat ID. |
| 2 | ChatName : string? | Represents the Chat Name. |
| 3 | ChatType : string | Represents the Chat Type. |
| 4 | ContextId : int? | Represents the Context ID. |
| 5 | ContextType : string? | Represents the Context Type. |
| 6 | IsOnline : bool | Indicates whether it is online. |
| 7 | LastMessage : string? | Represents the Last Message. |
| 8 | LastMessageAt : DateTime? | Represents the Last Message At. |
| 9 | PartnerAvatar : string? | Represents the Partner Avatar. |
| 10 | PartnerId : int? | Represents the Partner ID. |
| 11 | PartnerName : string? | Represents the Partner Name. |
| 12 | UnreadCount : int | Represents the Unread Count. |

### 17. CheckoutResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ClientSecret : string | Represents the Client Secret. |
| 2 | SessionId : string | Represents the Session ID. |
| 3 | SessionUrl : string | Represents the Session Url. |

### 18. CouponDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string | Represents the Coupon Code. |
| 2 | CouponType : string | Represents the Coupon Type. |
| 3 | DiscountValue : decimal | Represents the Discount Value. |

### 19. CouponResponseDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string | Represents the Coupon Code. |
| 2 | CouponId : int | Represents the Coupon ID. |
| 3 | CouponType : string? | Represents the Coupon Type. |
| 4 | DiscountValue : decimal | Represents the Discount Value. |
| 5 | EndDate : DateTime? | Represents the End Date. |
| 6 | IsActive : bool? | Indicates whether it is active. |
| 7 | ManagerId : int? | Represents the Manager ID. |
| 8 | MinOrderValue : decimal | Represents the Min Order Value. |
| 9 | StartDate : DateTime? | Represents the Start Date. |
| 10 | UsageLimit : int? | Represents the Usage Limit. |
| 11 | UsedCount : int? | Represents the Used Count. |

### 20. CourseCreateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryId : int | Represents the Category ID. |
| 2 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 3 | Description : string? | Represents the Description. |
| 4 | Price : decimal | Represents the Price. |
| 5 | Requirements : string? | Represents the Requirements. |
| 6 | ThumbnailFile : IFormFile? | Represents the Thumbnail File. |
| 7 | Title : string | Represents the Title. |
| 8 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 21. CourseDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | AppliedCouponType : string? | Represents the Applied Coupon Type. |
| 3 | AppliedCouponValue : decimal? | Represents the Applied Coupon Value. |
| 4 | CategoryId : int? | Represents the Category ID. |
| 5 | CategoryName : string? | Represents the Category Name. |
| 6 | CourseId : int | Represents the Course ID. |
| 7 | CourseQuizzes : List&lt;CourseQuizItemResponse&gt; | Represents the Course Quizzes. |
| 8 | CourseStatus : string? | Represents the Course Status. |
| 9 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 10 | CreatedAt : DateTime? | Represents the Created At. |
| 11 | Description : string? | Represents the Description. |
| 12 | InstructorAvatarUrl : string? | Represents the Instructor Avatar Url. |
| 13 | InstructorBio : string? | Represents the Instructor Bio. |
| 14 | InstructorCoursesCount : int | Represents the Instructor Courses Count. |
| 15 | InstructorId : int? | Represents the Instructor ID. |
| 16 | InstructorName : string? | Represents the Instructor Name. |
| 17 | InstructorProfessionalTitle : string? | Represents the Instructor Professional Title. |
| 18 | InstructorReviewCount : int | Represents the Instructor Review Count. |
| 19 | InstructorStudentsCount : int | Represents the Instructor Students Count. |
| 20 | IsEnrolled : bool | Indicates whether it is enrolled. |
| 21 | IsInAnyCart : bool | Indicates whether it is in any cart. |
| 22 | IsOwner : bool | Indicates whether it is owner. |
| 23 | IsRemoved : bool | Indicates whether it is removed. |
| 24 | LastApprovedAt : DateTime? | Represents the Last Approved At. |
| 25 | Lessons : List&lt;LessonResponse&gt; | Represents the Lessons. |
| 26 | Price : decimal | Represents the Price. |
| 27 | RatingAverage : decimal | Represents the Rating Average. |
| 28 | Requirements : string? | Represents the Requirements. |
| 29 | Title : string | Represents the Title. |
| 30 | TotalReviews : int | Represents the Total Reviews. |
| 31 | TotalStudents : int | Represents the Total Students. |
| 32 | UpdatedAt : DateTime? | Represents the Updated At. |
| 33 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 22. CourseDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CourseStatus : string? | Represents the Course Status. |
| 3 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 4 | Price : decimal | Represents the Price. |
| 5 | Title : string | Represents the Title. |

### 23. CourseModerationDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryName : string? | Represents the Category Name. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | CourseStatus : string? | Represents the Course Status. |
| 4 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | FlagCount : int | Represents the Flag Count. |
| 7 | InstructorName : string | Represents the Instructor Name. |
| 8 | IsRemoved : bool | Indicates whether it is removed. |
| 9 | Price : decimal | Represents the Price. |
| 10 | ThreatLevel : AiThreatLevel | Represents the Threat Level. |
| 11 | Title : string | Represents the Title. |
| 12 | UrgencyColor : string | Represents the Urgency Color. |
| 13 | UrgencyLevel : string | Represents the Urgency Level. |

### 24. CourseModerationLogAdminDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | InteractionType : string? | Represents the Interaction Type. |
| 3 | Title : string | Represents the Title. |
| 4 | TokenUsage : int | Represents the Token Usage. |

### 25. CourseModerationRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |

### 26. CourseModerationResult Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | FlaggedFields : List&lt;string&gt; | Represents the Flagged Fields. |
| 3 | ModerationStatus : string | Represents the Moderation Status. |
| 4 | OverallConfidenceScore : float | Represents the Overall Confidence Score. |
| 5 | StageLogs : List&lt;StageLog&gt; | Represents the Stage Logs. |
| 6 | TotalLatencyMs : float | Represents the Total Latency Ms. |

### 27. CourseQuizAddRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | OrderIndex : int | Represents the Order Index. |
| 3 | QuizId : int | Represents the Quiz ID. |

### 28. CourseQuizResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | IsHidden : bool | Indicates whether it is hidden. |
| 3 | OrderIndex : int | Represents the Order Index. |
| 4 | QuizId : int | Represents the Quiz ID. |

### 29. CourseReportDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessGrantedUntil : DateTime? | Represents the Access Granted Until. |
| 2 | CourseFlagCount : int | Represents the Course Flag Count. |
| 3 | CourseId : int? | Represents the Course ID. |
| 4 | CourseTitle : string? | Represents the Course Title. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | Description : string? | Represents the Description. |
| 7 | InstructorEmail : string? | Represents the Instructor Email. |
| 8 | InstructorName : string? | Represents the Instructor Name. |
| 9 | Reason : string? | Represents the Reason. |
| 10 | ReportId : int | Represents the Report ID. |
| 11 | ReporterEmail : string? | Represents the Reporter Email. |
| 12 | ReporterId : int? | Represents the Reporter ID. |
| 13 | ReporterName : string? | Represents the Reporter Name. |
| 14 | ResolutionNote : string? | Represents the Resolution Note. |
| 15 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 16 | ResolverEmail : string? | Represents the Resolver Email. |
| 17 | Status : string? | Represents the Status. |

### 30. CourseResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | AppliedCouponType : string? | Represents the Applied Coupon Type. |
| 3 | AppliedCouponValue : decimal? | Represents the Applied Coupon Value. |
| 4 | CategoryId : int? | Represents the Category ID. |
| 5 | CategoryName : string? | Represents the Category Name. |
| 6 | CourseId : int | Represents the Course ID. |
| 7 | CourseStatus : string? | Represents the Course Status. |
| 8 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 9 | CreatedAt : DateTime? | Represents the Created At. |
| 10 | Description : string? | Represents the Description. |
| 11 | FlagCount : int | Represents the Flag Count. |
| 12 | InstructorAvatarUrl : string? | Represents the Instructor Avatar Url. |
| 13 | InstructorBio : string? | Represents the Instructor Bio. |
| 14 | InstructorCoursesCount : int | Represents the Instructor Courses Count. |
| 15 | InstructorId : int? | Represents the Instructor ID. |
| 16 | InstructorName : string? | Represents the Instructor Name. |
| 17 | InstructorProfessionalTitle : string? | Represents the Instructor Professional Title. |
| 18 | InstructorReviewCount : int | Represents the Instructor Review Count. |
| 19 | InstructorStudentsCount : int | Represents the Instructor Students Count. |
| 20 | IsEnrolled : bool | Indicates whether it is enrolled. |
| 21 | IsInAnyCart : bool | Indicates whether it is in any cart. |
| 22 | IsOwner : bool | Indicates whether it is owner. |
| 23 | IsRemoved : bool | Indicates whether it is removed. |
| 24 | LastApprovedAt : DateTime? | Represents the Last Approved At. |
| 25 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 26 | Price : decimal | Represents the Price. |
| 27 | RatingAverage : decimal | Represents the Rating Average. |
| 28 | Requirements : string? | Represents the Requirements. |
| 29 | Title : string | Represents the Title. |
| 30 | TotalReviews : int | Represents the Total Reviews. |
| 31 | TotalStudents : int | Represents the Total Students. |
| 32 | UpdatedAt : DateTime? | Represents the Updated At. |
| 33 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 31. CourseStats Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | RatingAverage : decimal | Represents the Rating Average. |
| 3 | TotalLessons : int | Represents the Total Lessons. |
| 4 | TotalMaterials : int | Represents the Total Materials. |
| 5 | TotalReviews : int | Represents the Total Reviews. |
| 6 | TotalStudents : int | Represents the Total Students. |
| 7 | TotalVideoDuration : int | Represents the Total Video Duration. |

### 32. CourseUpdateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryId : int | Represents the Category ID. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | Description : string? | Represents the Description. |
| 4 | Price : decimal | Represents the Price. |
| 5 | Requirements : string? | Represents the Requirements. |
| 6 | ThumbnailFile : IFormFile? | Represents the Thumbnail File. |
| 7 | Title : string | Represents the Title. |
| 8 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 33. CreateAiModelRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelName : string | Represents the Model Name. |
| 3 | ModelPath : string | Represents the Model Path. |
| 4 | ModelProvider : string | Represents the Model Provider. |
| 5 | ModelStatus : string | Represents the Model Status. |
| 6 | ModelType : string | Represents the Model Type. |
| 7 | ModelVersion : string | Represents the Model Version. |
| 8 | ProcessType : string | Represents the Process Type. |

### 34. CreateChatDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ContextId : int? | Represents the Context ID. |
| 2 | ContextType : string? | Represents the Context Type. |
| 3 | TargetAccountId : int | Represents the Target Account ID. |

### 35. CreateCouponRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string | Represents the Coupon Code. |
| 2 | CouponType : string | Represents the Coupon Type. |
| 3 | DiscountValue : decimal | Represents the Discount Value. |
| 4 | EndDate : DateTime? | Represents the End Date. |
| 5 | MinOrderValue : decimal | Represents the Min Order Value. |
| 6 | StartDate : DateTime? | Represents the Start Date. |
| 7 | UsageLimit : int? | Represents the Usage Limit. |

### 36. CreateCourseReportRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | Reason : string | Represents the Reason. |

### 37. CreateCourseReviewReportRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseReviewId : int | Represents the Course Review ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | Reason : string | Represents the Reason. |

### 38. CreateLessonReviewReportRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | LessonReviewId : int | Represents the Lesson Review ID. |
| 3 | Reason : string | Represents the Reason. |

### 39. CreateStaffFERequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DisplayName : string | Represents the Display Name. |
| 2 | Email : string | Represents the Email. |
| 3 | Password : string | Represents the Password. |
| 4 | PhoneNumber : string? | Represents the Phone Number. |
| 5 | Username : string | Represents the Username. |

### 40. CreateStaffRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DisplayName : string | Represents the Display Name. |
| 2 | Email : string | Represents the Email. |
| 3 | Password : string | Represents the Password. |
| 4 | PhoneNumber : string? | Represents the Phone Number. |
| 5 | Username : string | Represents the Username. |

### 41. DeleteReviewRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ReviewId : int | Represents the Review ID. |
| 2 | Type : string? | Represents the Type. |

### 42. EnrolledCourseDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | EnrolledAt : DateTime? | Represents the Enrolled At. |
| 3 | InstructorName : string? | Represents the Instructor Name. |
| 4 | IsArchived : bool | Indicates whether it is archived. |
| 5 | LearnedMaterials : int | Represents the Learned Materials. |
| 6 | ProgressPercentage : decimal | Represents the Progress Percentage. |
| 7 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 8 | Title : string | Represents the Title. |
| 9 | TotalMaterials : int | Represents the Total Materials. |

### 43. FinancialSummaryResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CurrentTransferRate : decimal | Represents the Current Transfer Rate. |
| 2 | GrossRevenue : decimal | Represents the Gross Revenue. |
| 3 | MaturedEscrow : decimal | Represents the Matured Escrow. |
| 4 | PendingEscrow : decimal | Represents the Pending Escrow. |
| 5 | PlatformNetProfit : decimal | Represents the Platform Net Profit. |
| 6 | TotalPaidOut : decimal | Represents the Total Paid Out. |
| 7 | TotalRefunded : decimal | Represents the Total Refunded. |
| 8 | TotalTransactions : int | Represents the Total Transactions. |

### 44. FlagAccountRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Reason : string | Represents the Reason. |

### 45. ForgotPasswordRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |

### 46. GiftCheckoutRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CancelUrl : string | Represents the Cancel Url. |
| 2 | CardTheme : string | Represents the Card Theme. |
| 3 | CourseId : int | Represents the Course ID. |
| 4 | GiftMessage : string? | Represents the Gift Message. |
| 5 | RecipientEmail : string | Represents the Recipient Email. |
| 6 | RecipientName : string? | Represents the Recipient Name. |
| 7 | SuccessUrl : string | Represents the Success Url. |

### 47. GiftClaimRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | RedemptionToken : string | Represents the Redemption Token. |

### 48. GoogleLoginRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IdToken : string | Represents the ID Token. |

### 49. InstructorApplicationRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DocumentFile : IFormFile? | Represents the Document File. |
| 2 | DocumentFiles : List&lt;IFormFile&gt;? | Represents the Document Files. |
| 3 | ExpertiseCategories : string | Represents the Expertise Categories. |
| 4 | FacebookUrl : string? | Represents the Facebook Url. |
| 5 | LinkedinUrl : string? | Represents the Linkedin Url. |
| 6 | ProfessionalTitle : string | Represents the Professional Title. |
| 7 | RetainedDocumentUrls : List&lt;string&gt;? | Represents the Retained Document Urls. |
| 8 | StripeCountry : string | Represents the Stripe Country. |
| 9 | YoutubeUrl : string? | Represents the Youtube Url. |

### 50. InstructorApprovalDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApprovalStatus : string? | Represents the Approval Status. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | DocumentUrl : string? | Represents the Document Url. |
| 4 | Email : string | Represents the Email. |
| 5 | ExpertiseCategories : string? | Represents the Expertise Categories. |
| 6 | FacebookUrl : string? | Represents the Facebook Url. |
| 7 | FullName : string | Represents the Full Name. |
| 8 | InstructorId : int | Represents the Instructor ID. |
| 9 | LinkedInUrl : string? | Represents the Linked In Url. |
| 10 | ProfessionalTitle : string? | Represents the Professional Title. |
| 11 | YoutubeUrl : string? | Represents the Youtube Url. |

### 51. InstructorCourseRevenueProjection Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | InstructorId : int | Represents the Instructor ID. |
| 4 | InstructorName : string | Represents the Instructor Name. |
| 5 | LifetimeRevenue : decimal | Represents the Lifetime Revenue. |
| 6 | MonthlyRevenue : decimal | Represents the Monthly Revenue. |
| 7 | PreviousMonthRevenue : decimal | Represents the Previous Month Revenue. |
| 8 | SalesCount : int | Represents the Sales Count. |
| 9 | YearlyRevenue : decimal | Represents the Yearly Revenue. |

### 52. InstructorCourseRevenueResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | InstructorId : int | Represents the Instructor ID. |
| 4 | InstructorName : string | Represents the Instructor Name. |
| 5 | LifetimeRevenue : decimal | Represents the Lifetime Revenue. |
| 6 | MonthlyRevenue : decimal | Represents the Monthly Revenue. |
| 7 | PreviousMonthRevenue : decimal | Represents the Previous Month Revenue. |
| 8 | SalesCount : int | Represents the Sales Count. |
| 9 | TotalRevenue : decimal | Represents the Total Revenue. |
| 10 | YearlyRevenue : decimal | Represents the Yearly Revenue. |

### 53. InstructorDashboardDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ActiveCoursesCount : int | Represents the Active Courses Count. |
| 2 | AverageRating : decimal | Represents the Average Rating. |
| 3 | DraftCoursesCount : int | Represents the Draft Courses Count. |
| 4 | EnrollmentGrowthPercentage : double | Represents the Enrollment Growth Percentage. |
| 5 | InstructorRankPercentage : int | Represents the Instructor Rank Percentage. |
| 6 | PendingCoursesCount : int | Represents the Pending Courses Count. |
| 7 | TotalRevenue : decimal | Represents the Total Revenue. |
| 8 | TotalStudents : int | Represents the Total Students. |

### 54. InstructorPayoutDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | IsPaid : bool | Indicates whether it is paid. |
| 4 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 5 | PayoutDate : DateTime | Represents the Payout Date. |
| 6 | PayoutId : int | Represents the Payout ID. |
| 7 | PayoutStatus : string? | Represents the Payout Status. |
| 8 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 9 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 10 | TotalAmount : decimal | Represents the Total Amount. |

### 55. InstructorPublicProfileDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ActiveCoursesCount : int | Represents the Active Courses Count. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | AverageRating : decimal | Represents the Average Rating. |
| 4 | Bio : string? | Represents the Bio. |
| 5 | Courses : List&lt;InstructorCourseDto&gt; | Represents the Courses. |
| 6 | DocumentUrl : string? | Represents the Document Url. |
| 7 | Email : string? | Represents the Email. |
| 8 | ExpertiseCategories : string? | Represents the Expertise Categories. |
| 9 | FacebookUrl : string? | Represents the Facebook Url. |
| 10 | FirstName : string? | Represents the First Name. |
| 11 | InstructorId : int | Represents the Instructor ID. |
| 12 | LastName : string? | Represents the Last Name. |
| 13 | LinkedinUrl : string? | Represents the Linkedin Url. |
| 14 | ProfessionalTitle : string? | Represents the Professional Title. |
| 15 | TotalReviews : int | Represents the Total Reviews. |
| 16 | TotalStudents : int | Represents the Total Students. |
| 17 | YoutubeUrl : string? | Represents the Youtube Url. |

### 56. LessonCreateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | ThumbnailFile : IFormFile? | Represents the Thumbnail File. |
| 4 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 5 | Title : string | Represents the Title. |

### 57. LessonResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int? | Represents the Course ID. |
| 2 | CourseStatus : string? | Represents the Course Status. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | LearningMaterials : List&lt;MaterialResponse&gt; | Represents the Learning Materials. |
| 6 | LessonId : int | Represents the Lesson ID. |
| 7 | LessonStatus : string? | Represents the Lesson Status. |
| 8 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 9 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 10 | Title : string | Represents the Title. |
| 11 | UpdatedAt : DateTime? | Represents the Updated At. |

### 58. LoginRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Password : string | Represents the Password. |
| 2 | UsernameOrEmail : string | Represents the Username Or Email. |

### 59. LoginResponseDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessToken : string | Represents the Access Token. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | IsVerified : bool | Indicates whether it is verified. |
| 4 | RefreshToken : string | Represents the Refresh Token. |
| 5 | UserId : int | Represents the User ID. |
| 6 | UserName : string | Represents the User Name. |
| 7 | UserRole : string | Represents the User Role. |

### 60. ManagerProfileResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountCreatedAt : DateTime? | Represents the Account Created At. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | Bio : string? | Represents the Bio. |
| 4 | DisplayName : string | Represents the Display Name. |
| 5 | Email : string | Represents the Email. |
| 6 | FullName : string? | Represents the Full Name. |
| 7 | LockoutEnd : DateTime? | Represents the Lockout End. |
| 8 | LockoutStart : DateTime? | Represents the Lockout Start. |
| 9 | ManagerId : int | Represents the Manager ID. |
| 10 | PhoneNumber : string? | Represents the Phone Number. |
| 11 | Role : string? | Represents the Role. |

### 61. MaterialCreateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | MaterialFile : IFormFile? | Represents the Material File. |
| 3 | MaterialMetadata : MaterialMetadata? | Represents the Material Metadata. |
| 4 | MaterialUrl : string? | Represents the Material Url. |
| 5 | Title : string | Represents the Title. |

### 62. MaterialResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseStatus : string? | Represents the Course Status. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Description : string? | Represents the Description. |
| 4 | LearningStatus : string? | Represents the Learning Status. |
| 5 | LessonId : int? | Represents the Lesson ID. |
| 6 | MaterialId : int | Represents the Material ID. |
| 7 | MaterialMetadata : MaterialMetadata? | Represents the Material Metadata. |
| 8 | MaterialUrl : string? | Represents the Material Url. |
| 9 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 10 | Title : string | Represents the Title. |
| 11 | UpdatedAt : DateTime? | Represents the Updated At. |

### 63. MaterialTrashResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CloudPublicId : string? | Represents the Cloud Public ID. |
| 2 | CourseTitle : string? | Represents the Course Title. |
| 3 | DeletedAt : DateTime? | Represents the Deleted At. |
| 4 | FileType : string? | Represents the File Type. |
| 5 | LessonTitle : string? | Represents the Lesson Title. |
| 6 | MaterialId : int | Represents the Material ID. |
| 7 | Title : string | Represents the Title. |

### 64. MaterialUpdateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | Title : string | Represents the Title. |

### 65. MessageDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Attachments : List&lt;AttachmentDto&gt; | Represents the Attachments. |
| 2 | ChatId : int | Represents the Chat ID. |
| 3 | Content : string | Represents the Content. |
| 4 | IsSeen : bool | Indicates whether it is seen. |
| 5 | MessageId : int | Represents the Message ID. |
| 6 | MessageStatus : string | Represents the Message Status. |
| 7 | SenderAvatar : string? | Represents the Sender Avatar. |
| 8 | SenderId : int? | Represents the Sender ID. |
| 9 | SenderName : string | Represents the Sender Name. |
| 10 | SentAt : DateTime? | Represents the Sent At. |

### 66. ModerationFilterDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Category : string? | Represents the Category. |
| 2 | Page : int | Represents the Page. |
| 3 | PageSize : int | Represents the Page Size. |
| 4 | Search : string? | Represents the Search. |
| 5 | SortBy : string? | Represents the Sort By. |
| 6 | Status : string? | Represents the Status. |

### 67. NotificationAdminResponseDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string? | Represents the Content. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | IsRead : bool? | Indicates whether it is read. |
| 4 | LinkAction : string? | Represents the Link Action. |
| 5 | NotificationId : int | Represents the Notification ID. |
| 6 | ReceiverEmail : string? | Represents the Receiver Email. |
| 7 | ReceiverFullName : string? | Represents the Receiver Full Name. |
| 8 | ReceiverId : int? | Represents the Receiver ID. |
| 9 | ReceiverRole : string? | Represents the Receiver Role. |
| 10 | SenderId : int? | Represents the Sender ID. |
| 11 | SenderRole : string? | Represents the Sender Role. |
| 12 | Title : string? | Represents the Title. |

### 68. NotificationAdvancedDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string | Represents the Content. |
| 2 | Emails : List&lt;string&gt;? | Represents the Emails. |
| 3 | LinkAction : string? | Represents the Link Action. |
| 4 | TargetType : string | Represents the Target Type. |
| 5 | Title : string | Represents the Title. |

### 69. NotificationResponseDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string | Represents the Content. |
| 2 | CreatedAt : DateTime | Represents the Created At. |
| 3 | IsRead : bool? | Indicates whether it is read. |
| 4 | LinkAction : string? | Represents the Link Action. |
| 5 | NotificationId : int | Represents the Notification ID. |
| 6 | ReceiverId : int | Represents the Receiver ID. |
| 7 | SenderId : int | Represents the Sender ID. |
| 8 | Title : string | Represents the Title. |

### 70. NotificationSendDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string | Represents the Content. |
| 2 | ReceiverId : int | Represents the Receiver ID. |
| 3 | Title : string | Represents the Title. |

### 71. OrderItemDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponUsed : bool? | Represents the Coupon Used. |
| 2 | Course : CourseDto? | Represents the Course. |
| 3 | CourseId : int? | Represents the Course ID. |
| 4 | DiscountAmount : decimal | Represents the Discount Amount. |
| 5 | Id : int | Represents the ID. |
| 6 | OrderId : int? | Represents the Order ID. |
| 7 | PurchasePrice : decimal | Represents the Purchase Price. |

### 72. PagedReportRequestDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Page : int | Represents the Page. |
| 2 | PageSize : int | Represents the Page Size. |
| 3 | Status : string? | Represents the Status. |

### 73. PagedRequestDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Page : int | Represents the Page. |
| 2 | PageSize : int | Represents the Page Size. |

### 74. PagedResult~ReviewResponse~ Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : List&lt;ReviewResponse&gt; | Represents the Items. |
| 2 | TotalCount : int | Represents the Total Count. |

### 75. PagedResult~TransactionListDto~ Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : IEnumerable&lt;TransactionListDto&gt; | Represents the Items. |
| 2 | Page : int | Represents the Page. |
| 3 | PageSize : int | Represents the Page Size. |
| 4 | TotalCount : int | Represents the Total Count. |
| 5 | TotalPages : int | Represents the Total Pages. |

### 76. PagedResult~Transaction~ Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : List&lt;Transaction&gt; | Represents the Items. |
| 2 | PageCount : int | Represents the Page Count. |
| 3 | TotalCount : int | Represents the Total Count. |

### 77. PayoutDetailProjection Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseTitle : string | Represents the Course Title. |
| 2 | InstructorEmail : string | Represents the Instructor Email. |
| 3 | InstructorName : string | Represents the Instructor Name. |
| 4 | InstructorReceived : decimal | Represents the Instructor Received. |
| 5 | IsPaid : bool | Indicates whether it is paid. |
| 6 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 7 | PayoutDate : DateTime | Represents the Payout Date. |
| 8 | PayoutId : int | Represents the Payout ID. |
| 9 | PayoutStatus : string? | Represents the Payout Status. |
| 10 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 11 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 12 | TotalAmount : decimal | Represents the Total Amount. |
| 13 | TransactionDate : DateTime? | Represents the Transaction Date. |
| 14 | TransactionId : int | Represents the Transaction ID. |
| 15 | TransferRate : decimal | Represents the Transfer Rate. |

### 78. PayoutDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseTitle : string | Represents the Course Title. |
| 2 | InstructorEmail : string | Represents the Instructor Email. |
| 3 | InstructorName : string | Represents the Instructor Name. |
| 4 | InstructorReceived : decimal | Represents the Instructor Received. |
| 5 | IsPaid : bool | Indicates whether it is paid. |
| 6 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 7 | PayoutDate : DateTime | Represents the Payout Date. |
| 8 | PayoutId : int | Represents the Payout ID. |
| 9 | PayoutStatus : string | Represents the Payout Status. |
| 10 | PlatformReceived : decimal | Represents the Platform Received. |
| 11 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 12 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 13 | TotalAmount : decimal | Represents the Total Amount. |
| 14 | TransactionDate : DateTime? | Represents the Transaction Date. |
| 15 | TransactionId : int | Represents the Transaction ID. |
| 16 | TransferRate : decimal | Represents the Transfer Rate. |

### 79. PlatformBalanceResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Available : decimal | Represents the Available. |
| 2 | AvailableBalance : decimal | Represents the Available Balance. |
| 3 | Currency : string | Represents the Currency. |
| 4 | Incoming : decimal | Represents the Incoming. |
| 5 | PayoutScheduleAnchor : string? | Represents the Payout Schedule Anchor. |
| 6 | PayoutScheduleInterval : string | Represents the Payout Schedule Interval. |
| 7 | PendingBalance : decimal | Represents the Pending Balance. |
| 8 | Total : decimal | Represents the Total. |

### 80. ProgressResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CompletedMaterialIds : List&lt;int&gt; | Represents the Completed Material Ids. |
| 2 | EnrollmentId : int | Represents the Enrollment ID. |
| 3 | LearnedMaterialCount : int | Represents the Learned Material Count. |
| 4 | TotalMaterialCount : int | Represents the Total Material Count. |

### 81. QuestionBankLessonSummaryResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvailableQuestionCount : int | Represents the Available Question Count. |
| 2 | LessonId : int | Represents the Lesson ID. |
| 3 | Title : string | Represents the Title. |

### 82. QuizAddQuestionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Explanation : string? | Represents the Explanation. |
| 2 | LessonId : int | Represents the Lesson ID. |
| 3 | Options : List&lt;QuizOptionRequest&gt; | Represents the Options. |
| 4 | QuestionText : string | Represents the Question Text. |
| 5 | QuestionType : QuizQuestionType | Represents the Question Type. |

### 83. QuizAttemptDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttemptId : int | Represents the Attempt ID. |
| 2 | CorrectCount : int | Represents the Correct Count. |
| 3 | IsPassed : bool | Indicates whether it is passed. |
| 4 | Questions : List&lt;QuizAttemptQuestionDetailResponse&gt; | Represents the Questions. |
| 5 | QuizId : int | Represents the Quiz ID. |
| 6 | QuizTitle : string | Represents the Quiz Title. |
| 7 | Score : int | Represents the Score. |
| 8 | SubmittedAt : DateTime? | Represents the Submitted At. |
| 9 | TotalQuestions : int | Represents the Total Questions. |

### 84. QuizAttemptQuestionDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Explanation : string? | Represents the Explanation. |
| 2 | IsCorrect : bool | Indicates whether it is correct. |
| 3 | Options : List&lt;QuizOptionDetailResponse&gt; | Represents the Options. |
| 4 | QuestionId : int | Represents the Question ID. |
| 5 | QuestionText : string | Represents the Question Text. |
| 6 | QuestionType : QuizQuestionType | Represents the Question Type. |
| 7 | SelectedOptionId : int? | Represents the Selected Option ID. |

### 85. QuizAttemptResultResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttemptId : int | Represents the Attempt ID. |
| 2 | CorrectCount : int | Represents the Correct Count. |
| 3 | IsPassed : bool | Indicates whether it is passed. |
| 4 | Score : int | Represents the Score. |
| 5 | SubmittedAt : DateTime? | Represents the Submitted At. |
| 6 | TotalQuestions : int | Represents the Total Questions. |

### 86. QuizAttemptSubmitRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Answers : List&lt;AttemptAnswerRequest&gt; | Represents the Answers. |
| 2 | AttemptId : int | Represents the Attempt ID. |

### 87. QuizAttemptSummaryResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttemptId : int | Represents the Attempt ID. |
| 2 | IsPassed : bool | Indicates whether it is passed. |
| 3 | QuizId : int | Represents the Quiz ID. |
| 4 | QuizTitle : string | Represents the Quiz Title. |
| 5 | Score : int | Represents the Score. |
| 6 | StartedAt : DateTime? | Represents the Started At. |
| 7 | SubmittedAt : DateTime? | Represents the Submitted At. |
| 8 | UserFullName : string | Represents the User Full Name. |
| 9 | UserId : int | Represents the User ID. |

### 88. QuizCreateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | PassingScore : int | Represents the Passing Score. |
| 4 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 5 | Title : string | Represents the Title. |
| 6 | TotalQuestions : int | Represents the Total Questions. |

### 89. QuizDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | IsHidden : bool | Indicates whether it is hidden. |
| 6 | IsRemoved : bool | Indicates whether it is removed. |
| 7 | PassingScore : int | Represents the Passing Score. |
| 8 | QuestionCount : int | Represents the Question Count. |
| 9 | QuizId : int | Represents the Quiz ID. |
| 10 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 11 | Title : string | Represents the Title. |
| 12 | TotalQuestions : int | Represents the Total Questions. |
| 13 | UpdatedAt : DateTime? | Represents the Updated At. |

### 90. QuizForStudentResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttemptId : int | Represents the Attempt ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | PassingScore : int | Represents the Passing Score. |
| 4 | Questions : List&lt;QuizQuestionForStudentResponse&gt; | Represents the Questions. |
| 5 | QuizId : int | Represents the Quiz ID. |
| 6 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 7 | Title : string | Represents the Title. |

### 91. QuizLessonDistributionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | LessonId : int | Represents the Lesson ID. |
| 2 | QuestionCount : int | Represents the Question Count. |

### 92. QuizLessonDistributionResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | LessonId : int | Represents the Lesson ID. |
| 2 | QuestionCount : int | Represents the Question Count. |

### 93. QuizOptionDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsCorrect : bool | Indicates whether it is correct. |
| 2 | OptionId : int | Represents the Option ID. |
| 3 | OptionText : string | Represents the Option Text. |
| 4 | OrderIndex : int | Represents the Order Index. |

### 94. QuizOptionForStudentResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | OptionId : int | Represents the Option ID. |
| 2 | OptionText : string | Represents the Option Text. |
| 3 | OrderIndex : int | Represents the Order Index. |

### 95. QuizOptionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsCorrect : bool | Indicates whether it is correct. |
| 2 | OptionText : string | Represents the Option Text. |

### 96. QuizOptionResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsCorrect : bool | Indicates whether it is correct. |
| 2 | OptionId : int | Represents the Option ID. |
| 3 | OptionText : string | Represents the Option Text. |
| 4 | OrderIndex : int | Represents the Order Index. |

### 97. QuizQuestionForStudentResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Options : List&lt;QuizOptionForStudentResponse&gt; | Represents the Options. |
| 2 | QuestionId : int | Represents the Question ID. |
| 3 | QuestionText : string | Represents the Question Text. |
| 4 | QuestionType : QuizQuestionType | Represents the Question Type. |

### 98. QuizQuestionResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Explanation : string? | Represents the Explanation. |
| 3 | LessonId : int? | Represents the Lesson ID. |
| 4 | Options : List&lt;QuizOptionResponse&gt; | Represents the Options. |
| 5 | QuestionId : int | Represents the Question ID. |
| 6 | QuestionText : string | Represents the Question Text. |
| 7 | QuestionType : QuizQuestionType | Represents the Question Type. |

### 99. QuizSummaryResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | IsHidden : bool | Indicates whether it is hidden. |
| 6 | IsRemoved : bool | Indicates whether it is removed. |
| 7 | PassingScore : int | Represents the Passing Score. |
| 8 | QuestionCount : int | Represents the Question Count. |
| 9 | QuizId : int | Represents the Quiz ID. |
| 10 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 11 | Title : string | Represents the Title. |
| 12 | TotalQuestions : int | Represents the Total Questions. |
| 13 | UpdatedAt : DateTime? | Represents the Updated At. |

### 100. QuizUpdateOptionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsCorrect : bool | Indicates whether it is correct. |
| 2 | OptionId : int? | Represents the Option ID. |
| 3 | OptionText : string | Represents the Option Text. |

### 101. QuizUpdateQuestionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Explanation : string? | Represents the Explanation. |
| 2 | LessonId : int | Represents the Lesson ID. |
| 3 | Options : List&lt;QuizUpdateOptionRequest&gt; | Represents the Options. |
| 4 | QuestionText : string | Represents the Question Text. |
| 5 | QuestionType : QuizQuestionType | Represents the Question Type. |

### 102. QuizUpdateRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | Distributions : List&lt;QuizLessonDistributionRequest&gt; | Represents the Distributions. |
| 3 | PassingScore : int | Represents the Passing Score. |
| 4 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 5 | Title : string | Represents the Title. |
| 6 | TotalQuestions : int | Represents the Total Questions. |

### 103. RefundDecisionRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AdminNote : string? | Represents the Admin Note. |

### 104. RefundEligibilityDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountFlagCount : int | Represents the Account Flag Count. |
| 2 | CompletedVideoDurationHours : double | Represents the Completed Video Duration Hours. |
| 3 | CourseTotalDurationHours : double | Represents the Course Total Duration Hours. |
| 4 | PastRefundedCountForCourse : int | Represents the Past Refunded Count For Course. |
| 5 | RefundRequestsLast14DaysCount : int | Represents the Refund Requests Last14days Count. |
| 6 | StudentProgressPercentage : double | Represents the Student Progress Percentage. |

### 105. RefundResultDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsAutoRejected : bool | Indicates whether it is auto rejected. |
| 2 | RejectReason : string? | Represents the Reject Reason. |

### 106. RefundResultResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | EnrollmentRevoked : bool | Represents the Enrollment Revoked. |
| 2 | RefundedAmount : decimal | Represents the Refunded Amount. |
| 3 | StripeRefundId : string | Represents the Stripe Refund ID. |
| 4 | StripeReversalId : string? | Represents the Stripe Reversal ID. |
| 5 | TransactionStatus : string | Represents the Transaction Status. |

### 107. RegisterRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmPassword : string | Represents the Confirm Password. |
| 2 | Email : string | Represents the Email. |
| 3 | FullName : string | Represents the Full Name. |
| 4 | Password : string | Represents the Password. |
| 5 | Username : string | Represents the Username. |

### 108. RejectCourseDetailedRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Items : List&lt;RejectCourseItemDto&gt; | Represents the Items. |

### 109. RejectCourseItemDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | LessonId : int? | Represents the Lesson ID. |
| 2 | LessonTitle : string? | Represents the Lesson Title. |
| 3 | MaterialId : int? | Represents the Material ID. |
| 4 | MaterialTitle : string? | Represents the Material Title. |
| 5 | Reason : string | Represents the Reason. |
| 6 | Target : string | Represents the Target. |

### 110. ReportStatsResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | TotalPending : int | Represents the Total Pending. |
| 2 | TotalPendingCourseReports : int | Represents the Total Pending Course Reports. |
| 3 | TotalPendingCourseReviewReports : int | Represents the Total Pending Course Review Reports. |
| 4 | TotalPendingLessonReviewReports : int | Represents the Total Pending Lesson Review Reports. |
| 5 | TotalRejectedToday : int | Represents the Total Rejected Today. |
| 6 | TotalResolvedToday : int | Represents the Total Resolved Today. |

### 111. ResetPasswordRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmPassword : string | Represents the Confirm Password. |
| 2 | Email : string | Represents the Email. |
| 3 | NewPassword : string | Represents the New Password. |
| 4 | OtpCode : string | Represents the Otp Code. |

### 112. ResolveReportRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | RemoveContent : bool | Represents the Remove Content. |
| 2 | ResolutionNote : string? | Represents the Resolution Note. |
| 3 | Status : string | Represents the Status. |

### 113. ReviewModerationLogAdminDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Comment : string | Represents the Comment. |
| 2 | ReviewId : int | Represents the Review ID. |

### 114. ReviewReportDetailResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int? | Represents the Course ID. |
| 2 | CourseTitle : string? | Represents the Course Title. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | LessonId : int? | Represents the Lesson ID. |
| 6 | LessonTitle : string? | Represents the Lesson Title. |
| 7 | Reason : string? | Represents the Reason. |
| 8 | ReportId : int | Represents the Report ID. |
| 9 | ReporterEmail : string? | Represents the Reporter Email. |
| 10 | ReporterId : int? | Represents the Reporter ID. |
| 11 | ReporterName : string? | Represents the Reporter Name. |
| 12 | ResolutionNote : string? | Represents the Resolution Note. |
| 13 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 14 | ResolverEmail : string? | Represents the Resolver Email. |
| 15 | ReviewAuthorEmail : string? | Represents the Review Author Email. |
| 16 | ReviewAuthorName : string? | Represents the Review Author Name. |
| 17 | ReviewComment : string? | Represents the Review Comment. |
| 18 | ReviewId : int? | Represents the Review ID. |
| 19 | ReviewRating : float? | Represents the Review Rating. |
| 20 | ReviewType : string | Represents the Review Type. |
| 21 | Status : string? | Represents the Status. |

### 115. ReviewRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Comment : string? | Represents the Comment. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | LessonId : int? | Represents the Lesson ID. |
| 4 | Rating : int | Represents the Rating. |

### 116. ReviewResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Comment : string? | Represents the Comment. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | IsInstructor : bool | Indicates whether it is instructor. |
| 4 | IsRemoved : bool | Indicates whether it is removed. |
| 5 | LessonId : int? | Represents the Lesson ID. |
| 6 | LessonTitle : string? | Represents the Lesson Title. |
| 7 | Rating : int | Represents the Rating. |
| 8 | ReviewId : int | Represents the Review ID. |
| 9 | UpdatedAt : DateTime? | Represents the Updated At. |
| 10 | UserAvatarUrl : string? | Represents the User Avatar Url. |
| 11 | UserFullName : string? | Represents the User Full Name. |
| 12 | UserId : int | Represents the User ID. |

### 117. SendMessageDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Attachments : List&lt;AttachmentInputDto&gt;? | Represents the Attachments. |
| 2 | ChatId : int | Represents the Chat ID. |
| 3 | Content : string | Represents the Content. |

### 118. StripeAccountStatusDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChargesEnabled : bool | Represents the Charges Enabled. |
| 2 | DetailsSubmitted : bool | Represents the Details Submitted. |
| 3 | PayoutsEnabled : bool | Represents the Payouts Enabled. |

### 119. StripeConnectSetupResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | OnboardingUrl : string | Represents the Onboarding Url. |
| 2 | StripeAccountId : string | Represents the Stripe Account ID. |

### 120. StripePlatformBalanceDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvailableBalance : decimal | Represents the Available Balance. |
| 2 | Currency : string? | Represents the Currency. |
| 3 | PendingBalance : decimal | Represents the Pending Balance. |

### 121. StripeSetupResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | OnboardingUrl : string | Represents the Onboarding Url. |
| 2 | StripeAccountId : string | Represents the Stripe Account ID. |

### 122. StripeWithdrawalResponseDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Status : string? | Represents the Status. |
| 4 | StripePayoutId : string | Represents the Stripe Payout ID. |

### 123. StudentRefundRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Reason : string | Represents the Reason. |

### 124. SupportRequestDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string | Represents the Content. |
| 2 | TargetRole : string | Represents the Target Role. |

### 125. SupportTicketDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | InitialMessage : string | Represents the Initial Message. |
| 2 | RequestedAt : DateTime | Represents the Requested At. |
| 3 | SenderAvatar : string? | Represents the Sender Avatar. |
| 4 | SenderId : int | Represents the Sender ID. |
| 5 | SenderName : string | Represents the Sender Name. |
| 6 | TargetRole : string | Represents the Target Role. |
| 7 | TicketId : string | Represents the Ticket ID. |

### 126. TransactionDetailDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | BuyerEmail : string | Represents the Buyer Email. |
| 2 | BuyerName : string | Represents the Buyer Name. |
| 3 | CouponCode : string? | Represents the Coupon Code. |
| 4 | CouponDiscountValue : decimal? | Represents the Coupon Discount Value. |
| 5 | CouponType : string? | Represents the Coupon Type. |
| 6 | CouponUsed : bool | Represents the Coupon Used. |
| 7 | CourseThumbnail : string? | Represents the Course Thumbnail. |
| 8 | CourseTitle : string | Represents the Course Title. |
| 9 | Currency : string? | Represents the Currency. |
| 10 | Date : DateTime? | Represents the Date. |
| 11 | DiscountAmount : decimal | Represents the Discount Amount. |
| 12 | GrossAmount : decimal | Represents the Gross Amount. |
| 13 | InstructorEmail : string | Represents the Instructor Email. |
| 14 | InstructorName : string | Represents the Instructor Name. |
| 15 | InstructorPayout : decimal | Represents the Instructor Payout. |
| 16 | IsPaid : bool | Indicates whether it is paid. |
| 17 | OriginalPrice : decimal | Represents the Original Price. |
| 18 | PlatformProfit : decimal | Represents the Platform Profit. |
| 19 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 20 | RefundReason : string? | Represents the Refund Reason. |
| 21 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 22 | Status : string? | Represents the Status. |
| 23 | StripePaymentIntentId : string? | Represents the Stripe Payment Intent ID. |
| 24 | StripeSessionId : string? | Represents the Stripe Session ID. |
| 25 | TransactionId : int | Represents the Transaction ID. |
| 26 | TransferRate : decimal | Represents the Transfer Rate. |

### 127. TransactionExtDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | PaymentMethod : string? | Represents the Payment Method. |
| 3 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 4 | RefundReason : string? | Represents the Refund Reason. |
| 5 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 6 | TransactionId : int | Represents the Transaction ID. |

### 128. TransactionListDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | BuyerName : string | Represents the Buyer Name. |
| 3 | CourseTitle : string | Represents the Course Title. |
| 4 | CreatedAt : DateTime? | Represents the Created At. |
| 5 | Currency : string? | Represents the Currency. |
| 6 | Date : DateTime? | Represents the Date. |
| 7 | InstructorName : string | Represents the Instructor Name. |
| 8 | IsCourseOwner : bool | Indicates whether it is course owner. |
| 9 | IsGift : bool | Indicates whether it is gift. |
| 10 | IsGiftClaimed : bool | Indicates whether it is gift claimed. |
| 11 | PaymentMethod : string? | Represents the Payment Method. |
| 12 | PayoutCurrency : string? | Represents the Payout Currency. |
| 13 | RecipientEmail : string? | Represents the Recipient Email. |
| 14 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 15 | RefundReason : string? | Represents the Refund Reason. |
| 16 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 17 | Status : string? | Represents the Status. |
| 18 | StripeSessionId : string? | Represents the Stripe Session ID. |
| 19 | TransactionExt : TransactionExtDto? | Represents the Transaction Ext. |
| 20 | TransactionId : int | Represents the Transaction ID. |
| 21 | UserEmail : string? | Represents the User Email. |
| 22 | UserName : string? | Represents the User Name. |

### 129. UpdateAiModelRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelPath : string | Represents the Model Path. |
| 3 | ModelProvider : string | Represents the Model Provider. |
| 4 | ModelVersion : string | Represents the Model Version. |

### 130. UpdateApprovalStatusDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | InstructorId : int | Represents the Instructor ID. |
| 2 | Reason : string? | Represents the Reason. |
| 3 | Status : string | Represents the Status. |

### 131. UpdateCouponRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | EndDate : DateTime? | Represents the End Date. |
| 2 | IsActive : bool? | Indicates whether it is active. |
| 3 | UsageLimit : int? | Represents the Usage Limit. |

### 132. UpdateManagerProfileRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvatarFile : IFormFile? | Represents the Avatar File. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | DisplayName : string | Represents the Display Name. |
| 4 | FullName : string? | Represents the Full Name. |
| 5 | PhoneNumber : string? | Represents the Phone Number. |

### 133. UpdateProfileRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvatarFile : IFormFile? | Represents the Avatar File. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | DateOfBirth : DateTime? | Represents the Date Of Birth. |
| 4 | Email : string | Represents the Email. |
| 5 | FullName : string | Represents the Full Name. |
| 6 | PhoneNumber : string? | Represents the Phone Number. |

### 134. UpdateStaffRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountStatus : string | Represents the Account Status. |
| 2 | DisplayName : string | Represents the Display Name. |
| 3 | Password : string? | Represents the Password. |
| 4 | PhoneNumber : string? | Represents the Phone Number. |

### 135. UpdateStatusRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Status : string | Represents the Status. |

### 136. UpdateThresholdsRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | SimilarityScoreThreshold : float | Represents the Similarity Score Threshold. |
| 2 | SpamConfidenceThreshold : float | Represents the Spam Confidence Threshold. |
| 3 | ToxicityConfidenceThreshold : float | Represents the Toxicity Confidence Threshold. |

### 137. UserDto Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Bio : string? | Represents the Bio. |
| 2 | DateOfBirth : DateOnly? | Represents the Date Of Birth. |
| 3 | FullName : string | Represents the Full Name. |
| 4 | UserId : int | Represents the User ID. |

### 138. UserProfileResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvatarUrl : string? | Represents the Avatar Url. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | DateOfBirth : DateTime? | Represents the Date Of Birth. |
| 4 | Email : string | Represents the Email. |
| 5 | FullName : string? | Represents the Full Name. |
| 6 | IsVerified : bool | Indicates whether it is verified. |
| 7 | PhoneNumber : string? | Represents the Phone Number. |
| 8 | Username : string | Represents the Username. |

### 139. UserProfileResponseDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Bio : string? | Represents the Bio. |
| 2 | DateOfBirth : DateTime? | Represents the Date Of Birth. |
| 3 | Email : string | Represents the Email. |
| 4 | FullName : string? | Represents the Full Name. |
| 5 | IsVerified : bool | Indicates whether it is verified. |
| 6 | PhoneNumber : string? | Represents the Phone Number. |
| 7 | Username : string | Represents the Username. |

### 140. VerifyEmailRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |
| 2 | Otp : string | Represents the Otp. |

### 141. VerifyOtpRequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |
| 2 | Otp : string | Represents the Otp. |
| 3 | OtpCode : string | Represents the Otp Code. |

### 142. WishlistResponseDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AverageRating : double | Represents the Average Rating. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | EnrollmentCount : int | Represents the Enrollment Count. |
| 4 | InstructorName : string | Represents the Instructor Name. |
| 5 | IsEnrolled : bool | Indicates whether it is enrolled. |
| 6 | Price : decimal | Represents the Price. |
| 7 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 8 | Title : string | Represents the Title. |

### 143. WithdrawRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal? | Represents the Amount. |
| 2 | Description : string? | Represents the Description. |

### 144. WithdrawResponse Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | CreatedAt : DateTime | Represents the Created At. |
| 3 | Status : string | Represents the Status. |
| 4 | StripePayoutId : string | Represents the Stripe Payout ID. |
| 5 | WithdrawalId : int | Represents the Withdrawal ID. |

### 145. WithdrawalHistoryItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | ArrivedAt : DateTime? | Represents the Arrived At. |
| 3 | CreatedAt : DateTime | Represents the Created At. |
| 4 | Currency : string | Represents the Currency. |
| 5 | Description : string? | Represents the Description. |
| 6 | ManagerName : string? | Represents the Manager Name. |
| 7 | Status : string | Represents the Status. |
| 8 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 9 | WithdrawalId : int | Represents the Withdrawal ID. |

## Layer: Entity
### 1. Account Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountCreatedAt : DateTime? | Represents the Account Created At. |
| 2 | AccountFlagCount : int? | Represents the Account Flag Count. |
| 3 | AccountId : int | Represents the Account ID. |
| 4 | AccountLastLoginAt : DateTime? | Represents the Account Last Login At. |
| 5 | AccountStatus : string? | Represents the Account Status. |
| 6 | AccountUpdatedAt : DateTime? | Represents the Account Updated At. |
| 7 | AuthProvider : string? | Represents the Auth Provider. |
| 8 | AvatarUrl : string? | Represents the Avatar Url. |
| 9 | Balance : decimal? | Represents the Balance. |
| 10 | ChatParticipants : ICollection&lt;ChatParticipant&gt; | Represents the Chat Participants. |
| 11 | CreatedAt : DateTime? | Represents the Created At. |
| 12 | DefaultCurrency : string? | Represents the Default Currency. |
| 13 | Email : string | Represents the Email. |
| 14 | FlagCount : int | Represents the Flag Count. |
| 15 | InstructorPayouts : ICollection&lt;InstructorPayout&gt; | Represents the Instructor Payouts. |
| 16 | IsActive : bool? | Indicates whether it is active. |
| 17 | IsVerified : bool | Indicates whether it is verified. |
| 18 | LastLoginDate : DateTime? | Represents the Last Login Date. |
| 19 | Lockouts : ICollection&lt;Lockout&gt; | Represents the Lockouts. |
| 20 | Manager : Manager? | Represents the Manager. |
| 21 | Messages : ICollection&lt;Message&gt; | Represents the Messages. |
| 22 | NotificationReceivers : ICollection&lt;Notification&gt; | Represents the Notification Receivers. |
| 23 | NotificationSenders : ICollection&lt;Notification&gt; | Represents the Notification Senders. |
| 24 | PasswordHash : string? | Represents the Password Hash. |
| 25 | PhoneNumber : string? | Represents the Phone Number. |
| 26 | RefreshToken : string? | Represents the Refresh Token. |
| 27 | RefreshTokenExpiryTime : DateTime? | Represents the Refresh Token Expiry Time. |
| 28 | SystemConfigs : ICollection&lt;SystemConfig&gt; | Represents the System Configs. |
| 29 | TransactionAccountFromNavigations : ICollection&lt;Transaction&gt; | Represents the Transaction Account From Navigations. |
| 30 | TransactionAccountToNavigations : ICollection&lt;Transaction&gt; | Represents the Transaction Account To Navigations. |
| 31 | TransactionFroms : ICollection&lt;Transaction&gt; | Represents the Transaction Froms. |
| 32 | TransactionTos : ICollection&lt;Transaction&gt; | Represents the Transaction Tos. |
| 33 | User : User? | Represents the User. |
| 34 | UserId : int? | Represents the User ID. |
| 35 | UserReportReporters : ICollection&lt;UserReport&gt; | Represents the User Report Reporters. |
| 36 | UserReportResolvers : ICollection&lt;UserReport&gt; | Represents the User Report Resolvers. |
| 37 | UserReportTargets : ICollection&lt;UserReport&gt; | Represents the User Report Targets. |
| 38 | Username : string? | Represents the Username. |

### 2. AiModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseAiIntegrations : ICollection&lt;CourseAiIntegration&gt; | Represents the Course Ai Integrations. |
| 2 | CourseReviewModerationLogs : ICollection&lt;CourseReviewModerationLog&gt; | Represents the Course Review Moderation Logs. |
| 3 | Description : string? | Represents the Description. |
| 4 | LessonReviewModerationLogs : ICollection&lt;LessonReviewModerationLog&gt; | Represents the Lesson Review Moderation Logs. |
| 5 | MessageModerationLogs : ICollection&lt;MessageModerationLog&gt; | Represents the Message Moderation Logs. |
| 6 | ModelCreatedAt : DateTime? | Represents the Model Created At. |
| 7 | ModelId : int | Represents the Model ID. |
| 8 | ModelName : string | Represents the Model Name. |
| 9 | ModelPath : string? | Represents the Model Path. |
| 10 | ModelProvider : string? | Represents the Model Provider. |
| 11 | ModelStatus : string? | Represents the Model Status. |
| 12 | ModelType : string? | Represents the Model Type. |
| 13 | ModelUpdatedAt : DateTime? | Represents the Model Updated At. |
| 14 | ModelVersion : string? | Represents the Model Version. |
| 15 | ProcessType : string? | Represents the Process Type. |

### 3. CartItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddedDate : DateTime? | Represents the Added Date. |
| 2 | CartItemId : int | Represents the Cart Item ID. |
| 3 | Course : Course? | Represents the Course. |
| 4 | CourseId : int? | Represents the Course ID. |
| 5 | Id : int | Represents the ID. |
| 6 | Price : decimal? | Represents the Price. |
| 7 | User : User? | Represents the User. |
| 8 | UserId : int? | Represents the User ID. |

### 4. Category Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoriesName : string | Represents the Categories Name. |
| 2 | CategoryId : int | Represents the Category ID. |
| 3 | CategoryStatus : string? | Represents the Category Status. |
| 4 | CreatedAt : DateTime? | Represents the Created At. |
| 5 | Description : string? | Represents the Description. |
| 6 | UpdatedAt : DateTime? | Represents the Updated At. |

### 5. Chat Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChatId : int | Represents the Chat ID. |
| 2 | ChatName : string? | Represents the Chat Name. |
| 3 | ChatType : string | Represents the Chat Type. |
| 4 | ChatType : string? | Represents the Chat Type. |
| 5 | ContextId : int? | Represents the Context ID. |
| 6 | ContextType : string? | Represents the Context Type. |
| 7 | CreatedAt : DateTime? | Represents the Created At. |
| 8 | LastMessageAt : DateTime? | Represents the Last Message At. |

### 6. ChatParticipant Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountId : int | Represents the Account ID. |
| 2 | ChatId : int | Represents the Chat ID. |
| 3 | ClearedAt : DateTime? | Represents the Cleared At. |
| 4 | JoinedAt : DateTime? | Represents the Joined At. |
| 5 | LastReadAt : DateTime? | Represents the Last Read At. |
| 6 | Role : string | Represents the Role. |
| 7 | UnreadCount : int | Represents the Unread Count. |

### 7. Coupon Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string | Represents the Coupon Code. |
| 2 | CouponId : int | Represents the Coupon ID. |
| 3 | CouponType : string? | Represents the Coupon Type. |
| 4 | Courses : ICollection&lt;Course&gt; | Represents the Courses. |
| 5 | DiscountValue : decimal | Represents the Discount Value. |
| 6 | EndDate : DateTime? | Represents the End Date. |
| 7 | IsActive : bool? | Indicates whether it is active. |
| 8 | Manager : Manager? | Represents the Manager. |
| 9 | ManagerId : int? | Represents the Manager ID. |
| 10 | MinOrderValue : decimal | Represents the Min Order Value. |
| 11 | StartDate : DateTime? | Represents the Start Date. |
| 12 | UsageLimit : int? | Represents the Usage Limit. |
| 13 | UsedCount : int? | Represents the Used Count. |

### 8. Course Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CartItems : ICollection&lt;CartItem&gt; | Represents the Cart Items. |
| 2 | Category : Category? | Represents the Category. |
| 3 | CategoryId : int? | Represents the Category ID. |
| 4 | Coupon : Coupon? | Represents the Coupon. |
| 5 | CouponId : int? | Represents the Coupon ID. |
| 6 | CourseAiIntegrations : ICollection&lt;CourseAiIntegration&gt; | Represents the Course Ai Integrations. |
| 7 | CourseExt : CourseExt? | Represents the Course Ext. |
| 8 | CourseFlagCount : int? | Represents the Course Flag Count. |
| 9 | CourseId : int | Represents the Course ID. |
| 10 | CourseQuizzes : ICollection&lt;CourseQuiz&gt; | Represents the Course Quizzes. |
| 11 | CourseStatus : string? | Represents the Course Status. |
| 12 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 13 | CreatedAt : DateTime? | Represents the Created At. |
| 14 | Description : string? | Represents the Description. |
| 15 | Enrollments : ICollection&lt;Enrollment&gt; | Represents the Enrollments. |
| 16 | FieldModerationFeedbacks : ICollection&lt;CourseFieldModerationFeedback&gt; | Represents the Field Moderation Feedbacks. |
| 17 | Instructor : Instructor? | Represents the Instructor. |
| 18 | InstructorId : int? | Represents the Instructor ID. |
| 19 | IsRemoved : bool | Indicates whether it is removed. |
| 20 | LastApprovedAt : DateTime? | Represents the Last Approved At. |
| 21 | Lessons : ICollection&lt;Lesson&gt; | Represents the Lessons. |
| 22 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 23 | OrderItems : ICollection&lt;OrderItem&gt; | Represents the Order Items. |
| 24 | Price : decimal | Represents the Price. |
| 25 | Requirements : string? | Represents the Requirements. |
| 26 | Status : string | Represents the Status. |
| 27 | ThreatLevel : AiThreatLevel | Represents the Threat Level. |
| 28 | Title : string | Represents the Title. |
| 29 | UpdatedAt : DateTime? | Represents the Updated At. |
| 30 | WhatYouWillLearn : string? | Represents the What You Will Learn. |
| 31 | WishlistItems : ICollection&lt;WishlistItem&gt; | Represents the Wishlist Items. |

### 9. CourseAiUsageLog Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ErrorMessage : string? | Represents the Error Message. |
| 2 | InputJson : string? | Represents the Input Json. |
| 3 | IntegrationId : int? | Represents the Integration ID. |
| 4 | InteractionType : string? | Represents the Interaction Type. |
| 5 | LatencyMs : float? | Represents the Latency Ms. |
| 6 | LogCreatedAt : DateTime? | Represents the Log Created At. |
| 7 | LogId : int | Represents the Log ID. |
| 8 | OutputJson : string? | Represents the Output Json. |
| 9 | TokenUsage : float? | Represents the Token Usage. |

### 10. CourseFieldModerationFeedback Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Course : Course | Represents the Course. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | DateAdded : DateTime? | Represents the Date Added. |
| 4 | FeedbackId : int | Represents the Feedback ID. |
| 5 | FeedbackText : string | Represents the Feedback Text. |
| 6 | FieldName : string | Represents the Field Name. |

### 11. CourseQuiz Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | IsHidden : bool | Indicates whether it is hidden. |
| 3 | OrderIndex : int | Represents the Order Index. |
| 4 | QuizId : int | Represents the Quiz ID. |

### 12. CourseReport Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessGrantedUntil : DateTime? | Represents the Access Granted Until. |
| 2 | Course : Course? | Represents the Course. |
| 3 | CourseId : int? | Represents the Course ID. |
| 4 | CourseReportId : int | Represents the Course Report ID. |
| 5 | CourseReportsStatus : string? | Represents the Course Reports Status. |
| 6 | CreatedAt : DateTime? | Represents the Created At. |
| 7 | Description : string? | Represents the Description. |
| 8 | Reason : string? | Represents the Reason. |
| 9 | Reporter : Account? | Represents the Reporter. |
| 10 | ReporterId : int? | Represents the Reporter ID. |
| 11 | ResolutionNote : string? | Represents the Resolution Note. |
| 12 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 13 | Resolver : Account? | Represents the Resolver. |
| 14 | ResolverId : int? | Represents the Resolver ID. |

### 13. CourseReview Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AiThreatLevel : AiThreatLevel | Represents the Ai Threat Level. |
| 2 | Comment : string? | Represents the Comment. |
| 3 | CourseReviewId : int | Represents the Course Review ID. |
| 4 | CourseReviewStatus : string? | Represents the Course Review Status. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | Enrollment : Enrollment? | Represents the Enrollment. |
| 7 | EnrollmentId : int | Represents the Enrollment ID. |
| 8 | IsRemoved : bool? | Indicates whether it is removed. |
| 9 | Rating : int? | Represents the Rating. |
| 10 | Reports : ICollection&lt;Report&gt; | Represents the Reports. |
| 11 | UpdatedAt : DateTime? | Represents the Updated At. |

### 14. CourseReviewModerationLog Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseReviewId : int? | Represents the Course Review ID. |
| 2 | ErrorMessage : string? | Represents the Error Message. |
| 3 | InputJson : string? | Represents the Input Json. |
| 4 | LatencyMs : float? | Represents the Latency Ms. |
| 5 | LogCreatedAt : DateTime? | Represents the Log Created At. |
| 6 | LogId : int | Represents the Log ID. |
| 7 | ModelId : int? | Represents the Model ID. |
| 8 | OutputJson : string? | Represents the Output Json. |

### 15. CourseReviewReport Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessGrantedUntil : DateTime? | Represents the Access Granted Until. |
| 2 | CourseReview : CourseReview? | Represents the Course Review. |
| 3 | CourseReviewId : int? | Represents the Course Review ID. |
| 4 | CourseReviewReportId : int | Represents the Course Review Report ID. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | Description : string? | Represents the Description. |
| 7 | Reason : string? | Represents the Reason. |
| 8 | Reporter : Account? | Represents the Reporter. |
| 9 | ReporterId : int? | Represents the Reporter ID. |
| 10 | ResolutionNote : string? | Represents the Resolution Note. |
| 11 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 12 | Resolver : Account? | Represents the Resolver. |
| 13 | ResolverId : int? | Represents the Resolver ID. |
| 14 | UserReportsStatus : string? | Represents the User Reports Status. |

### 16. Enrollment Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CompletedDate : DateOnly? | Represents the Completed Date. |
| 2 | Course : Course? | Represents the Course. |
| 3 | CourseId : int? | Represents the Course ID. |
| 4 | CourseReviews : ICollection&lt;CourseReview&gt; | Represents the Course Reviews. |
| 5 | Description : string? | Represents the Description. |
| 6 | EnrollDate : DateOnly? | Represents the Enroll Date. |
| 7 | EnrollmentId : int | Represents the Enrollment ID. |
| 8 | EnrollmentStatus : string? | Represents the Enrollment Status. |
| 9 | IsCompleted : bool? | Indicates whether it is completed. |
| 10 | LastAccessedAt : DateTime? | Represents the Last Accessed At. |
| 11 | LessonReviews : ICollection&lt;LessonReview&gt; | Represents the Lesson Reviews. |
| 12 | MaterialCompletions : ICollection&lt;MaterialCompletion&gt; | Represents the Material Completions. |
| 13 | Title : string? | Represents the Title. |
| 14 | User : User? | Represents the User. |
| 15 | UserId : int? | Represents the User ID. |

### 17. Gift Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CardTheme : string? | Represents the Card Theme. |
| 2 | ClaimedAt : DateTime? | Represents the Claimed At. |
| 3 | ClaimedByUser : User? | Represents the Claimed By User. |
| 4 | ClaimedByUserId : int? | Represents the Claimed By User ID. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | DeliveryStatus : string? | Represents the Delivery Status. |
| 7 | GiftId : int | Represents the Gift ID. |
| 8 | GiftMessage : string? | Represents the Gift Message. |
| 9 | IsClaimed : bool | Indicates whether it is claimed. |
| 10 | OrderItem : OrderItem | Represents the Order Item. |
| 11 | OrderItemId : int | Represents the Order Item ID. |
| 12 | RecipientEmail : string | Represents the Recipient Email. |
| 13 | RecipientName : string? | Represents the Recipient Name. |
| 14 | RedemptionToken : string | Represents the Redemption Token. |
| 15 | Sender : Account? | Represents the Sender. |
| 16 | SenderId : int? | Represents the Sender ID. |
| 17 | UpdatedAt : DateTime? | Represents the Updated At. |

### 18. Instructor Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApprovalStatus : string? | Represents the Approval Status. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | ChargesEnabled : bool? | Represents the Charges Enabled. |
| 4 | Courses : ICollection&lt;Course&gt; | Represents the Courses. |
| 5 | DocumentUrl : string? | Represents the Document Url. |
| 6 | ExpertiseCategories : string? | Represents the Expertise Categories. |
| 7 | FacebookUrl : string? | Represents the Facebook Url. |
| 8 | InstructorId : int | Represents the Instructor ID. |
| 9 | InstructorNavigation : User | Represents the Instructor Navigation. |
| 10 | InstructorPayouts : ICollection&lt;InstructorPayout&gt; | Represents the Instructor Payouts. |
| 11 | LinkedinUrl : string? | Represents the Linkedin Url. |
| 12 | PayoutsEnabled : bool? | Represents the Payouts Enabled. |
| 13 | ProfessionalTitle : string? | Represents the Professional Title. |
| 14 | Quizzes : ICollection&lt;Quiz&gt; | Represents the Quizzes. |
| 15 | RejectionReason : string? | Represents the Rejection Reason. |
| 16 | StripeAccountId : string? | Represents the Stripe Account ID. |
| 17 | StripeCountry : string? | Represents the Stripe Country. |
| 18 | StripeOnboardingStatus : string? | Represents the Stripe Onboarding Status. |
| 19 | UserId : int | Represents the User ID. |
| 20 | YoutubeUrl : string? | Represents the Youtube Url. |

### 19. InstructorPayout Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Instructor : Instructor? | Represents the Instructor. |
| 2 | InstructorId : int? | Represents the Instructor ID. |
| 3 | IsPaid : bool | Indicates whether it is paid. |
| 4 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 5 | PayoutAmount : decimal | Represents the Payout Amount. |
| 6 | PayoutDate : DateTime | Represents the Payout Date. |
| 7 | PayoutId : int | Represents the Payout ID. |
| 8 | PayoutStatus : string | Represents the Payout Status. |
| 9 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 10 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 11 | Transaction : Transaction? | Represents the Transaction. |
| 12 | TransactionId : int? | Represents the Transaction ID. |

### 20. InstructorStats Class
| No | Method / Property | Description |
|---|---|---|
| 1 | InstructorId : int | Represents the Instructor ID. |
| 2 | InstructorRating : double | Represents the Instructor Rating. |
| 3 | TotalRevenue : decimal | Represents the Total Revenue. |
| 4 | TotalStudentsCount : int | Represents the Total Students Count. |

### 21. LearningMaterial Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CloudPublicId : string? | Represents the Cloud Public ID. |
| 2 | CourseStatus : string? | Represents the Course Status. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | IsRemoved : bool | Indicates whether it is removed. |
| 6 | LearningStatus : string? | Represents the Learning Status. |
| 7 | Lesson : Lesson? | Represents the Lesson. |
| 8 | LessonId : int? | Represents the Lesson ID. |
| 9 | MaterialCompletions : ICollection&lt;MaterialCompletion&gt; | Represents the Material Completions. |
| 10 | MaterialId : int | Represents the Material ID. |
| 11 | MaterialMetadata : MaterialMetadata? | Represents the Material Metadata. |
| 12 | MaterialUrl : string? | Represents the Material Url. |
| 13 | MediaEmbeddings : ICollection&lt;MediaEmbedding&gt; | Represents the Media Embeddings. |
| 14 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 15 | TextEmbeddings : ICollection&lt;TextEmbedding&gt; | Represents the Text Embeddings. |
| 16 | ThreatLevel : AiThreatLevel | Represents the Threat Level. |
| 17 | Title : string | Represents the Title. |
| 18 | UpdatedAt : DateTime? | Represents the Updated At. |

### 22. Lesson Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Course : Course? | Represents the Course. |
| 2 | CourseId : int? | Represents the Course ID. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | IsRemoved : bool | Indicates whether it is removed. |
| 6 | LearningMaterials : ICollection&lt;LearningMaterial&gt; | Represents the Learning Materials. |
| 7 | LessonId : int | Represents the Lesson ID. |
| 8 | LessonReviews : ICollection&lt;LessonReview&gt; | Represents the Lesson Reviews. |
| 9 | LessonStatus : string? | Represents the Lesson Status. |
| 10 | ModerationFeedback : string? | Represents the Moderation Feedback. |
| 11 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 12 | Title : string | Represents the Title. |
| 13 | UpdatedAt : DateTime? | Represents the Updated At. |

### 23. LessonReview Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AiThreatLevel : AiThreatLevel | Represents the Ai Threat Level. |
| 2 | Comment : string? | Represents the Comment. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Enrollment : Enrollment? | Represents the Enrollment. |
| 5 | EnrollmentId : int | Represents the Enrollment ID. |
| 6 | IsRemoved : bool? | Indicates whether it is removed. |
| 7 | Lesson : Lesson? | Represents the Lesson. |
| 8 | LessonId : int | Represents the Lesson ID. |
| 9 | LessonReviewId : int | Represents the Lesson Review ID. |
| 10 | LessonReviewStatus : string? | Represents the Lesson Review Status. |
| 11 | Rating : int? | Represents the Rating. |
| 12 | Reports : ICollection&lt;Report&gt; | Represents the Reports. |
| 13 | UpdatedAt : DateTime? | Represents the Updated At. |

### 24. LessonReviewModerationLog Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ErrorMessage : string? | Represents the Error Message. |
| 2 | InputJson : string? | Represents the Input Json. |
| 3 | LatencyMs : float? | Represents the Latency Ms. |
| 4 | LessonReviewId : int? | Represents the Lesson Review ID. |
| 5 | LogCreatedAt : DateTime? | Represents the Log Created At. |
| 6 | LogId : int | Represents the Log ID. |
| 7 | ModelId : int? | Represents the Model ID. |
| 8 | OutputJson : string? | Represents the Output Json. |

### 25. LessonReviewReport Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessGrantedUntil : DateTime? | Represents the Access Granted Until. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Description : string? | Represents the Description. |
| 4 | LessonReview : LessonReview? | Represents the Lesson Review. |
| 5 | LessonReviewId : int? | Represents the Lesson Review ID. |
| 6 | LessonReviewReportId : int | Represents the Lesson Review Report ID. |
| 7 | Reason : string? | Represents the Reason. |
| 8 | Reporter : Account? | Represents the Reporter. |
| 9 | ReporterId : int? | Represents the Reporter ID. |
| 10 | ResolutionNote : string? | Represents the Resolution Note. |
| 11 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 12 | Resolver : Account? | Represents the Resolver. |
| 13 | ResolverId : int? | Represents the Resolver ID. |
| 14 | UserReportsStatus : string? | Represents the User Reports Status. |

### 26. Lockout Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Account : Account? | Represents the Account. |
| 2 | AccountId : int? | Represents the Account ID. |
| 3 | IsActive : bool | Indicates whether it is active. |
| 4 | LockoutEnd : DateTime? | Represents the Lockout End. |
| 5 | LockoutId : int | Represents the Lockout ID. |
| 6 | LockoutLevel : string? | Represents the Lockout Level. |
| 7 | LockoutStart : DateTime? | Represents the Lockout Start. |
| 8 | LockoutType : string? | Represents the Lockout Type. |

### 27. Manager Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Account : Account | Represents the Account. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | Bio : string? | Represents the Bio. |
| 4 | Coupons : ICollection&lt;Coupon&gt; | Represents the Coupons. |
| 5 | DisplayName : string | Represents the Display Name. |
| 6 | FullName : string? | Represents the Full Name. |
| 7 | ManagerId : int | Represents the Manager ID. |
| 8 | ManagerNavigation : Account | Represents the Manager Navigation. |
| 9 | PhoneNumber : string? | Represents the Phone Number. |
| 10 | Role : string? | Represents the Role. |
| 11 | SystemConfigs : ICollection&lt;SystemConfig&gt; | Represents the System Configs. |

### 28. MaterialMetadata Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Duration : int? | Represents the Duration. |
| 2 | FileExtension : string? | Represents the File Extension. |
| 3 | FileSize : long? | Represents the File Size. |
| 4 | FileType : string? | Represents the File Type. |
| 5 | PageCount : int? | Represents the Page Count. |

### 29. Message Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChatId : int | Represents the Chat ID. |
| 2 | Content : string | Represents the Content. |
| 3 | IsSeen : bool | Indicates whether it is seen. |
| 4 | MessageId : int | Represents the Message ID. |
| 5 | MessageStatus : string | Represents the Message Status. |
| 6 | ReceivedAt : DateTime? | Represents the Received At. |
| 7 | SenderId : int? | Represents the Sender ID. |
| 8 | SentAt : DateTime? | Represents the Sent At. |

### 30. MessageAttachment Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttachmentId : int | Represents the Attachment ID. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | FileName : string? | Represents the File Name. |
| 4 | FileSize : long? | Represents the File Size. |
| 5 | FileType : string? | Represents the File Type. |
| 6 | FileUrl : string | Represents the File Url. |
| 7 | MessageId : int | Represents the Message ID. |

### 31. Notification Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string? | Represents the Content. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | IsRead : bool? | Indicates whether it is read. |
| 4 | IsRemoved : bool? | Indicates whether it is removed. |
| 5 | LinkAction : string? | Represents the Link Action. |
| 6 | NotificationId : int | Represents the Notification ID. |
| 7 | Receiver : Account? | Represents the Receiver. |
| 8 | ReceiverId : int? | Represents the Receiver ID. |
| 9 | Sender : Account? | Represents the Sender. |
| 10 | SenderId : int? | Represents the Sender ID. |
| 11 | Title : string? | Represents the Title. |

### 32. OrderInfo Class
| No | Method / Property | Description |
|---|---|---|
| 1 | OrderDate : DateTime? | Represents the Order Date. |
| 2 | OrderId : int | Represents the Order ID. |
| 3 | OrderStatus : string? | Represents the Order Status. |
| 4 | PaymentMethod : string? | Represents the Payment Method. |
| 5 | UserId : int? | Represents the User ID. |

### 33. OrderItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string? | Represents the Coupon Code. |
| 2 | CouponType : string? | Represents the Coupon Type. |
| 3 | CouponUsed : bool? | Represents the Coupon Used. |
| 4 | Course : Course? | Represents the Course. |
| 5 | CourseId : int? | Represents the Course ID. |
| 6 | DiscountAmount : decimal | Represents the Discount Amount. |
| 7 | Id : int | Represents the ID. |
| 8 | Order : OrderInfo? | Represents the Order. |
| 9 | OrderId : int? | Represents the Order ID. |
| 10 | OriginalPrice : decimal? | Represents the Original Price. |
| 11 | PurchasePrice : decimal | Represents the Purchase Price. |
| 12 | Transactions : ICollection&lt;Transaction&gt; | Represents the Transactions. |

### 34. PlatformWithdrawal Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | ArrivedAt : DateTime? | Represents the Arrived At. |
| 3 | CreatedAt : DateTime | Represents the Created At. |
| 4 | Currency : string | Represents the Currency. |
| 5 | Description : string? | Represents the Description. |
| 6 | Manager : Manager? | Represents the Manager. |
| 7 | ManagerId : int? | Represents the Manager ID. |
| 8 | Status : string | Represents the Status. |
| 9 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 10 | WithdrawalId : int | Represents the Withdrawal ID. |

### 35. Quiz Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Description : string? | Represents the Description. |
| 4 | InstructorId : int | Represents the Instructor ID. |
| 5 | IsHidden : bool | Indicates whether it is hidden. |
| 6 | IsRemoved : bool | Indicates whether it is removed. |
| 7 | PassingScore : int | Represents the Passing Score. |
| 8 | QuizId : int | Represents the Quiz ID. |
| 9 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 10 | Title : string | Represents the Title. |
| 11 | TotalQuestions : int | Represents the Total Questions. |
| 12 | UpdatedAt : DateTime? | Represents the Updated At. |

### 36. QuizAttempt Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AttemptId : int | Represents the Attempt ID. |
| 2 | IsPassed : bool | Indicates whether it is passed. |
| 3 | QuizId : int | Represents the Quiz ID. |
| 4 | Score : int | Represents the Score. |
| 5 | StartedAt : DateTime? | Represents the Started At. |
| 6 | SubmittedAt : DateTime? | Represents the Submitted At. |
| 7 | UserId : int | Represents the User ID. |

### 37. QuizAttemptAnswer Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AnswerId : int | Represents the Answer ID. |
| 2 | AttemptId : int | Represents the Attempt ID. |
| 3 | IsCorrect : bool | Indicates whether it is correct. |
| 4 | QuestionId : int | Represents the Question ID. |
| 5 | SelectedOptionId : int? | Represents the Selected Option ID. |

### 38. QuizOption Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IsCorrect : bool | Indicates whether it is correct. |
| 2 | OptionId : int | Represents the Option ID. |
| 3 | OptionText : string | Represents the Option Text. |
| 4 | OrderIndex : int | Represents the Order Index. |
| 5 | QuestionId : int | Represents the Question ID. |

### 39. QuizQuestion Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Explanation : string? | Represents the Explanation. |
| 4 | LessonId : int | Represents the Lesson ID. |
| 5 | QuestionId : int | Represents the Question ID. |
| 6 | QuestionText : string | Represents the Question Text. |
| 7 | QuestionType : QuizQuestionType | Represents the Question Type. |

### 40. SystemConfig Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfigId : int | Represents the Config ID. |
| 2 | ConfigKey : string | Represents the Config Key. |
| 3 | ConfigValue : string? | Represents the Config Value. |
| 4 | Description : string? | Represents the Description. |
| 5 | Manager : Manager? | Represents the Manager. |
| 6 | ManagerId : int? | Represents the Manager ID. |
| 7 | UpdatedAt : DateTime? | Represents the Updated At. |

### 41. Transaction Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountFrom : int? | Represents the Account From. |
| 2 | AccountFromNavigation : Account? | Represents the Account From Navigation. |
| 3 | AccountTo : int? | Represents the Account To. |
| 4 | AccountToNavigation : Account? | Represents the Account To Navigation. |
| 5 | Amount : decimal | Represents the Amount. |
| 6 | CreatedAt : DateTime | Represents the Created At. |
| 7 | Currency : string? | Represents the Currency. |
| 8 | InstructorPayouts : ICollection&lt;InstructorPayout&gt; | Represents the Instructor Payouts. |
| 9 | OrderItem : OrderItem? | Represents the Order Item. |
| 10 | OrderItemId : int? | Represents the Order Item ID. |
| 11 | RefundReason : string? | Represents the Refund Reason. |
| 12 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 13 | StripePaymentintentId : string? | Represents the Stripe Paymentintent ID. |
| 14 | StripeSessionId : string? | Represents the Stripe Session ID. |
| 15 | TransactionCreatedAt : DateTime? | Represents the Transaction Created At. |
| 16 | TransactionExt : TransactionExt? | Represents the Transaction Ext. |
| 17 | TransactionId : int | Represents the Transaction ID. |
| 18 | TransactionType : string? | Represents the Transaction Type. |
| 19 | TransactionsStatus : string? | Represents the Transactions Status. |
| 20 | TransferRate : decimal | Represents the Transfer Rate. |

### 42. TransactionExt Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | PaymentMethod : string? | Represents the Payment Method. |
| 3 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 4 | RefundReason : string? | Represents the Refund Reason. |
| 5 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 6 | Transaction : Transaction | Represents the Transaction. |
| 7 | TransactionId : int | Represents the Transaction ID. |

### 43. User Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Account : Account | Represents the Account. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | Bio : string? | Represents the Bio. |
| 4 | CartItems : ICollection&lt;CartItem&gt; | Represents the Cart Items. |
| 5 | DateOfBirth : DateOnly? | Represents the Date Of Birth. |
| 6 | Email : string | Represents the Email. |
| 7 | Enrollments : ICollection&lt;Enrollment&gt; | Represents the Enrollments. |
| 8 | FullName : string | Represents the Full Name. |
| 9 | Instructor : Instructor? | Represents the Instructor. |
| 10 | IsVerified : bool | Indicates whether it is verified. |
| 11 | OrderInfos : ICollection&lt;OrderInfo&gt; | Represents the Order Infos. |
| 12 | PhoneNumber : string? | Represents the Phone Number. |
| 13 | QuizAttempts : ICollection&lt;QuizAttempt&gt; | Represents the Quiz Attempts. |
| 14 | UserAddresses : ICollection&lt;UserAddress&gt; | Represents the User Addresses. |
| 15 | UserId : int | Represents the User ID. |
| 16 | UserNavigation : Account | Represents the User Navigation. |
| 17 | UserPayments : ICollection&lt;UserPayment&gt; | Represents the User Payments. |
| 18 | Username : string | Represents the Username. |
| 19 | WishlistItems : ICollection&lt;WishlistItem&gt; | Represents the Wishlist Items. |

### 44. UserLockout Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreatedAt : DateTime? | Represents the Created At. |
| 2 | LockoutEnd : DateTime? | Represents the Lockout End. |
| 3 | LockoutId : int | Represents the Lockout ID. |
| 4 | LockoutType : string | Represents the Lockout Type. |
| 5 | Reason : string? | Represents the Reason. |
| 6 | User : User? | Represents the User. |
| 7 | UserId : int | Represents the User ID. |

### 45. WishlistItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | CreatedAt : DateTime | Represents the Created At. |
| 3 | UserId : int | Represents the User ID. |
| 4 | WishlistItemId : int | Represents the Wishlist Item ID. |

## Layer: Enumeration
### 1. TransactionStatus Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Pending | Represents the Pending. |
| 2 | RefundPending | Represents the Refund Pending. |
| 3 | Refunded | Represents the Refunded. |
| 4 | Succeeded | Represents the Succeeded. |

## Layer: Frontend Controller
### 1. AccountController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChangePassword(request: ChangePasswordViewModel) : Task&lt;IActionResult&gt; | Executes the "Change Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: ChangePasswordViewModel` as input parameters. |
| 2 | EditProfile(request: UpdateProfileViewModel) : Task&lt;IActionResult&gt; | Executes the "Edit Profile" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: UpdateProfileViewModel` as input parameters. |
| 3 | ForgotPassword(model: ForgotPasswordViewModel) : Task&lt;IActionResult&gt; | Executes the "Forgot Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: ForgotPasswordViewModel` as input parameters. |
| 4 | GoogleLogin(model: GoogleLoginViewModel) : Task&lt;IActionResult&gt; | Executes the "Google Login" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: GoogleLoginViewModel` as input parameters. |
| 5 | Login() : Task&lt;IActionResult&gt; | Executes the "Login" operation asynchronously, returning `Task<IActionResult>`. |
| 6 | Login(model: LoginViewModel) : Task&lt;IActionResult&gt; | Executes the "Login" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: LoginViewModel` as input parameters. |
| 7 | Logout() : Task&lt;IActionResult&gt; | Executes the "Logout" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | Profile() : Task&lt;IActionResult&gt; | Executes the "Profile" operation asynchronously, returning `Task<IActionResult>`. |
| 9 | Register() : Task&lt;IActionResult&gt; | Executes the "Register" operation asynchronously, returning `Task<IActionResult>`. |
| 10 | Register(model: RegisterViewModel) : Task&lt;IActionResult&gt; | Executes the "Register" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: RegisterViewModel` as input parameters. |
| 11 | ResetPassword(model: ResetPasswordViewModel) : Task&lt;IActionResult&gt; | Executes the "Reset Password" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: ResetPasswordViewModel` as input parameters. |
| 12 | VerifyOtp(model: VerifyOtpViewModel) : Task&lt;IActionResult&gt; | Executes the "Verify Otp" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: VerifyOtpViewModel` as input parameters. |
| 13 | VerifyOtp(request: VerifyOtpRequestDTO) : Task&lt;IActionResult&gt; | Executes the "Verify Otp" operation asynchronously, returning `Task<IActionResult>`. It accepts `request: VerifyOtpRequestDTO` as input parameters. |

### 2. AdminAccountController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateStaff(model: CreateStaffFERequestDTO) : Task&lt;IActionResult&gt; | Executes the "Create Staff" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: CreateStaffFERequestDTO` as input parameters. |
| 2 | Detail(id: int) : Task&lt;IActionResult&gt; | Executes the "Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | EditStaff(id: int, model: UpdateStaffFERequestDTO) : Task&lt;IActionResult&gt; | Executes the "Edit Staff" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, model: UpdateStaffFERequestDTO` as input parameters. |
| 4 | FlagAccount(id: int, model: FlagAccountFERequestDTO) : Task&lt;IActionResult&gt; | Executes the "Flag Account" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, model: FlagAccountFERequestDTO` as input parameters. |
| 5 | Index(keyword: string?, role: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `keyword: string?, role: string?, page: int` as input parameters. |
| 6 | ToggleBan(id: int) : Task&lt;IActionResult&gt; | Executes the "Toggle Ban" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |

### 3. AdminAiServiceController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddModel(req: CreateAiModelRequest) : Task&lt;IActionResult&gt; | Executes the "Add Model" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: CreateAiModelRequest` as input parameters. |
| 2 | EditModel(id: int, req: UpdateAiModelRequest) : Task&lt;IActionResult&gt; | Executes the "Edit Model" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, req: UpdateAiModelRequest` as input parameters. |
| 3 | Index(tab: string, subtab: string, modelPage: int, coursePage: int, cReviewPage: int, lReviewPage: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `tab: string, subtab: string, modelPage: int, coursePage: int, cReviewPage: int, lReviewPage: int` as input parameters. |
| 4 | ToggleModelStatus(id: int) : Task&lt;IActionResult&gt; | Executes the "Toggle Model Status" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | UpdateThresholds(req: UpdateThresholdsRequest) : Task&lt;IActionResult&gt; | Executes the "Update Thresholds" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: UpdateThresholdsRequest` as input parameters. |

### 4. AdminApprovalController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Detail(id: int) : Task&lt;IActionResult&gt; | Executes the "Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 2 | Index(page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int` as input parameters. |
| 3 | Process(id: int, status: string, reason: string?) : Task&lt;IActionResult&gt; | Executes the "Process" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, status: string, reason: string?` as input parameters. |

### 5. AdminFinanceController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveRefundDecision(transactionId: int, adminNote: string?) : Task&lt;IActionResult&gt; | Executes the "Approve Refund Decision" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, adminNote: string?` as input parameters. |
| 2 | Index(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, tab: string, year: int?, month: int?, payoutPage: int, withdrawPage: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, tab: string, year: int?, month: int?, payoutPage: int, withdrawPage: int` as input parameters. |
| 3 | Index(year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `year: int?, month: int?` as input parameters. |
| 4 | RejectRefundDecision(transactionId: int, adminNote: string?) : Task&lt;IActionResult&gt; | Executes the "Reject Refund Decision" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, adminNote: string?` as input parameters. |
| 5 | SetPayoutDays(payoutDays: string) : Task&lt;IActionResult&gt; | Executes the "Set Payout Days" operation asynchronously, returning `Task<IActionResult>`. It accepts `payoutDays: string` as input parameters. |
| 6 | SetTransferRate(rate: decimal) : Task&lt;IActionResult&gt; | Executes the "Set Transfer Rate" operation asynchronously, returning `Task<IActionResult>`. It accepts `rate: decimal` as input parameters. |
| 7 | Withdraw(amount: decimal?, description: string?) : Task&lt;IActionResult&gt; | Executes the "Withdraw" operation asynchronously, returning `Task<IActionResult>`. It accepts `amount: decimal?, description: string?` as input parameters. |
| 8 | _api : ApiClient | Provides the Api dependency. |

### 6. AdminModerationController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveCourse(id: int, feedback: string?) : Task&lt;IActionResult&gt; | Executes the "Approve Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, feedback: string?` as input parameters. |
| 2 | Courses(search: string?, category: string?, status: string?, sortBy: string?, page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "Courses" operation asynchronously, returning `Task<IActionResult>`. It accepts `search: string?, category: string?, status: string?, sortBy: string?, page: int, pageSize: int` as input parameters. |
| 3 | FlagCourse(id: int, reason: string) : Task&lt;IActionResult&gt; | Executes the "Flag Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, reason: string` as input parameters. |
| 4 | GetCourseReports(status: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Get Course Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `status: string?, page: int` as input parameters. |
| 5 | GetCourseReviewReports(status: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Get Course Review Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `status: string?, page: int` as input parameters. |
| 6 | GetLessonReviewReports(status: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Get Lesson Review Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `status: string?, page: int` as input parameters. |
| 7 | GetReportStats() : Task&lt;IActionResult&gt; | Executes the "Get Report Stats" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | RejectCourseDetailed(body: JsonElement) : Task&lt;IActionResult&gt; | Executes the "Reject Course Detailed" operation asynchronously, returning `Task<IActionResult>`. It accepts `body: JsonElement` as input parameters. |
| 9 | Reports(coursePage: int, cReviewPage: int, lReviewPage: int) : Task&lt;IActionResult&gt; | Executes the "Reports" operation asynchronously, returning `Task<IActionResult>`. It accepts `coursePage: int, cReviewPage: int, lReviewPage: int` as input parameters. |
| 10 | ResolveCourseReport(reportId: int, model: ResolveReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Resolve Course Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, model: ResolveReportViewModel` as input parameters. |
| 11 | ResolveCourseReviewReport(reportId: int, model: ResolveReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Resolve Course Review Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, model: ResolveReportViewModel` as input parameters. |
| 12 | ResolveLessonReviewReport(reportId: int, model: ResolveReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Resolve Lesson Review Report" operation asynchronously, returning `Task<IActionResult>`. It accepts `reportId: int, model: ResolveReportViewModel` as input parameters. |

### 7. AdminProfileController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Edit(model: UpdateAdminProfileViewModel) : Task&lt;IActionResult&gt; | Executes the "Edit" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: UpdateAdminProfileViewModel` as input parameters. |
| 2 | Index() : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. |

### 8. CartController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToCart(courseId: int, returnUrl: string?) : Task&lt;IActionResult&gt; | Executes the "Add To Cart" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, returnUrl: string?` as input parameters. |
| 2 | AddToCartAjax(id: int) : Task&lt;IActionResult&gt; | Executes the "Add To Cart Ajax" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | ApplyCoupon(model: ApplyCouponViewModel) : Task&lt;IActionResult&gt; | Executes the "Apply Coupon" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: ApplyCouponViewModel` as input parameters. |
| 4 | Checkout() : Task&lt;IActionResult&gt; | Executes the "Checkout" operation asynchronously, returning `Task<IActionResult>`. |
| 5 | CheckoutSuccess(payment_intent_id: string) : Task&lt;IActionResult&gt; | Executes the "Checkout Success" operation asynchronously, returning `Task<IActionResult>`. It accepts `payment_intent_id: string` as input parameters. |
| 6 | CreatePaymentIntentAjax(couponCode: string) : Task&lt;IActionResult&gt; | Executes the "Create Payment Intent Ajax" operation asynchronously, returning `Task<IActionResult>`. It accepts `couponCode: string` as input parameters. |
| 7 | GetCouponMapFromCookie() : Dictionary&lt;int, string&gt; | Executes the "Get Coupon Map From Cookie" operation, returning `Dictionary<int, string>`. |
| 8 | Index() : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. |
| 9 | RemoveCoupon(couponCode: string) : Task&lt;IActionResult&gt; | Executes the "Remove Coupon" operation asynchronously, returning `Task<IActionResult>`. It accepts `couponCode: string` as input parameters. |
| 10 | RemoveFromCart(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Remove From Cart" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 11 | SaveCouponMapToCookie(map: Dictionary&lt;int, string&gt;) | Executes the "Save Coupon Map To Cookie" operation, returning `void`. It accepts `map: Dictionary<int, string>` as input parameters. |

### 9. ChatController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Admin() : IActionResult | Executes the "Admin" operation, returning an HTTP action response. |
| 2 | ClearHistory(chatId: int) : Task&lt;IActionResult&gt; | Executes the "Clear History" operation asynchronously, returning `Task<IActionResult>`. It accepts `chatId: int` as input parameters. |
| 3 | CreateChat(dto: CreateChatDto) : Task&lt;IActionResult&gt; | Executes the "Create Chat" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: CreateChatDto` as input parameters. |
| 4 | GetActorFromCookie() : string | Executes the "Get Actor From Cookie" operation, returning `string`. |
| 5 | GetHistory(roomId: int) : Task&lt;IActionResult&gt; | Executes the "Get History" operation asynchronously, returning `Task<IActionResult>`. It accepts `roomId: int` as input parameters. |
| 6 | GetList() : Task&lt;IActionResult&gt; | Executes the "Get List" operation asynchronously, returning `Task<IActionResult>`. |
| 7 | Index() : IActionResult | Executes the "Index" operation, returning an HTTP action response. |
| 8 | Instructor() : IActionResult | Executes the "Instructor" operation, returning an HTTP action response. |
| 9 | Learner() : IActionResult | Executes the "Learner" operation, returning an HTTP action response. |
| 10 | RequestSupport(dto: SupportRequestDto) : Task&lt;IActionResult&gt; | Executes the "Request Support" operation asynchronously, returning `Task<IActionResult>`. It accepts `dto: SupportRequestDto` as input parameters. |
| 11 | Search(q: string) : Task&lt;IActionResult&gt; | Executes the "Search" operation asynchronously, returning `Task<IActionResult>`. It accepts `q: string` as input parameters. |
| 12 | UploadAttachment(file: IFormFile) : Task&lt;IActionResult&gt; | Executes the "Upload Attachment" operation asynchronously, returning `Task<IActionResult>`. It accepts `file: IFormFile` as input parameters. |

### 10. CourseController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteReview(reviewId: int, type: string) : Task&lt;IActionResult&gt; | Executes the "Delete Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `reviewId: int, type: string` as input parameters. |
| 2 | Details(id: int) : Task&lt;IActionResult&gt; | Executes the "Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | EnrollFree(id: int) : Task&lt;IActionResult&gt; | Executes the "Enroll Free" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 4 | GetProgress(id: int) : Task&lt;IActionResult&gt; | Executes the "Get Progress" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 5 | GetReviews(id: int, page: int, pageSize: int, starFilter: int?) : Task&lt;IActionResult&gt; | Executes the "Get Reviews" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 6 | Index(query: string, category: string, sort: string, price: string, rating: string, page: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `query: string, category: string, sort: string, price: string, rating: string, page: int` as input parameters. |
| 7 | Learn(id: int) : Task&lt;IActionResult&gt; | Executes the "Learn" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 8 | MyCourses() : Task&lt;IActionResult&gt; | Executes the "My Courses" operation asynchronously, returning `Task<IActionResult>`. |
| 9 | SubmitReview(body: JsonElement, source: string) : Task&lt;IActionResult&gt; | Executes the "Submit Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `body: JsonElement, source: string` as input parameters. |

### 11. CourseQuizController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Add(req: CourseQuizAddRequest) : Task&lt;IActionResult&gt; | Executes the "Add" operation asynchronously, returning `Task<IActionResult>`. It accepts `req: CourseQuizAddRequest` as input parameters. |

### 12. GiftController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CheckRecipient(email: string, courseId: int) : Task&lt;IActionResult&gt; | Executes the "Check Recipient" operation asynchronously, returning `Task<IActionResult>`. It accepts `email: string, courseId: int` as input parameters. |
| 2 | Checkout(courseId: int, recipientEmail: string, recipientName: string?, giftMessage: string?, cardTheme: string) : Task&lt;IActionResult&gt; | Executes the "Checkout" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, recipientEmail: string, recipientName: string?, giftMessage: string?, cardTheme: string` as input parameters. |
| 3 | CheckoutSuccess(sessionId: string?, paymentIntentId: string?) : Task&lt;IActionResult&gt; | Executes the "Checkout Success" operation asynchronously, returning `Task<IActionResult>`. It accepts `sessionId: string?, paymentIntentId: string?` as input parameters. |
| 4 | ClaimGiftAjax(token: string) : Task&lt;IActionResult&gt; | Executes the "Claim Gift Ajax" operation asynchronously, returning `Task<IActionResult>`. It accepts `token: string` as input parameters. |
| 5 | CreateGiftPaymentIntentAjax(courseId: int, recipientEmail: string, recipientName: string?, giftMessage: string?, cardTheme: string) : Task&lt;IActionResult&gt; | Executes the "Create Gift Payment Intent Ajax" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, recipientEmail: string, recipientName: string?, giftMessage: string?, cardTheme: string` as input parameters. |
| 6 | Setup(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Setup" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 7 | Setup(model: GiftSetupViewModel) : Task&lt;IActionResult&gt; | Executes the "Setup" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: GiftSetupViewModel` as input parameters. |

### 13. InstructorController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Apply() : Task&lt;IActionResult&gt; | Executes the "Apply" operation asynchronously, returning `Task<IActionResult>`. |
| 2 | Apply(model: InstructorApplyViewModel) : Task&lt;IActionResult&gt; | Executes the "Apply" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: InstructorApplyViewModel` as input parameters. |
| 3 | LoadCategoriesAsync() : Task&lt;List&gt; | Executes the "Load Categories" operation asynchronously, returning `Task<List>`. |
| 4 | LoadEmailVerifiedAsync() : Task | Executes the "Load Email Verified" operation asynchronously, returning `Task`. |
| 5 | LoadStripeCountriesAsync() : Task&lt;List&gt; | Executes the "Load Stripe Countries" operation asynchronously, returning `Task<List>`. |
| 6 | Profile(id: int) : Task&lt;IActionResult&gt; | Executes the "Profile" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 7 | SetupPayout() : Task&lt;IActionResult&gt; | Executes the "Setup Payout" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | StripeReturn(instructorId: int) : Task&lt;IActionResult&gt; | Executes the "Stripe Return" operation asynchronously, returning `Task<IActionResult>`. It accepts `instructorId: int` as input parameters. |

### 14. InstructorCourseController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddLesson(courseId: int, title: string) : Task&lt;IActionResult&gt; | Executes the "Add Lesson" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, title: string` as input parameters. |
| 2 | AddMaterial(model: AddMaterialViewModel) : Task&lt;IActionResult&gt; | Executes the "Add Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: AddMaterialViewModel` as input parameters. |
| 3 | Create() : Task&lt;IActionResult&gt; | Executes the "Create" operation asynchronously, returning `Task<IActionResult>`. |
| 4 | Create(model: CreateCourseViewModel) : Task&lt;IActionResult&gt; | Executes the "Create" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: CreateCourseViewModel` as input parameters. |
| 5 | Delete(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 6 | Editor(id: int) : Task&lt;IActionResult&gt; | Executes the "Editor" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 7 | GetCategoriesAsync() : Task&lt;List&gt;CategoryViewModel&lt;&gt; | Executes the "Get Categories" operation asynchronously, returning `Task<List>CategoryViewModel<>`. |
| 8 | Index(searchTerm: string?, status: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `searchTerm: string?, status: string?, page: int` as input parameters. |
| 9 | LoadStripeStatusAsync() : Task | Executes the "Load Stripe Status" operation asynchronously, returning `Task`. |
| 10 | LoadTransferRateAsync() : Task | Executes the "Load Transfer Rate" operation asynchronously, returning `Task`. |
| 11 | ModerateCourse(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Moderate Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 12 | PermanentDeleteMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Permanent Delete Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 13 | RemoveLesson(lessonId: int) : Task&lt;IActionResult&gt; | Executes the "Remove Lesson" operation asynchronously, returning `Task<IActionResult>`. It accepts `lessonId: int` as input parameters. |
| 14 | RemoveMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Remove Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 15 | RestoreMaterial(materialId: int) : Task&lt;IActionResult&gt; | Executes the "Restore Material" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int` as input parameters. |
| 16 | Trash() : Task&lt;IActionResult&gt; | Executes the "Trash" operation asynchronously, returning `Task<IActionResult>`. |
| 17 | UpdateCourseStatus(courseId: int, status: string) : Task&lt;IActionResult&gt; | Executes the "Update Course Status" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, status: string` as input parameters. |
| 18 | UpdateDetails(model: UpdateCourseDetailsViewModel) : Task&lt;IActionResult&gt; | Executes the "Update Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: UpdateCourseDetailsViewModel` as input parameters. |
| 19 | UpdateMaterialDetails(materialId: int, requestBody: object) : Task&lt;IActionResult&gt; | Executes the "Update Material Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `materialId: int, requestBody: object` as input parameters. |

### 15. NotificationController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Admin() : IActionResult | Executes the "Admin" operation, returning an HTTP action response. |
| 2 | Delete(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | Details(id: int) : Task&lt;IActionResult&gt; | Executes the "Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 4 | GetAllAdmin() : Task&lt;IActionResult&gt; | Executes the "Get All Admin" operation asynchronously, returning `Task<IActionResult>`. |
| 5 | GetListTiny() : Task&lt;IActionResult&gt; | Executes the "Get List Tiny" operation asynchronously, returning `Task<IActionResult>`. |
| 6 | GetMyNotifications() : Task&lt;IActionResult&gt; | Executes the "Get My Notifications" operation asynchronously, returning `Task<IActionResult>`. |
| 7 | GetUnreadSummary() : Task&lt;IActionResult&gt; | Executes the "Get Unread Summary" operation asynchronously, returning `Task<IActionResult>`. |
| 8 | Index() : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. |
| 9 | MarkAllAsRead() : Task&lt;IActionResult&gt; | Executes the "Mark All As Read" operation asynchronously, returning `Task<IActionResult>`. |
| 10 | MarkAsRead(id: int) : Task&lt;IActionResult&gt; | Executes the "Mark As Read" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 11 | SearchEmails(query: string) : Task&lt;IActionResult&gt; | Executes the "Search Emails" operation asynchronously, returning `Task<IActionResult>`. It accepts `query: string` as input parameters. |
| 12 | SendAdvanced(payload: object) : Task&lt;IActionResult&gt; | Executes the "Send Advanced" operation asynchronously, returning `Task<IActionResult>`. It accepts `payload: object` as input parameters. |
| 13 | _api : ApiClient | Provides the Api dependency. |
| 14 | _client : HttpClient | Provides the Client dependency. |

### 16. QuestionBankController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseLessonsSummary(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Course Lessons Summary" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 2 | CreateQuestion(courseId: int, request: object) : Task&lt;IActionResult&gt; | Executes the "Create Question" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int, request: object` as input parameters. |
| 3 | DeleteQuestion(qId: int) : Task&lt;IActionResult&gt; | Executes the "Delete Question" operation asynchronously, returning `Task<IActionResult>`. It accepts `qId: int` as input parameters. |
| 4 | GetQuestions(lessonId: int) : Task&lt;IActionResult&gt; | Executes the "Get Questions" operation asynchronously, returning `Task<IActionResult>`. It accepts `lessonId: int` as input parameters. |
| 5 | UpdateQuestion(qId: int, request: object) : Task&lt;IActionResult&gt; | Executes the "Update Question" operation asynchronously, returning `Task<IActionResult>`. It accepts `qId: int, request: object` as input parameters. |

### 17. QuizAttemptController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Details(attemptId: int) : Task&lt;IActionResult&gt; | Executes the "Details" operation asynchronously, returning `Task<IActionResult>`. It accepts `attemptId: int` as input parameters. |
| 2 | History(quizId: int, page: int, pageSize: int) : Task&lt;IActionResult&gt; | Executes the "History" operation asynchronously, returning `Task<IActionResult>`. It accepts `quizId: int, page: int, pageSize: int` as input parameters. |
| 3 | Submit(payload: JsonElement) : Task&lt;IActionResult&gt; | Executes the "Submit" operation asynchronously, returning `Task<IActionResult>`. It accepts `payload: JsonElement` as input parameters. |
| 4 | Take(quizId: int) : Task&lt;IActionResult&gt; | Executes the "Take" operation asynchronously, returning `Task<IActionResult>`. It accepts `quizId: int` as input parameters. |

### 18. QuizController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Create(model: QuizCreateViewModel) : Task&lt;IActionResult&gt; | Executes the "Create" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: QuizCreateViewModel` as input parameters. |
| 2 | Delete(id: int) : Task&lt;IActionResult&gt; | Executes the "Delete" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 3 | Index(searchTerm: string?, page: int) : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. It accepts `searchTerm: string?, page: int` as input parameters. |
| 4 | UpdateSettings(id: int, model: QuizUpdateViewModel) : Task&lt;IActionResult&gt; | Executes the "Update Settings" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int, model: QuizUpdateViewModel` as input parameters. |

### 19. ReportController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ReportCourse(model: CreateCourseReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Report Course" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: CreateCourseReportViewModel` as input parameters. |
| 2 | ReportCourseReview(model: CreateReviewReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Report Course Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: CreateReviewReportViewModel` as input parameters. |
| 3 | ReportLessonReview(model: CreateReviewReportViewModel) : Task&lt;IActionResult&gt; | Executes the "Report Lesson Review" operation asynchronously, returning `Task<IActionResult>`. It accepts `model: CreateReviewReportViewModel` as input parameters. |

### 20. TransactionController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Detail(id: int) : Task&lt;IActionResult&gt; | Executes the "Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 2 | History(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;IActionResult&gt; | Executes the "History" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |
| 3 | Instructor(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, tab: string, payoutPage: int, payoutPageSize: int, payoutKeyword: string?, payoutSortBy: string?, payoutStatus: string?, year: int?, month: int?, courseSortBy: string?) : Task&lt;IActionResult&gt; | Executes the "Instructor" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, tab: string, payoutPage: int, payoutPageSize: int, payoutKeyword: string?, payoutSortBy: string?, payoutStatus: string?, year: int?, month: int?, courseSortBy: string?` as input parameters. |
| 4 | Instructor(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?, courseSortBy: string?) : Task&lt;IActionResult&gt; | Executes the "Instructor" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?, courseSortBy: string?` as input parameters. |
| 5 | Instructor(page: int, pageSize: int, keyword: string?, sortBy: string?, tab: string, status: string?, year: int?, month: int?) : Task&lt;IActionResult&gt; | Executes the "Instructor" operation asynchronously, returning `Task<IActionResult>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, tab: string, status: string?, year: int?, month: int?` as input parameters. |
| 6 | InstructorDetail(id: int) : Task&lt;IActionResult&gt; | Executes the "Instructor Detail" operation asynchronously, returning `Task<IActionResult>`. It accepts `id: int` as input parameters. |
| 7 | RequestRefund(transactionId: int, reason: string) : Task&lt;IActionResult&gt; | Executes the "Request Refund" operation asynchronously, returning `Task<IActionResult>`. It accepts `transactionId: int, reason: string` as input parameters. |
| 8 | _api : ApiClient | Provides the Api dependency. |

### 21. WishlistController Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Index() : Task&lt;IActionResult&gt; | Executes the "Index" operation asynchronously, returning `Task<IActionResult>`. |
| 2 | Remove(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Remove" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |
| 3 | ToggleAjax(courseId: int) : Task&lt;IActionResult&gt; | Executes the "Toggle Ajax" operation asynchronously, returning `Task<IActionResult>`. It accepts `courseId: int` as input parameters. |

## Layer: Hub
### 1. ChatHub Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAccountId() : int | Executes the "Get Account Id" operation, returning `int`. |
| 2 | MarkAsRead(chatId: int) : Task | Executes the "Mark As Read" operation asynchronously, returning `Task`. It accepts `chatId: int` as input parameters. |
| 3 | SendMessage(dto: SendMessageDto) : Task | Executes the "Send Message" operation asynchronously, returning `Task`. It accepts `dto: SendMessageDto` as input parameters. |
| 4 | _chatService : IChatService | Provides the Chat Service dependency. |

### 2. NotificationHub Class
| No | Method / Property | Description |
|---|---|---|
| 1 | SendAsync(method: string) : Task | Executes the "Send" operation asynchronously, returning `Task`. It accepts `method: string` as input parameters. |

## Layer: Repository
### 1. AdminFinanceRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddWithdrawalAsync(withdrawal: PlatformWithdrawal) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Add Withdrawal" operation asynchronously, returning `Task<int>`. It accepts `withdrawal: PlatformWithdrawal` as input parameters. |
| 2 | GetGrossRevenueAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Gross Revenue" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 3 | GetInstructorCourseRevenuesByInstructorAsync(instructorId: int, year: int, month: int) : Task&lt;List&gt;InstructorCourseRevenueProjection&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Course Revenues By Instructor" operation asynchronously, returning `Task<List>InstructorCourseRevenueProjection<>`. It accepts `instructorId: int, year: int, month: int` as input parameters. |
| 4 | GetMaturedEscrowAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Matured Escrow" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 5 | GetPayoutDetailsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;(List&gt;PayoutDetailProjection&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Payout Details" operation asynchronously, returning `Task<(List>PayoutDetailProjection<, int)>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 6 | GetPayoutDetailsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;PayoutDetailProjection&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get Payout Details" operation asynchronously, returning `Task<Tuple>List<PayoutDetailProjection>, int<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 7 | GetPendingEscrowAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Pending Escrow" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 8 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 9 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;Transaction&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<Tuple>List<Transaction>, int<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 10 | GetRefundEligibilityMetricsAsync(transactionId: int, studentId: int, courseId: int) : Task&lt;RefundEligibilityDto&gt; | Provides the concrete implementation to execute the "Get Refund Eligibility Metrics" operation asynchronously, returning `Task<RefundEligibilityDto>`. It accepts `transactionId: int, studentId: int, courseId: int` as input parameters. |
| 11 | GetStripeTransferIdByDestinationPaymentAsync(destinationPaymentId: string) : Task&lt;string?&gt; | Provides the concrete implementation to execute the "Get Stripe Transfer Id By Destination Payment" operation asynchronously, returning `Task<string?>`. It accepts `destinationPaymentId: string` as input parameters. |
| 12 | GetSucceededTransactionCountAsync(year: int?, month: int?) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Succeeded Transaction Count" operation asynchronously, returning `Task<int>`. It accepts `year: int?, month: int?` as input parameters. |
| 13 | GetTotalPaidOutAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Total Paid Out" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 14 | GetTotalRefundedAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Total Refunded" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 15 | GetTransactionWithFullGraphAsync(transactionId: int) : Task&lt;Transaction?&gt; | Provides the concrete implementation to execute the "Get Transaction With Full Graph" operation asynchronously, returning `Task<Transaction?>`. It accepts `transactionId: int` as input parameters. |
| 16 | GetWithdrawalsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;PlatformWithdrawal&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get Withdrawals" operation asynchronously, returning `Task<Tuple>List<PlatformWithdrawal>, int<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 17 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 18 | _context : AppDbContext | Provides the Context dependency. |

### 2. AiModelRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Add(model: AiModel) : AiModel | Provides the concrete implementation to execute the "Add" operation, returning `AiModel`. It accepts `model: AiModel` as input parameters. |
| 2 | GetAllAdminAsync() : Task&lt;List&gt;AiModel&lt;&gt; | Provides the concrete implementation to execute the "Get All Admin" operation asynchronously, returning `Task<List>AiModel<>`. |
| 3 | GetByIdAsync(id: int) : Task&lt;AiModel?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<AiModel?>`. It accepts `id: int` as input parameters. |
| 4 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;AiModel&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>AiModel<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(model: AiModel) : AiModel | Provides the concrete implementation to execute the "Update" operation, returning `AiModel`. It accepts `model: AiModel` as input parameters. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

### 3. CartRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCartItemAsync(item: CartItem) : Task | Provides the concrete implementation to execute the "Add Cart Item" operation asynchronously, returning `Task`. It accepts `item: CartItem` as input parameters. |
| 2 | GetCartItemAsync(userId: int, courseId: int) : Task&lt;CartItem?&gt; | Provides the concrete implementation to execute the "Get Cart Item" operation asynchronously, returning `Task<CartItem?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetCartItemsWithDetailsAsync(userId: int) : Task&lt;List&gt;CartItem&lt;&gt; | Provides the concrete implementation to execute the "Get Cart Items With Details" operation asynchronously, returning `Task<List>CartItem<>`. It accepts `userId: int` as input parameters. |
| 4 | IsCourseInAnyCartAsync(courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Course In Any Cart" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int` as input parameters. |
| 5 | IsCourseInCartAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Course In Cart" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 6 | RemoveCartItem(item: CartItem) | Provides the concrete implementation to execute the "Remove Cart Item" operation, returning `void`. It accepts `item: CartItem` as input parameters. |
| 7 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 8 | _context : AppDbContext | Provides the Context dependency. |

### 4. ChatRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddMessageAsync(message: Message) : Task | Provides the concrete implementation to execute the "Add Message" operation asynchronously, returning `Task`. It accepts `message: Message` as input parameters. |
| 2 | GetActiveChatsCountAsync(id: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Active Chats Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 3 | GetChatByIdAsync(chatId: int) : Task&lt;Chat?&gt; | Provides the concrete implementation to execute the "Get Chat By Id" operation asynchronously, returning `Task<Chat?>`. It accepts `chatId: int` as input parameters. |
| 4 | GetMessagesByChatIdAsync(chatId: int) : Task&lt;List&gt;Message&lt;&gt; | Provides the concrete implementation to execute the "Get Messages By Chat Id" operation asynchronously, returning `Task<List>Message<>`. It accepts `chatId: int` as input parameters. |
| 5 | GetParticipantAsync(chatId: int, accountId: int) : Task&lt;ChatParticipant?&gt; | Provides the concrete implementation to execute the "Get Participant" operation asynchronously, returning `Task<ChatParticipant?>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 6 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 7 | GetParticipantsByAccountIdAsync(accountId: int) : Task&lt;List&gt;ChatParticipant&lt;&gt; | Provides the concrete implementation to execute the "Get Participants By Account Id" operation asynchronously, returning `Task<List>ChatParticipant<>`. It accepts `accountId: int` as input parameters. |
| 8 | HasActiveReportForChatAsync(chatId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Has Active Report For Chat" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int` as input parameters. |
| 9 | IsParticipantAsync(chatId: int, accountId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Participant" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 10 | MarkAsReadAsync(chatId: int, accountId: int) : Task | Provides the concrete implementation to execute the "Mark As Read" operation asynchronously, returning `Task`. It accepts `chatId: int, accountId: int` as input parameters. |
| 11 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 12 | SearchParticipantsByAccountIdAsync(accountId: int, query: string) : Task&lt;List&gt;ChatParticipant&lt;&gt; | Provides the concrete implementation to execute the "Search Participants By Account Id" operation asynchronously, returning `Task<List>ChatParticipant<>`. It accepts `accountId: int, query: string` as input parameters. |
| 13 | UpdateChatAsync(chat: Chat) : Task | Provides the concrete implementation to execute the "Update Chat" operation asynchronously, returning `Task`. It accepts `chat: Chat` as input parameters. |
| 14 | UpdateParticipantAsync(participant: ChatParticipant) : Task | Provides the concrete implementation to execute the "Update Participant" operation asynchronously, returning `Task`. It accepts `participant: ChatParticipant` as input parameters. |
| 15 | _context : AppDbContext | Provides the Context dependency. |

### 5. CheckoutRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddOrderAsync(order: OrderInfo) : Task | Provides the concrete implementation to execute the "Add Order" operation asynchronously, returning `Task`. It accepts `order: OrderInfo` as input parameters. |
| 2 | AddOrderItemAsync(item: OrderItem) : Task | Provides the concrete implementation to execute the "Add Order Item" operation asynchronously, returning `Task`. It accepts `item: OrderItem` as input parameters. |
| 3 | AddTransactionAsync(transaction: Transaction) : Task | Provides the concrete implementation to execute the "Add Transaction" operation asynchronously, returning `Task`. It accepts `transaction: Transaction` as input parameters. |
| 4 | BeginTransactionAsync() : Task&lt;IDbContextTransaction&gt; | Provides the concrete implementation to execute the "Begin Transaction" operation asynchronously, returning `Task<IDbContextTransaction>`. |
| 5 | GetCartItemsWithCourseAndInstructorAsync(userId: string) : Task&lt;List&gt;CartItem&lt;&gt; | Provides the concrete implementation to execute the "Get Cart Items With Course And Instructor" operation asynchronously, returning `Task<List>CartItem<>`. It accepts `userId: string` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

### 6. CouponRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(coupon: Coupon) : Task&lt;void&gt; | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task<void>`. It accepts `coupon: Coupon` as input parameters. |
| 2 | GetActiveAvailableCouponsAsync(now: DateTime) : Task&lt;List&gt;Coupon&lt;&gt; | Provides the concrete implementation to execute the "Get Active Available Coupons" operation asynchronously, returning `Task<List>Coupon<>`. It accepts `now: DateTime` as input parameters. |
| 3 | GetAllAsync(filterId: int?, isActive: bool?, type: string?, search: string?) : Task&lt;List&gt;Coupon&lt;&gt; | Provides the concrete implementation to execute the "Get All" operation asynchronously, returning `Task<List>Coupon<>`. It accepts `filterId: int?, isActive: bool?, type: string?, search: string?` as input parameters. |
| 4 | GetByCodeAsync(code: string) : Task&lt;Coupon?&gt; | Provides the concrete implementation to execute the "Get By Code" operation asynchronously, returning `Task<Coupon?>`. It accepts `code: string` as input parameters. |
| 5 | GetByIdAsync(id: int, filterId: int?) : Task&lt;Coupon?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Coupon?>`. It accepts `id: int, filterId: int?` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 7 | Update(coupon: Coupon) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `coupon: Coupon` as input parameters. |
| 8 | _context : AppDbContext | Provides the Context dependency. |

### 7. CourseAiUsageLogRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;CourseAiUsageLog&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>CourseAiUsageLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 2 | _context : AppDbContext | Provides the Context dependency. |

### 8. CourseRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(course: Course) : Task | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task`. It accepts `course: Course` as input parameters. |
| 2 | GetActiveEnrollmentAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Provides the concrete implementation to execute the "Get Active Enrollment" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetAllPublishedCoursesPagedAsync(search: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?) : Task&lt;(IEnumerable&gt;Course&lt; Courses, int TotalCount)&gt; | Provides the concrete implementation to execute the "Get All Published Courses Paged" operation asynchronously, returning `Task<(IEnumerable>Course< Courses, int TotalCount)>`. It accepts `search: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?` as input parameters. |
| 4 | GetByIdAsync(id: int) : Task&lt;Course?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Course?>`. It accepts `id: int` as input parameters. |
| 5 | GetCourseStatsAsync(courseId: int) : Task&lt;CourseStats?&gt; | Provides the concrete implementation to execute the "Get Course Stats" operation asynchronously, returning `Task<CourseStats?>`. It accepts `courseId: int` as input parameters. |
| 6 | GetCourseWithDetailsAsync(courseId: int) : Task&lt;Course?&gt; | Provides the concrete implementation to execute the "Get Course With Details" operation asynchronously, returning `Task<Course?>`. It accepts `courseId: int` as input parameters. |
| 7 | GetInstructorCoursesPagedAsync(instructorId: int, search: string?, status: string?, page: int?, pageSize: int?) : Task&lt;(IEnumerable&gt;Course&lt; Courses, int TotalCount)&gt; | Provides the concrete implementation to execute the "Get Instructor Courses Paged" operation asynchronously, returning `Task<(IEnumerable>Course< Courses, int TotalCount)>`. It accepts `instructorId: int, search: string?, status: string?, page: int?, pageSize: int?` as input parameters. |
| 8 | GetPendingCoursesModerationAsync(filter: ModerationFilterDto) : Task&lt;PagedResult&gt;CourseModerationDto&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Courses Moderation" operation asynchronously, returning `Task<PagedResult>CourseModerationDto<>`. It accepts `filter: ModerationFilterDto` as input parameters. |
| 9 | IsEnrolledAsync(accountId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Enrolled" operation asynchronously, returning `Task<bool>`. It accepts `accountId: int, courseId: int` as input parameters. |
| 10 | IsOwnerAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Owner" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 11 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 12 | Update(course: Course) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `course: Course` as input parameters. |
| 13 | _context : AppDbContext | Provides the Context dependency. |

### 9. CourseReviewModerationLogRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;CourseReviewModerationLog&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>CourseReviewModerationLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 2 | _context : AppDbContext | Provides the Context dependency. |

### 10. EnrollmentRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddEnrollmentAsync(enrollment: Enrollment) : Task | Provides the concrete implementation to execute the "Add Enrollment" operation asynchronously, returning `Task`. It accepts `enrollment: Enrollment` as input parameters. |
| 2 | BeginTransactionAsync() : Task&lt;IDbContextTransaction&gt; | Provides the concrete implementation to execute the "Begin Transaction" operation asynchronously, returning `Task<IDbContextTransaction>`. |
| 3 | GetCompletedMaterialCountAsync(enrollmentId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Completed Material Count" operation asynchronously, returning `Task<int>`. It accepts `enrollmentId: int` as input parameters. |
| 4 | GetCompletedMaterialIdsAsync(enrollmentId: int) : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get Completed Material Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `enrollmentId: int` as input parameters. |
| 5 | GetEnrolledUserIdsAsync(courseId: int) : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get Enrolled User Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `courseId: int` as input parameters. |
| 6 | GetEnrollmentAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Provides the concrete implementation to execute the "Get Enrollment" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 7 | GetEnrollmentWithProgressAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Provides the concrete implementation to execute the "Get Enrollment With Progress" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 8 | GetMyEnrolledCoursesAsync(userId: int) : Task&lt;List&gt;Enrollment&lt;&gt; | Provides the concrete implementation to execute the "Get My Enrolled Courses" operation asynchronously, returning `Task<List>Enrollment<>`. It accepts `userId: int` as input parameters. |
| 9 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 10 | _context : AppDbContext | Provides the Context dependency. |

### 11. GiftRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetByOrderItemIdAsync(orderItemId: int) : Task&lt;Gift?&gt; | Provides the concrete implementation to execute the "Get By Order Item Id" operation asynchronously, returning `Task<Gift?>`. It accepts `orderItemId: int` as input parameters. |
| 2 | GetByTokenAsync(token: string) : Task&lt;Gift?&gt; | Provides the concrete implementation to execute the "Get By Token" operation asynchronously, returning `Task<Gift?>`. It accepts `token: string` as input parameters. |
| 3 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 4 | Update(gift: Gift) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `gift: Gift` as input parameters. |
| 5 | _context : AppDbContext | Provides the Context dependency. |

### 12. InstructorRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(instructor: Instructor) : Task | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task`. It accepts `instructor: Instructor` as input parameters. |
| 2 | CountActiveCoursesAsync(instructorId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Count Active Courses" operation asynchronously, returning `Task<int>`. It accepts `instructorId: int` as input parameters. |
| 3 | GetByIdAsync(id: int) : Task&lt;Instructor?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Instructor?>`. It accepts `id: int` as input parameters. |
| 4 | GetByIdWithNavigationAsync(userId: int) : Task&lt;Instructor&gt; | Provides the concrete implementation to execute the "Get By Id With Navigation" operation asynchronously, returning `Task<Instructor>`. It accepts `userId: int` as input parameters. |
| 5 | GetDashboardDtoAsync(userId: int) : Task&lt;InstructorDashboardDto?&gt; | Provides the concrete implementation to execute the "Get Dashboard Dto" operation asynchronously, returning `Task<InstructorDashboardDto?>`. It accepts `userId: int` as input parameters. |
| 6 | GetEnrollmentGrowthAsync(instructorId: int) : Task&lt;double&gt; | Provides the concrete implementation to execute the "Get Enrollment Growth" operation asynchronously, returning `Task<double>`. It accepts `instructorId: int` as input parameters. |
| 7 | GetInstructorRankingPercentageAsync(instructorId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Instructor Ranking Percentage" operation asynchronously, returning `Task<int>`. It accepts `instructorId: int` as input parameters. |
| 8 | GetInstructorsWithStripeAsync() : Task&lt;List&gt;Instructor&lt;&gt; | Provides the concrete implementation to execute the "Get Instructors With Stripe" operation asynchronously, returning `Task<List>Instructor<>`. |
| 9 | GetPayoutsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;InstructorPayoutDto&lt;&gt; | Provides the concrete implementation to execute the "Get Payouts" operation asynchronously, returning `Task<PagedResult>InstructorPayoutDto<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 10 | GetPendingInstructorsAsync(page: int, pageSize: int) : Task&lt;(IEnumerable&gt;Instructor&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Pending Instructors" operation asynchronously, returning `Task<(IEnumerable>Instructor<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 11 | GetStatsAsync(userId: int) : Task&lt;InstructorStats?&gt; | Provides the concrete implementation to execute the "Get Stats" operation asynchronously, returning `Task<InstructorStats?>`. It accepts `userId: int` as input parameters. |
| 12 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 13 | Update(instructor: Instructor) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `instructor: Instructor` as input parameters. |
| 14 | _context : AppDbContext | Provides the Context dependency. |

### 13. LessonRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(lesson: Lesson) : Task | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task`. It accepts `lesson: Lesson` as input parameters. |
| 2 | GetByCourseIdAsync(courseId: int) : Task&lt;List&gt;Lesson&lt;&gt; | Provides the concrete implementation to execute the "Get By Course Id" operation asynchronously, returning `Task<List>Lesson<>`. It accepts `courseId: int` as input parameters. |
| 3 | GetByIdAsync(id: int) : Task&lt;Lesson?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Lesson?>`. It accepts `id: int` as input parameters. |
| 4 | GetByIdAsync(lessonId: int) : Task&lt;Lesson?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Lesson?>`. It accepts `lessonId: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(lesson: Lesson) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `lesson: Lesson` as input parameters. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

### 14. LessonReviewModerationLogRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;LessonReviewModerationLog&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>LessonReviewModerationLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 2 | _context : AppDbContext | Provides the Context dependency. |

### 15. LockoutRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(lockout: Lockout) : Task&lt;void&gt; | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task<void>`. It accepts `lockout: Lockout` as input parameters. |
| 2 | GetActiveLockoutAsync(accountId: int, lockoutType: string) : Task&lt;Lockout?&gt; | Provides the concrete implementation to execute the "Get Active Lockout" operation asynchronously, returning `Task<Lockout?>`. It accepts `accountId: int, lockoutType: string` as input parameters. |
| 3 | RemoveAccountLockoutsAsync(accountId: int) : Task&lt;void&gt; | Provides the concrete implementation to execute the "Remove Account Lockouts" operation asynchronously, returning `Task<void>`. It accepts `accountId: int` as input parameters. |
| 4 | _context : AppDbContext | Provides the Context dependency. |

### 16. ManagerRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetManagerByIdAsync(managerId: int) : Task&lt;Manager?&gt; | Provides the concrete implementation to execute the "Get Manager By Id" operation asynchronously, returning `Task<Manager?>`. It accepts `managerId: int` as input parameters. |
| 2 | UpdateManagerProfileAsync(managerId: int, displayName: string, fullName: string?, phoneNumber: string?, avatarUrl: string?, bio: string?) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Manager Profile" operation asynchronously, returning `Task<bool>`. It accepts `managerId: int, displayName: string, fullName: string?, phoneNumber: string?, avatarUrl: string?, bio: string?` as input parameters. |
| 3 | _context : AppDbContext | Provides the Context dependency. |

### 17. MaterialRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Delete(material: LearningMaterial) | Provides the concrete implementation to execute the "Delete" operation, returning `void`. It accepts `material: LearningMaterial` as input parameters. |
| 2 | GetByCourseIdAsync(courseId: int) : Task&lt;List&gt;LearningMaterial&lt;&gt; | Provides the concrete implementation to execute the "Get By Course Id" operation asynchronously, returning `Task<List>LearningMaterial<>`. It accepts `courseId: int` as input parameters. |
| 3 | GetByIdAsync(materialId: int) : Task&lt;LearningMaterial?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<LearningMaterial?>`. It accepts `materialId: int` as input parameters. |
| 4 | GetTrashMaterialsAsync(instructorId: int) : Task&lt;List&gt;LearningMaterial&lt;&gt; | Provides the concrete implementation to execute the "Get Trash Materials" operation asynchronously, returning `Task<List>LearningMaterial<>`. It accepts `instructorId: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(material: LearningMaterial) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `material: LearningMaterial` as input parameters. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

### 18. NotificationRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(notification: Notification) : Task | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task`. It accepts `notification: Notification` as input parameters. |
| 2 | AddRangeAsync(notifications: IEnumerable&lt;Notification&gt;) : Task | Provides the concrete implementation to execute the "Add Range" operation asynchronously, returning `Task`. It accepts `notifications: IEnumerable<Notification>` as input parameters. |
| 3 | AutoCleanupAdminNotificationsAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Auto Cleanup Admin Notifications" operation asynchronously, returning `Task<int>`. |
| 4 | Delete(notification: Notification) | Provides the concrete implementation to execute the "Delete" operation, returning `void`. It accepts `notification: Notification` as input parameters. |
| 5 | GetAllAsync(page: int, pageSize: int) : Task&lt;(List&gt;Notification&lt; Items, int TotalCount)&gt; | Provides the concrete implementation to execute the "Get All" operation asynchronously, returning `Task<(List>Notification< Items, int TotalCount)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 6 | GetByIdAsync(id: int) : Task&lt;Notification?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Notification?>`. It accepts `id: int` as input parameters. |
| 7 | GetByReceiverIdAsync(userId: int, page: int, pageSize: int) : Task&lt;(List&gt;Notification&lt; Items, int TotalCount)&gt; | Provides the concrete implementation to execute the "Get By Receiver Id" operation asynchronously, returning `Task<(List>Notification< Items, int TotalCount)>`. It accepts `userId: int, page: int, pageSize: int` as input parameters. |
| 8 | GetSentNotificationsCountAsync(id: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Sent Notifications Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 9 | GetSentNotificationsCountAsync(senderId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Sent Notifications Count" operation asynchronously, returning `Task<int>`. It accepts `senderId: int` as input parameters. |
| 10 | GetUnreadByReceiverIdAsync(userId: int) : Task&lt;List&gt;Notification&lt;&gt; | Provides the concrete implementation to execute the "Get Unread By Receiver Id" operation asynchronously, returning `Task<List>Notification<>`. It accepts `userId: int` as input parameters. |
| 11 | GetUnreadCountAsync(userId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `userId: int` as input parameters. |
| 12 | MarkAsReadAsync(notificationId: int, userId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Mark As Read" operation asynchronously, returning `Task<bool>`. It accepts `notificationId: int, userId: int` as input parameters. |
| 13 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 14 | Update(notification: Notification) | Provides the concrete implementation to execute the "Update" operation, returning `void`. It accepts `notification: Notification` as input parameters. |
| 15 | UpdateRange(notifications: IEnumerable&lt;Notification&gt;) | Provides the concrete implementation to execute the "Update Range" operation, returning `void`. It accepts `notifications: IEnumerable<Notification>` as input parameters. |
| 16 | _context : AppDbContext | Provides the Context dependency. |

### 19. QuestionBankRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuestionAsync(question: QuizQuestion) : Task&lt;QuizQuestion&gt; | Provides the concrete implementation to execute the "Add Question" operation asynchronously, returning `Task<QuizQuestion>`. It accepts `question: QuizQuestion` as input parameters. |
| 2 | DeleteQuestionAsync(questionId: int) : Task | Provides the concrete implementation to execute the "Delete Question" operation asynchronously, returning `Task`. It accepts `questionId: int` as input parameters. |
| 3 | GetQuestionByIdAsync(questionId: int) : Task&lt;QuizQuestion?&gt; | Provides the concrete implementation to execute the "Get Question By Id" operation asynchronously, returning `Task<QuizQuestion?>`. It accepts `questionId: int` as input parameters. |
| 4 | GetQuestionsByCourseAsync(courseId: int) : Task&lt;List&gt;QuizQuestion&lt;&gt; | Provides the concrete implementation to execute the "Get Questions By Course" operation asynchronously, returning `Task<List>QuizQuestion<>`. It accepts `courseId: int` as input parameters. |
| 5 | GetQuestionsByLessonAsync(lessonId: int) : Task&lt;List&gt;QuizQuestion&lt;&gt; | Provides the concrete implementation to execute the "Get Questions By Lesson" operation asynchronously, returning `Task<List>QuizQuestion<>`. It accepts `lessonId: int` as input parameters. |
| 6 | UpdateQuestionAsync(question: QuizQuestion) : Task&lt;QuizQuestion&gt; | Provides the concrete implementation to execute the "Update Question" operation asynchronously, returning `Task<QuizQuestion>`. It accepts `question: QuizQuestion` as input parameters. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

### 20. QuizRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseQuizAsync(courseQuiz: CourseQuiz) : Task | Provides the concrete implementation to execute the "Add Course Quiz" operation asynchronously, returning `Task`. It accepts `courseQuiz: CourseQuiz` as input parameters. |
| 2 | CreateAsync(quiz: Quiz) : Task&lt;Quiz&gt; | Provides the concrete implementation to execute the "Create" operation asynchronously, returning `Task<Quiz>`. It accepts `quiz: Quiz` as input parameters. |
| 3 | GetAttemptByIdAsync(attemptId: int) : Task&lt;QuizAttempt?&gt; | Provides the concrete implementation to execute the "Get Attempt By Id" operation asynchronously, returning `Task<QuizAttempt?>`. It accepts `attemptId: int` as input parameters. |
| 4 | GetAttemptsByQuizAndUserAsync(quizId: int, userId: int, page: int, pageSize: int) : Task&lt;List&gt;QuizAttempt&lt;&gt; | Provides the concrete implementation to execute the "Get Attempts By Quiz And User" operation asynchronously, returning `Task<List>QuizAttempt<>`. It accepts `quizId: int, userId: int, page: int, pageSize: int` as input parameters. |
| 5 | GetByIdAsync(quizId: int) : Task&lt;Quiz?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<Quiz?>`. It accepts `quizId: int` as input parameters. |
| 6 | GetByInstructorAsync(instructorId: int) : Task&lt;List&gt;Quiz&lt;&gt; | Provides the concrete implementation to execute the "Get By Instructor" operation asynchronously, returning `Task<List>Quiz<>`. It accepts `instructorId: int` as input parameters. |
| 7 | GetQuizWithQuestionsAsync(quizId: int) : Task&lt;Quiz?&gt; | Provides the concrete implementation to execute the "Get Quiz With Questions" operation asynchronously, returning `Task<Quiz?>`. It accepts `quizId: int` as input parameters. |
| 8 | SaveAttemptAsync(attempt: QuizAttempt) : Task&lt;QuizAttempt&gt; | Provides the concrete implementation to execute the "Save Attempt" operation asynchronously, returning `Task<QuizAttempt>`. It accepts `attempt: QuizAttempt` as input parameters. |
| 9 | SoftDeleteAsync(quizId: int) : Task | Provides the concrete implementation to execute the "Soft Delete" operation asynchronously, returning `Task`. It accepts `quizId: int` as input parameters. |
| 10 | UpdateAsync(quiz: Quiz) : Task | Provides the concrete implementation to execute the "Update" operation asynchronously, returning `Task`. It accepts `quiz: Quiz` as input parameters. |
| 11 | _context : AppDbContext | Provides the Context dependency. |

### 21. ReportRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseReportAsync(report: CourseReport) : Task | Provides the concrete implementation to execute the "Add Course Report" operation asynchronously, returning `Task`. It accepts `report: CourseReport` as input parameters. |
| 2 | AddCourseReviewReportAsync(report: CourseReviewReport) : Task | Provides the concrete implementation to execute the "Add Course Review Report" operation asynchronously, returning `Task`. It accepts `report: CourseReviewReport` as input parameters. |
| 3 | AddLessonReviewReportAsync(report: LessonReviewReport) : Task | Provides the concrete implementation to execute the "Add Lesson Review Report" operation asynchronously, returning `Task`. It accepts `report: LessonReviewReport` as input parameters. |
| 4 | CountCourseReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Count Course Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 5 | CountCourseReviewReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Count Course Review Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 6 | CountLessonReviewReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Count Lesson Review Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 7 | GetAllCourseReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;CourseReport&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get All Course Reports" operation asynchronously, returning `Task<Tuple>List<CourseReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 8 | GetAllCourseReviewReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;CourseReviewReport&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get All Course Review Reports" operation asynchronously, returning `Task<Tuple>List<CourseReviewReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 9 | GetAllLessonReviewReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;LessonReviewReport&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get All Lesson Review Reports" operation asynchronously, returning `Task<Tuple>List<LessonReviewReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 10 | GetCourseReportByIdAsync(reportId: int) : Task&lt;CourseReport?&gt; | Provides the concrete implementation to execute the "Get Course Report By Id" operation asynchronously, returning `Task<CourseReport?>`. It accepts `reportId: int` as input parameters. |
| 11 | GetCourseReviewReportByIdAsync(reportId: int) : Task&lt;CourseReviewReport?&gt; | Provides the concrete implementation to execute the "Get Course Review Report By Id" operation asynchronously, returning `Task<CourseReviewReport?>`. It accepts `reportId: int` as input parameters. |
| 12 | GetLessonReviewReportByIdAsync(reportId: int) : Task&lt;LessonReviewReport?&gt; | Provides the concrete implementation to execute the "Get Lesson Review Report By Id" operation asynchronously, returning `Task<LessonReviewReport?>`. It accepts `reportId: int` as input parameters. |
| 13 | GetPendingCourseReportAsync(reporterId: int, courseId: int, reason: string) : Task&lt;CourseReport?&gt; | Provides the concrete implementation to execute the "Get Pending Course Report" operation asynchronously, returning `Task<CourseReport?>`. It accepts `reporterId: int, courseId: int, reason: string` as input parameters. |
| 14 | GetPendingCourseReviewReportAsync(reporterId: int, courseReviewId: int, reason: string) : Task&lt;CourseReviewReport?&gt; | Provides the concrete implementation to execute the "Get Pending Course Review Report" operation asynchronously, returning `Task<CourseReviewReport?>`. It accepts `reporterId: int, courseReviewId: int, reason: string` as input parameters. |
| 15 | GetPendingLessonReviewReportAsync(reporterId: int, lessonReviewId: int, reason: string) : Task&lt;LessonReviewReport?&gt; | Provides the concrete implementation to execute the "Get Pending Lesson Review Report" operation asynchronously, returning `Task<LessonReviewReport?>`. It accepts `reporterId: int, lessonReviewId: int, reason: string` as input parameters. |
| 16 | GetResolvedReportsCountAsync(id: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Resolved Reports Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 17 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 18 | UpdateCourseReport(report: CourseReport) | Provides the concrete implementation to execute the "Update Course Report" operation, returning `void`. It accepts `report: CourseReport` as input parameters. |
| 19 | UpdateCourseReviewReport(report: CourseReviewReport) | Provides the concrete implementation to execute the "Update Course Review Report" operation, returning `void`. It accepts `report: CourseReviewReport` as input parameters. |
| 20 | UpdateLessonReviewReport(report: LessonReviewReport) | Provides the concrete implementation to execute the "Update Lesson Review Report" operation, returning `void`. It accepts `report: LessonReviewReport` as input parameters. |
| 21 | _context : AppDbContext | Provides the Context dependency. |

### 22. ReviewRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseReviewAsync(review: CourseReview) : Task | Provides the concrete implementation to execute the "Add Course Review" operation asynchronously, returning `Task`. It accepts `review: CourseReview` as input parameters. |
| 2 | AddLessonReviewAsync(review: LessonReview) : Task | Provides the concrete implementation to execute the "Add Lesson Review" operation asynchronously, returning `Task`. It accepts `review: LessonReview` as input parameters. |
| 3 | GetCourseReviewByIdAsync(reviewId: int) : Task&lt;CourseReview?&gt; | Provides the concrete implementation to execute the "Get Course Review By Id" operation asynchronously, returning `Task<CourseReview?>`. It accepts `reviewId: int` as input parameters. |
| 4 | GetCourseReviewsWithDetailsAsync(courseId: int, page: int, pageSize: int, starFilter: int?) : Task&lt;(List&gt;CourseReview&lt; Items, int TotalCount)&gt; | Provides the concrete implementation to execute the "Get Course Reviews With Details" operation asynchronously, returning `Task<(List>CourseReview< Items, int TotalCount)>`. It accepts `courseId: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 5 | GetLessonReviewByIdAsync(reviewId: int) : Task&lt;LessonReview?&gt; | Provides the concrete implementation to execute the "Get Lesson Review By Id" operation asynchronously, returning `Task<LessonReview?>`. It accepts `reviewId: int` as input parameters. |
| 6 | HasPendingCourseReviewReportsAsync(reviewId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Has Pending Course Review Reports" operation asynchronously, returning `Task<bool>`. It accepts `reviewId: int` as input parameters. |
| 7 | HasPendingLessonReviewReportsAsync(reviewId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Has Pending Lesson Review Reports" operation asynchronously, returning `Task<bool>`. It accepts `reviewId: int` as input parameters. |
| 8 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 9 | UpdateCourseReview(review: CourseReview) | Provides the concrete implementation to execute the "Update Course Review" operation, returning `void`. It accepts `review: CourseReview` as input parameters. |
| 10 | UpdateLessonReview(review: LessonReview) | Provides the concrete implementation to execute the "Update Lesson Review" operation, returning `void`. It accepts `review: LessonReview` as input parameters. |
| 11 | _context : AppDbContext | Provides the Context dependency. |

### 23. SystemConfigRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAllConfigAsync() : Task&lt;List&gt;Dictionary&lt;string, object&gt;&lt;&gt; | Provides the concrete implementation to execute the "Get All Config" operation asynchronously, returning `Task<List>Dictionary<string, object><>`. |
| 2 | GetValueAsync(key: string) : Task&lt;string?&gt; | Provides the concrete implementation to execute the "Get Value" operation asynchronously, returning `Task<string?>`. It accepts `key: string` as input parameters. |
| 3 | GetValueAsync(key: string) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Get Value" operation asynchronously, returning `Task<string>`. It accepts `key: string` as input parameters. |
| 4 | SetConfigByKeyAsync(key: string, value: Dictionary&lt;string, object&gt;) : Task | Provides the concrete implementation to execute the "Set Config By Key" operation asynchronously, returning `Task`. It accepts `key: string, value: Dictionary<string, object>` as input parameters. |
| 5 | UpsertConfigAsync(configKey: string, configValue: string, description: string?) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Upsert Config" operation asynchronously, returning `Task<int>`. It accepts `configKey: string, configValue: string, description: string?` as input parameters. |
| 6 | _context : AppDbContext | Provides the Context dependency. |

### 24. TransactionRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAccountTransactionsSummaryAsync(id: int) : Task&lt;AccountTransactionSummaryDto&gt; | Provides the concrete implementation to execute the "Get Account Transactions Summary" operation asynchronously, returning `Task<AccountTransactionSummaryDto>`. It accepts `id: int` as input parameters. |
| 2 | GetInstructorTotalRevenueAsync(id: int) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Instructor Total Revenue" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 3 | GetInstructorTotalWithdrawnAsync(id: int) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Instructor Total Withdrawn" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 4 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;(List&gt;TransactionListDto&lt;, int)&gt; | Provides the concrete implementation to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<(List>TransactionListDto<, int)>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 5 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 6 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;ValueTuple&gt;List&lt;TransactionListDto&gt;,int&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<ValueTuple>List<TransactionListDto>,int<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 7 | GetTotalSpentAsync(id: int) : Task&lt;decimal&gt; | Provides the concrete implementation to execute the "Get Total Spent" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 8 | GetTransactionDetailAsync(transactionId: int) : Task&lt;TransactionDetailDto?&gt; | Provides the concrete implementation to execute the "Get Transaction Detail" operation asynchronously, returning `Task<TransactionDetailDto?>`. It accepts `transactionId: int` as input parameters. |
| 9 | GetTransactionsAsync(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;ValueTuple&gt;List&lt;TransactionListDto&gt;,int&lt;&gt; | Provides the concrete implementation to execute the "Get Transactions" operation asynchronously, returning `Task<ValueTuple>List<TransactionListDto>,int<>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 10 | GetUserTransactionsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Provides the concrete implementation to execute the "Get User Transactions" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |
| 11 | RejectPendingRefundForGiftClaimedAsync(orderItemId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Reject Pending Refund For Gift Claimed" operation asynchronously, returning `Task<bool>`. It accepts `orderItemId: int` as input parameters. |
| 12 | _context : AppDbContext | Provides the Context dependency. |

### 25. UserLockoutRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetActiveLockoutAsync(userId: int, type: string) : Task&lt;UserLockout?&gt; | Provides the concrete implementation to execute the "Get Active Lockout" operation asynchronously, returning `Task<UserLockout?>`. It accepts `userId: int, type: string` as input parameters. |
| 2 | _context : AppDbContext | Provides the Context dependency. |

### 26. UserRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAccountByEmailAsync(email: string) : Task&lt;Account?&gt; | Provides the concrete implementation to execute the "Get Account By Email" operation asynchronously, returning `Task<Account?>`. It accepts `email: string` as input parameters. |
| 2 | GetAccountByEmailOrUsernameAsync(usernameOrEmail: string) : Task&lt;Account?&gt; | Provides the concrete implementation to execute the "Get Account By Email Or Username" operation asynchronously, returning `Task<Account?>`. It accepts `usernameOrEmail: string` as input parameters. |
| 3 | GetAccountByIdAsync(accountId: int) : Task&lt;Account&gt; | Provides the concrete implementation to execute the "Get Account By Id" operation asynchronously, returning `Task<Account>`. It accepts `accountId: int` as input parameters. |
| 4 | GetAccountsPagedAsync(keyword: string?, role: string?, page: int, pageSize: int) : Task&lt;PagedResult&gt;Account&lt;&gt; | Provides the concrete implementation to execute the "Get Accounts Paged" operation asynchronously, returning `Task<PagedResult>Account<>`. It accepts `keyword: string?, role: string?, page: int, pageSize: int` as input parameters. |
| 5 | GetAllUserIdsAsync() : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get All User Ids" operation asynchronously, returning `Task<List>int<>`. |
| 6 | GetByIdAsync(id: int) : Task&lt;User?&gt; | Provides the concrete implementation to execute the "Get By Id" operation asynchronously, returning `Task<User?>`. It accepts `id: int` as input parameters. |
| 7 | GetChatByIdAsync(chatId: int) : Task&lt;Chat?&gt; | Provides the concrete implementation to execute the "Get Chat By Id" operation asynchronously, returning `Task<Chat?>`. It accepts `chatId: int` as input parameters. |
| 8 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 9 | GetRoleByAccountIdAsync(accountId: int) : Task&lt;string?&gt; | Provides the concrete implementation to execute the "Get Role By Account Id" operation asynchronously, returning `Task<string?>`. It accepts `accountId: int` as input parameters. |
| 10 | GetRoleByAccountIdAsync(accountId: int) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Get Role By Account Id" operation asynchronously, returning `Task<string>`. It accepts `accountId: int` as input parameters. |
| 11 | GetUserByIdAsync(userId: int) : Task&lt;User?&gt; | Provides the concrete implementation to execute the "Get User By Id" operation asynchronously, returning `Task<User?>`. It accepts `userId: int` as input parameters. |
| 12 | GetUserIdsForAdminSenderAsync() : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get User Ids For Admin Sender" operation asynchronously, returning `Task<List>int<>`. |
| 13 | GetUserIdsForStaffSenderAsync() : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get User Ids For Staff Sender" operation asynchronously, returning `Task<List>int<>`. |
| 14 | IsEmailExistsAsync(email: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Email Exists" operation asynchronously, returning `Task<bool>`. It accepts `email: string` as input parameters. |
| 15 | IsUsernameExistsAsync(username: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Username Exists" operation asynchronously, returning `Task<bool>`. It accepts `username: string` as input parameters. |
| 16 | RegisterManagerAsync(account: Account, manager: Manager) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Register Manager" operation asynchronously, returning `Task<bool>`. It accepts `account: Account, manager: Manager` as input parameters. |
| 17 | RegisterUserAsync(account: Account, user: User) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Register User" operation asynchronously, returning `Task<bool>`. It accepts `account: Account, user: User` as input parameters. |
| 18 | RevokeRefreshTokenAsync(accountId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Revoke Refresh Token" operation asynchronously, returning `Task<int>`. It accepts `accountId: int` as input parameters. |
| 19 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 20 | SaveRefreshTokenAsync(accountId: int, refreshToken: string) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Refresh Token" operation asynchronously, returning `Task<int>`. It accepts `accountId: int, refreshToken: string` as input parameters. |
| 21 | SearchEmailsByQueryAsync(query: string, senderId: int, senderRole: string, take: int) : Task&lt;List&gt;string&lt;&gt; | Provides the concrete implementation to execute the "Search Emails By Query" operation asynchronously, returning `Task<List>string<>`. It accepts `query: string, senderId: int, senderRole: string, take: int` as input parameters. |
| 22 | UpdateAccountAsync(account: Account) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Update Account" operation asynchronously, returning `Task<int>`. It accepts `account: Account` as input parameters. |
| 23 | UpdateAccountAsync(account: Account) | Provides the concrete implementation to execute the "Update Account" operation asynchronously, returning `void`. It accepts `account: Account` as input parameters. |
| 24 | UpdateChatAsync(chat: Chat) : Task | Provides the concrete implementation to execute the "Update Chat" operation asynchronously, returning `Task`. It accepts `chat: Chat` as input parameters. |
| 25 | UpdateEmailVerifiedAsync(email: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Email Verified" operation asynchronously, returning `Task<bool>`. It accepts `email: string` as input parameters. |
| 26 | UpdateLastLoginAsync(accountId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Update Last Login" operation asynchronously, returning `Task<int>`. It accepts `accountId: int` as input parameters. |
| 27 | UpdateUserProfileAsync(userId: int, fullName: string, bio: string, dob: DateTime?, avatarUrl: string?, phoneNumber: string?, email: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update User Profile" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, fullName: string, bio: string, dob: DateTime?, avatarUrl: string?, phoneNumber: string?, email: string` as input parameters. |
| 28 | _context : AppDbContext | Provides the Context dependency. |

### 27. WishlistRepository Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(newItem: WishlistItem) : Task | Provides the concrete implementation to execute the "Add" operation asynchronously, returning `Task`. It accepts `newItem: WishlistItem` as input parameters. |
| 2 | ExistsAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Exists" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetByUserAndCourseAsync(userId: int, courseId: int) : Task&lt;WishlistItem?&gt; | Provides the concrete implementation to execute the "Get By User And Course" operation asynchronously, returning `Task<WishlistItem?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 4 | GetByUserIdAsync(userId: int) : Task&lt;List&gt;WishlistItem&lt;&gt; | Provides the concrete implementation to execute the "Get By User Id" operation asynchronously, returning `Task<List>WishlistItem<>`. It accepts `userId: int` as input parameters. |
| 5 | RemoveAsync(item: WishlistItem) | Provides the concrete implementation to execute the "Remove" operation asynchronously, returning `void`. It accepts `item: WishlistItem` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Provides the concrete implementation to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 7 | _context : AppDbContext | Provides the Context dependency. |

## Layer: Repository Interface
### 1. IAdminFinanceRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddWithdrawalAsync(withdrawal: PlatformWithdrawal) : Task&lt;int&gt; | Defines the contract to execute the "Add Withdrawal" operation asynchronously, returning `Task<int>`. It accepts `withdrawal: PlatformWithdrawal` as input parameters. |
| 2 | GetGrossRevenueAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Gross Revenue" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 3 | GetInstructorCourseRevenuesByInstructorAsync(instructorId: int, year: int, month: int) : Task&lt;List&gt;InstructorCourseRevenueProjection&lt;&gt; | Defines the contract to execute the "Get Instructor Course Revenues By Instructor" operation asynchronously, returning `Task<List>InstructorCourseRevenueProjection<>`. It accepts `instructorId: int, year: int, month: int` as input parameters. |
| 4 | GetMaturedEscrowAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Matured Escrow" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 5 | GetPayoutDetailsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;(List&gt;PayoutDetailProjection&lt;, int)&gt; | Defines the contract to execute the "Get Payout Details" operation asynchronously, returning `Task<(List>PayoutDetailProjection<, int)>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 6 | GetPayoutDetailsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;PayoutDetailProjection&gt;, int&lt;&gt; | Defines the contract to execute the "Get Payout Details" operation asynchronously, returning `Task<Tuple>List<PayoutDetailProjection>, int<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 7 | GetPendingEscrowAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Pending Escrow" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 8 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Defines the contract to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 9 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;Transaction&gt;, int&lt;&gt; | Defines the contract to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<Tuple>List<Transaction>, int<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 10 | GetRefundEligibilityMetricsAsync(transactionId: int, studentId: int, courseId: int) : Task&lt;RefundEligibilityDto&gt; | Defines the contract to execute the "Get Refund Eligibility Metrics" operation asynchronously, returning `Task<RefundEligibilityDto>`. It accepts `transactionId: int, studentId: int, courseId: int` as input parameters. |
| 11 | GetStripeTransferIdByDestinationPaymentAsync(destinationPaymentId: string) : Task&lt;string?&gt; | Defines the contract to execute the "Get Stripe Transfer Id By Destination Payment" operation asynchronously, returning `Task<string?>`. It accepts `destinationPaymentId: string` as input parameters. |
| 12 | GetSucceededTransactionCountAsync(year: int?, month: int?) : Task&lt;int&gt; | Defines the contract to execute the "Get Succeeded Transaction Count" operation asynchronously, returning `Task<int>`. It accepts `year: int?, month: int?` as input parameters. |
| 13 | GetTotalPaidOutAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Total Paid Out" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 14 | GetTotalRefundedAsync(year: int?, month: int?) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Total Refunded" operation asynchronously, returning `Task<decimal>`. It accepts `year: int?, month: int?` as input parameters. |
| 15 | GetTransactionWithFullGraphAsync(transactionId: int) : Task&lt;Transaction?&gt; | Defines the contract to execute the "Get Transaction With Full Graph" operation asynchronously, returning `Task<Transaction?>`. It accepts `transactionId: int` as input parameters. |
| 16 | GetWithdrawalsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;PlatformWithdrawal&gt;, int&lt;&gt; | Defines the contract to execute the "Get Withdrawals" operation asynchronously, returning `Task<Tuple>List<PlatformWithdrawal>, int<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 17 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |

### 2. IAiModelRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | Add(model: AiModel) : AiModel | Defines the contract to execute the "Add" operation, returning `AiModel`. It accepts `model: AiModel` as input parameters. |
| 2 | GetAllAdminAsync() : Task&lt;List&gt;AiModel&lt;&gt; | Defines the contract to execute the "Get All Admin" operation asynchronously, returning `Task<List>AiModel<>`. |
| 3 | GetByIdAsync(id: int) : Task&lt;AiModel?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<AiModel?>`. It accepts `id: int` as input parameters. |
| 4 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;AiModel&lt;, int)&gt; | Defines the contract to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>AiModel<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(model: AiModel) : AiModel | Defines the contract to execute the "Update" operation, returning `AiModel`. It accepts `model: AiModel` as input parameters. |

### 3. ICartRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCartItemAsync(item: CartItem) : Task | Defines the contract to execute the "Add Cart Item" operation asynchronously, returning `Task`. It accepts `item: CartItem` as input parameters. |
| 2 | GetCartItemAsync(userId: int, courseId: int) : Task&lt;CartItem?&gt; | Defines the contract to execute the "Get Cart Item" operation asynchronously, returning `Task<CartItem?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetCartItemsWithDetailsAsync(userId: int) : Task&lt;List&gt;CartItem&lt;&gt; | Defines the contract to execute the "Get Cart Items With Details" operation asynchronously, returning `Task<List>CartItem<>`. It accepts `userId: int` as input parameters. |
| 4 | IsCourseInAnyCartAsync(courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Course In Any Cart" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int` as input parameters. |
| 5 | IsCourseInCartAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Course In Cart" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 6 | RemoveCartItem(item: CartItem) | Defines the contract to execute the "Remove Cart Item" operation, returning `void`. It accepts `item: CartItem` as input parameters. |
| 7 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |

### 4. IChatRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddMessageAsync(message: Message) : Task | Defines the contract to execute the "Add Message" operation asynchronously, returning `Task`. It accepts `message: Message` as input parameters. |
| 2 | GetActiveChatsCountAsync(id: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Active Chats Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 3 | GetChatByIdAsync(chatId: int) : Task&lt;Chat?&gt; | Defines the contract to execute the "Get Chat By Id" operation asynchronously, returning `Task<Chat?>`. It accepts `chatId: int` as input parameters. |
| 4 | GetMessagesByChatIdAsync(chatId: int) : Task&lt;List&gt;Message&lt;&gt; | Defines the contract to execute the "Get Messages By Chat Id" operation asynchronously, returning `Task<List>Message<>`. It accepts `chatId: int` as input parameters. |
| 5 | GetParticipantAsync(chatId: int, accountId: int) : Task&lt;ChatParticipant?&gt; | Defines the contract to execute the "Get Participant" operation asynchronously, returning `Task<ChatParticipant?>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 6 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 7 | GetParticipantsByAccountIdAsync(accountId: int) : Task&lt;List&gt;ChatParticipant&lt;&gt; | Defines the contract to execute the "Get Participants By Account Id" operation asynchronously, returning `Task<List>ChatParticipant<>`. It accepts `accountId: int` as input parameters. |
| 8 | HasActiveReportForChatAsync(chatId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Has Active Report For Chat" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int` as input parameters. |
| 9 | IsParticipantAsync(chatId: int, accountId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Participant" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 10 | MarkAsReadAsync(chatId: int, accountId: int) : Task | Defines the contract to execute the "Mark As Read" operation asynchronously, returning `Task`. It accepts `chatId: int, accountId: int` as input parameters. |
| 11 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 12 | SearchParticipantsByAccountIdAsync(accountId: int, query: string) : Task&lt;List&gt;ChatParticipant&lt;&gt; | Defines the contract to execute the "Search Participants By Account Id" operation asynchronously, returning `Task<List>ChatParticipant<>`. It accepts `accountId: int, query: string` as input parameters. |
| 13 | UpdateChatAsync(chat: Chat) : Task | Defines the contract to execute the "Update Chat" operation asynchronously, returning `Task`. It accepts `chat: Chat` as input parameters. |
| 14 | UpdateParticipantAsync(participant: ChatParticipant) : Task | Defines the contract to execute the "Update Participant" operation asynchronously, returning `Task`. It accepts `participant: ChatParticipant` as input parameters. |

### 5. ICheckoutRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddOrderAsync(order: OrderInfo) : Task | Defines the contract to execute the "Add Order" operation asynchronously, returning `Task`. It accepts `order: OrderInfo` as input parameters. |
| 2 | AddOrderItemAsync(item: OrderItem) : Task | Defines the contract to execute the "Add Order Item" operation asynchronously, returning `Task`. It accepts `item: OrderItem` as input parameters. |
| 3 | AddTransactionAsync(transaction: Transaction) : Task | Defines the contract to execute the "Add Transaction" operation asynchronously, returning `Task`. It accepts `transaction: Transaction` as input parameters. |
| 4 | BeginTransactionAsync() : Task&lt;IDbContextTransaction&gt; | Defines the contract to execute the "Begin Transaction" operation asynchronously, returning `Task<IDbContextTransaction>`. |
| 5 | GetCartItemsWithCourseAndInstructorAsync(userId: string) : Task&lt;List&gt;CartItem&lt;&gt; | Defines the contract to execute the "Get Cart Items With Course And Instructor" operation asynchronously, returning `Task<List>CartItem<>`. It accepts `userId: string` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |

### 6. ICouponRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(coupon: Coupon) : Task&lt;void&gt; | Defines the contract to execute the "Add" operation asynchronously, returning `Task<void>`. It accepts `coupon: Coupon` as input parameters. |
| 2 | GetActiveAvailableCouponsAsync(now: DateTime) : Task&lt;List&gt;Coupon&lt;&gt; | Defines the contract to execute the "Get Active Available Coupons" operation asynchronously, returning `Task<List>Coupon<>`. It accepts `now: DateTime` as input parameters. |
| 3 | GetAllAsync(filterId: int?, isActive: bool?, type: string?, search: string?) : Task&lt;List&gt;Coupon&lt;&gt; | Defines the contract to execute the "Get All" operation asynchronously, returning `Task<List>Coupon<>`. It accepts `filterId: int?, isActive: bool?, type: string?, search: string?` as input parameters. |
| 4 | GetByCodeAsync(code: string) : Task&lt;Coupon?&gt; | Defines the contract to execute the "Get By Code" operation asynchronously, returning `Task<Coupon?>`. It accepts `code: string` as input parameters. |
| 5 | GetByIdAsync(id: int, filterId: int?) : Task&lt;Coupon?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Coupon?>`. It accepts `id: int, filterId: int?` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 7 | Update(coupon: Coupon) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `coupon: Coupon` as input parameters. |

### 7. ICourseAiUsageLogRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;CourseAiUsageLog&lt;, int)&gt; | Defines the contract to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>CourseAiUsageLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |

### 8. ICourseRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(course: Course) : Task | Defines the contract to execute the "Add" operation asynchronously, returning `Task`. It accepts `course: Course` as input parameters. |
| 2 | GetActiveEnrollmentAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Defines the contract to execute the "Get Active Enrollment" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetAllPublishedCoursesPagedAsync(search: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?) : Task&lt;(IEnumerable&gt;Course&lt; Courses, int TotalCount)&gt; | Defines the contract to execute the "Get All Published Courses Paged" operation asynchronously, returning `Task<(IEnumerable>Course< Courses, int TotalCount)>`. It accepts `search: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?` as input parameters. |
| 4 | GetByIdAsync(id: int) : Task&lt;Course?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Course?>`. It accepts `id: int` as input parameters. |
| 5 | GetCourseStatsAsync(courseId: int) : Task&lt;CourseStats?&gt; | Defines the contract to execute the "Get Course Stats" operation asynchronously, returning `Task<CourseStats?>`. It accepts `courseId: int` as input parameters. |
| 6 | GetCourseWithDetailsAsync(courseId: int) : Task&lt;Course?&gt; | Defines the contract to execute the "Get Course With Details" operation asynchronously, returning `Task<Course?>`. It accepts `courseId: int` as input parameters. |
| 7 | GetInstructorCoursesPagedAsync(instructorId: int, search: string?, status: string?, page: int?, pageSize: int?) : Task&lt;(IEnumerable&gt;Course&lt; Courses, int TotalCount)&gt; | Defines the contract to execute the "Get Instructor Courses Paged" operation asynchronously, returning `Task<(IEnumerable>Course< Courses, int TotalCount)>`. It accepts `instructorId: int, search: string?, status: string?, page: int?, pageSize: int?` as input parameters. |
| 8 | GetPendingCoursesModerationAsync(filter: ModerationFilterDto) : Task&lt;PagedResult&gt;CourseModerationDto&lt;&gt; | Defines the contract to execute the "Get Pending Courses Moderation" operation asynchronously, returning `Task<PagedResult>CourseModerationDto<>`. It accepts `filter: ModerationFilterDto` as input parameters. |
| 9 | IsEnrolledAsync(accountId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Enrolled" operation asynchronously, returning `Task<bool>`. It accepts `accountId: int, courseId: int` as input parameters. |
| 10 | IsOwnerAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Owner" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 11 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 12 | Update(course: Course) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `course: Course` as input parameters. |

### 9. ICourseReviewModerationLogRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;CourseReviewModerationLog&lt;, int)&gt; | Defines the contract to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>CourseReviewModerationLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |

### 10. IEnrollmentRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddEnrollmentAsync(enrollment: Enrollment) : Task | Defines the contract to execute the "Add Enrollment" operation asynchronously, returning `Task`. It accepts `enrollment: Enrollment` as input parameters. |
| 2 | BeginTransactionAsync() : Task&lt;IDbContextTransaction&gt; | Defines the contract to execute the "Begin Transaction" operation asynchronously, returning `Task<IDbContextTransaction>`. |
| 3 | GetCompletedMaterialCountAsync(enrollmentId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Completed Material Count" operation asynchronously, returning `Task<int>`. It accepts `enrollmentId: int` as input parameters. |
| 4 | GetCompletedMaterialIdsAsync(enrollmentId: int) : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get Completed Material Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `enrollmentId: int` as input parameters. |
| 5 | GetEnrolledUserIdsAsync(courseId: int) : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get Enrolled User Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `courseId: int` as input parameters. |
| 6 | GetEnrollmentAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Defines the contract to execute the "Get Enrollment" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 7 | GetEnrollmentWithProgressAsync(userId: int, courseId: int) : Task&lt;Enrollment?&gt; | Defines the contract to execute the "Get Enrollment With Progress" operation asynchronously, returning `Task<Enrollment?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 8 | GetMyEnrolledCoursesAsync(userId: int) : Task&lt;List&gt;Enrollment&lt;&gt; | Defines the contract to execute the "Get My Enrolled Courses" operation asynchronously, returning `Task<List>Enrollment<>`. It accepts `userId: int` as input parameters. |
| 9 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |

### 11. IGiftRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetByOrderItemIdAsync(orderItemId: int) : Task&lt;Gift?&gt; | Defines the contract to execute the "Get By Order Item Id" operation asynchronously, returning `Task<Gift?>`. It accepts `orderItemId: int` as input parameters. |
| 2 | GetByTokenAsync(token: string) : Task&lt;Gift?&gt; | Defines the contract to execute the "Get By Token" operation asynchronously, returning `Task<Gift?>`. It accepts `token: string` as input parameters. |
| 3 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 4 | Update(gift: Gift) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `gift: Gift` as input parameters. |

### 12. IInstructorRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(instructor: Instructor) : Task | Defines the contract to execute the "Add" operation asynchronously, returning `Task`. It accepts `instructor: Instructor` as input parameters. |
| 2 | CountActiveCoursesAsync(instructorId: int) : Task&lt;int&gt; | Defines the contract to execute the "Count Active Courses" operation asynchronously, returning `Task<int>`. It accepts `instructorId: int` as input parameters. |
| 3 | GetByIdAsync(id: int) : Task&lt;Instructor?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Instructor?>`. It accepts `id: int` as input parameters. |
| 4 | GetByIdWithNavigationAsync(userId: int) : Task&lt;Instructor&gt; | Defines the contract to execute the "Get By Id With Navigation" operation asynchronously, returning `Task<Instructor>`. It accepts `userId: int` as input parameters. |
| 5 | GetDashboardDtoAsync(userId: int) : Task&lt;InstructorDashboardDto?&gt; | Defines the contract to execute the "Get Dashboard Dto" operation asynchronously, returning `Task<InstructorDashboardDto?>`. It accepts `userId: int` as input parameters. |
| 6 | GetEnrollmentGrowthAsync(instructorId: int) : Task&lt;double&gt; | Defines the contract to execute the "Get Enrollment Growth" operation asynchronously, returning `Task<double>`. It accepts `instructorId: int` as input parameters. |
| 7 | GetInstructorRankingPercentageAsync(instructorId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Instructor Ranking Percentage" operation asynchronously, returning `Task<int>`. It accepts `instructorId: int` as input parameters. |
| 8 | GetInstructorsWithStripeAsync() : Task&lt;List&gt;Instructor&lt;&gt; | Defines the contract to execute the "Get Instructors With Stripe" operation asynchronously, returning `Task<List>Instructor<>`. |
| 9 | GetPayoutsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;InstructorPayoutDto&lt;&gt; | Defines the contract to execute the "Get Payouts" operation asynchronously, returning `Task<PagedResult>InstructorPayoutDto<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 10 | GetPendingInstructorsAsync(page: int, pageSize: int) : Task&lt;(IEnumerable&gt;Instructor&lt;, int)&gt; | Defines the contract to execute the "Get Pending Instructors" operation asynchronously, returning `Task<(IEnumerable>Instructor<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 11 | GetStatsAsync(userId: int) : Task&lt;InstructorStats?&gt; | Defines the contract to execute the "Get Stats" operation asynchronously, returning `Task<InstructorStats?>`. It accepts `userId: int` as input parameters. |
| 12 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 13 | Update(instructor: Instructor) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `instructor: Instructor` as input parameters. |

### 13. ILessonRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(lesson: Lesson) : Task | Defines the contract to execute the "Add" operation asynchronously, returning `Task`. It accepts `lesson: Lesson` as input parameters. |
| 2 | GetByCourseIdAsync(courseId: int) : Task&lt;List&gt;Lesson&lt;&gt; | Defines the contract to execute the "Get By Course Id" operation asynchronously, returning `Task<List>Lesson<>`. It accepts `courseId: int` as input parameters. |
| 3 | GetByIdAsync(id: int) : Task&lt;Lesson?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Lesson?>`. It accepts `id: int` as input parameters. |
| 4 | GetByIdAsync(lessonId: int) : Task&lt;Lesson?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Lesson?>`. It accepts `lessonId: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(lesson: Lesson) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `lesson: Lesson` as input parameters. |

### 14. ILessonReviewModerationLogRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetPagedAdminAsync(page: int, pageSize: int) : Task&lt;(List&gt;LessonReviewModerationLog&lt;, int)&gt; | Defines the contract to execute the "Get Paged Admin" operation asynchronously, returning `Task<(List>LessonReviewModerationLog<, int)>`. It accepts `page: int, pageSize: int` as input parameters. |

### 15. ILockoutRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(lockout: Lockout) : Task&lt;void&gt; | Defines the contract to execute the "Add" operation asynchronously, returning `Task<void>`. It accepts `lockout: Lockout` as input parameters. |
| 2 | GetActiveLockoutAsync(accountId: int, lockoutType: string) : Task&lt;Lockout?&gt; | Defines the contract to execute the "Get Active Lockout" operation asynchronously, returning `Task<Lockout?>`. It accepts `accountId: int, lockoutType: string` as input parameters. |
| 3 | RemoveAccountLockoutsAsync(accountId: int) : Task&lt;void&gt; | Defines the contract to execute the "Remove Account Lockouts" operation asynchronously, returning `Task<void>`. It accepts `accountId: int` as input parameters. |

### 16. IManagerRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetManagerByIdAsync(managerId: int) : Task&lt;Manager?&gt; | Defines the contract to execute the "Get Manager By Id" operation asynchronously, returning `Task<Manager?>`. It accepts `managerId: int` as input parameters. |
| 2 | UpdateManagerProfileAsync(managerId: int, displayName: string, fullName: string?, phoneNumber: string?, avatarUrl: string?, bio: string?) : Task&lt;bool&gt; | Defines the contract to execute the "Update Manager Profile" operation asynchronously, returning `Task<bool>`. It accepts `managerId: int, displayName: string, fullName: string?, phoneNumber: string?, avatarUrl: string?, bio: string?` as input parameters. |

### 17. IMaterialRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | Delete(material: LearningMaterial) | Defines the contract to execute the "Delete" operation, returning `void`. It accepts `material: LearningMaterial` as input parameters. |
| 2 | GetByCourseIdAsync(courseId: int) : Task&lt;List&gt;LearningMaterial&lt;&gt; | Defines the contract to execute the "Get By Course Id" operation asynchronously, returning `Task<List>LearningMaterial<>`. It accepts `courseId: int` as input parameters. |
| 3 | GetByIdAsync(materialId: int) : Task&lt;LearningMaterial?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<LearningMaterial?>`. It accepts `materialId: int` as input parameters. |
| 4 | GetTrashMaterialsAsync(instructorId: int) : Task&lt;List&gt;LearningMaterial&lt;&gt; | Defines the contract to execute the "Get Trash Materials" operation asynchronously, returning `Task<List>LearningMaterial<>`. It accepts `instructorId: int` as input parameters. |
| 5 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 6 | Update(material: LearningMaterial) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `material: LearningMaterial` as input parameters. |

### 18. INotificationRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(notification: Notification) : Task | Defines the contract to execute the "Add" operation asynchronously, returning `Task`. It accepts `notification: Notification` as input parameters. |
| 2 | AddRangeAsync(notifications: IEnumerable&lt;Notification&gt;) : Task | Defines the contract to execute the "Add Range" operation asynchronously, returning `Task`. It accepts `notifications: IEnumerable<Notification>` as input parameters. |
| 3 | AutoCleanupAdminNotificationsAsync() : Task&lt;int&gt; | Defines the contract to execute the "Auto Cleanup Admin Notifications" operation asynchronously, returning `Task<int>`. |
| 4 | Delete(notification: Notification) | Defines the contract to execute the "Delete" operation, returning `void`. It accepts `notification: Notification` as input parameters. |
| 5 | GetAllAsync(page: int, pageSize: int) : Task&lt;(List&gt;Notification&lt; Items, int TotalCount)&gt; | Defines the contract to execute the "Get All" operation asynchronously, returning `Task<(List>Notification< Items, int TotalCount)>`. It accepts `page: int, pageSize: int` as input parameters. |
| 6 | GetByIdAsync(id: int) : Task&lt;Notification?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Notification?>`. It accepts `id: int` as input parameters. |
| 7 | GetByReceiverIdAsync(userId: int, page: int, pageSize: int) : Task&lt;(List&gt;Notification&lt; Items, int TotalCount)&gt; | Defines the contract to execute the "Get By Receiver Id" operation asynchronously, returning `Task<(List>Notification< Items, int TotalCount)>`. It accepts `userId: int, page: int, pageSize: int` as input parameters. |
| 8 | GetSentNotificationsCountAsync(id: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Sent Notifications Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 9 | GetSentNotificationsCountAsync(senderId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Sent Notifications Count" operation asynchronously, returning `Task<int>`. It accepts `senderId: int` as input parameters. |
| 10 | GetUnreadByReceiverIdAsync(userId: int) : Task&lt;List&gt;Notification&lt;&gt; | Defines the contract to execute the "Get Unread By Receiver Id" operation asynchronously, returning `Task<List>Notification<>`. It accepts `userId: int` as input parameters. |
| 11 | GetUnreadCountAsync(userId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `userId: int` as input parameters. |
| 12 | MarkAsReadAsync(notificationId: int, userId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Mark As Read" operation asynchronously, returning `Task<bool>`. It accepts `notificationId: int, userId: int` as input parameters. |
| 13 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 14 | Update(notification: Notification) | Defines the contract to execute the "Update" operation, returning `void`. It accepts `notification: Notification` as input parameters. |
| 15 | UpdateRange(notifications: IEnumerable&lt;Notification&gt;) | Defines the contract to execute the "Update Range" operation, returning `void`. It accepts `notifications: IEnumerable<Notification>` as input parameters. |

### 19. IQuestionBankRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuestionAsync(question: QuizQuestion) : Task&lt;QuizQuestion&gt; | Defines the contract to execute the "Add Question" operation asynchronously, returning `Task<QuizQuestion>`. It accepts `question: QuizQuestion` as input parameters. |
| 2 | DeleteQuestionAsync(questionId: int) : Task | Defines the contract to execute the "Delete Question" operation asynchronously, returning `Task`. It accepts `questionId: int` as input parameters. |
| 3 | GetQuestionByIdAsync(questionId: int) : Task&lt;QuizQuestion?&gt; | Defines the contract to execute the "Get Question By Id" operation asynchronously, returning `Task<QuizQuestion?>`. It accepts `questionId: int` as input parameters. |
| 4 | GetQuestionsByCourseAsync(courseId: int) : Task&lt;List&gt;QuizQuestion&lt;&gt; | Defines the contract to execute the "Get Questions By Course" operation asynchronously, returning `Task<List>QuizQuestion<>`. It accepts `courseId: int` as input parameters. |
| 5 | GetQuestionsByLessonAsync(lessonId: int) : Task&lt;List&gt;QuizQuestion&lt;&gt; | Defines the contract to execute the "Get Questions By Lesson" operation asynchronously, returning `Task<List>QuizQuestion<>`. It accepts `lessonId: int` as input parameters. |
| 6 | UpdateQuestionAsync(question: QuizQuestion) : Task&lt;QuizQuestion&gt; | Defines the contract to execute the "Update Question" operation asynchronously, returning `Task<QuizQuestion>`. It accepts `question: QuizQuestion` as input parameters. |

### 20. IQuizRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseQuizAsync(courseQuiz: CourseQuiz) : Task | Defines the contract to execute the "Add Course Quiz" operation asynchronously, returning `Task`. It accepts `courseQuiz: CourseQuiz` as input parameters. |
| 2 | CreateAsync(quiz: Quiz) : Task&lt;Quiz&gt; | Defines the contract to execute the "Create" operation asynchronously, returning `Task<Quiz>`. It accepts `quiz: Quiz` as input parameters. |
| 3 | GetAttemptByIdAsync(attemptId: int) : Task&lt;QuizAttempt?&gt; | Defines the contract to execute the "Get Attempt By Id" operation asynchronously, returning `Task<QuizAttempt?>`. It accepts `attemptId: int` as input parameters. |
| 4 | GetAttemptsByQuizAndUserAsync(quizId: int, userId: int, page: int, pageSize: int) : Task&lt;List&gt;QuizAttempt&lt;&gt; | Defines the contract to execute the "Get Attempts By Quiz And User" operation asynchronously, returning `Task<List>QuizAttempt<>`. It accepts `quizId: int, userId: int, page: int, pageSize: int` as input parameters. |
| 5 | GetByIdAsync(quizId: int) : Task&lt;Quiz?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<Quiz?>`. It accepts `quizId: int` as input parameters. |
| 6 | GetByInstructorAsync(instructorId: int) : Task&lt;List&gt;Quiz&lt;&gt; | Defines the contract to execute the "Get By Instructor" operation asynchronously, returning `Task<List>Quiz<>`. It accepts `instructorId: int` as input parameters. |
| 7 | GetQuizWithQuestionsAsync(quizId: int) : Task&lt;Quiz?&gt; | Defines the contract to execute the "Get Quiz With Questions" operation asynchronously, returning `Task<Quiz?>`. It accepts `quizId: int` as input parameters. |
| 8 | SaveAttemptAsync(attempt: QuizAttempt) : Task&lt;QuizAttempt&gt; | Defines the contract to execute the "Save Attempt" operation asynchronously, returning `Task<QuizAttempt>`. It accepts `attempt: QuizAttempt` as input parameters. |
| 9 | SoftDeleteAsync(quizId: int) : Task | Defines the contract to execute the "Soft Delete" operation asynchronously, returning `Task`. It accepts `quizId: int` as input parameters. |
| 10 | UpdateAsync(quiz: Quiz) : Task | Defines the contract to execute the "Update" operation asynchronously, returning `Task`. It accepts `quiz: Quiz` as input parameters. |

### 21. IReportRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseReportAsync(report: CourseReport) : Task | Defines the contract to execute the "Add Course Report" operation asynchronously, returning `Task`. It accepts `report: CourseReport` as input parameters. |
| 2 | AddCourseReviewReportAsync(report: CourseReviewReport) : Task | Defines the contract to execute the "Add Course Review Report" operation asynchronously, returning `Task`. It accepts `report: CourseReviewReport` as input parameters. |
| 3 | AddLessonReviewReportAsync(report: LessonReviewReport) : Task | Defines the contract to execute the "Add Lesson Review Report" operation asynchronously, returning `Task`. It accepts `report: LessonReviewReport` as input parameters. |
| 4 | CountCourseReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Defines the contract to execute the "Count Course Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 5 | CountCourseReviewReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Defines the contract to execute the "Count Course Review Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 6 | CountLessonReviewReportsByStatusAsync(status: string, resolvedAtDate: DateTime?) : Task&lt;int&gt; | Defines the contract to execute the "Count Lesson Review Reports By Status" operation asynchronously, returning `Task<int>`. It accepts `status: string, resolvedAtDate: DateTime?` as input parameters. |
| 7 | GetAllCourseReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;CourseReport&gt;, int&lt;&gt; | Defines the contract to execute the "Get All Course Reports" operation asynchronously, returning `Task<Tuple>List<CourseReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 8 | GetAllCourseReviewReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;CourseReviewReport&gt;, int&lt;&gt; | Defines the contract to execute the "Get All Course Review Reports" operation asynchronously, returning `Task<Tuple>List<CourseReviewReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 9 | GetAllLessonReviewReportsAsync(status: string?, page: int, pageSize: int) : Task&lt;Tuple&gt;List&lt;LessonReviewReport&gt;, int&lt;&gt; | Defines the contract to execute the "Get All Lesson Review Reports" operation asynchronously, returning `Task<Tuple>List<LessonReviewReport>, int<>`. It accepts `status: string?, page: int, pageSize: int` as input parameters. |
| 10 | GetCourseReportByIdAsync(reportId: int) : Task&lt;CourseReport?&gt; | Defines the contract to execute the "Get Course Report By Id" operation asynchronously, returning `Task<CourseReport?>`. It accepts `reportId: int` as input parameters. |
| 11 | GetCourseReviewReportByIdAsync(reportId: int) : Task&lt;CourseReviewReport?&gt; | Defines the contract to execute the "Get Course Review Report By Id" operation asynchronously, returning `Task<CourseReviewReport?>`. It accepts `reportId: int` as input parameters. |
| 12 | GetLessonReviewReportByIdAsync(reportId: int) : Task&lt;LessonReviewReport?&gt; | Defines the contract to execute the "Get Lesson Review Report By Id" operation asynchronously, returning `Task<LessonReviewReport?>`. It accepts `reportId: int` as input parameters. |
| 13 | GetPendingCourseReportAsync(reporterId: int, courseId: int, reason: string) : Task&lt;CourseReport?&gt; | Defines the contract to execute the "Get Pending Course Report" operation asynchronously, returning `Task<CourseReport?>`. It accepts `reporterId: int, courseId: int, reason: string` as input parameters. |
| 14 | GetPendingCourseReviewReportAsync(reporterId: int, courseReviewId: int, reason: string) : Task&lt;CourseReviewReport?&gt; | Defines the contract to execute the "Get Pending Course Review Report" operation asynchronously, returning `Task<CourseReviewReport?>`. It accepts `reporterId: int, courseReviewId: int, reason: string` as input parameters. |
| 15 | GetPendingLessonReviewReportAsync(reporterId: int, lessonReviewId: int, reason: string) : Task&lt;LessonReviewReport?&gt; | Defines the contract to execute the "Get Pending Lesson Review Report" operation asynchronously, returning `Task<LessonReviewReport?>`. It accepts `reporterId: int, lessonReviewId: int, reason: string` as input parameters. |
| 16 | GetResolvedReportsCountAsync(id: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Resolved Reports Count" operation asynchronously, returning `Task<int>`. It accepts `id: int` as input parameters. |
| 17 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 18 | UpdateCourseReport(report: CourseReport) | Defines the contract to execute the "Update Course Report" operation, returning `void`. It accepts `report: CourseReport` as input parameters. |
| 19 | UpdateCourseReviewReport(report: CourseReviewReport) | Defines the contract to execute the "Update Course Review Report" operation, returning `void`. It accepts `report: CourseReviewReport` as input parameters. |
| 20 | UpdateLessonReviewReport(report: LessonReviewReport) | Defines the contract to execute the "Update Lesson Review Report" operation, returning `void`. It accepts `report: LessonReviewReport` as input parameters. |

### 22. IReviewRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddCourseReviewAsync(review: CourseReview) : Task | Defines the contract to execute the "Add Course Review" operation asynchronously, returning `Task`. It accepts `review: CourseReview` as input parameters. |
| 2 | AddLessonReviewAsync(review: LessonReview) : Task | Defines the contract to execute the "Add Lesson Review" operation asynchronously, returning `Task`. It accepts `review: LessonReview` as input parameters. |
| 3 | GetCourseReviewByIdAsync(reviewId: int) : Task&lt;CourseReview?&gt; | Defines the contract to execute the "Get Course Review By Id" operation asynchronously, returning `Task<CourseReview?>`. It accepts `reviewId: int` as input parameters. |
| 4 | GetCourseReviewsWithDetailsAsync(courseId: int, page: int, pageSize: int, starFilter: int?) : Task&lt;(List&gt;CourseReview&lt; Items, int TotalCount)&gt; | Defines the contract to execute the "Get Course Reviews With Details" operation asynchronously, returning `Task<(List>CourseReview< Items, int TotalCount)>`. It accepts `courseId: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 5 | GetLessonReviewByIdAsync(reviewId: int) : Task&lt;LessonReview?&gt; | Defines the contract to execute the "Get Lesson Review By Id" operation asynchronously, returning `Task<LessonReview?>`. It accepts `reviewId: int` as input parameters. |
| 6 | HasPendingCourseReviewReportsAsync(reviewId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Has Pending Course Review Reports" operation asynchronously, returning `Task<bool>`. It accepts `reviewId: int` as input parameters. |
| 7 | HasPendingLessonReviewReportsAsync(reviewId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Has Pending Lesson Review Reports" operation asynchronously, returning `Task<bool>`. It accepts `reviewId: int` as input parameters. |
| 8 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 9 | UpdateCourseReview(review: CourseReview) | Defines the contract to execute the "Update Course Review" operation, returning `void`. It accepts `review: CourseReview` as input parameters. |
| 10 | UpdateLessonReview(review: LessonReview) | Defines the contract to execute the "Update Lesson Review" operation, returning `void`. It accepts `review: LessonReview` as input parameters. |

### 23. ISystemConfigRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAllConfigAsync() : Task&lt;List&gt;Dictionary&lt;string, object&gt;&lt;&gt; | Defines the contract to execute the "Get All Config" operation asynchronously, returning `Task<List>Dictionary<string, object><>`. |
| 2 | GetValueAsync(key: string) : Task&lt;string?&gt; | Defines the contract to execute the "Get Value" operation asynchronously, returning `Task<string?>`. It accepts `key: string` as input parameters. |
| 3 | GetValueAsync(key: string) : Task&lt;string&gt; | Defines the contract to execute the "Get Value" operation asynchronously, returning `Task<string>`. It accepts `key: string` as input parameters. |
| 4 | SetConfigByKeyAsync(key: string, value: Dictionary&lt;string, object&gt;) : Task | Defines the contract to execute the "Set Config By Key" operation asynchronously, returning `Task`. It accepts `key: string, value: Dictionary<string, object>` as input parameters. |
| 5 | UpsertConfigAsync(configKey: string, configValue: string, description: string?) : Task&lt;int&gt; | Defines the contract to execute the "Upsert Config" operation asynchronously, returning `Task<int>`. It accepts `configKey: string, configValue: string, description: string?` as input parameters. |

### 24. ITransactionRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAccountTransactionsSummaryAsync(id: int) : Task&lt;AccountTransactionSummaryDto&gt; | Defines the contract to execute the "Get Account Transactions Summary" operation asynchronously, returning `Task<AccountTransactionSummaryDto>`. It accepts `id: int` as input parameters. |
| 2 | GetInstructorTotalRevenueAsync(id: int) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Instructor Total Revenue" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 3 | GetInstructorTotalWithdrawnAsync(id: int) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Instructor Total Withdrawn" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 4 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;(List&gt;TransactionListDto&lt;, int)&gt; | Defines the contract to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<(List>TransactionListDto<, int)>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 5 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Defines the contract to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 6 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;ValueTuple&gt;List&lt;TransactionListDto&gt;,int&lt;&gt; | Defines the contract to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<ValueTuple>List<TransactionListDto>,int<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 7 | GetTotalSpentAsync(id: int) : Task&lt;decimal&gt; | Defines the contract to execute the "Get Total Spent" operation asynchronously, returning `Task<decimal>`. It accepts `id: int` as input parameters. |
| 8 | GetTransactionDetailAsync(transactionId: int) : Task&lt;TransactionDetailDto?&gt; | Defines the contract to execute the "Get Transaction Detail" operation asynchronously, returning `Task<TransactionDetailDto?>`. It accepts `transactionId: int` as input parameters. |
| 9 | GetTransactionsAsync(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;ValueTuple&gt;List&lt;TransactionListDto&gt;,int&lt;&gt; | Defines the contract to execute the "Get Transactions" operation asynchronously, returning `Task<ValueTuple>List<TransactionListDto>,int<>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 10 | GetUserTransactionsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;Tuple&gt;List&lt;TransactionListDto&gt;, int&lt;&gt; | Defines the contract to execute the "Get User Transactions" operation asynchronously, returning `Task<Tuple>List<TransactionListDto>, int<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |
| 11 | RejectPendingRefundForGiftClaimedAsync(orderItemId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Reject Pending Refund For Gift Claimed" operation asynchronously, returning `Task<bool>`. It accepts `orderItemId: int` as input parameters. |

### 25. IUserLockoutRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetActiveLockoutAsync(userId: int, type: string) : Task&lt;UserLockout?&gt; | Defines the contract to execute the "Get Active Lockout" operation asynchronously, returning `Task<UserLockout?>`. It accepts `userId: int, type: string` as input parameters. |

### 26. IUserRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAccountByEmailAsync(email: string) : Task&lt;Account?&gt; | Defines the contract to execute the "Get Account By Email" operation asynchronously, returning `Task<Account?>`. It accepts `email: string` as input parameters. |
| 2 | GetAccountByEmailOrUsernameAsync(usernameOrEmail: string) : Task&lt;Account?&gt; | Defines the contract to execute the "Get Account By Email Or Username" operation asynchronously, returning `Task<Account?>`. It accepts `usernameOrEmail: string` as input parameters. |
| 3 | GetAccountByIdAsync(accountId: int) : Task&lt;Account&gt; | Defines the contract to execute the "Get Account By Id" operation asynchronously, returning `Task<Account>`. It accepts `accountId: int` as input parameters. |
| 4 | GetAccountsPagedAsync(keyword: string?, role: string?, page: int, pageSize: int) : Task&lt;PagedResult&gt;Account&lt;&gt; | Defines the contract to execute the "Get Accounts Paged" operation asynchronously, returning `Task<PagedResult>Account<>`. It accepts `keyword: string?, role: string?, page: int, pageSize: int` as input parameters. |
| 5 | GetAllUserIdsAsync() : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get All User Ids" operation asynchronously, returning `Task<List>int<>`. |
| 6 | GetByIdAsync(id: int) : Task&lt;User?&gt; | Defines the contract to execute the "Get By Id" operation asynchronously, returning `Task<User?>`. It accepts `id: int` as input parameters. |
| 7 | GetChatByIdAsync(chatId: int) : Task&lt;Chat?&gt; | Defines the contract to execute the "Get Chat By Id" operation asynchronously, returning `Task<Chat?>`. It accepts `chatId: int` as input parameters. |
| 8 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 9 | GetRoleByAccountIdAsync(accountId: int) : Task&lt;string?&gt; | Defines the contract to execute the "Get Role By Account Id" operation asynchronously, returning `Task<string?>`. It accepts `accountId: int` as input parameters. |
| 10 | GetRoleByAccountIdAsync(accountId: int) : Task&lt;string&gt; | Defines the contract to execute the "Get Role By Account Id" operation asynchronously, returning `Task<string>`. It accepts `accountId: int` as input parameters. |
| 11 | GetUserByIdAsync(userId: int) : Task&lt;User?&gt; | Defines the contract to execute the "Get User By Id" operation asynchronously, returning `Task<User?>`. It accepts `userId: int` as input parameters. |
| 12 | GetUserIdsForAdminSenderAsync() : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get User Ids For Admin Sender" operation asynchronously, returning `Task<List>int<>`. |
| 13 | GetUserIdsForStaffSenderAsync() : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get User Ids For Staff Sender" operation asynchronously, returning `Task<List>int<>`. |
| 14 | IsEmailExistsAsync(email: string) : Task&lt;bool&gt; | Defines the contract to execute the "Is Email Exists" operation asynchronously, returning `Task<bool>`. It accepts `email: string` as input parameters. |
| 15 | IsUsernameExistsAsync(username: string) : Task&lt;bool&gt; | Defines the contract to execute the "Is Username Exists" operation asynchronously, returning `Task<bool>`. It accepts `username: string` as input parameters. |
| 16 | RegisterManagerAsync(account: Account, manager: Manager) : Task&lt;bool&gt; | Defines the contract to execute the "Register Manager" operation asynchronously, returning `Task<bool>`. It accepts `account: Account, manager: Manager` as input parameters. |
| 17 | RegisterUserAsync(account: Account, user: User) : Task&lt;bool&gt; | Defines the contract to execute the "Register User" operation asynchronously, returning `Task<bool>`. It accepts `account: Account, user: User` as input parameters. |
| 18 | RevokeRefreshTokenAsync(accountId: int) : Task&lt;int&gt; | Defines the contract to execute the "Revoke Refresh Token" operation asynchronously, returning `Task<int>`. It accepts `accountId: int` as input parameters. |
| 19 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |
| 20 | SaveRefreshTokenAsync(accountId: int, refreshToken: string) : Task&lt;int&gt; | Defines the contract to execute the "Save Refresh Token" operation asynchronously, returning `Task<int>`. It accepts `accountId: int, refreshToken: string` as input parameters. |
| 21 | SearchEmailsByQueryAsync(query: string, senderId: int, senderRole: string, take: int) : Task&lt;List&gt;string&lt;&gt; | Defines the contract to execute the "Search Emails By Query" operation asynchronously, returning `Task<List>string<>`. It accepts `query: string, senderId: int, senderRole: string, take: int` as input parameters. |
| 22 | UpdateAccountAsync(account: Account) : Task&lt;int&gt; | Defines the contract to execute the "Update Account" operation asynchronously, returning `Task<int>`. It accepts `account: Account` as input parameters. |
| 23 | UpdateAccountAsync(account: Account) | Defines the contract to execute the "Update Account" operation asynchronously, returning `void`. It accepts `account: Account` as input parameters. |
| 24 | UpdateChatAsync(chat: Chat) : Task | Defines the contract to execute the "Update Chat" operation asynchronously, returning `Task`. It accepts `chat: Chat` as input parameters. |
| 25 | UpdateEmailVerifiedAsync(email: string) : Task&lt;bool&gt; | Defines the contract to execute the "Update Email Verified" operation asynchronously, returning `Task<bool>`. It accepts `email: string` as input parameters. |
| 26 | UpdateLastLoginAsync(accountId: int) : Task&lt;int&gt; | Defines the contract to execute the "Update Last Login" operation asynchronously, returning `Task<int>`. It accepts `accountId: int` as input parameters. |
| 27 | UpdateUserProfileAsync(userId: int, fullName: string, bio: string, dob: DateTime?, avatarUrl: string?, phoneNumber: string?, email: string) : Task&lt;bool&gt; | Defines the contract to execute the "Update User Profile" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, fullName: string, bio: string, dob: DateTime?, avatarUrl: string?, phoneNumber: string?, email: string` as input parameters. |

### 27. IWishlistRepository Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddAsync(newItem: WishlistItem) : Task | Defines the contract to execute the "Add" operation asynchronously, returning `Task`. It accepts `newItem: WishlistItem` as input parameters. |
| 2 | ExistsAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Exists" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 3 | GetByUserAndCourseAsync(userId: int, courseId: int) : Task&lt;WishlistItem?&gt; | Defines the contract to execute the "Get By User And Course" operation asynchronously, returning `Task<WishlistItem?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 4 | GetByUserIdAsync(userId: int) : Task&lt;List&gt;WishlistItem&lt;&gt; | Defines the contract to execute the "Get By User Id" operation asynchronously, returning `Task<List>WishlistItem<>`. It accepts `userId: int` as input parameters. |
| 5 | RemoveAsync(item: WishlistItem) | Defines the contract to execute the "Remove" operation asynchronously, returning `void`. It accepts `item: WishlistItem` as input parameters. |
| 6 | SaveChangesAsync() : Task&lt;int&gt; | Defines the contract to execute the "Save Changes" operation asynchronously, returning `Task<int>`. |

## Layer: Service
### 1. AdminAccountService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateStaffAsync(request: CreateStaffRequestDTO) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Create Staff" operation asynchronously, returning `Task<bool>`. It accepts `request: CreateStaffRequestDTO` as input parameters. |
| 2 | FlagAccountAsync(id: int, reason: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Flag Account" operation asynchronously, returning `Task<bool>`. It accepts `id: int, reason: string` as input parameters. |
| 3 | GetAccountDetailAsync(id: int) : Task&lt;AdminAccountDetailDto&gt; | Provides the concrete implementation to execute the "Get Account Detail" operation asynchronously, returning `Task<AdminAccountDetailDto>`. It accepts `id: int` as input parameters. |
| 4 | GetAccountTransactionsAsync(id: int) : Task&lt;AccountTransactionSummaryDto&gt; | Provides the concrete implementation to execute the "Get Account Transactions" operation asynchronously, returning `Task<AccountTransactionSummaryDto>`. It accepts `id: int` as input parameters. |
| 5 | GetAccountsPagedAsync(keyword: string?, role: string?, page: int, pageSize: int) : Task&lt;PagedResult&gt;AdminAccountListDto&lt;&gt; | Provides the concrete implementation to execute the "Get Accounts Paged" operation asynchronously, returning `Task<PagedResult>AdminAccountListDto<>`. It accepts `keyword: string?, role: string?, page: int, pageSize: int` as input parameters. |
| 6 | IsUsernameExistsAsync(username: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Username Exists" operation asynchronously, returning `Task<bool>`. It accepts `username: string` as input parameters. |
| 7 | ToggleBanAsync(id: int, adminId: int) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Toggle Ban" operation asynchronously, returning `Task<string>`. It accepts `id: int, adminId: int` as input parameters. |
| 8 | UpdateStaffAsync(id: int, request: UpdateStaffRequestDTO) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Staff" operation asynchronously, returning `Task<bool>`. It accepts `id: int, request: UpdateStaffRequestDTO` as input parameters. |
| 9 | _chatRepository : IChatRepository | Provides the Chat Repository dependency. |
| 10 | _enrollmentRepository : IEnrollmentRepository | Provides the Enrollment Repository dependency. |
| 11 | _lockoutRepository : ILockoutRepository | Provides the Lockout Repository dependency. |
| 12 | _notificationRepository : INotificationRepository | Provides the Notification Repository dependency. |
| 13 | _notificationService : INotificationService | Provides the Notification Service dependency. |
| 14 | _reportRepository : IReportRepository | Provides the Report Repository dependency. |
| 15 | _transactionRepository : ITransactionRepository | Provides the Transaction Repository dependency. |
| 16 | _userRepository : IUserRepository | Provides the User Repository dependency. |

### 2. AdminFinanceService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveRefundAsync(transactionId: int, adminNote: string) : Task | Provides the concrete implementation to execute the "Approve Refund" operation asynchronously, returning `Task`. It accepts `transactionId: int, adminNote: string` as input parameters. |
| 2 | CreateWithdrawalAsync(request: WithdrawRequest, managerId: int) : Task&lt;WithdrawResponse&gt; | Provides the concrete implementation to execute the "Create Withdrawal" operation asynchronously, returning `Task<WithdrawResponse>`. It accepts `request: WithdrawRequest, managerId: int` as input parameters. |
| 3 | GetFinancialSummaryAsync(year: int?, month: int?) : Task&lt;FinancialSummaryResponse&gt; | Provides the concrete implementation to execute the "Get Financial Summary" operation asynchronously, returning `Task<FinancialSummaryResponse>`. It accepts `year: int?, month: int?` as input parameters. |
| 4 | GetInstructorCourseRevenuesByInstructorAsync(instructorId: int, year: int, month: int) : Task&lt;List&gt;InstructorCourseRevenueResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Course Revenues By Instructor" operation asynchronously, returning `Task<List>InstructorCourseRevenueResponse<>`. It accepts `instructorId: int, year: int, month: int` as input parameters. |
| 5 | GetInstructorPayoutsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;PagedResult&gt;PayoutDetailResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Payouts" operation asynchronously, returning `Task<PagedResult>PayoutDetailResponse<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 6 | GetPayoutDaysConfigAsync() : Task&lt;string&gt; | Provides the concrete implementation to execute the "Get Payout Days Config" operation asynchronously, returning `Task<string>`. |
| 7 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 8 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;Transaction&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<PagedResult>Transaction<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 9 | GetPlatformBalanceAsync() : Task&lt;PlatformBalanceResponse&gt; | Provides the concrete implementation to execute the "Get Platform Balance" operation asynchronously, returning `Task<PlatformBalanceResponse>`. |
| 10 | GetWithdrawalHistoryAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;PagedResult&gt;WithdrawalHistoryItem&lt;&gt; | Provides the concrete implementation to execute the "Get Withdrawal History" operation asynchronously, returning `Task<PagedResult>WithdrawalHistoryItem<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 11 | RefundTransactionAsync(transactionId: int, reason: string?) : Task&lt;RefundResultResponse&gt; | Provides the concrete implementation to execute the "Refund Transaction" operation asynchronously, returning `Task<RefundResultResponse>`. It accepts `transactionId: int, reason: string?` as input parameters. |
| 12 | RejectRefundAsync(transactionId: int, adminNote: string) : Task | Provides the concrete implementation to execute the "Reject Refund" operation asynchronously, returning `Task`. It accepts `transactionId: int, adminNote: string` as input parameters. |
| 13 | RequestRefundAsync(transactionId: int, studentId: int, reason: string) : Task&lt;RefundResultDto&gt; | Provides the concrete implementation to execute the "Request Refund" operation asynchronously, returning `Task<RefundResultDto>`. It accepts `transactionId: int, studentId: int, reason: string` as input parameters. |
| 14 | SetPayoutDaysConfigAsync(payoutDays: string) : Task | Provides the concrete implementation to execute the "Set Payout Days Config" operation asynchronously, returning `Task`. It accepts `payoutDays: string` as input parameters. |
| 15 | SetTransferRateAsync(rate: decimal) : Task | Provides the concrete implementation to execute the "Set Transfer Rate" operation asynchronously, returning `Task`. It accepts `rate: decimal` as input parameters. |
| 16 | _configRepo : ISystemConfigRepository | Provides the Config Repo dependency. |
| 17 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 18 | _financeRepo : IAdminFinanceRepository | Provides the Finance Repo dependency. |
| 19 | _giftRepo : IGiftRepository | Provides the Gift Repo dependency. |
| 20 | _hubContext : IHubContext&lt;FinanceHub&gt; | Provides the Hub Context dependency. |
| 21 | _instructorRepo : IInstructorRepository | Provides the Instructor Repo dependency. |
| 22 | _logger : ILogger&lt;AdminFinanceService&gt; | Provides the Logger dependency. |
| 23 | _notiService : INotificationService | Provides the Noti Service dependency. |
| 24 | _paymentGateway : IPaymentGatewayService | Provides the Payment Gateway dependency. |
| 25 | _repo : IAdminFinanceRepository | Provides the Repo dependency. |
| 26 | _stripeConnect : IStripeConnectService | Provides the Stripe Connect dependency. |

### 3. AiConfigurationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | UpdateThresholdsAsync(req: UpdateThresholdsRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Thresholds" operation asynchronously, returning `Task<bool>`. It accepts `req: UpdateThresholdsRequest` as input parameters. |
| 2 | _configRepo : ISystemConfigRepository | Provides the Config Repo dependency. |

### 4. AiModelManagementService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddModelAsync(req: CreateAiModelRequest) : Task&lt;AiModelAdminDto&gt; | Provides the concrete implementation to execute the "Add Model" operation asynchronously, returning `Task<AiModelAdminDto>`. It accepts `req: CreateAiModelRequest` as input parameters. |
| 2 | GetAllModelsAsync() : Task&lt;List&gt;AiModelAdminDto&lt;&gt; | Provides the concrete implementation to execute the "Get All Models" operation asynchronously, returning `Task<List>AiModelAdminDto<>`. |
| 3 | GetPagedModelsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;AiModelAdminDto&lt;&gt; | Provides the concrete implementation to execute the "Get Paged Models" operation asynchronously, returning `Task<PagedResult>AiModelAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 4 | ToggleModelStatusAsync(id: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Toggle Model Status" operation asynchronously, returning `Task<bool>`. It accepts `id: int` as input parameters. |
| 5 | UpdateModelAsync(id: int, req: UpdateAiModelRequest) : Task&lt;AiModelAdminDto&gt; | Provides the concrete implementation to execute the "Update Model" operation asynchronously, returning `Task<AiModelAdminDto>`. It accepts `id: int, req: UpdateAiModelRequest` as input parameters. |
| 6 | _aiModelRepo : IAiModelRepository | Provides the Ai Model Repo dependency. |

### 5. AiModerationLogService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetCourseModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;CourseModerationLogAdminDto&lt;&gt; | Provides the concrete implementation to execute the "Get Course Moderation Logs" operation asynchronously, returning `Task<PagedResult>CourseModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 2 | GetCourseReviewModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;ReviewModerationLogAdminDto&lt;&gt; | Provides the concrete implementation to execute the "Get Course Review Moderation Logs" operation asynchronously, returning `Task<PagedResult>ReviewModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 3 | GetLessonReviewModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;ReviewModerationLogAdminDto&lt;&gt; | Provides the concrete implementation to execute the "Get Lesson Review Moderation Logs" operation asynchronously, returning `Task<PagedResult>ReviewModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 4 | _courseLogRepo : ICourseAiUsageLogRepository | Provides the Course Log Repo dependency. |
| 5 | _courseReviewLogRepo : ICourseReviewModerationLogRepository | Provides the Course Review Log Repo dependency. |
| 6 | _lessonReviewLogRepo : ILessonReviewModerationLogRepository | Provides the Lesson Review Log Repo dependency. |

### 6. AiModerationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | HealthCheckAsync() : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Health Check" operation asynchronously, returning `Task<bool>`. |
| 2 | ModerateCourseFullPipelineAsync(semanticReq: SemanticDuplicationRequest, harmfulReq: CourseHarmfulRequest) : Task&lt;CourseModerationResult&gt; | Provides the concrete implementation to execute the "Moderate Course Full Pipeline" operation asynchronously, returning `Task<CourseModerationResult>`. It accepts `semanticReq: SemanticDuplicationRequest, harmfulReq: CourseHarmfulRequest` as input parameters. |

### 7. AuthService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ForgotPasswordAsync(email: string) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Forgot Password" operation asynchronously, returning `Task<string>`. It accepts `email: string` as input parameters. |
| 2 | GoogleLoginAsync(request: GoogleLoginRequestDTO) : Task&lt;LoginResponseDTO?&gt; | Provides the concrete implementation to execute the "Google Login" operation asynchronously, returning `Task<LoginResponseDTO?>`. It accepts `request: GoogleLoginRequestDTO` as input parameters. |
| 3 | IsEmailVerifiedAsync(userId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Email Verified" operation asynchronously, returning `Task<bool>`. It accepts `userId: int` as input parameters. |
| 4 | LoginAsync(request: LoginRequestDTO) : Task&lt;LoginResponseDTO?&gt; | Provides the concrete implementation to execute the "Login" operation asynchronously, returning `Task<LoginResponseDTO?>`. It accepts `request: LoginRequestDTO` as input parameters. |
| 5 | LogoutAsync(accountId: int) : Task&lt;void&gt; | Provides the concrete implementation to execute the "Logout" operation asynchronously, returning `Task<void>`. It accepts `accountId: int` as input parameters. |
| 6 | RegisterAsync(request: RegisterRequestDTO) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Register" operation asynchronously, returning `Task<string>`. It accepts `request: RegisterRequestDTO` as input parameters. |
| 7 | ResetPasswordAsync(email: string, otp: string, newPassword: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Reset Password" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string, newPassword: string` as input parameters. |
| 8 | VerifyEmailAsync(email: string, otp: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Verify Email" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string` as input parameters. |
| 9 | VerifyOtpForResetAsync(email: string, otp: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Verify Otp For Reset" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string` as input parameters. |
| 10 | _emailService : IEmailService | Provides the Email Service dependency. |
| 11 | _lockoutRepository : ILockoutRepository | Provides the Lockout Repository dependency. |
| 12 | _otpService : IOtpService | Provides the Otp Service dependency. |
| 13 | _userRepository : IUserRepository | Provides the User Repository dependency. |

### 8. BackgroundTaskQueue Class
| No | Method / Property | Description |
|---|---|---|
| 1 | QueueBackgroundWorkItemAsync&lt;ICourseAiModerationService&gt;(workItem: Func) : ValueTask | Provides the concrete implementation to execute the "Queue Background Work Item Async~ICourse Ai Moderation Service~" operation asynchronously, returning `ValueTask`. It accepts `workItem: Func` as input parameters. |

### 9. CartService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToCartAsync(userId: int, courseId: int) : Task | Provides the concrete implementation to execute the "Add To Cart" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetCartSummaryAsync(userId: int, couponCode: string?) : Task&lt;CartSummaryResponseDTO&gt; | Provides the concrete implementation to execute the "Get Cart Summary" operation asynchronously, returning `Task<CartSummaryResponseDTO>`. It accepts `userId: int, couponCode: string?` as input parameters. |
| 3 | GetCartSummaryAsync(userId: int, couponCode: string?) : Task&lt;CartSummaryResponse&gt; | Provides the concrete implementation to execute the "Get Cart Summary" operation asynchronously, returning `Task<CartSummaryResponse>`. It accepts `userId: int, couponCode: string?` as input parameters. |
| 4 | RemoveFromCartAsync(userId: int, courseId: int) : Task | Provides the concrete implementation to execute the "Remove From Cart" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |
| 5 | _cartRepository : ICartRepository | Provides the Cart Repository dependency. |
| 6 | _couponRepo : ICouponRepository | Provides the Coupon Repo dependency. |
| 7 | _couponRepository : ICouponRepository | Provides the Coupon Repository dependency. |
| 8 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 9 | _repo : ICartRepository | Provides the Repo dependency. |

### 10. ChatService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ClearChatHistoryAsync(chatId: int, accountId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Clear Chat History" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 2 | CreateSupportRequestAsync(senderId: int, dto: SupportRequestDto) : Task&lt;SupportTicketDto&gt; | Provides the concrete implementation to execute the "Create Support Request" operation asynchronously, returning `Task<SupportTicketDto>`. It accepts `senderId: int, dto: SupportRequestDto` as input parameters. |
| 3 | GetChatHistoryAsync(chatId: int, accountId: int) : Task&lt;List&gt;MessageDto&lt;&gt; | Provides the concrete implementation to execute the "Get Chat History" operation asynchronously, returning `Task<List>MessageDto<>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 4 | GetMyChatsAsync(accountId: int) : Task&lt;List&gt;ChatListDto&lt;&gt; | Provides the concrete implementation to execute the "Get My Chats" operation asynchronously, returning `Task<List>ChatListDto<>`. It accepts `accountId: int` as input parameters. |
| 5 | GetOrCreateChatAsync(senderId: int, dto: CreateChatDto) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Or Create Chat" operation asynchronously, returning `Task<int>`. It accepts `senderId: int, dto: CreateChatDto` as input parameters. |
| 6 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Provides the concrete implementation to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 7 | HasAccessToChatAsync(accountId: int, chatId: int) : Task&lt;bool&gt; | Executes the "Has Access To Chat" operation asynchronously, returning `Task<bool>`. It accepts `accountId: int, chatId: int` as input parameters. |
| 8 | MarkChatAsReadAsync(chatId: int, accountId: int) : Task&lt;void&gt; | Executes the "Mark Chat As Read" operation asynchronously, returning `Task<void>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 9 | SaveMessageAsync(senderId: int, dto: SendMessageDto) : Task&lt;MessageDto&gt; | Provides the concrete implementation to execute the "Save Message" operation asynchronously, returning `Task<MessageDto>`. It accepts `senderId: int, dto: SendMessageDto` as input parameters. |
| 10 | SearchChatsAsync(accountId: int, query: string) : Task&lt;List&gt;ChatListDto&lt;&gt; | Provides the concrete implementation to execute the "Search Chats" operation asynchronously, returning `Task<List>ChatListDto<>`. It accepts `accountId: int, query: string` as input parameters. |
| 11 | UpdateChatAndNotifyRecipientsAsync(senderId: int, chatId: int, sentAt: DateTime?) : Task&lt;void&gt; | Executes the "Update Chat And Notify Recipients" operation asynchronously, returning `Task<void>`. It accepts `senderId: int, chatId: int, sentAt: DateTime?` as input parameters. |
| 12 | ValidateMessageSendingAccessAsync(senderId: int, chatId: int) : Task&lt;void&gt; | Executes the "Validate Message Sending Access" operation asynchronously, returning `Task<void>`. It accepts `senderId: int, chatId: int` as input parameters. |
| 13 | _chatRepository : IChatRepository | Provides the Chat Repository dependency. |
| 14 | _configuration : IConfiguration | Provides the Configuration dependency. |
| 15 | _redisService : IRedisService | Provides the Redis Service dependency. |
| 16 | _repo : IChatRepository | Provides the Repo dependency. |
| 17 | _userRepository : IUserRepository | Provides the User Repository dependency. |

### 11. CheckoutService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddEntitiesToContext(order: OrderInfo, orderItem: OrderItem, transaction: Transaction) | Executes the "Add Entities To Context" operation, returning `void`. It accepts `order: OrderInfo, orderItem: OrderItem, transaction: Transaction` as input parameters. |
| 2 | ClearCartItemsFromContext(userId: string) | Executes the "Clear Cart Items From Context" operation, returning `void`. It accepts `userId: string` as input parameters. |
| 3 | CreateEntitiesAsync() : Task | Executes the "Create Entities" operation asynchronously, returning `Task`. |
| 4 | InitiatePaymentIntentAsync(userId: string, couponCode: string) : Task&lt;CheckoutResponse&gt; | Provides the concrete implementation to execute the "Initiate Payment Intent" operation asynchronously, returning `Task<CheckoutResponse>`. It accepts `userId: string, couponCode: string` as input parameters. |
| 5 | IsTransactionAlreadyProcessedAsync(sessionId: string) : Task&lt;bool&gt; | Executes the "Is Transaction Already Processed" operation asynchronously, returning `Task<bool>`. It accepts `sessionId: string` as input parameters. |
| 6 | ProcessPaymentIntentSuccessAsync(paymentIntentId: string) : Task | Provides the concrete implementation to execute the "Process Payment Intent Success" operation asynchronously, returning `Task`. It accepts `paymentIntentId: string` as input parameters. |
| 7 | ProcessSuccessfulPaymentCoreAsync(sessionId: string, paymentIntentId: string) : Task | Executes the "Process Successful Payment Core" operation asynchronously, returning `Task`. It accepts `sessionId: string, paymentIntentId: string` as input parameters. |
| 8 | _paymentGateway : IPaymentGatewayService | Provides the Payment Gateway dependency. |
| 9 | _repo : ICheckoutRepository | Provides the Repo dependency. |

### 12. CouponService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateAsync(req: CreateCouponRequestDTO, managerId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Create" operation asynchronously, returning `Task<bool>`. It accepts `req: CreateCouponRequestDTO, managerId: int` as input parameters. |
| 2 | GetAll(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool) : Task&lt;List&lt;CouponResponseDTO&gt;&gt; | Provides the concrete implementation to execute the "Get All" operation asynchronously, returning `Task<List<CouponResponseDTO>>`. It accepts `managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool` as input parameters. |
| 3 | NormalizeType(type: string) : string | Executes the "Normalize Type" operation, returning `string`. It accepts `type: string` as input parameters. |
| 4 | SoftDeleteAsync(id: int, userId: int, isAdmin: bool) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Soft Delete" operation asynchronously, returning `Task<bool>`. It accepts `id: int, userId: int, isAdmin: bool` as input parameters. |
| 5 | UpdateAsync(id: int, req: UpdateCouponRequestDTO, userId: int, isAdmin: bool) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update" operation asynchronously, returning `Task<bool>`. It accepts `id: int, req: UpdateCouponRequestDTO, userId: int, isAdmin: bool` as input parameters. |
| 6 | ValidateCreate(req: CreateCouponRequestDTO) | Executes the "Validate Create" operation, returning `void`. It accepts `req: CreateCouponRequestDTO` as input parameters. |
| 7 | _couponRepository : ICouponRepository | Provides the Coupon Repository dependency. |

### 13. CourseAiModerationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateModerationRequests(courseId: int, prep: PrepareForCourseAIModerationResult) : (SemanticDuplicationRequest, CourseHarmfulRequest) | Executes the "Create Moderation Requests" operation, returning `(SemanticDuplicationRequest, CourseHarmfulRequest)`. It accepts `courseId: int, prep: PrepareForCourseAIModerationResult` as input parameters. |
| 2 | GetCourseForModerationAsync(courseId: int) : Task&lt;CourseModerationDetailResponse?&gt; | Executes the "Get Course For Moderation" operation asynchronously, returning `Task<CourseModerationDetailResponse?>`. It accepts `courseId: int` as input parameters. |
| 3 | HandleCourseModerationWithAIAsync(request: CourseModerationRequest) : Task&lt;CourseModerationResult&gt; | Provides the concrete implementation to execute the "Handle Course Moderation With AI" operation asynchronously, returning `Task<CourseModerationResult>`. It accepts `request: CourseModerationRequest` as input parameters. |
| 4 | LogCourseAiModeration(command: LogCourseAiModerationCommand) : Task | Executes the "Log Course Ai Moderation" operation asynchronously, returning `Task`. It accepts `command: LogCourseAiModerationCommand` as input parameters. |
| 5 | NotifyManagersAsync(title: string, content: string, linkAction: string?) : Task | Executes the "Notify Managers" operation asynchronously, returning `Task`. It accepts `title: string, content: string, linkAction: string?` as input parameters. |
| 6 | PrepareForCourseAIModeration(courseId: int) : Task&lt;PrepareForCourseAIModerationResult&gt; | Executes the "Prepare For Course AIModeration" operation asynchronously, returning `Task<PrepareForCourseAIModerationResult>`. It accepts `courseId: int` as input parameters. |
| 7 | ResolveCourseAIModerationResult(result: CourseModerationResult) : Task | Executes the "Resolve Course AIModeration Result" operation asynchronously, returning `Task`. It accepts `result: CourseModerationResult` as input parameters. |
| 8 | StartCourseModerationAsync(request: CourseModerationRequest, instructorId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Start Course Moderation" operation asynchronously, returning `Task<bool>`. It accepts `request: CourseModerationRequest, instructorId: int` as input parameters. |
| 9 | UpdateCourseStatusAndClearCacheAsync(courseId: int, status: string, instructorId: int) : Task | Executes the "Update Course Status And Clear Cache" operation asynchronously, returning `Task`. It accepts `courseId: int, status: string, instructorId: int` as input parameters. |
| 10 | _aiModerationService : IAiModerationService | Provides the Ai Moderation Service dependency. |
| 11 | _courseCommandService : ICourseCommandService | Provides the Course Command Service dependency. |
| 12 | _redisService : IRedisService | Provides the Redis Service dependency. |
| 13 | _taskQueue : IBackgroundTaskQueue | Provides the Task Queue dependency. |

### 14. CourseCommandService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCourseAsync(request: CourseCreateRequest, instructorId: int) : Task&lt;CourseResponse&gt; | Provides the concrete implementation to execute the "Create Course" operation asynchronously, returning `Task<CourseResponse>`. It accepts `request: CourseCreateRequest, instructorId: int` as input parameters. |
| 2 | DeleteCourseAsync(courseId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Delete Course" operation asynchronously, returning `Task`. It accepts `courseId: int, instructorId: int` as input parameters. |
| 3 | ReactivateCourseContentAsync(courseId: int) : Task | Executes the "Reactivate Course Content" operation asynchronously, returning `Task`. It accepts `courseId: int` as input parameters. |
| 4 | UpdateCourseAsync(courseId: int, request: CourseUpdateRequest, instructorId: int) : Task&lt;CourseResponse&gt; | Provides the concrete implementation to execute the "Update Course" operation asynchronously, returning `Task<CourseResponse>`. It accepts `courseId: int, request: CourseUpdateRequest, instructorId: int` as input parameters. |
| 5 | UpdateCourseHashesAsync(course: Course) : Task | Executes the "Update Course Hashes" operation asynchronously, returning `Task`. It accepts `course: Course` as input parameters. |
| 6 | UpdateCourseStatusAndFeedbackAsync(courseId: int, status: string?, feedback: string?, threatLevel: AiThreatLevel?) : Task | Provides the concrete implementation to execute the "Update Course Status And Feedback" operation asynchronously, returning `Task`. It accepts `courseId: int, status: string?, feedback: string?, threatLevel: AiThreatLevel?` as input parameters. |
| 7 | UpdateCourseStatusAsync(courseId: int, status: string, instructorId: int) : Task | Provides the concrete implementation to execute the "Update Course Status" operation asynchronously, returning `Task`. It accepts `courseId: int, status: string, instructorId: int` as input parameters. |
| 8 | ValidateCourseDurationLimitsAsync(course: Course, instructorId: int) : Task | Executes the "Validate Course Duration Limits" operation asynchronously, returning `Task`. It accepts `course: Course, instructorId: int` as input parameters. |
| 9 | ValidateCourseStatusUpdateAsync(courseId: int, status: string, instructorId: int) : Task&lt;Course&gt; | Executes the "Validate Course Status Update" operation asynchronously, returning `Task<Course>`. It accepts `courseId: int, status: string, instructorId: int` as input parameters. |
| 10 | ValidateLandingPageRequirements(course: Course) | Executes the "Validate Landing Page Requirements" operation, returning `void`. It accepts `course: Course` as input parameters. |
| 11 | _contentHashService : IContentHashService | Provides the Content Hash Service dependency. |
| 12 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 13 | _instructorRepository : IInstructorRepository | Provides the Instructor Repository dependency. |
| 14 | _redisService : IRedisService | Provides the Redis Service dependency. |
| 15 | _uploadService : IFileUploadService | Provides the Upload Service dependency. |

### 15. CourseModerationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveCourseAsync(courseId: int, feedback: string?) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Approve Course" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int, feedback: string?` as input parameters. |
| 2 | FlagCourseAsync(courseId: int, reason: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Flag Course" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int, reason: string` as input parameters. |
| 3 | GetPendingCoursesAsync(filter: ModerationFilterDto) : Task&lt;PagedResult&gt;CourseModerationDto&lt;&gt; | Provides the concrete implementation to execute the "Get Pending Courses" operation asynchronously, returning `Task<PagedResult>CourseModerationDto<>`. It accepts `filter: ModerationFilterDto` as input parameters. |
| 4 | RejectCourseDetailedAsync(request: RejectCourseDetailedRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Reject Course Detailed" operation asynchronously, returning `Task<bool>`. It accepts `request: RejectCourseDetailedRequest` as input parameters. |
| 5 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 6 | _embeddingService : IEmbeddingService | Provides the Embedding Service dependency. |
| 7 | _enrollmentRepository : IEnrollmentRepository | Provides the Enrollment Repository dependency. |
| 8 | _lessonRepository : ILessonRepository | Provides the Lesson Repository dependency. |
| 9 | _materialRepository : IMaterialRepository | Provides the Material Repository dependency. |
| 10 | _notificationService : INotificationService | Provides the Notification Service dependency. |
| 11 | _redisService : IRedisService | Provides the Redis Service dependency. |

### 16. CourseQueryService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetCategoriesAsync() : Task&lt;IEnumerable&gt;CategoryResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Categories" operation asynchronously, returning `Task<IEnumerable>CategoryResponse<>`. |
| 2 | GetCourseWithDetailsAsync(courseId: int, userId: int?, userRole: string?) : Task&lt;CourseDetailResponse&gt; | Provides the concrete implementation to execute the "Get Course With Details" operation asynchronously, returning `Task<CourseDetailResponse>`. It accepts `courseId: int, userId: int?, userRole: string?` as input parameters. |
| 3 | GetInstructorCoursesPagedAsync(instructorId: int, search: string?, status: string?, page: int?, pageSize: int?) : Task&lt;PagedResult&gt;CourseResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Courses Paged" operation asynchronously, returning `Task<PagedResult>CourseResponse<>`. It accepts `instructorId: int, search: string?, status: string?, page: int?, pageSize: int?` as input parameters. |
| 4 | GetPublishedCoursesPagedAsync(query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?, userId: int?) : Task&lt;PagedResult&gt;CourseResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Published Courses Paged" operation asynchronously, returning `Task<PagedResult>CourseResponse<>`. It accepts `query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?, userId: int?` as input parameters. |
| 5 | _cartRepository : ICartRepository | Provides the Cart Repository dependency. |
| 6 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 7 | _instructorRepository : IInstructorRepository | Provides the Instructor Repository dependency. |
| 8 | _redisService : IRedisService | Provides the Redis Service dependency. |

### 17. EmailService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | SendEmailAsync(email: string, subject: string, body: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Send Email" operation asynchronously, returning `Task<bool>`. It accepts `email: string, subject: string, body: string` as input parameters. |

### 18. EmbeddingService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | PersistPendingMaterialEmbeddingsAsync(courseId: int, excludedMaterialIds: HashSet&lt;int&gt;) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Persist Pending Material Embeddings" operation asynchronously, returning `Task<int>`. It accepts `courseId: int, excludedMaterialIds: HashSet<int>` as input parameters. |

### 19. EnrollmentService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | EnrollFreeAsync(userId: int, courseId: int) : Task | Provides the concrete implementation to execute the "Enroll Free" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetMyEnrolledCoursesAsync(userId: int) : Task&lt;List&gt;EnrolledCourseDto&lt;&gt; | Provides the concrete implementation to execute the "Get My Enrolled Courses" operation asynchronously, returning `Task<List>EnrolledCourseDto<>`. It accepts `userId: int` as input parameters. |
| 3 | GetProgressAsync(userId: int, courseId: int) : Task&lt;ProgressResponse?&gt; | Provides the concrete implementation to execute the "Get Progress" operation asynchronously, returning `Task<ProgressResponse?>`. It accepts `userId: int, courseId: int` as input parameters. |
| 4 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 5 | _notificationService : INotificationService | Provides the Notification Service dependency. |
| 6 | _repo : IEnrollmentRepository | Provides the Repo dependency. |
| 7 | _userRepo : IUserRepository | Provides the User Repo dependency. |

### 20. FileUploadService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteFileAsync(url: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Delete File" operation asynchronously, returning `Task<bool>`. It accepts `url: string` as input parameters. |
| 2 | UploadImageAsync(avatarFile: IFormFile) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Upload Image" operation asynchronously, returning `Task<string>`. It accepts `avatarFile: IFormFile` as input parameters. |
| 3 | UploadImageAsync(file: IFormFile) : Task&lt;string?&gt; | Provides the concrete implementation to execute the "Upload Image" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |
| 4 | UploadImageAsync(file: IFormFile) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Upload Image" operation asynchronously, returning `Task<string>`. It accepts `file: IFormFile` as input parameters. |
| 5 | UploadVideoAsync(file: IFormFile) : Task&lt;string?&gt; | Provides the concrete implementation to execute the "Upload Video" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |

### 21. GiftCheckoutService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | InitiateGiftPaymentIntentAsync(userId: int, request: GiftCheckoutRequest) : Task&lt;CheckoutResponse&gt; | Provides the concrete implementation to execute the "Initiate Gift Payment Intent" operation asynchronously, returning `Task<CheckoutResponse>`. It accepts `userId: int, request: GiftCheckoutRequest` as input parameters. |
| 2 | ProcessPaymentIntentSuccessAsync(paymentIntentId: string) : Task | Provides the concrete implementation to execute the "Process Payment Intent Success" operation asynchronously, returning `Task`. It accepts `paymentIntentId: string` as input parameters. |
| 3 | _paymentGateway : IPaymentGatewayService | Provides the Payment Gateway dependency. |
| 4 | _repo : ICheckoutRepository | Provides the Repo dependency. |

### 22. GiftService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ClaimGiftAsync(userId: int, token: string) : Task | Provides the concrete implementation to execute the "Claim Gift" operation asynchronously, returning `Task`. It accepts `userId: int, token: string` as input parameters. |
| 2 | IsRecipientEnrolledAsync(email: string, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is Recipient Enrolled" operation asynchronously, returning `Task<bool>`. It accepts `email: string, courseId: int` as input parameters. |
| 3 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 4 | _enrollmentRepo : IEnrollmentRepository | Provides the Enrollment Repo dependency. |
| 5 | _giftRepo : IGiftRepository | Provides the Gift Repo dependency. |
| 6 | _transactionRepo : ITransactionRepository | Provides the Transaction Repo dependency. |
| 7 | _userRepo : IUserRepository | Provides the User Repo dependency. |

### 23. InstructorApprovalService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveOrRejectAsync(dto: UpdateApprovalStatusDto) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Approve Or Reject" operation asynchronously, returning `Task<bool>`. It accepts `dto: UpdateApprovalStatusDto` as input parameters. |
| 2 | GetDetailAsync(id: int) : Task&lt;InstructorApprovalDto?&gt; | Provides the concrete implementation to execute the "Get Detail" operation asynchronously, returning `Task<InstructorApprovalDto?>`. It accepts `id: int` as input parameters. |
| 3 | GetPendingListAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;InstructorApprovalDto&lt;&gt; | Provides the concrete implementation to execute the "Get Pending List" operation asynchronously, returning `Task<PagedResult>InstructorApprovalDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 4 | _notiService : INotificationService | Provides the Noti Service dependency. |
| 5 | _repo : IInstructorRepository | Provides the Repo dependency. |

### 24. InstructorService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetInstructorDashboardAsync(userId: int) : Task&lt;InstructorDashboardDto?&gt; | Provides the concrete implementation to execute the "Get Instructor Dashboard" operation asynchronously, returning `Task<InstructorDashboardDto?>`. It accepts `userId: int` as input parameters. |
| 2 | GetPayoutsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;InstructorPayoutDto&lt;&gt; | Provides the concrete implementation to execute the "Get Payouts" operation asynchronously, returning `Task<PagedResult>InstructorPayoutDto<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 3 | GetPublicProfileAsync(instructorId: int) : Task&lt;InstructorPublicProfileDto?&gt; | Provides the concrete implementation to execute the "Get Public Profile" operation asynchronously, returning `Task<InstructorPublicProfileDto?>`. It accepts `instructorId: int` as input parameters. |
| 4 | SetupStripePayoutAsync(userId: int) : Task&lt;StripeSetupResponse&gt; | Provides the concrete implementation to execute the "Setup Stripe Payout" operation asynchronously, returning `Task<StripeSetupResponse>`. It accepts `userId: int` as input parameters. |
| 5 | SubmitApplicationAsync(userId: int, request: InstructorApplicationRequest) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Submit Application" operation asynchronously, returning `Task<string>`. It accepts `userId: int, request: InstructorApplicationRequest` as input parameters. |
| 6 | VerifyStripeOnboardingAsync(instructorId: int) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Verify Stripe Onboarding" operation asynchronously, returning `Task<string>`. It accepts `instructorId: int` as input parameters. |
| 7 | _instructorRepository : IInstructorRepository | Provides the Instructor Repository dependency. |
| 8 | _repo : IInstructorRepository | Provides the Repo dependency. |
| 9 | _stripeConnect : IStripeConnectService | Provides the Stripe Connect dependency. |
| 10 | _uploadService : IFileUploadService | Provides the Upload Service dependency. |
| 11 | _userRepo : IUserRepository | Provides the User Repo dependency. |

### 25. LessonService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddMaterialToLessonAsync(lessonId: int, request: MaterialCreateRequest, instructorId: int) : Task&lt;MaterialResponse&gt; | Provides the concrete implementation to execute the "Add Material To Lesson" operation asynchronously, returning `Task<MaterialResponse>`. It accepts `lessonId: int, request: MaterialCreateRequest, instructorId: int` as input parameters. |
| 2 | CreateLessonAsync(request: LessonCreateRequest, instructorId: int) : Task&lt;LessonResponse&gt; | Provides the concrete implementation to execute the "Create Lesson" operation asynchronously, returning `Task<LessonResponse>`. It accepts `request: LessonCreateRequest, instructorId: int` as input parameters. |
| 3 | DeleteLessonAsync(lessonId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Delete Lesson" operation asynchronously, returning `Task`. It accepts `lessonId: int, instructorId: int` as input parameters. |
| 4 | GetTrashMaterialsAsync(instructorId: int) : Task&lt;IEnumerable&gt;MaterialTrashResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Trash Materials" operation asynchronously, returning `Task<IEnumerable>MaterialTrashResponse<>`. It accepts `instructorId: int` as input parameters. |
| 5 | PermanentDeleteMaterialAsync(materialId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Permanent Delete Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 6 | RemoveMaterialAsync(materialId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Remove Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 7 | RestoreMaterialAsync(materialId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Restore Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 8 | UpdateMaterialDetailsAsync(materialId: int, request: MaterialUpdateRequest, instructorId: int) : Task&lt;MaterialResponse&gt; | Provides the concrete implementation to execute the "Update Material Details" operation asynchronously, returning `Task<MaterialResponse>`. It accepts `materialId: int, request: MaterialUpdateRequest, instructorId: int` as input parameters. |
| 9 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 10 | _lessonRepo : ILessonRepository | Provides the Lesson Repo dependency. |
| 11 | _materialRepository : IMaterialRepository | Provides the Material Repository dependency. |
| 12 | _uploadService : IFileUploadService | Provides the Upload Service dependency. |

### 26. ManagerProfileService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetProfileAsync(managerId: int) : Task&lt;ManagerProfileResponse?&gt; | Provides the concrete implementation to execute the "Get Profile" operation asynchronously, returning `Task<ManagerProfileResponse?>`. It accepts `managerId: int` as input parameters. |
| 2 | UpdateProfileAsync(managerId: int, request: UpdateManagerProfileRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Profile" operation asynchronously, returning `Task<bool>`. It accepts `managerId: int, request: UpdateManagerProfileRequest` as input parameters. |
| 3 | _managerRepo : IManagerRepository | Provides the Manager Repo dependency. |
| 4 | _uploadService : IFileUploadService | Provides the Upload Service dependency. |

### 27. NotificationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteNotificationAsync(notiId: int, userId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Delete Notification" operation asynchronously, returning `Task<bool>`. It accepts `notiId: int, userId: int` as input parameters. |
| 2 | GetAccountRole(account: Account?, defaultRole: string) : string | Executes the "Get Account Role" operation, returning `string`. It accepts `account: Account?, defaultRole: string` as input parameters. |
| 3 | GetAllNotificationsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationResponseDto&lt;&gt; | Provides the concrete implementation to execute the "Get All Notifications" operation asynchronously, returning `Task<PagedResult>NotificationResponseDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 4 | GetAllNotificationsForAdminAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationAdminResponseDto&lt;&gt; | Provides the concrete implementation to execute the "Get All Notifications For Admin" operation asynchronously, returning `Task<PagedResult>NotificationAdminResponseDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 5 | GetNotificationDetailAsync(id: int, userId: int) : Task&lt;NotificationResponseDto?&gt; | Provides the concrete implementation to execute the "Get Notification Detail" operation asynchronously, returning `Task<NotificationResponseDto?>`. It accepts `id: int, userId: int` as input parameters. |
| 6 | GetNotificationsForUserAsync(userId: int, page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationResponseDto&lt;&gt; | Provides the concrete implementation to execute the "Get Notifications For User" operation asynchronously, returning `Task<PagedResult>NotificationResponseDto<>`. It accepts `userId: int, page: int, pageSize: int` as input parameters. |
| 7 | GetUnreadCountAsync(userId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `userId: int` as input parameters. |
| 8 | MapToAdminResponseDtos(notifications: IEnumerable&lt;Notification&gt;) : List&lt;NotificationAdminResponseDto&gt; | Executes the "Map To Admin Response Dtos" operation, returning `List<NotificationAdminResponseDto>`. It accepts `notifications: IEnumerable<Notification>` as input parameters. |
| 9 | MarkAllAsReadAsync(userId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Mark All As Read" operation asynchronously, returning `Task<bool>`. It accepts `userId: int` as input parameters. |
| 10 | MarkAsReadAsync(notificationId: int, userId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Mark As Read" operation asynchronously, returning `Task<bool>`. It accepts `notificationId: int, userId: int` as input parameters. |
| 11 | SearchEmailsAsync(query: string, senderId: int, senderRole: string) : Task&lt;List&gt;string&lt;&gt; | Provides the concrete implementation to execute the "Search Emails" operation asynchronously, returning `Task<List>string<>`. It accepts `query: string, senderId: int, senderRole: string` as input parameters. |
| 12 | SendAdvancedAsync(dto: NotificationAdvancedDto, senderId: int, senderRole: string) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Send Advanced" operation asynchronously, returning `Task<int>`. It accepts `dto: NotificationAdvancedDto, senderId: int, senderRole: string` as input parameters. |
| 13 | SendBulkNotificationsAsync(dtos: IEnumerable&lt;NotificationBulkDto&gt;) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Send Bulk Notifications" operation asynchronously, returning `Task<bool>`. It accepts `dtos: IEnumerable<NotificationBulkDto>` as input parameters. |
| 14 | SendNotificationAsync(receiverId: int, title: string, content: string, linkAction: string?) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Send Notification" operation asynchronously, returning `Task<bool>`. It accepts `receiverId: int, title: string, content: string, linkAction: string?` as input parameters. |
| 15 | _context : AppDbContext | Provides the Context dependency. |
| 16 | _hubContext : IHubContext&lt;NotificationHub&gt; | Provides the Hub Context dependency. |
| 17 | _repo : INotificationRepository | Provides the Repo dependency. |
| 18 | _userRepo : IUserRepository | Provides the User Repo dependency. |

### 28. OtpService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConsumeOtp(email: string, otp: string, purpose: string) : bool | Provides the concrete implementation to execute the "Consume Otp" operation, returning `bool`. It accepts `email: string, otp: string, purpose: string` as input parameters. |
| 2 | GenerateOtp(email: string, context: string) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Generate Otp" operation asynchronously, returning `Task<string>`. It accepts `email: string, context: string` as input parameters. |
| 3 | ValidateOtp(email: string, otp: string, context: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Validate Otp" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string, context: string` as input parameters. |

### 29. QuestionBankService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuestionAsync(courseId: int, request: QuizAddQuestionRequest, instructorId: int) : Task&lt;QuizQuestionResponse&gt; | Provides the concrete implementation to execute the "Add Question" operation asynchronously, returning `Task<QuizQuestionResponse>`. It accepts `courseId: int, request: QuizAddQuestionRequest, instructorId: int` as input parameters. |
| 2 | DeleteQuestionAsync(questionId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Delete Question" operation asynchronously, returning `Task`. It accepts `questionId: int, instructorId: int` as input parameters. |
| 3 | GetLessonsSummaryByCourseAsync(courseId: int, instructorId: int) : Task&lt;List&gt;QuestionBankLessonSummaryResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Lessons Summary By Course" operation asynchronously, returning `Task<List>QuestionBankLessonSummaryResponse<>`. It accepts `courseId: int, instructorId: int` as input parameters. |
| 4 | GetQuestionsByLessonAsync(lessonId: int, instructorId: int) : Task&lt;List&gt;QuizQuestionResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Questions By Lesson" operation asynchronously, returning `Task<List>QuizQuestionResponse<>`. It accepts `lessonId: int, instructorId: int` as input parameters. |
| 5 | UpdateQuestionAsync(questionId: int, request: QuizUpdateQuestionRequest, instructorId: int) : Task&lt;QuizQuestionResponse&gt; | Provides the concrete implementation to execute the "Update Question" operation asynchronously, returning `Task<QuizQuestionResponse>`. It accepts `questionId: int, request: QuizUpdateQuestionRequest, instructorId: int` as input parameters. |
| 6 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 7 | _lessonRepository : ILessonRepository | Provides the Lesson Repository dependency. |
| 8 | _questionBankRepository : IQuestionBankRepository | Provides the Question Bank Repository dependency. |

### 30. QuizService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuizToCourseAsync(request: AddQuizToCourseRequest, instructorId: int) : Task&lt;CourseQuizResponse&gt; | Provides the concrete implementation to execute the "Add Quiz To Course" operation asynchronously, returning `Task<CourseQuizResponse>`. It accepts `request: AddQuizToCourseRequest, instructorId: int` as input parameters. |
| 2 | CreateQuizAsync(request: QuizCreateRequest, instructorId: int) : Task&lt;QuizDetailResponse&gt; | Provides the concrete implementation to execute the "Create Quiz" operation asynchronously, returning `Task<QuizDetailResponse>`. It accepts `request: QuizCreateRequest, instructorId: int` as input parameters. |
| 3 | GetAttemptDetailAsync(attemptId: int, userId: int) : Task&lt;QuizAttemptDetailResponse&gt; | Provides the concrete implementation to execute the "Get Attempt Detail" operation asynchronously, returning `Task<QuizAttemptDetailResponse>`. It accepts `attemptId: int, userId: int` as input parameters. |
| 4 | GetMyQuizAttemptsAsync(quizId: int, userId: int, request: PagedRequestDto) : Task&lt;List&gt;QuizAttemptSummaryResponse&lt;&gt; | Provides the concrete implementation to execute the "Get My Quiz Attempts" operation asynchronously, returning `Task<List>QuizAttemptSummaryResponse<>`. It accepts `quizId: int, userId: int, request: PagedRequestDto` as input parameters. |
| 5 | GetMyQuizzesAsync(instructorId: int) : Task&lt;List&gt;QuizSummaryResponse&lt;&gt; | Provides the concrete implementation to execute the "Get My Quizzes" operation asynchronously, returning `Task<List>QuizSummaryResponse<>`. It accepts `instructorId: int` as input parameters. |
| 6 | GetQuizForStudentAsync(quizId: int, userId: int) : Task&lt;QuizForStudentResponse&gt; | Provides the concrete implementation to execute the "Get Quiz For Student" operation asynchronously, returning `Task<QuizForStudentResponse>`. It accepts `quizId: int, userId: int` as input parameters. |
| 7 | SoftDeleteQuizAsync(quizId: int, instructorId: int) : Task | Provides the concrete implementation to execute the "Soft Delete Quiz" operation asynchronously, returning `Task`. It accepts `quizId: int, instructorId: int` as input parameters. |
| 8 | SubmitAttemptAsync(request: QuizAttemptSubmitRequest, userId: int) : Task&lt;QuizAttemptResultResponse&gt; | Provides the concrete implementation to execute the "Submit Attempt" operation asynchronously, returning `Task<QuizAttemptResultResponse>`. It accepts `request: QuizAttemptSubmitRequest, userId: int` as input parameters. |
| 9 | UpdateQuizSettingsAsync(quizId: int, request: QuizUpdateRequest, instructorId: int) : Task&lt;QuizDetailResponse&gt; | Provides the concrete implementation to execute the "Update Quiz Settings" operation asynchronously, returning `Task<QuizDetailResponse>`. It accepts `quizId: int, request: QuizUpdateRequest, instructorId: int` as input parameters. |
| 10 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 11 | _quizRepo : IQuizRepository | Provides the Quiz Repo dependency. |
| 12 | _quizRepository : IQuizRepository | Provides the Quiz Repository dependency. |

### 31. RedisService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ClearUnreadCountAsync(accountId: int, chatId: int) : Task | Provides the concrete implementation to execute the "Clear Unread Count" operation asynchronously, returning `Task`. It accepts `accountId: int, chatId: int` as input parameters. |
| 2 | GetCacheAsync(key: string) : Task&lt;T?&gt; | Provides the concrete implementation to execute the "Get Cache" operation asynchronously, returning `Task<T?>`. It accepts `key: string` as input parameters. |
| 3 | GetUnreadCountAsync(accountId: int, chatId: int) : Task&lt;int&gt; | Provides the concrete implementation to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `accountId: int, chatId: int` as input parameters. |
| 4 | IncrementUnreadCountAsync(accountId: int, chatId: int) : Task | Provides the concrete implementation to execute the "Increment Unread Count" operation asynchronously, returning `Task`. It accepts `accountId: int, chatId: int` as input parameters. |
| 5 | IsUserOnlineAsync(accountId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is User Online" operation asynchronously, returning `Task<bool>`. It accepts `accountId: int` as input parameters. |
| 6 | RemoveCacheAsync(key: string) : Task | Provides the concrete implementation to execute the "Remove Cache" operation asynchronously, returning `Task`. It accepts `key: string` as input parameters. |
| 7 | RemoveCacheAsync(key: string) | Provides the concrete implementation to execute the "Remove Cache" operation asynchronously, returning `void`. It accepts `key: string` as input parameters. |
| 8 | SetCacheAsync(key: string, value: T, expiry: TimeSpan?) : Task | Provides the concrete implementation to execute the "Set Cache" operation asynchronously, returning `Task`. It accepts `key: string, value: T, expiry: TimeSpan?` as input parameters. |
| 9 | _db : IDatabase | Provides the Db dependency. |
| 10 | _redis : IConnectionMultiplexer | Provides the Redis dependency. |

### 32. ReportModerationService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApplyCoursePenaltyAsync(course: Course, removeContent: bool, resolutionNote: string) : Task | Executes the "Apply Course Penalty" operation asynchronously, returning `Task`. It accepts `course: Course, removeContent: bool, resolutionNote: string` as input parameters. |
| 2 | ApplyCourseReviewPenaltyAsync(review: CourseReview, removeContent: bool, resolutionNote: string) : Task | Executes the "Apply Course Review Penalty" operation asynchronously, returning `Task`. It accepts `review: CourseReview, removeContent: bool, resolutionNote: string` as input parameters. |
| 3 | ApplyLessonReviewPenaltyAsync(review: LessonReview, removeContent: bool, resolutionNote: string) : Task | Executes the "Apply Lesson Review Penalty" operation asynchronously, returning `Task`. It accepts `review: LessonReview, removeContent: bool, resolutionNote: string` as input parameters. |
| 4 | GetAllCourseReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;CourseReportDetailResponse&lt;&gt; | Provides the concrete implementation to execute the "Get All Course Reports" operation asynchronously, returning `Task<PagedResult>CourseReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 5 | GetAllCourseReviewReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;ReviewReportDetailResponse&lt;&gt; | Provides the concrete implementation to execute the "Get All Course Review Reports" operation asynchronously, returning `Task<PagedResult>ReviewReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 6 | GetAllLessonReviewReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;ReviewReportDetailResponse&lt;&gt; | Provides the concrete implementation to execute the "Get All Lesson Review Reports" operation asynchronously, returning `Task<PagedResult>ReviewReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 7 | GetCourseReportAndEntityAsync(reportId: int) : Task&lt;Tuple&gt;CourseReport, Course&lt;&gt; | Executes the "Get Course Report And Entity" operation asynchronously, returning `Task<Tuple>CourseReport, Course<>`. It accepts `reportId: int` as input parameters. |
| 8 | GetCourseReviewReportAndEntityAsync(reportId: int) : Task&lt;Tuple&gt;CourseReviewReport, CourseReview&lt;&gt; | Executes the "Get Course Review Report And Entity" operation asynchronously, returning `Task<Tuple>CourseReviewReport, CourseReview<>`. It accepts `reportId: int` as input parameters. |
| 9 | GetLessonReviewReportAndEntityAsync(reportId: int) : Task&lt;Tuple&gt;LessonReviewReport, LessonReview&lt;&gt; | Executes the "Get Lesson Review Report And Entity" operation asynchronously, returning `Task<Tuple>LessonReviewReport, LessonReview<>`. It accepts `reportId: int` as input parameters. |
| 10 | GetReportStatsAsync() : Task&lt;ReportStatsResponse&gt; | Provides the concrete implementation to execute the "Get Report Stats" operation asynchronously, returning `Task<ReportStatsResponse>`. |
| 11 | NotifyAdminOnEscalationAsync(status: string, reportId: int, reportType: string) : Task | Executes the "Notify Admin On Escalation" operation asynchronously, returning `Task`. It accepts `status: string, reportId: int, reportType: string` as input parameters. |
| 12 | NotifyReporterOfOutcomeAsync(reporterId: int?, linkAction: string, status: string, reportType: string) : Task | Executes the "Notify Reporter Of Outcome" operation asynchronously, returning `Task`. It accepts `reporterId: int?, linkAction: string, status: string, reportType: string` as input parameters. |
| 13 | ResolveCourseReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Resolve Course Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |
| 14 | ResolveCourseReviewReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Resolve Course Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |
| 15 | ResolveLessonReviewReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Resolve Lesson Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |
| 16 | ResolveLinkActionAsync(reportType: string, targetId: int?) : Task&lt;string&gt; | Executes the "Resolve Link Action" operation asynchronously, returning `Task<string>`. It accepts `reportType: string, targetId: int?` as input parameters. |
| 17 | SaveReportChangesAsync() : Task | Executes the "Save Report Changes" operation asynchronously, returning `Task`. |
| 18 | SendResolutionNotificationsAsync(status: string, reportId: int, reporterId: int?, reportType: string, targetId: int?) : Task | Executes the "Send Resolution Notifications" operation asynchronously, returning `Task`. It accepts `status: string, reportId: int, reporterId: int?, reportType: string, targetId: int?` as input parameters. |
| 19 | ValidateReportResolutionAccessAsync(currentStatus: string, resolverId: int) : Task | Executes the "Validate Report Resolution Access" operation asynchronously, returning `Task`. It accepts `currentStatus: string, resolverId: int` as input parameters. |
| 20 | _redisService : IRedisService | Provides the Redis Service dependency. |
| 21 | _reportRepo : IReportRepository | Provides the Report Repo dependency. |

### 33. ReportSubmissionService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCourseReportAsync(reporterId: int, request: CreateCourseReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Create Course Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateCourseReportRequest` as input parameters. |
| 2 | CreateCourseReviewReportAsync(reporterId: int, request: CreateCourseReviewReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Create Course Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateCourseReviewReportRequest` as input parameters. |
| 3 | CreateLessonReviewReportAsync(reporterId: int, request: CreateLessonReviewReportRequest) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Create Lesson Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateLessonReviewReportRequest` as input parameters. |
| 4 | NotifyManagersAsync(title: string, content: string, linkAction: string?) : Task | Executes the "Notify Managers" operation asynchronously, returning `Task`. It accepts `title: string, content: string, linkAction: string?` as input parameters. |
| 5 | SaveChangesAndHandleExceptionsAsync() : Task | Executes the "Save Changes And Handle Exceptions" operation asynchronously, returning `Task`. |
| 6 | ValidateCourseReportAsync(reporterId: int, courseId: int, reason: string) : Task | Executes the "Validate Course Report" operation asynchronously, returning `Task`. It accepts `reporterId: int, courseId: int, reason: string` as input parameters. |
| 7 | ValidateCourseReviewReportAsync(reporterId: int, courseReviewId: int, reason: string) : Task | Executes the "Validate Course Review Report" operation asynchronously, returning `Task`. It accepts `reporterId: int, courseReviewId: int, reason: string` as input parameters. |
| 8 | ValidateLessonReviewReportAsync(reporterId: int, lessonReviewId: int, reason: string) : Task | Executes the "Validate Lesson Review Report" operation asynchronously, returning `Task`. It accepts `reporterId: int, lessonReviewId: int, reason: string` as input parameters. |
| 9 | _reportRepo : IReportRepository | Provides the Report Repo dependency. |

### 34. ReviewService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteReviewAsync(userId: int, request: DeleteReviewRequest) : Task | Provides the concrete implementation to execute the "Delete Review" operation asynchronously, returning `Task`. It accepts `userId: int, request: DeleteReviewRequest` as input parameters. |
| 2 | GetCourseReviewsAsync(courseId: int, page: int, pageSize: int, starFilter: int?) : Task&lt;PagedResult&gt;ReviewResponse&lt;&gt; | Provides the concrete implementation to execute the "Get Course Reviews" operation asynchronously, returning `Task<PagedResult>ReviewResponse<>`. It accepts `courseId: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 3 | SubmitReviewAsync(userId: int, request: ReviewRequest, requireCompletion: bool) : Task | Provides the concrete implementation to execute the "Submit Review" operation asynchronously, returning `Task`. It accepts `userId: int, request: ReviewRequest, requireCompletion: bool` as input parameters. |
| 4 | _courseRepo : ICourseRepository | Provides the Course Repo dependency. |
| 5 | _enrollmentRepo : IEnrollmentRepository | Provides the Enrollment Repo dependency. |
| 6 | _lockoutRepo : IUserLockoutRepository | Provides the Lockout Repo dependency. |
| 7 | _reviewRepo : IReviewRepository | Provides the Review Repo dependency. |

### 35. StripeConnectService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreatePlatformWithdrawalAsync(amount: decimal, description: string, managerId: int) : Task&lt;StripeWithdrawalResponseDto&gt; | Provides the concrete implementation to execute the "Create Platform Withdrawal" operation asynchronously, returning `Task<StripeWithdrawalResponseDto>`. It accepts `amount: decimal, description: string, managerId: int` as input parameters. |
| 2 | GetAccountStatusAsync(stripeAccountId: string) : Task&lt;StripeAccountStatusDto&gt; | Provides the concrete implementation to execute the "Get Account Status" operation asynchronously, returning `Task<StripeAccountStatusDto>`. It accepts `stripeAccountId: string` as input parameters. |
| 3 | GetPlatformBalanceAsync() : Task&lt;PlatformBalanceResponse&gt; | Provides the concrete implementation to execute the "Get Platform Balance" operation asynchronously, returning `Task<PlatformBalanceResponse>`. |
| 4 | GetPlatformBalanceAsync() : Task&lt;StripePlatformBalanceDto&gt; | Provides the concrete implementation to execute the "Get Platform Balance" operation asynchronously, returning `Task<StripePlatformBalanceDto>`. |
| 5 | SetupExpressAccountAsync(userId: int, email: string, country: string, title: string, categories: string, existingStripeAccountId: string?) : Task&lt;StripeConnectSetupResponse&gt; | Provides the concrete implementation to execute the "Setup Express Account" operation asynchronously, returning `Task<StripeConnectSetupResponse>`. It accepts `userId: int, email: string, country: string, title: string, categories: string, existingStripeAccountId: string?` as input parameters. |
| 6 | _configuration : IConfiguration | Provides the Configuration dependency. |

### 36. StripePaymentService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CreatePaymentIntentAsync(...) : Task&lt;Tuple&gt; | Provides the concrete implementation to execute the "Create Payment Intent" operation asynchronously, returning `Task<Tuple>`. It accepts `...` as input parameters. |
| 2 | CreatePaymentIntentAsync(amount: decimal, currency: string, metadata: Dictionary&lt;string, string&gt;?) : Task&lt;Tuple&gt; | Provides the concrete implementation to execute the "Create Payment Intent" operation asynchronously, returning `Task<Tuple>`. It accepts `amount: decimal, currency: string, metadata: Dictionary<string, string>?` as input parameters. |
| 3 | GetPaymentIntentMetadataAsync(...) : Task&lt;Dictionary&gt; | Provides the concrete implementation to execute the "Get Payment Intent Metadata" operation asynchronously, returning `Task<Dictionary>`. It accepts `...` as input parameters. |
| 4 | GetPaymentIntentMetadataAsync(paymentIntentId: string) : Task&lt;Dictionary&gt;string, string&lt;&gt; | Provides the concrete implementation to execute the "Get Payment Intent Metadata" operation asynchronously, returning `Task<Dictionary>string, string<>`. It accepts `paymentIntentId: string` as input parameters. |
| 5 | RefundAsync(paymentIntentId: string, amount: decimal, reason: string?) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Refund" operation asynchronously, returning `Task<string>`. It accepts `paymentIntentId: string, amount: decimal, reason: string?` as input parameters. |
| 6 | ReverseTransferAsync(stripeTransferId: string) : Task&lt;string&gt; | Provides the concrete implementation to execute the "Reverse Transfer" operation asynchronously, returning `Task<string>`. It accepts `stripeTransferId: string` as input parameters. |

### 37. TransactionService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResultTransactionListDto&gt; | Provides the concrete implementation to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<PagedResultTransactionListDto>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 2 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Provides the concrete implementation to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 3 | GetTransactionDetailAsync(transactionId: int) : Task&lt;TransactionDetailDto?&gt; | Provides the concrete implementation to execute the "Get Transaction Detail" operation asynchronously, returning `Task<TransactionDetailDto?>`. It accepts `transactionId: int` as input parameters. |
| 4 | GetTransactionsAsync(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Provides the concrete implementation to execute the "Get Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 5 | GetUserTransactionsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Provides the concrete implementation to execute the "Get User Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |
| 6 | _repo : ITransactionRepository | Provides the Repo dependency. |
| 7 | _transactionRepository : ITransactionRepository | Provides the Transaction Repository dependency. |

### 38. UserProfileService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ChangePasswordAsync(userId: int, currentPassword: string, newPassword: string) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Change Password" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, currentPassword: string, newPassword: string` as input parameters. |
| 2 | GetUserProfileAsync(userId: int) : Task&lt;UserProfileResponse?&gt; | Provides the concrete implementation to execute the "Get User Profile" operation asynchronously, returning `Task<UserProfileResponse?>`. It accepts `userId: int` as input parameters. |
| 3 | GetUserProfileAsync(userId: int) : Task&lt;UserProfileResponseDTO?&gt; | Provides the concrete implementation to execute the "Get User Profile" operation asynchronously, returning `Task<UserProfileResponseDTO?>`. It accepts `userId: int` as input parameters. |
| 4 | UpdateProfileAsync(userId: int, request: UpdateProfileRequestDTO) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Update Profile" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, request: UpdateProfileRequestDTO` as input parameters. |
| 5 | _fileUploadService : IFileUploadService | Provides the File Upload Service dependency. |
| 6 | _userRepository : IUserRepository | Provides the User Repository dependency. |

### 39. WishlistService Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Add To Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetWishlistAsync(userId: int) : Task&lt;List&gt;WishlistResponseDTO&lt;&gt; | Provides the concrete implementation to execute the "Get Wishlist" operation asynchronously, returning `Task<List>WishlistResponseDTO<>`. It accepts `userId: int` as input parameters. |
| 3 | IsInWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Is In Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 4 | RemoveFromWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Provides the concrete implementation to execute the "Remove From Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 5 | _courseRepository : ICourseRepository | Provides the Course Repository dependency. |
| 6 | _wishlistRepository : IWishlistRepository | Provides the Wishlist Repository dependency. |

## Layer: Service Interface
### 1. IAdminAccountService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateStaffAsync(request: CreateStaffRequestDTO) : Task&lt;bool&gt; | Defines the contract to execute the "Create Staff" operation asynchronously, returning `Task<bool>`. It accepts `request: CreateStaffRequestDTO` as input parameters. |
| 2 | FlagAccountAsync(id: int, reason: string) : Task&lt;bool&gt; | Defines the contract to execute the "Flag Account" operation asynchronously, returning `Task<bool>`. It accepts `id: int, reason: string` as input parameters. |
| 3 | GetAccountDetailAsync(id: int) : Task&lt;AdminAccountDetailDto&gt; | Defines the contract to execute the "Get Account Detail" operation asynchronously, returning `Task<AdminAccountDetailDto>`. It accepts `id: int` as input parameters. |
| 4 | GetAccountTransactionsAsync(id: int) : Task&lt;AccountTransactionSummaryDto&gt; | Defines the contract to execute the "Get Account Transactions" operation asynchronously, returning `Task<AccountTransactionSummaryDto>`. It accepts `id: int` as input parameters. |
| 5 | GetAccountsPagedAsync(keyword: string?, role: string?, page: int, pageSize: int) : Task&lt;PagedResult&gt;AdminAccountListDto&lt;&gt; | Defines the contract to execute the "Get Accounts Paged" operation asynchronously, returning `Task<PagedResult>AdminAccountListDto<>`. It accepts `keyword: string?, role: string?, page: int, pageSize: int` as input parameters. |
| 6 | IsUsernameExistsAsync(username: string) : Task&lt;bool&gt; | Defines the contract to execute the "Is Username Exists" operation asynchronously, returning `Task<bool>`. It accepts `username: string` as input parameters. |
| 7 | ToggleBanAsync(id: int, adminId: int) : Task&lt;string&gt; | Defines the contract to execute the "Toggle Ban" operation asynchronously, returning `Task<string>`. It accepts `id: int, adminId: int` as input parameters. |
| 8 | UpdateStaffAsync(id: int, request: UpdateStaffRequestDTO) : Task&lt;bool&gt; | Defines the contract to execute the "Update Staff" operation asynchronously, returning `Task<bool>`. It accepts `id: int, request: UpdateStaffRequestDTO` as input parameters. |

### 2. IAdminFinanceService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveRefundAsync(transactionId: int, adminNote: string) : Task | Defines the contract to execute the "Approve Refund" operation asynchronously, returning `Task`. It accepts `transactionId: int, adminNote: string` as input parameters. |
| 2 | CreateWithdrawalAsync(request: WithdrawRequest, managerId: int) : Task&lt;WithdrawResponse&gt; | Defines the contract to execute the "Create Withdrawal" operation asynchronously, returning `Task<WithdrawResponse>`. It accepts `request: WithdrawRequest, managerId: int` as input parameters. |
| 3 | GetFinancialSummaryAsync(year: int?, month: int?) : Task&lt;FinancialSummaryResponse&gt; | Defines the contract to execute the "Get Financial Summary" operation asynchronously, returning `Task<FinancialSummaryResponse>`. It accepts `year: int?, month: int?` as input parameters. |
| 4 | GetInstructorCourseRevenuesByInstructorAsync(instructorId: int, year: int, month: int) : Task&lt;List&gt;InstructorCourseRevenueResponse&lt;&gt; | Defines the contract to execute the "Get Instructor Course Revenues By Instructor" operation asynchronously, returning `Task<List>InstructorCourseRevenueResponse<>`. It accepts `instructorId: int, year: int, month: int` as input parameters. |
| 5 | GetInstructorPayoutsAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;PagedResult&gt;PayoutDetailResponse&lt;&gt; | Defines the contract to execute the "Get Instructor Payouts" operation asynchronously, returning `Task<PagedResult>PayoutDetailResponse<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 6 | GetPayoutDaysConfigAsync() : Task&lt;string&gt; | Defines the contract to execute the "Get Payout Days Config" operation asynchronously, returning `Task<string>`. |
| 7 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Defines the contract to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 8 | GetPendingRefundRequestsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;Transaction&lt;&gt; | Defines the contract to execute the "Get Pending Refund Requests" operation asynchronously, returning `Task<PagedResult>Transaction<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 9 | GetPlatformBalanceAsync() : Task&lt;PlatformBalanceResponse&gt; | Defines the contract to execute the "Get Platform Balance" operation asynchronously, returning `Task<PlatformBalanceResponse>`. |
| 10 | GetWithdrawalHistoryAsync(year: int?, month: int?, page: int, pageSize: int) : Task&lt;PagedResult&gt;WithdrawalHistoryItem&lt;&gt; | Defines the contract to execute the "Get Withdrawal History" operation asynchronously, returning `Task<PagedResult>WithdrawalHistoryItem<>`. It accepts `year: int?, month: int?, page: int, pageSize: int` as input parameters. |
| 11 | RefundTransactionAsync(transactionId: int, reason: string?) : Task&lt;RefundResultResponse&gt; | Defines the contract to execute the "Refund Transaction" operation asynchronously, returning `Task<RefundResultResponse>`. It accepts `transactionId: int, reason: string?` as input parameters. |
| 12 | RejectRefundAsync(transactionId: int, adminNote: string) : Task | Defines the contract to execute the "Reject Refund" operation asynchronously, returning `Task`. It accepts `transactionId: int, adminNote: string` as input parameters. |
| 13 | RequestRefundAsync(transactionId: int, studentId: int, reason: string) : Task&lt;RefundResultDto&gt; | Defines the contract to execute the "Request Refund" operation asynchronously, returning `Task<RefundResultDto>`. It accepts `transactionId: int, studentId: int, reason: string` as input parameters. |
| 14 | SetPayoutDaysConfigAsync(payoutDays: string) : Task | Defines the contract to execute the "Set Payout Days Config" operation asynchronously, returning `Task`. It accepts `payoutDays: string` as input parameters. |
| 15 | SetTransferRateAsync(rate: decimal) : Task | Defines the contract to execute the "Set Transfer Rate" operation asynchronously, returning `Task`. It accepts `rate: decimal` as input parameters. |

### 3. IAiConfigurationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | UpdateThresholdsAsync(req: UpdateThresholdsRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Update Thresholds" operation asynchronously, returning `Task<bool>`. It accepts `req: UpdateThresholdsRequest` as input parameters. |

### 4. IAiModelManagementService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddModelAsync(req: CreateAiModelRequest) : Task&lt;AiModelAdminDto&gt; | Defines the contract to execute the "Add Model" operation asynchronously, returning `Task<AiModelAdminDto>`. It accepts `req: CreateAiModelRequest` as input parameters. |
| 2 | GetAllModelsAsync() : Task&lt;List&gt;AiModelAdminDto&lt;&gt; | Defines the contract to execute the "Get All Models" operation asynchronously, returning `Task<List>AiModelAdminDto<>`. |
| 3 | GetPagedModelsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;AiModelAdminDto&lt;&gt; | Defines the contract to execute the "Get Paged Models" operation asynchronously, returning `Task<PagedResult>AiModelAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 4 | ToggleModelStatusAsync(id: int) : Task&lt;bool&gt; | Defines the contract to execute the "Toggle Model Status" operation asynchronously, returning `Task<bool>`. It accepts `id: int` as input parameters. |
| 5 | UpdateModelAsync(id: int, req: UpdateAiModelRequest) : Task&lt;AiModelAdminDto&gt; | Defines the contract to execute the "Update Model" operation asynchronously, returning `Task<AiModelAdminDto>`. It accepts `id: int, req: UpdateAiModelRequest` as input parameters. |

### 5. IAiModerationLogService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetCourseModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;CourseModerationLogAdminDto&lt;&gt; | Defines the contract to execute the "Get Course Moderation Logs" operation asynchronously, returning `Task<PagedResult>CourseModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 2 | GetCourseReviewModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;ReviewModerationLogAdminDto&lt;&gt; | Defines the contract to execute the "Get Course Review Moderation Logs" operation asynchronously, returning `Task<PagedResult>ReviewModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |
| 3 | GetLessonReviewModerationLogsAsync(req: PagedRequestDto) : Task&lt;PagedResult&gt;ReviewModerationLogAdminDto&lt;&gt; | Defines the contract to execute the "Get Lesson Review Moderation Logs" operation asynchronously, returning `Task<PagedResult>ReviewModerationLogAdminDto<>`. It accepts `req: PagedRequestDto` as input parameters. |

### 6. IAiModerationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | HealthCheckAsync() : Task&lt;bool&gt; | Defines the contract to execute the "Health Check" operation asynchronously, returning `Task<bool>`. |
| 2 | ModerateCourseFullPipelineAsync(semanticReq: SemanticDuplicationRequest, harmfulReq: CourseHarmfulRequest) : Task&lt;CourseModerationResult&gt; | Defines the contract to execute the "Moderate Course Full Pipeline" operation asynchronously, returning `Task<CourseModerationResult>`. It accepts `semanticReq: SemanticDuplicationRequest, harmfulReq: CourseHarmfulRequest` as input parameters. |

### 7. IAuthService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ForgotPasswordAsync(email: string) : Task&lt;string&gt; | Defines the contract to execute the "Forgot Password" operation asynchronously, returning `Task<string>`. It accepts `email: string` as input parameters. |
| 2 | GoogleLoginAsync(request: GoogleLoginRequestDTO) : Task&lt;LoginResponseDTO?&gt; | Defines the contract to execute the "Google Login" operation asynchronously, returning `Task<LoginResponseDTO?>`. It accepts `request: GoogleLoginRequestDTO` as input parameters. |
| 3 | IsEmailVerifiedAsync(userId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Email Verified" operation asynchronously, returning `Task<bool>`. It accepts `userId: int` as input parameters. |
| 4 | LoginAsync(request: LoginRequestDTO) : Task&lt;LoginResponseDTO?&gt; | Defines the contract to execute the "Login" operation asynchronously, returning `Task<LoginResponseDTO?>`. It accepts `request: LoginRequestDTO` as input parameters. |
| 5 | LogoutAsync(accountId: int) : Task&lt;void&gt; | Defines the contract to execute the "Logout" operation asynchronously, returning `Task<void>`. It accepts `accountId: int` as input parameters. |
| 6 | RegisterAsync(request: RegisterRequestDTO) : Task&lt;string&gt; | Defines the contract to execute the "Register" operation asynchronously, returning `Task<string>`. It accepts `request: RegisterRequestDTO` as input parameters. |
| 7 | ResetPasswordAsync(email: string, otp: string, newPassword: string) : Task&lt;bool&gt; | Defines the contract to execute the "Reset Password" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string, newPassword: string` as input parameters. |
| 8 | VerifyEmailAsync(email: string, otp: string) : Task&lt;bool&gt; | Defines the contract to execute the "Verify Email" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string` as input parameters. |
| 9 | VerifyOtpForResetAsync(email: string, otp: string) : Task&lt;bool&gt; | Defines the contract to execute the "Verify Otp For Reset" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string` as input parameters. |

### 8. IBackgroundTaskQueue Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | QueueBackgroundWorkItemAsync&lt;ICourseAiModerationService&gt;(workItem: Func) : ValueTask | Defines the contract to execute the "Queue Background Work Item Async~ICourse Ai Moderation Service~" operation asynchronously, returning `ValueTask`. It accepts `workItem: Func` as input parameters. |

### 9. ICartService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToCartAsync(userId: int, courseId: int) : Task | Defines the contract to execute the "Add To Cart" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetCartSummaryAsync(userId: int, couponCode: string?) : Task&lt;CartSummaryResponseDTO&gt; | Defines the contract to execute the "Get Cart Summary" operation asynchronously, returning `Task<CartSummaryResponseDTO>`. It accepts `userId: int, couponCode: string?` as input parameters. |
| 3 | GetCartSummaryAsync(userId: int, couponCode: string?) : Task&lt;CartSummaryResponse&gt; | Defines the contract to execute the "Get Cart Summary" operation asynchronously, returning `Task<CartSummaryResponse>`. It accepts `userId: int, couponCode: string?` as input parameters. |
| 4 | RemoveFromCartAsync(userId: int, courseId: int) : Task | Defines the contract to execute the "Remove From Cart" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |

### 10. IChatService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ClearChatHistoryAsync(chatId: int, accountId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Clear Chat History" operation asynchronously, returning `Task<bool>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 2 | CreateSupportRequestAsync(senderId: int, dto: SupportRequestDto) : Task&lt;SupportTicketDto&gt; | Defines the contract to execute the "Create Support Request" operation asynchronously, returning `Task<SupportTicketDto>`. It accepts `senderId: int, dto: SupportRequestDto` as input parameters. |
| 3 | GetChatHistoryAsync(chatId: int, accountId: int) : Task&lt;List&gt;MessageDto&lt;&gt; | Defines the contract to execute the "Get Chat History" operation asynchronously, returning `Task<List>MessageDto<>`. It accepts `chatId: int, accountId: int` as input parameters. |
| 4 | GetMyChatsAsync(accountId: int) : Task&lt;List&gt;ChatListDto&lt;&gt; | Defines the contract to execute the "Get My Chats" operation asynchronously, returning `Task<List>ChatListDto<>`. It accepts `accountId: int` as input parameters. |
| 5 | GetOrCreateChatAsync(senderId: int, dto: CreateChatDto) : Task&lt;int&gt; | Defines the contract to execute the "Get Or Create Chat" operation asynchronously, returning `Task<int>`. It accepts `senderId: int, dto: CreateChatDto` as input parameters. |
| 6 | GetParticipantIdsAsync(chatId: int) : Task&lt;List&gt;int&lt;&gt; | Defines the contract to execute the "Get Participant Ids" operation asynchronously, returning `Task<List>int<>`. It accepts `chatId: int` as input parameters. |
| 7 | SaveMessageAsync(senderId: int, dto: SendMessageDto) : Task&lt;MessageDto&gt; | Defines the contract to execute the "Save Message" operation asynchronously, returning `Task<MessageDto>`. It accepts `senderId: int, dto: SendMessageDto` as input parameters. |
| 8 | SearchChatsAsync(accountId: int, query: string) : Task&lt;List&gt;ChatListDto&lt;&gt; | Defines the contract to execute the "Search Chats" operation asynchronously, returning `Task<List>ChatListDto<>`. It accepts `accountId: int, query: string` as input parameters. |

### 11. ICheckoutService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | InitiatePaymentIntentAsync(userId: string, couponCode: string) : Task&lt;CheckoutResponse&gt; | Defines the contract to execute the "Initiate Payment Intent" operation asynchronously, returning `Task<CheckoutResponse>`. It accepts `userId: string, couponCode: string` as input parameters. |
| 2 | ProcessPaymentIntentSuccessAsync(paymentIntentId: string) : Task | Defines the contract to execute the "Process Payment Intent Success" operation asynchronously, returning `Task`. It accepts `paymentIntentId: string` as input parameters. |

### 12. ICloudinaryUploadService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | UploadFileAsync(file: IFormFile) : Task&lt;string?&gt; | Defines the contract to execute the "Upload File" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |
| 2 | UploadImageAsync(file: IFormFile) : Task&lt;string?&gt; | Defines the contract to execute the "Upload Image" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |
| 3 | UploadVideoAsync(file: IFormFile) : Task&lt;string?&gt; | Defines the contract to execute the "Upload Video" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |

### 13. IContentHashService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ComputeCourseHashAsync(text: string) : Task&lt;string&gt; | Defines the contract to execute the "Compute Course Hash" operation asynchronously, returning `Task<string>`. It accepts `text: string` as input parameters. |
| 2 | SaveCourseHashesAsync(command: SaveCourseHashesCommand) : Task | Defines the contract to execute the "Save Course Hashes" operation asynchronously, returning `Task`. It accepts `command: SaveCourseHashesCommand` as input parameters. |

### 14. ICouponService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateAsync(req: CreateCouponRequestDTO, managerId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Create" operation asynchronously, returning `Task<bool>`. It accepts `req: CreateCouponRequestDTO, managerId: int` as input parameters. |
| 2 | GetAll(managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool) : Task&lt;List&lt;CouponResponseDTO&gt;&gt; | Defines the contract to execute the "Get All" operation asynchronously, returning `Task<List<CouponResponseDTO>>`. It accepts `managerId: int?, isActive: bool?, type: string?, search: string?, isAdmin: bool` as input parameters. |
| 3 | SoftDeleteAsync(id: int, userId: int, isAdmin: bool) : Task&lt;bool&gt; | Defines the contract to execute the "Soft Delete" operation asynchronously, returning `Task<bool>`. It accepts `id: int, userId: int, isAdmin: bool` as input parameters. |
| 4 | UpdateAsync(id: int, req: UpdateCouponRequestDTO, userId: int, isAdmin: bool) : Task&lt;bool&gt; | Defines the contract to execute the "Update" operation asynchronously, returning `Task<bool>`. It accepts `id: int, req: UpdateCouponRequestDTO, userId: int, isAdmin: bool` as input parameters. |

### 15. ICourseAiModerationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | HandleCourseModerationWithAIAsync(request: CourseModerationRequest) : Task&lt;CourseModerationResult&gt; | Defines the contract to execute the "Handle Course Moderation With AI" operation asynchronously, returning `Task<CourseModerationResult>`. It accepts `request: CourseModerationRequest` as input parameters. |
| 2 | StartCourseModerationAsync(request: CourseModerationRequest, instructorId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Start Course Moderation" operation asynchronously, returning `Task<bool>`. It accepts `request: CourseModerationRequest, instructorId: int` as input parameters. |

### 16. ICourseCommandService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCourseAsync(request: CourseCreateRequest, instructorId: int) : Task&lt;CourseResponse&gt; | Defines the contract to execute the "Create Course" operation asynchronously, returning `Task<CourseResponse>`. It accepts `request: CourseCreateRequest, instructorId: int` as input parameters. |
| 2 | DeleteCourseAsync(courseId: int, instructorId: int) : Task | Defines the contract to execute the "Delete Course" operation asynchronously, returning `Task`. It accepts `courseId: int, instructorId: int` as input parameters. |
| 3 | UpdateCourseAsync(courseId: int, request: CourseUpdateRequest, instructorId: int) : Task&lt;CourseResponse&gt; | Defines the contract to execute the "Update Course" operation asynchronously, returning `Task<CourseResponse>`. It accepts `courseId: int, request: CourseUpdateRequest, instructorId: int` as input parameters. |
| 4 | UpdateCourseStatusAndFeedbackAsync(courseId: int, status: string?, feedback: string?, threatLevel: AiThreatLevel?) : Task | Defines the contract to execute the "Update Course Status And Feedback" operation asynchronously, returning `Task`. It accepts `courseId: int, status: string?, feedback: string?, threatLevel: AiThreatLevel?` as input parameters. |
| 5 | UpdateCourseStatusAsync(courseId: int, status: string, instructorId: int) : Task | Defines the contract to execute the "Update Course Status" operation asynchronously, returning `Task`. It accepts `courseId: int, status: string, instructorId: int` as input parameters. |

### 17. ICourseModerationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveCourseAsync(courseId: int, feedback: string?) : Task&lt;bool&gt; | Defines the contract to execute the "Approve Course" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int, feedback: string?` as input parameters. |
| 2 | FlagCourseAsync(courseId: int, reason: string) : Task&lt;bool&gt; | Defines the contract to execute the "Flag Course" operation asynchronously, returning `Task<bool>`. It accepts `courseId: int, reason: string` as input parameters. |
| 3 | GetPendingCoursesAsync(filter: ModerationFilterDto) : Task&lt;PagedResult&gt;CourseModerationDto&lt;&gt; | Defines the contract to execute the "Get Pending Courses" operation asynchronously, returning `Task<PagedResult>CourseModerationDto<>`. It accepts `filter: ModerationFilterDto` as input parameters. |
| 4 | RejectCourseDetailedAsync(request: RejectCourseDetailedRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Reject Course Detailed" operation asynchronously, returning `Task<bool>`. It accepts `request: RejectCourseDetailedRequest` as input parameters. |

### 18. ICourseQueryService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetCategoriesAsync() : Task&lt;IEnumerable&gt;CategoryResponse&lt;&gt; | Defines the contract to execute the "Get Categories" operation asynchronously, returning `Task<IEnumerable>CategoryResponse<>`. |
| 2 | GetCourseWithDetailsAsync(courseId: int, userId: int?, userRole: string?) : Task&lt;CourseDetailResponse&gt; | Defines the contract to execute the "Get Course With Details" operation asynchronously, returning `Task<CourseDetailResponse>`. It accepts `courseId: int, userId: int?, userRole: string?` as input parameters. |
| 3 | GetInstructorCoursesPagedAsync(instructorId: int, search: string?, status: string?, page: int?, pageSize: int?) : Task&lt;PagedResult&gt;CourseResponse&lt;&gt; | Defines the contract to execute the "Get Instructor Courses Paged" operation asynchronously, returning `Task<PagedResult>CourseResponse<>`. It accepts `instructorId: int, search: string?, status: string?, page: int?, pageSize: int?` as input parameters. |
| 4 | GetPublishedCoursesPagedAsync(query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?, userId: int?) : Task&lt;PagedResult&gt;CourseResponse&lt;&gt; | Defines the contract to execute the "Get Published Courses Paged" operation asynchronously, returning `Task<PagedResult>CourseResponse<>`. It accepts `query: string?, category: string?, sort: string?, price: string?, rating: string?, page: int?, pageSize: int?, userId: int?` as input parameters. |

### 19. IEmailService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | SendEmailAsync(email: string, subject: string, body: string) : Task&lt;bool&gt; | Defines the contract to execute the "Send Email" operation asynchronously, returning `Task<bool>`. It accepts `email: string, subject: string, body: string` as input parameters. |

### 20. IEmbeddingService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | PersistPendingMaterialEmbeddingsAsync(courseId: int, excludedMaterialIds: HashSet&lt;int&gt;) : Task&lt;int&gt; | Defines the contract to execute the "Persist Pending Material Embeddings" operation asynchronously, returning `Task<int>`. It accepts `courseId: int, excludedMaterialIds: HashSet<int>` as input parameters. |

### 21. IEnrollmentService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | EnrollFreeAsync(userId: int, courseId: int) : Task | Defines the contract to execute the "Enroll Free" operation asynchronously, returning `Task`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetMyEnrolledCoursesAsync(userId: int) : Task&lt;List&gt;EnrolledCourseDto&lt;&gt; | Defines the contract to execute the "Get My Enrolled Courses" operation asynchronously, returning `Task<List>EnrolledCourseDto<>`. It accepts `userId: int` as input parameters. |
| 3 | GetProgressAsync(userId: int, courseId: int) : Task&lt;ProgressResponse?&gt; | Defines the contract to execute the "Get Progress" operation asynchronously, returning `Task<ProgressResponse?>`. It accepts `userId: int, courseId: int` as input parameters. |

### 22. IFileUploadService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteFileAsync(url: string) : Task&lt;bool&gt; | Defines the contract to execute the "Delete File" operation asynchronously, returning `Task<bool>`. It accepts `url: string` as input parameters. |
| 2 | UploadImageAsync(avatarFile: IFormFile) : Task&lt;string&gt; | Defines the contract to execute the "Upload Image" operation asynchronously, returning `Task<string>`. It accepts `avatarFile: IFormFile` as input parameters. |
| 3 | UploadImageAsync(file: IFormFile) : Task&lt;string?&gt; | Defines the contract to execute the "Upload Image" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |
| 4 | UploadImageAsync(file: IFormFile) : Task&lt;string&gt; | Defines the contract to execute the "Upload Image" operation asynchronously, returning `Task<string>`. It accepts `file: IFormFile` as input parameters. |
| 5 | UploadVideoAsync(file: IFormFile) : Task&lt;string?&gt; | Defines the contract to execute the "Upload Video" operation asynchronously, returning `Task<string?>`. It accepts `file: IFormFile` as input parameters. |

### 23. IGiftCheckoutService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | InitiateGiftPaymentIntentAsync(userId: int, request: GiftCheckoutRequest) : Task&lt;CheckoutResponse&gt; | Defines the contract to execute the "Initiate Gift Payment Intent" operation asynchronously, returning `Task<CheckoutResponse>`. It accepts `userId: int, request: GiftCheckoutRequest` as input parameters. |
| 2 | ProcessPaymentIntentSuccessAsync(paymentIntentId: string) : Task | Defines the contract to execute the "Process Payment Intent Success" operation asynchronously, returning `Task`. It accepts `paymentIntentId: string` as input parameters. |

### 24. IGiftService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ClaimGiftAsync(userId: int, token: string) : Task | Defines the contract to execute the "Claim Gift" operation asynchronously, returning `Task`. It accepts `userId: int, token: string` as input parameters. |
| 2 | IsRecipientEnrolledAsync(email: string, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is Recipient Enrolled" operation asynchronously, returning `Task<bool>`. It accepts `email: string, courseId: int` as input parameters. |

### 25. IInstructorApprovalService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ApproveOrRejectAsync(dto: UpdateApprovalStatusDto) : Task&lt;bool&gt; | Defines the contract to execute the "Approve Or Reject" operation asynchronously, returning `Task<bool>`. It accepts `dto: UpdateApprovalStatusDto` as input parameters. |
| 2 | GetDetailAsync(id: int) : Task&lt;InstructorApprovalDto?&gt; | Defines the contract to execute the "Get Detail" operation asynchronously, returning `Task<InstructorApprovalDto?>`. It accepts `id: int` as input parameters. |
| 3 | GetPendingListAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;InstructorApprovalDto&lt;&gt; | Defines the contract to execute the "Get Pending List" operation asynchronously, returning `Task<PagedResult>InstructorApprovalDto<>`. It accepts `page: int, pageSize: int` as input parameters. |

### 26. IInstructorService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetInstructorDashboardAsync(userId: int) : Task&lt;InstructorDashboardDto?&gt; | Defines the contract to execute the "Get Instructor Dashboard" operation asynchronously, returning `Task<InstructorDashboardDto?>`. It accepts `userId: int` as input parameters. |
| 2 | GetPayoutsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;InstructorPayoutDto&lt;&gt; | Defines the contract to execute the "Get Payouts" operation asynchronously, returning `Task<PagedResult>InstructorPayoutDto<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 3 | GetPublicProfileAsync(instructorId: int) : Task&lt;InstructorPublicProfileDto?&gt; | Defines the contract to execute the "Get Public Profile" operation asynchronously, returning `Task<InstructorPublicProfileDto?>`. It accepts `instructorId: int` as input parameters. |
| 4 | SetupStripePayoutAsync(userId: int) : Task&lt;StripeSetupResponse&gt; | Defines the contract to execute the "Setup Stripe Payout" operation asynchronously, returning `Task<StripeSetupResponse>`. It accepts `userId: int` as input parameters. |
| 5 | SubmitApplicationAsync(userId: int, request: InstructorApplicationRequest) : Task&lt;string&gt; | Defines the contract to execute the "Submit Application" operation asynchronously, returning `Task<string>`. It accepts `userId: int, request: InstructorApplicationRequest` as input parameters. |
| 6 | VerifyStripeOnboardingAsync(instructorId: int) : Task&lt;string&gt; | Defines the contract to execute the "Verify Stripe Onboarding" operation asynchronously, returning `Task<string>`. It accepts `instructorId: int` as input parameters. |

### 27. ILessonService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddMaterialToLessonAsync(lessonId: int, request: MaterialCreateRequest, instructorId: int) : Task&lt;MaterialResponse&gt; | Defines the contract to execute the "Add Material To Lesson" operation asynchronously, returning `Task<MaterialResponse>`. It accepts `lessonId: int, request: MaterialCreateRequest, instructorId: int` as input parameters. |
| 2 | CreateLessonAsync(request: LessonCreateRequest, instructorId: int) : Task&lt;LessonResponse&gt; | Defines the contract to execute the "Create Lesson" operation asynchronously, returning `Task<LessonResponse>`. It accepts `request: LessonCreateRequest, instructorId: int` as input parameters. |
| 3 | DeleteLessonAsync(lessonId: int, instructorId: int) : Task | Defines the contract to execute the "Delete Lesson" operation asynchronously, returning `Task`. It accepts `lessonId: int, instructorId: int` as input parameters. |
| 4 | GetTrashMaterialsAsync(instructorId: int) : Task&lt;IEnumerable&gt;MaterialTrashResponse&lt;&gt; | Defines the contract to execute the "Get Trash Materials" operation asynchronously, returning `Task<IEnumerable>MaterialTrashResponse<>`. It accepts `instructorId: int` as input parameters. |
| 5 | PermanentDeleteMaterialAsync(materialId: int, instructorId: int) : Task | Defines the contract to execute the "Permanent Delete Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 6 | RemoveMaterialAsync(materialId: int, instructorId: int) : Task | Defines the contract to execute the "Remove Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 7 | RestoreMaterialAsync(materialId: int, instructorId: int) : Task | Defines the contract to execute the "Restore Material" operation asynchronously, returning `Task`. It accepts `materialId: int, instructorId: int` as input parameters. |
| 8 | UpdateMaterialDetailsAsync(materialId: int, request: MaterialUpdateRequest, instructorId: int) : Task&lt;MaterialResponse&gt; | Defines the contract to execute the "Update Material Details" operation asynchronously, returning `Task<MaterialResponse>`. It accepts `materialId: int, request: MaterialUpdateRequest, instructorId: int` as input parameters. |

### 28. IManagerProfileService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetProfileAsync(managerId: int) : Task&lt;ManagerProfileResponse?&gt; | Defines the contract to execute the "Get Profile" operation asynchronously, returning `Task<ManagerProfileResponse?>`. It accepts `managerId: int` as input parameters. |
| 2 | UpdateProfileAsync(managerId: int, request: UpdateManagerProfileRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Update Profile" operation asynchronously, returning `Task<bool>`. It accepts `managerId: int, request: UpdateManagerProfileRequest` as input parameters. |

### 29. INotificationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteNotificationAsync(notiId: int, userId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Delete Notification" operation asynchronously, returning `Task<bool>`. It accepts `notiId: int, userId: int` as input parameters. |
| 2 | GetAllNotificationsAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationResponseDto&lt;&gt; | Defines the contract to execute the "Get All Notifications" operation asynchronously, returning `Task<PagedResult>NotificationResponseDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 3 | GetAllNotificationsForAdminAsync(page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationAdminResponseDto&lt;&gt; | Defines the contract to execute the "Get All Notifications For Admin" operation asynchronously, returning `Task<PagedResult>NotificationAdminResponseDto<>`. It accepts `page: int, pageSize: int` as input parameters. |
| 4 | GetNotificationDetailAsync(id: int, userId: int) : Task&lt;NotificationResponseDto?&gt; | Defines the contract to execute the "Get Notification Detail" operation asynchronously, returning `Task<NotificationResponseDto?>`. It accepts `id: int, userId: int` as input parameters. |
| 5 | GetNotificationsForUserAsync(userId: int, page: int, pageSize: int) : Task&lt;PagedResult&gt;NotificationResponseDto&lt;&gt; | Defines the contract to execute the "Get Notifications For User" operation asynchronously, returning `Task<PagedResult>NotificationResponseDto<>`. It accepts `userId: int, page: int, pageSize: int` as input parameters. |
| 6 | GetUnreadCountAsync(userId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `userId: int` as input parameters. |
| 7 | MarkAllAsReadAsync(userId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Mark All As Read" operation asynchronously, returning `Task<bool>`. It accepts `userId: int` as input parameters. |
| 8 | MarkAsReadAsync(notificationId: int, userId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Mark As Read" operation asynchronously, returning `Task<bool>`. It accepts `notificationId: int, userId: int` as input parameters. |
| 9 | SearchEmailsAsync(query: string, senderId: int, senderRole: string) : Task&lt;List&gt;string&lt;&gt; | Defines the contract to execute the "Search Emails" operation asynchronously, returning `Task<List>string<>`. It accepts `query: string, senderId: int, senderRole: string` as input parameters. |
| 10 | SendAdvancedAsync(dto: NotificationAdvancedDto, senderId: int, senderRole: string) : Task&lt;int&gt; | Defines the contract to execute the "Send Advanced" operation asynchronously, returning `Task<int>`. It accepts `dto: NotificationAdvancedDto, senderId: int, senderRole: string` as input parameters. |
| 11 | SendBulkNotificationsAsync(dtos: IEnumerable&lt;NotificationBulkDto&gt;) : Task&lt;bool&gt; | Defines the contract to execute the "Send Bulk Notifications" operation asynchronously, returning `Task<bool>`. It accepts `dtos: IEnumerable<NotificationBulkDto>` as input parameters. |
| 12 | SendNotificationAsync(receiverId: int, title: string, content: string, linkAction: string?) : Task&lt;bool&gt; | Defines the contract to execute the "Send Notification" operation asynchronously, returning `Task<bool>`. It accepts `receiverId: int, title: string, content: string, linkAction: string?` as input parameters. |

### 30. IOtpService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ConsumeOtp(email: string, otp: string, purpose: string) : bool | Defines the contract to execute the "Consume Otp" operation, returning `bool`. It accepts `email: string, otp: string, purpose: string` as input parameters. |
| 2 | GenerateOtp(email: string, context: string) : Task&lt;string&gt; | Defines the contract to execute the "Generate Otp" operation asynchronously, returning `Task<string>`. It accepts `email: string, context: string` as input parameters. |
| 3 | ValidateOtp(email: string, otp: string, context: string) : Task&lt;bool&gt; | Defines the contract to execute the "Validate Otp" operation asynchronously, returning `Task<bool>`. It accepts `email: string, otp: string, context: string` as input parameters. |

### 31. IPaymentGatewayService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreatePaymentIntentAsync(amount: decimal, currency: string, metadata: Dictionary&lt;string, string&gt;?) : Task&lt;Tuple&gt; | Defines the contract to execute the "Create Payment Intent" operation asynchronously, returning `Task<Tuple>`. It accepts `amount: decimal, currency: string, metadata: Dictionary<string, string>?` as input parameters. |
| 2 | GetPaymentIntentMetadataAsync(paymentIntentId: string) : Task&lt;Dictionary&gt; | Defines the contract to execute the "Get Payment Intent Metadata" operation asynchronously, returning `Task<Dictionary>`. It accepts `paymentIntentId: string` as input parameters. |
| 3 | GetPaymentIntentMetadataAsync(paymentIntentId: string) : Task&lt;Dictionary&gt;string, string&lt;&gt; | Defines the contract to execute the "Get Payment Intent Metadata" operation asynchronously, returning `Task<Dictionary>string, string<>`. It accepts `paymentIntentId: string` as input parameters. |
| 4 | RefundAsync(paymentIntentId: string, amount: decimal, reason: string?) : Task&lt;string&gt; | Defines the contract to execute the "Refund" operation asynchronously, returning `Task<string>`. It accepts `paymentIntentId: string, amount: decimal, reason: string?` as input parameters. |
| 5 | ReverseTransferAsync(stripeTransferId: string) : Task&lt;string&gt; | Defines the contract to execute the "Reverse Transfer" operation asynchronously, returning `Task<string>`. It accepts `stripeTransferId: string` as input parameters. |

### 32. IQuestionBankService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuestionAsync(courseId: int, request: QuizAddQuestionRequest, instructorId: int) : Task&lt;QuizQuestionResponse&gt; | Defines the contract to execute the "Add Question" operation asynchronously, returning `Task<QuizQuestionResponse>`. It accepts `courseId: int, request: QuizAddQuestionRequest, instructorId: int` as input parameters. |
| 2 | DeleteQuestionAsync(questionId: int, instructorId: int) : Task | Defines the contract to execute the "Delete Question" operation asynchronously, returning `Task`. It accepts `questionId: int, instructorId: int` as input parameters. |
| 3 | GetLessonsSummaryByCourseAsync(courseId: int, instructorId: int) : Task&lt;List&gt;QuestionBankLessonSummaryResponse&lt;&gt; | Defines the contract to execute the "Get Lessons Summary By Course" operation asynchronously, returning `Task<List>QuestionBankLessonSummaryResponse<>`. It accepts `courseId: int, instructorId: int` as input parameters. |
| 4 | GetQuestionsByLessonAsync(lessonId: int, instructorId: int) : Task&lt;List&gt;QuizQuestionResponse&lt;&gt; | Defines the contract to execute the "Get Questions By Lesson" operation asynchronously, returning `Task<List>QuizQuestionResponse<>`. It accepts `lessonId: int, instructorId: int` as input parameters. |
| 5 | UpdateQuestionAsync(questionId: int, request: QuizUpdateQuestionRequest, instructorId: int) : Task&lt;QuizQuestionResponse&gt; | Defines the contract to execute the "Update Question" operation asynchronously, returning `Task<QuizQuestionResponse>`. It accepts `questionId: int, request: QuizUpdateQuestionRequest, instructorId: int` as input parameters. |

### 33. IQuizService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddQuizToCourseAsync(request: AddQuizToCourseRequest, instructorId: int) : Task&lt;CourseQuizResponse&gt; | Defines the contract to execute the "Add Quiz To Course" operation asynchronously, returning `Task<CourseQuizResponse>`. It accepts `request: AddQuizToCourseRequest, instructorId: int` as input parameters. |
| 2 | CreateQuizAsync(request: QuizCreateRequest, instructorId: int) : Task&lt;QuizDetailResponse&gt; | Defines the contract to execute the "Create Quiz" operation asynchronously, returning `Task<QuizDetailResponse>`. It accepts `request: QuizCreateRequest, instructorId: int` as input parameters. |
| 3 | GetAttemptDetailAsync(attemptId: int, userId: int) : Task&lt;QuizAttemptDetailResponse&gt; | Defines the contract to execute the "Get Attempt Detail" operation asynchronously, returning `Task<QuizAttemptDetailResponse>`. It accepts `attemptId: int, userId: int` as input parameters. |
| 4 | GetMyQuizAttemptsAsync(quizId: int, userId: int, request: PagedRequestDto) : Task&lt;List&gt;QuizAttemptSummaryResponse&lt;&gt; | Defines the contract to execute the "Get My Quiz Attempts" operation asynchronously, returning `Task<List>QuizAttemptSummaryResponse<>`. It accepts `quizId: int, userId: int, request: PagedRequestDto` as input parameters. |
| 5 | GetMyQuizzesAsync(instructorId: int) : Task&lt;List&gt;QuizSummaryResponse&lt;&gt; | Defines the contract to execute the "Get My Quizzes" operation asynchronously, returning `Task<List>QuizSummaryResponse<>`. It accepts `instructorId: int` as input parameters. |
| 6 | GetQuizForStudentAsync(quizId: int, userId: int) : Task&lt;QuizForStudentResponse&gt; | Defines the contract to execute the "Get Quiz For Student" operation asynchronously, returning `Task<QuizForStudentResponse>`. It accepts `quizId: int, userId: int` as input parameters. |
| 7 | SoftDeleteQuizAsync(quizId: int, instructorId: int) : Task | Defines the contract to execute the "Soft Delete Quiz" operation asynchronously, returning `Task`. It accepts `quizId: int, instructorId: int` as input parameters. |
| 8 | SubmitAttemptAsync(request: QuizAttemptSubmitRequest, userId: int) : Task&lt;QuizAttemptResultResponse&gt; | Defines the contract to execute the "Submit Attempt" operation asynchronously, returning `Task<QuizAttemptResultResponse>`. It accepts `request: QuizAttemptSubmitRequest, userId: int` as input parameters. |
| 9 | UpdateQuizSettingsAsync(quizId: int, request: QuizUpdateRequest, instructorId: int) : Task&lt;QuizDetailResponse&gt; | Defines the contract to execute the "Update Quiz Settings" operation asynchronously, returning `Task<QuizDetailResponse>`. It accepts `quizId: int, request: QuizUpdateRequest, instructorId: int` as input parameters. |

### 34. IRedisService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ClearUnreadCountAsync(accountId: int, chatId: int) : Task | Defines the contract to execute the "Clear Unread Count" operation asynchronously, returning `Task`. It accepts `accountId: int, chatId: int` as input parameters. |
| 2 | GetCacheAsync(key: string) : Task&lt;T?&gt; | Defines the contract to execute the "Get Cache" operation asynchronously, returning `Task<T?>`. It accepts `key: string` as input parameters. |
| 3 | GetUnreadCountAsync(accountId: int, chatId: int) : Task&lt;int&gt; | Defines the contract to execute the "Get Unread Count" operation asynchronously, returning `Task<int>`. It accepts `accountId: int, chatId: int` as input parameters. |
| 4 | IncrementUnreadCountAsync(accountId: int, chatId: int) : Task | Defines the contract to execute the "Increment Unread Count" operation asynchronously, returning `Task`. It accepts `accountId: int, chatId: int` as input parameters. |
| 5 | IsUserOnlineAsync(accountId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is User Online" operation asynchronously, returning `Task<bool>`. It accepts `accountId: int` as input parameters. |
| 6 | RemoveCacheAsync(key: string) : Task | Defines the contract to execute the "Remove Cache" operation asynchronously, returning `Task`. It accepts `key: string` as input parameters. |
| 7 | RemoveCacheAsync(key: string) | Defines the contract to execute the "Remove Cache" operation asynchronously, returning `void`. It accepts `key: string` as input parameters. |
| 8 | SetCacheAsync(key: string, value: T, expiry: TimeSpan?) : Task | Defines the contract to execute the "Set Cache" operation asynchronously, returning `Task`. It accepts `key: string, value: T, expiry: TimeSpan?` as input parameters. |

### 35. IReportModerationService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetAllCourseReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;CourseReportDetailResponse&lt;&gt; | Defines the contract to execute the "Get All Course Reports" operation asynchronously, returning `Task<PagedResult>CourseReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 2 | GetAllCourseReviewReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;ReviewReportDetailResponse&lt;&gt; | Defines the contract to execute the "Get All Course Review Reports" operation asynchronously, returning `Task<PagedResult>ReviewReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 3 | GetAllLessonReviewReportsAsync(request: PagedReportRequestDto) : Task&lt;PagedResult&gt;ReviewReportDetailResponse&lt;&gt; | Defines the contract to execute the "Get All Lesson Review Reports" operation asynchronously, returning `Task<PagedResult>ReviewReportDetailResponse<>`. It accepts `request: PagedReportRequestDto` as input parameters. |
| 4 | GetReportStatsAsync() : Task&lt;ReportStatsResponse&gt; | Defines the contract to execute the "Get Report Stats" operation asynchronously, returning `Task<ReportStatsResponse>`. |
| 5 | ResolveCourseReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Resolve Course Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |
| 6 | ResolveCourseReviewReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Resolve Course Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |
| 7 | ResolveLessonReviewReportAsync(reportId: int, resolverId: int, request: ResolveReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Resolve Lesson Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reportId: int, resolverId: int, request: ResolveReportRequest` as input parameters. |

### 36. IReportSubmissionService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreateCourseReportAsync(reporterId: int, request: CreateCourseReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Create Course Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateCourseReportRequest` as input parameters. |
| 2 | CreateCourseReviewReportAsync(reporterId: int, request: CreateCourseReviewReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Create Course Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateCourseReviewReportRequest` as input parameters. |
| 3 | CreateLessonReviewReportAsync(reporterId: int, request: CreateLessonReviewReportRequest) : Task&lt;bool&gt; | Defines the contract to execute the "Create Lesson Review Report" operation asynchronously, returning `Task<bool>`. It accepts `reporterId: int, request: CreateLessonReviewReportRequest` as input parameters. |

### 37. IReviewService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | DeleteReviewAsync(userId: int, request: DeleteReviewRequest) : Task | Defines the contract to execute the "Delete Review" operation asynchronously, returning `Task`. It accepts `userId: int, request: DeleteReviewRequest` as input parameters. |
| 2 | GetCourseReviewsAsync(courseId: int, page: int, pageSize: int, starFilter: int?) : Task&lt;PagedResult&gt;ReviewResponse&lt;&gt; | Defines the contract to execute the "Get Course Reviews" operation asynchronously, returning `Task<PagedResult>ReviewResponse<>`. It accepts `courseId: int, page: int, pageSize: int, starFilter: int?` as input parameters. |
| 3 | SubmitReviewAsync(userId: int, request: ReviewRequest, requireCompletion: bool) : Task | Defines the contract to execute the "Submit Review" operation asynchronously, returning `Task`. It accepts `userId: int, request: ReviewRequest, requireCompletion: bool` as input parameters. |

### 38. IStripeConnectService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | CreatePlatformWithdrawalAsync(amount: decimal, description: string, managerId: int) : Task&lt;StripeWithdrawalResponseDto&gt; | Defines the contract to execute the "Create Platform Withdrawal" operation asynchronously, returning `Task<StripeWithdrawalResponseDto>`. It accepts `amount: decimal, description: string, managerId: int` as input parameters. |
| 2 | GetAccountStatusAsync(stripeAccountId: string) : Task&lt;StripeAccountStatusDto&gt; | Defines the contract to execute the "Get Account Status" operation asynchronously, returning `Task<StripeAccountStatusDto>`. It accepts `stripeAccountId: string` as input parameters. |
| 3 | GetPlatformBalanceAsync() : Task&lt;PlatformBalanceResponse&gt; | Defines the contract to execute the "Get Platform Balance" operation asynchronously, returning `Task<PlatformBalanceResponse>`. |
| 4 | GetPlatformBalanceAsync() : Task&lt;StripePlatformBalanceDto&gt; | Defines the contract to execute the "Get Platform Balance" operation asynchronously, returning `Task<StripePlatformBalanceDto>`. |
| 5 | SetupExpressAccountAsync(userId: int, email: string, country: string, title: string, categories: string, existingStripeAccountId: string?) : Task&lt;StripeConnectSetupResponse&gt; | Defines the contract to execute the "Setup Express Account" operation asynchronously, returning `Task<StripeConnectSetupResponse>`. It accepts `userId: int, email: string, country: string, title: string, categories: string, existingStripeAccountId: string?` as input parameters. |

### 39. ITransactionService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResultTransactionListDto&gt; | Defines the contract to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<PagedResultTransactionListDto>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 2 | GetInstructorTransactionsAsync(instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Defines the contract to execute the "Get Instructor Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `instructorId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 3 | GetTransactionDetailAsync(transactionId: int) : Task&lt;TransactionDetailDto?&gt; | Defines the contract to execute the "Get Transaction Detail" operation asynchronously, returning `Task<TransactionDetailDto?>`. It accepts `transactionId: int` as input parameters. |
| 4 | GetTransactionsAsync(page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Defines the contract to execute the "Get Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?, year: int?, month: int?` as input parameters. |
| 5 | GetUserTransactionsAsync(userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?) : Task&lt;PagedResult&gt;TransactionListDto&lt;&gt; | Defines the contract to execute the "Get User Transactions" operation asynchronously, returning `Task<PagedResult>TransactionListDto<>`. It accepts `userId: int, page: int, pageSize: int, keyword: string?, sortBy: string?, status: string?` as input parameters. |

### 40. IUserProfileService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | ChangePasswordAsync(userId: int, currentPassword: string, newPassword: string) : Task&lt;bool&gt; | Defines the contract to execute the "Change Password" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, currentPassword: string, newPassword: string` as input parameters. |
| 2 | GetUserProfileAsync(userId: int) : Task&lt;UserProfileResponse?&gt; | Defines the contract to execute the "Get User Profile" operation asynchronously, returning `Task<UserProfileResponse?>`. It accepts `userId: int` as input parameters. |
| 3 | GetUserProfileAsync(userId: int) : Task&lt;UserProfileResponseDTO?&gt; | Defines the contract to execute the "Get User Profile" operation asynchronously, returning `Task<UserProfileResponseDTO?>`. It accepts `userId: int` as input parameters. |
| 4 | UpdateProfileAsync(userId: int, request: UpdateProfileRequestDTO) : Task&lt;bool&gt; | Defines the contract to execute the "Update Profile" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, request: UpdateProfileRequestDTO` as input parameters. |

### 41. IWishlistService Interface
| No | Method / Property | Description |
|---|---|---|
| 1 | AddToWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Add To Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 2 | GetWishlistAsync(userId: int) : Task&lt;List&gt;WishlistResponseDTO&lt;&gt; | Defines the contract to execute the "Get Wishlist" operation asynchronously, returning `Task<List>WishlistResponseDTO<>`. It accepts `userId: int` as input parameters. |
| 3 | IsInWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Is In Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |
| 4 | RemoveFromWishlistAsync(userId: int, courseId: int) : Task&lt;bool&gt; | Defines the contract to execute the "Remove From Wishlist" operation asynchronously, returning `Task<bool>`. It accepts `userId: int, courseId: int` as input parameters. |

## Layer: View Model
### 1. AddMaterialViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | Duration : int? | Represents the Duration. |
| 3 | FileExtension : string? | Represents the File Extension. |
| 4 | FileSize : long? | Represents the File Size. |
| 5 | LessonId : int | Represents the Lesson ID. |
| 6 | MaterialUrl : string? | Represents the Material Url. |
| 7 | Title : string | Represents the Title. |
| 8 | Type : string | Represents the Type. |

### 2. AdminAccountDetailPageViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountDetail : AdminAccountDetailViewModel | Represents the Account Detail. |
| 2 | Transactions : AccountTransactionSummaryViewModel | Represents the Transactions. |

### 3. AdminAccountListViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Accounts : List&lt;AdminAccountListDto&gt; | Represents the Accounts. |
| 2 | CurrentPage : int | Represents the Current Page. |
| 3 | Keyword : string? | Represents the Keyword. |
| 4 | Role : string? | Represents the Role. |
| 5 | TotalPages : int | Represents the Total Pages. |

### 4. AdminAiServicePageViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AiModels : List&lt;AiModelViewModel&gt; | Represents the Ai Models. |
| 2 | AllActiveModels : List&lt;AiModelViewModel&gt; | Represents the All Active Models. |
| 3 | Config : AiConfigurationViewModel | Represents the Config. |
| 4 | CourseLogs : List&lt;CourseModerationLogViewModel&gt; | Represents the Course Logs. |
| 5 | CourseReviewLogs : List&lt;ReviewModerationLogViewModel&gt; | Represents the Course Review Logs. |
| 6 | LessonReviewLogs : List&lt;ReviewModerationLogViewModel&gt; | Represents the Lesson Review Logs. |

### 5. AdminFinanceUnifiedVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseRevenues : List&lt;InstructorCourseRevenueResponse&gt; | Represents the Course Revenues. |
| 2 | Dashboard : FinanceDashboardVM | Represents the Dashboard. |
| 3 | PayoutDays : string | Represents the Payout Days. |
| 4 | Payouts : PagedResult&lt;PayoutDetailVM&gt; | Represents the Payouts. |
| 5 | PendingRefunds : PagedResult&lt;RefundRequestVM&gt; | Represents the Pending Refunds. |
| 6 | Transactions : TransactionPagedVM | Represents the Transactions. |
| 7 | Withdraw : WithdrawPageVM | Represents the Withdraw. |
| 8 | Withdrawals : PagedResult&lt;WithdrawalHistoryVM&gt; | Represents the Withdrawals. |
| 9 | YearlySummaries : Dictionary&lt;int, FinancialSummaryVM&gt; | Represents the Yearly Summaries. |

### 6. AdminProfileViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountCreatedAt : DateTime? | Represents the Account Created At. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | Bio : string? | Represents the Bio. |
| 4 | DisplayName : string | Represents the Display Name. |
| 5 | Email : string | Represents the Email. |
| 6 | FullName : string? | Represents the Full Name. |
| 7 | LockoutEnd : DateTime? | Represents the Lockout End. |
| 8 | LockoutStart : DateTime? | Represents the Lockout Start. |
| 9 | ManagerId : int | Represents the Manager ID. |
| 10 | PhoneNumber : string? | Represents the Phone Number. |
| 11 | Role : string? | Represents the Role. |

### 7. AiModelViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelId : int | Represents the Model ID. |
| 3 | ModelName : string | Represents the Model Name. |
| 4 | ModelPath : string | Represents the Model Path. |
| 5 | ModelProvider : string | Represents the Model Provider. |
| 6 | ModelStatus : string | Represents the Model Status. |
| 7 | ModelType : string | Represents the Model Type. |
| 8 | ModelUpdatedAt : DateTime? | Represents the Model Updated At. |
| 9 | ModelVersion : string | Represents the Model Version. |
| 10 | ProcessType : string | Represents the Process Type. |

### 8. ApplyCouponViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CouponCode : string | Represents the Coupon Code. |

### 9. AvailableCouponViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConditionMessage : string | Represents the Condition Message. |
| 2 | CouponCode : string | Represents the Coupon Code. |
| 3 | CouponType : string | Represents the Coupon Type. |
| 4 | CourseId : int? | Represents the Course ID. |
| 5 | DiscountValue : decimal | Represents the Discount Value. |
| 6 | EndDate : DateTime? | Represents the End Date. |
| 7 | IsEligible : bool | Indicates whether it is eligible. |
| 8 | MinOrderValue : decimal | Represents the Min Order Value. |

### 10. BaseAiLogViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ErrorMessage : string? | Represents the Error Message. |
| 2 | InputJson : string? | Represents the Input Json. |
| 3 | LatencyMs : int | Represents the Latency Ms. |
| 4 | LogCreatedAt : DateTime | Represents the Log Created At. |
| 5 | LogId : int | Represents the Log ID. |
| 6 | ModelId : int | Represents the Model ID. |
| 7 | ModelName : string | Represents the Model Name. |
| 8 | OutputJson : string? | Represents the Output Json. |
| 9 | ResultStatus : string? | Represents the Result Status. |

### 11. CartItemViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | DiscountAmount : decimal | Represents the Discount Amount. |
| 4 | DiscountedPrice : decimal | Represents the Discounted Price. |
| 5 | InstructorName : string? | Represents the Instructor Name. |
| 6 | OriginalPrice : decimal | Represents the Original Price. |
| 7 | Price : decimal | Represents the Price. |
| 8 | ThumbnailUrl : string? | Represents the Thumbnail Url. |
| 9 | Title : string | Represents the Title. |

### 12. CartViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AppliedCouponCode : string? | Represents the Applied Coupon Code. |
| 2 | AvailableCoupons : List&lt;AvailableCouponViewModel&gt; | Represents the Available Coupons. |
| 3 | CouponMessage : string? | Represents the Coupon Message. |
| 4 | DiscountAmount : decimal | Represents the Discount Amount. |
| 5 | HasDiscount : bool | Indicates whether it has discount. |
| 6 | Items : List&lt;CartItemViewModel&gt; | Represents the Items. |
| 7 | SubTotal : decimal | Represents the Sub Total. |
| 8 | Subtotal : decimal | Represents the Subtotal. |
| 9 | Total : decimal | Represents the Total. |
| 10 | TotalAmount : decimal | Represents the Total Amount. |

### 13. CategoryViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoriesName : string | Represents the Categories Name. |
| 2 | CategoryId : int | Represents the Category ID. |

### 14. ChangePasswordViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmNewPassword : string | Represents the Confirm New Password. |
| 2 | CurrentPassword : string | Represents the Current Password. |
| 3 | NewPassword : string | Represents the New Password. |

### 15. CheckoutViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Cart : CartSummaryResponse | Represents the Cart. |
| 2 | ClientSecret : string | Represents the Client Secret. |
| 3 | Email : string | Represents the Email. |
| 4 | PaymentIntentId : string | Represents the Payment Intent ID. |
| 5 | PublishableKey : string | Represents the Publishable Key. |

### 16. CourseListViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | FlagCount : int | Represents the Flag Count. |
| 2 | Id : int | Represents the ID. |
| 3 | IsRemoved : bool | Indicates whether it is removed. |
| 4 | Rating : double | Represents the Rating. |
| 5 | Status : string | Represents the Status. |
| 6 | Students : int | Represents the Students. |
| 7 | ThumbnailUrl : string | Represents the Thumbnail Url. |
| 8 | Title : string | Represents the Title. |
| 9 | UpdatedAt : DateTime | Represents the Updated At. |

### 17. CourseModerationLogViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | InteractionType : string? | Represents the Interaction Type. |
| 3 | Title : string | Represents the Title. |
| 4 | TokenUsage : int | Represents the Token Usage. |

### 18. CourseModerationViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryName : string? | Represents the Category Name. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | CourseStatus : string? | Represents the Course Status. |
| 4 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | FlagCount : int | Represents the Flag Count. |
| 7 | InstructorName : string | Represents the Instructor Name. |
| 8 | IsRemoved : bool | Indicates whether it is removed. |
| 9 | Price : decimal | Represents the Price. |
| 10 | ThreatLevel : AiThreatLevel | Represents the Threat Level. |
| 11 | Title : string | Represents the Title. |
| 12 | UrgencyColor : string | Represents the Urgency Color. |
| 13 | UrgencyLevel : string | Represents the Urgency Level. |

### 19. CourseReportDetailViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccessGrantedUntil : DateTime? | Represents the Access Granted Until. |
| 2 | CourseFlagCount : int | Represents the Course Flag Count. |
| 3 | CourseId : int? | Represents the Course ID. |
| 4 | CourseTitle : string? | Represents the Course Title. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | Description : string? | Represents the Description. |
| 7 | InstructorEmail : string? | Represents the Instructor Email. |
| 8 | InstructorName : string? | Represents the Instructor Name. |
| 9 | Reason : string? | Represents the Reason. |
| 10 | ReportId : int | Represents the Report ID. |
| 11 | ReporterEmail : string? | Represents the Reporter Email. |
| 12 | ReporterId : int? | Represents the Reporter ID. |
| 13 | ReporterName : string? | Represents the Reporter Name. |
| 14 | ResolutionNote : string? | Represents the Resolution Note. |
| 15 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 16 | ResolverEmail : string? | Represents the Resolver Email. |
| 17 | Status : string? | Represents the Status. |

### 20. CreateAiModelRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelName : string | Represents the Model Name. |
| 3 | ModelPath : string | Represents the Model Path. |
| 4 | ModelProvider : string | Represents the Model Provider. |
| 5 | ModelStatus : string | Represents the Model Status. |
| 6 | ModelType : string | Represents the Model Type. |
| 7 | ModelVersion : string | Represents the Model Version. |
| 8 | ProcessType : string | Represents the Process Type. |

### 21. CreateCourseReportViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | Reason : string | Represents the Reason. |

### 22. CreateCourseViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvailableCategories : List&lt;CategoryViewModel&gt; | Represents the Available Categories. |
| 2 | CategoryId : int | Represents the Category ID. |
| 3 | CouponId : int? | Represents the Coupon ID. |
| 4 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 5 | Description : string? | Represents the Description. |
| 6 | Price : decimal | Represents the Price. |
| 7 | Requirements : string? | Represents the Requirements. |
| 8 | Title : string | Represents the Title. |
| 9 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 23. CreateReviewReportViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string? | Represents the Description. |
| 2 | Reason : string | Represents the Reason. |
| 3 | ReviewId : int | Represents the Review ID. |
| 4 | ReviewType : string | Represents the Review Type. |

### 24. FinanceDashboardVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Payouts : List&lt;PayoutDetailVM&gt; | Represents the Payouts. |
| 2 | Summary : FinancialSummaryVM | Represents the Summary. |

### 25. FinancialSummaryVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CurrentTransferRate : decimal | Represents the Current Transfer Rate. |
| 2 | GrossRevenue : decimal | Represents the Gross Revenue. |
| 3 | MaturedEscrow : decimal | Represents the Matured Escrow. |
| 4 | PendingEscrow : decimal | Represents the Pending Escrow. |
| 5 | PlatformNetProfit : decimal | Represents the Platform Net Profit. |
| 6 | TotalPaidOut : decimal | Represents the Total Paid Out. |
| 7 | TotalRefunded : decimal | Represents the Total Refunded. |
| 8 | TotalTransactions : int | Represents the Total Transactions. |

### 26. FlagAccountFERequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Reason : string | Represents the Reason. |

### 27. ForgotPasswordViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |

### 28. GiftSetupViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CardTheme : string | Represents the Card Theme. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | GiftMessage : string? | Represents the Gift Message. |
| 4 | RecipientEmail : string | Represents the Recipient Email. |
| 5 | RecipientName : string? | Represents the Recipient Name. |

### 29. GoogleLoginViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | IdToken : string | Represents the ID Token. |

### 30. InstructorApplyViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | DocumentFile : IFormFile? | Represents the Document File. |
| 2 | DocumentFiles : List&lt;IFormFile&gt;? | Represents the Document Files. |
| 3 | ExistingDocumentUrl : string? | Represents the Existing Document Url. |
| 4 | ExistingDocumentUrls : List&lt;string&gt;? | Represents the Existing Document Urls. |
| 5 | ExpertiseCategories : string | Represents the Expertise Categories. |
| 6 | FacebookUrl : string? | Represents the Facebook Url. |
| 7 | IsResubmit : bool | Indicates whether it is resubmit. |
| 8 | LinkedinUrl : string? | Represents the Linkedin Url. |
| 9 | ProfessionalTitle : string | Represents the Professional Title. |
| 10 | RejectionReason : string? | Represents the Rejection Reason. |
| 11 | RetainedDocumentUrls : List&lt;string&gt;? | Represents the Retained Document Urls. |
| 12 | StripeCountry : string | Represents the Stripe Country. |
| 13 | YoutubeUrl : string? | Represents the Youtube Url. |

### 31. InstructorApprovalViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ApprovalStatus : string? | Represents the Approval Status. |
| 2 | AvatarUrl : string? | Represents the Avatar Url. |
| 3 | DocumentUrl : string? | Represents the Document Url. |
| 4 | Email : string | Represents the Email. |
| 5 | ExpertiseCategories : string? | Represents the Expertise Categories. |
| 6 | FacebookUrl : string? | Represents the Facebook Url. |
| 7 | FullName : string | Represents the Full Name. |
| 8 | InstructorId : int | Represents the Instructor ID. |
| 9 | LinkedInUrl : string? | Represents the Linked In Url. |
| 10 | ProfessionalTitle : string? | Represents the Professional Title. |
| 11 | YoutubeUrl : string? | Represents the Youtube Url. |

### 32. InstructorFinancePageVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseRevenues : List&lt;InstructorCourseRevenueResponse&gt; | Represents the Course Revenues. |
| 2 | Payouts : InstructorPayoutPagedViewModel | Represents the Payouts. |
| 3 | Transactions : TransactionPagedVM | Represents the Transactions. |

### 33. InstructorPayoutPagedViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : List&lt;InstructorPayoutViewModel&gt; | Represents the Items. |
| 2 | Page : int | Represents the Page. |
| 3 | PageSize : int | Represents the Page Size. |
| 4 | TotalCount : int | Represents the Total Count. |
| 5 | TotalPages : int | Represents the Total Pages. |

### 34. InstructorPayoutViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | CourseTitle : string | Represents the Course Title. |
| 3 | IsPaid : bool | Indicates whether it is paid. |
| 4 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 5 | PayoutDate : DateTime | Represents the Payout Date. |
| 6 | PayoutId : int | Represents the Payout ID. |
| 7 | PayoutStatus : string? | Represents the Payout Status. |
| 8 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 9 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 10 | TotalAmount : decimal | Represents the Total Amount. |

### 35. InstructorStudioViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ActiveCoursesCount : int | Represents the Active Courses Count. |
| 2 | AverageRating : double | Represents the Average Rating. |
| 3 | Courses : List&lt;CourseListViewModel&gt; | Represents the Courses. |
| 4 | CurrentPage : int | Represents the Current Page. |
| 5 | DraftCoursesCount : int | Represents the Draft Courses Count. |
| 6 | EnrollmentGrowthPercentage : double | Represents the Enrollment Growth Percentage. |
| 7 | FilterStatus : string? | Represents the Filter Status. |
| 8 | InstructorRankPercentage : int | Represents the Instructor Rank Percentage. |
| 9 | PendingCoursesCount : int | Represents the Pending Courses Count. |
| 10 | SearchTerm : string? | Represents the Search Term. |
| 11 | TotalItems : int | Represents the Total Items. |
| 12 | TotalPages : int | Represents the Total Pages. |
| 13 | TotalRevenue : decimal | Represents the Total Revenue. |
| 14 | TotalStudents : int | Represents the Total Students. |

### 36. LoginViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Password : string | Represents the Password. |
| 2 | UsernameOrEmail : string | Represents the Username Or Email. |

### 37. MaterialTrashViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CloudPublicId : string? | Represents the Cloud Public ID. |
| 2 | CourseTitle : string? | Represents the Course Title. |
| 3 | DeletedAt : DateTime? | Represents the Deleted At. |
| 4 | FileType : string? | Represents the File Type. |
| 5 | LessonTitle : string? | Represents the Lesson Title. |
| 6 | MaterialId : int | Represents the Material ID. |
| 7 | Title : string | Represents the Title. |

### 38. NotificationViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Content : string | Represents the Content. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | IsRead : bool | Indicates whether it is read. |
| 4 | LinkAction : string? | Represents the Link Action. |
| 5 | NotificationId : int | Represents the Notification ID. |
| 6 | ReceiverEmail : string? | Represents the Receiver Email. |
| 7 | ReceiverFullName : string? | Represents the Receiver Full Name. |
| 8 | ReceiverId : int | Represents the Receiver ID. |
| 9 | ReceiverRole : string? | Represents the Receiver Role. |
| 10 | SenderId : int? | Represents the Sender ID. |
| 11 | SenderRole : string? | Represents the Sender Role. |
| 12 | Title : string | Represents the Title. |

### 39. PagedResult~RefundRequestVM~ Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : List&lt;RefundRequestVM&gt; | Represents the Items. |
| 2 | PageCount : int | Represents the Page Count. |
| 3 | TotalCount : int | Represents the Total Count. |

### 40. PayoutDetailVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseTitle : string | Represents the Course Title. |
| 2 | InstructorEmail : string | Represents the Instructor Email. |
| 3 | InstructorName : string | Represents the Instructor Name. |
| 4 | InstructorReceived : decimal | Represents the Instructor Received. |
| 5 | IsPaid : bool | Indicates whether it is paid. |
| 6 | PaidToBankAt : DateTime? | Represents the Paid To Bank At. |
| 7 | PayoutDate : DateTime | Represents the Payout Date. |
| 8 | PayoutId : int | Represents the Payout ID. |
| 9 | PayoutStatus : string | Represents the Payout Status. |
| 10 | PlatformReceived : decimal | Represents the Platform Received. |
| 11 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 12 | StripeTransferId : string? | Represents the Stripe Transfer ID. |
| 13 | TotalAmount : decimal | Represents the Total Amount. |
| 14 | TransactionDate : DateTime? | Represents the Transaction Date. |
| 15 | TransactionId : int | Represents the Transaction ID. |
| 16 | TransferRate : decimal | Represents the Transfer Rate. |

### 41. PlatformBalanceVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Available : decimal | Represents the Available. |
| 2 | Currency : string | Represents the Currency. |
| 3 | Incoming : decimal | Represents the Incoming. |
| 4 | PayoutScheduleAnchor : string? | Represents the Payout Schedule Anchor. |
| 5 | PayoutScheduleInterval : string | Represents the Payout Schedule Interval. |
| 6 | Total : decimal | Represents the Total. |

### 42. PublicCourseViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryName : string? | Represents the Category Name. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | CourseStatus : string? | Represents the Course Status. |
| 4 | CourseThumbnailUrl : string? | Represents the Course Thumbnail Url. |
| 5 | CreatedAt : DateTime? | Represents the Created At. |
| 6 | Description : string? | Represents the Description. |
| 7 | InstructorAvatarUrl : string? | Represents the Instructor Avatar Url. |
| 8 | InstructorBio : string? | Represents the Instructor Bio. |
| 9 | InstructorCoursesCount : int | Represents the Instructor Courses Count. |
| 10 | InstructorId : int? | Represents the Instructor ID. |
| 11 | InstructorName : string? | Represents the Instructor Name. |
| 12 | InstructorProfessionalTitle : string? | Represents the Instructor Professional Title. |
| 13 | InstructorReviewCount : int | Represents the Instructor Review Count. |
| 14 | InstructorStudentsCount : int | Represents the Instructor Students Count. |
| 15 | IsEnrolled : bool | Indicates whether it is enrolled. |
| 16 | IsInWishlist : bool | Indicates whether it is in wishlist. |
| 17 | IsOwner : bool | Indicates whether it is owner. |
| 18 | LastApprovedAt : DateTime? | Represents the Last Approved At. |
| 19 | Price : decimal | Represents the Price. |
| 20 | RatingAverage : decimal | Represents the Rating Average. |
| 21 | Requirements : string? | Represents the Requirements. |
| 22 | Title : string | Represents the Title. |
| 23 | TotalReviews : int | Represents the Total Reviews. |
| 24 | TotalStudents : int | Represents the Total Students. |
| 25 | WhatYouWillLearn : string? | Represents the What You Will Learn. |

### 43. QuizCreateViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | PassingScore : int | Represents the Passing Score. |
| 4 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 5 | Title : string | Represents the Title. |
| 6 | TotalQuestions : int | Represents the Total Questions. |

### 44. QuizLessonDistributionItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | LessonId : int | Represents the Lesson ID. |
| 2 | QuestionCount : int | Represents the Question Count. |

### 45. QuizListViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvailableCourses : List&lt;CourseListViewModel&gt; | Represents the Available Courses. |
| 2 | CurrentPage : int | Represents the Current Page. |
| 3 | FilterStatus : string? | Represents the Filter Status. |
| 4 | Quizzes : List&lt;QuizSummaryItem&gt; | Represents the Quizzes. |
| 5 | SearchTerm : string? | Represents the Search Term. |
| 6 | TotalCount : int | Represents the Total Count. |
| 7 | TotalPages : int | Represents the Total Pages. |

### 46. QuizSummaryItem Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseTitle : string | Represents the Course Title. |
| 2 | CreatedAt : DateTime? | Represents the Created At. |
| 3 | Description : string? | Represents the Description. |
| 4 | IsHidden : bool | Indicates whether it is hidden. |
| 5 | IsRemoved : bool | Indicates whether it is removed. |
| 6 | PassingScore : int | Represents the Passing Score. |
| 7 | QuestionCount : int | Represents the Question Count. |
| 8 | QuizId : int | Represents the Quiz ID. |
| 9 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 10 | Title : string | Represents the Title. |
| 11 | UpdatedAt : DateTime? | Represents the Updated At. |

### 47. QuizUpdateViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int | Represents the Course ID. |
| 2 | Description : string? | Represents the Description. |
| 3 | Distributions : List&lt;QuizLessonDistributionItem&gt; | Represents the Distributions. |
| 4 | PassingScore : int | Represents the Passing Score. |
| 5 | TimeLimitMinutes : int? | Represents the Time Limit Minutes. |
| 6 | Title : string | Represents the Title. |
| 7 | TotalQuestions : int | Represents the Total Questions. |

### 48. RefundRequestVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountFromNavigation : AccountDto? | Represents the Account From Navigation. |
| 2 | Amount : decimal | Represents the Amount. |
| 3 | BuyerName : string | Represents the Buyer Name. |
| 4 | CourseTitle : string | Represents the Course Title. |
| 5 | Currency : string? | Represents the Currency. |
| 6 | OrderItem : OrderItemDto? | Represents the Order Item. |
| 7 | RefundReason : string? | Represents the Refund Reason. |
| 8 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 9 | TransactionCreatedAt : DateTime? | Represents the Transaction Created At. |
| 10 | TransactionExt : TransactionExtDto? | Represents the Transaction Ext. |
| 11 | TransactionId : int | Represents the Transaction ID. |

### 49. RegisterViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmPassword : string | Represents the Confirm Password. |
| 2 | Email : string | Represents the Email. |
| 3 | FullName : string | Represents the Full Name. |
| 4 | Password : string | Represents the Password. |
| 5 | Username : string | Represents the Username. |

### 50. ReportModerationPageViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseReports : PagedResult&lt;CourseReportDetailViewModel&gt; | Represents the Course Reports. |
| 2 | CourseReviewReports : PagedResult&lt;ReviewReportDetailViewModel&gt; | Represents the Course Review Reports. |
| 3 | LessonReviewReports : PagedResult&lt;ReviewReportDetailViewModel&gt; | Represents the Lesson Review Reports. |
| 4 | Stats : ReportStatsViewModel | Represents the Stats. |

### 51. ReportStatsViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | TotalPending : int | Represents the Total Pending. |
| 2 | TotalPendingCourseReports : int | Represents the Total Pending Course Reports. |
| 3 | TotalPendingCourseReviewReports : int | Represents the Total Pending Course Review Reports. |
| 4 | TotalPendingLessonReviewReports : int | Represents the Total Pending Lesson Review Reports. |
| 5 | TotalRejectedToday : int | Represents the Total Rejected Today. |
| 6 | TotalResolvedToday : int | Represents the Total Resolved Today. |

### 52. ResetPasswordViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | ConfirmPassword : string | Represents the Confirm Password. |
| 2 | Email : string | Represents the Email. |
| 3 | NewPassword : string | Represents the New Password. |
| 4 | OtpCode : string | Represents the Otp Code. |

### 53. ResolveReportViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | RemoveContent : bool | Represents the Remove Content. |
| 2 | ResolutionNote : string? | Represents the Resolution Note. |
| 3 | Status : string | Represents the Status. |

### 54. ReviewModerationLogViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Comment : string | Represents the Comment. |
| 2 | ReviewId : int | Represents the Review ID. |

### 55. ReviewReportDetailViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CourseId : int? | Represents the Course ID. |
| 2 | CourseTitle : string? | Represents the Course Title. |
| 3 | CreatedAt : DateTime? | Represents the Created At. |
| 4 | Description : string? | Represents the Description. |
| 5 | LessonId : int? | Represents the Lesson ID. |
| 6 | LessonTitle : string? | Represents the Lesson Title. |
| 7 | Reason : string? | Represents the Reason. |
| 8 | ReportId : int | Represents the Report ID. |
| 9 | ReporterEmail : string? | Represents the Reporter Email. |
| 10 | ReporterId : int? | Represents the Reporter ID. |
| 11 | ReporterName : string? | Represents the Reporter Name. |
| 12 | ResolutionNote : string? | Represents the Resolution Note. |
| 13 | ResolvedAt : DateTime? | Represents the Resolved At. |
| 14 | ResolverEmail : string? | Represents the Resolver Email. |
| 15 | ReviewAuthorEmail : string? | Represents the Review Author Email. |
| 16 | ReviewAuthorName : string? | Represents the Review Author Name. |
| 17 | ReviewComment : string? | Represents the Review Comment. |
| 18 | ReviewId : int? | Represents the Review ID. |
| 19 | ReviewRating : float? | Represents the Review Rating. |
| 20 | ReviewType : string | Represents the Review Type. |
| 21 | Status : string? | Represents the Status. |

### 56. TransactionDetailVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | BuyerEmail : string | Represents the Buyer Email. |
| 2 | BuyerName : string | Represents the Buyer Name. |
| 3 | CouponCode : string? | Represents the Coupon Code. |
| 4 | CouponDiscountValue : decimal? | Represents the Coupon Discount Value. |
| 5 | CouponType : string? | Represents the Coupon Type. |
| 6 | CouponUsed : bool | Represents the Coupon Used. |
| 7 | CourseThumbnail : string? | Represents the Course Thumbnail. |
| 8 | CourseTitle : string | Represents the Course Title. |
| 9 | Currency : string? | Represents the Currency. |
| 10 | Date : DateTime? | Represents the Date. |
| 11 | DiscountAmount : decimal | Represents the Discount Amount. |
| 12 | GrossAmount : decimal | Represents the Gross Amount. |
| 13 | InstructorEmail : string | Represents the Instructor Email. |
| 14 | InstructorName : string | Represents the Instructor Name. |
| 15 | InstructorPayout : decimal | Represents the Instructor Payout. |
| 16 | IsPaid : bool | Indicates whether it is paid. |
| 17 | OriginalPrice : decimal | Represents the Original Price. |
| 18 | PlatformProfit : decimal | Represents the Platform Profit. |
| 19 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 20 | RefundReason : string? | Represents the Refund Reason. |
| 21 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 22 | Status : string? | Represents the Status. |
| 23 | StripePaymentIntentId : string? | Represents the Stripe Payment Intent ID. |
| 24 | StripeSessionId : string? | Represents the Stripe Session ID. |
| 25 | TransactionId : int | Represents the Transaction ID. |
| 26 | TransferRate : decimal | Represents the Transfer Rate. |

### 57. TransactionExtVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | RefundReason : string? | Represents the Refund Reason. |
| 2 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |

### 58. TransactionListItemVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | BuyerName : string | Represents the Buyer Name. |
| 3 | CourseTitle : string | Represents the Course Title. |
| 4 | CreatedAt : DateTime? | Represents the Created At. |
| 5 | Currency : string? | Represents the Currency. |
| 6 | Date : DateTime? | Represents the Date. |
| 7 | InstructorName : string | Represents the Instructor Name. |
| 8 | IsCourseOwner : bool | Indicates whether it is course owner. |
| 9 | IsGift : bool | Indicates whether it is gift. |
| 10 | IsGiftClaimed : bool | Indicates whether it is gift claimed. |
| 11 | PaymentMethod : string? | Represents the Payment Method. |
| 12 | PayoutCurrency : string? | Represents the Payout Currency. |
| 13 | RecipientEmail : string? | Represents the Recipient Email. |
| 14 | RefundAdminNote : string? | Represents the Refund Admin Note. |
| 15 | RefundReason : string? | Represents the Refund Reason. |
| 16 | RefundRequestedAt : DateTime? | Represents the Refund Requested At. |
| 17 | Status : string? | Represents the Status. |
| 18 | StripeSessionId : string? | Represents the Stripe Session ID. |
| 19 | TransactionExt : TransactionExtVM? | Represents the Transaction Ext. |
| 20 | TransactionId : int | Represents the Transaction ID. |
| 21 | UserEmail : string? | Represents the User Email. |
| 22 | UserName : string? | Represents the User Name. |

### 59. TransactionPagedVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Items : List&lt;TransactionListItemVM&gt; | Represents the Items. |
| 2 | Page : int | Represents the Page. |
| 3 | PageSize : int | Represents the Page Size. |
| 4 | TotalCount : int | Represents the Total Count. |
| 5 | TotalPages : int | Represents the Total Pages. |

### 60. UpdateAdminProfileViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvatarFile : IFormFile? | Represents the Avatar File. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | DisplayName : string | Represents the Display Name. |
| 4 | FullName : string? | Represents the Full Name. |
| 5 | PhoneNumber : string? | Represents the Phone Number. |

### 61. UpdateAiModelRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Description : string | Represents the Description. |
| 2 | ModelPath : string | Represents the Model Path. |
| 3 | ModelProvider : string | Represents the Model Provider. |
| 4 | ModelVersion : string | Represents the Model Version. |

### 62. UpdateCourseDetailsViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | CategoryId : int | Represents the Category ID. |
| 2 | CourseId : int | Represents the Course ID. |
| 3 | Description : string | Represents the Description. |
| 4 | Price : decimal | Represents the Price. |
| 5 | Requirements : string | Represents the Requirements. |
| 6 | ThumbnailUrl : string | Represents the Thumbnail Url. |
| 7 | Title : string | Represents the Title. |
| 8 | WhatYouWillLearn : string | Represents the What You Will Learn. |

### 63. UpdateProfileViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AvatarFile : IFormFile? | Represents the Avatar File. |
| 2 | Bio : string? | Represents the Bio. |
| 3 | DateOfBirth : DateTime? | Represents the Date Of Birth. |
| 4 | Email : string | Represents the Email. |
| 5 | FullName : string | Represents the Full Name. |
| 6 | PhoneNumber : string? | Represents the Phone Number. |

### 64. UpdateStaffFERequestDTO Class
| No | Method / Property | Description |
|---|---|---|
| 1 | AccountStatus : string | Represents the Account Status. |
| 2 | DisplayName : string | Represents the Display Name. |
| 3 | Password : string? | Represents the Password. |
| 4 | PhoneNumber : string? | Represents the Phone Number. |

### 65. UpdateThresholdsRequest Class
| No | Method / Property | Description |
|---|---|---|
| 1 | SimilarityScoreThreshold : float | Represents the Similarity Score Threshold. |
| 2 | SpamConfidenceThreshold : float | Represents the Spam Confidence Threshold. |
| 3 | ToxicityConfidenceThreshold : float | Represents the Toxicity Confidence Threshold. |

### 66. UserProfileViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |
| 2 | FullName : string | Represents the Full Name. |

### 67. VerifyOtpViewModel Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Email : string | Represents the Email. |
| 2 | OtpCode : string | Represents the Otp Code. |

### 68. WithdrawPageVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Balance : PlatformBalanceVM | Represents the Balance. |
| 2 | History : List&lt;WithdrawalHistoryVM&gt; | Represents the History. |

### 69. WithdrawalHistoryVM Class
| No | Method / Property | Description |
|---|---|---|
| 1 | Amount : decimal | Represents the Amount. |
| 2 | ArrivedAt : DateTime? | Represents the Arrived At. |
| 3 | CreatedAt : DateTime | Represents the Created At. |
| 4 | Currency : string | Represents the Currency. |
| 5 | Description : string? | Represents the Description. |
| 6 | ManagerName : string? | Represents the Manager Name. |
| 7 | Status : string | Represents the Status. |
| 8 | StripePayoutId : string? | Represents the Stripe Payout ID. |
| 9 | WithdrawalId : int | Represents the Withdrawal ID. |
