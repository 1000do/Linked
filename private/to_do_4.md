- Khi resolve course review, lesson review thì tiến hành update review bị report như sau:
	- Nếu ko tick xóa + accept report -> chỉ gửi thông báo warning cho author kèm linkaction tới review bị report, ko update review (ko quan trọng bn flag)
	- Nếu tick xóa + accept report: giữ nguyên như hiện tại
	- Nếu dismiss thì như cũ
	- Nếu underreview thì phải gửi thông báo mô tả cho chính xác chứ ko nên dùng chung với cái rejected
	- Nếu staff escalate cho admin thì phải gửi thông báo cho admin
	
- Khi resolve course report thì tiến hành update course bị report như sau :
	- Nếu ko tick xóa + accept report: -> chỉ gửi thông báo warning cho course owner kèm linkaction tới course bị report (trang edit, ko phải trang detail), ko update course (ko quan trọng bn flag)
	- Nếu tick xóa + accept report: (tái sử dụng logic của CourseModerationService.FlagCourseAsync)
			- course flag count < 2: 
				- course status = flagged 
				- course flag count += 1
				- thông báo cho course owner và reporter về kết quả của report 
			- course flag count >= 2: 
				- course status = archived
				- course flag count += 1 (ko reset flag count)
				- thông báo cho course owner và reporter về kết quả của report, riêng course owner thì thông báo luôn là nó bị lockout tới khi nào và lí do
				- thông báo cho tất học viên đang có enrollment vào các khóa học của course owner là thằng này bị lockout tới ngày bn và lí do 
			
	- Nếu dismiss thì như cũ
	- Nếu underreview thì phải gửi thông báo mô tả cho chính xác chứ ko nên dùng chung với cái rejected
	- Nếu staff escalate cho admin thì phải gửi thông báo cho admin
	
- Course đã bị archived và >= 3 flag thì ko cho instructor unhide hay sửa chữa gì hết

- Course Moderation Dashboard: Thêm nút unflag cho archived và flagged (bấm vô sẽ giảm 1 flag nếu flag hiện tại đang > 0)

- Lúc view course detail:
	- Đối với user: nếu chưa có enrollment thì chỉ cho view nếu course đang published, nếu đã có enrollment, xem thoải mái
	- Đối với staff,admin và owner (instructor tạo khóa học): cho view thoải mái
	- Nếu course status khác published, thêm 1 dòng tiêu đề position fixed trên giao diện (ở dưới header, hiện trạng thái hiện tại của khóa học để cảnh báo cho người xem) 
- Khi có 1 review mới thì ở trong thông báo gửi cho cousrse owner phải có linkaction dẫn tới nơi hiển thị review đó


- User tự xóa review của mình thì phải placeholder khác "This review was removed by the author."