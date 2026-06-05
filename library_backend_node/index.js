const express = require('express');
const session = require('express-session');
const cors    = require('cors');
const bcrypt  = require('bcrypt');
const mysql   = require('mysql2/promise');

const app = express();
const PORT = process.env.PORT || 3000;

const pool = mysql.createPool({
  host:     process.env.DB_HOST     || 'localhost',
  user:     process.env.DB_USER     || 'root',
  password: process.env.DB_PASS     || '',
  database: process.env.DB_NAME     || 'library_db',
  charset:  'utf8mb4',
  waitForConnections: true,
  connectionLimit: 10,
});

const ALLOWED_ORIGINS = [
  'http://localhost',
  'http://localhost:8080',
  'http://localhost:5500',
  'http://127.0.0.1:5500',
];

const LOGIN_TIMEOUT = 8 * 60 * 1000;

app.use(cors({
  origin: (origin, cb) => {
    if (!origin || ALLOWED_ORIGINS.includes(origin)) return cb(null, true);
    cb(null, true); 
  },
  credentials: true,
  methods: ['GET', 'POST', 'PUT', 'DELETE', 'PATCH', 'OPTIONS'],
  allowedHeaders: ['Content-Type'],
}));

app.options(/.*/, cors());  

app.use(express.json());

app.use(session({
  secret: process.env.SESSION_SECRET || 'library_secret_key_change_in_prod',
  resave: false,
  saveUninitialized: false,
  cookie: {
    httpOnly: true,
    sameSite: 'lax',
    maxAge: LOGIN_TIMEOUT
  },
}));

function requireAuth(req, res, next) {
  if (req.session && req.session.username) return next();
  return res.status(401).json({ error: 'Not authenticated.' });
}

function mapBook(row) {
  return {
    id:      Number(row.id),
    title:   row.title,
    author:  row.author,
    pages:   Number(row.pages),
    genre:   row.genre,
    is_lent: Boolean(row.is_lent),
    lent_to: row.lent_to || null,
  };
}


app.get(['/api/auth', '/api/auth/:action'], (req, res) => {
  const action = req.params.action || req.query.action;
  
  if (action === 'status') {
    if (req.session.username) {
      return res.json({ authenticated: true, username: req.session.username });
    }
    return res.json({ authenticated: false });
  }
  res.status(404).json({ error: 'Not found.' });
});

app.post(['/api/auth', '/api/auth/:action'], async (req, res) => {
  const action = req.params.action || req.query.action;

  if (action === 'login') {
    const { username = '', password = '' } = req.body;
    if (!username.trim() || !password) {
      return res.status(400).json({ error: 'Username and password are required.' });
    }
    try {
      const [rows] = await pool.query(
        'SELECT password_hash FROM users WHERE username = ?',
        [username.trim()]
      );
      if (rows.length && await bcrypt.compare(password, rows[0].password_hash)) {
        req.session.username = username.trim();
        return res.json({ username: username.trim(), message: 'Logged in.' });
      }
      return res.status(401).json({ error: 'Invalid username or password.' });
    } catch (err) {
      console.error(err);
      return res.status(500).json({ error: 'Server error.' });
    }
  }

  if (action === 'logout') {
    req.session.destroy(() => {
      res.json({ message: 'Logged out.' });
    });
    return;
  }

  if (action === 'register') {
    const { username = '', password = '' } = req.body;
    if (username.trim().length < 3) {
      return res.status(400).json({ error: 'Username must be at least 3 characters.' });
    }
    if (password.length < 6) {
      return res.status(400).json({ error: 'Password must be at least 6 characters.' });
    }
    try {
      const [existing] = await pool.query(
        'SELECT id FROM users WHERE username = ?',
        [username.trim()]
      );
      if (existing.length) {
        return res.status(409).json({ error: 'Username already taken.' });
      }
      const hash = await bcrypt.hash(password, 10);
      await pool.query(
        'INSERT INTO users (username, password_hash) VALUES (?, ?)',
        [username.trim(), hash]
      );
      req.session.username = username.trim();
      return res.json({ username: username.trim(), message: 'Registered.' });
    } catch (err) {
      console.error(err);
      return res.status(500).json({ error: 'Server error.' });
    }
  }

  res.status(404).json({ error: 'Not found.' });
});

