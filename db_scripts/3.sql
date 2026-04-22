-- ==============================================================================
-- XÓA BẢNG CŨ NẾU ĐÃ TỒN TẠI (Giúp bạn dễ dàng chạy lại script nhiều lần)
-- ==============================================================================
DROP TABLE IF EXISTS ai_activity_logs CASCADE;
DROP TABLE IF EXISTS ai_models_courses CASCADE;
DROP TABLE IF EXISTS ai_models CASCADE;
DROP TABLE IF EXISTS system_configs CASCADE;
DROP TABLE IF EXISTS user_reports CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS messages CASCADE;
DROP TABLE IF EXISTS chats CASCADE;
DROP TABLE IF EXISTS transactions CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS order_info CASCADE;
DROP TABLE IF EXISTS reviews CASCADE;
DROP TABLE IF EXISTS cart_items CASCADE;
DROP TABLE IF EXISTS wishlist_items CASCADE;
DROP TABLE IF EXISTS enrollment CASCADE;
DROP TABLE IF EXISTS enrollments CASCADE;
DROP TABLE IF EXISTS learning_materials CASCADE;
DROP TABLE IF EXISTS lessons CASCADE;
DROP TABLE IF EXISTS courses CASCADE;
DROP TABLE IF EXISTS coupons CASCADE;
DROP TABLE IF EXISTS categories CASCADE;
DROP TABLE IF EXISTS instructors CASCADE;
DROP TABLE IF EXISTS managers CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS accounts CASCADE;
DROP TABLE IF EXISTS material_p_hashes CASCADE;
DROP TABLE IF EXISTS material_embeddings CASCADE;


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
    password_hash TEXT,
    phone_number VARCHAR(50),
    account_status VARCHAR(50), -- VD: 'active', 'suspended', 'banned'
    account_flag_count INT DEFAULT 0, -- Theo dõi số lần account bị report ở mục reviews (ví dụ 1 lần thì xóa comment và cảnh cáo, 2 lần cấm comment trong 1 tháng, 3 lần ban luôn acc)
    auth_provider VARCHAR(50),  -- VD: 'local', 'google', 'facebook'
    avatar_url TEXT,
	refresh_token TEXT,
    refresh_token_expiry_time TIMESTAMP,
	is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    account_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_last_login_at TIMESTAMP
);

CREATE TABLE users (
    user_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    bio TEXT,
    date_of_birth DATE
    -- total_spent NUMERIC(12, 2) DEFAULT 0.00, -- SUM of their order_info.total_amount
    -- enrolled_courses_count INT DEFAULT 0 -- COUNT(*) of their enrollments
);



CREATE TABLE managers (
    manager_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    role VARCHAR(50), -- VD: 'admin', 'staff'
    display_name VARCHAR(255) NOT NULL
);

CREATE TABLE instructors (
    instructor_id INT PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    stripe_account_id VARCHAR(255),
    stripe_onboarding_status VARCHAR(50),
    payouts_enabled BOOLEAN DEFAULT FALSE,
    charges_enabled BOOLEAN DEFAULT FALSE
    -- instructor_rating NUMERIC(3,2) DEFAULT 0.0, -- AVERAGE of their courses.rating_average
    -- total_revenue NUMERIC(12, 2) DEFAULT 0.00 -- SUM of their instructor_payouts.payout_amount
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
    coupon_type VARCHAR(50), -- VD: 'percentage', 'fixed_amount'
    discount_value NUMERIC(10, 2) NOT NULL,
    start_date TIMESTAMP,
    end_date TIMESTAMP,
    usage_limit INT, -- giới hạn số lần sử dụng của coupon này
    used_count INT DEFAULT 0, -- mỗi lần learner sử dụng coupon này khi checkout thì sẽ tăng lên 1 (max = usage_limit)
    is_active BOOLEAN DEFAULT TRUE
);

CREATE TABLE courses (
    course_id SERIAL PRIMARY KEY,
    instructor_id INT REFERENCES instructors(instructor_id) ON DELETE SET NULL,
    category_id INT REFERENCES categories(category_id) ON DELETE SET NULL,
    coupon_id INT REFERENCES coupons(coupon_id) ON DELETE SET NULL,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    price NUMERIC(10, 2) NOT NULL, -- giá gốc trước khi qua discount
    course_thumbnail_url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    course_status VARCHAR(50), -- VD: 'draft', 'published', 'archived'
    course_flag_count INT DEFAULT 0 -- Theo dõi số lần course bị report (1 lần, 2 lần , 3 lần sẽ có mức phạt ngày càng nặng, cần lên Udemy tham khảo thêm)
    
	
);



