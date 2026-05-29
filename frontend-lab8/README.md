# Personal Library — AJAX Frontend

Static HTML/JS frontend. Works with **either** the PHP backend or the ASP.NET backend.

---

## Setup

### 1. Point to your backend

Open `config.js` and set `BASE_URL`:

```js
// PHP backend (XAMPP at htdocs/library):
const BASE_URL = 'http://localhost/library';

// ASP.NET backend (running on port 5050):
const BASE_URL = 'http://localhost:5050';
```

That is the **only change** needed to switch backends.

### 2. Serve the frontend

Because the AJAX calls use credentials (session cookies), browsers block
requests from `file://` origins in some configurations.
Use any local static server, for example:

**VS Code Live Server** (recommended):
- Install the "Live Server" extension
- Right-click `index.html` → *Open with Live Server*
- Runs on `http://localhost:5500`

**Python:**
```bash
cd frontend
python -m http.server 5500
# Open http://localhost:5500
```

**XAMPP:** Copy the entire `frontend/` folder into `htdocs/frontend/`
and open `http://localhost/frontend/`

---

## PHP backend — extra setup needed

See `php_backend_additions/README.md`. You need to:
1. Run `schema_update.sql` in phpMyAdmin to add the `users` table and `lent_to` column
2. Copy the files from `php_backend_additions/` into your `htdocs/library/` folder
3. Adjust `cors.php` if your frontend runs on a different port

---

## Pages

| File | Description |
|------|-------------|
| `login.html` | Login form — redirected here if not authenticated |
| `register.html` | Register a new user account |
| `index.html` | Browse books, AJAX genre filter with last-filter display |
| `add.html` | Add a new book |
| `edit.html?id=N` | Edit a book (pre-populated from API) |
| `lend.html?id=N&current_status=0/1` | Lend or return a book |

---

## Default credentials
| Username | Password |
|----------|----------|
| admin | admin123 |
