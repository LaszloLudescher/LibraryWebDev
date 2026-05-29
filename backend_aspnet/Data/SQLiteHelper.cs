using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace LibraryBackend.Data
{
    /// <summary>
    /// Minimal P/Invoke wrapper around libsqlite3 for use without NuGet packages.
    /// </summary>
    public class SQLiteHelper : IDisposable
    {
        // ── Native API ──────────────────────────────────────────────────────────
        private const string Lib = "libsqlite3.so.0";

        [DllImport(Lib)] static extern int sqlite3_open(string filename, out IntPtr db);
        [DllImport(Lib)] static extern int sqlite3_close(IntPtr db);
        [DllImport(Lib)] static extern int sqlite3_prepare_v2(IntPtr db, string sql, int nBytes, out IntPtr stmt, out IntPtr tail);
        [DllImport(Lib)] static extern int sqlite3_step(IntPtr stmt);
        [DllImport(Lib)] static extern int sqlite3_finalize(IntPtr stmt);
        [DllImport(Lib)] static extern int sqlite3_column_count(IntPtr stmt);
        [DllImport(Lib)] static extern IntPtr sqlite3_column_name(IntPtr stmt, int col);
        [DllImport(Lib)] static extern int sqlite3_column_type(IntPtr stmt, int col);
        [DllImport(Lib)] static extern IntPtr sqlite3_column_text(IntPtr stmt, int col);
        [DllImport(Lib)] static extern int sqlite3_column_int(IntPtr stmt, int col);
        [DllImport(Lib)] static extern long sqlite3_column_int64(IntPtr stmt, int col);
        [DllImport(Lib)] static extern int sqlite3_bind_text(IntPtr stmt, int idx, string val, int n, IntPtr destructor);
        [DllImport(Lib)] static extern int sqlite3_bind_int(IntPtr stmt, int idx, int val);
        [DllImport(Lib)] static extern int sqlite3_bind_int64(IntPtr stmt, int idx, long val);
        [DllImport(Lib)] static extern int sqlite3_bind_null(IntPtr stmt, int idx);
        [DllImport(Lib)] static extern long sqlite3_last_insert_rowid(IntPtr db);
        [DllImport(Lib)] static extern IntPtr sqlite3_errmsg(IntPtr db);
        [DllImport(Lib)] static extern int sqlite3_exec(IntPtr db, string sql, IntPtr cb, IntPtr data, out IntPtr errmsg);

        private static readonly IntPtr SQLITE_TRANSIENT = new IntPtr(-1);
        private const int SQLITE_ROW = 100;
        private const int SQLITE_DONE = 101;

        // ── Instance ────────────────────────────────────────────────────────────
        private IntPtr _db;

        public SQLiteHelper(string dbPath)
        {
            int rc = sqlite3_open(dbPath, out _db);
            if (rc != 0) throw new Exception($"Cannot open SQLite database: {dbPath}");
            // Enable WAL for better concurrency
            Execute("PRAGMA journal_mode=WAL;");
            Execute("PRAGMA foreign_keys=ON;");
        }

        public void Execute(string sql, params object?[] args)
        {
            using var cmd = PrepareStatement(sql, args);
            sqlite3_step(cmd.Stmt);
        }

        public long ExecuteInsert(string sql, params object?[] args)
        {
            using var cmd = PrepareStatement(sql, args);
            sqlite3_step(cmd.Stmt);
            return sqlite3_last_insert_rowid(_db);
        }

        public List<Dictionary<string, object?>> Query(string sql, params object?[] args)
        {
            using var cmd = PrepareStatement(sql, args);
            var results = new List<Dictionary<string, object?>>();
            int cols = sqlite3_column_count(cmd.Stmt);

            while (sqlite3_step(cmd.Stmt) == SQLITE_ROW)
            {
                var row = new Dictionary<string, object?>();
                for (int i = 0; i < cols; i++)
                {
                    string name = Marshal.PtrToStringUTF8(sqlite3_column_name(cmd.Stmt, i)) ?? $"col{i}";
                    int type = sqlite3_column_type(cmd.Stmt, i);
                    object? val = type switch
                    {
                        1 => sqlite3_column_int64(cmd.Stmt, i),   // INTEGER
                        2 => sqlite3_column_int(cmd.Stmt, i),      // FLOAT (simplified)
                        3 => Marshal.PtrToStringUTF8(sqlite3_column_text(cmd.Stmt, i)),  // TEXT
                        5 => null,                                  // NULL
                        _ => Marshal.PtrToStringUTF8(sqlite3_column_text(cmd.Stmt, i))
                    };
                    row[name] = val;
                }
                results.Add(row);
            }
            return results;
        }

        public Dictionary<string, object?>? QuerySingle(string sql, params object?[] args)
        {
            var rows = Query(sql, args);
            return rows.Count > 0 ? rows[0] : null;
        }

        public T? QueryScalar<T>(string sql, params object?[] args)
        {
            var row = QuerySingle(sql, args);
            if (row == null || row.Count == 0) return default;
            var val = row.Values.First();
            if (val == null) return default;
            return (T)Convert.ChangeType(val, typeof(T));
        }

        private StmtHandle PrepareStatement(string sql, object?[] args)
        {
            int rc = sqlite3_prepare_v2(_db, sql, -1, out IntPtr stmt, out _);
            if (rc != 0)
            {
                string err = Marshal.PtrToStringUTF8(sqlite3_errmsg(_db)) ?? "unknown error";
                throw new Exception($"SQLite prepare error: {err}\nSQL: {sql}");
            }

            for (int i = 0; i < args.Length; i++)
            {
                int idx = i + 1;
                switch (args[i])
                {
                    case null:
                        sqlite3_bind_null(stmt, idx); break;
                    case int iv:
                        sqlite3_bind_int(stmt, idx, iv); break;
                    case long lv:
                        sqlite3_bind_int64(stmt, idx, lv); break;
                    case bool bv:
                        sqlite3_bind_int(stmt, idx, bv ? 1 : 0); break;
                    default:
                        sqlite3_bind_text(stmt, idx, args[i]!.ToString() ?? "", -1, SQLITE_TRANSIENT); break;
                }
            }
            return new StmtHandle(stmt);
        }

        private class StmtHandle : IDisposable
        {
            public IntPtr Stmt { get; }
            public StmtHandle(IntPtr s) { Stmt = s; }
            public void Dispose() { sqlite3_finalize(Stmt); }
        }

        public void Dispose()
        {
            if (_db != IntPtr.Zero)
            {
                sqlite3_close(_db);
                _db = IntPtr.Zero;
            }
        }
    }
}
