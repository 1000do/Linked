# Diagram vs. Codebase Class Mapping Analysis

By analyzing the properties, methods, stereotypes, and relationships of the 41 "fake" classes/interfaces found in the Mermaid diagrams, here is the mapping to their **true** counterparts in the actual C# codebase. The discrepancies are mostly due to naming convention evolution (e.g., swapping noun/verb order like `CreateLesson` -> `LessonCreate`, or suffix changes like `ViewModel` -> `Request`/`Response`).

## 1. Authentication & Account Management

| Diagram Class | True Codebase Class | Why they match | Code Links | Diagrams |
| :--- | :--- | :--- | :--- | :--- |
| `AccountDetailViewModel` | `AdminAccountDetailPageViewModel` | Diagram shows it aggregating AdminAccountDetail and AccountTransactionSummary. | [AdminAccountViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminAccountViewModels.cs) | [cls_mm_view_account_detail.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_detail.mmd) |
| `AccountListViewModel` | `AdminAccountListViewModel` | Diagram properties: Keyword, Role, Accounts. | [AdminAccountViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminAccountViewModels.cs) | [cls_mm_filter_accounts.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_filter_accounts.mmd)<br>[cls_mm_search_accounts.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_search_accounts.mmd)<br>[cls_mm_view_account_list.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_list.mmd) |
| `AdminAccountDetail` | `AdminAccountDetailDto` | Diagram shows it used by AdminAccountService. | [AdminAccountDtos.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/AdminAccountDtos.cs) | [cls_mm_view_account_detail.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_detail.mmd) |
| `CreateStaffViewModel` | `CreateStaffFERequest` | Diagram properties: Email, Username, Password. | [AdminAccountViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminAccountViewModels.cs) | [cls_mm_add_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_add_account.mmd) |
| `EditStaffViewModel` | `UpdateStaffFERequest` | Diagram uses Edit, codebase uses Update. | [AdminAccountViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminAccountViewModels.cs) | [cls_mm_update_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_update_account.mmd) |
| `FlagAccountViewModel` | `FlagAccountFERequest` | Diagram property: Reason. | [AdminAccountViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminAccountViewModels.cs) | [cls_mm_flag_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_flag_account.mmd) |
| `ForgotPasswordViewModel` | `ResetPasswordRequest` | Diagram shows they hold Email. | [ResetPasswordRequest.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/ResetPasswordRequest.cs) | [cls_mm_forget_password.mmd](../../diagrams/class_mermaid/auth/cls_mm_forget_password.mmd) |
| `ForgotPasswordRequest` | `ResetPasswordViewModel` | Assumed ViewModel counterpart to ResetPasswordRequest. | Not found | [cls_mm_forget_password.mmd](../../diagrams/class_mermaid/auth/cls_mm_forget_password.mmd) |
| `GoogleLoginViewModel` | `GoogleLoginViewModel` | Assumed ViewModel counterpart to GoogleLoginRequest. | Not found | [cls_mm_login_google.mmd](../../diagrams/class_mermaid/auth/cls_mm_login_google.mmd) |
| `ResetPasswordViewModel` | `ResetPasswordRequest` | Diagram properties: Email, OtpCode, NewPassword. | [ResetPasswordRequest.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/ResetPasswordRequest.cs) | [cls_mm_forget_password.mmd](../../diagrams/class_mermaid/auth/cls_mm_forget_password.mmd) |
| `VerifyEmailRequest` | `VerifyOtpRequest` | Diagram properties: Email, Otp. | [VerifyOtpRequest.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/VerifyOtpRequest.cs) | [cls_mm_verify_email.mmd](../../diagrams/class_mermaid/profile/cls_mm_verify_email.mmd) |
| `VerifyOtpViewModel` | `VerifyOtpViewModel` | Assumed ViewModel counterpart to VerifyOtpRequest. | Not found | [cls_mm_forget_password.mmd](../../diagrams/class_mermaid/auth/cls_mm_forget_password.mmd) |
| `UserProfile` | `UserProfileViewModel` | Diagram properties: Email, FullName. | [UserProfileViewModel.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/UserProfileViewModel.cs) | [cls_mm_checkout.mmd](../../diagrams/class_mermaid/Hung/cls_mm_checkout.mmd) |

## 2. Course, Lesson & Material Management

