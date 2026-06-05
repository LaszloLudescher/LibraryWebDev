using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace LibraryBackend.Models
{
    public class Book
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("title")]
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 1)]
        public string Title { get; set; } = "";

        [JsonPropertyName("author")]
        [Required(ErrorMessage = "Author is required")]
        [StringLength(150, MinimumLength = 2)]
        public string Author { get; set; } = "";

        [JsonPropertyName("pages")]
        [Required]
        [Range(1, 10000, ErrorMessage = "Pages must be between 1 and 10000")]
        public int Pages { get; set; }

        [JsonPropertyName("genre")]
        [Required(ErrorMessage = "Genre is required")]
        public string Genre { get; set; } = "";

        [JsonPropertyName("is_lent")]
        public bool IsLent { get; set; }

        [JsonPropertyName("lent_to")]
        public string? LentTo { get; set; }
    }

    public class LendRequest
    {
        [JsonPropertyName("lent_to")]
        public string? LentTo { get; set; }
    }

    public class LoginRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }

    public class RegisterRequest
    {
        [JsonPropertyName("username")]
        public string? Username { get; set; }

        [JsonPropertyName("password")]
        public string? Password { get; set; }
    }
}
