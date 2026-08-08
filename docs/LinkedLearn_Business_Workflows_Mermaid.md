# 📊 BUSINESS WORKFLOWS – HỆ THỐNG LINKEDLEARN (MERMAID FLOWCHARTS)

Tài liệu chứa mã nguồn Mermaid Flowchart cho 6 quy trình nghiệp vụ chính của nền tảng **LinkedLearn**. Bạn có thể dùng trực tiếp trong GitHub Markdown, VS Code Mermaid Preview hoặc Notion.

---

## 👥 BẢNG MÀU QUY ĐỊNH VAI TRÒ (ACTORS)

| Actor | Tác nhân | Mã màu định danh |
| :--- | :--- | :--- |
| **Guest** | Khách vãng lai chưa đăng nhập | Slategray (`#64748b`) |
| **User** | Người dùng / Học viên | Indigo (`#4338ca`) |
| **Instructor** | Giảng viên / Người tạo khóa học | Teal (`#0f766e`) |
| **Staff** | Nhân viên kiểm duyệt / Hỗ trợ | Amber (`#b45309`) |
| **Admin** | Quản trị viên tối cao | Rose (`#be123c`) |
| **System** | Hệ thống xử lý tự động / Webhook / AI | Cyan (`#0891b2`) |

---

## 1️⃣ WORKFLOW 1: ONBOARDING & KÍCH HOẠT GIẢNG VIÊN

```mermaid
flowchart LR
    classDef guest fill:#f1f5f9,stroke:#64748b,stroke-width:2px,color:#0f172a
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef instructor fill:#ccfbf1,stroke:#0f766e,stroke-width:2px,color:#134e4a
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W1_1["1️⃣ Guest Đăng ký<br/>• Email & Mật khẩu<br/>• Tạo account User"]:::guest --> W1_2["2️⃣ User Nộp Hồ sơ<br/>• Bio, Kinh nghiệm<br/>• Bằng cấp, Chứng chỉ"]:::user
    W1_2 --> W1_3["3️⃣ System Tiếp nhận<br/>• Lưu Pending Profile<br/>• Báo Ticket cho Staff"]:::system
    W1_3 --> W1_4["4️⃣ Staff/Admin Kiểm tra<br/>• Xác minh thông tin<br/>• Đánh giá điều kiện"]:::staff
    W1_4 --> W1_5{"5️⃣ Duyệt?"}:::decision
    
    W1_5 -- "Có (Yes)" --> W1_6["6️⃣ System Kích hoạt<br/>• Đổi Role Instructor<br/>• Báo Link Stripe Connect"]:::system
    W1_6 --> W1_7["7️⃣ Stripe Onboarding<br/>• Nhập Bank Account<br/>• Hoàn tất Connect Payout"]:::instructor

    W1_5 -- "Không (No)" --> W1_R["📄 Yêu cầu bổ sung /<br/>❗ Từ chối + Lý do"]:::reject
```

---

## 2️⃣ WORKFLOW 2: TẠO KHÓA HỌC & AI/STAFF KIỂM DUYỆT

```mermaid
flowchart LR
    classDef instructor fill:#ccfbf1,stroke:#0f766e,stroke-width:2px,color:#134e4a
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W2_1["1️⃣ Instructor Dashboard<br/>• Chọn Tạo khóa học"]:::instructor --> W2_2["2️⃣ Soạn Khóa học<br/>• Tên, Mô tả, Giá<br/>• Upload Thumbnail"]:::instructor
    W2_2 --> W2_3["3️⃣ Thêm Bài học & Quiz<br/>• Video & Tài liệu<br/>• Tạo Quiz kiểm tra"]:::instructor
    W2_3 --> W2_4["4️⃣ AI Content Moderation<br/>• Quét Từ cấm<br/>• Gán AI Risk Score"]:::system
    W2_4 --> W2_5["5️⃣ Staff/Admin Kiểm tra<br/>• Review kết quả AI<br/>• Đánh giá chất lượng"]:::staff
    W2_5 --> W2_6{"6️⃣ Duyệt?"}:::decision

    W2_6 -- "Có (Yes)" --> W2_7["7️⃣ Publish Course<br/>• Trang thái Published<br/>• Hiển thị Marketplace"]:::system
    W2_6 -- "Không (No)" --> W2_R["📄 Yêu cầu chỉnh sửa /<br/>❗ Từ chối + Lý do"]:::reject
```

---

## 3️⃣ WORKFLOW 3: KHÁM PHÁ & TÌM KIẾM KHÓA HỌC

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b

    W3_1["1️⃣ User Login<br/>• Đăng nhập hệ thống<br/>• Tải Gợi ý cá nhân"]:::user --> W3_2["2️⃣ Search & Filter<br/>• Từ khóa, Danh mục<br/>• Lọc Giá, Sắp xếp Hot"]:::user
    W3_2 --> W3_3["3️⃣ View Course Detail<br/>• Đánh giá, Bài học<br/>• Xem Video học thử"]:::user
    W3_3 --> W3_4["4️⃣ View Instructor Profile<br/>• Bio, Bằng cấp thầy<br/>• Xem các khóa khác"]:::user
    W3_4 --> W3_5["5️⃣ Add Wishlist<br/>• Bấm 💖 Lưu khóa học<br/>• Đồng bộ thiết bị"]:::user
