# Library Backend — Node.js

Drop-in Node.js replacement for the PHP backend.  
The **same frontend** works with both backends — just change one line in `config.js`.

---

## API surface (identical to PHP)

| Method | URL | Auth required | Description |
|--------|-----|:---:|-------------|
| GET | `/api/auth?action=status` | No | Returns `{ authenticated, username }` |
| POST | `/api/auth?action=login` | No | Body: `{ username, password }` |
| POST | `/api/auth?action=logout` | No | Destroys session |
| POST | `/api/auth?action=register` | No | Body: `{ username, password }` |
| GET | `/api/books` | Yes | List all books (optional `?genre=`) |
| GET | `/api/books/:id` | Yes | Single book |
| POST | `/api/books` | Yes | Create book |
| PUT | `/api/books/:id` | Yes | Update book |
| DELETE | `/api/books/:id` | Yes | Delete book |
| POST | `/api/books_lend?id=:id` | Yes | Toggle lend status (body: `{ lent_to? }`) |

---

## Setup

### 1. Database
Run the provided SQL script in MySQL/MariaDB (same DB used by the PHP backend):

```bash
mysql -u root -p < database_setup.sql
```

This is **safe to run even if the DB already exists** — all statements use `IF NOT EXISTS` guards.

### 2. Install dependencies

```bash
npm install
```

### 3. Configure (optional)
The server reads these environment variables (all have defaults):

| Variable | Default | Description |
|----------|---------|-------------|
| `PORT` | `3000` | HTTP port |
| `DB_HOST` | `localhost` | MySQL host |
| `DB_USER` | `root` | MySQL user |
| `DB_PASS` | *(empty)* | MySQL password |
| `DB_NAME` | `library_db` | Database name |
| `SESSION_SECRET` | `library_secret_key_change_in_prod` | Session signing key |

You can set them inline or in a `.env` file (add `dotenv` if you prefer):

```bash
DB_PASS=mypassword node index.js
```

### 4. Start

```bash
node index.js
# → Library Node.js backend running on http://localhost:3000
```

---

## Switching the frontend

Open `frontend-lab8/config.js` and change **one line**:

```js
// PHP backend (XAMPP):
// const BASE_URL = 'http://localhost/library_backend_php';

// Node.js backend:
const BASE_URL = 'http://localhost:3000';
```

That's it — no other frontend change is needed.

---

## Test user
The `database_setup.sql` seeds a default account:

| Username | Password |
|----------|----------|
| `testuser` | `test123` |

Remove or replace this row before going to production.

---

## Session notes
Node uses `express-session` with an **in-memory store** by default (fine for development / lab demo). For production, swap in `connect-redis` or `connect-mysql-session`.
