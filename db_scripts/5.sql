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
DROP TABLE IF EXISTS user_avatar_frames CASCADE;
DROP TABLE IF EXISTS avatar_frames CASCADE;

DROP TABLE IF EXISTS lesson_review_moderation_logs CASCADE;
DROP TABLE IF EXISTS course_review_moderation_logs CASCADE;
DROP TABLE IF EXISTS audit_logs CASCADE;
DROP TABLE IF EXISTS message_attachments CASCADE;

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

-- ─── AVATAR FRAMES SYSTEM ────────────────────────────────────────────────
CREATE TABLE avatar_frames (
    id SERIAL PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    image_url TEXT NOT NULL,
    description TEXT,
    requirement_type VARCHAR(50),
    requirement_value INT DEFAULT 0,
    is_active BOOLEAN DEFAULT TRUE,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_avatar_frames (
    user_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    frame_id INT REFERENCES avatar_frames(id) ON DELETE CASCADE,
    unlocked_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    is_equipped BOOLEAN DEFAULT FALSE,
    PRIMARY KEY (user_id, frame_id)
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
SELECT setval(pg_get_serial_sequence('avatar_frames', 'id'), (SELECT COALESCE(MAX(id), 1) FROM avatar_frames));
SELECT setval(pg_get_serial_sequence('gifts', 'gift_id'), (SELECT COALESCE(MAX(gift_id), 1) FROM gifts));

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

    -- Seeding Avatar Frames
    INSERT INTO avatar_frames (name, image_url, description, requirement_type, requirement_value)
    VALUES 
    ('Khung Admin Tối Thượng', '/img/frames/admin_gold.webp', 'Dành cho quản trị viên cao cấp', 'MANUAL_GRANT', 0),
    ('Tân Binh Chăm Chỉ', '/img/frames/newbie_teal.webp', 'Hoàn thành khóa học đầu tiên', 'FINISH_COURSE', 1);

    RAISE NOTICE 'Seeding Admin, Staff & Avatar Frames hoàn tất!';
END $$;




INSERT INTO 
ai_models (model_name,model_type,model_provider,model_version, model_path, model_status,description, process_type)
VALUES
('harmful_text_classifier','classifier','local','1','/app/models/spam_1/,/app/models/toxic_3/','active','an ensemble of spam and toxic text classifier that was fine-tuned from distilbert multilingual cased','text'),
('clip','embedding_generator','openai','1','openai/clip-vit-base-patch32','active','a multimodal model that was used to generate embeddings','media'),
('distilbert','embedding_generator','hugging_face','1','distilbert-base-multilingual-cased','active','a language model that was used to generate embeddings','text');

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


        