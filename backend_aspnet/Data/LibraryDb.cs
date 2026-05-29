using LibraryBackend.Models;
using LibraryBackend.Data;
using Microsoft.AspNetCore.Identity;

namespace LibraryBackend.Data
{
    public class LibraryDb : IDisposable
    {
        private readonly SQLiteHelper _db;
        private static readonly PasswordHasher<string> _hasher = new();

        public LibraryDb(string dbPath)
        {
            _db = new SQLiteHelper(dbPath);
            Initialize();
        }

        private void Initialize()
        {
            _db.Execute(@"CREATE TABLE IF NOT EXISTS users (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                username TEXT NOT NULL UNIQUE,
                password_hash TEXT NOT NULL
            );");

            _db.Execute(@"CREATE TABLE IF NOT EXISTS books (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                title TEXT NOT NULL,
                author TEXT NOT NULL,
                pages INTEGER NOT NULL,
                genre TEXT NOT NULL,
                is_lent INTEGER NOT NULL DEFAULT 0,
                lent_to TEXT
            );");

            // Seed default admin user
            var count = _db.QueryScalar<long>("SELECT COUNT(*) FROM users");
            if (count == 0)
                _db.Execute("INSERT INTO users (username, password_hash) VALUES (?, ?)",
                    "admin", _hasher.HashPassword("admin", "admin123"));
        }

        // ── Auth ────────────────────────────────────────────────────────────────

        public bool ValidateUser(string username, string password)
        {
            var row = _db.QuerySingle("SELECT password_hash FROM users WHERE username = ?", username);
            if (row == null) return false;
            var hash = row["password_hash"]?.ToString() ?? "";
            var result = _hasher.VerifyHashedPassword("admin", hash, password);
            return result != PasswordVerificationResult.Failed;
        }

        public bool UserExists(string username) =>
            _db.QueryScalar<long>("SELECT COUNT(*) FROM users WHERE username = ?", username) > 0;

        public bool CreateUser(string username, string password)
        {
            try
            {
                _db.Execute("INSERT INTO users (username, password_hash) VALUES (?, ?)",
                    username, _hasher.HashPassword("admin", password));
                return true;
            }
            catch { return false; }
        }

        // ── Books ───────────────────────────────────────────────────────────────

        public List<Book> GetBooks(string? genre = null)
        {
            var rows = string.IsNullOrEmpty(genre)
                ? _db.Query("SELECT * FROM books ORDER BY id DESC")
                : _db.Query("SELECT * FROM books WHERE genre = ? ORDER BY id DESC", genre);
            return rows.Select(Map).ToList();
        }

        public Book? GetBook(long id)
        {
            var row = _db.QuerySingle("SELECT * FROM books WHERE id = ?", id);
            return row == null ? null : Map(row);
        }

        public long AddBook(Book b)
        {
            return _db.ExecuteInsert(
                "INSERT INTO books (title, author, pages, genre, is_lent, lent_to) VALUES (?,?,?,?,?,?)",
                b.Title, b.Author, b.Pages, b.Genre, b.IsLent ? 1 : 0, b.LentTo);
        }

        public bool UpdateBook(long id, Book b)
        {
            if (GetBook(id) == null) return false;
            _db.Execute("UPDATE books SET title=?,author=?,pages=?,genre=? WHERE id=?",
                b.Title, b.Author, b.Pages, b.Genre, id);
            return true;
        }

        public bool DeleteBook(long id)
        {
            if (GetBook(id) == null) return false;
            _db.Execute("DELETE FROM books WHERE id=?", id);
            return true;
        }

        public Book? ToggleLend(long id, string? lentTo)
        {
            var book = GetBook(id);
            if (book == null) return null;
            bool nowLent = !book.IsLent;
            _db.Execute("UPDATE books SET is_lent=?,lent_to=? WHERE id=?",
                nowLent ? 1 : 0, nowLent ? lentTo : null, id);
            return GetBook(id);
        }

        private static Book Map(Dictionary<string, object?> r) => new()
        {
            Id     = Convert.ToInt64(r["id"]),
            Title  = r["title"]?.ToString() ?? "",
            Author = r["author"]?.ToString() ?? "",
            Pages  = Convert.ToInt32(r["pages"]),
            Genre  = r["genre"]?.ToString() ?? "",
            IsLent = Convert.ToInt64(r["is_lent"]) == 1,
            LentTo = r["lent_to"]?.ToString()
        };

        public void Dispose() => _db.Dispose();
    }
}
