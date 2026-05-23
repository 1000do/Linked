# Frontend & Backend Implementation Status: Report Feature

> **Ngày cập nhật:** 23/05/2026  
> **Trạng thái:** Đã hoàn thành (Completed)  
> **Phạm vi:** Tính năng Report cho cả 4 vai trò: User, Instructor, Staff, Admin  
> **Tech stack thực tế:** ASP.NET Core MVC + Razor Views + TailwindCSS + SweetAlert2 + REST API  
> **Quy tắc ngôn ngữ:** Giao diện & thông báo lỗi → **tiếng Anh** · Chú thích mã nguồn → **tiếng Việt**

---

## Hiện trạng mã nguồn Backend (BE)

Các thành phần backend đã hoạt động ổn định và sẵn sàng phục vụ frontend:

| File | Mô tả |
|---|---|
| `Domain/Entities/CourseReport.cs` | Bảng `course_reports` |
| `Domain/Entities/CourseReviewReport.cs` | Bảng `course_review_reports` |
| `Domain/Entities/LessonReviewReport.cs` | Bảng `lesson_review_reports` |
| `Infrastructure/Repositories/ReportRepository.cs` | Thao tác dữ liệu báo cáo và bộ lọc trạng thái |
| `Application/Services/ReportService.cs` | Nghiệp vụ xử lý báo cáo, cập nhật trạng thái nội dung & thông báo (Notifications) |
| `Presentation/Controllers/ReportController.cs` | API Endpoints cho User/Instructor gửi báo cáo |
| `Presentation/Controllers/AdminModerationController.cs` | API Endpoints cho Staff/Admin quản lý và giải quyết báo cáo |

### BE API Endpoints hiện tại

```
POST   /api/report/courses                           ← User/Instructor gửi báo cáo khóa học
POST   /api/report/course-reviews                    ← Gửi báo cáo đánh giá khóa học
POST   /api/report/lesson-reviews                    ← Gửi báo cáo đánh giá bài học
GET    /api/report/my/courses                        ← Xem lịch sử báo cáo khóa học cá nhân
GET    /api/report/my/reviews                        ← Xem lịch sử báo cáo đánh giá cá nhân (gộp)
GET    /api/report/instructor/courses/{courseId}     ← Instructor xem danh sách báo cáo trên khóa học của mình

GET    /api/admin/moderation/reports/stats           ← Số liệu thống kê tổng quan báo cáo
GET    /api/admin/moderation/reports/courses         ← Lấy tất cả báo cáo khóa học (?status=)
GET    /api/admin/moderation/reports/course-reviews  ← Lấy tất cả báo cáo đánh giá khóa học (?status=)
GET    /api/admin/moderation/reports/lesson-reviews  ← Lấy tất cả báo cáo đánh giá bài học (?status=)
PATCH  /api/admin/moderation/reports/courses/{id}    ← Duyệt/Từ chối báo cáo khóa học
PATCH  /api/admin/moderation/reports/course-reviews/{id} ← Duyệt/Từ chối báo cáo đánh giá khóa học
PATCH  /api/admin/moderation/reports/lesson-reviews/{id} ← Duyệt/Từ chối báo cáo đánh giá bài học
DELETE /api/admin/moderation/courses/{id} ← Gỡ khóa học (xóa mềm/khóa, chỉ Admin)
```

---

## Thiết kế Giao diện & Trực quan (Design System)

| Token | Giá trị áp dụng thực tế |
|---|---|
| Font headings | Manrope (`font-headline`) |
| Font body | Inter (`font-body`) |
| Primary color — User/Instructor layout | `#006a62` (Deep Teal) |
| Primary color — Admin/Staff layout | `#10b981` (Emerald 500) |
| Card container | `bg-white rounded-[32px] border border-slate-100 shadow-sm` |
| Badge `pending` | `bg-amber-50 text-amber-600` + chấm tròn nhấp nháy (pulsing dot) |
| Badge `resolved` | `bg-emerald-50 text-emerald-600` |
| Badge `rejected` | `bg-slate-100 text-slate-500` |
| Badge `escalated` | `bg-red-50 text-red-600` |
| Modals | SweetAlert2 với thiết kế góc bo `rounded-[32px]` và nút Emerald |
| Icons | Google Material Symbols Outlined |

---

## Chi tiết Triển khai Frontend (FE)

