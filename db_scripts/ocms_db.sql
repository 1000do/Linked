-- ==============================================================================
-- XÓA BẢNG CŨ NẾU ĐÃ TỒN TẠI (Giúp bạn dễ dàng chạy lại script nhiều lần)
-- ==============================================================================
DROP TABLE IF EXISTS platform_withdrawals CASCADE;
DROP TABLE IF EXISTS ai_activity_logs CASCADE;
DROP TABLE IF EXISTS gifts CASCADE;
DROP TABLE IF EXISTS courses_ai_integrations CASCADE;
DROP TABLE IF EXISTS ai_models CASCADE;
DROP TABLE IF EXISTS system_configs CASCADE;
DROP TABLE IF EXISTS course_reports CASCADE;
DROP TABLE IF EXISTS course_review_reports CASCADE;
DROP TABLE IF EXISTS lesson_review_reports CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS messages CASCADE;
DROP TABLE IF EXISTS chats CASCADE;
DROP TABLE IF EXISTS course_ai_feedbacks CASCADE;
DROP TABLE IF EXISTS lesson_ai_feedbacks CASCADE;
DROP TABLE IF EXISTS learning_material_ai_feedbacks CASCADE;
DROP TABLE IF EXISTS lockouts CASCADE;
DROP TABLE IF EXISTS transaction_exts CASCADE;
DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS order_info CASCADE;
DROP TABLE IF EXISTS course_reviews CASCADE;
DROP TABLE IF EXISTS cart_items CASCADE;
DROP TABLE IF EXISTS wishlist_items CASCADE;
DROP TABLE IF EXISTS enrollments CASCADE;
DROP TABLE IF EXISTS material_completions CASCADE;
DROP TABLE IF EXISTS learning_materials CASCADE;
DROP TABLE IF EXISTS lessons CASCADE;
DROP TABLE IF EXISTS courses CASCADE;
DROP TABLE IF EXISTS coupons CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS instructors CASCADE;
DROP TABLE IF EXISTS managers CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS accounts CASCADE;
DROP TABLE IF EXISTS text_embeddings CASCADE;
DROP TABLE IF EXISTS media_embeddings CASCADE;
DROP TABLE IF EXISTS course_exts CASCADE;
DROP TABLE IF EXISTS material_exts CASCADE;
DROP TABLE IF EXISTS lesson_reviews CASCADE;
DROP TABLE IF EXISTS course_ai_usage_logs CASCADE;
DROP TABLE IF EXISTS message_moderation_logs CASCADE;
DROP TABLE IF EXISTS lesson_review_moderation_logs CASCADE;
DROP TABLE IF EXISTS course_review_moderation_logs CASCADE;
DROP TABLE IF EXISTS lesson_review_moderation_records CASCADE;
DROP TABLE IF EXISTS course_review_moderation_records CASCADE;
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS message_attachments CASCADE;
DROP TABLE IF EXISTS quiz_attempt_answers CASCADE;
DROP TABLE IF EXISTS quiz_attempt_questions CASCADE;
DROP TABLE IF EXISTS quiz_attempts CASCADE;
DROP TABLE IF EXISTS course_quizzes CASCADE;
DROP TABLE IF EXISTS quiz_lesson_distributions CASCADE;
DROP TABLE IF EXISTS quiz_options CASCADE;
DROP TABLE IF EXISTS quiz_questions CASCADE;
DROP TABLE IF EXISTS quizzes CASCADE;

-- ==============================================================================
-- Drop indexes if they exist
-- ==============================================================================
DROP INDEX IF EXISTS idx_reviews_enrollment;
DROP INDEX IF EXISTS idx_enrollments_course;
DROP INDEX IF EXISTS idx_enrollments_user;
DROP INDEX IF EXISTS idx_courses_instructor;
DROP INDEX IF EXISTS idx_lessons_course;
DROP INDEX IF EXISTS idx_materials_lesson;
DROP INDEX IF EXISTS idx_order_info_user;
DROP INDEX IF EXISTS idx_order_items_order;
DROP INDEX IF EXISTS idx_reviews_active;
DROP INDEX IF EXISTS idx_order_paid;
DROP INDEX IF EXISTS idx_material_duration;
DROP INDEX IF EXISTS idx_metadata_gin;
DROP INDEX IF EXISTS idx_course_reviews_enrollment;
DROP INDEX IF EXISTS idx_lesson_reviews_enrollment;
DROP INDEX IF EXISTS idx_lesson_reviews_lesson;
DROP INDEX IF EXISTS idx_course_reviews_active;

-- ==============================================================================
-- Use pgvector extension
-- ==============================================================================
CREATE EXTENSION IF NOT EXISTS vector;

-- ==============================================================================
-- 1. NHÓM QUẢN LÝ TÀI KHOẢN (Account & User Management)
-- ==============================================================================

CREATE TABLE accounts (
    account_id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    username VARCHAR(255) UNIQUE,
    password_hash TEXT,
    phone_number VARCHAR(50),
    account_status VARCHAR(50), -- VD: 'active', 'suspended', 'banned'
    account_flag_count INT DEFAULT 0,
    auth_provider VARCHAR(50),
    avatar_url TEXT,
    refresh_token TEXT,
    refresh_token_expiry_time TIMESTAMP,
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    account_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_last_login_at TIMESTAMP
);

CREATE TABLE lockouts (
    lockout_id SERIAL PRIMARY KEY,
    account_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    lockout_type VARCHAR(50), -- account, review, instructor
    lockout_level VARCHAR(50), -- moderate, severe
    lockout_start TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    lockout_end TIMESTAMP
);

CREATE INDEX IX_lockouts_account_id ON lockouts(account_id);

CREATE TABLE users (
    user_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    bio TEXT,
    date_of_birth DATE
);



CREATE TABLE managers (
    manager_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    role VARCHAR(50),
    display_name VARCHAR(255) NOT NULL,
    full_name VARCHAR(255),
    phone_number VARCHAR(50),
    avatar_url TEXT,
    bio TEXT
);

CREATE TABLE instructors (
    instructor_id INT PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    stripe_account_id VARCHAR(255),
    stripe_onboarding_status VARCHAR(50),
    payouts_enabled BOOLEAN DEFAULT FALSE,
    charges_enabled BOOLEAN DEFAULT FALSE,
    professional_title VARCHAR(255),
    expertise_categories VARCHAR(255),
    linkedin_url TEXT,
	youtube_url TEXT,
    facebook_url TEXT,
    document_url TEXT,
    approval_status VARCHAR(50) DEFAULT 'Pending',
    stripe_country VARCHAR(2),
    rejection_reason TEXT
);



-- ==============================================================================
-- 2. NHÓM QUẢN LÝ KHÓA HỌC (Course Management)
-- ==============================================================================

CREATE TABLE categories (
    category_id SERIAL PRIMARY KEY,
    categories_name VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    category_status VARCHAR(50)
);