```

---

## 4️⃣ WORKFLOW 4: MUA HÀNG, THANH TOÁN & REFUND

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef admin fill:#ffe4e6,stroke:#be123c,stroke-width:2px,color:#881337
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W4_1["1️⃣ Cart & Checkout<br/>• Xem Wishlist & Giỏ<br/>• Bấm Checkout"]:::user --> W4_2["2️⃣ Stripe Payment<br/>• Nhập Coupon giảm giá<br/>• Thanh toán thẻ"]:::user
    W4_2 --> W4_3["3️⃣ Transaction & Payout<br/>• Kích hoạt Enrollment<br/>• Ghi nhận Payout thầy"]:::system
    W4_3 --> W4_4["4️⃣ Request Refund<br/>• Trong 14 ngày mua<br/>• Nhập Lý do hoàn tiền"]:::user
    W4_4 --> W4_5["5️⃣ Admin Thẩm định<br/>• Kiểm tra điều kiện 14d<br/>• Tiến độ học < 20%"]:::admin
    W4_5 --> W4_6{"6️⃣ Duyệt?"}:::decision

    W4_6 -- "Có (Yes)" --> W4_7["7️⃣ Stripe Refund<br/>• Hoàn tiền qua Stripe<br/>• Hủy Enrollment & Payout"]:::system
    W4_6 -- "Không (No)" --> W4_R["📄 Từ chối Refund<br/>• Báo lý do không hợp lệ"]:::reject
```

---

## 5️⃣ WORKFLOW 5: HỌC TẬP, QUIZ & BÁO CÁO VI PHẠM

```mermaid
flowchart LR
    classDef user fill:#e0e7ff,stroke:#4338ca,stroke-width:2px,color:#1e1b4b
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f
    classDef system fill:#cffaff,stroke:#0891b2,stroke-width:2px,color:#164e63
    classDef decision fill:#fef08a,stroke:#ca8a04,stroke-width:2.5px,color:#451a03
    classDef reject fill:#ffe4e6,stroke:#e11d48,stroke-width:2px,color:#881337

    W5_1["1️⃣ Học Bài giảng<br/>• Xem Video bài học<br/>• Lưu tiến độ %"]:::user --> W5_2["2️⃣ SignalR Realtime Chat<br/>• Khung Chat bài học<br/>• Trao đổi với Giảng viên"]:::user
    W5_2 --> W5_3["3️⃣ Làm Bài Quiz<br/>• Trả lời trắc nghiệm<br/>• Xem điểm & đáp án"]:::user
    W5_3 --> W5_4["4️⃣ Review / Report<br/>• Đánh giá sao & nhận xét<br/>• Hoặc Báo cáo vi phạm"]:::user
    W5_4 --> W5_5["5️⃣ Staff/Admin Xác minh<br/>• Tiếp nhận ticket báo cáo<br/>• Thẩm tra bằng chứng"]:::staff
    W5_5 --> W5_6{"6️⃣ Duyệt?"}:::decision

    W5_6 -- "Có (Yes)" --> W5_7["7️⃣ Thực thi Án phạt<br/>• Ẩn/xóa nội dung<br/>• Khóa/cảnh cáo tài khoản"]:::system
    W5_6 -- "Không (No)" --> W5_R["📄 Bỏ qua Báo cáo<br/>• Đóng ticket vi phạm"]:::reject
```

---

## 6️⃣ WORKFLOW 6: QUẢN TRỊ HỆ THỐNG (BACK-OFFICE MANAGEMENT)

> 💡 **Lưu ý**: Khối Back-Office gồm 7 Module quản trị độc lập hoạt động song song, không sử dụng mũi tên quy trình liên tiếp (`-->`).

```mermaid
flowchart LR
    classDef admin fill:#ffe4e6,stroke:#be123c,stroke-width:2px,color:#881337
    classDef staff fill:#fef3c7,stroke:#b45309,stroke-width:2px,color:#78350f

    M1["1️⃣ Quản lý Instructor<br/>• Duyệt hồ sơ Đăng ký<br/>• Kiểm tra Bằng cấp"]:::staff
    M2["2️⃣ Quản lý Account & RBAC<br/>• Ban / Unban tài khoản<br/>• Phân quyền Role"]:::admin
    M3["3️⃣ Quản lý AI Service<br/>• Cấu hình AI Risk Score<br/>• Quản lý Từ cấm"]:::admin
    M4["4️⃣ Quản lý Khóa học<br/>• Phê duyệt / Từ chối khóa<br/>• Giám sát chất lượng"]:::staff
    M5["5️⃣ Quản lý Review & Chat<br/>• Đánh giá học viên<br/>• Giám sát SignalR Logs"]:::staff
    M6["6️⃣ Trung tâm Report<br/>• Tiếp nhận Báo cáo<br/>• Áp dụng chế tài vi phạm"]:::staff
    M7["7️⃣ Báo cáo Tài chính & Refund<br/>• Gross Revenue Ledger<br/>• Duyệt hoàn tiền Refund"]:::admin
```
