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
	
	
