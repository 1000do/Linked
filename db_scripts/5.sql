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
DROP TABLE IF EXISTS lesson_reviews CASCADE;
DROP TABLE IF EXISTS course_ai_usage_logs CASCADE;
DROP TABLE IF EXISTS message_moderation_logs CASCADE;
DROP TABLE IF EXISTS lesson_review_moderation_logs CASCADE;
DROP TABLE IF EXISTS course_review_moderation_logs CASCADE;
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
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE lesson_ai_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    lesson_id INT NOT NULL REFERENCES lessons(lesson_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
    date_added TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE learning_material_ai_feedbacks (
    feedback_id SERIAL PRIMARY KEY,
    material_id INT NOT NULL REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    field_name VARCHAR(100) NOT NULL,
    feedback_text TEXT NOT NULL,
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

SELECT setval(pg_get_serial_sequence('accounts', 'account_id'), (SELECT MAX(account_id) FROM accounts));
SELECT setval(pg_get_serial_sequence('categories', 'category_id'), (SELECT MAX(category_id) FROM categories));
SELECT setval(pg_get_serial_sequence('courses', 'course_id'), (SELECT MAX(course_id) FROM courses));
SELECT setval(pg_get_serial_sequence('lessons', 'lesson_id'), (SELECT MAX(lesson_id) FROM lessons));
SELECT setval(pg_get_serial_sequence('learning_materials', 'material_id'), (SELECT MAX(material_id) FROM learning_materials));
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
('harmful_text_classifier','classifier','local','1.0.0','/app/models/spam_1/,/app/models/toxic_3/','active','an ensemble of spam and toxic text classifier that was fine-tuned from distilbert multilingual cased','text'),
('clip','embedding_generator','openai','1.0.0','openai/clip-vit-base-patch32','active','a multimodal model that was used to generate embeddings','media'),
('distilbert','embedding_generator','hugging_face','1.0.0','distilbert-base-multilingual-cased','active','a language model that was used to generate embeddings','text'),
('paraphrase-multilingual-MiniLM-L12-v2','embedding_generator','hugging_face','1.0.0','sentence-transformers/paraphrase-multilingual-MiniLM-L12-v2','active','a multilingual language model that was used to generate embeddings','text');

INSERT INTO
system_configs(config_key,config_value,description)
VALUES
('course_harmful_text_classifier','/app/models/spam_1/,/app/models/toxic_3/','system config of course_harmful_text_classifier'),
('course_text_embedding_generator', 'distilbert-base-multilingual-cased','system config of course_text_embedding_generator'),
('course_media_embedding_generator','openai/clip-vit-base-patch32','system config of course_media_embedding_generator'),
('review_harmful_text_classifier','/app/models/spam_1/,/app/models/toxic_3/','system config of review_harmful_text_classifier');

INSERT INTO
system_configs(config_key,config_value,description)
VALUES
('moderation_threshold',
'{"similarity": 0.85,"spam": 0.85,"toxic": 0.85}',
'system config of AI moderation threshold');

-- =====================================================================
-- SEED COURSES, LESSONS, MATERIALS, QUESTION BANK
-- =====================================================================
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('2', '1', '9', NULL, 'Essence of linear algebra', '<p>Learn the core concept of linear algebra with a visuals-first approach. This course will take you step-by-step through the geometric intuition behind vectors, matrices, and linear transformations.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782213673/z1l7qknbnp11zlzyvirw.png', '2026-06-23 11:23:19.848023', '2026-06-23 15:43:06.289398', 'published', '0', '<p>- Understand vectors, linear combinations, and spans.</p><p>- Visualize linear transformations and matrices in 2D and 3D space.</p><p>- Compute and intuitively grasp the determinant.</p><p>- Master inverse matrices, column spaces, and null spaces.</p><p><br></p>', '<p>- Basic high school mathematics.</p><p>- No advanced calculus or prior linear algebra experience required.</p><p><br></p>', NULL, '2026-06-23 15:43:06.289399', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('6', '1', '12', NULL, 'Hướng Dẫn Thực Hành Excel Cơ Bản', '<p>Khóa học hướng dẫn thực hành các kỹ năng Excel cơ bản từ Trung Tâm Tin Học Sao Việt. Khóa học này sẽ giúp bạn làm quen với giao diện bảng tính và thành thạo các hàm tính toán, công cụ xử lý dữ liệu thông dụng nhất phục vụ cho công việc văn phòng hàng ngày.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782215468/uuyidzgrbbzzyp3dikre.webp', '2026-06-23 11:51:06.571487', '2026-06-23 15:42:59.780609', 'published', '0', '<p>- Nắm vững cách sử dụng các hàm điều kiện cơ bản như hàm IF.</p><p>- Thành thạo các hàm xử lý thời gian và xử lý số liệu đơn giản.</p><p>- Biết cách sử dụng công cụ tìm kiếm và thay thế dữ liệu một cách hiệu quả.</p><p>- Nắm rõ bộ hàm đếm (COUNT, COUNTA, COUNTBLANK) để thống kê dữ liệu.</p><p>- Kỹ năng lọc và sắp xếp dữ liệu cơ bản để quản lý bảng tính chuyên nghiệp.</p><p><br></p>', '<p>- Máy tính có cài đặt phần mềm Microsoft Excel.</p><p>- Không yêu cầu kinh nghiệm sử dụng Excel trước đó.</p><p>- Phù hợp cho người mới bắt đầu, sinh viên và nhân viên văn phòng.</p><p><br></p>', NULL, '2026-06-23 15:42:59.78061', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('4', '1', '7', NULL, 'Japanese Language Lessons', '<p>Learn Japanese with Japan Society. This course provides a structured introduction to the Japanese language, covering everything from basic greetings to essential vocabulary and grammar structures.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782214308/wrxvkbsqjtfgfmchtn9w.jpg', '2026-06-23 11:31:39.40256', '2026-06-23 15:43:01.85085', 'published', '0', '<p>- Basic Japanese greetings and introductions.</p><p>- How to count and use numbers in Japanese.</p><p>- Vocabulary for days of the week and days of the month.</p><p>- How to express going to a destination.</p><p>- Conjugating and using essential verbs (drinking, eating, seeing, listening, doing).</p><p>- Building a strong foundation for conversational Japanese.</p><p><br></p>', '<p>- No prior knowledge of Japanese is required.</p><p>- An interest in learning the Japanese language and culture.</p><p>- A willingness to practice pronunciation and vocabulary.</p><p><br></p>', NULL, '2026-06-23 15:43:01.850851', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('1', '1', '2', NULL, 'Full Stack Web Development Tutorial Course', '<p>A comprehensive guide to becoming a full stack web developer from scratch. You will learn frontend and backend technologies.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782210754/d5eg4nugx4qej8lxegyg.png', '2026-06-23 10:32:29.720532', '2026-06-24 09:27:51.466018', 'pending', '0', '<p>HTML, CSS, JavaScript, React, Node.js, Express, PostgreSQL</p><p><br></p>', '<p>No prior programming experience required. Basic computer knowledge is sufficient.</p><p><br></p>', 'AI flagged the following issues:\n- [material_14] Found a semantic duplication for material_14 on existing_material_11 (cosine similarity: 0.8528020761153733 >= 0.85)\n- [material_1, material_13, material_14] Harmful content detected in: material_1, material_13, material_14', '2026-06-24 09:23:40.378201', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('9', '1', '2', NULL, 'Design Patterns', '<p>A comprehensive guide to software design patterns by Geekific. In this course, you will learn the core principles of object-oriented design and how to implement popular creational, structural, and behavioral design patterns in Java. Mastering these patterns will help you write cleaner, more maintainable, and scalable code.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782215924/raofrjjf3phgqp9bdz7v.jpg', '2026-06-23 11:58:37.213522', '2026-06-23 15:42:52.882386', 'published', '0', '<p>- Understand what design patterns are and why they are essential in software engineering.</p><p>- Learn and implement Creational Patterns including Singleton, Factory Method, Abstract Factory, Builder, and Prototype.</p><p>- Explore Behavioral Patterns such as the Chain of Responsibility.</p><p>- Master core object-oriented design principles and best practices.</p><p>- Improve your ability to architect scalable and robust software systems in Java.</p><p><br></p>', '<p>- Basic to intermediate knowledge of Java programming.</p><p>- Solid understanding of Object-Oriented Programming (OOP) concepts such as classes, interfaces, inheritance, and polymorphism.</p><p><br></p>', NULL, '2026-06-23 15:42:52.882389', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('8', '1', '2', NULL, 'Flutter Crash Course', '<p>Learn how to create Flutter apps from scratch with Net Ninja. This crash course provides a complete introduction to the Flutter framework, teaching you how to build beautiful, natively compiled, multi-platform applications from a single codebase.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782215821/vxxcnnoahyugmbo6d1oh.png', '2026-06-23 11:56:52.228023', '2026-06-23 15:42:55.212375', 'published', '0', '<p>- Understand what Flutter is and its core architecture.</p><p>- Set up your development environment on both Windows and Mac.</p><p>- Create and configure a new Flutter project from scratch.</p><p>- Navigate the file structure and overview of a Flutter project.</p><p>- Master the use of basic Flutter widgets to build beautiful UIs.</p><p>- Understand and implement foundational widgets like MaterialApp and Scaffold.</p><p><br></p>', '<p>- Basic understanding of object-oriented programming concepts.</p><p>- No prior experience with Flutter or Dart is required.</p><p>- A computer running Windows, macOS, or Linux.</p><p><br></p>', NULL, '2026-06-23 15:42:55.212376', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('7', '1', '2', NULL, 'Docker Crash Course Tutorial', '<p>A complete crash course on Docker for beginners by Net Ninja. In this course, you will learn how to containerize your applications, work with images, and manage containers effectively. By the end, you''ll have a solid foundation for deploying modern applications using Docker.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782215720/emdj77qspcb4cpmmupxp.png', '2026-06-23 11:55:15.556299', '2026-06-23 15:42:57.541058', 'published', '0', '<p>- Understand what Docker is, how it works, and why it is useful for developers.</p><p>- Install Docker and set up your local development environment.</p><p>- Master the core concepts of Docker Images and Containers.</p><p>- Learn how to pull images from Docker Hub and use parent images.</p><p>- Write your own Dockerfile to create custom, optimized images.</p><p>- Use .dockerignore files to streamline your build process.</p><p>- Start, stop, and manage the complete lifecycle of your containers.</p><p><br></p>', '<p>- Basic familiarity with using a command-line interface (terminal or command prompt).</p><p>- General knowledge of web development or software development concepts is helpful but not strictly required.</p><p><br></p>', NULL, '2026-06-23 15:42:57.54106', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('5', '1', '9', NULL, 'Neural Networks and Deep Learning', '<p>Join Andrew Ng to explore the foundational concepts of neural networks and deep learning. Discover how AI is becoming the "new electricity" and learn how deep learning models are constructed, trained, and applied to real-world problems in this first course of the Deep Learning Specialization.</p><p><br></p>', '0.00', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782214819/fpqztqlnzr8bzrva0o16.png', '2026-06-23 11:47:32.237014', '2026-06-23 15:42:47.62407', 'published', '0', '<p>- Understand the major trends driving the rise of deep learning.</p><p>- Build, train, and apply fully connected deep neural networks.</p><p>- Learn how to implement efficient vectorized neural networks.</p><p>- Understand the key parameters and architecture of neural networks.</p><p>- Prepare yourself for more advanced topics in the Deep Learning Specialization.</p><p><br></p>', '<p>- Basic programming skills in Python.</p><p>- Understanding of basic linear algebra and machine learning concepts.</p><p>- Familiarity with basic calculus.</p><p><br></p>', NULL, '2026-06-23 15:42:47.624072', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('10', '1', '5', NULL, 'Khóa Học Edit Video Capcut Máy Tính Từ A-Z', '<p>Khóa học hướng dẫn edit video trên phần mềm Capcut máy tính (PC) từ cơ bản đến nâng cao bởi VA Media. Khóa học này được thiết kế chi tiết, từng bước giúp người mới bắt đầu dễ dàng làm quen với giao diện, công cụ và nhanh chóng thành thạo kỹ năng chỉnh sửa video chuyên nghiệp cho các nền tảng mạng xã hội.</p><p><br></p>', '9.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782216401/mihft8i7zr2rezuesq9o.png', '2026-06-23 12:06:33.216014', '2026-06-23 15:42:50.522712', 'published', '0', '<p>- Nắm vững các bước tải, cài đặt và làm quen với giao diện làm việc của Capcut PC.</p><p>- Biết cách tìm kiếm và khai thác các nguồn tài nguyên edit video miễn phí.</p><p>- Nắm rõ các tính năng và cách sử dụng toàn bộ công cụ chỉnh sửa cơ bản.</p><p>- Kỹ năng import dữ liệu, cắt ghép, thêm hiệu ứng, chuyển cảnh và âm thanh vào video.</p><p>- Thiết lập định dạng và xuất khung hình chuẩn cho các nền tảng TikTok, YouTube, Facebook.</p><p><br></p>', '<p>- Máy tính PC hoặc Laptop (Windows/MacOS) có cấu hình cơ bản đáp ứng được phần mềm Capcut.</p><p>- Cần có kết nối internet để tải tài nguyên và phần mềm.</p><p>- Đam mê sáng tạo video, không yêu cầu bất kỳ kinh nghiệm chỉnh sửa video nào trước đó.</p><p><br></p>', NULL, '2026-06-23 15:42:50.522724', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('3', '1', '7', NULL, 'Học Tiếng Anh Cho Người Mới Hoặc Mất Gốc | Bắt Đầu Từ Căn Bản', '<p>Bạn đang muốn học lại tiếng Anh từ đầu nhưng không biết bắt đầu từ đâu? Chuỗi video "Học Tiếng Anh Cho Người Mới Hoặc Mất Gốc" sẽ giúp bạn xây dựng nền tảng vững chắc, từ những kiến thức cơ bản nhất đến việc tự tin giao tiếp thực tế.</p><p><br></p>', '19.99', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782214177/ksyyufui9swc3jn4omyg.jpg', '2026-06-23 11:29:29.616376', '2026-06-23 15:43:04.001446', 'published', '0', '<p>- Xây dựng nền tảng tiếng Anh vững chắc từ con số 0.</p><p>- Nắm vững từ vựng và cụm từ thông dụng theo chủ đề hàng ngày.</p><p>- Sử dụng thành thạo các cấu trúc và mẫu câu giao tiếp cơ bản.</p><p>- Tự tin chào hỏi, giới thiệu bản thân và đàm thoại trong thực tế.</p><p>- Cải thiện khả năng nghe và phát âm thông qua phương pháp lặp lại.</p><p><br></p>', '<p>- Không yêu cầu kiến thức nền tảng về tiếng Anh.</p><p>- Phù hợp cho người mới bắt đầu hoàn toàn hoặc người đã mất gốc tiếng Anh.</p><p>- Chỉ cần một thiết bị có kết nối internet và tinh thần sẵn sàng học hỏi.</p><p><br></p>', NULL, '2026-06-23 15:43:04.001447', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('11', '1', '12', NULL, 'Tin Học Văn Phòng cho Người mới bắt đầu', '<p>Khóa học Tin Học Văn Phòng dành cho người mới bắt đầu. Khóa học này giúp các bạn mới làm quen với máy tính có thể tự học và nắm vững các kỹ năng cơ bản của Microsoft Word, từ việc tìm hiểu các phiên bản đến cách soạn thảo, định dạng và in ấn văn bản chuyên nghiệp.</p><p><br></p>', '0.00', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782229928/yr1by1m4gew6lpo9jd3i.png', '2026-06-23 15:52:15.247772', '2026-06-24 10:25:07.179658', 'published', '0', '<p>- Nắm vững kiến thức tổng quan về các phiên bản Word thường dùng hiện nay.</p><p>- Biết cách thiết lập bộ gõ và khắc phục các lỗi cơ bản như không gõ được tiếng Việt.</p><p>- Kỹ năng định dạng văn bản chuẩn xác theo đúng quy định hành chính.</p><p>- Thực hiện các thao tác căn chỉnh: chia cột, tạo chữ cái lớn đầu dòng (Drop Cap), và đánh số thứ tự động.</p><p>- Nắm rõ cách thiết lập trang, căn lề và in ấn văn bản Word một cách hoàn chỉnh.</p><p><br></p>', '<p>- Máy tính có cài đặt phần mềm Microsoft Word (các phiên bản từ 2007 đến 2021).</p><p>- Không yêu cầu kiến thức tin học trước đó, hoàn toàn phù hợp cho người mất gốc hoặc mới làm quen với máy tính.</p><p>- Tinh thần học hỏi và thực hành trực tiếp theo hướng dẫn.</p><p><br></p>', NULL, '2026-06-24 10:25:07.179659', 'f', '4');
INSERT INTO courses (course_id, instructor_id, category_id, coupon_id, title, description, price, course_thumbnail_url, created_at, updated_at, course_status, course_flag_count, what_you_will_learn, requirements, moderation_feedback, last_approved_at, is_removed, threat_level) VALUES ('12', '1', '1', NULL, 'Figma Design for beginners', '<p>Welcome to the Figma Design for beginners course! This course will walk you through the entire process of creating a website design for a personal portfolio website. We''ll start by teaching you the fundamental concepts and features that Figma Design offers, and then we''ll go on a creative journey together to make a website that you can customize to make your own using some of Figma''s most exciting features.</p><p><br></p>', '0.00', 'https://res.cloudinary.com/df8i0azc5/image/upload/v1782230240/dthvnzmlrkfmhnd2v9in.png', '2026-06-23 15:57:24.102492', '2026-06-23 17:22:54.042051', 'published', '0', '<p>- Set up a new Figma account and navigate the interface confidently.</p><p>- Understand how to organize and manage Figma design files.</p><p>- Master fundamental Figma design tools, features, and concepts.</p><p>- Step-by-step design of a complete landing page hero section.</p><p>- Create detailed and professional case study pages for your portfolio.</p><p>- Best practices for portfolio personalization to stand out.</p><p><br></p>', '<p>- A computer or laptop with an internet connection.</p><p>- No prior design experience or Figma knowledge is required.</p><p>- A willingness to learn and experiment creatively.</p><p><br></p>', NULL, '2026-06-23 17:22:54.042055', 'f', '4');

INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('1', '1db7eb9301f104b6b430c1ab3c3a532a', '7d064f7a830646276700619ab8d8e707', 'c002c7cf6bce5cafdfc908df971f2599', 'b71fe5e2a6e0b275faf962ed926b7475', 'b1ffd825a99f062e0770b970e102378b');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('2', '491afb7d79231b84459cc1ebbbb06b8a', 'f23422fd5d6631e5fb2ac0e140ad14a4', 'edbdba91e836b04cd3a595795e03501c', 'e59502e9b79d867095a5cb2f3829f75d', '0a4957fc6aeb843c5c6153919b1d833b');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('3', 'dadee14f02fff3f1fef453067a1ffe39', 'e9de14e4957252c0e8364b146d3d850d', 'a716ce9ed46dffb664a842b260cd805f', '226bfcda0ba8d9055b38c1932423beea', '380bf3ade39129fa7ccaffe8bd40e1d7');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('4', '9dfe1385ddcdca561d8d10957fa86204', 'cd5dde97cedcb14ac8daeda3694937ce', '8ebe31fcf468b45269b494846a916adb', '6f5b2cc45cee660d57bd1a227321bcc5', '14cedc70737b127cdba9408512ee026c');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('5', '57a5aafac0375c2a1c54a43ee3edfed2', '3eedd7374a5f127cfc0c459958b1957c', '8bc3ea2c31188bcfbf73a5a06c9b0d07', '3f65f940d2a0ac855c7d72e501bf0f6d', '9da01b4af17b7019667aae326f4fdf88');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('6', 'c4f8341af490e44ec524e94c3dd90ba3', '2aa2f0d2148034bcdf53b888dd1b19b0', '06996b6f2ef51feb8d1c6061eceffd8e', 'ca04f3bd14300b5298ccc4532a141671', '53b32771721f3715110e02d0aac5cff4');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('7', 'df5d75787a88abd633d16e82293e6f06', '2a7867b31576a486f318e08be1c94cbb', '6a87885d1bd7c7501a593b054d7880b1', 'e0cce0f5ada34b180631bc1ca3c5ffb0', '9ab644e6d8c6de9dd24d94b7f016d870');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('8', '073244be65750fa353fee5d97ce8c9ad', '7a13155ca5793e8f0fb17e5ea1aca1ca', '399e809b4105e01eea8df2a2c5aba4e6', '2f2df5fac3a8e4868336846b4a30a185', 'dfe33ccfbaad137d8e811190101497f9');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('9', '66c3370bf227f8b017a619de7f5897fa', 'f2c519db84e1e0cef9e1c26670dcf24f', '4fb570081859c5f3c6863e0ba7bc1c6f', 'b8952f2dc2bf2255adb71ed2efa04638', '4d767985e7e3581996e35d634daad9ac');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('10', '70678e0d00ecf9e02a0f4e62e8e8a4ae', '1cd864f8fffb86435b3b86f8ba00ac94', '00a431bde80917978cba7b3e5dcd5c0f', '754095b98db4ef84956f838abfe71662', 'f6b00f054bd9cbdadfa2b4507ca806a8');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('11', '50924db43392d54f5d727924c7d86ade', '45d2e3ef5d1085e0c143faed60e00c0e', 'b0c63e18f4f13a2f161b91dc08caf964', '174ad99daea0149ee0ade40f7fc821b1', '8770cb08e781b29cf7c626864cdd2b61');
INSERT INTO course_exts (course_id, title_hash, description_hash, what_you_will_learn_hash, requirements_hash, thumbnail_hash) VALUES ('12', 'd537620fa0ba3154679d41ca9f0122f4', 'aa73a83b37256ec7ca7a834b87438f7e', '63065cf2038891f193d3528f8ff9027d', '5354fbf6280be6736bff899f8ea49222', '5ff5d47a5fd0e27d7d7734c8595e4c84');

INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('2', '5', 'Welcome', NULL, NULL, '2026-06-23 13:19:00.815238', '2026-06-23 13:21:26.107636', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('3', '10', 'Giới thiệu khóa học', NULL, NULL, '2026-06-23 13:25:52.745504', '2026-06-23 15:12:16.192', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('4', '9', 'Introduction to Design Pattern', NULL, NULL, '2026-06-23 15:13:02.712119', '2026-06-23 15:13:02.712121', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('5', '8', 'What is Flutter?', NULL, NULL, '2026-06-23 15:14:49.691568', '2026-06-23 15:14:49.691569', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('6', '7', 'What is Docker?', NULL, NULL, '2026-06-23 15:16:19.095683', '2026-06-23 15:16:19.095685', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('7', '6', 'Cơ Bản về hàm IF', NULL, NULL, '2026-06-23 15:17:58.810144', '2026-06-23 15:19:12.173049', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('8', '4', 'Introduction', NULL, NULL, '2026-06-23 15:31:33.776224', '2026-06-23 15:31:33.776252', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('9', '3', 'CHÀO HỎI - Từ Vựng & Mẫu Câu Đơn Giản', NULL, NULL, '2026-06-23 15:34:05.249', '2026-06-23 15:34:05.249001', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('10', '2', 'Vectors', NULL, NULL, '2026-06-23 15:36:36.764745', '2026-06-23 15:36:36.764746', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('11', '12', 'Course overview', NULL, NULL, '2026-06-23 15:57:51.063018', '2026-06-23 15:57:51.063019', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('12', '11', 'Các phiên bản Word thường dùng ', NULL, NULL, '2026-06-23 15:59:12.634888', '2026-06-23 15:59:12.634889', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('1', '1', 'Introduction to Web Development ', NULL, NULL, '2026-06-23 10:33:16.714651', '2026-06-23 10:33:16.714715', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('13', '1', 'What is an IDE?', NULL, NULL, '2026-06-24 09:20:16.169221', '2026-06-24 09:20:16.169249', 'active', 'f');
INSERT INTO lessons (lesson_id, course_id, title, description, thumbnail_url, created_at, updated_at, lesson_status, is_removed) VALUES ('14', '1', 'Building Your First Website', NULL, NULL, '2026-06-24 09:21:45.356167', '2026-06-24 09:21:45.356168', 'active', 'f');

INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('7', '7', 'Hướng Dẫn Sử Dụng Hàm If Cơ Bản Trong Excel', '<p>Hàm IF là một trong những hàm phổ biến và quan trọng nhất trong Excel. Bạn dùng hàm IF để yêu cầu Excel kiểm tra một điều kiện và trả về một giá trị nếu điều kiện được ĐÚNG, hoặc trả về một giá trị khác nếu điều kiện đó SAI.</p>', '2026-06-23 15:18:50.052896', '2026-06-23 15:19:13.934083', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782227927/tumhwnacwmd91r3g51re.mp4', '{"duration": 320, "file_size": 7125767, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('5', '5', 'Flutter Crash Course #1', '<p>In this Flutter Crash Course tutorial series, you''ll learn how to make Flutter applications from scratch.</p>', '2026-06-23 15:15:07.16588', '2026-06-23 15:15:24.708703', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782227703/zjsua7sjbstv8f9obo4h.mp4', '{"duration": 415, "file_size": 8927633, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('12', '12', 'Các Phiên Bản Word Thường Dùng', '<p>Giới thiệu về các phiên bản thường dùng của Microsoft Office Word</p>', '2026-06-23 16:00:31.752646', '2026-06-23 16:01:38.126112', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782230427/cuyc6rwukijpmg9s97yx.mp4', '{"duration": 493, "file_size": 10914221, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('6', '6', 'Docker Crash Course #1', '<p>In this Docker tutorial series you''ll learn what Docker is &amp; how to use it to help improve the development experience both alone &amp; in a team.&nbsp;</p>', '2026-06-23 15:16:48.484438', '2026-06-23 15:16:51.650576', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782227805/aowvajnggwadbvedspuk.mp4', '{"duration": 446, "file_size": 11402253, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('3', '3', 'Giới thiệu khóa học', '<p>Khóa học chỉnh sửa video trên CapCut PC này bao gồm mọi thứ từ cơ bản đến nâng cao. Các bài học chi tiết sẽ giúp bạn hiểu đầy đủ các công cụ có sẵn trong CapCut dành cho PC. Học cách chỉnh sửa video hấp dẫn và chuyên nghiệp.</p>', '2026-06-23 15:11:25.693662', '2026-06-23 15:12:21.936918', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782227484/eetuoqcdiqtpzarnvnmo.mp4', '{"duration": 182, "file_size": 3905437, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('9', '9', 'Chào Hỏi - Từ Vựng & Mẫu Câu Đơn Giản', '<p>Trong video này, bạn sẽ học:</p><ul><li>Các từ vựng và cụm từ chào hỏi thông dụng như "Hello," "Hi," "Good morning," và "Good evening."</li><li>Các mẫu câu tự giới thiệu như "I am Lan," "I am from Vietnam"</li><li>Thực hành với câu mẫu và bài đàm thoại để luyện nghe, nói, và áp dụng vào thực tế.</li></ul><p>Bắt đầu hành trình học tiếng Anh dễ dàng và hiệu quả!&nbsp;</p>', '2026-06-23 15:35:15.16351', '2026-06-23 15:35:34.107411', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782228912/gltxtpmybahwkxbulqfp.mp4', '{"duration": 352, "file_size": 6702737, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('8', '8', 'Introduction - Japanese Lesson 1', '<p>This introductory sample lesson covers eight basic greetings:</p><p><br></p><ul><li>Good Morning - Ohayou (casual) gozaimasu (formal)</li><li>Good Afternoon - Konnichiwa</li><li>Good Evening - Konbanwa</li><li>Goodbye - Sayounara</li><li>Goodnight - Oyasumi nasai</li><li>Thank You - Arigatou (casual) gozaimasu (formal)</li><li>Excuse me, I''m sorry - Sumimasen</li><li>How do you do (nice to meet you) - Hajimemashite, dozo yoroshiku</li></ul>', '2026-06-23 15:33:18.803721', '2026-06-23 15:33:25.797021', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782228794/bbyuihn5s9pao8jnfpre.mp4', '{"duration": 296, "file_size": 14780165, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('10', '10', 'Vectors', '<p>Beginning the linear algebra series with the basics.</p><p><br></p>', '2026-06-23 15:37:14.517642', '2026-06-23 15:37:19.497177', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782229031/ytmfpg6exhzphedzffbk.mp4', '{"duration": 591, "file_size": 12642485, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('4', '4', 'What Are Design Patterns?', '<p>&nbsp;If you’re in the computer science domain, you definitely have heard of design patterns before, or even used a few patterns in practice with or without your knowledge.&nbsp;</p><p>In this video we attempt to answer: What are design patterns? Why were they created? and what is actually the main purpose behind them?</p>', '2026-06-23 15:13:57.189987', '2026-06-23 15:14:15.997649', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782227633/nqyq4fzxmtwdtihgqv2e.mp4', '{"duration": 441, "file_size": 9411965, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('2', '2', 'Welcome', '<p><br></p>', '2026-06-23 13:19:26.230505', '2026-06-23 13:21:35.0237', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782220763/zzqtnpnn81cucmwtxqfk.mp4', '{"duration": 332, "file_size": 6790266, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('11', '11', 'Course Overview', '<p>Welcome to the Figma Design for beginners course! This course will walk you through the entire process of creating a website design for a personal portfolio website. We''ll start by teaching you the fundamental concepts and features that Figma Design offers, and then we''ll go on a creative journey together to make a website that you can customize to make your own using some of Figma''s most exciting features.</p>', '2026-06-23 15:58:30.102236', '2026-06-23 15:58:34.50833', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782230308/qbxw7agkfwxupkc7bsvz.mp4', '{"duration": 136, "file_size": 2943442, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('1', '1', 'Introduction To Web Development  Full Stack Web Development Tutorial', '<p>This video is an overview of what we will be learning in this course.&nbsp;This course teaches us from the very basic fundamental level to the very end.&nbsp;</p><p><br></p><p>A) It starts with HTML and how to work with elements and tags and use them on the website accordingly.&nbsp;</p><p><br></p><p>B) Next comes the styling part with CSS, its concepts, how it works and what it does. At the end of HTML and CSS, we will be able to make static websites on our own.&nbsp;</p><p><br></p><p>C) To add functionalities to our websites we will be learning Javascript next. Next, we will be learning version control using Git.&nbsp;</p><p><br></p><p>D) The next part of the course will be about Bootstrap and how to use its classes and make responsive websites using bootstrap row and column properties.&nbsp;</p><p><br></p><p>E) Lastly, we will be entering into backend using node, MongoDB and finally learning a very powerful frontend framework, React.&nbsp;</p><p><br></p><p>F) We will be learning to work with databases and will be developing APIs of our own.&nbsp;</p>', '2026-06-23 10:33:41.148951', '2026-06-23 10:33:50.830821', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782210829/mlznn3teuq6dmtwrxqk9.mp4', '{"duration": 479, "file_size": 10471913, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('13', '13', 'What is an IDE?', '<p>What is an IDE?</p><p>An integrated development environment ( #IDE ) is a software application that provides comprehensive facilities to computer programmers for software development. It makes the workflow easier and supports various features like auto-complete, visual enhancement, has various plugins and tools that make writing code easier.&nbsp;</p>', '2026-06-24 09:20:44.739564', '2026-06-24 09:21:28.192551', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782292843/jubmzgpgqoqmdirv9bef.mp4', '{"duration": 337, "file_size": 11594848, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);
INSERT INTO learning_materials (material_id, lesson_id, title, description, created_at, updated_at, learning_status, moderation_feedback, material_url, material_metadata, cloud_public_id) VALUES ('14', '14', 'Building Your First Website', '<p>This video introduces us to the basic structure and elements of HTML.&nbsp;</p><p>This website is a very beginner level website using HTML only which shows the use of some basic HTML tags and how to use it inside the code. We will cover a few HTML tags like html, head, body and their structure.&nbsp;</p><p><br></p><p>All HTML documents must start with a document type declaration: !DOCTYPE HTML.The HTML document itself begins with html tag and ends with an HTML tag. The visible part of the HTML document is between body and /body.&nbsp;All of this will be discussed elaborately in the next video. We will learn how to work with headers and several other basic tags like p, br, hr, em, and know-how these tags add a different styling to the text body.</p>', '2026-06-24 09:22:26.197558', '2026-06-24 09:22:44.718554', 'active', NULL, 'https://res.cloudinary.com/df8i0azc5/video/upload/v1782292943/zkuexrkxe9uv0eqfrsqb.mp4', '{"duration": 768, "file_size": 16758951, "file_type": "video", "page_count": null, "file_extension": "mp4"}', NULL);

INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('1', '2', '[-0.0678177,0.064301096,0.16569768,-0.21836725,-0.09215929,-0.12620632,0.058111053,-0.030466227,0.668519,0.033554547,0.059526075,0.028474335,-0.0069370773,0.15679376,-0.048532933,0.1705791,0.20094122,-0.33239692,-0.04132519,0.042802844,0.106407456,0.066130996,-0.3010038,0.100138195,-0.12376527,0.3708969,0.13123374,0.061722044,-0.004503038,-0.3748946,-0.08167835,0.089641,-0.057221107,-0.098508246,0.48134273,0.009914101,-0.08663765,0.38340643,-0.05348375,-2.5099993,-0.085212596,-0.34999186,-0.26946726,-0.039156087,-0.14267616,0.53798705,0.42906588,-0.3363905,0.16185185,0.057798523,0.22819138,0.19242857,-0.100488424,-0.105195,-0.12622596,-0.107909985,0.30663532,0.2320908,0.10009333,0.015776416,-0.10892463,-0.44201604,0.22242944,-0.04463457,0.077837564,-0.03604414,-0.28252396,0.59996504,-0.09466777,0.21442303,-0.0720532,0.36501265,0.12214872,-0.12217037,0.1143183,-0.061097916,-0.12763874,-0.14715421,0.07716898,0.044536293,0.25587463,0.156501,0.07618819,0.19213079,-0.27304545,-0.23892136,1.148295,-0.20716469,0.34512758,-0.15427239,-0.2279447,-0.2855986,-5.1659093,-0.27860847,0.027195591,-0.031503003,0.009366668,-0.3330448,0.040031955,-0.23984993,-0.12052852,0.20994098,0.016079653,0.08149406,-0.03911371,-0.091613404,-2.0718055,-0.0070669013,-0.23725437,0.06414194,0.4592212,-0.05920907,0.054932047,0.06712983,-0.22015941,-0.23027211,-0.16714399,-0.31475368,0.09296031,-0.029012427,-0.28462997,0.44996136,0.036566854,0.094447814,-0.1770317,0.1647097,-0.22070862,-0.11444844,0.1118319,-0.06712263,0.29397303,0.114872105,-0.17984684,0.83827007,-0.3278069,0.14940038,-0.008347881,-0.09457522,-0.21569107,0.3600684,-0.13073182,-0.2200899,0.19507518,-0.19435157,-0.21610463,0.29351127,-0.07830972,0.5260828,-0.018428436,0.16317828,0.31299257,-0.18498693,0.9263965,0.016196229,-0.05173677,-0.08017749,-0.030499116,-0.13857909,0.2995723,-0.04499671,-0.21225514,-0.223892,-0.0012916484,-0.048465867,0.31056544,-0.62088966,0.10486325,0.004255575,-0.21293445,0.06593043,-0.13527094,0.1434018,0.16697854,-0.048279803,-0.10046597,0.07005446,-0.44615224,-0.11320612,0.31020412,0.11214137,-0.21701643,-0.41610888,-0.20822063,-0.40420118,-0.0904305,0.33251244,-0.02245178,0.36225307,-0.16882779,-0.15147844,0.3923543,-0.12182513,-0.22363475,0.2750372,-0.0052266903,0.16684043,-0.1488623,-0.002953226,0.010143973,-0.31014654,0.100502945,-0.32502136,-0.19188221,-0.028013632,0.13393849,-0.40378484,-0.1558166,-0.09175442,-0.17744622,0.15348825,-0.51562566,-0.14006427,0.304051,0.19424446,-0.15832731,-0.04621314,0.20912604,0.1608032,0.010620177,-0.17983678,0.48679462,-0.42469272,0.09642984,0.24997436,0.0076617943,0.10762817,0.2974597,0.3097959,0.07629464,0.15762648,-0.18221593,-0.122331955,0.12523939,0.021196617,-0.16697627,-0.35944963,0.05769749,-0.03700416,0.025244469,0.1904676,0.117958404,0.061865386,0.018740458,-0.32703763,0.055828445,1.0519028,0.12715253,0.26735297,0.15828952,-0.14744203,0.004293275,0.18513978,0.12546988,0.3208781,-0.014303739,0.06474135,-0.26895443,-0.05572493,-1.027244,-0.23222813,-0.08536273,-0.0024296122,0.2479109,0.4460368,0.082715824,0.29123017,0.22830313,-0.37406394,0.12268318,-0.25701022,-0.069703944,0.29913065,-0.23780754,0.1738934,0.108537525,-0.077813454,-0.23098822,0.0468438,0.12452782,0.19070508,0.05458379,0.28595263,-0.28021774,0.027521197,-0.21037437,0.2040535,-0.650819,-0.36398765,0.0679153,-0.49832088,0.21565306,0.10832795,0.06074618,-0.13756461,-0.18339744,0.63260865,-0.17101783,-0.14169364,0.26656827,-0.5118037,-0.18964161,-0.122566886,0.21710187,0.2681466,0.012637284,-0.38652173,0.12637804,-0.1802617,0.30404466,0.48679972,-0.08884346,0.050141353,0.8388467,-0.14535303,-0.15141377,0.15200247,0.065677606,-0.030147368,-0.5034414,-0.23156561,0.117364645,0.48756945,-0.2789356,-0.053527642,-0.51366776,-0.0052437144,0.17468843,-0.033494275,-0.24871111,-0.32202438,0.03544198,0.06418401,0.13965537,-0.33943388,-0.21185319,-0.13509262,0.09929693,0.24798752,-0.2526014,0.07843802,0.34673443,-0.10023394,-0.048798382,0.30288023,-0.14228073,0.3317967,-0.0146729555,-0.23254675,0.011979375,-0.09686873,-0.114050806,-0.3479788,0.020072201,-0.009739178,0.031468034,0.27803826,0.20834202,-0.17797984,0.10422376,-0.059781697,0.37495276,-0.14436384,0.13036321,0.35262087,-0.83102447,0.29456925,-0.3708596,-0.5669668,0.008962996,0.12410395,0.014807574,0.01290306,0.12488191,0.17202677,0.18278155,-0.16905475,-0.56348854,-0.13832542,-0.41819552,0.09745282,-0.061667524,0.09980647,-0.28326705,-0.08465127,0.056072604,-0.618656,-0.246932,-0.20763972,0.026973715,0.5593804,0.20934457,0.06572726,0.24513398,-0.006827633,0.17816873,0.40389588,-0.17950179,0.09106819,-0.38557687,-0.3861557,0.13619801,-0.120233424,0.40550256,0.1065804,0.13663098,-0.098709464,0.13420963,0.17264284,0.74989146,0.2744619,-0.07194323,0.32228968,-0.13661422,0.46799418,0.04351414,-0.26145688,0.44045952,-0.30237377,-0.19224887,-0.23455039,-0.24471308,-0.03832376,-0.150006,-0.14876255,0.061553776,-0.11674448,-0.20808227,-0.101385206,-1.3284816,-0.25559402,-0.44495386,0.2502273,0.16814621,-0.26722795,-0.054119103,-0.17445576,-0.3575932,0.07208531,-0.20008972,-0.08548419,-0.055909835,0.3613639,0.2649111,0.22012202,0.28624198,0.05930285,0.1922498,0.080703884,0.15849529,-0.06978473,-0.08630871,-0.048299663,0.2561235,0.06469926,0.21179457,0.03351229,-0.26230288,0.007633946,-0.13015315,0.30073744,0.08631211,0.03905972,-0.38732865,-0.04040198,-0.07426286,-0.20769738,0.22742143,-0.12938535,0.00962645,-0.074958265,0.18027206,-0.059748642,0.013366424,-0.31225878,-0.091544464,0.18786737,0.23440383,0.28573313,-0.19277857,0.11023842,-0.08684056,-0.32955316,0.21557724,0.08812497,-0.21945733,0.15186052,0.03811124,0.04062721,-0.024383185,-0.2568888,0.085506916,-0.42002934,0.39145273,-0.41501287,0.14583887,0.1037066,-0.04061668,0.11798445,0.25579256,-0.09487856,0.25916457,0.0035556443,-0.014122052,0.3840631,0.54009956,-0.3234362,-0.044325095,0.058454495,-0.06628357,0.6862034,-0.2028336,0.10312124]', '2026-06-23 13:24:35.777761');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('2', '3', '[-0.26833472,0.054996602,-0.31178844,0.1754153,0.11350837,-0.45739022,0.14141597,0.02461107,-0.3054477,0.21185504,0.3239143,0.23387128,0.3386565,-0.02046215,0.511195,-0.029366855,-0.6209138,-0.18587299,0.37670794,-0.33545858,0.26657906,0.15615246,-0.22789863,-0.21770759,-0.028722122,0.36919758,0.16334905,-0.003478527,0.15407541,0.288194,-0.17689124,0.26278168,-0.01555613,-0.099054016,0.5661897,-0.047775634,-0.056430127,0.08051524,0.12906496,-1.2038803,-0.2845469,0.19368905,-0.20144935,-0.13133079,-0.047319744,1.8019292,0.20788848,-0.399392,0.5321164,-0.23657787,0.35138425,0.1264514,-0.04315066,0.07713627,0.13515936,-0.011258575,0.5821728,-0.15543516,0.31955656,0.15128238,0.09735423,-0.47477362,-0.3030137,-0.34012258,-0.010039331,-0.34536317,-0.0150927305,1.0906153,0.2547271,0.23947038,-0.13176034,-0.24240893,-0.049720317,-0.09852204,0.25459334,-0.36865464,-0.20846127,0.31357127,-0.19012687,0.19968568,-0.24368356,-0.30982468,-0.1684004,0.4544994,0.42178786,0.1689444,-0.1727006,0.05803738,0.6107979,-0.26250362,-0.014993413,0.015224924,-4.0298696,0.07300673,-0.13429634,0.16723274,-0.17553437,0.08400154,-0.31044665,0.14714584,-0.12844831,-0.24361877,0.15561347,0.19604267,-0.4638341,0.048642904,-1.0807132,-0.24889098,0.0014311712,-0.10603837,0.036207344,-0.27216825,0.23657003,-0.15044779,-0.24350974,-0.41750178,-0.10241804,0.19288605,0.24208884,-0.072354056,0.32319388,0.76664096,-0.061900396,0.22037052,0.14491764,0.14911275,-0.19953287,-0.1383893,-0.18210727,-0.096593656,0.33762345,-0.31956565,0.25750276,0.8105334,0.15102631,0.30718192,-0.027792096,0.07612365,-0.4899763,0.30957976,0.11551778,-0.13239136,-0.017793125,-0.2065357,-0.31222594,-0.012771326,-0.27451858,0.08015836,0.5041922,-0.034794856,0.040357012,-0.5572226,0.27694166,-0.35414818,-0.27272397,-0.1306039,-0.086073175,-0.14458978,-0.23045169,0.014771314,0.006050926,-0.03231072,0.13801683,-0.27821153,-0.12743658,0.05080525,0.80287874,0.14506438,0.13941851,-0.50279903,-0.3007381,0.05962514,0.41929,-0.10799172,-0.00360641,0.2537934,0.4963747,-0.18843023,-0.028549587,-0.11936441,-0.140727,-0.19656906,0.043299276,0.16589679,0.01210894,-0.058862202,-0.34501284,-0.16925627,-0.14158683,0.27038336,-0.36941746,0.046556886,-0.051094383,-0.1624273,-0.03438299,0.1594418,0.07419138,0.06216836,-0.009585245,-0.06490866,-0.19532214,0.7927642,-0.198092,-0.046386216,-0.19683059,0.59458894,-0.25714156,-0.24054974,-0.6571363,0.015152031,-0.54960173,0.09093356,-0.0080866525,0.005760275,-0.07979694,-0.49376985,-0.1632241,0.37263006,0.006293888,-0.13537903,0.2069222,0.62392616,0.18266816,0.1878401,0.41367888,0.21149492,0.09851446,-0.27705565,0.076823615,-0.23063737,-0.34407184,-0.11031053,0.13252625,0.048854403,0.2434831,-0.011432329,-0.33690557,0.08199975,-0.08092116,-0.3380822,0.029502634,0.6419752,0.78046274,-0.18685855,0.15643132,0.1682111,0.27005598,0.19685692,0.10549208,-0.19984964,0.10316315,0.07462852,-0.10924422,0.17182662,0.059296902,0.23604639,-0.21748884,0.06976902,-0.53038764,0.18511236,-0.048530832,-0.26273882,0.20004407,0.8649744,0.058849584,-0.19184808,0.066952415,0.098348394,-0.38927367,-0.020864345,-0.06566614,-0.14728309,-0.09462192,0.24020855,-0.30192843,-0.21514918,-0.10862233,-0.22877824,-0.1040367,-0.042681623,0.14918603,0.35108015,-0.32667577,0.40481666,0.0032205551,0.008469604,0.10389048,-0.13979314,-0.15957713,-0.1157266,0.18945222,-0.18999687,-0.3225464,0.16553156,0.08679714,-0.011168129,-0.068654165,-0.07737034,0.05280172,0.16963595,0.36654276,-0.4003198,0.20107578,0.13076508,-0.0056837457,-0.07687113,-0.24620719,0.10906225,0.35845384,0.058756616,-0.081936896,0.54418385,0.8098271,0.32502368,0.48550496,0.33603886,0.47230002,-0.04005183,0.43293482,0.28567323,-0.15327786,0.74791,-0.07024822,-0.19805038,-0.3041551,-0.25948617,-0.16884656,-0.23681729,-0.22704542,-0.48634472,0.19656324,-0.09681172,0.104984954,0.047626454,0.12368868,0.2849786,-0.16433257,0.0038492884,0.033904277,-0.051075965,-0.11146312,-0.03843561,0.008738222,0.29065633,-0.2415733,-0.1662738,-0.387426,0.07989084,0.23318313,0.17877649,0.26503202,0.22936706,-0.32085338,0.28892463,0.37360603,0.14889984,-0.11161287,0.45650622,-0.18702708,-0.10601696,-0.27262244,-0.1636443,0.1542488,0.63943243,-0.31771636,-0.3640496,-0.27754727,-0.111518286,-0.20137918,-0.04625353,-0.00075469184,-0.21359861,0.08866589,0.14051208,0.34287155,-0.061582457,-0.2605112,-0.16382425,-1.0874416,0.057794727,0.50696415,-0.11876046,-0.1499195,0.027375478,0.13386254,-0.18060815,0.008305418,0.30016825,0.2906374,0.6710955,-0.03231773,-0.20362143,0.2531882,-0.11097171,0.101460695,-0.087145254,-0.041675184,0.123735905,0.062978886,-0.12475119,0.26153883,0.0016626414,0.20859027,-0.0754267,0.28158903,-0.2626768,-0.27012354,0.19872501,0.3409276,0.51216465,0.54392016,0.49253693,0.022695536,0.2319353,0.28340745,-0.42576402,0.067589566,-0.0097105475,-0.60661924,-0.17148651,-0.15026525,-0.08896233,0.05421246,0.04117695,-0.11792475,-0.057845265,-0.11456675,-0.19223823,-0.3809118,0.16510385,-0.32309297,-0.10672411,0.34854418,-0.16015545,-0.01565764,-0.2624611,-0.42276374,0.16022334,-0.09184125,-0.012879774,0.13340446,0.32416666,-0.24068712,0.2107874,0.2551011,-0.6229145,-0.21022817,0.028791001,-0.12018374,-0.28894153,0.332061,-0.07741063,0.29187125,-0.08860146,-0.34715497,0.15524322,-0.045769975,-0.16426486,0.0014667576,0.13168047,0.0071608387,-0.027627781,-0.23084703,-0.12846963,-0.09850656,0.002161426,-0.062430866,-0.07952675,-0.16137993,-0.29862756,0.0074806134,0.2593707,-0.4459219,-0.323009,-0.23611799,-0.10511041,0.037880752,0.46185926,0.15154216,0.12385073,-0.34284267,-0.6990765,0.052146602,-0.35849917,0.012507555,0.30233058,-0.12528749,-0.16564867,0.15671623,0.28056657,0.23124611,-0.2540695,0.19496837,-0.0008892726,0.0031709254,0.4075628,-0.095854275,-0.13684967,-0.06441718,0.10376918,-0.10928478,0.3745892,-0.3014566,0.19595158,0.5142383,0.0520261,-0.22357197,0.506062,-0.28197965,0.5958182,-0.12081022,0.04645584]', '2026-06-23 15:13:52.941074');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('3', '4', '[-0.025906013,0.18315332,-0.2541403,-0.26662356,0.076789305,-0.1195394,0.30760324,0.3477193,0.46570936,-0.05611931,0.024363719,0.2749179,0.17550443,-0.20875573,-0.062288057,0.07683443,0.060063913,0.12522891,-0.2698105,-0.08574841,0.24675567,0.24341036,-0.08758756,0.11587141,0.018023726,0.2222093,-0.29341915,0.029060038,-0.17172481,-0.09892232,-0.09917049,0.13882864,0.09435001,0.2008792,0.19252206,-0.09550698,0.030652022,0.34107307,0.20952635,-1.7565258,0.09332939,-0.21498558,-0.13298868,-0.12305263,-0.24130273,-0.23872584,0.41430372,-0.076637395,0.14614452,0.04457472,0.038971785,-0.20436883,0.10588986,-0.040960748,-0.11412517,0.22941308,0.19980048,-0.027873605,0.08077562,0.060157888,-0.42677638,-0.15998062,0.21559103,-0.16362399,-0.11761093,-0.063963845,-0.01154979,0.27438876,-0.1595164,-0.26574787,0.15688463,0.27309537,0.09359662,-0.1530405,0.10841625,0.2522729,-0.04740334,-0.38501424,-0.017777357,0.079593174,-0.058739543,-0.06279584,0.020221045,0.3201921,-0.22184902,0.07608703,0.44807994,0.079800576,0.34233248,-0.08917364,0.11994538,-0.008504297,-5.4978395,0.4328288,0.030833067,0.19293687,-0.03658366,-0.16532414,0.19851659,-0.18316491,-0.17199585,0.02099548,0.01142841,-0.08370253,-0.1110002,0.0851551,-1.8847984,0.066282675,-0.18789925,-0.099734254,0.18256587,-0.08788777,0.18167217,-0.09677832,0.024694292,-0.21059458,-0.16221623,-0.1305393,0.16198413,-0.15394196,-0.1989129,0.108508274,0.011782981,0.17565693,0.15328749,0.22381355,0.09617566,-0.0037672345,0.04925601,0.056063287,0.15713389,0.030435612,0.07943559,0.868245,-0.0076115355,0.23253772,-0.09775947,-0.09484518,-0.12357244,-0.025187995,0.11671368,-0.096099876,-0.1194557,0.10502625,-0.19326578,0.1195542,-0.21697101,0.03460245,0.13609158,-0.1788915,-0.08471201,0.004715929,0.7945922,0.031047495,0.056582354,-0.10796858,-0.25627452,0.1140501,0.14115977,0.3669278,0.09531367,-0.03297773,0.016546266,-0.038773518,0.52070725,-0.10899812,0.06502289,0.2399632,-0.15171425,-0.0850216,0.0489165,0.10317796,0.14089656,-0.20650598,0.079810396,0.09430195,-0.39801085,-0.009200117,0.017950255,0.27619198,0.116709724,-0.29758802,0.007974969,-0.0064892243,-0.07628956,0.19557507,0.10622423,0.17966822,0.029558714,0.110402115,-0.08406129,-0.024752256,0.028429309,0.10047003,-0.12783548,0.07480925,0.00828098,0.035084575,-0.3253984,-0.08405745,-0.011517054,0.03705632,-0.051052056,-0.088954166,-0.18722713,-0.0023176174,-0.2744226,0.16625232,-0.017579598,-0.06358814,-0.35683054,0.014566843,-0.10860967,-0.002331506,0.07835589,-0.2305802,-0.13895622,0.17649283,0.24438548,-0.13445088,0.24506208,-0.3149576,-0.18867928,0.16621596,-0.13496517,0.2197251,0.16976993,0.24137197,-0.00010414405,-0.12742728,-0.18938698,-0.06681557,0.15026464,0.10848963,-0.020763453,-0.28538743,0.1658457,-0.16637933,0.005860327,-0.25827023,0.13590491,-0.06478074,0.11356976,0.020338202,-0.06273681,0.60031265,0.124887295,0.25373775,0.2011864,-0.10226651,0.0040490464,-0.033763513,0.23117246,-0.054742355,0.07004658,-0.15839589,-0.123206176,-0.012865701,-1.356355,0.1218184,-0.10812156,-0.009836842,0.06813355,0.4625027,0.10357678,-0.17331028,0.0054514203,-0.176881,0.066292755,-0.10550722,0.14476603,0.14730112,0.13721055,-0.0032423765,0.013845818,0.051605474,-0.011634141,0.03619016,0.0099763125,-0.0048462073,0.00927826,-0.009210698,-0.26114836,-0.05660856,0.10590114,0.27957147,-0.5105609,-0.33574829,0.022073459,-0.054094154,-0.10449151,0.12547958,0.12587123,0.053299773,0.10080581,0.33700937,-0.1545078,-0.090774395,0.10092001,-0.20467676,0.3297492,0.073338225,-0.01807637,0.23759788,0.112622194,-0.14798588,-0.10353319,0.038571853,0.3287815,-0.025716197,-0.19370535,0.13711074,0.8679603,0.113454245,0.04165332,-0.0417627,-0.003638543,0.07215641,0.049402457,-0.21341565,0.20189767,0.79382646,-0.17990379,0.040493723,-0.07968122,0.087775156,-0.00092857506,-0.03675748,-0.008528022,-0.11765207,0.10262379,0.23899363,0.09337796,-0.18595895,-0.16383654,0.09432321,-0.080289446,0.069856286,0.028780468,0.24443093,0.15894738,-0.0718467,0.099591166,0.092892155,-0.16442594,0.152556,0.28165448,-0.15180428,0.20026396,0.014911667,-0.12204923,-0.07736477,-0.14952835,0.119661696,0.18131879,-0.22996795,0.084885105,0.026216254,0.05217766,0.089456566,0.24652895,0.086158276,-0.025608763,0.2884597,-0.45994017,-0.14816761,-0.09770304,0.20762715,0.31128106,-0.20889486,-0.08988958,0.057009887,0.10259836,-0.0776011,0.114317745,-0.17114742,-0.17789099,0.23421836,-0.50131166,-0.00039577475,-0.083717726,-0.18597785,-0.37632832,-0.23930742,0.14376289,-0.119590856,-0.040911656,-0.37795916,-0.34652844,0.7699734,0.03289068,-0.051226433,0.15559246,-0.1793475,0.011924504,0.11876316,-0.036928263,0.29888305,-0.40709955,-0.24379094,0.30156326,-0.0013675474,0.19868769,-0.1708206,-0.064544946,-0.03645396,-0.14338654,0.06160906,0.42774943,-0.087185614,-0.024033487,0.08415102,0.09982626,0.2991423,0.06062242,-0.00967055,0.09273976,0.05257067,-0.27305034,-0.16109951,-0.4246157,0.03329245,0.13036186,0.10431922,0.23575786,-0.075299606,0.108253956,-0.43363914,-1.3857247,-0.27844238,-0.18108808,0.21265224,0.011658883,-0.09345173,-0.12527926,-0.07822925,0.039725833,-0.04651001,0.13577256,0.03223558,-0.102999866,0.3207158,0.22379483,-0.017817419,-0.0067757107,-0.16000117,-0.045879498,0.056864303,0.23440696,-0.36544064,-0.19412406,-0.1696302,0.0936147,-0.46944922,-0.0341286,0.115022615,-0.21278739,-0.22558744,0.12239159,0.045573946,0.0071146954,-0.08726533,-0.002510503,0.06284619,0.107349224,-0.34364754,0.20653027,0.10053725,-0.0013086776,0.08845412,0.11649371,0.48646736,-0.052277472,-0.17914328,0.016734967,-0.086068325,-0.0152019495,-0.034320056,0.21247566,0.24036315,-0.022755483,-0.21206559,0.17392138,0.04904117,-0.005310663,0.17125976,0.11000396,-0.09220353,0.1415021,-0.14656115,0.0134521015,-0.2959312,0.2736197,-0.42311412,0.0014697418,-0.0036764664,-0.07092442,-0.0062703164,0.047518186,-0.17114428,0.21278796,0.007411489,-0.03524383,0.11487215,0.22316477,-0.2122864,0.0635118,-0.1685344,-0.14197154,0.6401316,-0.17861345,-0.11102068]', '2026-06-23 15:15:25.816692');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('4', '5', '[-0.01902217,0.039826624,0.055668633,-0.06937997,0.03878739,-0.24312751,0.1425473,0.19112347,0.35093492,-0.05791965,-0.07329845,-0.07723173,0.044708595,-0.01686539,-0.114100024,0.05694107,0.16155457,-0.06128033,-0.04882677,-0.17401774,0.6497347,0.10426027,-0.11449107,-0.03320145,-0.20676517,0.15384626,-0.19997627,0.09096435,-0.11908477,-0.043841872,-0.036710683,0.16719538,0.0103735635,0.24575329,0.37267634,-0.11998176,0.16376589,0.18800455,-0.03563482,-1.6007146,-0.13989012,-0.27256745,0.065366425,-0.32551333,-0.040099513,-0.04595733,0.0497356,-0.05531093,0.05910654,0.07444234,0.20612684,0.2682886,0.14439695,0.046282414,-0.3307096,-0.055010993,0.2637488,-0.15628503,0.040830128,0.37161732,-0.4651719,-0.04290785,0.015009486,-0.23529465,-0.15995829,-0.08009078,-0.24157073,-0.03596912,-0.3489938,-0.023535257,0.29045174,0.047893126,-0.10966319,0.20352356,-0.051495895,-0.12540178,0.075358294,-0.061493117,-0.030882508,0.029002488,-0.28405446,-0.1592279,0.20842762,-0.33064035,0.036786217,0.4232284,0.37247777,0.32283047,0.07129274,-0.29232106,0.061999884,0.11745666,-4.330695,-0.08812644,-0.16892919,0.084564455,-0.028925747,0.10018131,-0.048283167,-0.4786057,-0.07986421,0.012060519,-0.26063246,0.008881808,-0.09080475,-0.0057602585,-2.4459405,0.06835892,0.036431774,0.0630826,0.041518413,-0.021856375,-0.0075354865,-0.27365917,0.115830936,-0.17942359,-0.17678653,-0.14117298,-0.042036008,-0.2038712,0.054519825,0.20693983,-0.0035083687,0.18004787,0.011072529,-0.009767346,0.0796448,-0.091445826,0.10737502,-0.2532145,0.3531548,-0.18992375,-0.028715484,0.76601404,-0.011560982,0.04178303,-0.015256782,-0.41036558,-0.07739326,0.24963668,0.08328072,-0.070375435,0.21201149,-0.048725832,0.03348617,0.13465667,0.18964523,0.4951475,-0.16513199,-0.029645732,-0.047317035,-0.39888865,0.18823099,0.29478028,0.058828615,-0.3227053,-0.15685937,-0.037099857,0.14559975,0.40245983,0.046357237,-0.23488292,0.07219738,0.04865035,0.106236495,0.070278056,0.5373687,0.24429642,0.15051816,-0.09127862,-0.046591297,0.0029751228,0.19247101,-0.43214276,0.21789123,-0.049938258,-0.0022304475,0.00046272663,0.2107372,-0.08653752,0.10803961,-0.27696866,-0.26269644,-0.21857798,0.1556696,0.093421705,-0.0080275675,0.011358676,-0.00026136413,0.06610249,-0.16914378,-0.03671016,-0.0021596602,-0.14891528,0.021114023,-0.06773077,0.014235839,-0.007100992,-0.16115023,-0.03136078,0.21500993,0.112612806,-0.14927393,-0.38123962,-0.20543766,0.15840828,-0.041735206,0.1960891,-0.19116344,-0.009832142,-0.13493462,-0.20793736,0.2867121,-0.1928576,0.06191387,-0.18449637,0.08310453,-0.45299795,0.6472433,-0.16765864,0.41684803,-0.43079963,-0.07780097,0.10042811,-0.041490503,0.23201057,0.29976428,0.25031126,-0.056567375,-0.15532818,0.0925975,0.06828632,0.091112465,-0.03535352,-0.053991143,-0.17258012,0.23714273,-0.15690367,-0.022150243,-0.3325379,0.16378263,0.14835782,0.10515863,-0.094549105,0.06669849,0.77166176,0.0783582,0.22180635,0.050789226,-0.12722783,-0.2417983,-0.095094815,0.012948922,0.14485621,-0.03196366,-0.1121739,-0.198571,0.14322837,-1.2315964,0.23626888,-0.17827615,-0.10697302,0.07304414,0.22320944,0.24219507,-0.19050764,-0.12669584,-0.11688829,-0.06868224,-0.010552387,-0.04159854,0.31719607,0.0939914,0.19282526,-0.18284057,0.1820285,0.092070386,0.2999806,-0.23853582,-0.09276123,-0.14141943,0.18915886,-0.084522806,0.020135054,-0.048346493,-0.078643784,0.3070253,-0.23270734,-0.20960738,-0.19525312,0.09614542,-0.011721225,0.106674016,0.1580957,-0.10068369,0.27879617,-0.21796188,-0.028137835,0.0042487285,-0.1619336,0.128529,0.047497287,-0.05113489,-0.22852951,-0.01933232,-0.28968138,0.19126059,0.17457941,0.0358594,0.11264919,-0.19746189,0.025286283,0.7647668,0.08424422,0.15461351,-0.3617745,-0.100111395,-0.06372775,-0.38688925,0.34641007,-0.06923792,1.115307,-0.07083486,-0.36562917,-0.35905987,0.0041911746,0.049969066,-0.068332925,-0.2647,-0.43072748,-0.040986165,-0.052137434,-0.1700052,0.051615454,-0.165036,0.41892263,0.26757163,0.040395472,0.1787416,0.38283402,0.10868456,-0.15288381,-0.042773988,0.19415514,-0.15726395,-0.1317508,0.048280373,-0.081065424,0.09883409,0.056978215,0.039654102,-0.0512919,0.035087444,0.30172873,0.08922568,-0.02374397,0.27171472,0.4436343,0.2579269,0.11991555,0.1982034,-0.056281168,-0.0396465,0.594896,-0.51568425,0.03256686,-0.28596285,-0.21632063,0.20689249,0.0878547,0.11342022,0.16638735,-0.112476826,-0.17073114,-0.1076442,0.2112658,-0.11089428,0.073298275,-0.64295256,0.04743817,-0.3120366,-0.07647959,-0.35896993,-0.16304679,-0.052163523,-0.19018678,0.17778383,-0.1463954,0.0963489,0.44853792,0.088057786,-0.055504635,-0.055656765,0.26015553,-0.0048643043,0.10677124,-0.2564435,0.29227525,-0.46881095,-0.20347466,0.2560643,0.19145106,0.14825138,-0.23601173,0.16751239,-0.17541553,-0.33022967,-0.14605705,0.53927094,-0.076729365,0.06633068,-0.1199703,0.16982949,0.16748618,0.22919792,-0.17255422,0.21115434,0.424521,-0.37641105,-0.17964838,-0.34528112,0.028928855,0.3308724,-0.2063337,-0.06019815,-0.4740946,0.07322603,-0.27567342,-2.214448,-0.15522455,-0.14096622,0.011134297,-0.3849296,-0.0070133954,-0.086413436,-0.2564794,0.101538755,-0.03782577,-0.08289096,-0.07950142,0.11903009,0.11644166,0.17030373,0.06556292,0.06504802,-0.014943331,-0.06453263,0.14352207,0.27036172,-0.10943479,-0.36675343,-0.0714338,0.27807447,-0.089573465,0.051507067,0.39158896,-0.022231106,-0.19692083,0.07147119,0.1171716,-0.07567541,-0.15273981,-0.13320853,-0.0844822,0.16240996,-0.2157167,0.24746354,0.037608232,-0.034894604,0.07066178,-0.10002143,0.2561951,0.10570252,-0.48195228,-0.013231823,-0.23906663,-0.15451565,0.10062211,0.06540795,0.19012965,-0.12605807,-0.4596053,0.2431665,-0.11492598,0.079499975,0.2403803,-0.049176157,-0.08790195,-0.11963377,-0.13028198,0.057405714,-0.013761913,0.19255391,-0.17634504,-0.053927362,0.13447684,0.16573998,0.056276724,-0.14754681,0.08232713,0.14287804,0.1280145,0.024366174,0.059760068,-0.361275,0.021254543,0.27948317,-0.03331157,-0.0035537498,0.42079312,-0.076469585,-0.15752326]', '2026-06-23 15:18:27.800285');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('5', '7', '[0.28821158,0.36820626,-0.09966817,-0.054897416,-0.2826574,-0.21823575,0.20822139,0.31901416,-0.6169751,0.21946257,-0.20039527,0.03188087,-0.005100342,-0.1732475,0.15981755,0.0769758,0.22266868,0.25717765,0.08084231,-0.14903438,-0.109741665,-0.0768315,-0.17582244,-0.015606822,-0.042653065,0.35448486,-0.5571973,-0.01788301,-0.16545132,-0.12312689,-0.13143152,-0.04485316,-0.6032693,-0.033981685,0.49336067,-0.07118935,0.25291708,-0.13720818,-0.09579011,-1.8110747,0.04290979,-0.11966292,-0.081932545,-0.06709445,0.38907287,0.76139474,0.022155698,-0.5377703,0.2351669,-0.025449453,0.05317129,-0.2662852,0.09700993,-0.09558418,-0.11175943,-0.06415377,0.2873773,-0.39844242,0.16657823,0.16316469,-0.06341442,-0.07558329,-0.04939224,-0.14824197,-0.013681978,0.16378848,0.13626382,0.25474718,-0.27432647,-0.12533826,0.42345032,-0.097433396,0.17803447,-0.061361335,0.12549394,-0.0904534,-0.37852022,-0.26611373,0.058201488,-0.08652585,-0.20345746,0.003037823,-0.19545497,0.5471632,-0.28162032,0.016527642,-0.13705716,-0.07815083,0.40419468,-0.35858288,-0.04089939,0.012537235,-5.142462,-0.036074515,0.005972788,-0.16402738,-0.08263596,0.22413851,0.16416693,0.33621797,-0.29755938,-0.12875591,-0.026711674,0.09807385,0.04633908,0.52786833,-2.339672,-0.17729396,-0.3789637,-0.044979513,0.11658429,-0.07720444,0.25722855,0.08802679,0.21455944,-0.31647474,-0.13872485,0.113146275,0.34056014,-0.45624465,-0.026013514,0.35270786,-0.24912633,-0.1300121,-0.12399132,0.13232657,0.05156328,0.0077705723,-0.13219285,0.15362498,0.25255784,-0.23083545,0.12444938,0.8738024,-0.09234911,-0.13670856,0.2653549,-0.3812117,-0.11347616,0.3016603,-0.008659835,-0.27344647,-0.2531831,-0.36751902,-0.26139605,-0.020120895,-0.16708535,0.4484996,0.26429477,0.30382848,-0.2225184,-0.21823347,-0.15046057,-0.5496808,0.26284742,-0.11719216,-0.15265268,0.08297576,0.22347203,0.32907686,0.19056757,0.31746492,-0.3285227,-0.26225123,0.2957618,0.17932983,-0.15034312,0.041393477,-0.30386198,-0.3022526,-0.28945768,-0.18615049,0.13744657,0.15745762,-0.33116055,-0.1747003,1.1213924,0.14167538,0.59491885,-0.11689736,-0.16410144,-0.11999381,-0.22078058,0.18990229,0.02787965,0.003326235,-0.14199737,-0.12791012,-0.067733355,0.05696475,0.18789054,0.014376052,-0.27773103,-0.21144131,-0.2569367,0.15348345,0.39533028,0.29831654,0.08743157,-0.2447547,0.08467071,0.30727273,-0.16449279,-0.41908416,0.053539947,-0.02607384,0.004719917,0.17890704,-0.32887858,-0.018200856,-0.564873,-0.4409442,0.14213574,-0.07565714,-0.04411227,-0.07601951,0.0061960025,-0.052075364,-0.085889645,0.34505814,0.13834248,0.011577233,0.14477219,0.1566197,0.42673182,-0.20643765,0.3443643,0.1250312,-0.45005727,0.01342113,-0.35353583,0.3951858,-0.025434978,-0.045030527,0.328245,-0.5428316,0.09336585,-0.15145618,-0.11510548,-0.33988324,0.17960064,0.19324984,0.44678205,-0.24821323,-0.2842643,0.6421525,0.048189096,0.002204095,0.2767901,-0.0790793,-0.027658511,0.033134293,0.16902839,0.11470391,-0.042249568,0.14130144,-0.3179802,0.43446368,0.29409876,0.14997908,0.09689176,-0.19449644,-0.17863841,1.0161079,0.34943852,-0.07782901,-0.67936623,0.045630652,-0.16547853,-0.20594668,-0.102957934,-0.118877105,-0.24762258,0.086808264,-0.014101138,-0.17576602,0.097748026,-0.09422603,-0.0047806343,0.2528411,0.12081007,0.118583284,-0.0155648235,-0.114077985,0.22394338,0.2467418,0.6425865,-0.21932764,0.099421605,-0.159438,-0.11556829,-0.11998353,-0.019797245,-0.08328572,0.06308201,0.03271123,-0.22127976,-0.004829229,0.21122263,0.013528254,0.24111515,0.028549574,0.085878626,0.17273079,-0.18194124,-0.22090277,0.35638782,0.5339748,0.49196997,0.018217353,-0.04602366,0.17759795,0.87353224,0.16644432,0.07816856,0.07459099,0.44621262,-0.1924381,-0.046298202,0.29263642,0.069179274,0.41051912,-0.39923966,0.14691652,-0.29101405,0.22028631,-0.28403035,-0.047098983,-0.074801356,-0.035293736,0.13256352,-0.08266911,0.4148169,-0.37064448,0.29420358,0.045616377,0.15596905,0.21826579,0.07276029,0.1452347,-0.24143487,-0.5160231,-0.019694194,0.48318785,0.22926602,-0.27371833,0.14969797,0.1969678,0.13891648,0.021178696,0.43677613,0.30323857,-0.1602775,-0.06767647,-0.04094961,-0.16936125,0.25320092,-0.12811963,-0.46117687,-0.13308401,-0.06300896,0.21259868,-0.062489007,0.17356876,-1.4617131,-0.20748399,-0.1745508,-0.08786475,0.07956401,0.23501867,0.1526252,-0.15099636,0.45927796,0.03319492,-0.09084892,-0.1732256,-0.41695455,0.33526865,-0.48620963,-0.03925312,-0.15296793,0.2730335,0.24811658,-0.14368306,0.19412409,0.23546022,-0.052962966,0.10120688,0.1619039,0.0076774876,0.16103238,0.18232153,-0.30210638,-0.19153304,-0.23181188,-0.10459605,-0.0018492317,0.11129837,-0.51740414,-0.09994502,0.42755356,0.45810288,0.2657042,-0.28002256,0.013960572,0.20869263,-0.18920219,-0.048204675,0.5334962,0.27510637,0.026999438,0.30205354,-0.30548468,0.45622927,0.025037346,-0.05277414,0.24961944,-0.058386758,-0.016020264,0.14422448,-0.38542932,-0.10880759,-0.05214109,0.32182175,-0.093433514,-0.41461915,0.34905824,-0.06882797,-1.9148538,-0.07008265,-0.07187659,0.07286871,-0.03362356,0.28826618,-0.25773656,-0.3532981,-0.45924738,0.11370869,-0.20932972,-0.09592598,0.046442818,-0.15045857,-0.5897299,0.21596266,0.24189015,-0.60598534,-0.07537951,0.048157874,-0.47817355,-0.14191116,-0.4101008,0.21646003,-0.043812763,-0.11323941,0.16753824,0.6977293,-0.14506622,0.2887546,0.17688584,0.3821234,0.44241247,0.46473876,-0.28591788,-0.2241217,-0.21092117,-0.012912697,-0.123908274,-0.13870026,-0.017385127,-0.14741442,-0.091225274,-0.21520497,0.1946253,-0.06522834,-0.30991727,-0.08222892,-0.20221092,-0.05549851,-0.10539426,-0.17008214,-0.3684048,-0.5999452,0.17390057,-0.2208965,0.3909249,0.28957692,-0.04302416,-0.2959546,-0.23706402,-0.14243431,0.13567264,-0.11984766,0.3176503,-0.354802,-0.28756014,0.02953049,-0.1656736,-0.31561002,-0.050379816,-0.026582817,-0.0074515683,-0.0948999,-0.27182475,0.007176861,0.025448743,0.020331478,-0.080216214,0.3454654,0.24604163,1.0582358,0.2514408,0.32418182]', '2026-06-23 15:20:51.73039');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('6', '8', '[0.13686436,0.015016156,-0.2904566,-0.23725203,0.05117753,-0.12646368,0.060695328,0.2092327,0.10764166,-0.103376515,0.19127432,-0.112742014,0.54468143,0.28539047,0.36971122,0.059688047,-1.1328418,0.22599016,0.4145513,0.06260104,-0.3754434,0.14991656,-0.039275046,-0.061934542,-0.4561146,-0.02711962,0.096366815,-0.088487245,0.1876071,-0.2923726,-0.08327592,0.24973921,-0.17244641,-0.07115363,0.15943098,0.49959219,-0.21050514,-0.051592186,0.033209484,-1.1611013,-0.38717794,0.06724992,0.039842393,0.11824518,0.13118726,1.3738931,0.052204575,-0.092886016,0.45012268,0.0072159204,0.86795825,0.13057195,-0.09568493,-0.104434244,-0.18179882,-0.15353096,0.532528,-0.29711258,0.06915267,-0.42577317,0.5482453,-0.69458574,0.13200033,-0.21722296,-0.16944133,-0.24882641,0.3430747,0.7472317,0.056289848,-0.22505224,0.08767402,0.5731741,0.21386838,-0.2750994,-0.043530885,-0.22010121,-0.010552004,-0.41296604,0.14483812,-0.051932536,-0.008802566,-0.28715837,-0.20529735,-0.005329265,-0.033558358,-0.21585882,0.44658646,-0.42600092,0.5624529,-0.1879928,0.046291314,0.10556571,-5.880608,0.018606385,-0.19822013,0.12635647,0.029692,0.2456758,0.3084709,0.90270376,0.08841457,0.2960003,0.105676085,0.18449202,-0.14870359,0.20013925,-1.0697606,0.1876985,-0.11150667,0.028639838,0.18423676,-0.69427484,-0.10868472,0.08429443,0.17743245,-0.0025119812,-0.28704682,0.18803486,0.029227907,-0.25276962,0.23181021,0.1923993,-0.45851222,-0.16501695,0.12612006,0.048783068,0.13662903,-0.20439386,0.23274666,0.012412571,0.19492461,-0.18878575,-0.02484329,0.8701279,-0.043506548,0.18866074,-0.11950301,-0.38979903,-0.60763174,-0.06306213,-0.18196048,-0.018882977,0.47802275,0.23853451,0.1691751,-0.04536482,-0.033283018,0.1926336,0.06850683,0.4178272,0.4903963,-0.27427465,0.48127717,0.26932627,-0.088981494,-0.4648871,-0.271413,-0.33048072,-0.33677685,0.015649613,-0.5676332,-0.0045045284,0.20200758,0.10433596,-0.21526656,0.31841457,0.33725402,0.15583079,-0.23126172,0.12442979,-0.02995598,0.37778988,-0.24492335,-0.19996819,-0.2822423,0.034740116,-0.17883913,0.18071045,0.36962152,0.07742193,-0.28109837,-0.3462919,-0.31256655,-0.09443438,0.2497986,-0.10705537,0.017995566,0.2197194,0.16882887,0.16675799,0.08243127,-0.037529092,0.09483312,-0.17351526,-0.29473123,0.18692756,0.0462266,-0.02968921,-0.19432902,0.029494643,-0.049859375,0.0024672174,-0.011625511,-0.17580962,0.06954453,0.1109182,-0.42082006,0.042664405,-0.24954703,-0.029974792,-0.77520734,0.59318453,0.14754333,-0.18996152,0.20363714,0.03744265,-0.06540694,0.14647527,0.21743757,0.12163144,0.3613082,-0.014921303,0.14247324,0.33084595,0.34202307,0.0031581023,-0.011164352,0.08385718,0.41820425,-0.09192525,-0.4477823,-0.08127115,0.08612636,0.41053846,-0.05618052,0.01786019,-0.12619935,-0.20652583,0.0020807555,0.12524171,-0.3894027,0.3661368,0.3957777,-0.14205484,-0.42828634,0.32278228,0.11632073,-0.5169957,0.044837173,-0.2870027,-0.10644476,-0.37767762,-0.20219375,0.29343015,-0.2929046,0.005725486,-0.10634606,0.18632637,-0.38486066,-0.11406164,-0.14830765,-0.023691865,0.39992505,-0.37122673,0.056260604,0.003440555,0.23948416,0.14439122,0.2674157,0.1357585,0.16224171,0.16775683,0.023029977,-0.5605758,0.041656196,0.18265317,0.05330038,-0.20662554,-0.41473612,0.12565337,0.120192245,0.15389465,0.16357216,0.03199547,-0.07850958,0.29970968,-1.1144844,-0.10642073,-0.20630372,-0.22099835,0.2846958,0.05510891,-0.0657687,-0.29747877,0.037900366,-0.10393749,-0.15524022,0.4198781,-0.3049085,-0.08357885,0.20859578,-0.02124653,0.19468875,-0.06767211,0.28578347,0.19312815,-0.3980867,-0.06397922,0.72534436,0.62487596,0.010734478,0.07074976,0.87112045,-0.38183236,-0.010890269,0.15333362,0.39132756,-0.5825361,0.08807549,0.8974855,0.30088457,0.31194454,0.11084225,-0.04095686,-0.54812294,0.07357299,0.23897475,-0.53026026,-0.20291238,-0.4668627,0.16883028,0.22838798,0.27624753,-0.2587098,-0.15619655,-0.25239953,0.10501099,0.2663046,-0.1311996,0.11689502,-0.055308077,-0.20189331,-0.05578916,0.12568042,-0.12178832,0.08830144,-0.24398221,0.06928428,0.17588106,-0.1898973,0.78269243,-0.16198328,-0.33189693,0.080647275,0.054477755,0.18211454,0.23572999,0.07091445,-0.17193408,-0.06716136,-0.26046434,0.08343133,0.05542942,0.928283,-0.77512646,-0.12668304,0.017307345,-0.9855985,-0.03243055,-0.070680104,-0.04058807,-0.054366622,0.010892415,-0.115880325,-0.25017002,-0.14388114,0.15555243,0.1268968,-0.52763873,0.16687693,0.30155745,0.24757633,-0.20226654,-0.18342009,0.1541468,-0.3002019,-0.13876942,0.18276906,0.69976884,0.68066174,-0.7032628,-0.040916085,-0.06818084,0.07898244,0.17402008,0.14764065,0.17259195,-0.05087047,-0.028750628,-0.11380656,0.45363715,-0.044422615,0.6407477,0.24576226,-0.24680787,0.08658786,0.004135365,0.03376555,-0.10000746,0.06879844,0.05894637,-0.019557834,0.0702533,0.511258,-0.033606183,-0.351712,0.1245073,-0.23067768,-0.29073557,0.6817496,-0.18696247,-0.061907563,-0.24481879,-0.12720993,-0.052568767,-0.030221524,-0.40531918,-0.00020621979,-0.7031387,-0.059391633,0.01105282,0.090996,0.5327418,-0.4027477,0.07513336,-0.36942396,-0.17795421,0.06484029,0.10926824,0.032695185,0.041557804,0.93972725,-0.1797081,0.10059311,0.15252794,-0.071393035,-0.02963242,-0.29884145,-0.062135663,0.06403896,0.7769029,-0.12288954,-0.13212748,-0.05152239,-0.097713836,-0.10112733,0.022342592,0.0351494,0.16483553,-0.41697076,-0.0775117,-0.08042495,-0.11104965,-0.03371636,0.16315767,-0.21303792,0.010594031,-0.116607614,-0.0028998354,-0.26962376,-0.09173143,-0.08564098,0.04340507,-0.36493912,-0.43387422,-0.03550747,0.108610526,0.22153066,-0.09335362,-0.015738731,-0.203258,-0.24685942,0.11829088,-0.008419013,-0.15622507,-0.16464221,-0.2277572,0.07391623,0.047255203,-0.32433608,0.50544876,-0.22708917,0.40418577,-0.32119876,-0.3744114,-0.078413576,0.24245626,-0.027625084,0.26940715,0.01958244,0.09488833,0.20875797,0.21244925,0.104569614,1.2236679,0.084711,0.14486709,0.11704182,-0.037859254,-0.0042000366,0.0030441727,-0.21012178]', '2026-06-23 15:34:41.641855');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('7', '9', '[-0.3478613,0.15883316,-0.13767052,0.15227321,0.07747966,-0.35242954,0.27749574,0.038156178,-0.21936345,0.34596685,0.20116253,0.21418962,0.5105841,0.12287729,0.48834735,-0.022178046,-0.584413,0.122040875,0.20660807,-0.14054778,-0.2652558,0.10329041,-0.034288343,0.0021688982,-0.28900707,0.10250671,0.100354545,-0.17245242,-0.1598884,0.052699294,-0.08573377,0.15896235,-0.12147903,-0.03500758,0.20524658,-0.025414743,0.11157517,0.2614442,0.0599031,-1.0216941,-0.112976305,-0.10965978,-0.21420717,-0.37064886,-0.058414306,0.6947022,-0.05659953,-0.39896396,0.23466578,-0.09863329,0.4327908,0.14481837,-0.32024404,-0.21439819,-0.17678826,-0.17383824,0.84604985,0.02449575,0.025990564,0.05303891,0.34289828,0.034805402,-0.030508416,0.34045428,0.014156944,-0.42683345,0.0065972805,0.8147199,-0.06364113,0.019359197,-0.17128152,-0.23882014,-0.07580834,0.0858512,0.12942623,-0.014615852,-0.042253893,-0.25285637,-0.11931778,-0.10765514,0.17146711,-0.05638268,-0.24252439,0.032461956,0.29173923,0.23339461,-0.6780456,-0.09422889,0.032306198,0.24137515,0.111063726,0.0048921816,-5.718163,0.17232311,0.0965073,0.14234589,0.06632345,0.3214294,-0.0025230292,-0.09590437,-0.20433101,0.19144192,0.34386733,-0.14485551,-0.25751412,0.0653798,-0.42382032,0.039525185,-0.3717716,0.08731445,0.13072127,0.12960154,-0.256681,0.13879195,-0.034310706,-0.12456746,-0.30371806,-0.21377446,0.049668502,-0.21150732,0.27588138,0.62669075,0.25423738,0.048403297,0.27011773,-0.028026676,-0.1785606,-0.19339384,-0.22273944,0.35889384,0.13763224,0.17135948,0.04052447,0.92238843,0.042931806,0.33304673,0.28493264,-0.2190835,-0.40572822,-0.12728724,-0.10564209,-0.009852934,0.0818933,-0.24259381,-0.26429242,0.12775412,-0.2255961,0.2749436,0.2906663,0.24974094,-0.12951969,-0.040481422,0.09286508,-0.19500063,-0.012525801,-0.17044896,0.28399444,-0.12513202,0.07389481,-0.17553014,-0.13224961,0.0089581795,-0.062733136,0.19438495,0.08931598,0.14051329,0.45241585,0.13104099,-0.4200102,-0.43910903,-0.05720289,-0.1618428,0.2509081,-0.17453037,0.42353547,-0.4536561,-0.17595202,0.1508174,-0.11789823,0.1954473,-0.0047293482,-0.21908174,-0.027915811,0.13230361,-0.07017395,0.09518253,0.24677987,0.07202567,-0.2659624,0.101412304,-0.31269944,0.16423137,0.041989144,-0.2245891,-0.9257953,-0.025049485,0.2792903,-0.06977696,-0.1116228,-0.08639096,-0.13725199,0.33301556,-0.15374874,0.19561504,-0.033658955,-0.0710134,-0.023375671,-0.2793915,-0.30702555,0.10378609,-0.5664821,0.7579777,-0.16832918,0.20937496,-0.20617284,-0.102803275,0.050251793,-0.20863017,1.2637497,-0.0053140195,0.5204271,0.2820497,-0.34811828,0.23262154,0.2608078,0.13344918,0.063695006,-0.17546715,-0.049787298,-0.03864985,-0.22536968,-0.22337441,0.2503466,-0.039954614,0.07789078,0.6109821,-0.18877326,0.33137712,-0.18896036,-0.17283756,-0.121846445,0.10923383,0.44303036,-0.026270343,-0.09451285,-0.4410034,0.09938547,0.14069772,0.20274004,-0.0014452863,-0.016194098,-0.0058481838,-0.2093206,-0.058656134,-0.12240116,0.10894444,-0.1560581,-0.07691042,0.0738592,-0.11808526,-0.02226884,0.1625485,-0.012633712,0.93591475,0.12114679,-0.21666132,0.12499556,0.08136848,0.37463418,0.20040025,0.067963235,0.047016706,-0.47937262,0.00730678,-0.11909347,0.17490347,-0.20386934,0.05196787,0.31579167,-0.27412692,-0.019169046,0.26016524,-0.22710523,0.08757763,0.15593623,0.19662742,-0.4637945,0.007464175,0.123276085,0.02487156,0.10140216,-0.20998912,-0.08525726,0.2913824,-0.0072840825,0.097708054,-0.11338405,0.117228635,0.2016317,0.16687883,0.18258995,-0.61634314,0.066221245,-0.18703799,0.20526662,0.3900371,-0.36896563,0.06599558,0.08029194,0.3289605,0.08528095,0.39391786,0.9213709,0.05568527,0.21032883,0.33974427,0.3751747,0.32433626,0.061268087,0.08086492,-0.18708096,1.5322571,-0.08126156,-0.09149902,0.04934919,-0.01831468,0.037689593,-0.5995473,-0.14283924,-0.115373634,-0.18068494,-0.0063234866,-0.14838399,0.17583635,-0.16083086,0.22551872,0.03194686,0.4529416,-0.007948407,0.2626379,-0.04010725,-0.16098565,-0.29082477,0.16706511,-0.30818856,0.0092998,0.14421894,0.1779448,0.24829298,0.20592402,-0.29381385,0.2387753,0.055660233,0.25036,0.05603942,0.0856209,-0.34191763,0.46241167,-0.3568718,-0.28809726,0.090965174,0.1618426,0.2903548,0.70902604,-1.442955,0.12540199,-0.014843488,0.45870906,-0.05020267,0.020081874,-0.08873909,0.018056959,-0.096240185,0.17406535,0.17648143,-0.1920471,0.21771838,-0.26864722,-0.5483575,0.03167154,0.15712027,-0.39946195,-0.057990156,-0.2017927,-0.10827175,-0.10126686,0.019426782,0.029685624,-0.15380086,0.4829869,-0.3343735,-0.40490866,-0.05286555,-0.5855468,0.12570918,0.23435813,-0.042525727,0.37604246,-0.47402322,-0.34124917,-0.021557596,-0.02213472,-0.15994312,-0.1548816,0.034795515,-0.07861695,-0.4090965,-0.09072061,0.8605932,0.1995605,0.12204505,0.3504483,0.053110775,0.26041698,0.02845362,-0.17239489,-0.07003239,-0.03362598,-0.46687213,-0.18494834,-0.31160367,-0.002806701,-0.028947366,-0.6489159,-0.20203947,-0.082669675,-0.054761924,0.07304401,-0.7543764,0.2574991,-0.1481243,0.29175794,0.10222903,-0.21014944,0.31447282,-0.100355156,-0.27633587,-0.09927282,0.016716352,-0.163143,0.04054975,0.6220056,0.4844718,0.04197523,0.0009820185,-0.13573267,0.019957991,0.12996459,-0.08943344,0.112256296,-0.09370926,0.12347078,0.29293925,0.047027767,-0.139696,0.16055332,0.049083617,-0.0018633645,-0.08993507,-0.26244068,0.015732775,-0.028428622,0.14281283,-0.12213295,0.043663714,-0.13912235,0.32729188,-0.15135457,0.02113735,0.022989893,-0.14028907,0.04210273,0.19677171,-0.22913904,-0.08866899,-0.004375616,0.06284026,-0.011064436,-0.12628601,-0.07675359,0.2274806,-0.042576198,-0.065674506,-0.10520055,-0.063265234,0.02113606,-0.23520727,0.09779214,-0.059422072,-0.19880237,-0.11495685,-0.38747194,0.05165142,-0.33404166,0.11246006,0.20808077,0.043293957,0.26586214,0.014734741,-0.039203838,-0.47104976,0.0523626,0.011782324,0.23175164,0.57621384,-0.13813843,-0.23924741,-0.004040165,-0.055127207,0.43912202,-0.09682289,-0.037847582]', '2026-06-23 15:36:27.396903');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('8', '10', '[0.2232299,-0.25673622,-0.07702692,-0.29806602,-0.15651698,-0.2857293,0.28256145,0.4487205,0.2630656,0.2478105,-0.057821203,0.20668788,0.1611813,-0.0111854505,-0.033015084,0.109766714,-0.07333448,-0.059049357,-0.2671813,-0.16433015,0.019492581,-0.34212214,-0.14387684,0.16803539,-0.08358347,0.20477028,-0.3076507,0.07781423,-0.07828279,-0.35482678,0.36305016,-0.038697403,0.13667414,0.24115598,0.2580761,-0.12302325,-0.015784007,0.03257751,-0.16716982,-1.7804506,-0.05868924,0.21887864,0.052626155,-0.07679845,-0.18964805,0.067866206,0.017110497,0.046615805,-0.3792902,-0.21477124,-0.21608213,-0.2544652,-0.16301031,0.058958672,-0.20709597,0.09472524,0.015632186,0.17429024,-0.18720014,0.17392369,0.32267395,0.073701955,0.09637914,-0.03288823,-0.06138996,-0.09571831,-0.09578486,0.06346009,-0.09202612,-0.1853933,-0.13937548,-0.007479907,0.19099402,0.23982987,-0.0919001,-0.32402143,-0.13093698,-0.104663044,0.03585968,-0.2101991,-0.27132004,-0.024754198,-0.10166971,0.5748409,-0.22880606,-0.11006283,1.0859681,0.099611625,0.32148692,-0.2645095,0.22132015,0.1412594,-7.0319014,0.27105474,0.1599181,0.26054144,0.199172,-0.3281726,0.018387653,-0.4664339,-0.07413978,-0.012142876,0.17936428,-0.24039198,-0.1792232,0.024299681,-2.3235338,0.26064825,0.1789783,0.030397711,-0.036555458,-0.13614102,0.09142242,0.14498039,-0.20134111,-0.085205026,-0.04992141,-0.56002176,0.2681398,-0.0026518225,-0.20663448,0.091972955,-0.21445763,0.37037456,0.056243904,0.37974238,-0.24593505,0.1635191,0.2615725,-0.001990925,0.18609962,0.25372806,0.13101467,0.93179166,-0.13154516,0.08323319,-0.31705007,-0.09695152,-0.35905495,-0.13214621,-0.026102446,-0.29830843,-0.15026122,0.15120368,-0.36638868,0.25332788,-0.3391181,0.2321551,0.1380366,-0.19335254,0.14492396,0.21568002,0.4385658,-0.30487493,0.17388843,0.050062295,-0.42323145,0.016065136,0.22621627,-0.14938869,0.023166321,0.04922247,-0.12958223,-0.090732455,0.25249973,-0.073954456,0.028267758,0.18509331,-0.26385403,-0.083167456,0.06714513,0.2847465,-0.05902225,-0.27229393,-0.18351066,-0.15886442,-0.37533695,0.16018872,0.30252948,0.39633122,0.06651353,-0.34272242,-0.3170445,-0.057794385,0.089761145,-0.26380983,0.328951,0.49333057,0.24860018,0.21121807,0.19573495,-0.013653071,-0.08522997,-0.0280447,-0.081302695,-0.05932324,0.043193635,0.39522544,-0.06521166,0.3508631,0.1292986,-0.0008986667,-0.008039274,-0.5063479,0.21914126,-0.20717366,-0.12258498,0.4209695,0.0018744912,-0.107830435,-0.36350507,0.35820615,-0.14765109,-0.1968114,-0.02972933,-0.07178773,0.224013,0.26017714,-0.6516565,-0.25812033,0.24635138,-0.38087627,-0.0015858982,0.13623333,0.0012036983,-0.098888524,0.008253049,-0.2766316,-0.1151075,0.1800457,-0.09871399,0.26796934,-0.06389763,0.036459845,-0.050051894,-0.12857163,0.11323778,-0.5166744,-0.37551785,-0.30031285,0.005298202,-0.010561489,0.16494699,0.13227813,-0.020753192,0.4536698,0.17887135,0.41011727,0.3693565,0.24000898,0.089507714,-0.31661934,0.07239084,-0.07977766,-0.0003732157,-0.11527522,0.066343665,-0.27497855,-0.96591216,0.14027534,-0.3887308,-0.20232135,0.16430375,0.15336804,-0.118110254,-0.023227505,0.108235426,-0.33314326,-0.24286881,-0.061392616,-0.35390076,0.07760533,-0.028968692,-0.12818326,-0.16117968,-0.05549628,-0.25803155,0.23555751,0.15413544,0.17063047,0.24893919,-0.0068744607,-0.3134285,0.06636119,-0.13599446,0.3848653,0.062825896,-0.2914474,-0.10003751,0.014236245,0.5252989,0.014238325,0.124738626,-0.041220672,0.15684383,0.38278297,-0.46331683,0.06480269,0.025462862,-0.025920967,0.12868601,-0.4212714,-0.32558465,0.12323065,-0.025484856,-0.4583553,0.18478432,0.22461048,0.13620448,0.22162706,-0.2051516,0.2327534,0.9311217,0.030796258,-0.038403217,0.14946888,0.101986215,0.07685018,0.03814986,-0.28129888,0.2961034,1.489056,-0.07720797,0.39274183,-0.17971568,-0.06183008,-0.044843875,-0.27483025,0.33477494,0.24452408,0.06454331,0.3826743,0.36853153,-0.26488635,0.02386518,0.1557066,-0.079176724,0.24797705,0.14484158,0.36207676,-0.0051948815,-0.047117893,0.24055858,0.052377112,0.032889068,0.25406837,0.08994528,0.041091878,0.057614148,0.08479671,-0.21785375,-0.11347196,-0.10724148,-0.07480418,-0.13395871,-0.16448876,0.22118354,-0.5024087,-0.1091405,0.3331664,-0.41814414,-0.08560549,-0.027293485,-0.22675708,-1.046886,0.18465361,-0.28976724,-0.3585052,0.33488038,0.2584773,0.06732428,-0.08194668,0.49262202,-0.04230668,0.32170847,-0.49169582,-0.42898953,0.12153654,-0.47425446,-0.08002285,-0.3153716,-0.2018569,0.047918364,-0.32091033,0.30569205,0.025218925,0.065311484,-0.37701398,-0.0061824806,0.3236725,-0.24185655,0.07499836,0.16770387,-0.43756047,0.15323587,0.060713477,0.039327018,0.17624404,-0.38652942,-0.37658143,0.346901,0.2173098,-0.11233404,-0.2090193,-0.04852079,0.051141247,-0.10146334,0.06168816,0.55498564,0.018637484,0.12103273,0.00046473963,0.35100156,0.13464256,0.09793298,0.3381317,-0.30452314,-0.19924603,-0.10538447,-0.1597291,0.100183554,-0.40546685,0.04969265,-0.23491672,0.3138708,0.08972581,-0.19474132,-0.08224634,-0.720618,0.19062446,-0.0018189229,-0.15013114,-0.23102859,-0.049334325,-0.40220967,-0.37496024,0.12554905,-0.20837471,-0.098532036,-0.20536317,0.11066753,0.23059326,-0.30575782,0.17958839,0.3349887,-0.30179796,0.01610692,-0.16141392,0.11073466,-0.094580926,-0.0015588563,-0.12903483,-0.20419085,-0.50113356,0.18958376,0.11545672,-0.33854,-0.072421394,0.10246299,-0.025408817,0.16293956,-0.04244441,-0.08863037,-0.022592396,-0.2398978,0.095436215,0.48640698,0.12694493,-0.05353251,0.07803691,-0.113837756,0.2598889,-0.11139262,-0.21856605,-0.11269176,0.07718623,0.089227244,-0.008669036,-0.0019665235,0.10261423,-0.21127476,-0.25610057,-0.3637191,0.16256404,-0.15274689,-0.03834608,-0.06811741,0.02129098,-0.051551323,-0.30667275,0.22635503,-0.15082918,0.025212102,0.030281631,0.11907455,0.14641395,-0.025898425,-0.15763877,-0.017704267,0.14082558,-0.49696994,-0.07404133,0.20825115,-0.1750007,-0.0032534092,0.009332326,-0.11348908,0.0913075,-0.10616985,0.5167441,-0.18006358,0.21109891]', '2026-06-23 15:39:11.187904');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('10', '11', '[-0.009948534,-0.0553992,0.017024158,-0.2136961,0.05305451,-0.2211037,0.044725504,0.33760822,0.46022287,0.12363937,0.03590679,-0.21004024,-0.04190769,0.07669127,-0.16663985,0.1632616,-0.5425493,0.07971834,-0.22060221,-0.17938778,0.60082924,-0.21869764,0.014797792,-0.04123758,-0.05512157,0.21525505,-0.08757414,0.27871382,-0.25718978,0.15041132,-0.07876307,0.26627353,-0.094077215,0.10797017,0.34813493,-0.055995625,0.024238301,0.19249143,-0.0066017425,-2.0289524,-0.15006198,-0.22214973,-0.17496383,0.008273419,0.14059302,-0.005601953,0.1591416,-0.06495472,0.31105375,0.11662756,0.060291838,0.15666725,0.13226745,0.13201088,-0.24333093,-0.04192227,0.3135318,0.08689785,-0.04239143,0.29380092,-0.49790913,-0.21934918,-0.08505813,-0.07242935,-0.19635512,0.11615727,-0.08025806,0.13018842,-0.18359317,-0.0102088265,0.08498156,-0.032806326,0.0049995147,-0.13014756,0.13053797,-0.14961718,-0.19919634,-0.2020052,-0.054542482,-0.019089503,0.03709362,-0.23671623,-0.072038695,0.031157874,0.12417143,0.13704437,0.55896735,-0.10209682,0.07616226,-0.24198645,0.050351467,0.054439522,-6.3957515,0.4563866,-0.24044602,-0.031537376,0.07766592,0.071706615,0.09408159,-0.6100045,-0.12200014,0.01255001,0.09281557,0.13370106,-0.09858094,0.07611104,-2.175149,-0.101870395,-0.018921165,-0.20538712,0.033395298,-0.13785462,0.015795391,0.041529704,-0.055345856,-0.23491244,-0.10618462,-0.0326736,0.08714506,-0.030979423,-0.10272668,0.10784854,0.052739475,0.09094945,0.111148216,0.09495794,0.23454273,-0.07750893,-0.05702887,-0.17798042,-0.1416198,-0.30124676,-0.027548013,0.85569656,-0.00996585,-0.19301963,-0.14849812,-0.48492151,-0.28833345,-0.039506853,0.2652742,-0.12023446,0.010888355,-0.118436225,-0.0028225179,0.03443502,0.039011523,0.46463066,0.15496807,-0.027709037,0.024550052,-0.13151379,0.35885882,0.17448394,0.12166042,-0.273068,-0.20147109,0.01670403,0.110737264,0.19322598,-0.069948584,-0.15338756,0.033543862,-0.03027703,0.11836654,-0.1571809,0.7283205,0.09755582,-0.105373226,-0.33663553,-0.10073058,-0.06692663,-0.054656677,-0.054079726,-0.19862075,0.11238312,0.25487182,0.04341888,0.37466606,0.105711155,0.16222535,-0.22841333,0.05373766,-0.0055032144,-0.08746399,0.07663911,0.14129028,-0.11969848,-0.26866505,0.17121096,-0.05551368,-0.14777093,0.02426712,-0.08011706,-0.14050919,-0.08959955,-0.054057654,-0.20443691,-0.31517124,0.03482044,0.09744484,0.03785962,-0.16470672,-0.27779135,-0.047726408,-0.19490209,-0.046381414,0.14642398,-0.29566053,0.18705435,-0.3180313,0.09688839,0.28895187,-0.18133572,-0.08546068,-0.024136066,-0.22201544,-0.14471053,0.5656958,-0.020091344,0.28643405,-0.5268328,-0.040204167,0.13080579,0.046032082,0.20133369,0.4272659,0.123192795,-0.0046479055,-0.087781355,-0.22803247,0.029303111,0.23890664,-0.22593196,0.13798594,-0.20903553,0.07919019,-0.2777797,-0.033711363,-0.22712988,0.23363794,0.05073823,0.3903576,-0.24911827,-0.33235896,0.30222863,-0.094628245,0.28590336,0.10987412,0.020636601,0.049587376,-0.016631557,0.10050583,-0.026221054,-0.21630485,-0.002440758,-0.029785216,0.027174901,-1.4928062,-0.02428123,-0.097836405,0.004109542,-0.079229504,0.22454657,0.24899998,-0.13994333,-0.20536263,0.09474365,-0.11792644,-0.04379917,-0.10721491,0.03333829,-0.053536925,0.30939603,0.017315995,0.25471282,0.16310492,-0.07760873,-0.07941407,0.13144976,0.078914076,0.3209791,-0.068820685,-0.013863123,-0.038257852,0.04458856,-0.46638805,-0.211951,0.09589411,-0.112909146,0.07780031,0.16885267,-0.27354357,0.060927197,-0.16797319,0.1270194,-0.27491677,-0.039752636,-0.02800003,-0.24423139,0.057056446,-0.1512759,0.09208434,0.08509649,-0.06859655,-0.10292415,0.034737296,0.11180726,0.12245866,-0.081864804,-0.098952204,0.23195451,0.85498893,-0.14957246,-0.12411669,-0.061198708,-0.054088723,0.050834328,-0.33097857,0.072160244,-0.06380498,1.4838508,-0.16204381,-0.14398552,0.0099789705,-0.10715774,0.038549073,-0.1744727,-0.1455301,-0.17146389,-0.048804846,-0.0213827,-0.12904324,-0.05299247,-0.2761864,0.08729588,0.21731976,0.0641402,0.065282375,0.15851825,-0.005505996,0.067068994,-0.177403,0.16537885,0.19029577,-0.13005182,0.046097804,-0.11956781,0.3691864,0.04813558,-0.10071186,0.008825208,-0.05514797,-0.04902894,0.029787952,0.21288921,0.104373984,0.6240375,-0.05461514,0.12208105,0.1772501,0.020541087,0.25943437,0.19623291,-0.5237502,-0.10897241,-0.23146184,-0.052190386,-0.09297013,0.0010394605,0.04468057,-0.03063479,0.11943128,0.030677522,0.17979471,-0.17412396,0.17489307,0.27685028,-0.8091741,-0.08872947,-0.44994822,-0.077155374,-0.10072368,0.06948291,0.02011954,0.045508534,-0.044775784,-0.20720492,0.001187213,0.6786259,0.17279169,0.008672864,-0.054179307,-0.029364195,0.13456853,0.13743608,-0.035767835,0.006562267,-0.43404424,-0.22249557,0.21326753,-0.004307903,0.112925336,-0.37010145,-0.05312138,0.056563437,-0.14586447,-0.14544569,0.5861397,-0.114893906,0.54479486,0.07259118,0.066104345,0.27348736,0.020089323,0.058030307,-0.07848102,0.20063058,-0.17247605,-0.16489513,-0.35231724,-0.13079394,0.11641355,-0.6549387,0.00865123,-0.050332475,-0.00060247147,-0.08597926,-1.9516784,0.08282683,-0.04679012,0.15043987,0.036090143,0.023495032,0.05492951,-0.23972887,0.08140353,-0.28913525,-0.07748065,0.018445343,-0.047642685,0.18658467,-0.02705576,0.06807494,0.14945424,-0.16114055,-0.20358466,-0.13475588,-0.016656978,-0.06995449,-0.047730867,-0.008912285,0.17680596,0.27557,0.099937476,0.18419756,-0.03514198,-0.13199633,-0.06922402,0.037101455,-0.020975046,-0.08463354,0.032590013,0.0031807586,-0.11403802,-0.17413923,0.47728685,0.011009993,-0.010757216,-0.15464814,-0.001442714,0.18203445,-0.21795414,-0.39158553,-0.042145554,-0.07319934,-0.10550412,-0.01601788,0.005190049,0.2786479,-0.06216923,-0.34294635,-0.024146494,-0.021345465,0.20907518,0.21585152,-0.032296825,-0.21867318,-0.08284813,-0.24064384,0.2000413,0.015922993,0.076021746,-0.026724694,-0.19745974,0.20641686,0.041540287,0.004777094,-0.2899997,0.03833003,-0.017151084,-0.104707256,0.14298832,0.11345377,-0.18509112,-0.10178907,-0.16087519,-0.003910516,-0.008310688,0.2918574,-0.02291973,-0.16557671]', '2026-06-23 17:22:54.057556');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('11', '1', '[-0.22946708,0.21912979,-0.15708904,-0.30057502,-0.0008956066,-0.09159042,-0.24017231,0.20707543,-0.36532396,-0.46049416,0.30252323,0.10879026,-0.36772028,0.08924182,-0.36840135,-0.02732514,-0.60836875,0.16231608,0.32252708,0.09477811,0.99378836,-0.0851475,-0.1438158,-0.30241886,0.25788775,0.5592005,-0.12736717,0.14668791,-0.19759503,0.48257986,-0.14602913,0.229517,-0.053803485,0.32662338,0.27722389,0.15024534,-0.08220295,0.24592452,-0.3952463,-1.959066,-0.5653221,0.09171089,-0.33165735,-0.063546665,0.07388117,0.29411486,-0.16494937,-0.119293354,0.007470081,0.25957736,0.26494977,0.46568567,-0.24675255,-0.04102455,0.030072639,0.204571,0.33662578,-0.12681133,0.38545302,0.4279408,-1.0709289,0.32357568,-0.19468598,0.018351072,-0.20990467,-0.22942255,-0.057060055,-0.06131167,-0.37596947,-0.008198216,0.2545813,-0.19928664,0.52560425,-0.483604,0.055363856,-0.2774267,-0.18149212,-0.11888519,-0.41122952,0.020444416,-0.094363436,0.15036504,0.258905,-0.08263631,0.3933908,0.38050985,1.240983,-0.11574707,0.03563942,-0.1614632,0.08449424,0.07677887,-7.076151,-0.9259975,0.023440588,-0.30854428,-0.49197683,0.29699636,-0.096008815,-0.12919828,0.095192455,0.011635723,-0.10983574,0.016322827,0.044570312,0.10002,-2.0581245,-0.0073976344,-0.32200882,-0.044934604,0.09018497,-0.18968618,0.037429772,-0.05318113,-0.110764064,-0.30847755,0.06686603,0.19033675,0.026787532,-0.07117622,-0.023572998,0.31101376,-0.04037379,0.05433934,-0.40039438,0.06601168,0.33510762,-0.17621987,0.2805154,-0.59731436,0.5811588,-0.5506487,0.38517678,0.94032174,0.26823223,-0.33544517,0.09334133,-0.4399214,0.5267057,0.106404014,-0.05705676,-0.3163003,0.090679005,0.21471675,0.065084904,-0.06522508,0.19265127,0.72263813,0.20067,0.06327855,-0.2513099,-0.31539944,0.42060125,-0.0148402285,-0.19815177,-0.387928,0.49219754,0.3144232,0.13268994,0.3924427,-0.01640236,-0.28440398,0.23360382,-0.30083737,-0.12092472,0.12231967,0.15469179,0.06253573,0.097011216,-0.58134186,0.3597073,0.01982966,-0.31215915,0.21314394,-0.052227836,-0.019770825,0.35885525,-0.26751035,0.29822883,-1.0202755,0.28896597,-0.33351156,-0.12341616,-0.24716078,0.17683506,-0.11272048,-0.16920857,0.045582354,-0.35125488,0.018339103,0.07595978,-0.11793287,-0.22137533,0.09319774,0.16682377,0.17251302,-0.18390605,0.30596796,0.07073689,-0.094432555,0.31445318,-0.21704476,0.12930119,-0.29786864,0.5253203,0.0447672,-0.15113026,-0.20248413,-0.2449907,0.2436597,-0.8713872,-0.3652885,0.063207716,-0.059009824,-0.01778564,-0.02828256,0.090321794,-0.08445436,1.3431242,0.54746675,0.55620587,-0.6927832,0.038460173,0.20477162,0.17141166,0.24321535,0.19354258,-0.05794007,-0.110814385,0.076118514,-0.56571025,-0.36325458,-0.1786191,0.045988802,0.19866619,-0.44420493,-0.1261129,-0.15484437,0.055672817,-0.238056,0.3988166,0.28649497,0.4418121,0.0068050893,0.4216929,0.9466305,0.19939932,0.24790674,-0.108345106,0.26996025,-0.04944119,0.03516539,0.39633653,-0.02904789,-0.051426835,-0.12331661,-0.116828784,0.46783903,-1.7887366,-0.1948896,-0.23298821,-0.20205244,0.32688388,0.6862787,0.1907031,-0.07601257,0.08270216,0.17845444,0.19198823,-0.5454215,-0.0074770106,0.0015031769,-0.3886391,0.38404688,-0.028012741,0.017111521,-0.022345895,0.027151467,-0.09496724,0.15082274,-0.23188515,0.33508843,0.24779654,-0.14393094,0.1046653,-0.034961205,0.3489006,-0.09893849,0.075811766,-0.48351997,-0.26243874,0.34808502,-0.32796243,-0.11997155,-0.39332417,0.30267328,-0.033627976,-0.24661638,-0.34123927,-0.28101704,0.2583917,0.034018774,-0.12016541,-0.10249194,-0.2600885,-0.45194256,0.04241892,0.5880125,0.10249176,0.053503755,0.29062694,0.008184295,0.9395344,0.65459305,0.37835777,-0.27493417,-0.065795586,0.1532917,0.002977656,0.77317137,-0.37374395,0.42526895,-0.11101649,0.005136854,-0.4461126,0.29274312,-0.31718028,0.046677668,0.22622074,0.22470318,0.0356826,-0.33458158,-0.22937103,-0.25993147,0.22703868,0.21223046,0.16795236,0.2564096,0.00609058,-0.008245008,-0.19719769,0.3567759,0.032337118,0.25433725,0.13566403,0.10366408,0.44623226,-0.2591238,0.40164483,-0.28716713,-0.023209114,-0.2503503,0.092289,0.076449215,0.1288716,0.103672676,-0.09889019,0.67228276,0.10146299,-0.18189234,0.2552586,0.18172096,0.17982928,0.5114336,-0.38237193,-0.16009508,-0.0030943947,-0.809241,-0.32478228,-0.019360742,-0.071191676,0.35707986,-0.17129052,-0.46101454,0.099092536,0.03797872,-0.33819747,0.25380144,-0.6654056,0.19022258,0.12833315,0.11972693,0.118514806,0.13079669,-0.101461746,0.05336777,0.5679238,-0.036984015,0.023111071,-0.36340103,0.5012477,0.05366458,-0.3570277,0.1296789,0.10120032,0.02834135,0.326426,-0.07108698,-0.8697337,-0.4217059,-0.20697469,0.06440212,0.66196656,-0.03960434,0.06679303,-0.15934698,0.055421375,0.29786772,0.30321574,-0.3025203,0.45460296,0.058813732,-0.3677162,0.2248218,-0.14736968,-0.17169042,0.3157954,0.28482753,0.15600154,-0.1677773,-0.3121649,-0.21782972,0.06480595,0.36179426,0.05046257,0.15690318,0.008499984,-0.81593645,-1.3626553,-0.44839665,0.2255259,0.16681279,-0.6683639,-0.0136577105,0.056677707,0.14754048,-0.10476536,0.02956607,-0.10643656,-0.2542398,-0.030197462,-0.34660378,0.16300713,0.59130335,-0.1449374,-0.4797403,-0.101962686,-0.064116955,-0.34768903,-0.15427835,-0.91944176,-0.14653401,0.049176373,0.04540544,0.18866986,0.06600804,-0.5827486,-0.41527715,0.48925412,0.24355292,0.47165698,0.017213017,-0.3423523,0.07490495,0.118463024,-0.25920713,0.2743044,0.25542268,-0.11727522,-0.08120635,-0.047861785,0.5080481,-0.16174418,-0.025240969,-0.00020924298,-0.4123645,-0.037180793,0.42498022,-0.15967704,0.10588096,-0.13148698,-0.104903236,0.13972564,-0.3502378,0.15509775,0.5674298,-0.26372454,-0.31331325,-0.15350647,-0.29824826,0.49286383,-0.35236928,0.46290547,0.006345754,-0.34722131,0.30064818,0.22566344,0.1278566,-0.18995082,-0.34504902,0.42221174,-0.22764651,0.31907508,0.029382478,0.13489574,-0.2330328,0.25003177,-0.36620712,0.027768245,0.8090484,-0.05167422,-0.3834828]', '2026-06-24 09:27:51.38766');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('12', '13', '[-0.07597332,0.0894972,-0.2481829,0.0036929485,-0.0895199,-0.13092752,-0.023856174,-0.15767933,0.34155384,-0.19709939,0.24670167,-0.264418,0.120196074,-0.002950506,-0.23149593,0.05936772,-0.04649324,0.22308974,0.069027595,-0.2773507,1.23023,0.3379675,-0.124095134,-0.15339048,-0.16069423,0.008894745,-0.33690196,0.45778042,-0.28076178,0.25770342,-0.2328789,0.095717244,0.067860045,0.24149112,0.65671724,0.060406398,-0.044700973,0.45369795,0.32562158,-1.733659,-0.22522412,-0.2049791,-0.11442068,-0.5781983,0.0655792,0.27833855,0.07832538,-0.38786244,0.43846333,0.24092251,-0.15430896,0.14001359,0.05933501,0.14104284,-0.11297612,0.24558407,0.34111792,0.04845291,0.09484675,0.14815582,-0.95512587,-0.18585353,-0.031967476,-0.21286002,-0.1482723,-0.1574272,-0.2678631,0.47578666,-0.08447087,-0.16646549,0.2860867,0.02651342,0.32670632,-0.22702305,0.09505321,0.11731724,0.34623128,-0.08635465,-0.27165607,0.030300362,-0.07153026,0.05964628,0.037528157,-0.2946372,0.050420523,0.05394274,0.96089125,0.028348858,-0.017451463,-0.2166043,0.25274304,0.25846958,-5.6777997,-0.4496239,-0.035659283,-0.032391794,-0.44974253,0.043556865,0.058518488,-0.41542765,-0.2583147,0.07691698,-0.10969724,-0.16690327,-0.1731425,0.030879708,-2.5209315,-0.06912425,-0.15627745,0.3034074,0.2930022,-0.009660826,0.21049081,-0.30890772,-0.06422521,-0.08516117,-0.059544645,-0.12831976,0.07988233,-0.22837028,0.18745498,-0.015028954,-0.04257155,-0.21235517,-0.20778587,0.10580952,0.14698054,-0.3544347,0.019568235,-0.17898339,0.23128161,-0.41340786,-0.053951103,0.76386094,-0.14260073,-0.17087996,0.009654986,-0.42733476,-0.00070829707,0.07084087,0.08448797,-0.19138202,0.09570044,0.14431073,-0.21715643,0.14363939,0.005866029,0.55287623,0.24621806,0.08190423,0.13447168,-0.36572036,0.22595885,0.012105176,0.005031435,-0.73073226,0.09409923,0.31994662,0.11945158,0.17757285,-0.07112669,-0.045389377,0.3070777,0.023453278,0.3120859,-0.23516871,0.6741537,0.23565839,0.07147807,-0.069125436,0.21984856,0.1479513,-0.27488327,0.031393256,-0.095944844,-0.05605929,-0.024182152,0.04440133,0.05091845,-0.17270304,-0.05240607,-0.29960188,-0.18640234,-0.24601676,0.424175,0.20487528,0.15386987,0.0068159187,-0.36213255,0.11281656,0.23961256,-0.25085813,0.10028829,-0.024226468,0.11515974,0.24587455,0.048206918,-0.19908556,0.19624628,0.097381346,0.055367608,0.32892615,-0.29007208,-0.4788454,0.15835868,0.107129924,-0.293262,0.058615692,-0.31763768,0.017661937,-0.6048332,-0.044465058,0.28024542,0.0991889,-0.064371675,0.22948577,-0.04470608,-0.14338875,1.1278411,0.083559096,0.36840826,-0.45890242,0.18904518,0.09787886,-0.017483221,0.007850333,0.4360435,0.33507475,-0.12912586,0.024140565,-0.35434878,0.12096514,0.18286489,-0.19311841,0.20125894,-0.6728686,0.13909273,-0.19357169,-0.06304944,-0.42292926,0.2540287,-0.002805943,0.27240348,0.14818008,0.1282987,0.911272,-0.014875334,0.29809576,0.020194009,0.07071659,0.00033944417,0.031425674,0.018016133,-0.16097732,-0.15816519,0.014037949,-0.13077171,0.24389364,-1.4801692,0.1561626,0.02893146,-0.11321494,0.15550432,-0.030067988,0.4234193,-0.0114603555,-0.12904578,0.019146139,0.05115389,-0.17539522,-0.11551344,0.4357646,-0.20089856,0.12903467,-0.045277316,0.03467732,-0.022196276,0.20759988,-0.2969819,0.16114701,-0.1519682,0.3884968,0.30574813,0.12676501,0.0010166451,0.009143918,0.07580795,-0.19851057,0.21071935,-0.33111614,-0.06483037,0.13327472,-0.17490445,-0.037824184,-0.12041026,0.3334001,0.04633541,-0.10743262,-0.3105585,-0.47598213,0.063843146,-0.01039702,-0.07692329,-0.15355128,-0.20202766,-0.53169054,-0.0026944883,0.24937579,0.12140376,-0.022155412,-0.08933767,0.098493546,0.76204133,0.02422319,0.003348426,-0.57185864,-0.08902876,-0.004487094,0.07711008,0.5489631,-0.2993037,0.58201456,-0.16752832,-0.13757564,-0.36434788,-0.23938508,0.16186906,0.0068121636,-0.1485509,-0.3081226,0.14538904,0.07221105,0.099743895,-0.21776772,0.05148105,0.16533497,0.43875352,0.19962062,0.117444865,0.30457556,0.15064637,-0.110759206,-0.22982046,0.1509276,0.14469706,0.26463097,0.3410086,-0.0590369,0.43188953,-0.08588915,-0.38852894,-0.13129027,0.14873965,0.24530715,-0.18031453,0.41716778,0.37430692,0.65886414,0.018266087,-0.041107938,0.18195727,0.19544922,-0.06996139,0.97471523,-0.05398772,-0.21584974,-0.4313391,-0.57196474,-0.08972952,0.05895511,-0.059721157,-0.0479301,0.07107329,-0.06506382,-0.0077373195,-0.061483473,0.24696599,0.28642806,-0.35970953,0.05514511,-0.08302968,0.008527177,-0.23853633,-0.15438229,-0.00029109677,-0.2438956,0.28099772,-0.07090023,0.10472139,0.7672233,0.5951441,-0.15623596,-0.2866988,0.2817986,0.02395935,-0.08287525,0.094774514,0.19651645,-0.6166808,-0.1728813,0.042812534,0.1453096,0.59505635,-0.2774233,0.27925345,-0.18320538,-0.20931715,0.06918199,0.78961706,-0.25585163,0.25039896,-0.15062973,-0.17082684,0.4016264,-0.17840828,-0.10759374,0.36637965,0.20717075,-0.2828848,-0.15906085,-0.3330288,-0.045077577,0.09062757,0.03305425,-0.15437992,0.14435858,-0.09260443,-0.4937731,-1.4124467,-0.18318117,-0.28213388,0.11822523,-0.4811636,0.1758329,-0.20777929,-0.19932604,-0.06816217,-0.09778869,-0.29395428,-0.092936136,-0.18672192,0.10715391,0.21077523,0.43145788,0.3455585,-0.24682477,-0.19022784,0.0691973,0.10798503,-0.24960893,-0.4207359,-0.20038244,0.39876816,0.1981076,0.06642569,0.45457834,-0.17582594,-0.21857291,0.5648528,0.23039451,0.19060053,-0.27469108,-0.28935388,0.14628942,-0.18498747,0.072590604,0.5124569,0.09290109,-0.19206586,-0.05862288,-0.18748236,0.38927385,-0.0301675,-0.14854254,0.16247965,-0.41345954,-0.08589465,0.17172164,0.044793587,0.048155624,-0.06284216,-0.30551955,-0.018062795,-0.25779796,0.3769916,0.33011723,-0.06551469,-0.011282479,-0.09983982,-0.3258408,0.5350871,-0.31830508,0.3502161,-0.1797883,-0.1316579,0.26097366,0.1690384,-0.12697658,-0.20983465,0.06351829,0.3188615,-0.24640895,0.12489568,0.043802775,-0.24188215,-0.13148612,0.04490046,0.060343478,0.0064346096,0.61839473,-0.30179903,-0.30617538]', '2026-06-24 09:27:51.450597');
INSERT INTO media_embeddings (media_embedding_id, material_id, media_embedding, created_at) VALUES ('9', '12', '[0.0115461135,0.4037222,-0.32852238,0.2234527,-0.048498593,-0.30721554,0.1148281,-0.10370166,-0.12370253,0.26772156,-0.1589376,-0.08975425,0.011182603,-0.24948984,0.29560828,0.20544194,-0.0426194,0.15393403,0.27076888,-0.278928,0.37767166,0.09607106,0.16555184,-0.045832768,-0.15967211,0.14059652,-0.15155329,-0.122502394,-0.053117253,0.13999505,0.008273527,-0.077030584,-0.40183493,-0.061004173,0.5976575,-0.10579546,0.021954056,0.025031013,0.092422284,-1.4304162,0.19892325,-0.12371305,-0.13611238,-0.3193743,0.3538096,0.6393261,-0.010508822,-0.48830187,0.20306244,0.012976161,0.11313065,0.0807217,0.19203974,-0.081421286,0.10069452,0.25272775,-0.011118141,-0.1168831,0.09036711,0.48877093,0.04854812,-0.123131625,-0.08326172,0.011331718,0.033871185,-0.035974424,0.15955932,-0.16587348,-0.07815102,-0.22080128,0.07950617,0.07174901,0.064298645,-0.2250096,0.2205029,-0.021297859,-0.12366761,0.2450512,-0.056041565,0.0631571,0.05409985,-0.035033777,-0.10013769,-0.017908309,-0.035641946,0.25871772,0.19949748,-0.1849679,0.21600077,-0.13710514,0.08084408,0.20977215,-4.521937,-0.35571948,0.18304774,-0.060077406,-0.14225577,0.13972038,0.2728283,-0.29642493,-0.15981214,0.059535995,0.052950513,0.20715867,0.11381083,0.47855186,-1.7604489,-0.052451998,-0.24688263,0.12576339,0.17233576,-0.18516853,0.16152088,0.0036834688,0.036961064,-0.12903051,-0.14155145,0.016369931,0.072929,-0.2146213,-0.06947004,0.4131479,-0.19523646,5.004626e-05,-0.071041256,0.005212869,0.2963776,-0.12977226,0.061277516,-0.21013656,0.080948286,-0.33427113,-0.06750626,0.83697546,0.16163653,0.0072474987,0.38737774,-0.41354755,-0.18359387,0.25682005,-0.08790846,-0.13408832,0.07856024,-0.28373936,0.13976258,-0.15138814,-0.06844765,0.2743068,0.079126924,0.15108584,-0.36093882,-0.40317646,0.13027261,0.09185806,-0.09361473,-0.06052214,0.29020244,0.24106075,0.04411921,0.14030728,0.2874692,0.07682777,0.030923754,-0.029112965,-0.092250064,-0.07043587,0.54382974,0.45117638,-0.11229182,-0.1913818,-0.24080414,-0.22024654,0.06467484,-0.07870128,-0.049326483,0.13031533,0.3570546,0.08490238,0.21267451,-0.03858636,-0.042493466,-0.12362988,-0.24734332,0.11158671,-0.047437277,-0.05508202,-0.0347088,-0.2620983,-0.18165423,0.15481965,-0.16655721,0.08540642,-0.1122543,-0.38343558,-0.13874082,0.040199604,-0.021069761,0.071746886,0.055234246,-0.14750493,0.5984265,0.21836555,0.17329736,-0.12233204,0.115712404,0.22577737,0.24222283,0.09135799,-0.2363779,-0.08121375,-0.80591714,-0.029800594,0.30129707,0.09801721,-0.23600164,-0.16256557,-0.10298105,-0.37200144,0.66299593,-0.085941255,0.19093291,-0.26152208,-0.10880258,-0.037056223,0.22563817,0.009981303,0.23053135,-0.099994086,0.19573069,0.10005316,-0.20875946,0.054698177,0.07911441,-0.049204413,0.37858436,-0.5173978,-0.17556675,-0.18446659,0.045902643,-0.4168821,0.1610229,0.12985331,0.5283546,-0.076265864,-0.40962964,0.50787574,0.1903679,0.29418904,0.18839227,-0.09753425,-0.07728761,0.27748632,-0.012395126,-0.22045814,0.10961758,0.25130746,-0.0151236765,0.24764064,-1.186246,0.12657113,-0.10757666,0.08721886,0.047254086,0.85361,0.21545777,-0.02811228,-0.28657222,0.16244358,-0.1363702,0.048019815,-0.1479528,-0.019794764,-0.17713596,0.123342454,0.09073371,0.1379781,-0.030635126,-0.1513686,-0.045202374,0.12160913,-0.060482215,0.0867335,0.11619202,-0.054833945,-0.10549737,-0.15181051,0.14321575,0.0084218625,0.10354603,0.0626052,-0.15889089,0.23229307,-0.23414253,0.13898896,0.005439173,-0.028562058,-0.26866984,0.15183331,0.122625835,-0.1357603,-0.07280326,-0.07282429,0.12658416,-0.07412374,-0.038714,-0.2545072,0.18810128,0.3304044,0.30180743,-0.186606,0.19845584,0.1562418,0.83597183,0.33332133,0.17021853,-0.0648011,0.31210926,0.10463193,-0.11616776,0.29779273,-0.3547623,1.2049203,-0.16505784,0.13856418,-0.08835666,0.12216681,-0.32866475,0.02569759,0.20731172,-0.37518498,0.0023318967,0.063044995,0.0793727,-0.1490392,0.29226014,0.24858871,0.27433503,0.016559072,0.038984433,0.08337651,-0.24161534,-0.32788274,-0.05944058,0.40760854,0.29731685,-0.017871436,0.04551248,0.17682834,0.60011154,-0.12210007,0.39861405,0.12758988,-0.12422051,-0.1289292,-0.19584483,0.025491921,0.07598694,0.50145584,-0.02279198,-0.42230788,-0.05004385,0.2470579,0.071500264,0.31489334,-1.1546588,-0.316383,0.04091821,-0.07731385,0.15350811,-0.20034032,-0.1618004,-0.13389444,0.11498481,-0.047112223,0.10218142,-0.05369843,-0.048132043,0.36757782,-0.6628601,-0.2859983,-0.15004694,0.34884354,0.034166574,-0.1639795,0.100161254,-0.06845195,-0.10324932,0.083567,-0.005927841,-0.1378407,0.0429532,-0.09208936,-0.3072018,-0.15667166,-0.19751316,-0.085928984,-0.049220152,-0.13422057,-0.16144869,0.04797861,0.041761573,0.14691769,0.21516263,-0.14809623,0.04858565,-0.037472147,-0.38844937,-0.2834652,0.61929345,0.30672964,0.23361807,-0.065604895,-0.18464574,0.5336331,-0.2159114,-0.2311963,0.12972572,-0.17176142,-0.23287328,0.109148115,-0.37229595,-0.11052731,-0.14767092,0.30282006,-0.31552717,-0.33215088,0.13601553,-0.20157424,-1.2251997,0.167479,-0.20513651,0.100429475,-0.005563711,0.24676703,-0.30200526,-0.27775973,-0.046345126,0.16175555,-0.1905617,-0.079370536,0.28012303,-0.1126582,-0.07549719,0.17535697,0.2198494,-0.25004476,0.038203403,-0.059389878,-0.37815684,-0.3462711,-0.34072492,0.2364353,0.21153891,0.1716125,-0.043670785,0.12847753,-0.019124934,-0.20377022,-0.21016042,-0.04674953,0.32453057,0.029447041,-0.18711251,-0.24183865,-0.1599404,-0.18555276,0.27718875,0.17139016,0.0003403707,-0.3771657,0.06533717,-0.0026611085,-0.18750423,-0.16868612,-0.32268608,-0.21399643,-0.36703575,0.011541247,-0.036960624,0.184187,-0.38431406,-0.33078957,0.17835385,-0.6569858,0.24135335,0.18852004,0.15012428,0.0055337055,-0.06181671,-0.08003951,0.123517044,-0.18805777,0.24220833,-0.41356644,-0.1714603,-0.004229625,-0.058167547,-0.114840664,-0.1401176,-0.4399871,0.06877011,0.09410718,-0.035860237,0.23149988,0.024643445,0.07154468,0.096060716,0.46924707,-0.08328295,0.38803142,0.018264366,-0.16422106]', '2026-06-23 16:02:55.432433');


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
