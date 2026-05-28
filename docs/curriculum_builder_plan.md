# Kế Hoạch Triển Khai: Curriculum & Visual Roadmap Builder

## 🎯 Mục tiêu
Loại bỏ việc giáo viên phải gõ "lộ trình khóa học" bằng text thuần nhàm chán trong khung Mô tả. Thay vào đó, cung cấp cho họ 2 công cụ chuyên biệt để thiết kế khóa học một cách chuyên nghiệp và hiển thị cực kỳ trực quan cho học viên:
1. **Visual Roadmap Builder (Lộ trình học tập)**: Công cụ vẽ "đường thời gian/cột mốc" (Timeline) để học sinh nắm được lộ trình tổng quan (VD: Giai đoạn 1 -> Giai đoạn 2).
2. **Curriculum Builder (Nội dung chi tiết)**: Công cụ quản lý Chương/Bài học chi tiết bằng thao tác kéo thả (Drag & Drop).

---

## 🚀 Phần 1: Visual Roadmap Builder (Mô tả Lộ trình tổng quan)
Thay vì gõ text thuần vào ô "Description", ta sẽ tạo một khối riêng biệt để giáo viên "lên khung" lộ trình.

### Cách thức hoạt động:
- Giao diện có nút **"+ Thêm Cột Mốc (Milestone)"**.
- Giáo viên nhập: `Tên Giai đoạn` (VD: Tháng 1 - Nhập môn) và `Mô tả ngắn`.
- Ở trang hiển thị cho học sinh (Course Landing Page), dữ liệu này sẽ được CSS/Tailwind vẽ thành một **Timeline (Đường thời gian)** có các điểm nối với nhau cực kỳ chuyên nghiệp.

### Triển khai Code:
- Thêm 1 cột `roadmap_json JSONB` vào bảng `courses` trong `5.sql` (để lưu mảng các cột mốc).
- Cập nhật UI ở `Create.cshtml` và `Editor.cshtml` để có tool thêm/bớt Milestone thay vì dùng Rich Text Editor cho phần này.

---

## 🚀 Phần 2: Curriculum Builder (Cấu trúc bài học chi tiết)
Phần này giúp quản lý cấu trúc video/tài liệu thực tế của khóa học.

### Hiện trạng:
- Đã có bảng `lessons` (Chương) và `learning_materials` (Bài học).
- Đã có Inline Editing cho Bài học, nhưng chưa có cho Chương.
- Chưa có Kéo thả (Drag & Drop).

### Triển khai Code:
- **Database**: Thêm cột `sort_order INT DEFAULT 0` vào bảng `lessons` và `learning_materials` trong `5.sql` để lưu thứ tự.
- **Backend API**: Thêm Endpoints để nhận lưu thứ tự mới khi giáo viên kéo thả.
- **UI/UX (`Editor.cshtml`)**:
  - Tích hợp **SortableJS** để kéo thả thay đổi vị trí Chương và Bài học.
  - Áp dụng **Inline Editing** cho danh sách Chương (Click vào tên Chương bên trái để sửa ngay lập tức, giống như đang làm với Bài học bên phải).
  - Tự động lấy cấu trúc này để vẽ thành bảng **Course Content (Mục lục khóa học)** dạng Accordion cho học sinh xem (thay vì bắt giáo viên gõ tay).

---

## 🧪 Kế hoạch Kiểm thử (Verification)
1. **Roadmap**: Giáo viên thêm 3 cột mốc, lưu lại. Trình duyệt tải lại vẫn giữ nguyên 3 cột mốc. Hiển thị UI Timeline ra ngoài Front-end cho học sinh xem.
2. **Curriculum Drag & Drop**: Kéo một Chương từ vị trí số 1 xuống số 3, tải lại trang xem vị trí có được lưu thành công trên Server không.
3. **Curriculum Inline Editing**: Nhấp vào tên Chương, đổi tên và click ra ngoài, đảm bảo dữ liệu cập nhật tự động bằng AJAX.
