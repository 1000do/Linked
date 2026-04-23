# Hướng dẫn giải phóng dung lượng của docker

## Mở docker desktop
Mở tab Containers<br>
Ở cột Actions, click biểu tượng nút Play để start các container liên quan (hiên có frontend, backend, db là 3 cái)<br>
Đợi nó bật sáng hết cả đèn xanh
	
## Mở cmd

Chạy
```cmd
docker system df
docker system prune -a --volumes
```
	
Nhập "y" khi đc hỏi "Are you sure you want to continue?"<br>
Tắt docker desktop, tắt luôn ở góc dưới bên phải màn hình

## Mở thư mục (File Explorer)
Click vô thanh địa chỉ ở trên, truy cập vào:
```cmd
%localappdata%\Docker\wsl\disk
```
		
Chuột phải vô cái file docker_data.vhdx trong thư mục vừa truy cập, chọn "Copy as path", xong paste vô cái **PATH** ở **COMMAND** bên dưới rồi lưu nguyên cái **COMMAND** vô đâu đó (vd: Notepad)<br><br>
	
**COMMAND**:
```cmd
select vdisk file=PATH
attach vdisk readonly
compact vdisk
detach vdisk
exit
```
	
### Ví dụ:
**COMMAND**:
```cmd
select vdisk file="C:\Users\anhkc\AppData\Local\Docker\wsl\disk\docker_data.vhdx"
attach vdisk readonly
compact vdisk
detach vdisk
exit
```
## Vô lại cmd
Chạy
```cmd
wsl --shutdown
diskpart
```
Chọn Allow nếu được hỏi, cửa sổ **CMD DISKPART** sẽ đc mở<br>
Copy paste cái **COMMAND** đã lưu ở trên vô cửa sổ vừa đc mở, Enter chạy

#

# Hướng dẫn build, chạy các thứ
## Tắt postgresql trên máy (làm 1 lần duy nhất)
 - Dùng thanh search của windows để vào **Services**
 - Tìm cái service tên bắt đầu với **postgresql...**
 - Chuột phải vô nó rồi chọn **Properties** để mở cửa sổ setting cái service
    - Startup type: Manual
    - Service status: Stop
- Click ỌK để save

## Màn chính
- Mở docker desktop xong để đó
- Mở thư mục chứa project (Linked)
- Đè Shift, bấm chuột phải vô cửa sổ thư mục
- Chọn **Open in Terminal** để mở cmd tại thư mục project

```cmd
# TH1: Chạy lần đầu
# TH2: Có thay đổi trong database (cần reset lại hết dữ liệu)
docker compose down -v
docker compose up --build -d

# TH3: Rebuild + rerun project, ko làm thay đổi database
docker compose up --build -d

# TH4: Rebuild + rerun frontend, ko làm ảnh hưởng đến backend, db
docker compose build frontend
docker start linked-frontend-1

# TH5: Rebuild + rerun backend, ko làm ảnh hưởng đến frontend, db
docker compose build backend
docker start linked-backend-1 


# TH6: Chạy những lần sau
docker compose up -d
```

- Backend:
```cmd
localhost:5207/swagger
```
- Frontend:
```cmd
localhost:5208
```
- Database:
    - Mở pgAdmin (con voi)
    - Chuột phải Servers > Register > Server... để mở cửa sổ tạo server db
        - Tab General:
            - Name: docker
        - Tab Connection:
            - Host name/address: localhost
            - Port: 5432
            - Maintenance database: postgres
            - Username: postgres
            - Password: 123456
            - Save password? Bật
    - Click Save
    - Server db tên **docker** sẽ đc tạo ra, trong đó sẽ có 2 database, 1 cái tên **linked** là cái mình dùng






