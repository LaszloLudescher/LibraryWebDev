using LibraryBackend.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o =>
        o.JsonSerializerOptions.PropertyNamingPolicy = null); 

builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(o =>
{
    o.IdleTimeout = TimeSpan.FromMinutes(60);
    o.Cookie.HttpOnly = true;
    o.Cookie.IsEssential = true;
    o.Cookie.SameSite = SameSiteMode.Lax;  
    o.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    o.Cookie.Name = ".LibraryBackend.Session";
});

string mySqlConnStr = "Server=localhost;Database=library_db;User=root;Password=;";
builder.Services.AddSingleton(_ => new LibraryDb(mySqlConnStr));

// CORS — allow the static frontend to send credentials
builder.Services.AddCors(o => o.AddPolicy("Frontend", p =>
    p.WithOrigins(
        "http://localhost",           
        "http://localhost:8080",
        "http://localhost:5500",      
        "http://127.0.0.1:5500",
        "null"                        
    )
    .AllowAnyHeader()
    .AllowAnyMethod()
    .AllowCredentials()));           

var app = builder.Build();

app.UseCors("Frontend");
app.UseSession();
app.MapControllers();

app.Run("http://localhost:5050");