CREATE TABLE lessons (
    lesson_id SERIAL PRIMARY KEY,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    thumbnail_url TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    lesson_status VARCHAR(50)
	
);

CREATE TABLE learning_materials (
    material_id SERIAL PRIMARY KEY,
    lesson_id INT REFERENCES lessons(lesson_id) ON DELETE CASCADE,
    title VARCHAR(255) NOT NULL,
    description TEXT,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    learning_status VARCHAR(50), -- Trạng thái của 1 cái learning_material ( vd: active, auditing, inactive)
    material_url TEXT,
	--- Bỏ duration INT
	--- Thêm 2 cái dưới
	material_metadata JSONB, -- Lưu file_size, file_type, file_extension, duration và page_count (for PDF, Word, powerpoint)
	material_hash CHAR(32) -- Hash MD5 để check duplication cho file ko qua chỉnh sửa
	
);
   
-- p_hash dùng cho video và image, check duplication cho file đã qua chỉnh sửa ( bit_count(p_hash XOR :new_hash) )
CREATE TABLE material_p_hashes (
    material_id INT PRIMARY KEY REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    p_hash BIGINT[] NOT NULL, 
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Check duplication thông qua AI (tính cosine similarity của embedding) 
CREATE TABLE material_embeddings (
    embedding_id SERIAL PRIMARY KEY,
    material_id INT REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    embedding vector(768),
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 3. NHÓM HỌC TẬP & TƯƠNG TÁC (Learning & Engagement)
-- ==============================================================================

CREATE TABLE enrollments (
    enrollment_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
	UNIQUE(user_id,course_id),
    title VARCHAR(255),
    description TEXT,
    completed_date DATE,
    is_completed BOOLEAN DEFAULT FALSE,
    enroll_date DATE DEFAULT CURRENT_DATE,
    last_accessed_at TIMESTAMP,
    enrollment_status VARCHAR(50) -- VD: 'active', 'completed', 'dropped'
);

CREATE TABLE enrollment_progress (
	enrollment_id INT PRIMARY KEY REFERENCES enrollments(enrollment_id) ON DELETE CASCADE,
	learned_material_count INT NOT NULL DEFAULT 0, -- Lưu số lượng material đã học qua trong course, dùng để hiển thị progress bar trên UI = learned_material_count/courses.total_materials
	last_modified_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE wishlist_items (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE CASCADE,
	UNIQUE(user_id,course_id),
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE reviews (
    review_id SERIAL PRIMARY KEY,
	enrollment_id INT REFERENCES enrollments(enrollment_id) ON DELETE CASCADE,
    rating NUMERIC(3,2) CHECK (rating >= 0 AND rating <= 5),
    comment TEXT,
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
	UNIQUE(user_id,course_id),
    added_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    price NUMERIC(10, 2)
);

CREATE TABLE order_info (
    order_id SERIAL PRIMARY KEY,
    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    order_date TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    order_status VARCHAR(50), -- VD: 'pending', 'paid', 'cancelled'
    payment_method VARCHAR(50)
    -- total_amount NUMERIC(10, 2) NOT NULL, -- SUM of order_items.purchase_price
    -- BỎ CỘT discount_amount do total_amount là giá sau giảm
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT REFERENCES order_info(order_id) ON DELETE CASCADE,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
    purchase_price NUMERIC(10, 2) NOT NULL,
	coupon_used BOOLEAN DEFAULT FALSE
);

CREATE TABLE transactions (
    transaction_id SERIAL PRIMARY KEY,
    order_item_id INT REFERENCES order_items(id) ON DELETE SET NULL, -- Mỗi transaction tương ứng với 1 item trong order thay vì cả order
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

-- Bảng lưu dữ liệu những giao dịch chuyển tiền từ hệ thống vô tài khoản ngân hàng của instructor
CREATE TABLE instructor_payouts (
	payout_id SERIAL PRIMARY KEY, 
	transaction_id INT REFERENCES transactions(transaction_id) ON DELETE SET NULL,
	instructor_id INT REFERENCES instructors(instructor_id) ON DELETE SET NULL,
	payout_amount NUMERIC(10,2) NOT NULL, -- Số tiền sẽ chuyển cho instructor (đã trừ bớt phần sàn ăn)
	payout_date TIMESTAMP NOT NULL, -- Ngày mà hệ thống sẽ chuyển tiền cho instructor (theo lịch đã lên) 
	is_paid BOOLEAN NOT NULL DEFAULT FALSE
);

-- ==============================================================================
-- 5. NHÓM GIAO TIẾP & HỖ TRỢ (Communication & Reports)
-- ==============================================================================

CREATE TABLE chats (
    chat_id SERIAL PRIMARY KEY,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    last_message_at TIMESTAMP
);

CREATE TABLE messages (
    message_id SERIAL PRIMARY KEY,
    chat_id INT REFERENCES chats(chat_id) ON DELETE SET NULL,
    sender_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    receiver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    content TEXT NOT NULL,
    is_seen BOOLEAN DEFAULT FALSE,
    sent_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    received_at TIMESTAMP
);

CREATE TABLE notifications (
    notification_id SERIAL PRIMARY KEY,
    sender_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    receiver_id INT REFERENCES accounts(account_id) ON DELETE CASCADE,
    title VARCHAR(255),
    content TEXT,
    link_action TEXT,
    is_read BOOLEAN DEFAULT FALSE,
	is_removed BOOLEAN DEFAULT FALSE, -- THÊM XÓA MỀM
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE user_reports (
    report_id SERIAL PRIMARY KEY,
    reporter_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    target_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    resolver_id INT REFERENCES accounts(account_id) ON DELETE SET NULL,
    reason VARCHAR(255),
    description TEXT,
    user_reports_status VARCHAR(50), -- VD: 'pending', 'resolved', 'dismissed'
    resolution_note TEXT,
    resolved_at TIMESTAMP,
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
    model_type VARCHAR(50), -- VD: 'LLM', 'Image Gen'
    model_provider VARCHAR(50),   -- VD: 'OpenAI', 'Gemini'
    model_version VARCHAR(50),
    model_status VARCHAR(50),
    description TEXT,
    model_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    model_updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE ai_models_courses (
    id SERIAL PRIMARY KEY,
    model_id INT REFERENCES ai_models(model_id) ON DELETE SET NULL,
    course_id INT REFERENCES courses(course_id) ON DELETE SET NULL,
	UNIQUE(model_id,course_id),
    role VARCHAR(50),
    is_enabled BOOLEAN DEFAULT TRUE,
    config_json JSONB,
    assigned_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE ai_activity_logs (
    log_id SERIAL PRIMARY KEY,
    ai_model_course_id INT REFERENCES ai_models_courses(id) ON DELETE SET NULL,
    user_id INT REFERENCES users(user_id) ON DELETE SET NULL,
    interaction_type VARCHAR(50), -- VD: 'moderation', 'grading', 'question'
    input_json JSONB,
    output_json JSONB,
    latency_ms REAL,
    token_usage REAL,
    log_status VARCHAR(50),
    error_message TEXT,
    log_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- ==============================================================================
-- 7. VIEWS FOR DATA CONSISTENCY (The "Utmost Normalized" Part)
-- ==============================================================================

-- View to get Lesson Stats 
CREATE OR REPLACE VIEW view_lesson_stats AS
SELECT 
    l.lesson_id,
    l.course_id,

    COUNT(lm.material_id) AS material_count,

    COALESCE(SUM(
        (lm.material_metadata->>'duration')::INT
    ), 0) AS lesson_duration

FROM lessons l
LEFT JOIN learning_materials lm 
    ON lm.lesson_id = l.lesson_id

GROUP BY l.lesson_id, l.course_id;

-- View for Course Stats
CREATE OR REPLACE VIEW view_course_stats AS
SELECT 
    c.course_id,

    COALESCE(AVG(r.rating), 0) AS rating_average,

    COUNT(DISTINCT e.enrollment_id) AS total_students,

    COUNT(DISTINCT r.review_id) AS total_reviews,

    COUNT(DISTINCT l.lesson_id) AS total_lessons,

    COALESCE(SUM(vls.material_count), 0) AS total_materials,

    COALESCE(SUM(vls.lesson_duration), 0) AS total_duration

FROM courses c

LEFT JOIN enrollments e 
    ON e.course_id = c.course_id

LEFT JOIN reviews r 
    ON r.enrollment_id = e.enrollment_id
   AND r.is_removed = FALSE

LEFT JOIN lessons l 
    ON l.course_id = c.course_id

LEFT JOIN view_lesson_stats vls 
    ON vls.course_id = c.course_id

GROUP BY c.course_id;


-- View for User Stats
CREATE OR REPLACE VIEW view_user_stats AS
SELECT 
    u.user_id,

    COUNT(DISTINCT e.enrollment_id) AS enrolled_courses_count,

    COALESCE(SUM(oi.purchase_price), 0) AS total_spent

FROM users u

LEFT JOIN enrollments e 
    ON e.user_id = u.user_id

LEFT JOIN order_info o 
    ON o.user_id = u.user_id
   AND o.order_status = 'paid'

LEFT JOIN order_items oi 
    ON oi.order_id = o.order_id

GROUP BY u.user_id;


-- View for Order Stats
CREATE OR REPLACE VIEW view_order_stats AS
SELECT 
    o.order_id,
    o.user_id,

    COALESCE(SUM(oi.purchase_price), 0) AS total_amount

FROM order_info o

LEFT JOIN order_items oi 
    ON oi.order_id = o.order_id

GROUP BY o.order_id, o.user_id;

-- View for Instructor Stats
CREATE OR REPLACE VIEW view_instructor_stats AS
SELECT 
    i.instructor_id,

    COALESCE(AVG(r.rating), 0) AS instructor_rating,

    COALESCE(SUM(ip.payout_amount), 0) AS total_revenue,

    COUNT(DISTINCT e.enrollment_id) AS total_students_count

FROM instructors i

LEFT JOIN courses c 
    ON c.instructor_id = i.instructor_id

LEFT JOIN enrollments e 
    ON e.course_id = c.course_id

LEFT JOIN reviews r 
    ON r.enrollment_id = e.enrollment_id
   AND r.is_removed = FALSE

LEFT JOIN instructor_payouts ip 
    ON ip.instructor_id = i.instructor_id

GROUP BY i.instructor_id;

--Indexing
CREATE INDEX idx_reviews_enrollment ON reviews(enrollment_id);
CREATE INDEX idx_enrollments_course ON enrollments(course_id);
CREATE INDEX idx_enrollments_user ON enrollments(user_id);
CREATE INDEX idx_courses_instructor ON courses(instructor_id);
CREATE INDEX idx_lessons_course ON lessons(course_id);
CREATE INDEX idx_materials_lesson ON learning_materials(lesson_id);
CREATE INDEX idx_order_info_user ON order_info(user_id);
CREATE INDEX idx_order_items_order ON order_items(order_id);
CREATE INDEX idx_reviews_active ON reviews(enrollment_id) WHERE is_removed = FALSE;
CREATE INDEX idx_order_paid ON order_info(user_id) WHERE order_status = 'paid';
CREATE INDEX idx_material_duration 
ON learning_materials (
    ((material_metadata->>'duration')::INT)
);
CREATE INDEX idx_metadata_gin 
ON learning_materials 
USING GIN (material_metadata);

-- ==============================================================================
-- 8. SAMPLE DATA (EXCLUDING ACCOUNTS)
-- ==============================================================================

INSERT INTO categories (category_id, categories_name, description, category_status) 
VALUES 
(1, 'Design & Creative', 'Courses related to design, UX/UI, 3D modeling, and creative arts.', 'active'), 
(2, 'Development', 'Software development, programming languages, web & mobile dev.', 'active'), 
(3, 'Business', 'Business management, finance, marketing, and entrepreneurship.', 'active')
ON CONFLICT DO NOTHING;

-- ==============================================================================
-- 9. SAMPLE DATA FOR PRIMARY ACCOUNT (phuoctai228)
-- ==============================================================================

INSERT INTO accounts (account_id, email, password_hash, account_status, auth_provider, is_verified)
VALUES (1, 'instructor@gmail.com', '$2a$11$w/itTk1gtGMzyoG6pw0Sxe2aNOjeKAGDdyW9BA/HdhUUK6HHGUoRG', 'active', 'local', TRUE)
ON CONFLICT (account_id) DO NOTHING;

INSERT INTO users (user_id, full_name)
VALUES (1, 'instructor')
ON CONFLICT (user_id) DO NOTHING;

INSERT INTO instructors (instructor_id)
VALUES (1)
ON CONFLICT (instructor_id) DO NOTHING;

-- ==============================================================================
-- 10. SYNC SEQUENCES (Prevent duplicate key errors)
-- ==============================================================================

SELECT setval(pg_get_serial_sequence('accounts', 'account_id'), (SELECT MAX(account_id) FROM accounts));
SELECT setval(pg_get_serial_sequence('categories', 'category_id'), (SELECT MAX(category_id) FROM categories));
