-- ==============================================================================
-- SCRIPT SỬA ĐỔI DB: BỔ SUNG BẢNG KIỂM DUYỆT ĐÁNH GIÁ & ĐỒNG BỘ SEQUENCE
-- ==============================================================================

-- 1. Tạo bảng course_review_moderation_records nếu chưa tồn tại
CREATE TABLE IF NOT EXISTS course_review_moderation_records (
    record_id SERIAL PRIMARY KEY,
    course_review_id INT NOT NULL REFERENCES course_reviews(course_review_id) ON DELETE CASCADE,
    is_update BOOLEAN NOT NULL,
    temp_comment TEXT NOT NULL,
    temp_rating NUMERIC(3,2) NOT NULL CHECK (temp_rating >= 0 AND temp_rating <= 5),
    ai_moderation_status VARCHAR(50) NOT NULL CHECK (ai_moderation_status IN ('pending', 'manual_audit', 'flagged', 'approved')),
    ai_moderation_note TEXT NOT NULL,
    moderation_status VARCHAR(50) NOT NULL CHECK (moderation_status IN ('pending', 'approved', 'rejected')),
    moderation_note TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- 2. Tạo bảng lesson_review_moderation_records nếu chưa tồn tại
CREATE TABLE IF NOT EXISTS lesson_review_moderation_records (
    record_id SERIAL PRIMARY KEY,
    lesson_review_id INT NOT NULL REFERENCES lesson_reviews(lesson_review_id) ON DELETE CASCADE,
    is_update BOOLEAN NOT NULL,
    temp_comment TEXT NOT NULL,
    temp_rating NUMERIC(3,2) NOT NULL CHECK (temp_rating >= 0 AND temp_rating <= 5),
    ai_moderation_status VARCHAR(50) NOT NULL CHECK (ai_moderation_status IN ('pending', 'manual_audit', 'flagged', 'approved')),
    ai_moderation_note TEXT NOT NULL,
    moderation_status VARCHAR(50) NOT NULL CHECK (moderation_status IN ('pending', 'approved', 'rejected')),
    moderation_note TEXT NOT NULL,
    created_at TIMESTAMP NOT NULL,
    updated_at TIMESTAMP NOT NULL
);

-- 3. Cập nhật lại Sequence cho các bảng
SELECT setval(pg_get_serial_sequence('course_review_moderation_records', 'record_id'), COALESCE((SELECT MAX(record_id) FROM course_review_moderation_records), 1), false);
SELECT setval(pg_get_serial_sequence('lesson_review_moderation_records', 'record_id'), COALESCE((SELECT MAX(record_id) FROM lesson_review_moderation_records), 1), false);
SELECT setval(pg_get_serial_sequence('course_reviews', 'course_review_id'), COALESCE((SELECT MAX(course_review_id) FROM course_reviews), 1), false);
SELECT setval(pg_get_serial_sequence('lesson_reviews', 'lesson_review_id'), COALESCE((SELECT MAX(lesson_review_id) FROM lesson_reviews), 1), false);

-- 4. Dọn dẹp bản ghi review mồ côi bị lỗi từ trước
DELETE FROM course_reviews WHERE course_review_status = 'pending' AND course_review_id NOT IN (SELECT course_review_id FROM course_review_moderation_records);
DELETE FROM lesson_reviews WHERE lesson_review_status = 'pending' AND lesson_review_id NOT IN (SELECT lesson_review_id FROM lesson_review_moderation_records);