| Diagram Class | True Codebase Class | Why they match | Code Links | Diagrams |
| :--- | :--- | :--- | :--- | :--- |
| `AddMaterialRequest` | `MaterialCreateRequest` | Diagram properties: Title, MaterialType, VideoUrl. | [LessonDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/LessonDTOs.cs) | [cls_mm_add_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_learning_materials.mmd) |
| `UpdateMaterialRequest` | `MaterialUpdateRequest` | Diagram properties match AddMaterialRequest. | [LessonDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/LessonDTOs.cs) | [cls_mm_edit_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_learning_materials.mmd) |
| `UpdateMaterialViewModel` | `MaterialUpdateViewModel` | Assumed ViewModel counterpart to MaterialUpdateRequest. | Not found | [cls_mm_edit_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_learning_materials.mmd) |
| `LearningMaterialDto` | `MaterialResponse` | Diagram properties: MaterialId, Title, MaterialType. | [LessonDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/LessonDTOs.cs) | [cls_mm_add_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_learning_materials.mmd)<br>[cls_mm_edit_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_learning_materials.mmd) |
| `CreateLessonRequest` | `LessonCreateRequest` | Diagram properties: CourseId, Title, Description. | [LessonDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/LessonDTOs.cs) | [cls_mm_add_lessons.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_lessons.mmd) |
| `CreateLessonViewModel` | `LessonCreateViewModel` | Assumed ViewModel counterpart to LessonCreateRequest. | Not found | [cls_mm_add_lessons.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_lessons.mmd) |
| `LessonDto` | `LessonResponse` | Diagram properties: LessonId, Title, IsPublished. | [LessonDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/LessonDTOs.cs) | [cls_mm_add_lessons.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_lessons.mmd) |
| `EditCourseViewModel` | `UpdateCourseDetailsViewModel` | Diagram properties: CourseId, Title, CategoryId, Price. | [InstructorCourseVM.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/InstructorCourseVM.cs) | [cls_mm_edit_courses.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_courses.mmd) |
| `EnrolledCourseDto` | `EnrolledCourseViewModel` / `EnrolledCourseDto` | Appears in both ViewModel and DTO logic. | [EnrolledCourseViewModel.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/EnrolledCourseViewModel.cs) <br> Not found | [cls_mm_view_purchased_course_list.mmd](../../diagrams/class_mermaid/user_course/cls_mm_view_purchased_course_list.mmd) |
| `WishlistCourseResponse` | `WishlistResponse` | Diagram properties: CourseId, Title, Price. | [WishlistDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/WishlistDTOs.cs) | [cls_mm_view_wishlist.mmd](../../diagrams/class_mermaid/wishlist/cls_mm_view_wishlist.mmd) |

## 3. Quiz Module

The diagrams use a `QuizXxxStudentResponse` naming convention, whereas the actual codebase replaced `Student` with `ForStudent`.

| Diagram Class | True Codebase Class | Why they match | Code Links | Diagrams |
| :--- | :--- | :--- | :--- | :--- |
| `QuizStudentResponse` | `QuizForStudentResponse` | Diagram properties: TimeLimitMinutes, PassingScore. | [QuizDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/QuizDTOs.cs) | [cls_mm_take_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_take_quiz.mmd) |
| `QuizQuestionStudentResponse` | `QuizQuestionForStudentResponse` | Diagram properties: QuestionType, Points, Options. | [QuizDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/QuizDTOs.cs) | [cls_mm_take_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_take_quiz.mmd) |
| `QuizOptionStudentResponse` | `QuizOptionForStudentResponse` | Diagram properties: OptionId, Content. | [QuizDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/QuizDTOs.cs) | [cls_mm_take_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_take_quiz.mmd) |
| `QuizResponse` | `QuizDetailResponse` | Diagram properties: QuizId, Title. | [QuizDTOs.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/QuizDTOs.cs) | [cls_mm_create_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_create_quiz.mmd) |
| `QuizListResponse` | `QuizListViewModel` / `QuizListDto` | Assumed to appear in both ViewModel and DTO. | [QuizViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/QuizViewModels.cs) <br> Not found | [cls_mm_view_quiz_list.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_view_quiz_list.mmd) |
| `QuizAnswerSubmitRequest` | `QuizAnswerSubmitViewModel` / `QuizAnswerSubmitDto` | Assumed to appear in both ViewModel and DTO. | Not found <br> Not found | [cls_mm_submit_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_submit_quiz.mmd) |
| `QuizAttemptDetail` | `QuizAttemptQuestion` | Diagram is marked as `<<entity>>` with properties: SelectedOptionId. | [QuizAttemptQuestion.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/QuizAttemptQuestion.cs) | [cls_mm_submit_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_submit_quiz.mmd) |

