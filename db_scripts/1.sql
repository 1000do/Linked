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

-- ==============================================================================
-- 1. NHÓM QUẢN LÝ TÀI KHOẢN (Account & User Management)
-- ==============================================================================

CREATE TABLE accounts (
    account_id SERIAL PRIMARY KEY,
    email VARCHAR(255) UNIQUE NOT NULL,
    password_hash TEXT,
    phone_number VARCHAR(50),
    account_status VARCHAR(50), -- VD: 'active', 'suspended', 'banned'
    account_flag_count INT DEFAULT 0,
    auth_provider VARCHAR(50),  -- VD: 'local', 'google', 'facebook'
    avatar_url TEXT,

	refresh_token TEXT,
    refresh_token_expiry_time TIMESTAMP,

    account_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_updated_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    account_last_login_at TIMESTAMP
);

CREATE TABLE users (
    user_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    full_name VARCHAR(255) NOT NULL,
    bio TEXT,
    date_of_birth DATE,
    total_spent NUMERIC(12, 2) DEFAULT 0.00,
    enrolled_courses_count INT DEFAULT 0
);

CREATE TABLE managers (
    manager_id INT PRIMARY KEY REFERENCES accounts(account_id) ON DELETE CASCADE,
    role VARCHAR(50), -- VD: 'admin', 'moderator'
    display_name VARCHAR(255) NOT NULL
);

CREATE TABLE instructors (
    instructor_id INT PRIMARY KEY REFERENCES users(user_id) ON DELETE CASCADE,
    stripe_account_id VARCHAR(255),
    stripe_onboarding_status VARCHAR(50),
    payouts_enabled BOOLEAN DEFAULT FALSE,
    charges_enabled BOOLEAN DEFAULT FALSE,
    instructor_rating REAL DEFAULT 0.0,
    total_revenue NUMERIC(12, 2) DEFAULT 0.00
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
    course_status VARCHAR(50), -- VD: 'draft', 'published', 'archived'
    course_flag_count INT DEFAULT 0,
    total_lessons INT DEFAULT 0,
    rating_average REAL DEFAULT 0.0,
    total_students INT DEFAULT 0
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
    learning_status VARCHAR(50),
    material_url TEXT,
    duration VARCHAR(50)
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
    rating REAL CHECK (rating >= 0 AND rating <= 5),
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
    total_amount NUMERIC(10, 2) NOT NULL,
    discount_amount NUMERIC(10, 2) DEFAULT 0.00,
    order_status VARCHAR(50), -- VD: 'pending', 'paid', 'cancelled'
    payment_method VARCHAR(50)
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
    order_id INT REFERENCES order_info(order_id) ON DELETE SET NULL,
    amount NUMERIC(10, 2) NOT NULL,
    stripe_session_id VARCHAR(255),
    stripe_paymentintent_id VARCHAR(255),
    currency VARCHAR(10) DEFAULT 'VND',
    transactions_status VARCHAR(50), -- VD: 'succeeded', 'failed', 'refunded'
    transaction_type VARCHAR(50), -- VD: 'payment', 'payout'
    transaction_created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
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