using Microsoft.AspNetCore.Mvc;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers
{
    [ApiController]
    [Route("api/books")]
    public class BooksController : ControllerBase
    {
        private readonly LibraryDb _db;
        public BooksController(LibraryDb db) { _db = db; }

        private bool IsAuthenticated => HttpContext.Session.GetString("Username") != null;

        [HttpGet]
        public IActionResult GetBooks([FromQuery] string? genre)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });
            return Ok(_db.GetBooks(genre));
        }

        [HttpGet("{id}")]
        public IActionResult GetBook(long id)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });
            var book = _db.GetBook(id);
            if (book == null) return NotFound(new { error = "Book not found." });
            return Ok(book);
        }

        [HttpPost]
        public IActionResult AddBook([FromBody] Book model)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var id = _db.AddBook(model);
            model.Id = id;
            return CreatedAtAction(nameof(GetBook), new { id }, model);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateBook(long id, [FromBody] Book model)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });
            if (!ModelState.IsValid) return BadRequest(ModelState);

            if (!_db.UpdateBook(id, model))
                return NotFound(new { error = "Book not found." });

            model.Id = id;
            return Ok(model);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteBook(long id)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });

            if (!_db.DeleteBook(id))
                return NotFound(new { error = "Book not found." });

            return Ok(new { message = "Deleted." });
        }

        [HttpPatch("{id}/lend")]
        public IActionResult ToggleLend(long id, [FromBody] LendRequest req)
        {
            if (!IsAuthenticated) return Unauthorized(new { error = "Not authenticated." });

            var updated = _db.ToggleLend(id, req.LentTo);
            if (updated == null) return NotFound(new { error = "Book not found." });

            return Ok(updated);
        }
    }
}