app.get(['/api/books', '/api/books/:id'], requireAuth, async (req, res) => {
  try {
    if (req.params.id) {
      const [rows] = await pool.query('SELECT * FROM books WHERE id = ?', [req.params.id]);
      if (!rows.length) return res.status(404).json({ error: 'Book not found.' });
      return res.json(mapBook(rows[0]));
    }

    const genre = req.query.genre || '';
    let rows;
    if (genre) {
      [rows] = await pool.query(
        'SELECT * FROM books WHERE genre = ? ORDER BY id DESC', [genre]
      );
    } else {
      [rows] = await pool.query('SELECT * FROM books ORDER BY id DESC');
    }
    return res.json(rows.map(mapBook));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
  }
});

app.post('/api/books', requireAuth, async (req, res) => {
  const { title = '', author = '', pages, genre = '' } = req.body;
  if (!title.trim() || !author.trim() || !Number(pages) || Number(pages) < 1 || !genre.trim()) {
    return res.status(400).json({ error: 'All fields are required and pages must be > 0.' });
  }
  try {
    const [result] = await pool.query(
      'INSERT INTO books (title, author, pages, genre, is_lent) VALUES (?,?,?,?,0)',
      [title.trim(), author.trim(), Number(pages), genre.trim()]
    );
    const [rows] = await pool.query('SELECT * FROM books WHERE id = ?', [result.insertId]);
    return res.status(201).json(mapBook(rows[0]));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
  }
});

app.put('/api/books/:id', requireAuth, async (req, res) => {
  const id = Number(req.params.id);
  const { title = '', author = '', pages, genre = '' } = req.body;
  if (!title.trim() || !author.trim() || !Number(pages) || Number(pages) < 1 || !genre.trim()) {
    return res.status(400).json({ error: 'All fields are required and pages must be > 0.' });
  }
  try {
    const [result] = await pool.query(
      'UPDATE books SET title=?,author=?,pages=?,genre=? WHERE id=?',
      [title.trim(), author.trim(), Number(pages), genre.trim(), id]
    );
    if (result.affectedRows === 0) return res.status(404).json({ error: 'Book not found.' });
    const [rows] = await pool.query('SELECT * FROM books WHERE id = ?', [id]);
    return res.json(mapBook(rows[0]));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
  }
});

app.delete('/api/books/:id', requireAuth, async (req, res) => {
  try {
    await pool.query('DELETE FROM books WHERE id = ?', [req.params.id]);
    res.json({ message: 'Deleted.' });
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
  }
});

app.patch(['/api/books_lend', '/api/books/:id/lend'], requireAuth, async (req, res) => {
  const id = Number(req.params.id || req.query.id);
  
  if (!id) return res.status(400).json({ error: 'ID required.' });

  try {
    const [rows] = await pool.query('SELECT * FROM books WHERE id = ?', [id]);
    if (!rows.length) return res.status(404).json({ error: 'Book not found.' });

    const book    = rows[0];
    const nowLent = !Boolean(book.is_lent);
    const lentTo  = nowLent ? (req.body.lent_to || null) : null;

    await pool.query(
      'UPDATE books SET is_lent = ?, lent_to = ? WHERE id = ?',
      [nowLent ? 1 : 0, lentTo, id]
    );

    const [updated] = await pool.query('SELECT * FROM books WHERE id = ?', [id]);
    return res.json(mapBook(updated[0]));
  } catch (err) {
    console.error(err);
    res.status(500).json({ error: 'Server error.' });
  }
});

app.listen(PORT, () => {
  console.log(`Library Node.js backend running on http://localhost:${PORT}`);
  console.log('Database:', process.env.DB_NAME || 'library_db');
});