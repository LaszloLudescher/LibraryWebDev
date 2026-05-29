<!DOCTYPE html>
<html lang="en">
<head>
    <meta charset="UTF-8">
    <title>Personal Library Manager</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 30px; background-color: #f9f9f9; }
        h2 { color: #333; }
        .form-group { margin-bottom: 15px; }
        label { display: inline-block; width: 100px; font-weight: bold; }
        input[type="text"], input[type="number"], select { padding: 8px; width: 250px; border: 1px solid #ccc; border-radius: 4px; }
        table { border-collapse: collapse; width: 100%; margin-top: 15px; background: white; }
        th, td { border: 1px solid #ddd; padding: 12px; text-align: left; }
        th { background-color: #f2f2f2; }
        .nav { margin-bottom: 25px; padding-bottom: 15px; border-bottom: 2px solid #ddd; }
        .nav a { margin-right: 20px; text-decoration: none; font-weight: bold; color: #0056b3; }
        .btn { padding: 7px 12px; cursor: pointer; text-decoration: none; background: #e0e0e0; border: 1px solid #bbb; color: #333; border-radius: 4px; display: inline-block; }
        .btn:hover { background: #d0d0d0; }
        .btn-danger { background: #ffcdd2; color: #b71c1c; border-color: #ef9a9a; }
        .btn-danger:hover { background: #ef9a9a; }
        .btn-success { background: #c8e6c9; color: #1b5e20; border-color: #a5d6a7; }
    </style>
</head>
<body>
    <div class="nav">
        <a href="index.php">Browse Books</a>
        <a href="add.php">Add New Book</a>
    </div>