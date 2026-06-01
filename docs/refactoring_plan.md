# Kế Hoạch Tái Cấu Trúc Hệ Thống (Refactoring Plan)

Tài liệu này ghi nhận quyết định và kế hoạch tái cấu trúc hệ thống CourseMarketplaceBE nhằm tuân thủ các nguyên tắc SOLID và DRY, nâng cao chất lượng mã nguồn, dễ bảo trì và dễ mở rộng.

## Mục Tiêu

Hệ thống hiện tại đang gặp hai vấn đề lớn về kiến trúc:
1. **Vi phạm DRY (Don't Repeat Yourself):** Logic chuyển đổi (Mapping) từ Entity sang DTO bị phân mảnh, gán tay thủ công rải rác khắp nơi, dễ dẫn đến thiếu sót khi cập nhật thuộc tính.
2. **Vi phạm SRP (Single Responsibility Principle - SOLID):** Có sự xuất hiện của các "God Classes" như `ModerationService`, `CourseService`, `AdminFinanceService`. Những class này gánh quá nhiều trách nhiệm (tới 15 Dependencies) gây khó khăn cho việc Unit Test, bảo trì và dễ bị Git conflict khi làm việc nhóm.

## Phạm Vi Triển Khai

Theo sự đồng thuận, đội ngũ kỹ thuật sẽ ưu tiên thực hiện **Phase 1** và **Phase 2** ngay lập tức. Các Phase khác sẽ được đánh giá lại trong tương lai.

### [x] Phase 1: Tích hợp AutoMapper (Giải quyết vi phạm DRY)
- Đã cài đặt và cấu hình thư viện `AutoMapper`.
- Xây dựng `MappingProfile` tập trung toàn bộ cấu hình chuyển đổi object.
- Thay thế các đoạn code `.Select(x => new Dto { ... })` thủ công bằng `_mapper.Map<T>()` của AutoMapper.
- Các file trọng tâm đã sửa chữa: `ReportService`, `CourseService` và các API trả về danh sách dữ liệu.

### [x] Phase 2: Phân rã God Classes (Giải quyết vi phạm SRP)
- Phân tích và bóc tách các Class lớn thành nhiều Service nhỏ chuyên biệt theo Domain hoặc Use-case (CQRS).
- Trọng tâm 1: **`ModerationService`**
  - Trạng thái cũ: Ôm đồm duyệt khóa học, tích hợp kiểm duyệt AI, và hệ thống báo cáo (reports) của User.
  - Kết quả bóc tách: Phân rã thành 3 dịch vụ chuyên biệt: `CourseModerationService`, `CourseAiModerationService`, và `UserReportModerationService`.
- Trọng tâm 2: **`CourseService`**
  - Trạng thái cũ: Vừa thêm/sửa/xóa khóa học, vừa query dữ liệu học viên, vừa thống kê (hơn 650 dòng code, 10 dependencies).
  - Kết quả bóc tách: Áp dụng CQRS tách thành `CourseQueryService` (cho các truy vấn đọc) và `CourseCommandService` (cho việc ghi/cập nhật). Cập nhật toàn bộ các controller liên quan.

### [ ] Phase 3: Giải quyết "Magic Strings" và vi phạm OCP (Open/Closed Principle)
- **Vấn đề:** Hiện tại trạng thái (status) đang sử dụng String hardcode ("pending", "published", "archived", "active", "rejected"). Rất dễ gây lỗi đánh máy và khó khăn khi thêm trạng thái mới.
- **Giải pháp:** 
  - Chuyển đổi sang hệ thống Enum (`CourseStatus`, `ModerationStatus`, `LearningStatus`, v.v.).
  - Bổ sung Extension Methods hoặc Entity Framework Value Converters (nếu cần mapping kiểu string ở database).

### [ ] Phase 4: Tối ưu Xử lý Ngoại lệ (Global Exception Handling)
- **Vấn đề:** Các Controller hiện tại chứa quá nhiều khối lệnh `try-catch` lặp đi lặp lại chỉ để trả về `ApiResponse.ErrorResponse(ex.Message)`.
- **Giải pháp:** Xây dựng một `GlobalExceptionMiddleware` hoặc `ExceptionFilter` để hứng toàn bộ lỗi từ hệ thống, tự động map lỗi (như `BadRequestException`, `NotFoundException`, v.v.) sang HTTP Status Code tương ứng và format JSON thống nhất. Khi đó Controller sẽ hoàn toàn sạch bóng `try-catch`.

### [ ] Phase 5: Cải thiện DI Registration & Kiến trúc (Repository/Unit of Work)
- **Vấn đề:** `Program.cs` có nguy cơ phình to do khai báo hàng loạt `builder.Services.AddScoped<...>`. Hệ thống đang gọi `_repository.SaveChangesAsync()` rải rác ở nhiều service khác nhau dẫn đến khó quản lý transaction diện rộng.
- **Giải pháp:**
  - Gom nhóm đăng ký DI bằng các Extension Methods (ví dụ: `services.AddApplicationServices()`, `services.AddInfrastructureServices()`).
  - Đánh giá và áp dụng `UnitOfWork` pattern nếu có nhiều nghiệp vụ ghi dữ liệu liên kết chéo (multi-repository transactions).

### [ ] Phase 6: Tối ưu Performance & Caching
- **Vấn đề:** Việc gọi `IRedisService` đang bị chèn trực tiếp vào giữa luồng xử lý business logic.
- **Giải pháp:** Chuyển dịch Caching sang mô hình AOP (Aspect-Oriented Programming) bằng cách sử dụng các Attribute (như `[Cache(60)]`) đặt trên API Endpoint, hoặc dùng thư viện như `Scrutor` để bọc Caching Decorator bên ngoài các Interface, giúp Business Service "mù" (không cần biết) về Redis.

## Tiêu Chuẩn Nghiệm Thu
- Sau khi tái cấu trúc, toàn bộ dự án phải biên dịch (Build) thành công.
- Các API hiện hữu không bị thay đổi định dạng JSON trả về.
- Số lượng Dependency Injection trong các constructor của Service phải giảm xuống mức hợp lý (< 8).
