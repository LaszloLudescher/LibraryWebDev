<?php
// api/books_lend.php
// PATCH /api/books_lend.php?id=5
// Body: { "lent_to": "John" }  (null = mark as returned)

require_once __DIR__ . '/../cors.php';
require_once __DIR__ . '/../db.php';

if (empty($_SESSION['username'])) {
    http_response_code(401);
    echo json_encode(['error' => 'Not authenticated.']);
    exit;
}

$id   = isset($_GET['id']) ? (int)$_GET['id'] : null;
$data = json_decode(file_get_contents('php://input'), true);

if (!$id) { http_response_code(400); echo json_encode(['error' => 'ID required.']); exit; }

$stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
$stmt->execute([$id]);
$book = $stmt->fetch();
if (!$book) { http_response_code(404); echo json_encode(['error' => 'Book not found.']); exit; }

$nowLent = !(bool)$book['is_lent'];
$lentTo  = $nowLent ? ($data['lent_to'] ?? null) : null;

$stmt = $pdo->prepare("UPDATE books SET is_lent = ?, lent_to = ? WHERE id = ?");
$stmt->execute([$nowLent ? 1 : 0, $lentTo, $id]);

$stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
$stmt->execute([$id]);
$updated = $stmt->fetch();

echo json_encode([
    'id'      => (int)$updated['id'],
    'title'   => $updated['title'],
    'author'  => $updated['author'],
    'pages'   => (int)$updated['pages'],
    'genre'   => $updated['genre'],
    'is_lent' => (bool)$updated['is_lent'],
    'lent_to' => $updated['lent_to'] ?? null,
]);
