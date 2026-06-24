-- ==============================================================================
-- 1. DROP TỒN TẠI (Để an toàn nếu bạn lỡ chạy lỗi một nửa hoặc muốn chạy lại)
-- ==============================================================================
DROP TABLE IF EXISTS quiz_attempt_answers CASCADE;
DROP TABLE IF EXISTS quiz_attempt_questions CASCADE;
DROP TABLE IF EXISTS quiz_attempts CASCADE;
DROP TABLE IF EXISTS course_quizzes CASCADE;
DROP TABLE IF EXISTS quiz_lesson_distributions CASCADE;
DROP TABLE IF EXISTS quiz_options CASCADE;
DROP TABLE IF EXISTS quiz_questions CASCADE;
DROP TABLE IF EXISTS quizzes CASCADE;

-- ==============================================================================
-- 2. TẠO BẢNG NHÓM QUIZ (Quiz Management)
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

-- ==============================================================================
-- 3. TẠO INDEXES (Tối ưu tìm kiếm)
-- ==============================================================================
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