### 1. ViewModels — `Models/ReportViewModels.cs`
Các ViewModels đã được tạo đầy đủ để vận chuyển dữ liệu giữa Views và Controllers:
* `CreateCourseReportViewModel`: Chứa dữ liệu gửi yêu cầu báo cáo khóa học.
* `CreateReviewReportViewModel`: Dùng chung cho việc gửi báo cáo đánh giá khóa học & bài học.
* `ResolveReportViewModel`: Dữ liệu phản hồi của Staff/Admin gửi lên để giải quyết báo cáo.
* `MyCourseReportViewModel`: Hiển thị thông tin báo cáo khóa học của cá nhân.
* `MyReviewReportViewModel`: Hiển thị thông tin báo cáo đánh giá của cá nhân.
* `CourseReportDetailViewModel`: Hiển thị dòng thông tin báo cáo khóa học trong quản trị.
* `ReviewReportDetailViewModel`: Hiển thị dòng thông tin báo cáo đánh giá trong quản trị.
* `ReportStatsViewModel`: Phục vụ các thẻ thống kê tổng quan (Stats Card).
* `InstructorCourseReportSummaryViewModel`: Thống kê báo cáo mà giảng viên nhận được.

---

### 2. FE Controllers

#### `Controllers/ReportController.cs`
Xử lý các hành động gửi báo cáo từ phía người dùng học viên:
* `[HttpPost] ReportCourse`: Nhận form gửi báo cáo khóa học, chuyển tiếp tới API `/api/report/courses`.
* `[HttpPost] ReportCourseReview`: Nhận form gửi báo cáo review khóa học, chuyển tiếp tới API `/api/report/course-reviews`.
* `[HttpPost] ReportLessonReview`: Nhận form gửi báo cáo review bài học, chuyển tiếp tới API `/api/report/lesson-reviews`.
* `[HttpGet] MyReports`: Route hỗ trợ cả `/Account/MyReports` và `/Report/MyReports`. Lấy thông tin báo cáo khóa học và báo cáo đánh giá cá nhân từ BE, gộp và hiển thị trên View `MyReports.cshtml`.

#### `Controllers/AdminModerationController.cs`
Xử lý nghiệp vụ quản trị báo cáo cho Staff/Admin:
* `[HttpGet] Reports`: Trả về giao diện quản trị báo cáo `Reports.cshtml` kèm thông tin thống kê ban đầu.
* `[HttpGet] GetReportStats`: API AJAX trả về JSON thống kê.
* `[HttpGet] GetCourseReports(status)`: API AJAX lấy danh sách báo cáo khóa học theo bộ lọc.
* `[HttpGet] GetCourseReviewReports(status)`: API AJAX lấy danh sách báo cáo đánh giá khóa học theo bộ lọc.
* `[HttpGet] GetLessonReviewReports(status)`: API AJAX lấy danh sách báo cáo đánh giá bài học theo bộ lọc.
* `[HttpPost] ResolveCourseReport(reportId, model)`: Gọi PATCH tới BE giải quyết báo cáo khóa học.
* `[HttpPost] ResolveCourseReviewReport(reportId, model)`: Gọi PATCH tới BE giải quyết báo cáo đánh giá khóa học.
* `[HttpPost] ResolveLessonReviewReport(reportId, model)`: Gọi PATCH tới BE giải quyết báo cáo đánh giá bài học.
* `[HttpPost] RemoveCourse(courseId)`: Gọi DELETE tới BE gỡ khóa học khỏi nền tảng (xóa mềm/khóa, dành riêng cho Admin).

#### `Controllers/InstructorController.cs`
Xử lý giao diện báo cáo cho Giảng viên:
* `[HttpGet] Reports`: Trả về giao diện `/Instructor/Reports` và nạp danh sách khóa học của giảng viên vào `ViewBag.Courses` làm dropdown.
* `[HttpGet] GetReportsForCourse(courseId)`: API AJAX lấy các báo cáo thuộc khóa học được chọn qua `/api/report/instructor/courses/{courseId}`.

---

### 3. Views Razor & Tương tác phía Client

#### `Views/Report/MyReports.cshtml`
* **Đường dẫn truy cập:** `/Account/MyReports` hoặc `/Report/MyReports`.
* **Giao diện:** Thiết kế thanh bên (Profile Sidebar) đồng bộ với trang cá nhân. Phần nội dung chính chia làm 2 tab: "Course Reports" và "Review Reports" hiển thị bảng danh sách kèm theo nhãn trạng thái (Pending, Resolved, Rejected) và phản hồi từ quản trị viên.

