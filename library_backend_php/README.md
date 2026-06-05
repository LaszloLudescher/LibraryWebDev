# PHP Backend — Required API additions

The original PHP lab used server-rendered pages.
The new AJAX frontend calls a REST JSON API, so you need to add these files
to your existing XAMPP library folder (`htdocs/library/`).

## Files to add / replace

| File | Purpose |
|------|---------|
| `api/books.php` | GET list, GET single, POST, PUT, DELETE |
| `api/books_lend.php` | PATCH toggle lend status |
| `api/auth.php` | POST login, POST logout, GET status, POST register |
| `cors.php` | CORS + session headers (included by every api file) |

## Database change needed

Add `lent_to` column if it doesn't already exist:

```sql
ALTER TABLE books ADD COLUMN lent_to VARCHAR(100) DEFAULT NULL;
```

## CORS note

Because the frontend is served from a different origin than the PHP backend,
every API response needs:

```
Access-Control-Allow-Origin: http://localhost:5500
Access-Control-Allow-Credentials: true
Access-Control-Allow-Methods: GET,POST,PUT,DELETE,PATCH,OPTIONS
Access-Control-Allow-Headers: Content-Type
```

The `cors.php` helper handles this automatically.
