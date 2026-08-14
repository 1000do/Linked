CREATE TABLE IF NOT EXISTS checkout_sessions (
    checkout_session_id VARCHAR(50) PRIMARY KEY,
    user_id INT NOT NULL REFERENCES users(user_id) ON DELETE CASCADE,
    status VARCHAR(50) NOT NULL DEFAULT 'Pending', -- Pending, Completed, Expired, Cancelled
    total_amount NUMERIC(10, 2) NOT NULL,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    expires_at TIMESTAMP NOT NULL
);

CREATE TABLE IF NOT EXISTS gift_checkout_sessions (
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

CREATE TABLE IF NOT EXISTS material_exts (
    material_id INT PRIMARY KEY REFERENCES learning_materials(material_id) ON DELETE CASCADE,
    file_hash CHAR(32),
    
    CONSTRAINT uq_material_file_hash UNIQUE (file_hash)
);
-- Update AI models and system configs to use HuggingFace repo IDs instead of local paths
UPDATE ai_models 
SET model_path = 'ki4n-4nt/spam_text_classifier,ki4n-4nt/toxic_text_classifier' 
WHERE model_name = 'harmful_text_classifier';

UPDATE system_configs 
SET config_value = 'ki4n-4nt/spam_text_classifier,ki4n-4nt/toxic_text_classifier' 
WHERE config_key IN ('course_harmful_text_classifier', 'review_harmful_text_classifier');
