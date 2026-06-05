-- Run this script once to set up (or extend) the library_db database.
-- It is safe to run multiple times (IF NOT EXISTS / IF EXISTS guards).

CREATE DATABASE IF NOT EXISTS library_db
  CHARACTER SET utf8mb4
  COLLATE utf8mb4_unicode_ci;

USE library_db;

-- Books table
CREATE TABLE IF NOT EXISTS books (
  id       INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
  title    VARCHAR(255) NOT NULL,
  author   VARCHAR(255) NOT NULL,
  pages    INT          NOT NULL DEFAULT 0,
  genre    VARCHAR(100) NOT NULL DEFAULT '',
  is_lent  TINYINT(1)   NOT NULL DEFAULT 0,
  lent_to  VARCHAR(100)          DEFAULT NULL
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Add lent_to if upgrading from the original PHP lab schema
ALTER TABLE books
  ADD COLUMN IF NOT EXISTS lent_to VARCHAR(100) DEFAULT NULL;

-- Users table  (shared by PHP and Node backends)
CREATE TABLE IF NOT EXISTS users (
  id            INT          NOT NULL AUTO_INCREMENT PRIMARY KEY,
  username      VARCHAR(100) NOT NULL UNIQUE,
  password_hash VARCHAR(255) NOT NULL,
  created_at    TIMESTAMP    NOT NULL DEFAULT CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;

-- Optional: seed a test user  (password: test123)
-- The hash below was produced with bcrypt cost=10.
-- Remove or change before production use.
INSERT IGNORE INTO users (username, password_hash)
VALUES ('testuser', '$2b$10$hV3rNHKqxNJfflcjePMBheyYpNGBbXl6UwL0Bz2tdJi9vWbS.QHIO');
