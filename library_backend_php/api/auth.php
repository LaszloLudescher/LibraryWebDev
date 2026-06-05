<?php

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../db.php';

$method = $_SERVER['REQUEST_METHOD'];
$action = $_GET['action'] ?? '';

if ($method === 'GET' && $action === 'status') {
    if (!empty($_SESSION['username'])) {
        echo json_encode(['authenticated' => true, 'username' => $_SESSION['username']]);
    } else {
        echo json_encode(['authenticated' => false]);
    }
    exit;
}

if ($method === 'POST' && $action === 'login') {
    $data = json_decode(file_get_contents('php://input'), true);
    $username = trim($data['username'] ?? '');
    $password = $data['password'] ?? '';

    if (!$username || !$password) {
        http_response_code(400);
        echo json_encode(['error' => 'Username and password are required.']);
        exit;
    }

    $stmt = $pdo->prepare("SELECT password_hash FROM users WHERE username = ?");
    $stmt->execute([$username]);
    $row = $stmt->fetch();

    if ($row && password_verify($password, $row['password_hash'])) {
        $_SESSION['username'] = $username;
        echo json_encode(['username' => $username, 'message' => 'Logged in.']);
    } else {
        http_response_code(401);
        echo json_encode(['error' => 'Invalid username or password.']);
    }
    exit;
}

if ($method === 'POST' && $action === 'logout') {
    session_destroy();
    echo json_encode(['message' => 'Logged out.']);
    exit;
}

if ($method === 'POST' && $action === 'register') {
    $data = json_decode(file_get_contents('php://input'), true);
    $username = trim($data['username'] ?? '');
    $password = $data['password'] ?? '';

    if (strlen($username) < 3) {
        http_response_code(400);
        echo json_encode(['error' => 'Username must be at least 3 characters.']);
        exit;
    }
    if (strlen($password) < 6) {
        http_response_code(400);
        echo json_encode(['error' => 'Password must be at least 6 characters.']);
        exit;
    }

    $stmt = $pdo->prepare("SELECT id FROM users WHERE username = ?");
    $stmt->execute([$username]);
    if ($stmt->fetch()) {
        http_response_code(409);
        echo json_encode(['error' => 'Username already taken.']);
        exit;
    }

    $hash = password_hash($password, PASSWORD_DEFAULT);
    $stmt = $pdo->prepare("INSERT INTO users (username, password_hash) VALUES (?, ?)");
    $stmt->execute([$username, $hash]);

    $_SESSION['username'] = $username;
    echo json_encode(['username' => $username, 'message' => 'Registered.']);
    exit;
}

http_response_code(404);
echo json_encode(['error' => 'Not found.']);