CREATE TABLE coupons (
    coupon_id SERIAL PRIMARY KEY,
    manager_id INT REFERENCES managers(manager_id) ON DELETE SET NULL,
    coupon_code VARCHAR(50) UNIQUE NOT NULL,
    coupon_type VARCHAR(50),
    discount_value NUMERIC(10, 2) NOT NULL,
    min_order_value NUMERIC(10, 2) NOT NULL DEFAULT 0,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    usage_limit INT,
    used_count INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE courses (
    course_id SERIAL PRIMARY KEY,
    instructor_id INT REFERENCES instructors(instructor_id) ON DELETE SET NULL,
    category_id INT REFERENCES categories(category_id) ON DELETE SET NULL,
    coupon_id INT REFERENCES coupons(coupon_id) ON DELETE SET NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    price NUMERIC(10, 2) NOT NULL,
    course_thumbnail_url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    course_status VARCHAR(50),
    course_flag_count INT DEFAULT 0,
    what_you_will_learn TEXT,
    requirements TEXT,
    moderation_feedback TEXT,
    last_approved_at TIMESTAMP,
    is_removed BOOLEAN DEFAULT FALSE,
    threat_level INT DEFAULT 1
);

CREATE TABLE lessons (
    lesson_id SERIAL PRIMARY KEY,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    thumbnail_url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    lesson_status VARCHAR(50),
    moderation_feedback TEXT,
    is_removed BOOLEAN DEFAULT FALSE
);

CREATE TABLE learning_materials (
    material_id SERIAL PRIMARY KEY,
    lesson_id INT REFERENCES lessons(lesson_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    learning_status VARCHAR(50),
    moderation_feedback TEXT,
    material_url TEXT,
    material_metadata JSONB,
    cloud_public_id TEXT
);

-- CREATE TABLE course_exts (
--     course_id INT PRIMARY KEY REFERENCES courses(course_id) ON DELETE CASCADE,
--     title_hash CHAR(32) UNIQUE,
--     description_hash CHAR(32) UNIQUE,
--     what_you_will_learn_hash CHAR(32) UNIQUE,
--     requirements_hash CHAR(32) UNIQUE,
--     thumbnail_hash CHAR(32) UNIQUE
-- );

CREATE TABLE course_exts (
    course_id INT PRIMARY KEY REFERENCES courses(course_id) ON DELETE CASCADE,
    title_hash CHAR(32),
    description_hash CHAR(32),
    what_you_will_learn_hash CHAR(32),
    requirements_hash CHAR(32),
    thumbnail_hash CHAR(32),
    
    CONSTRAINT uq_title_hash UNIQUE (title_hash),
    CONSTRAINT uq_description_hash UNIQUE (description_hash),
    CONSTRAINT uq_what_you_will_learn_hash UNIQUE (what_you_will_learn_hash),
    CONSTRAINT uq_requirements_hash UNIQUE (requirements_hash),
    CONSTRAINT uq_thumbnail_hash UNIQUE (thumbnail_hash)
);

CREATE TABLE material_exts (
    material_id INT PRIMARY KEY REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    file_hash CHAR(32),
    
    CONSTRAINT uq_material_file_hash UNIQUE (file_hash)
);


CREATE TABLE text_embeddings (
    text_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    text_embedding vector(384),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE media_embeddings (
    media_embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    media_embedding vector(512),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 3. NHÓM HỌC TẬP & TƯƠNG TÁC (Learning & Engagement)
-- ==============================================================================

CREATE TABLE enrollments (
    enrollment_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
    UNIQUE(user_id, course_id),
    title VARCHAR(255),
    description TEXT,
    completed_date DATE,
    is_completed BOOLEAN DEFAULT FALSE,
    enroll_date DATE DEFAULT CURRENT_DATE,
    last_accessed_at TIMESTAMP,
    enrollment_status VARCHAR(50)
);



CREATE TABLE material_completions (
    id SERIAL PRIMARY KEY,
    enrollment_id INT NOT NULL REFERENCES enrollments(enrollment_id) ON DELETE CASCADE,
    material_id INT NOT NULL REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    completed_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UNIQUE(enrollment_id, material_id)
);

CREATE TABLE wishlist_items (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
    UNIQUE(user_id, course_id),
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_reviews (
    course_review_id SERIAL PRIMARY KEY,
    enrollment_id INT NOT NULL REFERENCES enrollments(enrollment_id) ON DELETE CASCADE,
    rating NUMERIC(3,2) CHECK (rating >= 0 AND rating <= 5),
    comment TEXT,
    course_review_status TEXT NOT NULL DEFAULT 'ok',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_removed BOOLEAN DEFAULT FALSE
);

CREATE TABLE lesson_reviews (
    lesson_review_id SERIAL PRIMARY KEY,
    enrollment_id INT NOT NULL REFERENCES enrollments(enrollment_id) ON DELETE CASCADE,
    lesson_id INT REFERENCES lessons(lesson_id) ON DELETE SET NULL,
    rating NUMERIC(3,2) CHECK (rating >= 0 AND rating <= 5),
    comment TEXT,
    lesson_review_status TEXT NOT NULL DEFAULT 'ok',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_removed BOOLEAN DEFAULT FALSE
);

CREATE TABLE course_review_moderation_records (
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

CREATE TABLE lesson_review_moderation_records (
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

-- ==============================================================================
-- 3B. NHÓM QUIZ (Quiz Management)
-- ==============================================================================

-- Bộ quiz
CREATE TABLE quizzes (
    quiz_id SERIAL PRIMARY KEY,
    instructor_id INT NOT NULL REFERENCES instructors(instructor_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    time_limit_minutes INT,                         -- NULL = không giới hạn thời gian
    passing_score INT NOT NULL DEFAULT 70           -- Điểm tối thiểu để pass (0–100)
        CHECK (passing_score >= 0 AND passing_score <= 100),
    total_questions INT NOT NULL DEFAULT 10,
    is_hidden BOOLEAN NOT NULL DEFAULT FALSE,       -- Ẩn quiz toàn cục
    is_removed BOOLEAN NOT NULL DEFAULT FALSE,      -- Xóa mềm
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Bảng phân bổ tỷ lệ phần trăm câu hỏi theo lesson cho mỗi quiz
CREATE TABLE quiz_lesson_distributions (
    distribution_id SERIAL PRIMARY KEY,
    quiz_id INT NOT NULL REFERENCES quizzes(quiz_id) ON DELETE CASCADE,
    lesson_id INT NOT NULL REFERENCES lessons(lesson_id) ON DELETE CASCADE,
    question_count INT NOT NULL DEFAULT 0,
    CONSTRAINT uq_quiz_lesson UNIQUE (quiz_id, lesson_id)
);

-- Ngân hàng câu hỏi (Course/Lesson)
-- question_type: 'SingleChoice' | 'MultiChoice' | 'TrueFalse'
CREATE TABLE quiz_questions (
    question_id SERIAL PRIMARY KEY,
    course_id INT NOT NULL REFERENCES courses(course_id) ON DELETE CASCADE,
    lesson_id INT REFERENCES lessons(lesson_id) ON DELETE SET NULL,
    question_text TEXT NOT NULL,
    explanation TEXT NULL,
    question_type VARCHAR(20) NOT NULL DEFAULT 'SingleChoice'
        CHECK (question_type IN ('SingleChoice', 'MultiChoice', 'TrueFalse')),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Đáp án của từng câu hỏi
CREATE TABLE quiz_options (
    option_id SERIAL PRIMARY KEY,
    question_id INT NOT NULL REFERENCES quiz_questions(question_id) ON DELETE CASCADE,
    option_text TEXT NOT NULL,
    is_correct BOOLEAN NOT NULL DEFAULT FALSE,
    order_index INT NOT NULL DEFAULT 0
);

-- Bảng nối: Quiz ↔ Course (1 quiz có thể thêm vào nhiều course)
CREATE TABLE course_quizzes (
    course_quiz_id SERIAL PRIMARY KEY,
    course_id INT NOT NULL REFERENCES courses(course_id) ON DELETE CASCADE,
    quiz_id INT NOT NULL REFERENCES quizzes(quiz_id) ON DELETE CASCADE,
    order_index INT NOT NULL DEFAULT 0,
    is_hidden BOOLEAN NOT NULL DEFAULT FALSE,       -- Ẩn quiz trong course này (không ảnh hưởng quiz gốc)
    added_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_course_quiz UNIQUE (course_id, quiz_id) -- Mỗi quiz chỉ xuất hiện 1 lần trong 1 course
);

-- Lịch sử làm quiz của học viên
CREATE TABLE quiz_attempts (
    attempt_id SERIAL PRIMARY KEY,
    quiz_id INT NOT NULL REFERENCES quizzes(quiz_id) ON DELETE CASCADE,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    score INT,                                      -- Điểm đạt được (0–100), NULL nếu chưa nộp
    is_passed BOOLEAN,                              -- NULL nếu chưa nộp
    started_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    submitted_at TIMESTAMP                          -- NULL nếu chưa nộp bài
);

-- Lưu trữ danh sách câu hỏi đã được sinh ra cho lượt làm quiz này
CREATE TABLE quiz_attempt_questions (
    attempt_question_id SERIAL PRIMARY KEY,
    attempt_id INT NOT NULL REFERENCES quiz_attempts(attempt_id) ON DELETE CASCADE,
    question_id INT NOT NULL REFERENCES quiz_questions(question_id) ON DELETE CASCADE,
    order_index INT NOT NULL DEFAULT 0,
    CONSTRAINT uq_attempt_question UNIQUE (attempt_id, question_id)
);

-- Câu trả lời của học viên trong từng lần làm quiz
CREATE TABLE quiz_attempt_answers (
    answer_id SERIAL PRIMARY KEY,
    attempt_id INT NOT NULL REFERENCES quiz_attempts(attempt_id) ON DELETE CASCADE,
    question_id INT NOT NULL REFERENCES quiz_questions(question_id) ON DELETE CASCADE,
    selected_option_id INT REFERENCES quiz_options(option_id) ON DELETE SET NULL  -- NULL nếu bỏ qua
);

-- Indexes cho Quiz
CREATE INDEX idx_quizzes_instructor ON quizzes(instructor_id);
CREATE INDEX idx_quizzes_course ON quizzes(course_id);
CREATE INDEX idx_quizzes_active ON quizzes(instructor_id) WHERE is_removed = FALSE;
CREATE INDEX idx_quiz_questions_course ON quiz_questions(course_id);
CREATE INDEX idx_quiz_questions_lesson ON quiz_questions(lesson_id);
CREATE INDEX idx_quiz_options_question ON quiz_options(question_id);
CREATE INDEX idx_course_quizzes_course ON course_quizzes(course_id);
CREATE INDEX idx_course_quizzes_quiz ON course_quizzes(quiz_id);
CREATE INDEX idx_quiz_attempts_user ON quiz_attempts(user_id);
CREATE INDEX idx_quiz_attempts_quiz ON quiz_attempts(quiz_id);
CREATE INDEX idx_quiz_attempt_questions_attempt ON quiz_attempt_questions(attempt_id);
CREATE INDEX idx_quiz_attempt_answers_attempt ON quiz_attempt_answers(attempt_id);

-- ==============================================================================
-- 4. NHÓM GIỎ HÀNG & THANH TOÁN (Sales & Transactions)
-- ==============================================================================

CREATE TABLE cart_items (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
    UNIQUE(user_id, course_id),
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    price NUMERIC(10, 2)
);

CREATE TABLE order_info (
    order_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    order_status VARCHAR(50),
    payment_method VARCHAR(50)
);


CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT REFERENCES order_info(order_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
    purchase_price NUMERIC(10, 2) NOT NULL,
	coupon_used BOOLEAN DEFAULT FALSE,
    -- ★ Snapshot giá gốc & coupon tại thời điểm mua (không bị ảnh hưởng khi giá khóa học thay đổi)
    original_price NUMERIC(10, 2),          -- Giá gốc khóa học lúc mua
    coupon_code VARCHAR(50),                -- Mã coupon đã dùng (VD: 'SUMMER20')
    coupon_type VARCHAR(50),                -- Loại coupon: 'percentage' hoặc 'fixed_amount'
    discount_amount NUMERIC(10, 2) DEFAULT 0 -- Số tiền giảm = original_price - purchase_price
);



CREATE TABLE transactions (
    transaction_id SERIAL PRIMARY KEY,
    order_item_id INT REFERENCES order_items(id) ON DELETE SET NULL, -- Mỗi transaction tương ứng with 1 item trong order thay vì cả order
	account_from INT REFERENCES accounts(account_id) ON DELETE SET NULL,
	account_to INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    amount NUMERIC(10, 2) NOT NULL,
	transfer_rate NUMERIC(5,2) NOT NULL DEFAULT 100.00, -- Phần trăm instructor nhận được
    stripe_session_id VARCHAR(255),
    stripe_paymentintent_id VARCHAR(255),
    currency VARCHAR(10) DEFAULT 'VND',
    transactions_status VARCHAR(50), -- VD: 'succeeded', 'failed', 'refunded'
    transaction_type VARCHAR(50), -- VD: 'payment', 'refund'
    transaction_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE transaction_exts (
    transaction_id INT PRIMARY KEY REFERENCES transactions(transaction_id) ON DELETE CASCADE,
    refund_reason TEXT,
    refund_admin_note TEXT,
    refund_requested_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE gifts (
    gift_id SERIAL PRIMARY KEY,
    order_item_id INT NOT NULL REFERENCES order_items(id) ON DELETE CASCADE,
    sender_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    recipient_email VARCHAR(255) NOT NULL,
    recipient_name VARCHAR(255),
    gift_message TEXT,
    card_theme VARCHAR(50) DEFAULT 'classic',
    redemption_token VARCHAR(255) UNIQUE NOT NULL,
    is_claimed BOOLEAN DEFAULT FALSE,
    claimed_by_user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    claimed_at TIMESTAMP,
    delivery_status VARCHAR(50) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_gifts_token ON gifts(redemption_token);
CREATE INDEX idx_gifts_recipient ON gifts(recipient_email);
CREATE INDEX idx_gifts_delivery ON gifts(delivery_status);

-- Bảng lưu dữ liệu những giao dịch chuyển tiền từ hệ thống vô tài khoản ngân hàng của instructor
CREATE TABLE instructor_payouts (
	payout_id SERIAL PRIMARY KEY, 
	transaction_id INT REFERENCES transactions(transaction_id) ON DELETE SET NULL,
	instructor_id INT REFERENCES instructors(instructor_id) ON DELETE SET NULL,
	payout_amount NUMERIC(10,2) NOT NULL, -- Số tiền sẽ chuyển cho instructor (đã trừ bớt phần sàn ăn)
	payout_date TIMESTAMP NOT NULL, -- Ngày mà hệ thống sẽ chuyển tiền cho instructor (theo lịch đã lên) 
	is_paid BOOLEAN NOT NULL DEFAULT FALSE,
	-- ★ PAYOUT STATUS: Track trạng thái thanh toán end-to-end
	-- 'pending'        → Chưa chuyển tiền
	-- 'transferred'    → Đã chuyển sang ví Stripe của giảng viên (Transfer thành công)
	-- 'in_transit'     → Stripe đang chuyển từ ví về ngân hàng
	-- 'paid'           → Đã về tài khoản ngân hàng thật (Webhook payout.paid xác nhận)
	-- 'failed'         → Lỗi trong quá trình chuyển tiền
	payout_status VARCHAR(20) NOT NULL DEFAULT 'pending'
		CHECK (payout_status IN ('pending', 'transferred', 'in_transit', 'paid', 'failed', 'refunded')),
	stripe_transfer_id VARCHAR(255),    -- ID lệnh Transfer từ Sàn → Connected Account (tx_xxx)
	stripe_payout_id VARCHAR(255),      -- ID lệnh Payout từ Connected Account → Bank (po_xxx)
	paid_to_bank_at TIMESTAMP           -- Thời điểm Stripe confirm tiền đã về ngân hàng
);

-- Bảng lưu lịch sử rút tiền lợi nhuận của Sàn (Admin) từ Stripe về ngân hàng
DROP TABLE IF EXISTS platform_withdrawals CASCADE;
CREATE TABLE platform_withdrawals (
	withdrawal_id SERIAL PRIMARY KEY,
	manager_id INT REFERENCES managers(manager_id) ON DELETE SET NULL,
	amount NUMERIC(10,2) NOT NULL,           -- Số tiền rút (USD)
	currency VARCHAR(10) DEFAULT 'usd',
	stripe_payout_id VARCHAR(255),           -- Mã Payout trên Stripe (po_xxx)
	status VARCHAR(20) NOT NULL DEFAULT 'pending'
		CHECK (status IN ('pending', 'in_transit', 'paid', 'failed', 'canceled')),
	description TEXT,                         -- Ghi chú
	created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	arrived_at TIMESTAMP                     -- Thời điểm tiền về ngân hàng
);

-- ==============================================================================
-- 5. NHÓM GIAO TIẾP & HỖ TRỢ (Communication & Reports)
-- ==============================================================================

CREATE TABLE chats (
    chat_id SERIAL PRIMARY KEY,
    chat_name VARCHAR(255),
    chat_type VARCHAR(50) DEFAULT 'private',
    context_type VARCHAR(50),
    context_id INT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_message_at TIMESTAMP
);

CREATE TABLE chat_participants (
    chat_id INT REFERENCES chats(chat_id) ON DELETE CASCADE,
    account_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    role VARCHAR(50) DEFAULT 'member',
    unread_count INT DEFAULT 0,
    last_read_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    joined_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    cleared_at TIMESTAMP,
    PRIMARY KEY (chat_id, account_id)
);

CREATE TABLE messages (
    message_id SERIAL PRIMARY KEY,
    chat_id INT REFERENCES chats(chat_id) ON DELETE CASCADE,
    sender_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    content TEXT NOT NULL,
    is_seen BOOLEAN DEFAULT FALSE,
    message_status VARCHAR(50) DEFAULT 'ok',
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    received_at TIMESTAMP
);

CREATE TABLE message_attachments (
    attachment_id SERIAL PRIMARY KEY,
    message_id INT REFERENCES messages(message_id) ON DELETE CASCADE,
    file_url TEXT NOT NULL,
    file_name VARCHAR(255),
    file_type VARCHAR(50),
    file_size BIGINT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE notifications (
    notification_id SERIAL PRIMARY KEY,
    sender_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    receiver_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    title VARCHAR(255),
    content TEXT,
    link_action TEXT,
    is_read BOOLEAN DEFAULT FALSE,
    is_removed BOOLEAN DEFAULT FALSE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_reports (
    course_report_id SERIAL PRIMARY KEY,
    reporter_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
    resolver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    reason VARCHAR(255),
    description TEXT,
    course_reports_status VARCHAR(50),
    resolution_note TEXT,
    resolved_at TIMESTAMP,
    access_granted_until TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_review_reports (
    course_review_report_id SERIAL PRIMARY KEY,
    reporter_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    course_review_id INT REFERENCES course_reviews(course_review_id) ON DELETE SET NULL,
    resolver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    reason VARCHAR(255),
    description TEXT,
    user_reports_status VARCHAR(50),
    resolution_note TEXT,
    resolved_at TIMESTAMP,
    access_granted_until TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE lesson_review_reports (
    lesson_review_report_id SERIAL PRIMARY KEY,
    reporter_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    lesson_review_id INT REFERENCES lesson_reviews(lesson_review_id) ON DELETE SET NULL,
    resolver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    reason VARCHAR(255),
    description TEXT,
    user_reports_status VARCHAR(50),
    resolution_note TEXT,
    resolved_at TIMESTAMP,
    access_granted_until TIMESTAMP,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE audit_logs (
    log_id SERIAL PRIMARY KEY,
    actor_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    action_type VARCHAR(100) NOT NULL, -- 'join_room', 'monitor_room', 'broadcast', 'delete_message'
    target_type VARCHAR(100), -- 'chat_room', 'message', 'user'
    target_id INT,
    details TEXT,
    ip_address VARCHAR(45),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 6. NHÓM HỆ THỐNG & TRÍ TUỆ NHÂN TẠO (System & AI Integration)
-- ==============================================================================

CREATE TABLE system_configs (
    config_id SERIAL PRIMARY KEY,
    manager_id INT REFERENCES managers(manager_id) ON DELETE SET NULL,
    config_key VARCHAR(255) UNIQUE NOT NULL,
    config_value TEXT,
    description TEXT,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE ai_models (
    model_id SERIAL PRIMARY KEY,
    model_name VARCHAR(255) NOT NULL,
    model_type VARCHAR(50),
    model_provider VARCHAR(50),
    model_version VARCHAR(50),
    model_status VARCHAR(50),
    description TEXT,
    model_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    model_updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
	model_path VARCHAR(255),
	process_type VARCHAR(255)
);

CREATE TABLE courses_ai_integrations (
    id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(model_id) ON DELETE SET NULL,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
    UNIQUE(model_id, course_id),
    role VARCHAR(50),
    is_enabled BOOLEAN NOT NULL DEFAULT TRUE,
    config_json JSONB,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_ai_usage_logs (
    log_id SERIAL PRIMARY KEY,
    integration_id INT REFERENCES courses_ai_integrations(id) ON DELETE SET NULL,
    interaction_type VARCHAR(50),
    input_json JSONB,
    output_json JSONB,
    latency_ms REAL,
    token_usage REAL,
    error_message TEXT,
    log_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE message_moderation_logs (
    log_id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(model_id) ON DELETE SET NULL,
    message_id INT REFERENCES messages(message_id) ON DELETE SET NULL,
    input_json JSONB,
    output_json JSONB,
    latency_ms REAL,
    error_message TEXT,
    log_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_review_moderation_logs (
    log_id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(model_id) ON DELETE SET NULL,
    course_review_id INT REFERENCES course_reviews(course_review_id) ON DELETE SET NULL,
    input_json JSONB,
    output_json JSONB,
    latency_ms REAL,
    error_message TEXT,
    log_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE lesson_review_moderation_logs (
    log_id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(model_id) ON DELETE SET NULL,
    lesson_review_id INT REFERENCES lesson_reviews(lesson_review_id) ON DELETE SET NULL,
    input_json JSONB,
    output_json JSONB,
    latency_ms REAL,
    error_message TEXT,
    log_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 7. VIEWS FOR DATA CONSISTENCY (The "Utmost Normalized" Part)
-- ==============================================================================

CREATE OR REPLACE VIEW view_lesson_stats AS
SELECT 
    l.lesson_id,
    l.course_id,
    COUNT(lm.material_id) AS material_count,
    COALESCE(SUM(
        (lm.material_metadata->>'duration')::INT
    ), 0) AS lesson_duration
FROM lessons l
LEFT JOIN learning_materials lm ON lm.lesson_id = l.lesson_id
GROUP BY l.lesson_id, l.course_id;

CREATE OR REPLACE VIEW view_course_stats AS
SELECT 
    c.course_id,
    COALESCE(AVG(cr.rating), 0) AS rating_average,
    COUNT(DISTINCT e.enrollment_id) AS total_students,
    COUNT(DISTINCT cr.course_review_id) AS total_reviews,
    COALESCE(cs.total_lessons, 0) AS total_lessons,
    COALESCE(cs.total_materials, 0) AS total_materials,
    COALESCE(cs.total_duration, 0) AS total_duration
FROM courses c
LEFT JOIN enrollments e ON e.course_id = c.course_id
LEFT JOIN course_reviews cr ON cr.enrollment_id = e.enrollment_id AND cr.is_removed = FALSE
LEFT JOIN (
    SELECT 
        course_id,
        COUNT(lesson_id) AS total_lessons,
        SUM(material_count) AS total_materials,
        SUM(lesson_duration) AS total_duration
    FROM view_lesson_stats
    GROUP BY course_id
) cs ON cs.course_id = c.course_id
GROUP BY c.course_id, cs.total_lessons, cs.total_materials, cs.total_duration;

CREATE OR REPLACE VIEW view_user_stats AS
SELECT 
    u.user_id,
    COUNT(DISTINCT e.enrollment_id) AS enrolled_courses_count,
    COALESCE(SUM(oi.purchase_price), 0) AS total_spent
FROM users u
LEFT JOIN enrollments e ON e.user_id = u.user_id
LEFT JOIN order_info o ON o.user_id = u.user_id AND o.order_status = 'paid'
LEFT JOIN order_items oi ON oi.order_id = o.order_id
GROUP BY u.user_id;

CREATE OR REPLACE VIEW view_order_stats AS
SELECT 
    o.order_id,
    o.user_id,
    COALESCE(SUM(oi.purchase_price), 0) AS total_amount
FROM order_info o
LEFT JOIN order_items oi ON oi.order_id = o.order_id
GROUP BY o.order_id, o.user_id;

CREATE OR REPLACE VIEW view_instructor_stats AS
SELECT 
    i.instructor_id,
    COALESCE(AVG(cr.rating), 0) AS instructor_rating,
    COALESCE(SUM(ip.payout_amount), 0) AS total_revenue,
    COUNT(DISTINCT e.enrollment_id) AS total_students_count
FROM instructors i
LEFT JOIN courses c ON c.instructor_id = i.instructor_id
LEFT JOIN enrollments e ON e.course_id = c.course_id
LEFT JOIN course_reviews cr ON cr.enrollment_id = e.enrollment_id AND cr.is_removed = FALSE
LEFT JOIN instructor_payouts ip ON ip.instructor_id = i.instructor_id
GROUP BY i.instructor_id;

CREATE TABLE course_field_moderation_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    course_id INT NOT NULL REFERENCES courses(course_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE course_ai_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    course_id INT NOT NULL REFERENCES courses(course_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
    moderation_status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE lesson_ai_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    lesson_id INT NOT NULL REFERENCES lessons(lesson_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
    moderation_status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE learning_material_ai_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    material_id INT NOT NULL REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
    moderation_status VARCHAR(50) NOT NULL DEFAULT 'PENDING',
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexing
CREATE INDEX idx_course_reviews_enrollment ON course_reviews(enrollment_id);
CREATE INDEX idx_lesson_reviews_enrollment ON lesson_reviews(enrollment_id);
CREATE INDEX idx_lesson_reviews_lesson ON lesson_reviews(lesson_id);
CREATE INDEX idx_enrollments_course ON enrollments(course_id);
CREATE INDEX idx_enrollments_user ON enrollments(user_id);
CREATE INDEX idx_courses_instructor ON courses(instructor_id);
CREATE INDEX idx_lessons_course ON lessons(course_id);
CREATE INDEX idx_materials_lesson ON learning_materials(lesson_id);
CREATE INDEX idx_order_info_user ON order_info(user_id);
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_course_reviews_active ON course_reviews(enrollment_id) WHERE is_removed = FALSE;
CREATE INDEX idx_order_paid ON order_info(user_id) WHERE order_status = 'paid';
CREATE INDEX idx_material_duration ON learning_materials (((material_metadata->>'duration')::INT));
CREATE INDEX idx_metadata_gin ON learning_materials USING GIN (material_metadata);
CREATE INDEX idx_audit_logs_actor ON audit_logs(actor_id);
CREATE INDEX idx_chat_participants_read ON chat_participants(account_id, last_read_at);

-- ==============================================================================
-- 8. SAMPLE DATA (EXCLUDING ACCOUNTS)
-- ==============================================================================



INSERT INTO categories (category_id, categories_name, description, category_status) 
VALUES 
(1, 'Design', 'Courses related to graphic design, UX/UI, 3D modeling, and creative arts.', 'active'), 
(2, 'Software Development', 'Software development, programming languages, web development, and mobile app creation.', 'active'), 
(3, 'Business', 'Business management, leadership, strategy, finance, and entrepreneurship.', 'active'),
(4, 'Marketing', 'Digital marketing, SEO, social media advertising, and content strategy.', 'active'),
(5, 'Photography & Video', 'Photography, video editing, cinematography, and digital imaging.', 'active'),
(6, 'Music', 'Music theory, instrument playing, audio production, and songwriting.', 'active'),
(7, 'Languages', 'Learn English, Japanese, Chinese, Spanish, and other languages.', 'active'),
(8, 'Health & Fitness', 'Fitness, nutrition, yoga, meditation, and personal well-being.', 'active'),
(9, 'Data Science & AI Engineering', 'Data science, machine learning, deep learning, and artificial intelligence.', 'active'),
(10, 'Personal Development', 'Public speaking, career development, memory improvement, and productivity.', 'active'),
(11, 'Finance & Investing', 'Personal finance, stock market investing, trading, and cryptocurrency.', 'active'),
(12, 'Office Productivity', 'Microsoft Excel, PowerPoint, Google Workspace, and office tools.', 'active'),
(13, 'Lifestyle', 'Cooking, baking, gaming, home improvement, and creative hobbies.', 'active')
ON CONFLICT (category_id) DO UPDATE SET categories_name = EXCLUDED.categories_name, description = EXCLUDED.description;

-- ==============================================================================
-- 9. SAMPLE DATA FOR PRIMARY ACCOUNT (phuoctai228)
-- ==============================================================================

INSERT INTO accounts (account_id, username, email, password_hash, account_status, auth_provider, is_verified)
VALUES (1,'instructor', 'instructor@gmail.com', '$2a$11$O7PrVmv/I5yxkexhkdrY2OB2tQf5c6Gy9P8hvqLIAF2NO34wt9C3i', 'active', 'local', TRUE)
ON CONFLICT (account_id) DO NOTHING;

INSERT INTO users (user_id, full_name)
VALUES (1, 'instructor')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO instructors (instructor_id)
VALUES (1)
ON CONFLICT (instructor_id) DO NOTHING;

-- ==============================================================================
-- 9B. SEED SYSTEM CONFIGS
-- ==============================================================================
INSERT INTO system_configs (config_key, config_value, description)
VALUES ('TransferRate', '80', 'Phần trăm (%) giảng viên nhận được từ mỗi giao dịch. VD: 80 = GV nhận 80%, Sàn giữ 20%.')
ON CONFLICT (config_key) DO UPDATE SET config_value = EXCLUDED.config_value, description = EXCLUDED.description;

INSERT INTO system_configs (config_key, config_value, description)
VALUES ('PayoutDay', '15', 'Ngày trong tháng thực hiện chia tiền cho giảng viên. VD: 15 = ngày 15 hàng tháng.')
ON CONFLICT (config_key) DO UPDATE SET config_value = EXCLUDED.config_value, description = EXCLUDED.description;

INSERT INTO system_configs (config_key, config_value, description)
VALUES ('StripeCountries', 
'[
    {"code":"US","name":"United States"},{"code":"GB","name":"United Kingdom"}
]', 'Danh sách quốc gia mà Stripe Connect hỗ trợ đăng ký tài khoản Express. Giảng viên chọn 1 trong số này khi đăng ký Stripe.')
ON CONFLICT (config_key) DO UPDATE SET config_value = EXCLUDED.config_value, description = EXCLUDED.description;

-- ==============================================================================
-- 10. SAMPLE DATA FOR COURSES, LESSONS, MATERIALS
-- ==============================================================================



-- ==============================================================================
-- 11. SYNC SEQUENCES (Prevent duplicate key errors)
-- ==============================================================================

SELECT setval(pg_get_serial_sequence('accounts', 'account_id'), (SELECT COALESCE(MAX(account_id), 1) FROM accounts));
SELECT setval(pg_get_serial_sequence('categories', 'category_id'), (SELECT COALESCE(MAX(category_id), 1) FROM categories));
SELECT setval(pg_get_serial_sequence('courses', 'course_id'), (SELECT COALESCE(MAX(course_id), 1) FROM courses));
SELECT setval(pg_get_serial_sequence('lessons', 'lesson_id'), (SELECT COALESCE(MAX(lesson_id), 1) FROM lessons));
SELECT setval(pg_get_serial_sequence('learning_materials', 'material_id'), (SELECT COALESCE(MAX(material_id), 1) FROM learning_materials));
SELECT setval(pg_get_serial_sequence('chats', 'chat_id'), (SELECT COALESCE(MAX(chat_id), 1) FROM chats));
SELECT setval(pg_get_serial_sequence('messages', 'message_id'), (SELECT COALESCE(MAX(message_id), 1) FROM messages));
SELECT setval(pg_get_serial_sequence('material_completions', 'id'), (SELECT COALESCE(MAX(id), 1) FROM material_completions));
SELECT setval(pg_get_serial_sequence('gifts', 'gift_id'), (SELECT COALESCE(MAX(gift_id), 1) FROM gifts));
SELECT setval(pg_get_serial_sequence('quizzes', 'quiz_id'), (SELECT COALESCE(MAX(quiz_id), 1) FROM quizzes));
SELECT setval(pg_get_serial_sequence('quiz_questions', 'question_id'), (SELECT COALESCE(MAX(question_id), 1) FROM quiz_questions));
SELECT setval(pg_get_serial_sequence('quiz_options', 'option_id'), (SELECT COALESCE(MAX(option_id), 1) FROM quiz_options));
SELECT setval(pg_get_serial_sequence('course_quizzes', 'course_quiz_id'), (SELECT COALESCE(MAX(course_quiz_id), 1) FROM course_quizzes));
SELECT setval(pg_get_serial_sequence('quiz_attempts', 'attempt_id'), (SELECT COALESCE(MAX(attempt_id), 1) FROM quiz_attempts));
SELECT setval(pg_get_serial_sequence('quiz_attempt_answers', 'answer_id'), (SELECT COALESCE(MAX(answer_id), 1) FROM quiz_attempt_answers));

DO $$
DECLARE
    new_account_id INT;
BEGIN
    -- 1. Tạo account Admin
    INSERT INTO accounts (
        email, username, password_hash, phone_number, account_status, 
        auth_provider, is_verified, account_created_at, account_updated_at
    ) VALUES (
        'admin@gmail.com',
        'admin',
        '$2a$11$O7PrVmv/I5yxkexhkdrY2OB2tQf5c6Gy9P8hvqLIAF2NO34wt9C3i',
        '+84123456789',
        'active',
        'local',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING account_id INTO new_account_id;

    -- Tạo manager (Admin)
    INSERT INTO managers (manager_id, role, display_name)
    VALUES (new_account_id, 'admin', 'Super Administrator');

    -- 2. Tạo account Staff
    INSERT INTO accounts (
        email, username, password_hash, phone_number, account_status, 
        auth_provider, is_verified, account_created_at, account_updated_at
    ) VALUES (
        'staff@gmail.com',
        'staff',
        '$2a$11$O7PrVmv/I5yxkexhkdrY2OB2tQf5c6Gy9P8hvqLIAF2NO34wt9C3i',
        '+84987654321',
        'active',
        'local',
        TRUE,
        CURRENT_TIMESTAMP,
        CURRENT_TIMESTAMP
    )
    RETURNING account_id INTO new_account_id;

    -- Tạo manager (Staff)
    INSERT INTO managers (manager_id, role, display_name)
    VALUES (new_account_id, 'staff', 'Hỗ trợ kỹ thuật');

    RAISE NOTICE 'Seeding Admin, Staff & Avatar Frames hoàn tất!';
END $$;




INSERT INTO 
ai_models (model_name,model_type,model_provider,model_version, model_path, model_status,description, process_type)
VALUES
('harmful_text_classifier','classifier','local','1.0.0','ki4n-4nt/spam_text_classifier,ki4n-4nt/toxic_text_classifier','active','an ensemble of spam and toxic text classifier that was fine-tuned from distilbert multilingual cased','text'),
('clip','embedding_generator','openai','1.0.0','openai/clip-vit-base-patch32','active','a multimodal model that was used to generate embeddings','media'),
('distilbert','embedding_generator','hugging_face','1.0.0','distilbert-base-multilingual-cased','active','a language model that was used to generate embeddings','text'),
('paraphrase-multilingual-MiniLM-L12-v2','embedding_generator','hugging_face','1.0.0','sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2','active','a multilingual language model that was used to generate embeddings','text');

INSERT INTO
system_configs(config_key,config_value,description)
VALUES
('course_harmful_text_classifier','ki4n-4nt/spam_text_classifier,ki4n-4nt/toxic_text_classifier','system config of course_harmful_text_classifier'),
('course_text_embedding_generator', 'distilbert-base-multilingual-cased','system config of course_text_embedding_generator'),
('course_media_embedding_generator','openai/clip-vit-base-patch32','system config of course_media_embedding_generator'),
('review_harmful_text_classifier','ki4n-4nt/spam_text_classifier,ki4n-4nt/toxic_text_classifier','system config of review_harmful_text_classifier');

INSERT INTO
system_configs(config_key,config_value,description)
VALUES
('moderation_threshold',
'{"similarity": 0.85,"spam": 0.85,"toxic": 0.85}',
'system config of AI moderation threshold');

-- =====================================================================
-- SEED COURSES, LESSONS, MATERIALS, QUESTION BANK
-- =====================================================================
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (4, 1, 7, NULL, 'Japanese Language Lessons', '<p>Learn Japanese with Japan Society. This course provides a structured introduction to the Japanese language, covering everything from basic greetings to essential vocabulary and grammar structures.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849104/whstculaiyquwybyan3k.jpg', '2026-06-23 11:31:39.40256', '2026-08-16 11:22:27.555421', 'published', 0, '<p>- Basic Japanese greetings and introductions.</p><p>- How to count and use numbers in Japanese.</p><p>- Vocabulary for days of the week and days of the month.</p><p>- How to express going to a destination.</p><p>- Conjugating and using essential verbs (drinking, eating, seeing, listening, doing).</p><p>- Building a strong foundation for conversational Japanese.</p><p><br></p>', '<p>- No prior knowledge of Japanese is required.</p><p>- An interest in learning the Japanese language and culture.</p><p>- A willingness to practice pronunciation and vocabulary.</p><p><br></p>', NULL, '2026-08-16 11:22:27.555422', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (8, 1, 2, NULL, 'Flutter Crash Course', '<p>Learn how to create Flutter apps from scratch with Net Ninja. This crash course provides a complete introduction to the Flutter framework, teaching you how to build beautiful, natively compiled, multi-platform applications from a single codebase.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849088/jmkwcxaavsw71yb1luxp.png', '2026-06-23 11:56:52.228023', '2026-08-16 11:22:25.435736', 'published', 0, '<p>- Understand what Flutter is and its core architecture.</p><p>- Set up your development environment on both Windows and Mac.</p><p>- Create and configure a new Flutter project from scratch.</p><p>- Navigate the file structure and overview of a Flutter project.</p><p>- Master the use of basic Flutter widgets to build beautiful UIs.</p><p>- Understand and implement foundational widgets like MaterialApp and Scaffold.</p><p><br></p>', '<p>- Basic understanding of object-oriented programming concepts.</p><p>- No prior experience with Flutter or Dart is required.</p><p>- A computer running Windows, macOS, or Linux.</p><p><br></p>', NULL, '2026-08-16 11:22:25.435739', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (12, 1, 1, NULL, 'Figma Design for beginners', '<p>Welcome to the Figma Design for beginners course! This course will walk you through the entire process of creating a website design for a personal portfolio website. We''ll start by teaching you the fundamental concepts and features that Figma Design offers, and then we''ll go on a creative journey together to make a website that you can customize to make your own using some of Figma''s most exciting features.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849074/jbfijhmtfy1fbscuqj3a.png', '2026-06-23 15:57:24.102492', '2026-08-16 11:22:23.435468', 'published', 0, '<p>- Set up a new Figma account and navigate the interface confidently.</p><p>- Understand how to organize and manage Figma design files.</p><p>- Master fundamental Figma design tools, features, and concepts.</p><p>- Step-by-step design of a complete landing page hero section.</p><p>- Create detailed and professional case study pages for your portfolio.</p><p>- Best practices for portfolio personalization to stand out.</p><p><br></p>', '<p>- A computer or laptop with an internet connection.</p><p>- No prior design experience or Figma knowledge is required.</p><p>- A willingness to learn and experiment creatively.</p><p><br></p>', NULL, '2026-08-16 11:22:23.43547', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (2, 1, 9, NULL, 'Essence of linear algebra', '<p>Learn the core concept of linear algebra with a visuals-first approach. This course will take you step-by-step through the geometric intuition behind vectors, matrices, and linear transformations.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849119/vhdn3ozypmlt9mva0z2j.png', '2026-06-23 11:23:19.848023', '2026-08-16 11:22:29.774713', 'published', 0, '<p>- Understand vectors, linear combinations, and spans.</p><p>- Visualize linear transformations and matrices in 2D and 3D space.</p><p>- Compute and intuitively grasp the determinant.</p><p>- Master inverse matrices, column spaces, and null spaces.</p><p><br></p>', '<p>- Basic high school mathematics.</p><p>- No advanced calculus or prior linear algebra experience required.</p><p><br></p>', NULL, '2026-08-16 11:22:29.774715', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (6, 1, 12, NULL, 'Hướng Dẫn Thực Hành Excel Cơ Bản', '<p>Khóa học hướng dẫn thực hành các kỹ năng Excel cơ bản từ Trung Tâm Tin Học Sao Việt. Khóa học này sẽ giúp bạn làm quen với giao diện bảng tính và thành thạo các hàm tính toán, công cụ xử lý dữ liệu thông dụng nhất phục vụ cho công việc văn phòng hàng ngày.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849056/i7zmkj7o6nkdepzbhhlb.webp', '2026-06-23 11:51:06.571487', '2026-08-16 11:22:20.495753', 'published', 0, '<p>- Nắm vững cách sử dụng các hàm điều kiện cơ bản như hàm IF.</p><p>- Thành thạo các hàm xử lý thời gian và xử lý số liệu đơn giản.</p><p>- Biết cách sử dụng công cụ tìm kiếm và thay thế dữ liệu một cách hiệu quả.</p><p>- Nắm rõ bộ hàm đếm (COUNT, COUNTA, COUNTBLANK) để thống kê dữ liệu.</p><p>- Kỹ năng lọc và sắp xếp dữ liệu cơ bản để quản lý bảng tính chuyên nghiệp.</p><p><br></p>', '<p>- Máy tính có cài đặt phần mềm Microsoft Excel.</p><p>- Không yêu cầu kinh nghiệm sử dụng Excel trước đó.</p><p>- Phù hợp cho người mới bắt đầu, sinh viên và nhân viên văn phòng.</p><p><br></p>', NULL, '2026-08-16 11:22:20.495754', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (3, 1, 7, NULL, 'Học Tiếng Anh Cho Người Mới Hoặc Mất Gốc | Bắt Đầu Từ Căn Bản', '<p>Bạn đang muốn học lại tiếng Anh từ đầu nhưng không biết bắt đầu từ đâu? Chuỗi video "Học Tiếng Anh Cho Người Mới Hoặc Mất Gốc" sẽ giúp bạn xây dựng nền tảng vững chắc, từ những kiến thức cơ bản nhất đến việc tự tin giao tiếp thực tế.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849151/htsv67gufv8w4dwlfavw.jpg', '2026-06-23 11:29:29.616376', '2026-08-16 11:22:33.634013', 'published', 0, '<p>- Xây dựng nền tảng tiếng Anh vững chắc từ con số 0.</p><p>- Nắm vững từ vựng và cụm từ thông dụng theo chủ đề hàng ngày.</p><p>- Sử dụng thành thạo các cấu trúc và mẫu câu giao tiếp cơ bản.</p><p>- Tự tin chào hỏi, giới thiệu bản thân và đàm thoại trong thực tế.</p><p>- Cải thiện khả năng nghe và phát âm thông qua phương pháp lặp lại.</p><p><br></p>', '<p>- Không yêu cầu kiến thức nền tảng về tiếng Anh.</p><p>- Phù hợp cho người mới bắt đầu hoàn toàn hoặc người đã mất gốc tiếng Anh.</p><p>- Chỉ cần một thiết bị có kết nối internet và tinh thần sẵn sàng học hỏi.</p><p><br></p>', NULL, '2026-08-16 11:22:33.634015', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (1, 1, 2, NULL, 'Full Stack Web Development Tutorial Course', '<p>A comprehensive guide to becoming a full stack web developer from scratch. You will learn frontend and backend technologies.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849166/elq4tnaoiiv0i58svheg.png', '2026-06-23 10:32:29.720532', '2026-08-16 11:22:09.397538', 'published', 0, '<p>HTML, CSS, JavaScript, React, Node.js, Express, PostgreSQL</p><p><br></p>', '<p>No prior programming experience required. Basic computer knowledge is sufficient.</p><p><br></p>', NULL, '2026-08-16 11:22:09.39754', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (5, 1, 9, NULL, 'Neural Networks and Deep Learning', '<p>Join Andrew Ng to explore the foundational concepts of neural networks and deep learning. Discover how AI is becoming the "new electricity" and learn how deep learning models are constructed, trained, and applied to real-world problems in this first course of the Deep Learning Specialization.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849000/di5npruhtx83nvqzkb1p.png', '2026-06-23 11:47:32.237014', '2026-08-16 11:22:13.696774', 'draft', 0, '<p>- Understand the major trends driving the rise of deep learning.</p><p>- Build, train, and apply fully connected deep neural networks.</p><p>- Learn how to implement efficient vectorized neural networks.</p><p>- Understand the key parameters and architecture of neural networks.</p><p>- Prepare yourself for more advanced topics in the Deep Learning Specialization.</p><p><br></p>', '<p>- Basic programming skills in Python.</p><p>- Understanding of basic linear algebra and machine learning concepts.</p><p>- Familiarity with basic calculus.</p><p><br></p>', NULL, '2026-08-16 11:22:13.696776', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (9, 1, 2, NULL, 'Design Patterns', '<p>A comprehensive guide to software design patterns by Geekific. In this course, you will learn the core principles of object-oriented design and how to implement popular creational, structural, and behavioral design patterns in Java. Mastering these patterns will help you write cleaner, more maintainable, and scalable code.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849020/nxx8fvmf5ovwvahpucww.jpg', '2026-06-23 11:58:37.213522', '2026-08-16 11:22:15.544138', 'published', 0, '<p>- Understand what design patterns are and why they are essential in software engineering.</p><p>- Learn and implement Creational Patterns including Singleton, Factory Method, Abstract Factory, Builder, and Prototype.</p><p>- Explore Behavioral Patterns such as the Chain of Responsibility.</p><p>- Master core object-oriented design principles and best practices.</p><p>- Improve your ability to architect scalable and robust software systems in Java.</p><p><br></p>', '<p>- Basic to intermediate knowledge of Java programming.</p><p>- Solid understanding of Object-Oriented Programming (OOP) concepts such as classes, interfaces, inheritance, and polymorphism.</p><p><br></p>', NULL, '2026-08-16 11:22:15.54414', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (7, 1, 2, NULL, 'Docker Crash Course Tutorial', '<p>A complete crash course on Docker for beginners by Net Ninja. In this course, you will learn how to containerize your applications, work with images, and manage containers effectively. By the end, you''ll have a solid foundation for deploying modern applications using Docker.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849036/xnbjz4iwgavcenh6cosk.png', '2026-06-23 11:55:15.556299', '2026-08-16 11:22:18.253455', 'published', 0, '<p>- Understand what Docker is, how it works, and why it is useful for developers.</p><p>- Install Docker and set up your local development environment.</p><p>- Master the core concepts of Docker Images and Containers.</p><p>- Learn how to pull images from Docker Hub and use parent images.</p><p>- Write your own Dockerfile to create custom, optimized images.</p><p>- Use .dockerignore files to streamline your build process.</p><p>- Start, stop, and manage the complete lifecycle of your containers.</p><p><br></p>', '<p>- Basic familiarity with using a command-line interface (terminal or command prompt).</p><p>- General knowledge of web development or software development concepts is helpful but not strictly required.</p><p><br></p>', NULL, '2026-08-16 11:22:18.253458', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (10, 1, 5, NULL, 'Khóa Học Edit Video Capcut Máy Tính Từ A-Z', '<p>Khóa học hướng dẫn edit video trên phần mềm Capcut máy tính (PC) từ cơ bản đến nâng cao bởi VA Media. Khóa học này được thiết kế chi tiết, từng bước giúp người mới bắt đầu dễ dàng làm quen với giao diện, công cụ và nhanh chóng thành thạo kỹ năng chỉnh sửa video chuyên nghiệp cho các nền tảng mạng xã hội.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786848951/iswfmwunwljbj9k0r4uu.png', '2026-06-23 12:06:33.216014', '2026-08-16 11:22:11.644165', 'published', 0, '<p>- Nắm vững các bước tải, cài đặt và làm quen với giao diện làm việc của Capcut PC.</p><p>- Biết cách tìm kiếm và khai thác các nguồn tài nguyên edit video miễn phí.</p><p>- Nắm rõ các tính năng và cách sử dụng toàn bộ công cụ chỉnh sửa cơ bản.</p><p>- Kỹ năng import dữ liệu, cắt ghép, thêm hiệu ứng, chuyển cảnh và âm thanh vào video.</p><p>- Thiết lập định dạng và xuất khung hình chuẩn cho các nền tảng TikTok, YouTube, Facebook.</p><p><br></p>', '<p>- Máy tính PC hoặc Laptop (Windows/MacOS) có cấu hình cơ bản đáp ứng được phần mềm Capcut.</p><p>- Cần có kết nối internet để tải tài nguyên và phần mềm.</p><p>- Đam mê sáng tạo video, không yêu cầu bất kỳ kinh nghiệm chỉnh sửa video nào trước đó.</p><p><br></p>', NULL, '2026-08-16 11:22:11.644172', false, 2);
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES (11, 1, 12, NULL, 'Tin Học Văn Phòng cho Người mới bắt đầu', '<p>Khóa học Tin Học Văn Phòng dành cho người mới bắt đầu. Khóa học này giúp các bạn mới làm quen với máy tính có thể tự học và nắm vững các kỹ năng cơ bản của Microsoft Word, từ việc tìm hiểu các phiên bản đến cách soạn thảo, định dạng và in ấn văn bản chuyên nghiệp.</p><p><br></p>', 0.00, 'https://res.cloudinary.com/dndah3xuz/image/upload/v1786849134/durs8omnnf6wniqsxw69.png', '2026-06-23 15:52:15.247772', '2026-08-16 11:22:31.814603', 'published', 0, '<p>- Nắm vững kiến thức tổng quan về các phiên bản Word thường dùng hiện nay.</p><p>- Biết cách thiết lập bộ gõ và khắc phục các lỗi cơ bản như không gõ được tiếng Việt.</p><p>- Kỹ năng định dạng văn bản chuẩn xác theo đúng quy định hành chính.</p><p>- Thực hiện các thao tác căn chỉnh: chia cột, tạo chữ cái lớn đầu dòng (Drop Cap), và đánh số thứ tự động.</p><p>- Nắm rõ cách thiết lập trang, căn lề và in ấn văn bản Word một cách hoàn chỉnh.</p><p><br></p>', '<p>- Máy tính có cài đặt phần mềm Microsoft Word (các phiên bản từ 2007 đến 2021).</p><p>- Không yêu cầu kiến thức tin học trước đó, hoàn toàn phù hợp cho người mất gốc hoặc mới làm quen với máy tính.</p><p>- Tinh thần học hỏi và thực hành trực tiếp theo hướng dẫn.</p><p><br></p>', NULL, '2026-08-16 11:22:31.814605', false, 2);

INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (10, '70678e0d00ecf9e02a0f4e62e8e8a4ae', '1cd864f8fffb86435b3b86f8ba00ac94', '00a431bde80917978cba7b3e5dcd5c0f', '754095b98db4ef84956f838abfe71662', 'f6b00f054bd9cbdadfa2b4507ca806a8');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (5, '57a5aafac0375c2a1c54a43ee3edfed2', '3eedd7374a5f127cfc0c459958b1957c', '8bc3ea2c31188bcfbf73a5a06c9b0d07', '3f65f940d2a0ac855c7d72e501bf0f6d', '9da01b4af17b7019667aae326f4fdf88');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (9, '66c3370bf227f8b017a619de7f5897fa', 'f2c519db84e1e0cef9e1c26670dcf24f', '4fb570081859c5f3c6863e0ba7bc1c6f', 'b8952f2dc2bf2255adb71ed2efa04638', '4d767985e7e3581996e35d634daad9ac');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (7, 'df5d75787a88abd633d16e82293e6f06', '2a7867b31576a486f318e08be1c94cbb', '6a87885d1bd7c7501a593b054d7880b1', 'e0cce0f5ada34b180631bc1ca3c5ffb0', '9ab644e6d8c6de9dd24d94b7f016d870');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (6, 'c4f8341af490e44ec524e94c3dd90ba3', '2aa2f0d2148034bcdf53b888dd1b19b0', '06996b6f2ef51feb8d1c6061eceffd8e', 'ca04f3bd14300b5298ccc4532a141671', '53b32771721f3715110e02d0aac5cff4');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (12, 'd537620fa0ba3154679d41ca9f0122f4', 'aa73a83b37256ec7ca7a834b87438f7e', '63065cf2038891f193d3528f8ff9027d', '5354fbf6280be6736bff899f8ea49222', '5ff5d47a5fd0e27d7d7734c8595e4c84');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (8, '073244be65750fa353fee5d97ce8c9ad', '7a13155ca5793e8f0fb17e5ea1aca1ca', '399e809b4105e01eea8df2a2c5aba4e6', '2f2df5fac3a8e4868336846b4a30a185', 'dfe33ccfbaad137d8e811190101497f9');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (4, '9dfe1385ddcdca561d8d10957fa86204', 'cd5dde97cedcb14ac8daeda3694937ce', '8ebe31fcf468b45269b494846a916adb', '6f5b2cc45cee660d57bd1a227321bcc5', '14cedc70737b127cdba9408512ee026c');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (2, '491afb7d79231b84459cc1ebbbb06b8a', 'f23422fd5d6631e5fb2ac0e140ad14a4', 'edbdba91e836b04cd3a595795e03501c', 'e59502e9b79d867095a5cb2f3829f75d', '0a4957fc6aeb843c5c6153919b1d833b');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (11, '50924db43392d54f5d727924c7d86ade', '45d2e3ef5d1085e0c143faed60e00c0e', 'b0c63e18f4f13a2f161b91dc08caf964', '174ad99daea0149ee0ade40f7fc821b1', '8770cb08e781b29cf7c626864cdd2b61');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (3, 'dadee14f02fff3f1fef453067a1ffe39', 'e9de14e4957252c0e8364b146d3d850d', 'a716ce9ed46dffb664a842b260cd805f', '226bfcda0ba8d9055b38c1932423beea', '380bf3ade39129fa7ccaffe8bd40e1d7');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES (1, '1db7eb9301f104b6b430c1ab3c3a532a', '7d064f7a830646276700619ab8d8e707', 'c002c7cf6bce5cafdfc908df971f2599', 'b71fe5e2a6e0b275faf962ed926b7475', 'b1ffd825a99f062e0770b970e102378b');

INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (2, 5, 'Welcome', NULL, NULL, '2026-06-23 13:19:00.815238', '2026-06-23 13:21:26.107636', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (3, 10, 'Giới thiệu khóa học', NULL, NULL, '2026-06-23 13:25:52.745504', '2026-06-23 15:12:16.192', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (4, 9, 'Introduction to Design Pattern', NULL, NULL, '2026-06-23 15:13:02.712119', '2026-06-23 15:13:02.712121', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (5, 8, 'What is Flutter?', NULL, NULL, '2026-06-23 15:14:49.691568', '2026-06-23 15:14:49.691569', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (6, 7, 'What is Docker?', NULL, NULL, '2026-06-23 15:16:19.095683', '2026-06-23 15:16:19.095685', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (7, 6, 'Cơ Bản về hàm IF', NULL, NULL, '2026-06-23 15:17:58.810144', '2026-06-23 15:19:12.173049', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (8, 4, 'Introduction', NULL, NULL, '2026-06-23 15:31:33.776224', '2026-06-23 15:31:33.776252', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (9, 3, 'CHÀO HỎI - Từ Vựng & Mẫu Câu Đơn Giản', NULL, NULL, '2026-06-23 15:34:05.249', '2026-06-23 15:34:05.249001', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (10, 2, 'Vectors', NULL, NULL, '2026-06-23 15:36:36.764745', '2026-06-23 15:36:36.764746', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (11, 12, 'Course overview', NULL, NULL, '2026-06-23 15:57:51.063018', '2026-06-23 15:57:51.063019', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (12, 11, 'Các phiên bản Word thường dùng ', NULL, NULL, '2026-06-23 15:59:12.634888', '2026-06-23 15:59:12.634889', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (1, 1, 'Introduction to Web Development ', NULL, NULL, '2026-06-23 10:33:16.714651', '2026-06-23 10:33:16.714715', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (13, 1, 'What is an IDE?', NULL, NULL, '2026-06-24 09:20:16.169221', '2026-06-24 09:20:16.169249', 'active', NULL, false);
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, moderation_feedback, is_removed) VALUES (14, 1, 'Building Your First Website', NULL, NULL, '2026-06-24 09:21:45.356167', '2026-06-24 09:21:45.356168', 'active', NULL, false);

INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (15, 3, 'Giới Thiệu Khóa Học Edit Video Trên Phần Mềm Capcut Pc - Bài 1', '<p>Khóa học chỉnh sửa video trên CapCut PC này bao gồm mọi thứ từ cơ bản đến nâng cao. Các bài học chi tiết sẽ giúp bạn hiểu đầy đủ các công cụ có sẵn trong CapCut dành cho PC. Học cách chỉnh sửa video hấp dẫn và chuyên nghiệp.</p>', '2026-08-16 02:21:54.915282', '2026-08-16 02:21:54.915305', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786846937/y3a8bduosc7bn6q2gibe.mp4', '{"duration": 182, "file_size": 3905437, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (17, 4, 'What Are Design Patterns  Introduction To Design Patterns And Principles  Geekific', '<p>&nbsp;If you’re in the computer science domain, you definitely have heard of design patterns before, or even used a few patterns in practice with or without your knowledge.&nbsp;</p><p>In this video we attempt to answer: What are design patterns? Why were they created? and what is actually the main purpose behind them?</p>', '2026-08-16 02:42:19.073808', '2026-08-16 02:42:19.073808', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848160/zkt6w0tfmajqrumcauqc.mp4', '{"duration": 441, "file_size": 9411965, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (18, 6, 'Docker Crash Course #1 - What is Docker', '<p>In this Docker tutorial series you''ll learn what Docker is &amp; how to use it to help improve the development experience both alone &amp; in a team.&nbsp;</p>', '2026-08-16 02:42:57.001679', '2026-08-16 02:42:57.001679', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848199/adktj4qjk4dlwoopssep.mp4', '{"duration": 446, "file_size": 11402253, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (19, 7, 'Hướng Dẫn Sử Dụng Hàm If Cơ Bản Trong Excel', '<p>Hàm IF là một trong những hàm phổ biến và quan trọng nhất trong Excel. Bạn dùng hàm IF để yêu cầu Excel kiểm tra một điều kiện và trả về một giá trị nếu điều kiện được ĐÚNG, hoặc trả về một giá trị khác nếu điều kiện đó SAI.</p>', '2026-08-16 02:43:28.005132', '2026-08-16 02:43:28.005132', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848230/yyhqhcr51it9amb69ont.mp4', '{"duration": 320, "file_size": 7125767, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (20, 11, 'Course Overview- Figma Design For Beginners [1 Of 13]', '<p>Welcome to the Figma Design for beginners course! This course will walk you through the entire process of creating a website design for a personal portfolio website. We''ll start by teaching you the fundamental concepts and features that Figma Design offers, and then we''ll go on a creative journey together to make a website that you can customize to make your own using some of Figma''s most exciting features.</p>', '2026-08-16 02:44:21.996472', '2026-08-16 02:44:21.996472', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848284/q7gftnj8seyaxj6cki35.mp4', '{"duration": 136, "file_size": 2943442, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (21, 5, 'Flutter Crash Course #1 - What Is Flutter', '<p>In this Flutter Crash Course tutorial series, you''ll learn how to make Flutter applications from scratch.</p>', '2026-08-16 02:44:50.373893', '2026-08-16 02:44:50.373893', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848312/okl1vobe9gtiauish9u1.mp4', '{"duration": 415, "file_size": 8927633, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (22, 8, 'Introduction - Japanese Lesson 1', '<p>This introductory sample lesson covers eight basic greetings:</p><p><br></p><ul><li>Good Morning - Ohayou (casual) gozaimasu (formal)</li><li>Good Afternoon - Konnichiwa</li><li>Good Evening - Konbanwa</li><li>Goodbye - Sayounara</li><li>Goodnight - Oyasumi nasai</li><li>Thank You - Arigatou (casual) gozaimasu (formal)</li><li>Excuse me, I''m sorry - Sumimasen</li><li>How do you do (nice to meet you) - Hajimemashite, dozo yoroshiku</li></ul>', '2026-08-16 02:45:20.124256', '2026-08-16 02:45:20.124256', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848342/zwrhm63pudhcdnw7x1xz.mp4', '{"duration": 296, "file_size": 14780165, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (23, 10, 'Vectors  Chapter 1, Essence Of Linear Algebra', '<p>Beginning the linear algebra series with the basics.</p><p><br></p>', '2026-08-16 02:46:12.137156', '2026-08-16 02:46:12.137156', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848394/nhie9v9hh4l6ygvmlu29.mp4', '{"duration": 591, "file_size": 12642485, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (24, 12, 'Học Word Bài 1- Các Phiên Bản Word Thường Dùng - Học Word Cho Người Mới Bắt Đầu', '<p>Giới thiệu về các phiên bản thường dùng của Microsoft Office Word</p>', '2026-08-16 02:47:00.194715', '2026-08-16 02:47:00.194715', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848442/kwycr59gi6ro5lgq2i4f.mp4', '{"duration": 493, "file_size": 10914221, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (25, 9, 'Bài 1 Học Tiếng Anh Cho Người Mới Bắt Đầu  Chào Hỏi - Từ Vựng & Mẫu Câu Đơn Giản', '<p>Trong video này, bạn sẽ học:</p><ul><li>Các từ vựng và cụm từ chào hỏi thông dụng như "Hello," "Hi," "Good morning," và "Good evening."</li><li>Các mẫu câu tự giới thiệu như "I am Lan," "I am from Vietnam"</li><li>Thực hành với câu mẫu và bài đàm thoại để luyện nghe, nói, và áp dụng vào thực tế.</li></ul><p>Bắt đầu hành trình học tiếng Anh dễ dàng và hiệu quả!&nbsp;</p>', '2026-08-16 02:47:30.415698', '2026-08-16 02:47:30.415698', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848473/daohoglcbxxwfute80aj.mp4', '{"duration": 352, "file_size": 6702737, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (16, 2, 'Welcome', '<p><br></p>', '2026-08-16 02:40:35.65422', '2026-08-16 04:27:31.509281', 'draft', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848057/fehr4bvlux9wj6tc6km9.mp4', '{"duration": 332, "file_size": 6790266, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (26, 1, 'Introduction To Web Development  Full Stack Web Development Tutorial', '<p>This video is an overview of what we will be learning in this course.&nbsp;This course teaches us from the very basic fundamental level to the very end.&nbsp;</p><p><br></p><p>A) It starts with HTML and how to work with elements and tags and use them on the website accordingly.&nbsp;</p><p><br></p><p>B) Next comes the styling part with CSS, its concepts, how it works and what it does. At the end of HTML and CSS, we will be able to make static websites on our own.&nbsp;</p><p><br></p><p>C) To add functionalities to our websites we will be learning Javascript next. Next, we will be learning version control using Git.&nbsp;</p><p><br></p><p>D) The next part of the course will be about Bootstrap and how to use its classes and make responsive websites using bootstrap row and column properties.&nbsp;</p><p><br></p><p>E) Lastly, we will be entering into backend using node, MongoDB and finally learning a very powerful frontend framework, React.&nbsp;</p><p><br></p><p>F) We will be learning to work with databases and will be developing APIs of our own.&nbsp;</p>', '2026-08-16 02:49:32.471799', '2026-08-16 02:49:32.471799', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848594/rqf26zmhjiyaqgjgmpky.mp4', '{"duration": 479, "file_size": 10471913, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (27, 13, 'What Is An Ide  Installing An Ide  Full Stack Web Development Tutorial', '<p>What is an IDE?</p><p>An integrated development environment ( #IDE ) is a software application that provides comprehensive facilities to computer programmers for software development. It makes the workflow easier and supports various features like auto-complete, visual enhancement, has various plugins and tools that make writing code easier.&nbsp;</p>', '2026-08-16 02:50:09.623132', '2026-08-16 02:50:09.623132', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848632/eqemubukyqtyh6dkkr2u.mp4', '{"duration": 337, "file_size": 11594848, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES (28, 14, 'Building Your First Website  Learn Html  Full Stack Web Development Tutorial Course', '<p>This video introduces us to the basic structure and elements of HTML.&nbsp;</p><p>This website is a very beginner level website using HTML only which shows the use of some basic HTML tags and how to use it inside the code. We will cover a few HTML tags like html, head, body and their structure.&nbsp;</p><p><br></p><p>All HTML documents must start with a document type declaration: !DOCTYPE HTML.The HTML document itself begins with html tag and ends with an HTML tag. The visible part of the HTML document is between body and /body.&nbsp;All of this will be discussed elaborately in the next video. We will learn how to work with headers and several other basic tags like p, br, hr, em, and know-how these tags add a different styling to the text body.</p>', '2026-08-16 02:50:27.641231', '2026-08-16 02:50:27.641231', 'active', NULL, 'https://res.cloudinary.com/dndah3xuz/video/upload/v1786848649/xhig6cdpbavlakjs7tof.mp4', '{"duration": 768, "file_size": 16758951, "file_type": "video", "page_count": null, "file_extension": "mp4", "original_file_hash": null}', NULL);

INSERT INTO material_exts (material_id, file_hash) VALUES (15, '5dd10ef672443d8cc539d92ed79a6715');
INSERT INTO material_exts (material_id, file_hash) VALUES (16, '04f17910156b832e6b523bffdd6a6c76');
INSERT INTO material_exts (material_id, file_hash) VALUES (17, '6dfc4ed950a23d7c57e63ba867fe58da');
INSERT INTO material_exts (material_id, file_hash) VALUES (18, 'e5fd2dcd073644eabaf87430707f7b42');
INSERT INTO material_exts (material_id, file_hash) VALUES (19, '1852c6413f22bdb5c6afb51f60df2801');
INSERT INTO material_exts (material_id, file_hash) VALUES (20, '30eabe32bcbcca3370b132d8edd1a80f');
INSERT INTO material_exts (material_id, file_hash) VALUES (21, 'c0e5be14117bd209ad32c8c5353ceb01');
INSERT INTO material_exts (material_id, file_hash) VALUES (22, 'acc1b0b7ec9f70fb97ef7de1a922b669');
INSERT INTO material_exts (material_id, file_hash) VALUES (23, 'fb0477cf60bcaeef9adfeaff54de1e14');
INSERT INTO material_exts (material_id, file_hash) VALUES (24, 'eafa71f4acd9cdc4e93bc2b12aa7ae0e');
INSERT INTO material_exts (material_id, file_hash) VALUES (25, 'caa3ddd31628d625e6f2671365ce50ec');
INSERT INTO material_exts (material_id, file_hash) VALUES (26, '74949481a100e151496784141aa4c207');
INSERT INTO material_exts (material_id, file_hash) VALUES (27, '90f96d7c8a025d76d291b9bb193e9f91');
INSERT INTO material_exts (material_id, file_hash) VALUES (28, 'e7198fd9c237bed39b78953029829ce4');

INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (14, 26, '[-0.22946696,0.2191297,-0.15708905,-0.30057502,-0.0008956043,-0.091590434,-0.24017215,0.20707545,-0.36532378,-0.46049416,0.30252317,0.108790256,-0.3677203,0.08924176,-0.3684014,-0.027325135,-0.6083687,0.16231604,0.32252705,0.09477808,0.993788,-0.08514753,-0.14381583,-0.3024188,0.25788772,0.5592007,-0.12736723,0.14668784,-0.19759499,0.4825797,-0.14602908,0.22951704,-0.053803507,0.3266233,0.27722383,0.1502454,-0.08220295,0.24592459,-0.3952463,-1.9590669,-0.5653223,0.09171087,-0.33165744,-0.06354667,0.07388116,0.2941148,-0.16494945,-0.11929341,0.007470081,0.2595773,0.26494992,0.46568573,-0.24675249,-0.041024566,0.030072644,0.20457108,0.33662558,-0.12681131,0.38545278,0.42794105,-1.070929,0.32357576,-0.194686,0.018351072,-0.20990466,-0.22942251,-0.057060033,-0.061311685,-0.37596974,-0.008198213,0.2545815,-0.19928674,0.5256042,-0.4836038,0.05536386,-0.27742657,-0.18149202,-0.1188852,-0.41122922,0.020444416,-0.09436347,0.15036502,0.25890514,-0.0826363,0.3933907,0.38050988,1.2409835,-0.11574711,0.03563941,-0.16146313,0.08449425,0.07677888,-7.076154,-0.92599785,0.023440592,-0.30854413,-0.4919769,0.29699644,-0.0960088,-0.12919828,0.095192425,0.01163572,-0.109835744,0.016322825,0.044570316,0.10002003,-2.0581229,-0.00739763,-0.3220088,-0.044934623,0.090184994,-0.18968615,0.03742977,-0.053181138,-0.110764034,-0.30847743,0.06686604,0.19033672,0.026787534,-0.071176186,-0.023572993,0.3110136,-0.040373787,0.054339338,-0.40039462,0.066011645,0.3351074,-0.17621996,0.2805155,-0.5973143,0.5811587,-0.55064887,0.3851766,0.94032204,0.26823223,-0.33544514,0.09334135,-0.43992153,0.52670556,0.10640403,-0.057056747,-0.31630018,0.09067901,0.21471673,0.06508496,-0.06522508,0.1926513,0.7226379,0.20067006,0.06327859,-0.25130987,-0.31539947,0.4206012,-0.014840222,-0.19815174,-0.38792783,0.4921976,0.3144232,0.1326899,0.3924425,-0.016402366,-0.28440386,0.23360386,-0.3008374,-0.12092468,0.122319624,0.15469186,0.06253576,0.097011186,-0.5813419,0.35970724,0.019829655,-0.31215906,0.21314402,-0.052227862,-0.019770829,0.35885522,-0.2675104,0.29822886,-1.0202754,0.28896612,-0.3335115,-0.12341615,-0.24716076,0.17683505,-0.11272055,-0.16920851,0.04558232,-0.351255,0.018339101,0.07595978,-0.11793287,-0.2213753,0.09319775,0.16682367,0.17251307,-0.18390597,0.30596802,0.07073692,-0.09443254,0.31445307,-0.21704477,0.12930118,-0.2978685,0.52532053,0.044767223,-0.15113033,-0.20248403,-0.24499072,0.2436597,-0.87138724,-0.3652886,0.06320769,-0.059009846,-0.017785639,-0.028282557,0.09032177,-0.084454395,1.3431246,0.5474669,0.5562061,-0.6927829,0.038460195,0.20477165,0.17141163,0.24321531,0.19354258,-0.057940066,-0.110814355,0.07611852,-0.5657103,-0.36325446,-0.1786191,0.045988813,0.1986661,-0.44420493,-0.12611286,-0.15484433,0.055672824,-0.23805593,0.39881665,0.28649473,0.44181198,0.0068050916,0.42169285,0.9466304,0.19939941,0.24790682,-0.108345054,0.26996022,-0.049441185,0.035165388,0.39633676,-0.029047908,-0.051426847,-0.123316586,-0.11682879,0.46783897,-1.7887369,-0.19488968,-0.23298825,-0.20205234,0.32688376,0.6862787,0.19070308,-0.07601259,0.08270215,0.17845458,0.19198832,-0.54542166,-0.0074770087,0.0015031743,-0.38863903,0.3840469,-0.028012738,0.017111521,-0.022345893,0.02715147,-0.0949672,0.15082277,-0.23188512,0.33508843,0.24779652,-0.14393103,0.104665324,-0.034961212,0.3489006,-0.09893846,0.0758118,-0.48351988,-0.26243865,0.348085,-0.32796252,-0.11997153,-0.39332384,0.3026733,-0.03362798,-0.24661642,-0.34123933,-0.28101704,0.25839168,0.03401878,-0.12016536,-0.10249196,-0.26008847,-0.4519427,0.042418953,0.5880124,0.10249177,0.05350375,0.29062694,0.008184296,0.9395341,0.65459305,0.37835765,-0.27493408,-0.06579561,0.15329173,0.0029776557,0.7731719,-0.37374398,0.42526898,-0.111016534,0.0051368456,-0.4461128,0.29274327,-0.31718028,0.046677664,0.22622086,0.22470312,0.035682607,-0.3345815,-0.22937109,-0.25993153,0.22703873,0.2122304,0.16795233,0.25640962,0.0060905823,-0.008245007,-0.19719774,0.35677564,0.03233714,0.25433728,0.13566405,0.10366414,0.4462322,-0.2591238,0.40164503,-0.2871671,-0.023209142,-0.2503504,0.092289075,0.07644921,0.12887162,0.1036727,-0.09889017,0.67228305,0.10146304,-0.18189238,0.25525862,0.18172091,0.17982933,0.51143354,-0.38237187,-0.16009517,-0.0030943956,-0.8092412,-0.3247823,-0.019360742,-0.07119164,0.35708004,-0.17129047,-0.46101454,0.099092595,0.037978716,-0.33819744,0.25380147,-0.66540545,0.19022259,0.1283331,0.11972692,0.1185148,0.13079669,-0.10146176,0.053367786,0.5679237,-0.036984004,0.023111042,-0.3634009,0.5012476,0.053664576,-0.35702765,0.12967892,0.10120029,0.02834135,0.32642597,-0.07108699,-0.86973387,-0.4217058,-0.20697474,0.06440213,0.66196674,-0.03960432,0.066793025,-0.15934695,0.05542137,0.29786754,0.30321586,-0.30252025,0.4546031,0.05881374,-0.36771604,0.22482184,-0.14736967,-0.17169039,0.31579545,0.28482747,0.15600148,-0.16777721,-0.31216502,-0.21782978,0.06480596,0.3617945,0.050462574,0.15690312,0.008499983,-0.8159366,-1.3626554,-0.44839662,0.22552598,0.16681287,-0.66836387,-0.013657702,0.056677703,0.14754042,-0.10476541,0.029566078,-0.10643659,-0.2542399,-0.030197466,-0.3466037,0.16300713,0.5913033,-0.14493735,-0.47974,-0.10196269,-0.06411698,-0.3476889,-0.15427832,-0.9194412,-0.1465341,0.049176358,0.04540543,0.18866996,0.066008,-0.5827486,-0.4152773,0.48925403,0.24355294,0.4716569,0.017213019,-0.3423523,0.07490494,0.11846302,-0.2592071,0.27430442,0.25542265,-0.1172752,-0.08120636,-0.04786177,0.508048,-0.16174413,-0.025240956,-0.00020923858,-0.4123644,-0.03718078,0.4249802,-0.15967707,0.10588092,-0.13148704,-0.10490326,0.13972569,-0.3502378,0.15509787,0.56742984,-0.26372454,-0.31331322,-0.1535064,-0.29824826,0.49286395,-0.3523692,0.46290573,0.0063457587,-0.3472211,0.30064827,0.22566347,0.12785669,-0.18995082,-0.34504917,0.42221168,-0.22764656,0.31907502,0.029382478,0.13489573,-0.2330327,0.2500317,-0.366207,0.02776824,0.80904794,-0.05167425,-0.38348293]', '2026-08-16 04:22:09.410942');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (15, 27, '[-0.07597332,0.08949719,-0.24818294,0.00369295,-0.08951993,-0.13092753,-0.02385617,-0.15767927,0.34155387,-0.19709937,0.24670176,-0.264418,0.12019611,-0.002950519,-0.23149593,0.059367724,-0.046493247,0.22308975,0.06902758,-0.27735057,1.23023,0.33796746,-0.124095105,-0.15339053,-0.16069421,0.008894738,-0.3369019,0.4577803,-0.28076184,0.25770342,-0.23287885,0.09571727,0.06786008,0.24149114,0.6567171,0.060406394,-0.044700988,0.4536979,0.32562152,-1.7336594,-0.22522405,-0.20497897,-0.11442069,-0.57819813,0.065579206,0.27833858,0.078325406,-0.3878625,0.4384635,0.24092242,-0.15430897,0.14001356,0.05933501,0.1410429,-0.11297612,0.24558417,0.34111792,0.048452906,0.09484671,0.14815585,-0.95512575,-0.18585359,-0.03196748,-0.21286009,-0.14827232,-0.15742718,-0.26786307,0.47578657,-0.08447089,-0.16646548,0.28608668,0.02651341,0.32670623,-0.22702312,0.09505318,0.11731726,0.34623116,-0.08635464,-0.27165598,0.030300377,-0.0715303,0.059646297,0.037528154,-0.2946373,0.050420545,0.053942747,0.9608911,0.028348843,-0.017451452,-0.21660425,0.25274307,0.25846943,-5.6778,-0.44962394,-0.03565931,-0.03239179,-0.44974256,0.043556843,0.058518488,-0.41542774,-0.25831473,0.07691694,-0.10969722,-0.16690321,-0.1731425,0.030879717,-2.520931,-0.069124244,-0.15627746,0.30340743,0.29300213,-0.0096608335,0.21049082,-0.30890772,-0.064225204,-0.0851612,-0.059544664,-0.12831978,0.07988234,-0.22837038,0.18745497,-0.015028962,-0.042571567,-0.21235512,-0.2077859,0.10580949,0.14698057,-0.3544347,0.01956828,-0.17898336,0.2312816,-0.4134078,-0.053951137,0.7638605,-0.14260072,-0.17087997,0.009654983,-0.4273347,-0.0007082908,0.07084088,0.08448798,-0.191382,0.09570047,0.14431073,-0.21715638,0.14363942,0.0058660307,0.55287606,0.24621814,0.08190417,0.13447165,-0.36572033,0.22595878,0.012105172,0.005031433,-0.73073244,0.09409921,0.31994668,0.11945159,0.17757277,-0.07112671,-0.045389395,0.30707774,0.023453278,0.31208596,-0.23516877,0.6741537,0.23565835,0.07147802,-0.06912544,0.21984869,0.14795126,-0.27488315,0.031393256,-0.095944814,-0.056059305,-0.024182096,0.044401307,0.050918445,-0.17270307,-0.05240604,-0.2996019,-0.18640229,-0.24601674,0.42417514,0.20487523,0.15386991,0.006815936,-0.3621326,0.11281652,0.23961249,-0.2508581,0.10028827,-0.024226453,0.115159765,0.24587455,0.048206925,-0.19908552,0.19624631,0.097381346,0.055367596,0.32892612,-0.29007205,-0.47884527,0.1583587,0.10712985,-0.29326203,0.058615696,-0.3176378,0.017661937,-0.6048333,-0.044465058,0.2802454,0.09918889,-0.064371645,0.22948572,-0.044706088,-0.14338881,1.127841,0.083559096,0.36840814,-0.45890215,0.18904519,0.09787887,-0.017483223,0.007850335,0.43604362,0.33507478,-0.12912586,0.024140563,-0.35434893,0.12096512,0.18286484,-0.19311841,0.20125891,-0.6728686,0.13909274,-0.1935718,-0.063049436,-0.42292932,0.25402883,-0.0028059406,0.2724035,0.14818011,0.1282987,0.91127175,-0.014875335,0.29809576,0.020194,0.070716605,0.0003394395,0.03142569,0.018016119,-0.1609773,-0.15816514,0.014037952,-0.13077171,0.24389349,-1.4801689,0.15616255,0.02893146,-0.11321499,0.15550432,-0.03006795,0.42341915,-0.011460356,-0.12904571,0.019146137,0.05115389,-0.17539524,-0.11551341,0.43576473,-0.20089854,0.12903471,-0.04527733,0.03467731,-0.022196267,0.2075999,-0.29698193,0.16114698,-0.15196824,0.38849685,0.30574834,0.12676503,0.001016656,0.009143909,0.07580795,-0.19851062,0.21071938,-0.3311161,-0.064830355,0.13327475,-0.17490445,-0.037824184,-0.12041027,0.3334,0.046335418,-0.10743258,-0.31055853,-0.47598204,0.06384316,-0.010397018,-0.076923296,-0.1535513,-0.20202765,-0.5316908,-0.0026944785,0.24937575,0.12140374,-0.022155415,-0.0893377,0.09849351,0.7620411,0.024223208,0.003348427,-0.5718585,-0.08902881,-0.00448708,0.0771101,0.54896295,-0.29930386,0.5820144,-0.16752836,-0.13757555,-0.36434788,-0.23938501,0.16186906,0.006812159,-0.14855087,-0.30812243,0.14538902,0.07221104,0.0997439,-0.21776767,0.051481068,0.165335,0.43875322,0.19962062,0.11744486,0.30457562,0.15064634,-0.11075922,-0.22982047,0.15092772,0.14469704,0.26463097,0.3410087,-0.059036914,0.43188938,-0.08588915,-0.388529,-0.1312902,0.14873967,0.24530713,-0.18031454,0.4171677,0.37430692,0.6588642,0.018266093,-0.04110792,0.18195732,0.19544926,-0.0699614,0.97471553,-0.053987715,-0.21584976,-0.43133926,-0.5719648,-0.08972953,0.058955107,-0.059721183,-0.047930118,0.07107328,-0.06506381,-0.00773731,-0.061483458,0.246966,0.286428,-0.35970956,0.055145122,-0.08302969,0.00852717,-0.23853633,-0.15438232,-0.00029109395,-0.24389566,0.2809977,-0.070900224,0.10472138,0.7672235,0.59514433,-0.15623596,-0.28669882,0.28179854,0.023959372,-0.08287529,0.094774514,0.1965164,-0.61668044,-0.17288125,0.042812545,0.14530967,0.5950562,-0.27742335,0.27925348,-0.18320537,-0.20931718,0.06918197,0.789617,-0.2558517,0.250399,-0.15062971,-0.17082678,0.40162638,-0.17840822,-0.10759374,0.3663796,0.20717071,-0.2828849,-0.15906091,-0.33302888,-0.045077577,0.09062756,0.03305425,-0.15437993,0.14435859,-0.09260442,-0.49377313,-1.4124471,-0.18318112,-0.2821339,0.11822528,-0.4811636,0.17583288,-0.20777926,-0.19932616,-0.06816215,-0.09778868,-0.29395434,-0.092936166,-0.18672195,0.10715394,0.2107753,0.43145788,0.3455586,-0.24682482,-0.190228,0.06919728,0.107985005,-0.24960896,-0.42073584,-0.20038241,0.3987682,0.19810759,0.06642565,0.45457846,-0.17582594,-0.2185729,0.5648529,0.23039459,0.19060054,-0.2746911,-0.28935385,0.14628944,-0.1849875,0.07259063,0.5124569,0.09290111,-0.19206594,-0.058622885,-0.18748239,0.38927382,-0.030167533,-0.14854257,0.16247974,-0.41345957,-0.08589468,0.17172164,0.044793595,0.048155647,-0.06284214,-0.30551955,-0.018062798,-0.25779793,0.37699166,0.33011723,-0.06551467,-0.011282478,-0.099839866,-0.3258408,0.5350869,-0.31830516,0.35021618,-0.17978826,-0.13165787,0.26097372,0.16903834,-0.12697668,-0.20983464,0.063518286,0.31886145,-0.24640898,0.12489566,0.043802787,-0.24188212,-0.13148607,0.04490046,0.060343497,0.0064346,0.61839473,-0.30179906,-0.3061753]', '2026-08-16 04:22:09.44107');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (16, 28, '[-0.009346024,0.01326194,-0.12262792,-0.05998034,0.12663879,-0.117703624,0.16618119,-0.122618556,0.019216964,-0.30131552,0.111489244,-0.28451526,-0.3311452,0.04840293,-0.24409573,-0.012702,-0.38626152,0.27926096,0.06728659,-0.26356864,0.65188026,0.23595677,-0.0013028218,-0.14589575,-0.24432023,-0.115321994,-0.11385443,0.6186105,-0.32728094,0.270708,-0.12598436,0.3190658,0.21479836,0.11061737,0.28040898,0.09040752,0.1526829,0.40816358,0.10278846,-1.8181055,-0.1378565,-0.2221862,-0.13488975,-0.44850174,0.23643799,0.46369058,0.2121065,-0.4083055,0.35266188,0.46102133,0.00701689,-0.39797625,-0.21244396,-0.1456517,-0.10563254,0.15853345,0.44424736,-0.02159819,0.028605318,0.10512046,-0.9742391,-0.015097631,-0.18355231,-0.42961895,-0.18448153,-0.12550339,-0.019421903,0.19075122,-0.11931617,-0.2914011,0.44909748,-0.083682835,0.019590544,-0.20640427,0.18910149,-0.0038881213,0.1855742,-0.088358566,-0.21284652,0.20212041,-0.056789145,0.17998154,0.30235806,0.028574327,-0.25401345,0.11916726,0.80215937,0.055045843,0.14859393,-0.35593915,0.22775434,0.12424013,-6.510861,-0.2342674,-0.10772947,-0.21145868,-0.2583489,-0.08849006,-0.19252643,-0.42812017,-0.13726397,0.008886743,-0.24184264,-0.26109102,-0.36036113,0.0932787,-2.60353,0.10710863,-0.43395808,0.32314593,0.18813214,-0.2624794,0.24784264,-0.10452444,-0.2293205,-0.09590908,0.06571535,-0.10403161,-0.16487421,0.10091448,0.27718374,0.19406638,-0.2171714,0.06471546,0.018814879,-0.024987426,0.26443067,-0.19218871,0.25701806,-0.38121256,0.22635445,-0.618988,0.14528385,0.8259925,-0.0047307624,-0.11319528,0.14532998,0.04716952,0.10438121,0.01827326,0.13165641,0.00054790376,0.034081038,-0.044650618,-0.12957771,0.10253343,-0.13749152,0.55602336,0.23692012,-0.0800455,0.27750188,-0.3444213,0.41344935,-0.12844399,0.25515845,-0.52562755,0.06459133,0.591753,0.38798165,0.015532776,0.061520092,0.20531131,0.25996017,0.10714758,0.3615211,-0.23330356,0.6499851,0.37214014,-0.16136514,-0.3042073,-0.08496738,0.19844773,0.23635668,0.20281975,0.09381978,-0.0905629,0.52929556,0.115254,0.23197229,-0.030692894,0.12307044,-0.36565903,-0.04336287,-0.04926539,0.025872396,0.13076825,0.033435848,0.1051851,-0.2641752,0.11156765,0.49540985,-0.22597112,0.055755373,0.026224215,0.11126825,0.34215975,0.08201923,-0.35953668,-0.13919212,0.21750176,0.145041,0.4558617,-0.07579508,-0.53098965,-0.015840042,-0.12985523,-0.2886526,0.48226175,-0.14658085,-0.1229859,-0.5678949,0.026831824,0.30855563,-0.13826041,-0.17739202,0.09398633,0.08231896,-0.024390228,0.8589387,0.26563847,0.2917264,-0.39025036,0.11337398,0.1381782,0.16086522,0.053933658,0.39296323,0.27538046,-0.14235325,-0.08188152,-0.50049293,0.22294363,0.06546736,0.13387369,0.06620836,-0.6432992,0.033704907,-0.1943421,0.17708819,-0.22958824,0.26706907,0.044803303,0.25530428,-0.3430809,0.06295172,0.64669365,-0.2175238,0.31121555,0.22939207,0.1055634,-0.15324022,-0.16605192,0.10300837,-0.004173735,-0.056308173,0.17803323,-0.10538209,0.14974488,-1.830872,0.19302942,-0.024216935,-0.5604846,-0.07438201,0.23400798,0.36551026,-0.006877118,-0.32627156,0.1464448,0.09614518,-0.26720494,-0.09701737,0.17975874,-0.21919058,0.19906306,-0.014415791,-0.021128079,-0.0071565807,0.09325835,-0.18480113,0.27178952,-0.034961205,0.43680686,-0.120336615,-0.05907712,-0.029345073,0.051217128,0.34782887,-0.2422004,-0.035753664,-0.15998007,0.15485765,0.08505998,-0.21747723,-0.25470272,-0.11290794,0.6106475,-0.3693583,-0.2543895,-0.087866634,-0.5367658,-0.14635119,-0.0309795,-0.07978222,-0.4887467,-0.22953068,-0.67514235,0.17471677,0.4479435,0.15554978,0.13135257,-0.11020142,0.10559139,0.8247379,0.09221724,0.016477099,-0.35879013,0.034163386,0.061315883,-0.06039975,0.6639091,-0.11563905,1.0411208,-0.27769068,0.15154782,-0.23069453,-0.18043602,0.069124766,-0.010517959,0.036690984,-0.05411336,0.057738293,0.09005568,0.08378359,-0.21746418,-0.13430457,0.336923,0.2270658,0.38073775,0.11915128,0.3168182,0.15471236,-0.08845051,-0.2961792,0.32999486,0.121267736,-0.24516569,0.31714785,-0.03664779,0.33000353,0.13197061,-0.12781543,0.000703525,0.06371712,0.32382,-0.08393127,0.27184573,0.2624275,0.61861664,0.15000926,-0.089897856,0.17114803,0.23754123,0.006549723,0.43267122,-0.60393167,-0.61996675,-0.5115724,-0.49350405,-0.22244439,-0.059791654,0.22737233,0.06523023,0.0020159225,-0.21786377,0.14956513,-0.1681663,-0.270179,0.47381234,-0.73230535,0.11059212,-0.107096285,-0.2361235,-0.044599984,-0.26437524,0.14900477,-0.29233903,0.3745236,-0.23266388,-0.054758,0.49788612,0.7001095,0.07955599,-0.25676736,0.08403315,0.15469731,-0.08819949,-0.039025277,0.13522017,-0.52403474,-0.5112339,-0.110363364,-0.034977723,0.45749405,0.09703932,0.2011563,-0.1267439,-0.42545947,-0.07528841,0.76555616,-0.22217503,0.5503165,-0.17036648,-0.22994666,0.46219698,-0.041060265,-0.13475503,0.43851602,0.3165974,-0.30731198,-0.31200576,-0.4136379,-0.19352686,0.14296089,0.15499942,-0.021507945,0.32007757,0.17239502,-0.6512832,-1.1444811,-0.21499759,-0.42301273,0.2379955,-0.2913841,-0.050188642,-0.16390316,-0.21724148,0.042605184,-0.23324807,0.076268785,-0.11619172,-0.27020526,0.0608154,0.018175116,0.19807689,0.46367854,-0.24137795,0.049425676,0.02204947,0.2856225,-0.17646948,-0.6167238,-0.1713606,0.29580757,0.14984111,0.16050662,0.26612678,-0.06766315,-0.21662652,0.37989384,0.24884683,0.4057214,-0.19646312,-0.30309576,0.0034634953,-0.4077712,0.06401773,0.59503025,-0.0028471027,-0.18031795,-0.09544278,-0.38752568,0.47119883,-0.19549188,-0.27800277,-0.10368147,-0.506883,0.018120522,-0.07055158,-0.023700753,0.34550783,0.11980276,-0.117859714,-0.051366504,-0.5006329,0.37370825,0.24053931,-0.030165348,-0.3724684,-0.303222,-0.47264495,0.3836305,-0.5433613,0.2631731,0.093666695,-0.25982746,0.30704963,-0.035450056,0.043525618,-0.07613522,-0.2379997,0.15036502,-0.114785686,0.07437225,-0.052685022,-0.11522642,-0.21243371,0.11223913,0.048258774,0.012004201,0.9443784,-0.39924222,-0.059657365]', '2026-08-16 04:22:09.450642');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (13, 15, '[-0.2683348,0.0549966,-0.3117884,0.17541534,0.113508396,-0.45739025,0.14141595,0.024611084,-0.30544767,0.21185507,0.3239142,0.23387122,0.3386564,-0.020462165,0.51119494,-0.029366856,-0.6209136,-0.1858729,0.37670803,-0.33545852,0.26657903,0.15615255,-0.22789863,-0.21770759,-0.028722126,0.3691977,0.16334893,-0.0034785266,0.15407541,0.28819388,-0.17689121,0.2627817,-0.01555613,-0.09905401,0.5661897,-0.047775622,-0.056430116,0.080515236,0.12906495,-1.2038803,-0.28454694,0.19368908,-0.20144933,-0.13133083,-0.04731975,1.8019288,0.20788851,-0.39939204,0.5321161,-0.23657785,0.35138425,0.12645143,-0.043150652,0.07713627,0.13515933,-0.011258578,0.58217263,-0.15543509,0.31955656,0.15128241,0.09735436,-0.47477362,-0.3030137,-0.34012252,-0.010039315,-0.34536302,-0.015092755,1.0906153,0.25472715,0.23947038,-0.13176039,-0.24240883,-0.04972031,-0.09852201,0.25459334,-0.36865467,-0.20846131,0.3135713,-0.19012687,0.19968568,-0.24368359,-0.3098247,-0.16840045,0.45449963,0.42178798,0.16894431,-0.17270055,0.058037385,0.61079794,-0.2625037,-0.014993424,0.01522492,-4.02987,0.07300673,-0.13429643,0.16723268,-0.17553431,0.08400151,-0.31044665,0.14714599,-0.12844826,-0.24361876,0.15561354,0.19604269,-0.46383402,0.0486429,-1.0807134,-0.24889094,0.0014311792,-0.10603839,0.03620731,-0.27216828,0.23657,-0.15044783,-0.24350984,-0.417502,-0.102418035,0.19288604,0.2420888,-0.07235404,0.3231939,0.766641,-0.06190039,0.2203706,0.14491756,0.14911272,-0.19953285,-0.13838936,-0.18210725,-0.09659367,0.3376234,-0.3195655,0.25750273,0.81053317,0.1510263,0.30718178,-0.027792078,0.07612365,-0.48997623,0.3095797,0.115517795,-0.13239141,-0.017793125,-0.20653568,-0.31222588,-0.012771329,-0.27451852,0.080158316,0.504192,-0.034794874,0.04035699,-0.5572227,0.27694154,-0.3541484,-0.27272397,-0.13060392,-0.086073145,-0.1445898,-0.23045173,0.014771336,0.006050931,-0.03231076,0.13801691,-0.2782116,-0.12743658,0.05080526,0.80287856,0.14506434,0.13941856,-0.50279903,-0.30073828,0.059625134,0.41928998,-0.10799174,-0.003606405,0.25379345,0.49637452,-0.18843023,-0.02854955,-0.119364426,-0.14072695,-0.19656903,0.04329928,0.16589677,0.012108938,-0.05886222,-0.345013,-0.16925617,-0.1415868,0.27038342,-0.3694176,0.046556905,-0.051094413,-0.16242728,-0.034383,0.15944184,0.07419139,0.062168352,-0.009585237,-0.064908646,-0.19532211,0.79276407,-0.19809194,-0.046386223,-0.19683063,0.5945893,-0.25714153,-0.24054976,-0.65713626,0.015152052,-0.549602,0.09093359,-0.008086659,0.005760286,-0.07979693,-0.49376997,-0.16322415,0.37262985,0.006293886,-0.13537905,0.20692216,0.62392616,0.18266813,0.18784016,0.4136789,0.2114949,0.098514505,-0.2770556,0.0768236,-0.23063736,-0.34407198,-0.11031054,0.1325262,0.048854407,0.2434831,-0.0114323385,-0.33690578,0.081999734,-0.08092118,-0.3380822,0.029502636,0.6419749,0.7804626,-0.18685855,0.15643127,0.16821109,0.27005592,0.19685686,0.10549211,-0.19984962,0.10316318,0.07462846,-0.10924423,0.17182665,0.059296906,0.23604633,-0.21748877,0.069769,-0.53038776,0.18511231,-0.04853082,-0.26273876,0.2000441,0.86497444,0.058849573,-0.19184817,0.0669524,0.09834833,-0.38927373,-0.020864345,-0.06566613,-0.147283,-0.094621874,0.24020866,-0.30192846,-0.21514915,-0.10862233,-0.22877821,-0.104036726,-0.042681593,0.14918594,0.35108015,-0.32667592,0.40481672,0.0032205577,0.008469607,0.10389049,-0.13979316,-0.15957715,-0.11572662,0.1894522,-0.18999688,-0.32254645,0.16553158,0.08679712,-0.011168129,-0.068654165,-0.07737037,0.052801736,0.16963588,0.36654282,-0.4003199,0.20107576,0.1307651,-0.0056837387,-0.076871105,-0.24620722,0.109062195,0.35845387,0.058756616,-0.08193688,0.54418397,0.8098272,0.32502374,0.48550475,0.33603895,0.47229987,-0.04005184,0.4329348,0.28567326,-0.15327783,0.7479102,-0.07024821,-0.1980504,-0.3041551,-0.25948626,-0.16884653,-0.2368173,-0.22704539,-0.48634464,0.19656326,-0.09681172,0.10498492,0.047626466,0.12368868,0.28497863,-0.1643325,0.0038492647,0.0339043,-0.051075973,-0.111463144,-0.03843561,0.008738218,0.29065633,-0.24157342,-0.16627382,-0.38742602,0.07989078,0.23318319,0.1787765,0.26503205,0.22936715,-0.32085332,0.2889247,0.37360612,0.14889987,-0.111612834,0.4565062,-0.18702705,-0.10601704,-0.27262253,-0.16364431,0.15424877,0.6394327,-0.31771633,-0.36404955,-0.27754733,-0.11151825,-0.20137918,-0.046253536,-0.0007546953,-0.2135986,0.08866588,0.14051208,0.34287155,-0.061582543,-0.26051122,-0.16382425,-1.0874418,0.057794735,0.5069639,-0.118760504,-0.14991939,0.027375473,0.13386257,-0.18060818,0.0083054295,0.30016848,0.2906373,0.67109543,-0.03231773,-0.20362146,0.25318822,-0.11097175,0.1014607,-0.0871453,-0.04167518,0.12373597,0.06297887,-0.12475119,0.26153883,0.001662645,0.20859027,-0.07542667,0.2815891,-0.26267672,-0.27012357,0.19872499,0.34092763,0.51216453,0.5439203,0.49253687,0.022695526,0.23193522,0.28340745,-0.42576376,0.067589566,-0.009710547,-0.60661924,-0.17148657,-0.15026529,-0.08896232,0.054212462,0.04117688,-0.117924735,-0.05784524,-0.114566766,-0.1922382,-0.38091186,0.16510387,-0.3230929,-0.10672407,0.34854406,-0.16015546,-0.01565762,-0.26246113,-0.4227638,0.1602233,-0.09184122,-0.01287977,0.13340448,0.32416677,-0.24068707,0.21078737,0.2551012,-0.6229146,-0.21022819,0.02879102,-0.1201837,-0.28894162,0.33206105,-0.07741063,0.29187143,-0.08860147,-0.3471551,0.15524326,-0.04576997,-0.16426477,0.0014667563,0.13168043,0.0071608424,-0.027627788,-0.23084699,-0.12846965,-0.098506555,0.002161437,-0.06243087,-0.07952677,-0.1613799,-0.2986276,0.0074806055,0.2593707,-0.44592214,-0.32300907,-0.23611793,-0.105110444,0.037880763,0.46185943,0.15154213,0.12385072,-0.34284273,-0.69907635,0.0521466,-0.35849926,0.012507571,0.30233055,-0.12528753,-0.16564864,0.15671623,0.2805666,0.23124604,-0.25406954,0.19496836,-0.00088926504,0.003170919,0.40756276,-0.09585429,-0.13684972,-0.06441715,0.10376916,-0.109284736,0.3745892,-0.30145657,0.19595155,0.5142383,0.052026108,-0.22357193,0.50606185,-0.28197956,0.5958181,-0.12081016,0.046455845]', '2026-08-16 03:04:18.172768');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (17, 16, '[-0.06781766,0.06430111,0.1656977,-0.21836717,-0.09215927,-0.12620634,0.058111075,-0.030466218,0.668519,0.03355453,0.059526075,0.028474323,-0.0069370945,0.15679373,-0.048532937,0.17057914,0.20094125,-0.33239692,-0.041325174,0.042802863,0.10640746,0.06613098,-0.30100384,0.10013823,-0.123765334,0.37089685,0.13123377,0.06172204,-0.004503048,-0.37489456,-0.08167834,0.08964094,-0.057221115,-0.098508306,0.4813428,0.00991411,-0.08663767,0.38340655,-0.053483747,-2.5099995,-0.0852126,-0.3499917,-0.26946712,-0.03915611,-0.14267622,0.53798693,0.42906606,-0.33639047,0.16185184,0.05779852,0.22819121,0.19242859,-0.10048842,-0.10519511,-0.12622598,-0.10790998,0.30663535,0.23209076,0.10009328,0.01577644,-0.10892469,-0.44201624,0.22242925,-0.04463458,0.07783757,-0.036044125,-0.28252396,0.5999654,-0.094667785,0.21442297,-0.07205321,0.36501256,0.12214876,-0.12217038,0.11431825,-0.061097927,-0.12763871,-0.14715414,0.077169046,0.044536285,0.25587454,0.15650092,0.0761882,0.19213076,-0.27304548,-0.23892134,1.1482952,-0.20716473,0.34512767,-0.15427242,-0.22794473,-0.28559852,-5.165908,-0.27860835,0.027195623,-0.031503003,0.009366665,-0.33304474,0.04003195,-0.23984979,-0.12052851,0.20994104,0.01607966,0.08149404,-0.03911368,-0.091613375,-2.0718057,-0.0070668855,-0.2372545,0.06414193,0.4592213,-0.05920908,0.05493205,0.067129835,-0.22015935,-0.23027214,-0.16714403,-0.31475368,0.092960306,-0.029012423,-0.28463,0.4499612,0.036566883,0.09444775,-0.17703159,0.16470964,-0.22070853,-0.114448436,0.11183193,-0.067122616,0.2939729,0.1148721,-0.17984685,0.8382699,-0.327807,0.14940041,-0.008347887,-0.09457528,-0.21569115,0.36006832,-0.13073175,-0.22008988,0.1950752,-0.19435161,-0.21610461,0.29351115,-0.078309685,0.5260829,-0.018428389,0.16317819,0.3129926,-0.18498695,0.9263967,0.01619623,-0.05173677,-0.080177486,-0.030499104,-0.13857913,0.29957238,-0.0449967,-0.21225515,-0.22389215,-0.0012916385,-0.048465874,0.31056538,-0.62089,0.10486316,0.0042555723,-0.21293427,0.06593044,-0.13527097,0.14340188,0.1669785,-0.048279814,-0.100465976,0.07005446,-0.44615233,-0.11320608,0.3102041,0.11214133,-0.21701641,-0.41610906,-0.20822065,-0.40420127,-0.09043047,0.33251244,-0.022451827,0.362253,-0.16882788,-0.15147844,0.39235416,-0.12182515,-0.2236348,0.27503726,-0.0052266717,0.16684042,-0.1488623,-0.002953262,0.010143973,-0.3101465,0.10050292,-0.3250215,-0.1918821,-0.02801363,0.13393843,-0.40378484,-0.15581664,-0.091754444,-0.17744623,0.1534882,-0.51562554,-0.14006424,0.30405098,0.19424434,-0.15832733,-0.046213184,0.20912592,0.1608033,0.010620213,-0.1798368,0.48679465,-0.4246928,0.096429825,0.24997433,0.0076617836,0.10762817,0.29745978,0.30979595,0.07629467,0.15762645,-0.18221584,-0.12233205,0.12523933,0.02119664,-0.1669763,-0.35944968,0.05769747,-0.037004158,0.02524447,0.1904675,0.1179584,0.06186548,0.018740451,-0.3270378,0.055828463,1.051903,0.12715256,0.26735306,0.15828952,-0.14744206,0.004293252,0.18513972,0.12546991,0.32087812,-0.014303727,0.06474137,-0.2689545,-0.055724937,-1.0272443,-0.23222804,-0.085362665,-0.0024296043,0.2479108,0.44603676,0.08271588,0.2912301,0.22830303,-0.37406406,0.122683145,-0.25701028,-0.06970394,0.29913065,-0.23780744,0.17389342,0.10853749,-0.07781347,-0.23098819,0.0468438,0.1245278,0.19070503,0.05458381,0.2859525,-0.28021777,0.02752121,-0.21037433,0.20405348,-0.65081894,-0.36398774,0.06791534,-0.49832076,0.21565302,0.10832798,0.060746137,-0.13756458,-0.18339752,0.6326088,-0.17101789,-0.14169367,0.2665681,-0.5118039,-0.18964157,-0.122566886,0.21710174,0.2681465,0.012637281,-0.38652188,0.12637803,-0.18026166,0.30404457,0.48679972,-0.08884346,0.050141316,0.8388469,-0.14535293,-0.15141375,0.15200248,0.06567763,-0.03014737,-0.50344145,-0.2315656,0.11736461,0.48756942,-0.2789357,-0.053527646,-0.51366776,-0.0052437526,0.17468846,-0.033494256,-0.24871129,-0.3220241,0.035441983,0.06418402,0.1396554,-0.33943394,-0.21185324,-0.13509256,0.099296905,0.24798767,-0.2526013,0.07843805,0.34673446,-0.10023392,-0.048798423,0.30288026,-0.14228073,0.33179656,-0.014672931,-0.23254684,0.011979319,-0.096868776,-0.11405084,-0.3479788,0.020072214,-0.009739173,0.031468052,0.2780382,0.20834203,-0.17797984,0.104223736,-0.05978169,0.37495273,-0.14436385,0.13036317,0.35262096,-0.8310242,0.2945693,-0.37085962,-0.5669668,0.008963011,0.124103904,0.0148076005,0.012903123,0.124881916,0.17202678,0.18278155,-0.16905482,-0.5634886,-0.13832542,-0.41819552,0.097452834,-0.061667547,0.09980647,-0.28326705,-0.08465128,0.056072608,-0.618656,-0.246932,-0.20763975,0.026973715,0.5593803,0.20934463,0.06572726,0.24513395,-0.006827641,0.17816867,0.40389588,-0.1795018,0.0910682,-0.38557678,-0.38615572,0.13619798,-0.120233506,0.40550253,0.10658038,0.13663088,-0.09870943,0.13420966,0.17264278,0.7498917,0.27446175,-0.07194323,0.32228988,-0.13661426,0.46799397,0.043514144,-0.26145688,0.44045952,-0.3023739,-0.1922488,-0.23455024,-0.24471298,-0.038323764,-0.15000594,-0.14876257,0.06155381,-0.116744466,-0.20808232,-0.10138523,-1.3284814,-0.25559384,-0.44495377,0.25022727,0.16814622,-0.26722792,-0.054119106,-0.17445575,-0.35759318,0.07208529,-0.20008975,-0.085484184,-0.055909846,0.3613638,0.264911,0.22012195,0.28624195,0.05930285,0.19224977,0.080703884,0.15849523,-0.0697847,-0.08630875,-0.048299685,0.25612345,0.06469927,0.21179463,0.033512328,-0.2623029,0.007633937,-0.13015316,0.30073744,0.08631214,0.039059706,-0.38732865,-0.04040195,-0.074262835,-0.20769738,0.22742146,-0.12938538,0.0096264435,-0.074958235,0.18027216,-0.05974863,0.013366421,-0.31225872,-0.0915445,0.18786737,0.23440382,0.2857331,-0.19277854,0.110238425,-0.086840525,-0.32955298,0.21557723,0.08812499,-0.21945745,0.15186055,0.0381113,0.040627204,-0.024383182,-0.25688887,0.085506886,-0.42002943,0.39145276,-0.415013,0.1458389,0.103706576,-0.04061667,0.1179844,0.25579253,-0.094878554,0.25916457,0.0035556538,-0.014122143,0.38406298,0.5400998,-0.3234362,-0.044325113,0.058454502,-0.06628359,0.6862038,-0.20283367,0.10312121]', '2026-08-16 04:22:13.70337');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (18, 17, '[-0.025906008,0.18315327,-0.2541402,-0.26662353,0.07678931,-0.11953937,0.30760336,0.3477192,0.46570924,-0.0561193,0.024363728,0.27491793,0.17550443,-0.20875567,-0.062288083,0.07683445,0.060063973,0.12522894,-0.26981062,-0.085748374,0.2467558,0.24341041,-0.08758754,0.11587139,0.018023737,0.22220938,-0.29341912,0.029060034,-0.17172493,-0.09892236,-0.09917049,0.1388286,0.09435005,0.20087923,0.19252214,-0.09550698,0.030652015,0.3410732,0.20952636,-1.756526,0.09332938,-0.21498562,-0.13298868,-0.123052716,-0.24130264,-0.23872595,0.41430366,-0.076637395,0.14614452,0.044574723,0.038971793,-0.20436887,0.10588985,-0.04096076,-0.11412513,0.22941317,0.19980034,-0.027873604,0.08077562,0.060157873,-0.42677662,-0.15998064,0.21559101,-0.16362402,-0.11761089,-0.06396389,-0.01154976,0.27438876,-0.15951644,-0.26574776,0.1568847,0.27309534,0.093596585,-0.1530405,0.10841622,0.2522729,-0.047403347,-0.38501433,-0.017777342,0.079593115,-0.05873954,-0.06279587,0.020221045,0.32019216,-0.22184892,0.076087005,0.4480799,0.07980054,0.34233233,-0.089173615,0.119945444,-0.008504305,-5.4978395,0.43282887,0.030833002,0.1929368,-0.03658372,-0.16532412,0.19851656,-0.18316494,-0.17199591,0.020995473,0.0114284,-0.08370253,-0.111000136,0.08515503,-1.8847982,0.06628271,-0.18789941,-0.099734195,0.18256588,-0.0878878,0.18167217,-0.09677831,0.024694305,-0.21059461,-0.16221625,-0.1305393,0.1619842,-0.15394202,-0.19891293,0.1085083,0.01178299,0.17565699,0.15328753,0.22381343,0.09617564,-0.0037672229,0.049256004,0.05606328,0.15713397,0.030435625,0.07943561,0.8682452,-0.007611561,0.23253761,-0.09775944,-0.094845176,-0.123572454,-0.025187986,0.11671364,-0.096099935,-0.11945573,0.1050263,-0.19326584,0.11955421,-0.21697108,0.03460245,0.13609163,-0.17889147,-0.084712,0.0047159162,0.7945922,0.031047493,0.056582343,-0.107968554,-0.2562745,0.11405005,0.14115982,0.36692783,0.09531365,-0.032977752,0.01654628,-0.03877351,0.5207073,-0.10899809,0.06502288,0.23996323,-0.15171427,-0.085021615,0.04891647,0.10317797,0.14089657,-0.20650582,0.07981037,0.09430199,-0.398011,-0.009200146,0.017950274,0.27619195,0.116709754,-0.297588,0.0079749655,-0.006489233,-0.07628958,0.19557509,0.10622427,0.1796682,0.02955871,0.1104022,-0.084061265,-0.024752237,0.028429324,0.10046997,-0.12783551,0.074809216,0.00828095,0.035084564,-0.32539842,-0.084057435,-0.011517056,0.037056316,-0.05105203,-0.08895424,-0.18722709,-0.0023176149,-0.27442253,0.16625227,-0.017579595,-0.06358813,-0.35683048,0.014566799,-0.108609624,-0.0023315076,0.078355886,-0.23058023,-0.13895623,0.17649281,0.2443854,-0.13445093,0.24506214,-0.31495747,-0.18867919,0.16621602,-0.13496521,0.2197251,0.16976994,0.241372,-0.000104148545,-0.12742722,-0.18938698,-0.06681557,0.15026467,0.10848962,-0.020763446,-0.2853875,0.16584569,-0.16637939,0.005860324,-0.25827026,0.13590488,-0.06478069,0.113569774,0.020338226,-0.06273681,0.6003126,0.12488735,0.25373787,0.20118657,-0.10226645,0.0040490385,-0.033763506,0.23117235,-0.054742362,0.07004656,-0.15839592,-0.12320619,-0.012865689,-1.3563545,0.12181837,-0.10812161,-0.009836848,0.06813353,0.46250302,0.10357681,-0.1733103,0.0054514133,-0.17688103,0.066292755,-0.10550716,0.14476606,0.1473011,0.13721052,-0.0032423837,0.013845806,0.051605448,-0.011634127,0.036190137,0.0099763125,-0.0048462152,0.009278277,-0.009210714,-0.26114845,-0.05660855,0.10590115,0.27957144,-0.510561,-0.33574808,0.022073474,-0.054094158,-0.10449154,0.12547952,0.12587123,0.05329982,0.10080578,0.33700934,-0.15450785,-0.09077436,0.100920066,-0.20467672,0.3297492,0.07333812,-0.018076357,0.23759791,0.11262225,-0.14798589,-0.10353324,0.038571853,0.32878137,-0.025716236,-0.19370542,0.13711073,0.86796016,0.11345426,0.041653316,-0.041762717,-0.0036385518,0.072156355,0.049402494,-0.21341547,0.20189765,0.79382706,-0.17990376,0.04049372,-0.079681225,0.08777517,-0.0009285639,-0.03675749,-0.008528066,-0.11765209,0.10262378,0.23899366,0.09337795,-0.18595895,-0.16383654,0.09432314,-0.0802894,0.0698563,0.028780485,0.24443103,0.15894732,-0.07184669,0.09959121,0.09289221,-0.16442595,0.15255597,0.28165448,-0.15180427,0.20026408,0.014911658,-0.122049294,-0.077364795,-0.14952838,0.11966172,0.18131877,-0.229968,0.0848851,0.026216246,0.052177675,0.08945657,0.24652897,0.08615827,-0.02560878,0.28845975,-0.45994037,-0.14816761,-0.09770299,0.20762745,0.31128088,-0.20889485,-0.08988957,0.057009857,0.10259836,-0.07760108,0.114317894,-0.17114747,-0.17789103,0.23421846,-0.5013117,-0.00039575272,-0.083717726,-0.1859779,-0.37632823,-0.23930754,0.14376292,-0.119590856,-0.040911667,-0.37795934,-0.3465285,0.7699732,0.032890677,-0.051226422,0.15559244,-0.17934743,0.011924483,0.118763134,-0.03692828,0.29888323,-0.4070992,-0.24379092,0.30156332,-0.0013675679,0.19868767,-0.17082055,-0.06454496,-0.036453933,-0.14338647,0.06160905,0.42774937,-0.08718566,-0.024033556,0.084150955,0.099826306,0.29914224,0.060622357,-0.009670595,0.09273976,0.052570656,-0.27305028,-0.16109952,-0.42461604,0.03329243,0.13036183,0.104319245,0.23575793,-0.07529962,0.10825394,-0.43363917,-1.385725,-0.27844247,-0.18108812,0.2126523,0.011658896,-0.09345175,-0.12527938,-0.07822924,0.03972583,-0.04651,0.13577254,0.03223557,-0.10299988,0.32071579,0.22379492,-0.017817449,-0.006775736,-0.16000114,-0.0458795,0.056864288,0.234407,-0.3654406,-0.19412412,-0.16963018,0.09361469,-0.46944925,-0.034128614,0.11502259,-0.2127874,-0.2255874,0.1223916,0.045573976,0.0071146986,-0.08726534,-0.002510501,0.06284626,0.10734921,-0.3436475,0.20653023,0.10053728,-0.0013086642,0.08845414,0.11649367,0.48646724,-0.05227745,-0.17914326,0.016734978,-0.08606832,-0.01520195,-0.034320045,0.21247576,0.24036314,-0.022755481,-0.21206553,0.17392145,0.04904116,-0.0053106705,0.17125973,0.11000398,-0.09220347,0.14150205,-0.14656109,0.013452076,-0.29593104,0.27361974,-0.42311394,0.0014697418,-0.0036764743,-0.07092447,-0.006270324,0.047518134,-0.1711443,0.21278785,0.0074115107,-0.03524383,0.114872105,0.22316477,-0.21228637,0.063511826,-0.1685344,-0.14197153,0.6401316,-0.17861348,-0.111020744]', '2026-08-16 04:22:15.551635');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (19, 18, '[-0.026207093,-0.011839279,-0.05276616,-0.24859723,0.3521321,-0.07852322,0.21561456,-0.030300505,0.23418386,-0.13508357,-0.2755274,0.17946416,0.11697548,0.1045006,0.120894834,-0.0060570287,0.18328914,0.4496825,0.19920093,-0.18272938,0.5568486,0.049591914,0.12404878,0.008104198,-0.015387285,-0.021809194,-0.09271192,0.43936342,-0.0006074832,0.19873598,-0.21451375,0.46299323,0.07522545,0.7825865,0.48798922,-0.14173664,0.0040641925,0.11644374,0.13821335,-1.3956233,-0.27285168,-0.31969574,-0.09029029,0.0028982991,-0.23615004,-0.7258128,0.21314615,-0.13335767,-0.036374025,0.44292057,-0.104716875,0.12009234,-0.03440288,-0.11141525,-0.033661358,0.21995597,0.10956401,0.020934764,0.052139733,0.31993908,-0.55801827,-0.23134027,0.037940256,-0.22262779,-0.2384233,-0.42671582,-0.038347825,0.34004068,-0.1979368,-0.042309925,-0.06647746,-0.023160907,-0.15812904,-0.21442038,0.04017135,-0.102450974,-0.0155984275,-0.092303,-0.06262147,0.21910569,0.045786653,0.014034404,-0.21823546,-0.41126657,0.11830224,0.20712247,0.4867288,0.15229979,-0.18487884,-0.124815024,0.17722292,0.20827894,-3.9137907,-0.08836191,-0.2016853,0.09798648,-0.08404035,0.034921866,0.28866896,-0.36308682,-0.17239334,0.046296924,-0.31331608,-0.19438195,0.05755004,-0.0048277993,-2.8022714,0.0006497152,-0.453505,-0.1828839,-0.09493972,0.13515125,-0.1804438,-0.27897197,0.16285208,-0.3172265,-0.12378328,-0.37730014,-0.016652344,-0.09026891,-0.3624544,0.56242967,0.30442744,-0.20489603,0.33762118,0.19256115,-0.008268813,-0.32562858,0.2562237,-0.20928703,0.0905889,0.052501082,0.058435846,0.7160466,0.06278203,0.24201499,-0.056128874,-0.0064619356,-0.022407386,0.23786613,0.1890117,-0.32660034,0.049886767,0.1514678,0.12521243,0.093933456,-0.019596536,0.4494139,0.12754029,0.15390605,0.15674864,-0.04987016,0.18888523,0.3072057,-0.13259694,-0.41406864,-0.20457067,0.001822342,0.04311046,0.53814775,0.056844693,-0.2375987,0.23390593,-0.25813428,0.21374133,-0.13822724,0.5748978,0.03738778,0.3884707,-0.021782504,0.19080845,-0.0032990184,0.22981282,-0.15710488,0.099497214,-0.061588787,-0.28061607,-0.055640657,0.211075,-0.0389754,0.17361736,-0.29369253,-0.18752366,0.0070915488,-0.041364517,0.10984555,-0.096168995,0.11159523,-0.42874807,-0.009896253,-0.2395189,0.052244946,-0.14983504,-0.09884399,-0.044392075,-0.13619521,-0.00033995573,-0.21617572,-0.0640376,-0.16197927,0.2622517,0.0970381,-0.2055875,-0.053934034,-0.14167595,-0.039610192,-0.15949664,-0.10472999,-0.49259332,0.2788712,0.18335758,-0.25595182,-0.07218799,0.13424703,0.1414222,0.24459735,-0.04973108,-0.024645142,0.67942613,-0.32011792,0.32104456,-0.4082369,0.3286396,0.20179813,-0.24818474,0.12966734,0.4418558,0.4258239,0.0093055125,-0.054186933,-0.13132156,-0.12083274,0.1635415,-0.15566222,0.17256308,-0.30126703,0.24867383,-0.03411153,-0.05292623,-0.05900801,0.38824394,0.32689008,0.2531924,-0.14674392,0.32171687,1.0863422,0.15622306,0.14321803,0.19443718,-0.0337399,-0.26716658,-0.13534424,0.3351074,-0.22744668,0.24450704,-0.06408697,-0.36330107,0.11882175,-1.3744102,0.10398205,-0.19078752,0.10849232,0.1953533,0.5848479,0.56178397,-0.16214551,-0.3129597,-0.27344373,0.09964216,-0.4037973,0.4223977,0.10000594,-0.050351553,0.22939064,-0.06205372,0.16973878,-0.123839185,-0.011834421,0.103895314,-0.14305252,-0.16835834,0.1611858,-0.3531502,0.08279931,0.119241625,0.19279389,0.024561677,-0.48724264,0.09989092,-0.26475787,-0.040634446,-0.008850293,-0.04048241,-0.030729411,-0.06532254,0.056002144,-0.3092441,0.12524228,0.17094819,-0.37639186,0.14659785,0.23237674,-0.053459577,0.10762042,0.26098147,0.15570629,-0.20367838,-0.05658748,-0.16763245,-0.066211864,-0.22903325,0.21910849,0.715372,0.24785076,0.097256094,-0.13306634,-0.06921412,0.0036429516,-0.1176032,0.3628657,-0.2674417,1.4423555,-0.1606242,-0.07756467,-0.060476348,-0.015347852,-0.06904785,-0.0874025,-0.40614355,-0.6536573,-0.094034806,-0.09843679,-0.22155735,0.029389037,-0.30491522,0.26044443,0.07172793,0.043111257,0.32903937,0.25045255,0.27047408,-0.08262577,0.24434803,0.13649991,-0.0066377847,-0.3656918,0.30177364,-0.17398717,0.33054763,0.1618307,0.04582979,-0.36578783,0.11685927,0.33734888,0.06623987,0.237536,0.030170506,0.54128885,0.3208882,-0.06161299,0.3970881,-0.10544681,0.24066766,0.71960425,-0.3434564,-0.030868659,-0.20756513,0.044627737,0.074149676,-0.05118677,-0.07201295,-0.11597927,0.0037897998,-0.05549346,0.04345304,0.06270108,0.023708295,-0.05008663,-0.8054698,-0.22050513,-0.28030512,-0.106232904,-0.27954125,-0.07824829,-0.17081383,-0.08255806,0.1260857,-0.23757249,0.14017905,0.49200836,0.13679297,-0.01385951,-0.011461981,0.03723902,0.06911843,0.17214361,-0.22606386,0.113533914,-0.30948204,-0.037328646,-0.02760758,-0.16217934,-0.17928602,-0.14443643,-0.16338895,0.040657338,-0.41603288,-0.054466225,0.47428703,-0.054757793,0.26531518,-0.035276514,0.06656345,0.091280006,0.2259805,-0.21359244,0.09283305,0.3681561,-0.33996257,-0.112331785,-0.17047004,0.16700621,0.54846835,-0.22488962,-0.25334987,-0.008393998,-0.098230705,-0.3602436,-2.3423073,-0.11342951,-0.19253375,0.1385707,-0.4532783,-0.228672,-0.21858154,0.005489337,0.2807443,0.05191169,0.06381569,0.047938973,-0.05171662,-0.04393115,-0.0678002,0.14339037,0.03910402,0.006591157,0.026438726,-0.14948747,0.4316743,0.09624126,-0.59256196,-0.40276155,0.15935765,-0.061124157,-0.026709426,0.45445815,-0.13072549,-0.1930888,0.059967518,0.23586746,-0.14313112,-0.2169002,-0.24060436,0.17730205,0.22206324,-0.12001787,0.22911155,-0.06892191,-0.014077683,-0.08455763,-0.059545327,0.0148212975,-0.06469909,-0.26080403,0.09492352,-0.024305701,-0.061612263,-0.014924679,0.09412804,0.57496405,-0.1046639,-0.11316892,0.36994514,0.027795065,0.14883578,0.35457286,-0.22536112,0.19137913,0.24725692,-0.21298373,0.22365095,-0.005914084,0.17249292,-0.115280315,0.14620402,0.26452154,0.22179145,0.065178394,0.0016979597,-0.099074945,-0.06301058,0.15713927,0.07038705,0.32595775,-0.423664,0.26088598,0.078850396,-0.253956,-0.29283822,0.3924895,-0.09850415,-0.20324582]', '2026-08-16 04:22:18.261296');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (20, 19, '[0.28821152,0.3682061,-0.09966815,-0.05489742,-0.2826575,-0.21823578,0.20822152,0.3190141,-0.6169751,0.21946257,-0.20039533,0.03188087,-0.005100333,-0.17324747,0.15981749,0.076975785,0.22266865,0.25717768,0.08084235,-0.14903438,-0.109741665,-0.076831505,-0.17582242,-0.015606823,-0.042653054,0.35448486,-0.55719715,-0.017883033,-0.16545132,-0.123126924,-0.13143156,-0.044853143,-0.60326916,-0.033981673,0.4933607,-0.07118937,0.25291714,-0.13720821,-0.09579012,-1.8110754,0.04290979,-0.11966289,-0.08193257,-0.06709447,0.38907295,0.7613947,0.022155691,-0.5377702,0.23516698,-0.025449444,0.05317129,-0.26628524,0.09701,-0.095584184,-0.111759394,-0.064153746,0.28737748,-0.39844254,0.16657819,0.16316473,-0.063414395,-0.07558329,-0.049392205,-0.14824204,-0.013681977,0.1637885,0.13626385,0.25474724,-0.2743265,-0.12533826,0.42345023,-0.09743342,0.17803441,-0.06136132,0.12549396,-0.090453394,-0.3785202,-0.26611382,0.058201466,-0.08652586,-0.20345749,0.0030378308,-0.195455,0.5471631,-0.28162026,0.01652764,-0.1370571,-0.07815083,0.40419456,-0.35858288,-0.040899392,0.012537243,-5.1424627,-0.036074523,0.0059727686,-0.16402733,-0.082635954,0.22413856,0.16416696,0.3362181,-0.2975593,-0.12875591,-0.026711682,0.09807384,0.04633909,0.52786845,-2.339672,-0.177294,-0.3789637,-0.044979535,0.11658431,-0.07720446,0.25722852,0.0880268,0.21455948,-0.31647468,-0.13872476,0.11314626,0.34056023,-0.4562446,-0.026013512,0.3527078,-0.24912637,-0.13001208,-0.1239913,0.13232651,0.05156329,0.0077705584,-0.13219284,0.15362503,0.25255775,-0.23083548,0.12444941,0.87380284,-0.09234915,-0.13670859,0.2653549,-0.3812117,-0.11347615,0.3016604,-0.008659836,-0.2734464,-0.25318313,-0.36751908,-0.26139593,-0.0201209,-0.16708538,0.44849965,0.2642949,0.30382836,-0.22251847,-0.21823357,-0.15046056,-0.5496809,0.26284736,-0.11719215,-0.1526527,0.08297572,0.223472,0.32907677,0.19056758,0.31746504,-0.32852277,-0.2622511,0.29576185,0.17932984,-0.15034312,0.041393474,-0.303862,-0.30225265,-0.28945762,-0.18615049,0.13744658,0.15745762,-0.33116052,-0.1747003,1.1213923,0.14167535,0.59491915,-0.11689737,-0.16410147,-0.11999383,-0.22078055,0.18990228,0.027879644,0.0033262272,-0.14199737,-0.12791014,-0.06773329,0.056964766,0.1878905,0.014376051,-0.277731,-0.2114413,-0.25693676,0.1534834,0.39533004,0.29831663,0.08743161,-0.24475469,0.084670715,0.30727267,-0.16449277,-0.41908404,0.053539887,-0.026073834,0.004719917,0.1789071,-0.3288785,-0.018200856,-0.5648728,-0.4409441,0.1421357,-0.075657144,-0.04411227,-0.07601949,0.0061960113,-0.052075382,-0.08588963,0.3450582,0.13834243,0.011577268,0.14477219,0.15661971,0.42673177,-0.2064377,0.3443641,0.12503117,-0.45005715,0.013421147,-0.35353577,0.3951859,-0.025434963,-0.04503055,0.32824498,-0.54283154,0.09336588,-0.15145622,-0.11510547,-0.33988318,0.17960067,0.19324985,0.44678202,-0.24821325,-0.2842644,0.6421525,0.048189104,0.002204111,0.27679008,-0.079079255,-0.027658498,0.03313429,0.16902833,0.11470389,-0.04224956,0.14130136,-0.3179802,0.43446362,0.2940988,0.14997908,0.09689172,-0.19449647,-0.17863841,1.0161079,0.3494385,-0.077829,-0.67936623,0.04563064,-0.16547854,-0.2059466,-0.10295791,-0.11887705,-0.24762255,0.08680827,-0.014101159,-0.17576605,0.09774796,-0.094226025,-0.0047806334,0.2528412,0.12081009,0.11858323,-0.015564821,-0.114078,0.22394347,0.24674176,0.6425866,-0.21932766,0.09942162,-0.15943797,-0.11556832,-0.11998355,-0.019797275,-0.08328572,0.06308202,0.032711234,-0.22127977,-0.004829235,0.21122263,0.013528256,0.24111523,0.028549572,0.085878626,0.17273079,-0.18194126,-0.22090268,0.35638785,0.5339749,0.49196985,0.01821736,-0.046023663,0.17759794,0.8735323,0.16644427,0.078168586,0.074591,0.44621253,-0.19243811,-0.046298202,0.29263633,0.06917925,0.4105192,-0.39923966,0.14691648,-0.2910141,0.22028627,-0.28403032,-0.047098987,-0.074801385,-0.0352937,0.13256347,-0.0826691,0.41481695,-0.37064454,0.2942035,0.045616373,0.155969,0.21826582,0.07276034,0.14523472,-0.24143492,-0.51602316,-0.019694181,0.4831879,0.22926588,-0.27371815,0.14969806,0.19696784,0.1389164,0.021178698,0.4367761,0.30323842,-0.16027759,-0.06767647,-0.040949613,-0.16936128,0.25320092,-0.12811966,-0.46117672,-0.13308392,-0.06300898,0.21259859,-0.062489018,0.17356876,-1.4617126,-0.207484,-0.17455077,-0.087864764,0.07956403,0.23501861,0.1526252,-0.15099636,0.45927805,0.033194892,-0.09084893,-0.1732255,-0.4169547,0.33526883,-0.48620963,-0.039253097,-0.15296796,0.2730335,0.24811654,-0.14368305,0.19412407,0.23546015,-0.052962985,0.10120689,0.16190389,0.0076774857,0.16103235,0.18232153,-0.3021063,-0.191533,-0.23181203,-0.10459603,-0.0018492399,0.11129835,-0.51740396,-0.09994502,0.42755362,0.45810294,0.26570413,-0.2800226,0.013960555,0.20869264,-0.18920226,-0.04820467,0.53349584,0.27510628,0.02699946,0.30205345,-0.30548474,0.45622915,0.025037354,-0.052774146,0.24961942,-0.058386832,-0.016020268,0.14422452,-0.38542938,-0.10880754,-0.0521411,0.3218217,-0.09343351,-0.4146191,0.3490582,-0.06882795,-1.914854,-0.07008266,-0.07187657,0.0728687,-0.033623565,0.28826606,-0.2577366,-0.3532981,-0.4592475,0.11370871,-0.20932962,-0.095925994,0.046442818,-0.15045854,-0.58972985,0.21596262,0.24189019,-0.6059855,-0.075379536,0.04815787,-0.47817364,-0.14191112,-0.41010082,0.21646014,-0.04381277,-0.11323941,0.1675383,0.6977291,-0.14506623,0.28875458,0.17688586,0.38212344,0.44241256,0.4647389,-0.2859179,-0.2241217,-0.21092121,-0.012912701,-0.12390826,-0.13870026,-0.017385129,-0.14741433,-0.0912253,-0.21520497,0.19462529,-0.06522833,-0.30991736,-0.082228936,-0.20221093,-0.055498526,-0.105394274,-0.17008218,-0.36840475,-0.5999456,0.17390057,-0.22089653,0.39092493,0.28957695,-0.043024156,-0.29595459,-0.237064,-0.14243428,0.13567269,-0.11984763,0.3176503,-0.35480198,-0.2875601,0.029530494,-0.16567343,-0.31561005,-0.050379828,-0.026582802,-0.007451571,-0.094899915,-0.27182493,0.0071768705,0.025448762,0.020331481,-0.08021622,0.34546536,0.24604166,1.058236,0.25144076,0.32418188]', '2026-08-16 04:22:20.502126');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (21, 20, '[-0.009948531,-0.0553992,0.01702416,-0.21369614,0.053054504,-0.22110371,0.04472551,0.33760816,0.46022287,0.123639375,0.035906788,-0.21004026,-0.041907694,0.07669127,-0.16663991,0.1632616,-0.54254925,0.07971834,-0.22060217,-0.17938782,0.60082924,-0.21869756,0.014797787,-0.041237585,-0.05512157,0.21525502,-0.087574154,0.27871385,-0.25718984,0.15041134,-0.07876306,0.26627344,-0.09407721,0.107970156,0.34813496,-0.055995613,0.024238294,0.19249141,-0.0066017457,-2.028953,-0.15006195,-0.22214976,-0.17496388,0.008273424,0.14059302,-0.0056018815,0.15914159,-0.06495472,0.31105366,0.11662759,0.060291845,0.1566672,0.13226743,0.1320109,-0.24333094,-0.041922275,0.31353176,0.0868978,-0.04239144,0.29380095,-0.4979091,-0.21934915,-0.08505815,-0.07242933,-0.19635512,0.11615726,-0.08025805,0.1301884,-0.18359314,-0.010208827,0.084981576,-0.032806322,0.004999521,-0.13014755,0.13053797,-0.14961721,-0.19919628,-0.20200521,-0.054542497,-0.019089513,0.03709362,-0.23671623,-0.0720387,0.031157875,0.124171466,0.13704439,0.5589674,-0.102096826,0.076162264,-0.24198638,0.050351467,0.054439507,-6.395752,0.45638666,-0.24044606,-0.031537388,0.07766593,0.07170661,0.0940816,-0.61000425,-0.12200014,0.012550018,0.09281559,0.13370103,-0.09858093,0.076111026,-2.1751492,-0.101870395,-0.018921161,-0.20538704,0.0333953,-0.13785462,0.015795391,0.041529715,-0.05534586,-0.23491244,-0.10618462,-0.032673597,0.087145075,-0.030979423,-0.10272666,0.107848555,0.052739494,0.090949476,0.111148216,0.09495794,0.23454271,-0.077508934,-0.05702888,-0.17798042,-0.14161985,-0.30124685,-0.027548013,0.85569674,-0.0099658165,-0.19301958,-0.1484981,-0.48492137,-0.28833342,-0.03950685,0.26527426,-0.12023447,0.010888362,-0.1184362,-0.0028225242,0.03443502,0.039011516,0.4646307,0.15496808,-0.027709035,0.024550056,-0.13151379,0.35885888,0.17448395,0.12166041,-0.273068,-0.20147114,0.01670404,0.11073727,0.19322596,-0.069948584,-0.15338758,0.033543862,-0.030276995,0.11836657,-0.15718094,0.7283203,0.09755581,-0.10537323,-0.33663544,-0.10073057,-0.066926636,-0.05465668,-0.054079726,-0.19862069,0.112383105,0.25487182,0.04341887,0.374666,0.10571116,0.16222535,-0.22841337,0.05373766,-0.005503215,-0.08746398,0.07663908,0.14129029,-0.11969848,-0.26866502,0.17121102,-0.055513684,-0.14777096,0.024267126,-0.08011705,-0.1405092,-0.08959952,-0.05405766,-0.20443696,-0.31517118,0.034820445,0.09744484,0.037859615,-0.1647067,-0.27779132,-0.047726408,-0.19490205,-0.046381403,0.146424,-0.2956604,0.18705434,-0.3180313,0.09688839,0.2889518,-0.1813357,-0.085460655,-0.024136074,-0.2220154,-0.14471051,0.5656957,-0.020091347,0.286434,-0.52683276,-0.040204167,0.13080578,0.046032086,0.20133379,0.4272659,0.123192795,-0.0046479097,-0.08778134,-0.22803259,0.029303113,0.23890662,-0.22593199,0.13798593,-0.20903552,0.07919016,-0.2777797,-0.033711355,-0.2271299,0.23363794,0.050738227,0.39035767,-0.24911824,-0.332359,0.30222848,-0.09462824,0.28590333,0.10987412,0.02063661,0.049587354,-0.016631559,0.10050585,-0.026221056,-0.21630485,-0.0024407574,-0.029785212,0.027174896,-1.4928062,-0.02428124,-0.097836405,0.004109541,-0.079229504,0.22454663,0.249,-0.13994333,-0.20536265,0.09474361,-0.11792644,-0.043799166,-0.107214905,0.033338305,-0.053536933,0.30939585,0.017315997,0.2547128,0.16310489,-0.077608705,-0.07941409,0.13144973,0.07891407,0.32097918,-0.0688207,-0.013863121,-0.038257863,0.044588555,-0.466388,-0.21195094,0.09589415,-0.11290913,0.0778003,0.16885269,-0.27354354,0.060927197,-0.16797322,0.12701933,-0.2749168,-0.039752644,-0.028000033,-0.24423143,0.05705644,-0.15127589,0.09208435,0.08509651,-0.068596564,-0.10292415,0.0347373,0.111807235,0.12245868,-0.0818648,-0.09895222,0.2319545,0.8549891,-0.14957245,-0.12411666,-0.06119872,-0.054088708,0.050834347,-0.33097854,0.07216026,-0.06380496,1.4838513,-0.1620438,-0.1439855,0.0099789705,-0.10715773,0.038549073,-0.17447266,-0.14553013,-0.17146394,-0.048804857,-0.021382706,-0.12904322,-0.052992478,-0.27618644,0.08729586,0.21731971,0.06414021,0.065282375,0.15851831,-0.0055059996,0.06706899,-0.177403,0.16537882,0.19029579,-0.13005179,0.046097808,-0.119567804,0.3691864,0.04813559,-0.10071189,0.008825207,-0.055147965,-0.04902894,0.029787946,0.21288913,0.10437396,0.62403756,-0.054615133,0.122081034,0.1772501,0.020541085,0.2594344,0.1962329,-0.52375007,-0.1089724,-0.23146184,-0.05219039,-0.092970125,0.0010394613,0.044680573,-0.03063479,0.11943125,0.030677548,0.17979473,-0.17412402,0.1748931,0.27685025,-0.80917394,-0.088729486,-0.44994807,-0.07715537,-0.10072366,0.0694829,0.020119542,0.04550853,-0.044775773,-0.20720495,0.0011872126,0.6786258,0.1727917,0.008672861,-0.054179322,-0.029364202,0.13456857,0.1374361,-0.035767842,0.0065622614,-0.4340442,-0.22249556,0.21326755,-0.004307904,0.11292536,-0.3701014,-0.053121388,0.05656343,-0.14586447,-0.14544569,0.5861397,-0.1148939,0.5447949,0.07259117,0.06610435,0.27348736,0.02008932,0.058030304,-0.078481026,0.20063059,-0.17247602,-0.16489515,-0.35231733,-0.13079394,0.11641353,-0.65493876,0.008651233,-0.050332468,-0.00060248334,-0.08597928,-1.9516784,0.08282682,-0.04679012,0.15043986,0.03609017,0.023495033,0.054929517,-0.23972887,0.08140352,-0.28913522,-0.07748065,0.018445354,-0.04764269,0.18658465,-0.027055772,0.06807493,0.14945424,-0.16114058,-0.20358464,-0.13475588,-0.016656982,-0.069954485,-0.04773086,-0.008912282,0.17680599,0.27557003,0.09993746,0.18419757,-0.035141967,-0.13199632,-0.06922403,0.037101466,-0.020975044,-0.08463354,0.032590024,0.003180752,-0.114038065,-0.17413919,0.47728673,0.011009996,-0.010757219,-0.15464815,-0.0014427233,0.18203439,-0.2179541,-0.39158535,-0.042145558,-0.07319934,-0.10550411,-0.016017878,0.005190044,0.27864787,-0.062169228,-0.34294632,-0.024146495,-0.021345463,0.2090752,0.2158515,-0.032296833,-0.21867315,-0.0828481,-0.24064381,0.20004126,0.015922997,0.07602172,-0.0267247,-0.1974597,0.20641685,0.04154029,0.004777085,-0.2899997,0.038330037,-0.017151086,-0.10470725,0.1429883,0.11345376,-0.18509117,-0.101789065,-0.16087513,-0.0039105136,-0.008310695,0.29185736,-0.02291973,-0.16557671]', '2026-08-16 04:22:23.441398');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (22, 21, '[-0.019022169,0.03982663,0.05566867,-0.069380015,0.038787402,-0.24312754,0.14254723,0.19112353,0.35093486,-0.057919655,-0.07329845,-0.07723174,0.0447086,-0.016865399,-0.11409999,0.056941092,0.16155456,-0.06128033,-0.048826773,-0.17401768,0.64973474,0.10426034,-0.11449105,-0.033201434,-0.20676517,0.15384622,-0.19997635,0.09096432,-0.11908479,-0.043841846,-0.036710694,0.16719536,0.010373562,0.24575333,0.3726762,-0.119981736,0.16376586,0.18800457,-0.035634838,-1.6007143,-0.13989007,-0.27256748,0.065366454,-0.32551342,-0.040099528,-0.045957334,0.04973562,-0.055310916,0.059106544,0.0744423,0.20612697,0.2682886,0.14439696,0.046282414,-0.33070958,-0.055010978,0.26374868,-0.15628502,0.04083013,0.3716173,-0.46517184,-0.04290786,0.015009499,-0.23529464,-0.15995826,-0.08009083,-0.2415707,-0.035969116,-0.3489937,-0.023535281,0.2904519,0.047893144,-0.10966333,0.20352355,-0.051495887,-0.12540178,0.07535837,-0.061493125,-0.030882536,0.029002488,-0.28405452,-0.1592279,0.20842753,-0.33064035,0.036786254,0.4232285,0.372478,0.3228304,0.07129272,-0.2923212,0.061999835,0.11745665,-4.330695,-0.08812644,-0.16892911,0.084564425,-0.02892573,0.10018132,-0.04828317,-0.47860578,-0.07986419,0.012060531,-0.26063263,0.008881806,-0.09080473,-0.0057602646,-2.4459405,0.068358935,0.036431756,0.06308261,0.04151841,-0.0218564,-0.007535498,-0.27365914,0.11583097,-0.1794236,-0.17678651,-0.14117298,-0.042035956,-0.20387128,0.054519847,0.20693986,-0.0035083755,0.18004791,0.011072545,-0.009767346,0.0796448,-0.091445796,0.10737502,-0.25321427,0.35315472,-0.18992387,-0.02871547,0.7660142,-0.011560964,0.04178304,-0.015256783,-0.41036543,-0.07739329,0.24963664,0.08328065,-0.07037548,0.21201147,-0.048725855,0.03348617,0.13465673,0.18964511,0.4951477,-0.16513203,-0.02964569,-0.047317047,-0.39888847,0.18823098,0.29478034,0.05882862,-0.32270533,-0.15685926,-0.037099857,0.14559972,0.4024596,0.046357278,-0.23488283,0.07219736,0.048650417,0.10623645,0.07027804,0.53736854,0.24429642,0.15051818,-0.09127861,-0.046591304,0.0029751286,0.19247101,-0.43214273,0.21789117,-0.049938288,-0.0022303613,0.0004627314,0.21073721,-0.08653751,0.108039655,-0.2769687,-0.2626964,-0.21857792,0.15566957,0.093421735,-0.008027554,0.011358676,-0.00026135053,0.06610252,-0.16914383,-0.03671018,-0.0021596558,-0.14891529,0.021114038,-0.06773074,0.014235846,-0.007100992,-0.16115026,-0.031360768,0.21500996,0.11261288,-0.1492739,-0.3812398,-0.20543763,0.1584082,-0.041735183,0.19608906,-0.19116336,-0.009832134,-0.1349346,-0.20793733,0.28671214,-0.1928576,0.06191388,-0.18449637,0.08310453,-0.45299765,0.6472435,-0.16765866,0.41684812,-0.43079963,-0.077800944,0.1004281,-0.04149051,0.23201056,0.29976428,0.25031143,-0.056567393,-0.15532808,0.092597514,0.06828634,0.09111244,-0.035353534,-0.05399117,-0.17258015,0.23714277,-0.15690367,-0.022150239,-0.33253807,0.16378273,0.14835782,0.10515864,-0.094549134,0.06669849,0.771662,0.07835817,0.2218063,0.05078923,-0.12722784,-0.24179816,-0.09509487,0.012948931,0.14485627,-0.031963654,-0.11217394,-0.198571,0.14322837,-1.2315962,0.23626883,-0.17827614,-0.10697303,0.073044196,0.22320944,0.24219511,-0.19050772,-0.12669586,-0.116888314,-0.06868225,-0.010552385,-0.041598555,0.3171962,0.0939914,0.19282535,-0.18284066,0.18202847,0.09207032,0.29998043,-0.23853576,-0.09276127,-0.14141938,0.1891589,-0.084522806,0.020135047,-0.048346482,-0.078643784,0.30702528,-0.23270735,-0.20960723,-0.19525318,0.096145466,-0.011721215,0.10667396,0.15809566,-0.10068373,0.27879623,-0.21796201,-0.028137827,0.00424871,-0.16193363,0.12852906,0.047497213,-0.05113489,-0.22852951,-0.019332297,-0.2896814,0.19126052,0.17457938,0.035859413,0.11264917,-0.19746184,0.025286274,0.7647666,0.0842442,0.15461348,-0.3617744,-0.100111365,-0.06372775,-0.38688955,0.34641013,-0.06923793,1.1153067,-0.07083485,-0.36562914,-0.35905993,0.004191173,0.049969036,-0.06833289,-0.26470003,-0.43072754,-0.040986136,-0.052137427,-0.17000514,0.051615443,-0.16503607,0.41892272,0.26757154,0.040395513,0.17874166,0.38283393,0.10868458,-0.15288384,-0.042773955,0.19415514,-0.15726389,-0.13175094,0.048280377,-0.08106543,0.09883405,0.056978177,0.03965411,-0.0512919,0.035087433,0.30172879,0.08922569,-0.023743989,0.27171484,0.44363427,0.2579268,0.11991555,0.19820344,-0.05628117,-0.039646495,0.5948961,-0.5156846,0.032566857,-0.2859628,-0.21632068,0.20689233,0.0878547,0.11342021,0.16638726,-0.11247687,-0.17073116,-0.10764421,0.2112657,-0.110894226,0.07329828,-0.6429527,0.04743818,-0.31203666,-0.07647957,-0.35896984,-0.16304679,-0.052163538,-0.1901868,0.17778388,-0.14639544,0.09634889,0.44853795,0.0880578,-0.055504672,-0.055656746,0.26015547,-0.004864305,0.10677125,-0.25644338,0.29227507,-0.46881112,-0.2034747,0.2560643,0.19145109,0.1482514,-0.2360118,0.16751237,-0.17541546,-0.33022976,-0.14605711,0.53927094,-0.0767294,0.066330686,-0.11997043,0.16982947,0.16748618,0.22919786,-0.17255417,0.21115434,0.42452103,-0.37641102,-0.17964835,-0.345281,0.028928844,0.3308725,-0.20633371,-0.060198173,-0.47409454,0.07322605,-0.27567324,-2.2144473,-0.1552245,-0.14096625,0.011134294,-0.3849298,-0.00701336,-0.086413465,-0.25647944,0.101538755,-0.03782578,-0.08289096,-0.0795014,0.11903009,0.1164417,0.17030373,0.0655629,0.06504805,-0.014943339,-0.06453261,0.14352208,0.27036163,-0.10943486,-0.3667534,-0.071433835,0.27807447,-0.08957345,0.051507063,0.39158896,-0.022231117,-0.19692084,0.0714712,0.11717159,-0.07567543,-0.15273978,-0.13320845,-0.08448224,0.16241,-0.21571667,0.24746361,0.037608236,-0.03489461,0.07066177,-0.10002145,0.25619504,0.10570257,-0.48195228,-0.013231811,-0.23906653,-0.15451564,0.10062211,0.065407954,0.19012968,-0.12605804,-0.4596053,0.2431665,-0.11492601,0.079499975,0.24038038,-0.04917617,-0.08790196,-0.11963371,-0.130282,0.057405706,-0.013761911,0.19255394,-0.17634507,-0.053927377,0.13447689,0.16573994,0.056276713,-0.14754677,0.08232716,0.14287801,0.12801452,0.024366166,0.059760075,-0.3612749,0.021254541,0.27948314,-0.033311553,-0.003553743,0.42079312,-0.076469585,-0.15752329]', '2026-08-16 04:22:25.443742');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (23, 22, '[0.13686442,0.015016153,-0.29045662,-0.23725218,0.051177517,-0.12646367,0.060695343,0.20923269,0.107641675,-0.1033765,0.19127432,-0.11274198,0.5446815,0.2853905,0.36971095,0.059688065,-1.1328413,0.22599013,0.41455138,0.06260104,-0.3754434,0.14991653,-0.039275054,-0.061934534,-0.45611456,-0.02711961,0.09636679,-0.088487245,0.18760714,-0.29237255,-0.0832759,0.24973918,-0.17244636,-0.07115362,0.15943092,0.49959242,-0.21050517,-0.051592153,0.033209465,-1.1611019,-0.38717812,0.06724995,0.0398424,0.118245184,0.13118725,1.3738928,0.05220458,-0.09288606,0.45012257,0.0072159166,0.86795807,0.13057193,-0.095684886,-0.10443424,-0.18179877,-0.15353093,0.5325278,-0.29711252,0.06915266,-0.42577323,0.5482455,-0.6945859,0.13200034,-0.21722296,-0.16944145,-0.24882652,0.3430747,0.7472317,0.05628985,-0.22505233,0.08767404,0.5731742,0.21386828,-0.27509946,-0.04353091,-0.22010115,-0.010551995,-0.41296607,0.14483818,-0.051932532,-0.0088025555,-0.28715822,-0.20529741,-0.0053292625,-0.033558358,-0.21585876,0.44658655,-0.42600104,0.5624526,-0.18799283,0.046291284,0.105565675,-5.880609,0.01860638,-0.19822003,0.12635638,0.029691998,0.24567579,0.30847082,0.9027041,0.08841461,0.29600027,0.105676085,0.18449202,-0.14870356,0.20013933,-1.0697607,0.18769847,-0.11150667,0.02863985,0.18423685,-0.69427526,-0.108684726,0.084294416,0.17743242,-0.0025119877,-0.28704676,0.18803486,0.029227914,-0.25276968,0.23181018,0.19239932,-0.45851237,-0.16501698,0.12612005,0.04878306,0.13662907,-0.20439383,0.23274654,0.012412573,0.19492462,-0.18878579,-0.024843283,0.87012804,-0.043506566,0.18866076,-0.119503066,-0.38979894,-0.60763156,-0.063062124,-0.18196051,-0.01888299,0.4780229,0.23853442,0.16917515,-0.04536483,-0.03328303,0.19263358,0.06850685,0.41782716,0.4903963,-0.27427462,0.48127705,0.2693264,-0.088981494,-0.46488717,-0.2714131,-0.33048075,-0.33677682,0.015649615,-0.56763333,-0.0045045377,0.2020076,0.104335904,-0.21526659,0.31841457,0.33725402,0.15583082,-0.23126177,0.12442977,-0.02995598,0.37778986,-0.24492338,-0.19996822,-0.28224233,0.03474011,-0.17883918,0.18071045,0.3696215,0.077421896,-0.28109843,-0.3462919,-0.31256667,-0.094434336,0.24979852,-0.10705539,0.017995568,0.21971937,0.16882892,0.16675803,0.08243128,-0.03752909,0.09483307,-0.17351532,-0.29473114,0.1869276,0.04622658,-0.029689228,-0.19432904,0.02949465,-0.04985937,0.0024672146,-0.011625509,-0.17580956,0.069544524,0.110918164,-0.4208202,0.042664394,-0.24954708,-0.02997477,-0.77520716,0.5931843,0.14754333,-0.18996157,0.20363721,0.037442688,-0.065406926,0.14647534,0.21743754,0.12163141,0.36130825,-0.014921309,0.14247319,0.330846,0.34202307,0.0031581004,-0.01116435,0.08385718,0.41820428,-0.09192527,-0.44778237,-0.08127112,0.08612639,0.4105385,-0.05618052,0.017860198,-0.12619935,-0.20652582,0.0020807555,0.12524171,-0.38940266,0.3661369,0.3957777,-0.14205474,-0.42828643,0.32278234,0.1163207,-0.51699543,0.04483718,-0.28700265,-0.10644479,-0.37767762,-0.20219377,0.2934302,-0.29290465,0.0057254797,-0.10634606,0.1863264,-0.3848607,-0.11406165,-0.1483077,-0.023691885,0.399925,-0.37122664,0.05626061,0.0034405554,0.23948422,0.14439121,0.26741576,0.13575852,0.16224179,0.1677569,0.02302998,-0.56057584,0.041656196,0.18265314,0.05330037,-0.20662555,-0.4147361,0.12565342,0.12019218,0.15389463,0.16357222,0.03199548,-0.07850956,0.29970962,-1.1144847,-0.106420696,-0.20630375,-0.22099839,0.28469592,0.055108912,-0.065768674,-0.29747862,0.037900366,-0.1039375,-0.15524018,0.41987795,-0.3049085,-0.083578795,0.20859581,-0.021246525,0.19468878,-0.06767209,0.28578368,0.19312811,-0.39808658,-0.06397922,0.7253443,0.6248761,0.010734477,0.07074974,0.87112105,-0.3818324,-0.010890261,0.15333354,0.39132753,-0.5825361,0.08807553,0.8974858,0.30088457,0.31194448,0.11084229,-0.040956855,-0.548123,0.073573,0.23897475,-0.5302604,-0.20291238,-0.46686292,0.1688303,0.22838794,0.27624762,-0.25870982,-0.15619653,-0.2523995,0.105010994,0.26630473,-0.13119958,0.116895065,-0.05530807,-0.20189327,-0.05578917,0.12568042,-0.1217883,0.08830148,-0.24398218,0.06928431,0.17588115,-0.18989737,0.7826925,-0.16198328,-0.33189702,0.080647275,0.054477744,0.18211451,0.23572998,0.07091442,-0.17193408,-0.06716134,-0.26046443,0.083431326,0.055429414,0.9282829,-0.7751264,-0.12668306,0.017307347,-0.98559815,-0.032430552,-0.07068008,-0.040588062,-0.054366592,0.01089241,-0.11588027,-0.25017,-0.14388108,0.15555245,0.12689684,-0.5276385,0.16687693,0.3015574,0.24757631,-0.2022665,-0.1834201,0.15414676,-0.30020183,-0.13876945,0.18276908,0.6997688,0.6806622,-0.7032628,-0.0409161,-0.0681808,0.07898245,0.17402011,0.14764068,0.172592,-0.050870482,-0.028750615,-0.11380661,0.45363712,-0.04442261,0.6407476,0.24576212,-0.24680778,0.08658785,0.004135354,0.033765547,-0.10000746,0.06879843,0.058946375,-0.01955784,0.07025328,0.51125795,-0.033606164,-0.3517119,0.12450728,-0.23067774,-0.29073575,0.6817497,-0.18696256,-0.061907563,-0.24481884,-0.12720989,-0.05256877,-0.03022154,-0.40531915,-0.0002062144,-0.7031389,-0.059391614,0.0110528255,0.09099602,0.532742,-0.4027476,0.07513336,-0.36942393,-0.17795423,0.06484029,0.10926825,0.032695193,0.041557807,0.9397279,-0.1797081,0.10059313,0.15252802,-0.071393,-0.029632421,-0.29884157,-0.06213567,0.06403895,0.7769031,-0.12288951,-0.1321275,-0.05152239,-0.09771383,-0.101127364,0.022342598,0.03514941,0.16483553,-0.4169708,-0.07751168,-0.08042496,-0.11104969,-0.033716336,0.16315767,-0.21303795,0.010594044,-0.116607584,-0.0028998381,-0.26962364,-0.09173144,-0.08564102,0.043405082,-0.36493927,-0.43387428,-0.03550748,0.108610526,0.22153054,-0.093353614,-0.015738724,-0.20325801,-0.24685933,0.118290864,-0.008419011,-0.15622517,-0.16464218,-0.22775723,0.07391626,0.0472552,-0.32433596,0.5054488,-0.22708918,0.40418565,-0.32119876,-0.37441137,-0.07841355,0.24245627,-0.027625076,0.26940712,0.01958244,0.09488833,0.20875795,0.21244934,0.10456964,1.2236675,0.08471098,0.14486714,0.11704185,-0.037859254,-0.0042000464,0.0030441675,-0.21012172]', '2026-08-16 04:22:27.561885');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (24, 23, '[0.22322994,-0.2567361,-0.07702693,-0.29806587,-0.15651698,-0.28572917,0.2825613,0.44872054,0.2630656,0.2478105,-0.05782124,0.20668784,0.16118133,-0.011185453,-0.03301508,0.10976674,-0.073334485,-0.05904936,-0.26718128,-0.16433021,0.01949259,-0.3421221,-0.14387684,0.16803548,-0.08358345,0.20477022,-0.30765072,0.077814214,-0.0782828,-0.3548268,0.36305028,-0.038697388,0.13667406,0.24115603,0.25807595,-0.12302332,-0.01578399,0.03257751,-0.16716975,-1.7804509,-0.058689233,0.21887858,0.052626166,-0.07679847,-0.18964812,0.06786615,0.01711048,0.046615817,-0.3792902,-0.21477138,-0.2160821,-0.25446522,-0.16301031,0.058958657,-0.207096,0.09472525,0.01563219,0.17429039,-0.18720014,0.17392369,0.32267407,0.07370195,0.09637918,-0.032888234,-0.06138994,-0.095718294,-0.09578483,0.06346008,-0.09202614,-0.18539329,-0.13937543,-0.007479907,0.1909942,0.23982994,-0.0919001,-0.32402143,-0.13093701,-0.10466307,0.035859667,-0.2101991,-0.2713202,-0.024754241,-0.10166968,0.5748412,-0.2288061,-0.11006282,1.0859687,0.09961163,0.32148692,-0.26450947,0.22132006,0.14125942,-7.0318995,0.2710548,0.159918,0.26054165,0.19917195,-0.32817262,0.01838766,-0.46643385,-0.07413977,-0.012142895,0.17936432,-0.24039192,-0.17922328,0.024299681,-2.3235347,0.26064816,0.17897817,0.030397708,-0.036555413,-0.13614096,0.09142243,0.1449804,-0.20134117,-0.08520504,-0.04992141,-0.56002206,0.26813978,-0.00265182,-0.2066344,0.09197299,-0.21445748,0.37037447,0.056243878,0.37974253,-0.24593493,0.16351907,0.26157233,-0.0019909232,0.18609963,0.25372803,0.13101467,0.9317917,-0.13154517,0.083233155,-0.31705025,-0.096951395,-0.3590548,-0.13214622,-0.026102435,-0.2983084,-0.15026131,0.15120372,-0.36638856,0.25332794,-0.3391181,0.23215519,0.1380366,-0.19335264,0.1449239,0.21568005,0.43856558,-0.304875,0.17388839,0.050062314,-0.4232314,0.016065136,0.22621633,-0.14938869,0.023166312,0.04922245,-0.12958223,-0.09073245,0.25249976,-0.07395446,0.028267816,0.18509325,-0.26385394,-0.08316749,0.06714516,0.28474647,-0.059022248,-0.27229384,-0.1835107,-0.15886447,-0.3753369,0.16018872,0.30252957,0.39633107,0.06651352,-0.3427224,-0.31704438,-0.057794422,0.08976113,-0.26380995,0.32895094,0.49333072,0.24860011,0.21121798,0.19573493,-0.013653069,-0.08522994,-0.028044716,-0.08130269,-0.059323244,0.043193623,0.3952254,-0.065211706,0.35086316,0.12929858,-0.00089868065,-0.008039274,-0.50634795,0.21914123,-0.2071736,-0.12258504,0.42096928,0.0018744903,-0.1078305,-0.3635053,0.358206,-0.14765108,-0.19681151,-0.029729351,-0.071787745,0.22401296,0.2601771,-0.65165675,-0.25812024,0.24635144,-0.38087627,-0.0015858981,0.13623339,0.0012036916,-0.0988886,0.008253041,-0.27663168,-0.115107484,0.18004572,-0.09871399,0.26796946,-0.063897684,0.036459856,-0.050051916,-0.12857163,0.11323775,-0.51667446,-0.3755179,-0.30031288,0.0052982033,-0.0105614895,0.16494696,0.13227813,-0.020753186,0.45366982,0.1788714,0.41011733,0.36935654,0.24000902,0.089507736,-0.31661934,0.072390854,-0.07977768,-0.000373226,-0.11527522,0.06634364,-0.27497855,-0.9659132,0.14027534,-0.38873065,-0.20232132,0.16430381,0.15336804,-0.118110254,-0.023227517,0.10823537,-0.3331431,-0.2428687,-0.06139263,-0.35390082,0.07760532,-0.028968701,-0.12818323,-0.16117968,-0.055496287,-0.25803155,0.2355575,0.15413551,0.1706305,0.24893923,-0.00687445,-0.3134285,0.0663612,-0.13599448,0.38486522,0.062825896,-0.29144725,-0.100037515,0.014236225,0.52529883,0.014238337,0.124738604,-0.041220702,0.15684393,0.38278276,-0.46331698,0.06480267,0.025462866,-0.025920963,0.12868604,-0.42127132,-0.32558474,0.123230696,-0.025484871,-0.45835528,0.1847843,0.2246105,0.13620456,0.22162715,-0.20515154,0.23275332,0.93112195,0.030796276,-0.03840323,0.14946881,0.10198625,0.07685024,0.03814987,-0.28129905,0.29610342,1.4890558,-0.077207975,0.39274186,-0.17971571,-0.06183009,-0.044843893,-0.2748301,0.33477503,0.24452399,0.06454338,0.38267434,0.36853155,-0.2648864,0.02386519,0.15570657,-0.079176754,0.24797697,0.14484161,0.36207694,-0.005194885,-0.0471179,0.24055845,0.052377116,0.032889098,0.25406826,0.08994527,0.04109184,0.057614125,0.084796675,-0.21785372,-0.113472044,-0.10724149,-0.07480422,-0.13395876,-0.16448873,0.22118357,-0.5024086,-0.10914047,0.33316618,-0.41814414,-0.08560549,-0.027293459,-0.22675695,-1.0468857,0.18465354,-0.28976718,-0.35850522,0.3348803,0.25847736,0.06732427,-0.081946656,0.49262184,-0.042306755,0.32170838,-0.4916959,-0.42898953,0.1215365,-0.47425482,-0.08002282,-0.31537172,-0.20185691,0.04791836,-0.32091013,0.305692,0.025218919,0.06531148,-0.3770141,-0.0061824657,0.32367265,-0.24185665,0.07499839,0.16770379,-0.43756035,0.15323581,0.060713466,0.039327,0.17624405,-0.38652933,-0.37658155,0.34690094,0.2173098,-0.11233408,-0.20901921,-0.0485208,0.051141262,-0.10146339,0.06168816,0.55498594,0.018637486,0.12103269,0.0004647683,0.3510016,0.1346426,0.097932965,0.33813164,-0.30452326,-0.19924593,-0.105384484,-0.15972906,0.100183584,-0.40546665,0.049692713,-0.23491669,0.31387076,0.08972582,-0.1947413,-0.08224635,-0.7206179,0.1906245,-0.0018189251,-0.15013117,-0.23102857,-0.049334303,-0.40220964,-0.37496,0.12554905,-0.20837466,-0.09853204,-0.20536312,0.11066754,0.23059313,-0.30575767,0.17958842,0.33498868,-0.30179802,0.016106911,-0.161414,0.110734686,-0.094580926,-0.0015588488,-0.12903486,-0.20419082,-0.5011335,0.18958367,0.1154568,-0.33854,-0.07242144,0.10246292,-0.025408827,0.16293953,-0.042444423,-0.088630386,-0.022592397,-0.23989776,0.09543622,0.4864069,0.12694488,-0.053532504,0.07803696,-0.113837846,0.25988898,-0.11139268,-0.21856615,-0.11269186,0.077186175,0.08922728,-0.008669037,-0.0019665302,0.102614276,-0.21127479,-0.25610062,-0.3637194,0.16256407,-0.15274693,-0.038346075,-0.068117425,0.021290962,-0.051551335,-0.3066726,0.22635505,-0.15082932,0.025212107,0.030281616,0.11907458,0.14641389,-0.025898412,-0.1576388,-0.017704269,0.1408256,-0.4969698,-0.074041344,0.20825113,-0.17500076,-0.003253411,0.009332334,-0.113489024,0.09130747,-0.106169835,0.51674414,-0.18006359,0.21109885]', '2026-08-16 04:22:29.78142');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (25, 24, '[0.011546099,0.4037223,-0.32852235,0.22345261,-0.04849862,-0.30721572,0.11482818,-0.10370165,-0.12370245,0.26772156,-0.15893766,-0.08975419,0.011182576,-0.24948981,0.29560846,0.205442,-0.042619396,0.15393402,0.27076888,-0.27892816,0.37767184,0.09607117,0.16555186,-0.045832757,-0.15967211,0.14059648,-0.15155333,-0.1225024,-0.053117175,0.13999516,0.0082735205,-0.07703058,-0.4018349,-0.061004173,0.5976573,-0.10579549,0.02195403,0.025031012,0.09242224,-1.430417,0.19892317,-0.12371306,-0.13611236,-0.31937447,0.35380954,0.6393261,-0.010508815,-0.48830187,0.2030625,0.012976182,0.11313063,0.0807217,0.19203977,-0.08142132,0.100694545,0.25272772,-0.011118122,-0.11688311,0.0903671,0.48877072,0.0485481,-0.12313172,-0.0832618,0.011331723,0.033871103,-0.035974402,0.1595594,-0.16587344,-0.07815103,-0.22080128,0.07950619,0.07174901,0.06429863,-0.22500955,0.22050293,-0.021297855,-0.12366765,0.24505122,-0.05604155,0.06315712,0.05409987,-0.035033744,-0.10013772,-0.017908325,-0.0356419,0.2587178,0.19949749,-0.18496794,0.21600094,-0.13710526,0.080844074,0.20977217,-4.521934,-0.3557192,0.18304762,-0.060077395,-0.14225574,0.13972043,0.2728283,-0.29642496,-0.15981203,0.05953601,0.05295053,0.20715886,0.11381078,0.4785519,-1.7604492,-0.05245199,-0.24688272,0.12576334,0.1723357,-0.1851684,0.16152091,0.0036834741,0.036961053,-0.12903053,-0.14155142,0.01636993,0.07292899,-0.21462135,-0.0694701,0.4131478,-0.19523644,5.003668e-05,-0.07104125,0.0052128755,0.2963775,-0.12977226,0.06127752,-0.21013652,0.08094831,-0.33427095,-0.06750621,0.8369754,0.16163652,0.0072474997,0.3873776,-0.41354772,-0.18359387,0.25682014,-0.087908424,-0.13408834,0.0785603,-0.2837395,0.13976267,-0.15138806,-0.06844764,0.27430686,0.079126865,0.151086,-0.3609388,-0.40317628,0.13027267,0.09185807,-0.09361481,-0.060522143,0.2902025,0.24106082,0.044119194,0.14030729,0.28746933,0.07682773,0.030923821,-0.029112972,-0.09225008,-0.07043585,0.54382956,0.45117635,-0.11229187,-0.19138172,-0.24080408,-0.22024664,0.06467485,-0.07870131,-0.049326472,0.13031541,0.35705474,0.0849024,0.21267444,-0.0385863,-0.04249351,-0.12362992,-0.24734329,0.11158683,-0.047437254,-0.05508209,-0.03470886,-0.26209822,-0.18165416,0.15481964,-0.16655713,0.08540642,-0.11225425,-0.38343552,-0.13874091,0.040199663,-0.02106977,0.071746916,0.055234246,-0.14750502,0.5984266,0.21836555,0.17329736,-0.12233209,0.1157124,0.22577727,0.24222285,0.09135786,-0.23637794,-0.08121373,-0.80591714,-0.029800592,0.30129683,0.0980172,-0.23600161,-0.16256553,-0.1029811,-0.37200114,0.6629962,-0.085941255,0.19093302,-0.26152214,-0.10880258,-0.037056174,0.22563817,0.0099813035,0.23053123,-0.099994056,0.19573087,0.100053154,-0.20875949,0.05469817,0.07911442,-0.049204417,0.37858427,-0.5173975,-0.17556684,-0.18446662,0.045902666,-0.4168821,0.16102287,0.1298533,0.5283546,-0.07626586,-0.4096297,0.5078758,0.19036794,0.29418883,0.18839219,-0.09753429,-0.07728765,0.27748632,-0.01239513,-0.22045808,0.10961766,0.2513075,-0.015123645,0.24764068,-1.1862466,0.12657128,-0.107576706,0.08721883,0.04725407,0.85360986,0.21545783,-0.028112246,-0.28657213,0.16244353,-0.13637014,0.04801979,-0.1479528,-0.019794775,-0.1771358,0.123342484,0.090733714,0.13797817,-0.030635135,-0.15136845,-0.045202397,0.12160909,-0.060482215,0.08673345,0.11619196,-0.05483397,-0.10549733,-0.15181063,0.14321572,0.008421873,0.10354607,0.06260517,-0.1588909,0.2322931,-0.23414263,0.13898908,0.0054392023,-0.02856206,-0.26866984,0.15183328,0.12262579,-0.1357603,-0.072803274,-0.072824255,0.12658417,-0.074123725,-0.03871401,-0.25450712,0.18810126,0.33040434,0.30180734,-0.18660606,0.19845575,0.15624183,0.8359714,0.3333216,0.1702185,-0.064801104,0.31210938,0.10463198,-0.11616778,0.29779255,-0.3547622,1.2049205,-0.16505785,0.13856424,-0.08835671,0.12216676,-0.32866448,0.025697565,0.20731175,-0.37518513,0.002331865,0.06304498,0.07937271,-0.14903918,0.2922602,0.24858876,0.2743349,0.016559077,0.038984425,0.08337652,-0.24161534,-0.3278829,-0.059440598,0.4076084,0.2973167,-0.017871458,0.045512423,0.1768284,0.6001116,-0.12210004,0.39861402,0.12758984,-0.1242205,-0.12892917,-0.19584484,0.025491895,0.07598694,0.50145584,-0.022791985,-0.42230764,-0.050043866,0.24705788,0.071500264,0.31489316,-1.1546576,-0.31638294,0.0409182,-0.07731382,0.15350801,-0.20034021,-0.16180038,-0.13389438,0.11498486,-0.04711223,0.10218149,-0.053698402,-0.048131976,0.36757788,-0.66286016,-0.2859983,-0.15004691,0.34884325,0.034166593,-0.1639795,0.10016118,-0.06845196,-0.10324934,0.083567046,-0.0059278132,-0.13784091,0.042953186,-0.09208938,-0.3072022,-0.15667145,-0.19751321,-0.08592902,-0.04922019,-0.1342206,-0.16144858,0.047978632,0.041761566,0.14691767,0.2151627,-0.14809616,0.04858561,-0.03747214,-0.38844937,-0.2834652,0.6192936,0.30672947,0.2336181,-0.065604895,-0.18464583,0.5336326,-0.2159114,-0.23119639,0.12972564,-0.17176147,-0.23287338,0.10914815,-0.37229574,-0.11052724,-0.14767078,0.3028201,-0.31552738,-0.33215117,0.1360154,-0.20157431,-1.2251998,0.167479,-0.20513661,0.100429535,-0.005563686,0.24676707,-0.30200547,-0.2777597,-0.046345085,0.16175567,-0.19056185,-0.07937053,0.2801231,-0.11265817,-0.07549715,0.17535685,0.21984956,-0.25004488,0.03820342,-0.05938989,-0.3781567,-0.34627104,-0.34072506,0.2364352,0.21153904,0.17161259,-0.043670725,0.12847751,-0.01912492,-0.20377035,-0.21016048,-0.04674952,0.32453066,0.029447038,-0.18711254,-0.24183893,-0.15994035,-0.18555291,0.27718878,0.17138997,0.00034035757,-0.3771657,0.06533721,-0.0026611376,-0.18750438,-0.16868612,-0.32268625,-0.21399644,-0.36703572,0.011541249,-0.036960617,0.18418705,-0.38431433,-0.33078957,0.17835371,-0.6569862,0.24135329,0.1885202,0.1501242,0.005533681,-0.06181673,-0.08003949,0.12351702,-0.18805774,0.24220829,-0.41356653,-0.17146035,-0.0042296234,-0.05816752,-0.114840664,-0.14011766,-0.43998697,0.0687701,0.09410709,-0.0358602,0.23149985,0.024643453,0.071544714,0.09606066,0.46924695,-0.08328301,0.3880315,0.018264346,-0.16422115]', '2026-08-16 04:22:31.821072');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES (26, 25, '[-0.3478613,0.15883319,-0.13767059,0.15227318,0.07747965,-0.35242954,0.2774959,0.03815617,-0.21936348,0.34596688,0.20116256,0.21418965,0.51058406,0.12287728,0.48834732,-0.022178048,-0.5844127,0.12204088,0.20660812,-0.14054775,-0.2652558,0.1032904,-0.03428835,0.0021689031,-0.28900713,0.10250672,0.10035457,-0.17245246,-0.15988842,0.052699305,-0.08573376,0.15896228,-0.121479005,-0.035007566,0.20524655,-0.02541475,0.11157529,0.2614442,0.05990311,-1.021694,-0.11297627,-0.10965975,-0.21420726,-0.37064892,-0.058414314,0.69470227,-0.05659956,-0.3989639,0.23466584,-0.09863334,0.43279076,0.14481834,-0.3202441,-0.2143982,-0.17678823,-0.17383821,0.8460497,0.024495741,0.025990559,0.053038925,0.3428984,0.0348054,-0.030508406,0.34045428,0.014156938,-0.42683306,0.006597283,0.8147197,-0.06364112,0.019359192,-0.17128158,-0.23882018,-0.07580832,0.085851215,0.12942617,-0.014615865,-0.042253874,-0.25285637,-0.11931778,-0.10765514,0.17146713,-0.056382686,-0.24252442,0.032461956,0.29173923,0.23339464,-0.6780455,-0.09422889,0.032306176,0.24137512,0.11106375,0.004892176,-5.7181644,0.17232308,0.09650731,0.14234582,0.066323444,0.3214295,-0.002523036,-0.09590439,-0.20433101,0.19144191,0.34386745,-0.14485554,-0.25751412,0.06537976,-0.4238202,0.0395252,-0.37177148,0.08731445,0.13072121,0.12960154,-0.2566809,0.13879195,-0.03431072,-0.12456744,-0.30371782,-0.21377456,0.049668513,-0.2115075,0.2758814,0.62669057,0.25423753,0.048403278,0.27011782,-0.02802667,-0.1785606,-0.19339384,-0.22273944,0.35889384,0.13763213,0.17135945,0.04052446,0.9223886,0.042931814,0.3330467,0.28493273,-0.21908349,-0.405728,-0.12728725,-0.10564206,-0.009852927,0.08189329,-0.24259374,-0.26429233,0.12775414,-0.22559597,0.27494347,0.29066622,0.24974093,-0.12951966,-0.040481452,0.092865095,-0.1950006,-0.01252579,-0.17044894,0.28399456,-0.12513202,0.073894806,-0.17553018,-0.1322496,0.008958189,-0.062733084,0.19438492,0.08931596,0.14051324,0.452416,0.13104095,-0.4200104,-0.43910912,-0.057202883,-0.16184278,0.250908,-0.17453031,0.42353553,-0.4536559,-0.17595203,0.15081748,-0.1178983,0.19544713,-0.004729356,-0.21908174,-0.02791581,0.13230367,-0.070173934,0.095182575,0.2467799,0.07202566,-0.26596242,0.10141232,-0.31269947,0.16423133,0.04198914,-0.22458911,-0.9257953,-0.025049485,0.2792903,-0.069776975,-0.1116228,-0.086390994,-0.13725203,0.33301556,-0.15374874,0.19561508,-0.033658944,-0.07101339,-0.023375688,-0.27939153,-0.30702564,0.10378607,-0.5664824,0.75797784,-0.16832913,0.20937504,-0.20617291,-0.10280325,0.050251823,-0.20862997,1.2637492,-0.0053140125,0.520427,0.2820497,-0.34811825,0.23262154,0.26080787,0.1334491,0.063695,-0.17546718,-0.049787294,-0.038649827,-0.2253696,-0.22337435,0.2503465,-0.03995462,0.077890754,0.61098206,-0.18877321,0.3313773,-0.18896034,-0.17283756,-0.121846475,0.10923388,0.44303042,-0.026270337,-0.09451285,-0.44100356,0.09938547,0.1406977,0.20274009,-0.0014452914,-0.0161941,-0.0058481866,-0.20932068,-0.05865615,-0.12240118,0.108944446,-0.15605812,-0.07691043,0.07385913,-0.11808531,-0.02226884,0.1625485,-0.012633701,0.93591464,0.12114676,-0.21666138,0.124995604,0.08136852,0.37463427,0.20040011,0.06796321,0.047016717,-0.4793727,0.007306782,-0.11909346,0.17490342,-0.2038693,0.051967867,0.31579176,-0.27412683,-0.019169051,0.26016527,-0.22710513,0.087577626,0.15593624,0.19662747,-0.46379438,0.007464168,0.123276144,0.024871575,0.101402104,-0.20998913,-0.0852573,0.2913824,-0.0072840867,0.09770803,-0.11338411,0.117228694,0.2016317,0.16687886,0.18258993,-0.6163433,0.06622123,-0.18703805,0.20526662,0.39003694,-0.36896563,0.065995544,0.080291964,0.32896033,0.085280925,0.3939179,0.92137086,0.05568527,0.21032885,0.33974433,0.3751748,0.3243362,0.061268095,0.080864914,-0.18708105,1.532257,-0.081261575,-0.09149901,0.049349185,-0.01831468,0.03768955,-0.59954756,-0.14283921,-0.11537365,-0.18068488,-0.00632348,-0.14838398,0.17583634,-0.16083087,0.22551873,0.031946886,0.45294145,-0.0079484135,0.26263785,-0.040107243,-0.16098563,-0.2908248,0.1670651,-0.3081885,0.0092997905,0.14421888,0.17794481,0.24829303,0.20592408,-0.2938139,0.23877546,0.05566023,0.25036007,0.056039397,0.08562092,-0.34191743,0.46241173,-0.35687178,-0.28809735,0.09096513,0.1618426,0.29035476,0.7090262,-1.4429556,0.12540191,-0.014843487,0.45870912,-0.05020264,0.020081857,-0.088739105,0.018056972,-0.09624019,0.17406538,0.17648149,-0.19204703,0.2177185,-0.26864713,-0.5483575,0.03167153,0.15712026,-0.39946187,-0.057990145,-0.20179267,-0.108271755,-0.10126685,0.01942678,0.029685628,-0.15380086,0.48298693,-0.33437338,-0.40490854,-0.052865542,-0.5855468,0.12570913,0.23435812,-0.04252572,0.37604257,-0.4740233,-0.3412492,-0.021557588,-0.02213472,-0.15994297,-0.1548816,0.034795515,-0.07861692,-0.40909648,-0.09072061,0.8605933,0.19956051,0.122045055,0.35044825,0.05311078,0.26041698,0.028453624,-0.17239493,-0.0700324,-0.033625975,-0.4668722,-0.18494834,-0.31160358,-0.0028066952,-0.028947352,-0.6489159,-0.20203945,-0.0826697,-0.054761913,0.073044024,-0.75437623,0.25749916,-0.14812435,0.29175785,0.102228984,-0.21014942,0.3144728,-0.10035511,-0.27633584,-0.09927281,0.016716346,-0.16314307,0.040549744,0.6220059,0.48447165,0.04197523,0.0009820168,-0.13573258,0.019957982,0.1299646,-0.089433506,0.112256385,-0.09370928,0.12347083,0.29293922,0.04702776,-0.13969602,0.16055332,0.049083598,-0.0018633667,-0.08993507,-0.26244065,0.015732776,-0.028428629,0.14281273,-0.12213295,0.04366369,-0.13912238,0.32729185,-0.15135461,0.02113732,0.022989875,-0.14028905,0.042102735,0.19677176,-0.22913909,-0.08866899,-0.00437562,0.06284026,-0.011064447,-0.12628607,-0.07675361,0.22748068,-0.042576198,-0.06567449,-0.10520051,-0.063265234,0.021136051,-0.23520727,0.09779208,-0.059422027,-0.19880243,-0.11495689,-0.38747215,0.05165143,-0.33404168,0.11246007,0.20808081,0.04329394,0.2658622,0.014734749,-0.039203834,-0.47104982,0.052362625,0.0117823295,0.23175165,0.57621396,-0.13813837,-0.23924744,-0.0040401663,-0.055127192,0.4391219,-0.09682289,-0.037847586]', '2026-08-16 04:22:33.64091');

DO $$
DECLARE
    v_lesson RECORD;
    v_question_id INT;
    i INT;
BEGIN
    -- Loop through every lesson
    FOR v_lesson IN SELECT lesson_id, course_id, title FROM lessons LOOP
        
        -- Insert 10 questions for each lesson
        FOR i IN 1..10 LOOP
            
            -- Insert the question into quiz_questions
            INSERT INTO quiz_questions (
                course_id, 
                lesson_id, 
                question_text, 
                explanation, 
                question_type
            )
            VALUES (
                v_lesson.course_id, 
                v_lesson.lesson_id, 
                'Sample Question ' || i || ' testing knowledge on: ' || COALESCE(v_lesson.title, 'Unknown Lesson'), 
                'This is the detailed explanation for question ' || i || ' of lesson ' || v_lesson.lesson_id || '.', 
                'SingleChoice'
            ) RETURNING question_id INTO v_question_id;

            -- Insert 4 options for the newly created question (1 correct, 3 incorrect)
            INSERT INTO quiz_options (question_id, option_text, is_correct, order_index)
            VALUES 
                (v_question_id, 'The logically correct answer for question ' || i, TRUE, 1),
                (v_question_id, 'A plausible but incorrect distractor', FALSE, 2),
                (v_question_id, 'An obviously incorrect answer', FALSE, 3),
                (v_question_id, 'None of the above', FALSE, 4);
                
        END LOOP;
        
    END LOOP;
END $$;


CREATE UNIQUE INDEX ix_quizzes_title_instructor_id ON quizzes (title, instructor_id) WHERE is_removed = false;

-- ==============================================================================
-- Cập nhật lại các sequence sau khi insert dữ liệu mẫu có sẵn ID
-- ==============================================================================
SELECT setval('courses_course_id_seq', COALESCE((SELECT MAX(course_id)+1 FROM courses), 1), false);
SELECT setval('lessons_lesson_id_seq', COALESCE((SELECT MAX(lesson_id)+1 FROM lessons), 1), false);
SELECT setval('learning_materials_material_id_seq', COALESCE((SELECT MAX(material_id)+1 FROM learning_materials), 1), false);
SELECT setval('media_embeddings_media_embedding_id_seq', COALESCE((SELECT MAX(media_embedding_id)+1 FROM media_embeddings), 1), false);

-- ==============================================================================
-- Bảng checkout_sessions
-- ==============================================================================
CREATE TABLE checkout_sessions (
    checkout_session_id VARCHAR(50) PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Expired, Cancelled
    total_amount NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL
);

-- ==============================================================================
-- Bảng gift_checkout_sessions
-- ==============================================================================
CREATE TABLE gift_checkout_sessions (
    gift_session_id VARCHAR(50) PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    course_id INT NOT NULL REFERENCES courses(course_id) ON DELETE CASCADE,
    recipient_email VARCHAR(255) NOT NULL,
    recipient_name VARCHAR(255),
    gift_message TEXT,
    card_theme VARCHAR(50),
    total_amount NUMERIC(10, 2) NOT NULL,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL
);
