# Hướng dẫn cài đặt các biến môi trường (.env)

Tạo một file tên là `.env` ở thư mục gốc của project (ngang hàng với `docker-compose.yml`) và copy nội dung từ file `.env.example` sang. Dưới đây là giải thích chi tiết cách thiết lập từng biến:

### 1. Database (PostgreSQL)
- `DB_HOST`: Tên service của database trong docker (mặc định là `db`).
- `DB_PORT`: Cổng kết nối (mặc định `5432`).
- `DB_NAME`: Tên database bạn muốn tạo (vd: `linked`).
- `DB_USER`: Tên user của postgres (mặc định `postgres`).
- `DB_PASSWORD`: Mật khẩu database (tuỳ chọn mật khẩu của bạn, vd: `123456`).

### 2. JWT (Xác thực người dùng)
- `JWT_KEY`: Chuỗi bí mật dùng để mã hoá token. Cần đặt một chuỗi ngẫu nhiên, dài và bảo mật (vd: `Day_La_Chuoi_Secret_Key_Sieu_Bao_Mat_123456`).
- `JWT_ISSUER`: Tên hệ thống phát hành token (vd: `CourseMarketplaceBE`).
- `JWT_AUDIENCE`: Đối tượng sử dụng token (vd: `CourseMarketplaceUser`).
- `JWT_DURATION`: Thời gian hết hạn của token tính bằng phút (vd: `1440` tương đương 24 giờ).

