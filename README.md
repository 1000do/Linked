# Hướng dẫn giải phóng dung lượng của docker

## Mở docker desktop
	Mở tab Containers
	Ở cột Actions Start hết container liên quan (hiên có frontend, backend, db là 3 cái)
	Đợi nó bật sáng hết cả đèn xanh
	
## Mở cmd

	
	```cmd
	docker system df
	docker system prune -a --volumes
	```
	
	Nhập "y" khi đc hỏi "Are you sure you want to continue?"
	

## Tắt docker desktop, tắt luôn ở góc dưới bên phải màn hình

## Mở thư mục (File Explorer)
	Click vô thanh địa chỉ ở trên, truy cập vào:
		%localappdata%\Docker\wsl\disk
		
	Chuột phải vô cái file docker_data.vhdx trong thư mục vừa truy cập, chọn "Copy as path", xong paste vô cái **PATH** ở dưới
	
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
	
## Vô lại cmd
	```cmd
	wsl --shutdown
	diskpart
	```
	Chọn Allow nếu được hỏi, cửa sổ **CMD DISKPART** sẽ đc mở
	Copy paste cái **COMMAND** ở trên vô cửa sổ vừa đc mở, Enter chạy
	
	