## 4. Finance, Cart & Payments

| Diagram Class | True Codebase Class | Why they match | Code Links | Diagrams |
| :--- | :--- | :--- | :--- | :--- |
| `AccountTransactionSummary` | `AccountTransactionSummaryDto` | Diagram properties: TotalSpent, TotalRevenue. | [AdminAccountDtos.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/AdminAccountDtos.cs) | [cls_mm_view_account_detail.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_detail.mmd) |
| `ApplyCouponViewModel` | `ApplyCouponRequest` | Diagram property: CouponCode. | [ApplyCouponRequest.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/ApplyCouponRequest.cs) | [cls_mm_apply_coupon.mmd](../../diagrams/class_mermaid/cart/cls_mm_apply_coupon.mmd) |
| `CouponDto` | `AvailableCouponDto` | Diagram properties: DiscountValue, CouponType. | [AvailableCouponDto.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/AvailableCouponDto.cs) | [cls_mm_apply_coupon.mmd](../../diagrams/class_mermaid/cart/cls_mm_apply_coupon.mmd)<br>[cls_mm_cancel_coupon.mmd](../../diagrams/class_mermaid/cart/cls_mm_cancel_coupon.mmd) |
| `Payout` | `InstructorPayout` | Diagram shows it relating to Transaction. | [InstructorPayout.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Domain/Entities/InstructorPayout.cs) | [cls_mm_view_balance_system.mmd](../../diagrams/class_mermaid/Hung/cls_mm_view_balance_system.mmd) |
| `StripePaymentGatewayService` | `StripePaymentService` | Diagram shows methods: ReverseTransferAsync, RefundAsync. | [StripePaymentService.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Services/StripePaymentService.cs) | [cls_mm_approve_refund_request.mmd](../../diagrams/class_mermaid/Hung/cls_mm_approve_refund_request.mmd) |
| `TransactionExtVM` | `TransactionExtDto` | Diagram properties: RefundReason, RefundRequestedAt. | [AdminFinanceViewModels.cs](../../CourseMarketplaceFE/CourseMarketplaceFE/Models/AdminFinanceViewModels.cs) | [cls_mm_view_instructor_transaction_history.mmd](../../diagrams/class_mermaid/Hung/cls_mm_view_instructor_transaction_history.mmd) |

## 5. Services & Controllers

| Diagram Class | True Codebase Class | Why they match | Code Links | Diagrams |
| :--- | :--- | :--- | :--- | :--- |
| `AdminCourseController` | `AdminModerationController` | Diagram shows methods ApproveCourse, RejectCourse. | [AdminModerationController.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Presentation/Controllers/AdminModerationController.cs) | [cls_mm_approve_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_approve_course.mmd)<br>[cls_mm_filter_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_filter_courses_admin.mmd)<br>[cls_mm_flag_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_flag_course.mmd)<br>[cls_mm_reject_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_reject_course.mmd)<br>[cls_mm_search_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_search_courses_admin.mmd)<br>[cls_mm_sort_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_sort_courses_admin.mmd)<br>[cls_mm_view_course_moderation_list.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_view_course_moderation_list.mmd) |
| `LearningController` | `CoursePublicController` | Diagram shows frontend methods Index, Progress. | [CoursePublicController.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Presentation/Controllers/CoursePublicController.cs) | [cls_mm_go_to_learning.mmd](../../diagrams/class_mermaid/user_course/cls_mm_go_to_learning.mmd)<br>[cls_mm_view_learning_progress.mmd](../../diagrams/class_mermaid/user_course/cls_mm_view_learning_progress.mmd) |
| `FileUploadService` <br> `ICloudinaryUploadService` | `CloudinaryUploadService` <br> `IFileUploadService` | Diagram shows UploadImageAsync, UploadVideoAsync. | [CloudinaryUploadService.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Infrastructure/Services/CloudinaryUploadService.cs) <br> [IFileUploadService.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/IServices/IFileUploadService.cs) | None |
| `NotificationDto` | `NotificationAdvancedDto` | Diagram properties: Title, Content, LinkAction. | [NotificationDto.cs](../../CourseMarketplaceBE/CourseMarketplaceBE/Application/DTOs/NotificationDto.cs) | [cls_mm_view_notification_list.mmd](../../diagrams/class_mermaid/notification/cls_mm_view_notification_list.mmd) |