### 3. Cloudinary (Lưu trữ ảnh & video)
Tạo tài khoản miễn phí tại [Cloudinary](https://cloudinary.com/) để lấy các thông tin này ở trang Dashboard.
- `CLOUDINARY_CLOUD_NAME`: Tên cloud name của bạn.
- `CLOUDINARY_API_KEY`: API Key được cấp.
- `CLOUDINARY_API_SECRET`: API Secret được cấp.
- `CLOUDINARY_UPLOAD_PRESET`: Upload Preset. Cần vào Settings > Upload > Add upload preset (chọn Signing Mode là `Unsigned`).

### 4. Email & Google Login
- `EMAIL_HOST`: Server gửi mail (nếu dùng gmail thì để `smtp.gmail.com`).
- `EMAIL_PORT`: Cổng SMTP (mặc định `587`).
- `EMAIL_ENABLESSL`: Bật SSL (`true`).
- `EMAIL_EMAIL`: Địa chỉ email dùng để gửi mã OTP/Thông báo.
- `EMAIL_PASSWORD`: Mật khẩu ứng dụng (App Password). 
  - *Lưu ý*: **KHÔNG** dùng mật khẩu đăng nhập tài khoản. Bạn cần vào Quản lý tài khoản Google > Bảo mật > Xác minh 2 bước > Mật khẩu ứng dụng để tạo 1 cái mã 16 chữ số.
- `GOOGLE_CLIENT_ID`: Dùng cho chức năng Login with Google. Tạo tài khoản Google Cloud Console, tạo dự án và lấy Client ID.

### 5. URL & Cấu hình môi trường
- `FRONTEND_BASE_URL`: Link của frontend (vd: `http://localhost:5208`).
- `ALLOWED_ORIGINS`: Các link được phép gọi API BE (chống CORS). Các link cách nhau bằng dấu phẩy (vd: `http://localhost:5208,http://localhost:5207`).
- `ASPNETCORE_ENVIRONMENT`: Môi trường chạy (để `Development` khi code).
- `NGROK_TOKEN`: Token ngrok (nếu dùng). Lấy tại [ngrok.com](https://ngrok.com/).

### 6. Stripe (Thanh toán trực tuyến)
Đăng ký tài khoản tại [Stripe](https://stripe.com/) và bật chế độ **Test mode**.
- `STRIPE_PUBLISHABLE_KEY`: Lấy tại trang chủ Dashboard (bắt đầu bằng `pk_test_...`).
- `STRIPE_SECRET_KEY`: Lấy tại trang chủ Dashboard (bắt đầu bằng `sk_test_...`).
- `STRIPE_CONNECT_WEBHOOK_SECRET`: Bạn cần thiết lập Webhook trong tab Developers để Stripe gửi thông báo thanh toán về BE (bắt đầu bằng `whsec_...`).

### 7. Redis (Bộ nhớ đệm / Cache)
- `REDIS_HOST`: Tên service Redis trong docker (mặc định `redis`).
- `REDIS_PORT`: Cổng Redis (mặc định `6379`).
- `REDIS_DB`: Index database (mặc định `0`).
- `REDIS_PASSWORD`: Mật khẩu (thường để trống ở local).

### 8. AI Moderation (Kiểm duyệt nội dung tự động)
Hầu hết các thông số đều để mặc định. Bạn chỉ cần lưu ý:
- `HF_TOKEN`: Token truy cập HuggingFace để tải Model AI. Bạn cần tạo tài khoản [HuggingFace](https://huggingface.co/), vào Settings > Access Tokens để tạo 1 cái token.
- `DEVICE`: Card đồ hoạ sử dụng. Nếu máy bạn không có card NVIDIA thì để `cpu`, nếu có NVIDIA để `cuda`, nếu dùng Macbook chip M thì để `mps`.
- `AI_DEBUG`: Để `true` nếu muốn in log chi tiết ra terminal.
- Các thông số mô hình (`SPAM_MODEL_PATH`, `TOXIC_MODEL_PATH`, `CLIP_MODEL_NAME`, `WHISPER_MODEL_NAME`, v.v.) và kích thước embedding (`MEDIA_EMBEDDING_DIM`, `TEXT_EMBEDDING_DIM`): Không nên thay đổi trừ khi bạn tự train model khác.
- `REQUEST_TIMEOUT`: Thời gian chờ tối đa cho các request AI (mặc định `1800` giây).
- `AI_PORT`: Cổng chạy service AI (mặc định `8000`).

#

# 🚀 Hướng dẫn khởi chạy Project

## Bước 1: Tắt PostgreSQL mặc định trên máy (Chỉ làm 1 lần duy nhất)
*Lý do: Để tránh bị trùng cổng (port 5432) với Database trong Docker.*
- Bấm nút Windows, gõ tìm và mở ứng dụng **Services**.
- Tìm đến dòng nào bắt đầu bằng chữ **postgresql...**
- Chuột phải vào nó > Chọn **Properties**.
- Chỉnh **Startup type** thành `Manual`.
- Bấm nút **Stop** ở phần Service status.
- Bấm **OK** để lưu lại.

## Bước 2: Chạy Project
- **Mở phần mềm Docker Desktop** và đợi cho đến khi icon chuyển màu xanh (báo hiệu Docker đã sẵn sàng).
- Mở thư mục chứa project (`Linked`).
- Đè phím `Shift` + Click chuột phải vào khoảng trống trong thư mục > Chọn **Open in Terminal** (hoặc **Open PowerShell window here**).
- Tại màn hình dòng lệnh đen, **chọn copy và chạy 1 trong các trường hợp sau** tuỳ vào mục đích của bạn:

### 🌟 Chọn 1 trường hợp phù hợp nhất với bạn lúc này:

**1. Chạy lần đầu tiên** HOẶC **Muốn xoá sạch toàn bộ dữ liệu cũ để làm lại từ đầu:**
```bash
docker compose down -v
docker compose up --build -d
```

**2. Chạy bình thường hằng ngày (Nhanh nhất):**
```bash
docker compose up -d
```

**3. Khi Database có thay đổi cấu trúc (Cập nhật Schema):**
- **Cách A: Cho phép xoá hết dữ liệu cũ (Cách đơn giản nhất)**
  Bạn dùng lệnh xoá hoàn toàn Volume cũ rồi build lại từ đầu:
  ```bash
  docker compose down -v
  docker compose up --build -d
  ```
- **Cách B: Giữ lại dữ liệu hiện tại (Không muốn xoá)**
  Bạn phải tự cập nhật DB thủ công (chạy script update SQL hoặc Migrate). Sau khi update DB xong, bạn chạy lệnh build lại bình thường:
  ```bash
  docker compose up --build -d
  ```

**4. Chỉ cập nhật riêng code Frontend (Không đụng đến Backend và DB):**
```bash
docker compose up -d --build frontend
```

**5. Chỉ cập nhật riêng code Backend (Không đụng đến Frontend và DB):**
```bash
docker compose up -d --build backend
```

## Bước 3: Cách truy cập sau khi chạy thành công
- **Frontend (Giao diện web):** Truy cập [http://localhost:5208](http://localhost:5208)
- **Backend (API Swagger):** Truy cập [http://localhost:5207/swagger](http://localhost:5207/swagger)
- **Database (Xem dữ liệu bằng pgAdmin):**
  1. Mở phần mềm **pgAdmin** (biểu tượng con voi).
  2. Chuột phải vào chữ **Servers** (cột bên trái) > **Register** > **Server...**
  3. Tab **General**: Mục Name nhập là `docker`.
  4. Tab **Connection** nhập như sau:
     - Host name/address: `localhost`
     - Port: `5432`
     - Maintenance database: `postgres`
     - Username: `postgres`
     - Password: `123456`
     - Save password?: Bật lên.
  5. Bấm **Save**. Lúc này ở cột trái sẽ xuất hiện server tên `docker`, mở ra bạn sẽ thấy database tên `linked` để sử dụng.

---

# 🧹 Hướng dẫn dọn dẹp và giải phóng dung lượng Docker
*Lý do: Dùng Docker lâu ngày ổ C: sẽ bị đầy. Quá trình này giúp xóa các rác thừa và thu nhỏ ổ cứng ảo của Docker.*

## Phần 1: Dọn rác bên trong Docker
Docker sẽ xóa TẤT CẢ mọi thứ đang bị tắt. Do đó chúng ta cần "bảo vệ" project này bằng cách bật nó lên trước khi dọn dẹp.

1. Mở Docker Desktop, vào tab **Containers**.
2. Bấm nút ▶️ (Play) để khởi động toàn bộ các container của project này (frontend, backend, db). Đợi chúng sáng đèn xanh hết.
3. Mở Terminal (CMD), chạy lần lượt 2 lệnh sau:
   ```bash
   docker system df
   docker system prune -a --volumes
   ```
4. Hệ thống sẽ hỏi `Are you sure you want to continue?`, bạn gõ `y` và nhấn Enter.
5. Sau khi xoá xong, **Tắt hoàn toàn Docker Desktop** (Chuột phải vào icon con cá voi ở góc phải dưới cùng màn hình > Chọn `Quit Docker Desktop`).

## Phần 2: Thu nhỏ dung lượng ổ cứng ảo (Nâng cao)
1. Mở thư mục (File Explorer), copy và dán đường dẫn này vào thanh địa chỉ bên trên rồi Enter:
   ```cmd
   %localappdata%\Docker\wsl\disk
   ```
2. Bạn sẽ thấy 1 file tên là `docker_data.vhdx`. Bấm **Shift + Chuột phải** vào file đó > Chọn **Copy as path**.
3. Mở lại Terminal (CMD), chạy lần lượt 2 lệnh:
   ```bash
   wsl --shutdown
   diskpart
   ```
4. Nếu Windows hỏi có cho phép chạy không, hãy chọn **Yes** (Allow). Một cửa sổ mới màu đen tên là `DISKPART` sẽ hiện lên.
5. Tại cửa sổ mới này, bạn copy lần lượt từng lệnh dưới đây dán vào và Enter. 
   *(Lưu ý: Thay chữ `[DÁN_PATH_VÀO_ĐÂY]` bằng đường dẫn bạn vừa Copy ở bước 2)*

   ```cmd
   select vdisk file=[DÁN_PATH_VÀO_ĐÂY]
   attach vdisk readonly
   compact vdisk
   detach vdisk
   exit
   ```
   **Ví dụ câu lệnh đúng sẽ trông như thế này:** 
   `select vdisk file="C:\Users\anhkc\AppData\Local\Docker\wsl\disk\docker_data.vhdx"`
