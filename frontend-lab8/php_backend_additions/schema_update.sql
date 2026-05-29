-- Run these in phpMyAdmin or MySQL CLI against your library_db database

-- 1. Add users table (for login/register)
CREATE TABLE IF NOT EXISTS users (
    id            INT AUTO_INCREMENT PRIMARY KEY,
    username      VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    created_at    TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- 2. Insert default admin user  (password: admin123)
-- The hash below is generated with PHP password_hash('admin123', PASSWORD_DEFAULT)
INSERT IGNORE INTO users (username, password_hash)
VALUES ('admin', '$2y$10$92IXUNpkjO0rOQ5byMi.Ye4oKoEa3Ro9llC/.og/at2uheWG/igi.');

-- 3. Add lent_to column to books if it doesn't exist yet
ALTER TABLE books ADD COLUMN IF NOT EXISTS lent_to VARCHAR(100) DEFAULT NULL;