## 6. Distinct Diagrams Used
The following diagrams contain one or more of the mapped classes above:

### Hưng
- [cls_mm_approve_refund_request.mmd](../../diagrams/class_mermaid/Hung/cls_mm_approve_refund_request.mmd)
- [cls_mm_checkout.mmd](../../diagrams/class_mermaid/Hung/cls_mm_checkout.mmd)
- [cls_mm_view_balance_system.mmd](../../diagrams/class_mermaid/Hung/cls_mm_view_balance_system.mmd)
- [cls_mm_view_instructor_transaction_history.mmd](../../diagrams/class_mermaid/Hung/cls_mm_view_instructor_transaction_history.mmd)

### An
- admin_course
    - [cls_mm_approve_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_approve_course.mmd)
    - [cls_mm_filter_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_filter_courses_admin.mmd)
    - [cls_mm_flag_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_flag_course.mmd)
    - [cls_mm_reject_course.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_reject_course.mmd)
    - [cls_mm_search_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_search_courses_admin.mmd)
    - [cls_mm_sort_courses_admin.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_sort_courses_admin.mmd)
    - [cls_mm_view_course_moderation_list.mmd](../../diagrams/class_mermaid/admin_course/cls_mm_view_course_moderation_list.mmd)

### Ngọc
- auth
    - [cls_mm_forget_password.mmd](../../diagrams/class_mermaid/auth/cls_mm_forget_password.mmd)
    - [cls_mm_login_google.mmd](../../diagrams/class_mermaid/auth/cls_mm_login_google.mmd)

- cart
    - [cls_mm_apply_coupon.mmd](../../diagrams/class_mermaid/cart/cls_mm_apply_coupon.mmd)
    - [cls_mm_cancel_coupon.mmd](../../diagrams/class_mermaid/cart/cls_mm_cancel_coupon.mmd)

- manage_account
    - [cls_mm_add_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_add_account.mmd)
    - [cls_mm_filter_accounts.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_filter_accounts.mmd)
    - [cls_mm_flag_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_flag_account.mmd)
    - [cls_mm_search_accounts.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_search_accounts.mmd)
    - [cls_mm_update_account.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_update_account.mmd)
    - [cls_mm_view_account_detail.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_detail.mmd)
    - [cls_mm_view_account_list.mmd](../../diagrams/class_mermaid/manage_account/cls_mm_view_account_list.mmd)

- profile
    - [cls_mm_verify_email.mmd](../../diagrams/class_mermaid/profile/cls_mm_verify_email.mmd)

- wishlist
    - [cls_mm_view_wishlist.mmd](../../diagrams/class_mermaid/wishlist/cls_mm_view_wishlist.mmd)




### Hùng
- notification
    - [cls_mm_view_notification_list.mmd](../../diagrams/class_mermaid/notification/cls_mm_view_notification_list.mmd)


### Tài
- instructor_course
    - [cls_mm_add_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_learning_materials.mmd)
    - [cls_mm_add_lessons.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_add_lessons.mmd)
    - [cls_mm_edit_courses.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_courses.mmd)
    - [cls_mm_edit_learning_materials.mmd](../../diagrams/class_mermaid/instructor_course/cls_mm_edit_learning_materials.mmd)


- quiz_module
    - [cls_mm_create_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_create_quiz.mmd)
    - [cls_mm_submit_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_submit_quiz.mmd)
    - [cls_mm_take_quiz.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_take_quiz.mmd)
    - [cls_mm_view_quiz_list.mmd](../../diagrams/class_mermaid/quiz_module/cls_mm_view_quiz_list.mmd)

- user_course
    - [cls_mm_go_to_learning.mmd](../../diagrams/class_mermaid/user_course/cls_mm_go_to_learning.mmd)
    - [cls_mm_view_learning_progress.mmd](../../diagrams/class_mermaid/user_course/cls_mm_view_learning_progress.mmd)
    - [cls_mm_view_purchased_course_list.mmd](../../diagrams/class_mermaid/user_course/cls_mm_view_purchased_course_list.mmd)