<?php
// cors.php — include at the top of every api/*.php file

// Allow the static frontend origin (adjust if yours differs)
$allowed_origins = [
    'http://localhost',
    'http://localhost:8080',
    'http://localhost:5500',
    'http://127.0.0.1:5500',
];

$origin = $_SERVER['HTTP_ORIGIN'] ?? '';
if (in_array($origin, $allowed_origins)) {
    header('Access-Control-Allow-Origin: ' . $origin);
} else {
    // file:// sends no Origin header — allow for dev convenience
    header('Access-Control-Allow-Origin: ' . ($origin ?: '*'));
}

header('Access-Control-Allow-Credentials: true');
header('Access-Control-Allow-Methods: GET, POST, PUT, DELETE, PATCH, OPTIONS');
header('Access-Control-Allow-Headers: Content-Type');

// Handle pre-flight
if ($_SERVER['REQUEST_METHOD'] === 'OPTIONS') {
    http_response_code(200);
    exit;
}

header('Content-Type: application/json; charset=utf-8');
session_start();
