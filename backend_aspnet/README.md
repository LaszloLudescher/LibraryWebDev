# Personal Library — ASP.NET Backend

Pure JSON REST API backend. No views, no Razor pages.
The same AJAX frontend works with this backend and with PHP —
swap one line in `config.js`.

---

## Requirements
- .NET 8 SDK — https://dotnet.microsoft.com/download/dotnet/8.0
- `sqlite3.dll` (Windows only) — https://www.sqlite.org/download.html
  → Under *Precompiled Binaries for Windows*, download the 64-bit DLL zip
  → Copy `sqlite3.dll` into this folder (next to `backend_aspnet.csproj`)

### Linux / Mac
`libsqlite3` is pre-installed. No extra step needed.

### Windows
Open `Data/SQLiteHelper.cs` and change:
```csharp
private const string Lib = "libsqlite3.so.0";
```
to:
```csharp
private const string Lib = "sqlite3";
```

---

## Run
```bash
cd backend_aspnet
dotnet run
# Listening on http://localhost:5050
```

The SQLite database (`library.db`) is created automatically on first run.

---

## Default credentials
| Username | Password  |
|----------|-----------|
| admin    | admin123  |

---

## API Endpoints

### Auth
| Method | URL | Body | Description |
|--------|-----|------|-------------|
| POST | `/api/auth/login` | `{username, password}` | Login, creates session |
| POST | `/api/auth/logout` | — | Destroy session |
| POST | `/api/auth/register` | `{username, password}` | Register new user |
| GET  | `/api/auth/status` | — | `{authenticated, username}` |

### Books (all require session)
| Method | URL | Body | Description |
|--------|-----|------|-------------|
| GET | `/api/books` | — | All books |
| GET | `/api/books?genre=Fiction` | — | Filtered by genre |
| GET | `/api/books/{id}` | — | Single book |
| POST | `/api/books` | `{title,author,pages,genre}` | Add book |
| PUT | `/api/books/{id}` | `{title,author,pages,genre}` | Update book |
| DELETE | `/api/books/{id}` | — | Delete book |
| PATCH | `/api/books/{id}/lend` | `{lent_to: "name"}` | Toggle lend status |

All responses are JSON. Unauthenticated requests return `401`.

---

## Switch the frontend to this backend

In `frontend/config.js`, change:
```js
const BASE_URL = 'http://localhost/library';   // PHP
```
to:
```js
const BASE_URL = 'http://localhost:5050';       // ASP.NET
```
That's the only change needed.
