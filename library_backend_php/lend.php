<?php
require 'db.php';

if (isset($_GET['id']) && isset($_GET['current_status'])) {
    $newStatus = $_GET['current_status'] == 1 ? 0 : 1;
    
    $stmt = $pdo->prepare("UPDATE books SET is_lent = ? WHERE id = ?");
    $stmt->execute([$newStatus, $_GET['id']]);
}

header("Location: index.php");
exit;
?>