<?php
require 'db.php';
include 'header.php';

$error = '';

if ($_SERVER['REQUEST_METHOD'] === 'POST') {
    $title = trim($_POS T['title']);
    $author = trim($_POST['author']);
    $pages = (int)$_POST['pages'];
    $genre = trim($_POST['genre']);

    
    if (empty($title) || empty($author) || $pages <= 0 || empty($genre)) {
        $error = "All fields are required, and pages must be greater than 0.";
    } else {
        $stmt = $pdo->prepare("INSERT INTO books (title, author, pages, genre) VALUES (?, ?, ?, ?)");
        $stmt->execute([$title, $author, $pages, $genre]);
        header("Location: index.php");
        exit;
    }
}
?>

<h2>Add a New Book</h2>
<?php if ($error): ?><p style="color:red; font-weight:bold;"><?= $error ?></p><?php endif; ?>

<form method="POST" action="add.php" onsubmit="return confirm('Are you sure you want to add this book to your library?');">
    <div class="form-group">
        <label>Title:</label>
        <input type="text" name="title" required>
    </div>
    <div class="form-group">
        <label>Author:</label>
        <input type="text" name="author" required>
    </div>
    <div class="form-group">
        <label>Pages:</label>
        <input type="number" name="pages" required min="1">
    </div>
    <div class="form-group">
        <label>Genre:</label>
        <select name="genre" required>
            <option value="">Select Genre...</option>
            <option value="Fiction">Fiction</option>
            <option value="Non-Fiction">Non-Fiction</option>
            <option value="Sci-Fi">Sci-Fi</option>
            <option value="Fantasy">Fantasy</option>
        </select>
    </div>
    <button type="submit" class="btn btn-success">Save Book</button>
    <a href="index.php" class="btn" onclick="return confirm('Cancel adding this book?');">Cancel</a>
</form>

</body></html>