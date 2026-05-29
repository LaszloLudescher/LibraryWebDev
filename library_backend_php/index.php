<?php
ini_set('display_errors', 1);
ini_set('display_startup_errors', 1);
error_reporting(E_ALL);

include 'header.php'; 
?>
<h2>Library Dashboard</h2>

<div class="form-group">
    <label for="genreFilter">Filter Genre:</label>
    <select id="genreFilter">
        <option value="">All Categories</option>
        <option value="Fiction">Fiction</option>
        <option value="Non-Fiction">Non-Fiction</option>
        <option value="Sci-Fi">Sci-Fi</option>
        <option value="Fantasy">Fantasy</option>
    </select>
    <button class="btn" onclick="fetchBooks()">Apply Filter</button>
</div>

<p id="lastFilterDisplay" style="color: #666; font-style: italic; font-size: 0.9em;"></p>

<div id="booksContainer">
    </div>

<script>
function fetchBooks() {
    const genre = document.getElementById('genreFilter').value;

    const previousFilter = localStorage.getItem('lastLibraryFilter');
    const displayValue = previousFilter ? previousFilter : 'None (First Visit)';
    document.getElementById('lastFilterDisplay').innerText = "Filter used for the previous browsing action: " + displayValue;
    

    localStorage.setItem('lastLibraryFilter', genre === '' ? 'All Categories' : genre);


    const xhr = new XMLHttpRequest();
    xhr.open('GET', 'get_books.php?genre=' + encodeURIComponent(genre), true);
    xhr.onload = function() {
        if (this.status === 200) {
            document.getElementById('booksContainer').innerHTML = this.responseText;
        }
    };
    xhr.send();
}

function confirmDelete(id) {
    if (confirm("Are you sure you want to delete this book? This action cannot be undone.")) {
        window.location.href = 'delete.php?id=' + id;
    }
}

window.onload = fetchBooks;
</script>

</body>
</html>