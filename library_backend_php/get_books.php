<?php
require 'db.php';

$genre = isset($_GET['genre']) ? $_GET['genre'] : '';


if ($genre === '') {
    $stmt = $pdo->query("SELECT * FROM books ORDER BY id DESC");
} else {
    $stmt = $pdo->prepare("SELECT * FROM books WHERE genre = ? ORDER BY id DESC");
    $stmt->execute([$genre]);
}

$books = $stmt->fetchAll();

if (count($books) > 0) {
    echo "<table>
            <tr>
                <th>Title</th>
                <th>Author</th>
                <th>Pages</th>
                <th>Genre</th>
                <th>Status</th>
                <th>Actions</th>
            </tr>";
            
    foreach ($books as $book) {
        $statusText = $book['is_lent'] ? "<span style='color:red;'>Lent Out</span>" : "<span style='color:green;'>Available</span>";
        $lendActionText = $book['is_lent'] ? "Mark Returned" : "Lend Book";
        $lendBtnClass = $book['is_lent'] ? "btn-success" : "btn";
        
        echo "<tr>";
        echo "<td>" . htmlspecialchars($book['title']) . "</td>";
        echo "<td>" . htmlspecialchars($book['author']) . "</td>";
        echo "<td>" . htmlspecialchars($book['pages']) . "</td>";
        echo "<td>" . htmlspecialchars($book['genre']) . "</td>";
        echo "<td>" . $statusText . "</td>";
        echo "<td>
                <a href='edit.php?id=" . $book['id'] . "' class='btn'>Edit</a> 
                <a href='lend.php?id=" . $book['id'] . "&current_status=" . $book['is_lent'] . "' class='btn " . $lendBtnClass . "'>" . $lendActionText . "</a> 
                <button onclick='confirmDelete(" . $book['id'] . ")' class='btn btn-danger'>Delete</button>
              </td>";
        echo "</tr>";
    }
    echo "</table>";
} else {
    echo "<p>No books found in this category.</p>";
}
?>