#### `Views/Instructor/Reports.cshtml`
* **Đường dẫn truy cập:** `/Instructor/Reports`.
* **Giao diện:** Nằm trong layout của Instructor. Có dropdown cho phép giảng viên chọn khóa học của mình. Khi chọn, gọi AJAX đến `GetReportsForCourse` hiển thị danh sách các báo cáo gửi tới khóa học đó kèm lý do chi tiết. Chỉ hiển thị chế độ Đọc (Read-only), giảng viên không có quyền giải quyết hay xóa báo cáo.

#### `Views/AdminModeration/Reports.cshtml`
* **Đường dẫn truy cập:** `/AdminModeration/Reports`.
* **Giao diện:** Giao diện quản trị toàn diện gồm 4 thẻ thống kê ở trên cùng (Pending Course, Pending Reviews, Resolved Today, Rejected Today), thanh điều hướng gồm 4 tab chính (**Course Reports**, **Review Reports**, **User Reports** dành cho chat, và **Detailed Stats**) cùng bộ lọc trạng thái.
* **Quy trình duyệt Báo cáo qua SweetAlert2:**
  * Khi nhấn "Moderate" trên một dòng báo cáo, một popup SweetAlert2 tùy biến hiện ra. 
  * Nếu báo cáo có trạng thái `escalated` và người dùng hiện tại là `staff`, nút Moderate này sẽ bị vô hiệu hóa (disabled).
  * Hộp thoại cho phép chọn trạng thái xử lý (Accept Report / Dismiss Report / Under Review / Escalate).
  * **Cơ chế xử phạt vi phạm (3 Strikes System):** Khi "Accept Report" và đánh dấu có vi phạm:
    * **Đối với Khóa học (Course):** 
      * Lần 1 & 2: Chuyển `CourseStatus` về `rejected` (ẩn khỏi trang chủ, chỉ người tạo xem được để sửa), cảnh cáo giảng viên. Số lượng cờ (Flag) sẽ KHÔNG bị reset về 0 kể cả khi khóa học được duyệt lại (giữ hiển thị số lần vi phạm).
      * Lần 3: Chuyển `CourseStatus` sang `archived` (khóa vĩnh viễn), khóa role Instructor của người tạo trong 30 ngày. Hệ thống sẽ tự động gửi thông báo đến toàn bộ học viên đã đăng ký khóa học rằng: *"Trong 30 ngày tới, khóa học sẽ không được cập nhật và bạn không thể liên lạc với giảng viên"*. Số cờ chỉ được reset về 0 sau khi thi hành xong hình phạt cờ 3.
    * **Đối với Đánh giá (Review):**
      * Lần 1: Cảnh báo, xóa/ẩn comment nhưng vẫn để lại hiển thị thay thế: *"This comment has been removed for violating community standards"*.
      * Lần 2: Cấm quyền bình luận/đánh giá (Comment Ban) trong 7 ngày trên toàn hệ thống.
      * Lần 3: Khóa tài khoản (Account Ban) 30 ngày. Nếu tài khoản bị khóa đang có role Instructor, hệ thống sẽ thực hiện gửi thông báo đến tất cả học viên của họ tương tự như vi phạm khóa học cờ 3.
  * Ô nhập ghi chú lý do (Moderator Note).
  * **Đặc quyền của Admin:** Nếu tài khoản đăng nhập có vai trò `admin`, ở cuối popup SweetAlert2 báo cáo khóa học sẽ xuất hiện nút màu đỏ ngắn gọn **"Remove"** để gỡ khóa học khỏi nền tảng (xóa mềm/khóa). Staff thông thường sẽ không nhìn thấy nút này.

#### `Views/Course/Details.cshtml` (và Lesson Player)
* Nút `⚑ Report` được đặt tại phần thông tin chính khóa học (Course Hero) bên cạnh nút Wishlist. Chỉ hiển thị với học viên đã đăng nhập và đã ghi danh (enrolled), ẩn đi nếu đó là giảng viên của chính khóa học đó.
* Biểu tượng cờ báo cáo `flag` nhỏ được đặt ở góc các bình luận/đánh giá (Review Card) cả trên trang chi tiết khóa học và trình học bài (Lesson Player) phục vụ tính năng báo cáo nội dung đánh giá xấu. Khi một review bị xác nhận vi phạm và xóa, nó vẫn hiển thị trên giao diện nhưng nội dung được thay thế bằng nhãn thông báo vi phạm tiêu chuẩn cộng đồng.
* Mọi hành động click nút báo cáo đều mở ra một hộp thoại SweetAlert2 để người dùng chọn Lý do (Reason) và nhập Chi tiết mô tả (Description) trước khi gửi đi.

---

### 4. Cập nhật Layout & Menu điều hướng

