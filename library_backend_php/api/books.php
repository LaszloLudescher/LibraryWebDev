<?php

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../db.php';

if (empty($_SESSION['username'])) {
    http_response_code(401);
    echo json_encode(['error' => 'Not authenticated.']);
    exit;
}

$method = $_SERVER['REQUEST_METHOD'];
$id = isset($_GET['id']) ? (int)$_GET['id'] : null;

if ($method === 'GET') {
    if ($id) {
        $stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
        $stmt->execute([$id]);
        $book = $stmt->fetch();
        if (!$book) { http_response_code(404); echo json_encode(['error' => 'Book not found.']); exit; }
        echo json_encode(mapBook($book));
    } else {
        $genre = $_GET['genre'] ?? '';
        if ($genre !== '') {
            $stmt = $pdo->prepare("SELECT * FROM books WHERE genre = ? ORDER BY id DESC");
            $stmt->execute([$genre]);
        } else {
            $stmt = $pdo->query("SELECT * FROM books ORDER BY id DESC");
        }
        echo json_encode(array_map('mapBook', $stmt->fetchAll()));
    }
    exit;
}

if ($method === 'POST') {
    $data  = json_decode(file_get_contents('php://input'), true);
    $title  = trim($data['title']  ?? '');
    $author = trim($data['author'] ?? '');
    $pages  = (int)($data['pages'] ?? 0);
    $genre  = trim($data['genre']  ?? '');

    if (!$title || !$author || $pages < 1 || !$genre) {
        http_response_code(400);
        echo json_encode(['error' => 'All fields are required and pages must be > 0.']);
        exit;
    }

    $stmt = $pdo->prepare("INSERT INTO books (title, author, pages, genre, is_lent) VALUES (?,?,?,?,0)");
    $stmt->execute([$title, $author, $pages, $genre]);
    $newId = (int)$pdo->lastInsertId();

    http_response_code(201);
    $stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
    $stmt->execute([$newId]);
    echo json_encode(mapBook($stmt->fetch()));
    exit;
}

if ($method === 'PUT') {
    if (!$id) { http_response_code(400); echo json_encode(['error' => 'ID required.']); exit; }

    $data   = json_decode(file_get_contents('php://input'), true);
    $title  = trim($data['title']  ?? '');
    $author = trim($data['author'] ?? '');
    $pages  = (int)($data['pages'] ?? 0);
    $genre  = trim($data['genre']  ?? '');

    if (!$title || !$author || $pages < 1 || !$genre) {
        http_response_code(400);
        echo json_encode(['error' => 'All fields are required and pages must be > 0.']);
        exit;
    }

    $stmt = $pdo->prepare("UPDATE books SET title=?,author=?,pages=?,genre=? WHERE id=?");
    $stmt->execute([$title, $author, $pages, $genre, $id]);

    $stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
    $stmt->execute([$id]);
    $book = $stmt->fetch();
    if (!$book) { http_response_code(404); echo json_encode(['error' => 'Book not found.']); exit; }
    echo json_encode(mapBook($book));
    exit;
}

if ($method === 'DELETE') {
    if (!$id) { http_response_code(400); echo json_encode(['error' => 'ID required.']); exit; }

    $stmt = $pdo->prepare("DELETE FROM books WHERE id = ?");
    $stmt->execute([$id]);
    echo json_encode(['message' => 'Deleted.']);
    exit;
}

http_response_code(405);
echo json_encode(['error' => 'Method not allowed.']);

function mapBook(array $row): array {
    return [
        'id'      => (int)$row['id'],
        'title'   => $row['title'],
        'author'  => $row['author'],
        'pages'   => (int)$row['pages'],
        'genre'   => $row['genre'],
        'is_lent' => (bool)$row['is_lent'],
        'lent_to' => $row['lent_to'] ?? null,
    ];
}
