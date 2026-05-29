using LibraryBackend.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = null); // keep explicit JsonPropertyName attributes

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.None;  // needed for cross-origin AJAX
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    o.Cookie.Name = ".LibraryBackend.Session";
});

// SQLite database — file created automatically on first run
var dbPath = Path.Combine(builder.Environment.ContentRootPath, "library.db");
builder.Services.AddSingleton(_ => new LibraryDb(dbPath));

// CORS — allow the static frontend to send credentials
builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(
        "http://localhost",           // Apache/Nginx serving the frontend
        "http://localhost:8080",
        "http://localhost:5500",      // VS Code Live Server
        "http://127.0.0.1:5500",
        "null"                        // file:// origins (open HTML directly)
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));            // required so the session cookie travels with AJAX

var app = builder.Build();

app.UseCors("Frontend");
app.UseSession();
app.MapControllers();

app.Run("http://localhost:5050");