#### `Views/Shared/_InstructorLayout.cshtml`
* Đã thêm mục **"Reports"** với cờ hiệu `flag` và thẻ badge hiển thị số lượng `<span id="sidebar-reports-badge"...>` (ẩn mặc định, hiển thị khi có báo cáo mới cần xử lý) tại thanh bên điều hướng của Giảng viên.
* Đường dẫn: `/Instructor/Reports`.
* Điều kiện kích hoạt trạng thái active: `currentController == "Instructor" && currentAction == "Reports"`.
* Chỉ hiển thị khi trạng thái giảng viên đã được duyệt thành công (`Approved`).

#### `Views/Shared/_AdminLayout.cshtml`
* Đã sửa nhãn menu từ "User Reports" thành **"Report Center"** và cập nhật biểu tượng cờ `flag` chỉ tới trang `/AdminModeration/Reports`. Tự động áp dụng hiệu ứng kích hoạt menu (`nav-link-active`) khi quản trị viên truy cập trang này.


---

## Risk Register & Xử lý lỗi trong thực tế

1. **Lỗi báo cáo trùng lặp (Duplicate Report):** Client bắt mã lỗi `400` từ BE và hiển thị cảnh báo: *"You already have a pending report for this item. Please wait for it to be reviewed."*
2. **Học viên chưa ghi danh cố tình báo cáo:** Nút báo cáo bị ẩn. Nếu truy cập bằng công cụ bên thứ ba để gửi API, BE sẽ từ chối với thông báo *"Bạn cần ghi danh vào khóa học trước khi báo cáo."*
3. **Giảng viên tự báo cáo khóa học của mình:** Ẩn nút trên giao diện. BE kiểm tra và từ chối nếu `InstructorId == ReporterId`.
4. **Hết hạn Token (401 Unauthorized):** Hệ thống tự động chuyển hướng người dùng về trang đăng nhập `/Account/Login`.
5. **Staff cố tình gọi xóa cứng khóa học:** Phân quyền lọc ở cả giao diện (ẩn nút) lẫn ở tầng Controller Backend (`[Authorize(Roles = "admin")]`). Staff gửi request trực tiếp sẽ nhận về lỗi `403 Forbidden`.
6. **XSS trong nội dung mô tả báo cáo:** Sử dụng phương thức gán dữ liệu an toàn `textContent` hoặc hàm tự định nghĩa `escapeHtml` phía JavaScript trước khi hiển thị lên giao diện bảng báo cáo để tránh bị tiêm mã độc.

---

## Trạng thái Xác minh Nghiệp vụ (Checklist)

### Học viên (User)
- [x] Nút báo cáo khóa học hiển thị đúng điều kiện trên trang chi tiết khóa học.
- [x] Popup chọn lý do báo cáo mở ra bình thường khi click cờ báo cáo khóa học/đánh giá.
- [x] Gửi báo cáo thành công hiển thị thông báo tiếng Anh chính xác.
- [x] Lịch sử báo cáo tại trang `/Report/MyReports` hiển thị đầy đủ, chính xác thông tin và trạng thái.

### Giảng viên (Instructor)
- [x] Menu "Reports" hiển thị trên thanh bên.
- [x] Trang báo cáo của giảng viên tải đúng danh sách khóa học sở hữu và lọc được báo cáo tương ứng.
- [x] Giảng viên chỉ có quyền xem báo cáo khóa học nhận được, không thể tự chỉnh sửa hay duyệt báo cáo.

### Kiểm duyệt viên (Staff)
- [x] Trang `/AdminModeration/Reports` hiển thị đủ dữ liệu các bảng báo cáo (bao gồm cả tab User Reports cho chat) và thống kê.
- [x] Các báo cáo có trạng thái `escalated` sẽ vô hiệu hóa (disabled) nút Moderate đối với Staff.
- [x] Popup xử lý báo cáo hoạt động tốt, cập nhật dữ liệu tức thì xuống cơ sở dữ liệu và tự động làm mới (refresh) giao diện bảng.
- [x] **Không thấy** nút gỡ "Remove" khóa học trong modal xử lý.

### Quản trị viên (Admin)
- [x] Có đầy đủ quyền hạn của Staff.
- [x] Có quyền phê duyệt giải quyết các báo cáo ở trạng thái `escalated`.
- [x] **Nhìn thấy** nút đỏ ngắn gọn **"Remove"** trong modal xử lý báo cáo khóa học.
- [x] Nhấn nút gỡ kích hoạt popup xác nhận bước 2, khi xác nhận sẽ thực hiện gỡ khóa học khỏi hệ thống (soft-delete).
