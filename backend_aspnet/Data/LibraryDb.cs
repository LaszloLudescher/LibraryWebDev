using Dapper;
using LibraryBackend.Models;
using MySqlConnector;
using BCrypt.Net;

namespace LibraryBackend.Data
{
    public class LibraryDb
    {
        private readonly string _connectionString;

        public LibraryDb(string connectionString)
        {
            _connectionString = connectionString;
        }

        private MySqlConnection GetConnection() => new MySqlConnection(_connectionString);

        // ── Auth ────────────────────────────────────────────────────────────────

        public bool ValidateUser(string username, string password)
        {
            using var db = GetConnection();
            var hash = db.QuerySingleOrDefault<string>("SELECT password_hash FROM users WHERE username = @u", new { u = username });
            if (hash == null) return false;

            return BCrypt.Net.BCrypt.Verify(password, hash);
        }

        public bool UserExists(string username)
        {
            using var db = GetConnection();
            return db.ExecuteScalar<int>("SELECT COUNT(*) FROM users WHERE username = @u", new { u = username }) > 0;
        }

        public bool CreateUser(string username, string password)
        {
            using var db = GetConnection();
            var hash = BCrypt.Net.BCrypt.HashPassword(password);
            return db.Execute("INSERT INTO users (username, password_hash) VALUES (@u, @h)", new { u = username, h = hash }) > 0;
        }

        // ── Books ───────────────────────────────────────────────────────────────

        public List<Book> GetBooks(string? genre = null)
        {
            using var db = GetConnection();
            var sql = "SELECT id as Id, title as Title, author as Author, pages as Pages, genre as Genre, is_lent as IsLent, lent_to as LentTo FROM books";
            if (string.IsNullOrEmpty(genre))
            {
                return db.Query<Book>($"{sql} ORDER BY id DESC").ToList();
            }
            return db.Query<Book>($"{sql} WHERE genre = @g ORDER BY id DESC", new { g = genre }).ToList();
        }

        public Book? GetBook(long id)
        {
            using var db = GetConnection();
            return db.QuerySingleOrDefault<Book>(
                "SELECT id as Id, title as Title, author as Author, pages as Pages, genre as Genre, is_lent as IsLent, lent_to as LentTo FROM books WHERE id = @id",
                new { id });
        }

        public long AddBook(Book b)
        {
            using var db = GetConnection();
            var sql = "INSERT INTO books (title, author, pages, genre, is_lent, lent_to) VALUES (@Title, @Author, @Pages, @Genre, @IsLent, @LentTo); SELECT LAST_INSERT_ID();";
            return db.ExecuteScalar<long>(sql, b);
        }

        public bool UpdateBook(long id, Book b)
        {
            using var db = GetConnection();
            b.Id = id;
            return db.Execute("UPDATE books SET title=@Title, author=@Author, pages=@Pages, genre=@Genre WHERE id=@Id", b) > 0;
        }

        public bool DeleteBook(long id)
        {
            using var db = GetConnection();
            return db.Execute("DELETE FROM books WHERE id=@id", new { id }) > 0;
        }

        public Book? ToggleLend(long id, string? lentTo)
        {
            using var db = GetConnection();
            var book = GetBook(id);
            if (book == null) return null;

            bool nowLent = !book.IsLent;
            db.Execute("UPDATE books SET is_lent=@Lent, lent_to=@LentTo WHERE id=@id",
                new { Lent = nowLent, LentTo = nowLent ? lentTo : null, id });

            return GetBook(id);
        }
    }
}