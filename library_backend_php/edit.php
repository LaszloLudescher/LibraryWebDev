<?php
require 'db.php';
include 'header.php';

if (!isset($_GET['id'])) {
    die("No book ID provided.");
}
$id = $_GET['id'];


$stmt = $pdo->prepare("SELECT * FROM books WHERE id = ?");
$stmt->execute([$id]);
$book = $stmt->fetch();

if (!$book) die("Book not found.");

$error = '';
if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $title = trim($_POST['title']);
    $author = trim($_POST['author']);
    $pages = (int)$_POST['pages'];
    $genre = trim($_POST['genre']);

    if (empty($title) || empty($author) || $pages <= 0 || empty($genre)) {
        $error = "All fields are required, and pages must be greater than 0.";
    } else {
        $stmt = $pdo->prepare("UPDATE books SET title = ?, author = ?, pages = ?, genre = ? WHERE id = ?");
        $stmt->execute([$title, $author, $pages, $genre, $id]);
        header("Location: index.php");
        exit;
    }
}
?>

<h2>Edit Book Details</h2>
<?php if ($error): ?><p style="color:red; font-weight:bold;"><?= $error ?></p><?php endif; ?>

<form method="POST" action="edit.php?id=<?= $id ?>" onsubmit="return confirm('Save these changes?');">
    <div class="form-group">
        <label>Title:</label>
        <input type="text" name="title" value="<?= htmlspecialchars($book['title']) ?>" required>
    </div>
    <div class="form-group">
        <label>Author:</label>
        <input type="text" name="author" value="<?= htmlspecialchars($book['author']) ?>" required>
    </div>
    <div class="form-group">
        <label>Pages:</label>
        <input type="number" name="pages" value="<?= htmlspecialchars($book['pages']) ?>" required min="1">
    </div>
    <div class="form-group">
        <label>Genre:</label>
        <select name="genre" required>
            <option value="Fiction" <?= $book['genre'] == 'Fiction' ? 'selected' : '' ?>>Fiction</option>
            <option value="Non-Fiction" <?= $book['genre'] == 'Non-Fiction' ? 'selected' : '' ?>>Non-Fiction</option>
            <option value="Sci-Fi" <?= $book['genre'] == 'Sci-Fi' ? 'selected' : '' ?>>Sci-Fi</option>
            <option value="Fantasy" <?= $book['genre'] == 'Fantasy' ? 'selected' : '' ?>>Fantasy</option>
        </select>
    </div>
    <button type="submit" class="btn btn-success">Update Book</button>
    <a href="index.php" class="btn" onclick="return confirm('Discard edits and return to browsing?');">Cancel</a>
</form>

</body></html>