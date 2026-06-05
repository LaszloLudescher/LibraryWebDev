using Microsoft.AspNetCore.Mvc;
using LibraryBackend.Data;
using LibraryBackend.Models;

namespace LibraryBackend.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly LibraryDb _db;
        public AuthController(LibraryDb db) { _db = db; }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { error = "Username and password are required." });

            if (!_db.ValidateUser(req.Username, req.Password))
                return Unauthorized(new { error = "Invalid username or password." });

            HttpContext.Session.SetString("Username", req.Username);
            return Ok(new { username = req.Username, message = "Logged in." });
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return Ok(new { message = "Logged out." });
        }

        [HttpPost("register")]
        public IActionResult Register([FromBody] RegisterRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { error = "Username and password are required." });

            if (req.Username.Length < 3)
                return BadRequest(new { error = "Username must be at least 3 characters." });

            if (req.Password.Length < 6)
                return BadRequest(new { error = "Password must be at least 6 characters." });

            if (_db.UserExists(req.Username))
                return Conflict(new { error = "Username already taken." });

            _db.CreateUser(req.Username, req.Password);
            HttpContext.Session.SetString("Username", req.Username);
            return Ok(new { username = req.Username, message = "Registered." });
        }

        [HttpGet("status")]
        public IActionResult Status()
        {
            var username = HttpContext.Session.GetString("Username");
            if (username != null)
                return Ok(new { authenticated = true, username });
            return Ok(new { authenticated = false });
        }
    }
}
