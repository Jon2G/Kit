using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Kit.Daemon.Sync;
using Kit.SetUpConnectionString;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Exceptions;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using Kit.Sql.Readers;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
using SQLitePCL;
using NotNullConstraintViolationException = Kit.Sql.Exceptions.NotNullConstraintViolationException;
using static Kit.Sql.Base.BaseTableQuery;

namespace Kit.Sql.Sqlite
{
    /// <summary>
    /// An open connection to a SQLite database.
    /// </summary>
    [Attributes.Preserve(AllMembers = true)]
    public class SQLiteConnection : SqlBase, IDisposable
    {
        #region BaseSqlite

        private Type AssemblyType;
        private string ScriptResourceName;

        public readonly int DBVersion;

        /// <summary>
        /// Gets the database path used by this connection.
        /// </summary>
        public string DatabasePath { get; protected set; }

        protected new SQLiteConnectionString ConnectionString;

        public SQLiteConnection SetDbScriptResource<T>(string ScriptResourceName)
        {
            AssemblyType = typeof(T);
            this.ScriptResourceName = ScriptResourceName;
            return this;
        }

        public SQLiteConnection(FileInfo file, int DBVersion)
        : this(new SQLiteConnectionString(file.FullName), DBVersion)
        {
            //if (Tools.Instance is null)
            //{
            //    throw new Exception("Please call Tools.Init(); before using it");
            //}
            file.Refresh();
            DatabasePath = file.FullName;
        }

        public override string MappingSuffix => "_sqlite";

        protected override Base.TableMapping _GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
            return new Kit.Sql.Sqlite.TableMapping(type, createFlags);
        }

        public SQLiteConnection CheckTables(IEnumerable<Type> Tables)
        {
            return (SQLiteConnection)CheckTables(DBVersion, Tables);
        }

        public SQLiteConnection CheckTables(params Type[] Tables)
        {
            return (SQLiteConnection)CheckTables(DBVersion, Tables);
        }

        public override SqlBase CheckTables(int DBVersion, params Type[] Tables)
        {
            CreateTable<DatabaseVersion>();
            DatabaseVersion Version = Table<DatabaseVersion>().FirstOrDefault();
            if ((Version?.Version ?? 0) != DBVersion)
            {
                this.Close();
                GC.Collect();
                GC.WaitForPendingFinalizers();
                File.Delete(DatabasePath);
                RenewConnection();
                CreateSchema();
                return CheckTables(DBVersion, Tables);
            }
            CreateTable<ChangesHistory>();
            CreateTable<SyncHistory>();
            CreateTable<Configuracion>();
            CreateTable<DeviceInformation>();
            foreach (Type table in Tables)
            {
                CreateTable(table);
            }
            return this;
        }

        //private void RevisarTablaDbVersion(SQLiteConnection connection)
        //{
        //    if (TableExists("DB_VERSION"))
        //    {
        //        return;
        //    }
        //    connection.Execute(@"CREATE TABLE DB_VERSION ( VERSION VARCHAR NOT NULL )");
        //    connection.Execute($"INSERT INTO DB_VERSION(VERSION) VALUES('{DBVersion}')");
        //}

        private void CreateSchema()
        {
            if (AssemblyType != null && !string.IsNullOrEmpty(ScriptResourceName))
            {
                CreateDbFromScript();
            }
            CreateTable<DatabaseVersion>();
            Insert(new DatabaseVersion()
            {
                Version = DBVersion
            });
            CreateTable<DeviceInformation>();
            Insert(new DeviceInformation()
            {
                DeviceId = Daemon.Devices.Device.Current.DeviceId,
                IsFirstLaunchTime = true
            });
        }

        private void CreateDbFromScript()
        {
            string sql = string.Empty;
            using (ReflectionCaller reflection = new ReflectionCaller())
            {
                sql = ReflectionCaller.ToText(reflection
                    .GetAssembly(AssemblyType)
                    .GetResource(ScriptResourceName));
            }
            if (!string.IsNullOrEmpty(sql))
            {
                Batch(sql);
            }
            AssemblyType = null;
            ScriptResourceName = null;
        }

        public bool EliminarDB()
        {
            try
            {
                FileInfo file = new FileInfo(DatabasePath);
                if (file.Exists)
                {
                    File.Delete(DatabasePath);
                }
                return true;
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "");
                return false;
            }
        }

        public SQLiteConnection Conecction()
        {
            SQLiteConnection con;
            try
            {
                con = new SQLiteConnection(DatabasePath, this.DBVersion);
                return con;
            }
            catch (Exception ex)
            {
                con = null;
                Log.Logger.Error(ex, "");
            }
            return con;
        }

        public SQLiteAsyncConnection ConecctionAsync()
        {
            SQLiteAsyncConnection con;
            try
            {
                con = new SQLiteAsyncConnection(DatabasePath);
                return con;
            }
            catch (Exception ex)
            {
                con = null;
                Log.Logger.Error(ex, "");
            }
            return con;
        }

        public int Querry(string sql, params object[] args)
        {
            int afectadas = -1;
            using (SQLiteConnection con = Conecction())
            {
                afectadas = con.Execute(sql, args);
                con.Close();
            }

            return afectadas;
        }

        public override object Single(string sql)
        {
            object result = default;
            try
            {
                using (IReader reader = Read(sql))
                {
                    if (reader.Read())
                    {
                        result = (reader[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }

        public override T Single<T>(string sql)
        {
            T result = default;
            try
            {
                result = Sqlh.Parse<T>(Single(sql));
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }

        public T Single<T>(SQLiteConnection con, string sql) where T : IConvertible
        {
            T result = default;
            try
            {
                using (IReader reader = Read(sql, con))
                {
                    if (reader.Read())
                    {
                        result = Sqlh.Parse<T>(reader[0]);
                    }
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Al ejecutar un single en SQLHLite");
            }
            return result;
        }

        public override int EXEC(string sql)
        {
            return EXEC(sql);
        }

        public int InsertAndRecoverPK(string sql, params object[] parametros)
        {
            int Id = 1;
            using (var con = Conecction())
            {
                if (EXEC(con, sql, parametros) != SqlBase.Error)
                {
                    Id = LastScopeIdentity(con);
                }
                con.Close();
            }
            return Id;
        }

        public int EXEC(string sql, params object[] parametros)
        {
            Log.Logger.Debug(sql);
            int afectadas = -1;
            using (SQLiteConnection con = Conecction())
            {
                afectadas = con.Execute(sql, parametros);
                con.Close();
            }
            return afectadas;
        }

        public int EXEC(SQLiteConnection con, string sql, params object[] parametros)
        {
            Log.Logger.Debug(sql);
            return con.Execute(sql, parametros);
        }

        public T ExecuteRead<T>(string sql, params object[] parametros)
        {
            Log.Logger.Debug(sql);
            T result = default;
            using (SQLiteConnection con = Conecction())
            {
                result = con.ExecuteScalar<T>(sql, parametros);
                con.Close();
            }
            return result;
        }

        public List<object> Lista(string sql)
        {
            List<object> result = new List<object>();
            using (IReader reader = Read(sql))
            {
                while (reader.Read())
                {
                    result.Add(reader[0]);
                }
            }
            return result;
        }

        public override List<T> Lista<T>(string sql)
        {
            List<T> result = new List<T>();
            using (IReader reader = Read(sql))
            {
                while (reader.Read())
                {
                    result.Add(Sqlh.Parse<T>(reader[0]));
                }
            }
            return result;
        }

        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, params object[] parameters)
            where T : IConvertible
            where Q : IConvertible
        {
            List<Tuple<T, Q>> result = new List<Tuple<T, Q>>();

            using (IReader reader = Read(sql))
            {
                while (reader.Read())
                {
                    result.Add(new Tuple<T, Q>(Sqlh.Parse<T>(reader[0]), Sqlh.Parse<Q>(reader[1])));
                }
            }

            return result;
        }

        public DataTable DataTable(string Querry, string TableName = null)
        {
            DataTable result = new DataTable(TableName);
            using (SqliteReader reader = (SqliteReader)Read(Querry))
            {
                if (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        result.Columns.Add(reader.Fields[i]);
                    }
                }
                else
                {
                    return result;
                }
                do
                {
                    List<object> row = new List<object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row.Add(reader[i]);
                    }

                    result.Rows.Add(row.ToArray());
                } while (reader.Read());
            }
            return result;
        }

        public override IReader Read(string sql)
        {
            try
            {
                return new SqliteReader(Conecction(), sql);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al compilar y ejecutar un leector");
                return new FakeReader();
            }
        }

        public IReader Read(string sql, SQLiteConnection connection)
        {
            try
            {
                return new SqliteReader(connection, sql);
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al compilar y ejecutar un leector");
                return null;
            }
        }

        public override bool Exists(string sql, params Condition[] parametros)
        {
            return Exists(sql, parametros.Select(x => x.Value));
        }

        public bool Exists(string sql, params object[] parameters)
        {
            using (IReader reader = Read(sql))
            {
                return reader?.Read() ?? false;
            }
        }

        public override bool ViewExists(string viewName)
        {
            return Exists($"SELECT name FROM sqlite_master WHERE type='view' AND name='{viewName}'");
        }

        public override bool TableExists(string TableName)
        {
            return Exists($"SELECT name FROM sqlite_master WHERE type='table' AND name='{TableName}';");
        }

        public void Batch(string sql)
        {
            StringBuilder sqlBatch = new StringBuilder();
            try
            {
                Regex regex = new Regex(@"^.*\(.*\);$/gms");
                regex.Split(sql);

                foreach (string line in sql.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "--GO" || (sqlBatch.Length > 0 && sqlBatch.EndsWith(");")))
                    {
                        string batch = sqlBatch.ToString();
                        //if (IsBalanced(batch))
                        //{
                        if (!string.IsNullOrEmpty(batch))
                            EXEC(batch);
                        sqlBatch.Clear();
                        //}
                    }
                    if (!line.StartsWith("--"))
                    {
                        sqlBatch.Append(line.Trim());
                    }
                }
                if (sqlBatch.Length > 0)
                {
                    EXEC(sqlBatch.ToString());
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
            }
        }

        private bool IsBalanced(string batch)
        {
            int open = batch.Count(x => x == '(');
            int closed = batch.Count(x => x == ')');

            return open == closed;
        }

        public int LastScopeIdentity(SQLiteConnection con)
        {
            return Single<int>(con, "SELECT last_insert_rowid();");
        }

        #endregion BaseSqlite

        private bool _open;
        private TimeSpan _busyTimeout;

        private int _transactionDepth = 0;
        private Random _rand = new Random();

        public sqlite3 Handle { get; private set; }
        private static readonly sqlite3 NullHandle = default(sqlite3);
        private static readonly sqlite3_backup NullBackupHandle = default(sqlite3_backup);

        /// <summary>
        /// Gets the SQLite library version number. 3007014 would be v3.7.14
        /// </summary>
        public int LibVersionNumber { get; private set; }

        /// <summary>
        /// Whether to store DateTime properties as ticks (true) or strings (false).
        /// </summary>
        public bool StoreDateTimeAsTicks { get; private set; }

        /// <summary>
        /// Whether to store TimeSpan properties as ticks (true) or strings (false).
        /// </summary>
        public bool StoreTimeSpanAsTicks { get; private set; }

        /// <summary>
        /// The format to use when storing DateTime properties as strings. Ignored if StoreDateTimeAsTicks is true.
        /// </summary>
        /// <value>The date time string format.</value>
        public string DateTimeStringFormat { get; private set; }

        /// <summary>
        /// The DateTimeStyles value to use when parsing a DateTime property string.
        /// </summary>
        /// <value>The date time style.</value>
        internal System.Globalization.DateTimeStyles DateTimeStyle { get; private set; }

        /// <summary>
        /// Whether <see cref="SQLiteConnection"/> has been disposed and the database is closed.
        /// </summary>
        public override bool IsClosed
        {
            get => !_open;
        }

#if USE_SQLITEPCL_RAW && !NO_SQLITEPCL_RAW_BATTERIES

        static SQLiteConnection()
        {
            SQLitePCL.Batteries_V2.Init();
        }

#endif

        /// <summary>
        /// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
        /// </summary>
        /// <param name="databasePath">
        /// Specifies the path to the database file.
        /// </param>
        /// <param name="storeDateTimeAsTicks">
        /// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
        /// absolutely do want to store them as Ticks in all new projects. The value of false is
        /// only here for backwards compatibility. There is a *significant* speed advantage, with no
        /// down sides, when setting storeDateTimeAsTicks = true.
        /// If you use DateTimeOffset properties, it will be always stored as ticks regardingless
        /// the storeDateTimeAsTicks parameter.
        /// </param>
        public SQLiteConnection(string databasePath, int DbVersion, bool storeDateTimeAsTicks = true)
            : this(new SQLiteConnectionString(databasePath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create, storeDateTimeAsTicks), DbVersion)
        {
        }

        /// <summary>
        /// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
        /// </summary>
        /// <param name="databasePath">
        /// Specifies the path to the database file.
        /// </param>
        /// <param name="openFlags">
        /// Flags controlling how the connection should be opened.
        /// </param>
        /// <param name="storeDateTimeAsTicks">
        /// Specifies whether to store DateTime properties as ticks (true) or strings (false). You
        /// absolutely do want to store them as Ticks in all new projects. The value of false is
        /// only here for backwards compatibility. There is a *significant* speed advantage, with no
        /// down sides, when setting storeDateTimeAsTicks = true.
        /// If you use DateTimeOffset properties, it will be always stored as ticks regardingless
        /// the storeDateTimeAsTicks parameter.
        /// </param>
        public SQLiteConnection(string databasePath, SQLiteOpenFlags openFlags, int DBVersion, bool storeDateTimeAsTicks = true)
            : this(new SQLiteConnectionString(databasePath, openFlags, storeDateTimeAsTicks), DBVersion)
        {
        }

        /// <summary>
        /// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
        /// </summary>
        /// <param name="connectionString">
        /// Details on how to find and open the database.
        /// </param>
        public SQLiteConnection(SQLiteConnectionString connectionString)
            : this(connectionString, 0)
        {
        }

        public SQLiteConnection(SQLiteConnectionString connectionString, int DBVersion)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));
            if (connectionString.DatabasePath == null)
                throw new InvalidOperationException("DatabasePath must be specified");

            #region SqliteHelperInit

            DatabasePath = connectionString.DatabasePath;
            this.DBVersion = DBVersion;

            #endregion SqliteHelperInit

            DatabasePath = connectionString.DatabasePath;

            LibVersionNumber = SQLite3.LibVersionNumber();
            this.ConnectionString = connectionString;
            RenewConnection();

            StoreDateTimeAsTicks = connectionString.StoreDateTimeAsTicks;
            StoreTimeSpanAsTicks = connectionString.StoreTimeSpanAsTicks;
            DateTimeStringFormat = connectionString.DateTimeStringFormat;
            DateTimeStyle = connectionString.DateTimeStyle;

            BusyTimeout = TimeSpan.FromSeconds(1.0);

            connectionString.PreKeyAction?.Invoke(this);
            if (connectionString.Key is string stringKey)
            {
                SetKey(stringKey);
            }
            else if (connectionString.Key is byte[] bytesKey)
            {
                SetKey(bytesKey);
            }
            else if (connectionString.Key != null)
            {
                throw new InvalidOperationException("Encryption keys must be strings or byte arrays");
            }
            connectionString.PostKeyAction?.Invoke(this);
        }

        public SQLiteConnection(string ConnectionString) : base(ConnectionString)
        {
        }

        public override SqlBase RenewConnection()
        {
#if NETFX_CORE
			SQLite3.SetDirectory(/*temp directory type*/2, Windows.Storage.ApplicationData.Current.TemporaryFolder.Path);
#endif

            sqlite3 handle;

#if SILVERLIGHT || USE_CSHARP_SQLITE || USE_SQLITEPCL_RAW
            var r = SQLite3.Open(ConnectionString.DatabasePath, out handle, (int)ConnectionString.OpenFlags,
                ConnectionString.VfsName);
#else
			// open using the byte[]
			// in the case where the path may include Unicode
			// force open to using UTF-8 using sqlite3_open_v2
			var databasePathAsBytes = GetNullTerminatedUtf8 (connectionString.DatabasePath);
			var r = SQLite3.Open (databasePathAsBytes, out handle, (int)connectionString.OpenFlags, connectionString.VfsName);
#endif

            Handle = handle;
            if (r != SQLite3.Result.OK)
            {
                throw SQLiteException.New(r, String.Format("Could not open database file: {0} ({1})", DatabasePath, r));
            }
            _open = true;
            return this;
        }

        /// <summary>
        /// Enables the write ahead logging. WAL is significantly faster in most scenarios
        /// by providing better concurrency and better disk IO performance than the normal
        /// journal mode. You only need to call this function once in the lifetime of the database.
        /// </summary>
        public void EnableWriteAheadLogging()
        {
            ExecuteScalar<string>("PRAGMA journal_mode=WAL");
        }

        /// <summary>
        /// Convert an input string to a quoted SQL string that can be safely used in queries.
        /// </summary>
        /// <returns>The quoted string.</returns>
        /// <param name="unsafeString">The unsafe string to quote.</param>
        private static string Quote(string unsafeString)
        {
            // TODO: Doesn't call sqlite3_mprintf("%Q", u) because we're waiting on https://github.com/ericsink/SQLitePCL.raw/issues/153
            if (unsafeString == null)
                return "NULL";
            var safe = unsafeString.Replace("'", "''");
            return "'" + safe + "'";
        }

        /// <summary>
        /// Sets the key used to encrypt/decrypt the database with "pragma key = ...".
        /// This must be the first thing you call before doing anything else with this connection
        /// if your database is encrypted.
        /// This only has an effect if you are using the SQLCipher nuget package.
        /// </summary>
        /// <param name="key">Ecryption key plain text that is converted to the real encryption key using PBKDF2 key derivation</param>
        private void SetKey(string key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            var q = Quote(key);
            Execute("pragma key = " + q);
        }

        /// <summary>
        /// Sets the key used to encrypt/decrypt the database.
        /// This must be the first thing you call before doing anything else with this connection
        /// if your database is encrypted.
        /// This only has an effect if you are using the SQLCipher nuget package.
        /// </summary>
        /// <param name="key">256-bit (32 byte) ecryption key data</param>
        private void SetKey(byte[] key)
        {
            if (key == null)
                throw new ArgumentNullException(nameof(key));
            if (key.Length != 32)
                throw new ArgumentException("Key must be 32 bytes (256-bit)", nameof(key));
            var s = String.Join("", key.Select(x => x.ToString("X2")));
            Execute("pragma key = \"x'" + s + "'\"");
        }

        /// <summary>
        /// Enable or disable extension loading.
        /// </summary>
        public void EnableLoadExtension(bool enabled)
        {
            SQLite3.Result r = SQLite3.EnableLoadExtension(Handle, enabled ? 1 : 0);
            if (r != SQLite3.Result.OK)
            {
                string msg = SQLite3.GetErrmsg(Handle);
                throw SQLiteException.New(r, msg);
            }
        }

#if !USE_SQLITEPCL_RAW
		static byte[] GetNullTerminatedUtf8 (string s)
		{
			var utf8Length = System.Text.Encoding.UTF8.GetByteCount (s);
			var bytes = new byte [utf8Length + 1];
			utf8Length = System.Text.Encoding.UTF8.GetBytes(s, 0, s.Length, bytes, 0);
			return bytes;
		}
#endif

        /// <summary>
        /// Sets a busy handler to sleep the specified amount of time when a table is locked.
        /// The handler will sleep multiple times until a total time of <see cref="BusyTimeout"/> has accumulated.
        /// </summary>
        public TimeSpan BusyTimeout
        {
            get { return _busyTimeout; }
            set
            {
                _busyTimeout = value;
                if (Handle != NullHandle)
                {
                    SQLite3.BusyTimeout(Handle, (int)_busyTimeout.TotalMilliseconds);
                }
            }
        }

        private struct IndexedColumn
        {
            public int Order;
            public string ColumnName;
        }

        private struct IndexInfo
        {
            public string IndexName;
            public string TableName;
            public bool Unique;
            public List<IndexedColumn> Columns;
        }

        /// <summary>
        /// Executes a "drop table" on the database.  This is non-recoverable.
        /// </summary>
        public int DropTable<T>()
        {
            return DropTable((Kit.Sql.Sqlite.TableMapping)GetMapping(typeof(T)));
        }

        /// <summary>
        /// Executes a "drop table" on the database.  This is non-recoverable.
        /// </summary>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        public int DropTable(TableMapping map)
        {
            var query = string.Format("drop table if exists \"{0}\"", map.TableName);
            return Execute(query);
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <param name="ty">Type to reflect to a database table.</param>
        /// <param name="createFlags">Optional flags allowing implicit PK and indexes based on naming conventions.</param>
        /// <returns>
        /// Whether the table was created or migrated.
        /// </returns>
        public override CreateTableResult CreateTable(Kit.Sql.Base.TableMapping map, CreateFlags createFlags = CreateFlags.None)
        {
            // Present a nice error if no columns specified
            if (map.Columns.Length == 0)
            {
                throw new Exception(string.Format("Cannot create a table without columns (does '{0}' have public properties?)", map.MappedType.FullName));
            }

            // Check if the table exists
            var result = CreateTableResult.Created;
            var existingCols = GetTableInfo(map.TableName);

            // Create or migrate it
            if (existingCols.Count == 0)
            {
                // Facilitate virtual tables a.k.a. full-text search.
                bool fts3 = (createFlags & CreateFlags.FullTextSearch3) != 0;
                bool fts4 = (createFlags & CreateFlags.FullTextSearch4) != 0;
                bool fts = fts3 || fts4;
                var @virtual = fts ? "virtual " : string.Empty;
                var @using = fts3 ? "using fts3 " : fts4 ? "using fts4 " : string.Empty;

                // Build query.
                var query = "create " + @virtual + "table if not exists \"" + map.TableName + "\" " + @using + "(\n";
                var decls = map.Columns.Select(p => Orm.SqlDecl(p, StoreDateTimeAsTicks, StoreTimeSpanAsTicks));

                var decl = string.Join(",\n", decls.ToArray());
                query += decl;
                query += ")";
                if (map.WithoutRowId)
                {
                    query += " without rowid";
                }

                var InitTableAttributetype = typeof(InitTableAttribute);
                var InitMethod = map.MappedType.GetMethods()
                      .Where(m => m.GetCustomAttributes(InitTableAttributetype, false).Any()).FirstOrDefault();
                if (InitMethod is not null && !InitMethod.IsStatic)
                {
                    throw new Exception($"Init table method must be static at {map.TableName}");
                }
                Execute(query);
                InitMethod?.Invoke(null, new object[] { this });
            }
            else
            {
                result = CreateTableResult.Migrated;
                MigrateTable((Kit.Sql.Sqlite.TableMapping)map, existingCols);
            }

            var indexes = new Dictionary<string, IndexInfo>();
            foreach (var c in map.Columns)
            {
                foreach (var i in c.Indices)
                {
                    var iname = i.Name ?? map.TableName + "_" + c.Name;
                    IndexInfo iinfo;
                    if (!indexes.TryGetValue(iname, out iinfo))
                    {
                        iinfo = new IndexInfo
                        {
                            IndexName = iname,
                            TableName = map.TableName,
                            Unique = i.Unique,
                            Columns = new List<IndexedColumn>()
                        };
                        indexes.Add(iname, iinfo);
                    }

                    if (i.Unique != iinfo.Unique)
                        throw new Exception("All the columns in an index must have the same value for their Unique property");

                    iinfo.Columns.Add(new IndexedColumn
                    {
                        Order = i.Order,
                        ColumnName = c.Name
                    });
                }
            }

            foreach (var indexName in indexes.Keys)
            {
                var index = indexes[indexName];
                var columns = index.Columns.OrderBy(i => i.Order).Select(i => i.ColumnName).ToArray();
                CreateIndex(indexName, index.TableName, columns, index.Unique);
            }

            return result;
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database for each type. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated for each type.
        /// </returns>
        public CreateTablesResult CreateTables<T, T2>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
        {
            return CreateTables(createFlags, typeof(T), typeof(T2));
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database for each type. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated for each type.
        /// </returns>
        public CreateTablesResult CreateTables<T, T2, T3>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new()
        {
            return CreateTables(createFlags, typeof(T), typeof(T2), typeof(T3));
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database for each type. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated for each type.
        /// </returns>
        public CreateTablesResult CreateTables<T, T2, T3, T4>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
        {
            return CreateTables(createFlags, typeof(T), typeof(T2), typeof(T3), typeof(T4));
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database for each type. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated for each type.
        /// </returns>
        public CreateTablesResult CreateTables<T, T2, T3, T4, T5>(CreateFlags createFlags = CreateFlags.None)
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
        {
            return CreateTables(createFlags, typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
        }

        /// <summary>
        /// Executes a "create table if not exists" on the database for each type. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated for each type.
        /// </returns>
        public CreateTablesResult CreateTables(CreateFlags createFlags = CreateFlags.None, params Type[] types)
        {
            var result = new CreateTablesResult();
            foreach (Type type in types)
            {
                var aResult = CreateTable(type, createFlags);
                result.Results[type] = aResult;
            }
            return result;
        }

        /// <summary>
        /// Creates an index for the specified table and columns.
        /// </summary>
        /// <param name="indexName">Name of the index to create</param>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="columnNames">An array of column names to index</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Zero on success.</returns>
        public int CreateIndex(string indexName, string tableName, string[] columnNames, bool unique = false)
        {
            const string sqlFormat = "create {2} index if not exists \"{3}\" on \"{0}\"(\"{1}\")";
            var sql = String.Format(sqlFormat, tableName, string.Join("\", \"", columnNames), unique ? "unique" : "", indexName);
            return Execute(sql);
        }

        /// <summary>
        /// Creates an index for the specified table and column.
        /// </summary>
        /// <param name="indexName">Name of the index to create</param>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="columnName">Name of the column to index</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Zero on success.</returns>
        public int CreateIndex(string indexName, string tableName, string columnName, bool unique = false)
        {
            return CreateIndex(indexName, tableName, new string[] { columnName }, unique);
        }

        /// <summary>
        /// Creates an index for the specified table and column.
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="columnName">Name of the column to index</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Zero on success.</returns>
        public int CreateIndex(string tableName, string columnName, bool unique = false)
        {
            return CreateIndex(tableName + "_" + columnName, tableName, columnName, unique);
        }

        /// <summary>
        /// Creates an index for the specified table and columns.
        /// </summary>
        /// <param name="tableName">Name of the database table</param>
        /// <param name="columnNames">An array of column names to index</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Zero on success.</returns>
        public int CreateIndex(string tableName, string[] columnNames, bool unique = false)
        {
            return CreateIndex(tableName + "_" + string.Join("_", columnNames), tableName, columnNames, unique);
        }

        /// <summary>
        /// Creates an index for the specified object property.
        /// e.g. CreateIndex&lt;Client&gt;(c => c.Name);
        /// </summary>
        /// <typeparam name="T">Type to reflect to a database table.</typeparam>
        /// <param name="property">Property to index</param>
        /// <param name="unique">Whether the index should be unique</param>
        /// <returns>Zero on success.</returns>
        public int CreateIndex<T>(Expression<Func<T, object>> property, bool unique = false)
        {
            MemberExpression mx;
            if (property.Body.NodeType == ExpressionType.Convert)
            {
                mx = ((UnaryExpression)property.Body).Operand as MemberExpression;
            }
            else
            {
                mx = (property.Body as MemberExpression);
            }
            var propertyInfo = mx.Member as PropertyInfo;
            if (propertyInfo == null)
            {
                throw new ArgumentException("The lambda expression 'property' should point to a valid Property");
            }

            var propName = propertyInfo.Name;

            var map = GetMapping<T>();
            var colName = map.FindColumnWithPropertyName(propName).Name;

            return CreateIndex(map.TableName, colName, unique);
        }

        [Attributes.Preserve(AllMembers = true)]
        public class ColumnInfo
        {
            //			public int cid { get; set; }

            [Column("name")]
            public string Name { get; set; }

            //			[Column ("type")]
            //			public string ColumnType { get; set; }

            public int notnull { get; set; }

            //			public string dflt_value { get; set; }

            //			public int pk { get; set; }

            public override string ToString()
            {
                return Name;
            }
        }

        /// <summary>
        /// Query the built-in sqlite table_info table for a specific tables columns.
        /// </summary>
        /// <returns>The columns contains in the table.</returns>
        /// <param name="tableName">Table name.</param>
        public List<ColumnInfo> GetTableInfo(string tableName)
        {
            var query = "pragma table_info(\"" + tableName + "\")";
            return Query<ColumnInfo>(query);
        }

        private void MigrateTable(TableMapping map, List<ColumnInfo> existingCols)
        {
            var toBeAdded = new List<TableMapping.Column>();

            foreach (var p in map.Columns)
            {
                var found = false;
                foreach (var c in existingCols)
                {
                    found = (string.Compare(p.Name, c.Name, StringComparison.OrdinalIgnoreCase) == 0);
                    if (found)
                        break;
                }
                if (!found)
                {
                    toBeAdded.Add(p);
                }
            }

            foreach (Base.TableMapping.Column p in toBeAdded)
            {
                var addCol = "alter table \"" + map.TableName + "\" add column " + Orm.SqlDecl(p, StoreDateTimeAsTicks, StoreTimeSpanAsTicks);
                Execute(addCol);
            }
        }

        /// <summary>
        /// Creates a new SQLiteCommand. Can be overridden to provide a sub-class.
        /// </summary>
        /// <seealso cref="SQLiteCommand.OnInstanceCreated"/>
        protected virtual SQLiteCommand NewCommand()
        {
            return new SQLiteCommand(this);
        }

        /// <summary>
        /// Creates a new SQLiteCommand given the command text with arguments. Place a '?'
        /// in the command text for each of the arguments.
        /// </summary>
        /// <param name="cmdText">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="ps">
        /// Arguments to substitute for the occurences of '?' in the command text.
        /// </param>
        /// <returns>
        /// A <see cref="SQLiteCommand"/>
        /// </returns>
        public override CommandBase CreateCommand(string cmdText, params object[] ps)
        {
            if (!_open)
                RenewConnection();

            var cmd = NewCommand();
            cmd.CommandText = cmdText;

            foreach (var o in ps)
            {
                cmd.Bind(o);
            }
            return cmd;
        }

        /// <summary>
        /// Creates a new SQLiteCommand given the command text with named arguments. Place a "[@:$]VVV"
        /// in the command text for each of the arguments. VVV represents an alphanumeric identifier.
        /// For example, @name :name and $name can all be used in the query.
        /// </summary>
        /// <param name="cmdText">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of "[@:$]VVV" in the command text.
        /// </param>
        /// <returns>
        /// A <see cref="SQLiteCommand" />
        /// </returns>
        public SQLiteCommand CreateCommand(string cmdText, Dictionary<string, object> args)
        {
            if (!_open)
                throw SQLiteException.New(SQLite3.Result.Error, "Cannot create commands from unopened database");

            SQLiteCommand cmd = NewCommand();
            cmd.CommandText = cmdText;
            foreach (var kv in args)
            {
                cmd.Bind(kv.Key, kv.Value);
            }
            return cmd;
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// Use this method instead of Query when you don't expect rows back. Such cases include
        /// INSERTs, UPDATEs, and DELETEs.
        /// You can set the Trace or TimeExecution properties of the connection
        /// to profile execution.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// The number of rows modified in the database as a result of this execution.
        /// </returns>
        public int Execute(string query, params object[] args)
        {
            CommandBase cmd = CreateCommand(query, args);
            Log.Logger.Debug(query);
            int r = cmd.ExecuteNonQuery();
            Log.Logger.Debug($"Rows affected [{r}]");
            return r;
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// Use this method when return primitive values.
        /// You can set the Trace or TimeExecution properties of the connection
        /// to profile execution.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// The number of rows modified in the database as a result of this execution.
        /// </returns>
        public T ExecuteScalar<T>(string query, params object[] args)
        {
            CommandBase cmd = CreateCommand(query, args);
            T r = cmd.ExecuteScalar<T>();
            return r;
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns each row of the result using the mapping automatically generated for
        /// the given type.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for each row returned by the query.
        /// </returns>
        public List<T> Query<T>(string query, params object[] args) where T : new()
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteQuery<T>();
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns the first column of each row of the result.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for the first column of each row returned by the query.
        /// </returns>
        public List<T> QueryScalars<T>(string query, params object[] args)
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteQueryScalars<T>().ToList();
        }

        /// <summary>
        /// /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns each row of the result using the mapping automatically generated for
        /// the given type.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for each row returned by the query.
        /// The enumerator (retrieved by calling GetEnumerator() on the result of this method)
        /// will call sqlite3_step on each call to MoveNext, so the database
        /// connection must remain open for the lifetime of the enumerator.
        /// </returns>
        public override IEnumerable<T> DeferredQuery<T>(string query, params object[] args)
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteDeferredQuery<T>();
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns each row of the result using the specified mapping. This function is
        /// only used by libraries in order to query the database via introspection. It is
        /// normally not used.
        /// </summary>
        /// <param name="map">
        /// A <see cref="TableMapping"/> to use to convert the resulting rows
        /// into objects.
        /// </param>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for each row returned by the query.
        /// </returns>
        public List<object> Query(TableMapping map, string query, params object[] args)
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteQuery<object>(map);
        }

        /// <summary>
        /// WARNING: Changes made through this method will not be tracked on history.
        /// Creates a SQLiteCommand given the command text (SQL) with arguments. Place a '?'
        /// in the command text for each of the arguments and then executes that command.
        /// It returns each row of the result using the specified mapping. This function is
        /// only used by libraries in order to query the database via introspection. It is
        /// normally not used.
        /// </summary>
        /// <param name="map">
        /// A <see cref="TableMapping"/> to use to convert the resulting rows
        /// into objects.
        /// </param>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// An enumerable with one result for each row returned by the query.
        /// The enumerator (retrieved by calling GetEnumerator() on the result of this method)
        /// will call sqlite3_step on each call to MoveNext, so the database
        /// connection must remain open for the lifetime of the enumerator.
        /// </returns>
        public IEnumerable<object> DeferredQuery(TableMapping map, string query, params object[] args)
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteDeferredQuery<object>(map);
        }

        /// <summary>
        /// Returns a queryable interface to the table represented by the given type.
        /// </summary>
        /// <returns>
        /// A queryable object that is able to translate Where, OrderBy, and Take
        /// queries into native SQL.
        /// </returns>
        public override TableQuery<T> Table<T>()
        {
            return new SQLiteTableQuery<T>(this);
        }

        public override BaseTableQuery Table(Type Type)
        {
            var queryType = typeof(SQLiteTableQuery<>);
            queryType = queryType.MakeGenericType(Type);

            return (BaseTableQuery)Activator.CreateInstance(queryType, new object[] { this });
        }

        /// <summary>
        /// Attempts to retrieve an object with the given primary key from the table
        /// associated with the specified type. Use of this method requires that
        /// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
        /// </summary>
        /// <param name="pk">
        /// The primary key.
        /// </param>
        /// <returns>
        /// The object with the given primary key. Throws a not found exception
        /// if the object is not found.
        /// </returns>
        public T Get<T>(object pk) where T : new()
        {
            var map = GetMapping(typeof(T));
            return Query<T>(map.GetByPrimaryKeySql, pk).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to retrieve an object with the given primary key from the table
        /// associated with the specified type. Use of this method requires that
        /// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
        /// </summary>
        /// <param name="pk">
        /// The primary key.
        /// </param>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        /// <returns>
        /// The object with the given primary key. Throws a not found exception
        /// if the object is not found.
        /// </returns>
        public object Get(object pk, TableMapping map)
        {
            return Query(map, map.GetByPrimaryKeySql, pk).First();
        }

        /// <summary>
        /// Attempts to retrieve the first object that matches the predicate from the table
        /// associated with the specified type.
        /// </summary>
        /// <param name="predicate">
        /// A predicate for which object to find.
        /// </param>
        /// <returns>
        /// The object that matches the given predicate. Throws a not found exception
        /// if the object is not found.
        /// </returns>
        public T Get<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            return Table<T>().Where(predicate).First();
        }

        /// <summary>
        /// Attempts to retrieve an object with the given primary key from the table
        /// associated with the specified type. Use of this method requires that
        /// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
        /// </summary>
        /// <param name="pk">
        /// The primary key.
        /// </param>
        /// <returns>
        /// The object with the given primary key or null
        /// if the object is not found.
        /// </returns>
        public override T Find<T>(object pk)
        {
            var map = GetMapping(typeof(T));
            if (map.PK is null)
            {
                throw new NotSupportedException(
                    $"No se puede ejecutar Find en la tabla {map.TableName} porque no tiene llave primaria");
            }
            return Query<T>(map.GetByPrimaryKeySql, pk).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to retrieve an object with the given primary key from the table
        /// associated with the specified type. Use of this method requires that
        /// the given type have a designated PrimaryKey (using the PrimaryKeyAttribute).
        /// </summary>
        /// <param name="pk">
        /// The primary key.
        /// </param>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        /// <returns>
        /// The object with the given primary key or null
        /// if the object is not found.
        /// </returns>
        public object Find(object pk, TableMapping map)
        {
            return Query(map, map.GetByPrimaryKeySql, pk).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to retrieve the first object that matches the predicate from the table
        /// associated with the specified type.
        /// </summary>
        /// <param name="predicate">
        /// A predicate for which object to find.
        /// </param>
        /// <returns>
        /// The object that matches the given predicate or null
        /// if the object is not found.
        /// </returns>
        public T Find<T>(Expression<Func<T, bool>> predicate) where T : new()
        {
            return Table<T>().Where(predicate).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to retrieve the first object that matches the query from the table
        /// associated with the specified type.
        /// </summary>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// The object that matches the given predicate or null
        /// if the object is not found.
        /// </returns>
        public T FindWithQuery<T>(string query, params object[] args) where T : new()
        {
            return Query<T>(query, args).FirstOrDefault();
        }

        /// <summary>
        /// Attempts to retrieve the first object that matches the query from the table
        /// associated with the specified type.
        /// </summary>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        /// <param name="query">
        /// The fully escaped SQL.
        /// </param>
        /// <param name="args">
        /// Arguments to substitute for the occurences of '?' in the query.
        /// </param>
        /// <returns>
        /// The object that matches the given predicate or null
        /// if the object is not found.
        /// </returns>
        public object FindWithQuery(TableMapping map, string query, params object[] args)
        {
            return Query(map, query, args).FirstOrDefault();
        }

        /// <summary>
        /// Whether <see cref="BeginTransaction"/> has been called and the database is waiting for a <see cref="Commit"/>.
        /// </summary>
        public bool IsInTransaction
        {
            get { return _transactionDepth > 0; }
        }

        /// <summary>
        /// Begins a new transaction. Call <see cref="Commit"/> to end the transaction.
        /// </summary>
        /// <example cref="System.InvalidOperationException">Throws if a transaction has already begun.</example>
        public void BeginTransaction()
        {
            // The BEGIN command only works if the transaction stack is empty,
            //    or in other words if there are no pending transactions.
            // If the transaction stack is not empty when the BEGIN command is invoked,
            //    then the command fails with an error.
            // Rather than crash with an error, we will just ignore calls to BeginTransaction
            //    that would result in an error.
            if (Interlocked.CompareExchange(ref _transactionDepth, 1, 0) == 0)
            {
                try
                {
                    Execute("begin transaction");
                }
                catch (Exception ex)
                {
                    var sqlExp = ex as SQLiteException;
                    if (sqlExp != null)
                    {
                        // It is recommended that applications respond to the errors listed below
                        //    by explicitly issuing a ROLLBACK command.
                        // TODO: This rollback failsafe should be localized to all throw sites.
                        switch (sqlExp.Result)
                        {
                            case SQLite3.Result.IOError:
                            case SQLite3.Result.Full:
                            case SQLite3.Result.Busy:
                            case SQLite3.Result.NoMem:
                            case SQLite3.Result.Interrupt:
                                RollbackTo(null, true);
                                break;
                        }
                    }
                    else
                    {
                        // Call decrement and not VolatileWrite in case we've already
                        //    created a transaction point in SaveTransactionPoint since the catch.
                        Interlocked.Decrement(ref _transactionDepth);
                    }

                    throw;
                }
            }
            else
            {
                // Calling BeginTransaction on an already open transaction is invalid
                throw new InvalidOperationException("Cannot begin a transaction while already in a transaction.");
            }
        }

        /// <summary>
        /// Creates a savepoint in the database at the current point in the transaction timeline.
        /// Begins a new transaction if one is not in progress.
        ///
        /// Call <see cref="RollbackTo(string)"/> to undo transactions since the returned savepoint.
        /// Call <see cref="Release"/> to commit transactions after the savepoint returned here.
        /// Call <see cref="Commit"/> to end the transaction, committing all changes.
        /// </summary>
        /// <returns>A string naming the savepoint.</returns>
        public string SaveTransactionPoint()
        {
            int depth = Interlocked.Increment(ref _transactionDepth) - 1;
            string retVal = "S" + _rand.Next(short.MaxValue) + "D" + depth;

            try
            {
                Execute("savepoint " + retVal);
            }
            catch (Exception ex)
            {
                var sqlExp = ex as SQLiteException;
                if (sqlExp != null)
                {
                    // It is recommended that applications respond to the errors listed below
                    //    by explicitly issuing a ROLLBACK command.
                    // TODO: This rollback failsafe should be localized to all throw sites.
                    switch (sqlExp.Result)
                    {
                        case SQLite3.Result.IOError:
                        case SQLite3.Result.Full:
                        case SQLite3.Result.Busy:
                        case SQLite3.Result.NoMem:
                        case SQLite3.Result.Interrupt:
                            RollbackTo(null, true);
                            break;
                    }
                }
                else
                {
                    Interlocked.Decrement(ref _transactionDepth);
                }

                throw;
            }

            return retVal;
        }

        /// <summary>
        /// Rolls back the transaction that was begun by <see cref="BeginTransaction"/> or <see cref="SaveTransactionPoint"/>.
        /// </summary>
        public void Rollback()
        {
            RollbackTo(null, false);
        }

        /// <summary>
        /// Rolls back the savepoint created by <see cref="BeginTransaction"/> or SaveTransactionPoint.
        /// </summary>
        /// <param name="savepoint">The name of the savepoint to roll back to, as returned by <see cref="SaveTransactionPoint"/>.  If savepoint is null or empty, this method is equivalent to a call to <see cref="Rollback"/></param>
        public void RollbackTo(string savepoint)
        {
            RollbackTo(savepoint, false);
        }

        /// <summary>
        /// Rolls back the transaction that was begun by <see cref="BeginTransaction"/>.
        /// </summary>
        /// <param name="savepoint">The name of the savepoint to roll back to, as returned by <see cref="SaveTransactionPoint"/>.  If savepoint is null or empty, this method is equivalent to a call to <see cref="Rollback"/></param>
        /// <param name="noThrow">true to avoid throwing exceptions, false otherwise</param>
        private void RollbackTo(string savepoint, bool noThrow)
        {
            // Rolling back without a TO clause rolls backs all transactions
            //    and leaves the transaction stack empty.
            try
            {
                if (String.IsNullOrEmpty(savepoint))
                {
                    if (Interlocked.Exchange(ref _transactionDepth, 0) > 0)
                    {
                        Execute("rollback");
                    }
                }
                else
                {
                    DoSavePointExecute(savepoint, "rollback to ");
                }
            }
            catch (SQLiteException)
            {
                if (!noThrow)
                    throw;
            }
            // No need to rollback if there are no transactions open.
        }

        /// <summary>
        /// Releases a savepoint returned from <see cref="SaveTransactionPoint"/>.  Releasing a savepoint
        ///    makes changes since that savepoint permanent if the savepoint began the transaction,
        ///    or otherwise the changes are permanent pending a call to <see cref="Commit"/>.
        ///
        /// The RELEASE command is like a COMMIT for a SAVEPOINT.
        /// </summary>
        /// <param name="savepoint">The name of the savepoint to release.  The string should be the result of a call to <see cref="SaveTransactionPoint"/></param>
        public void Release(string savepoint)
        {
            try
            {
                DoSavePointExecute(savepoint, "release ");
            }
            catch (SQLiteException ex)
            {
                if (ex.Result == SQLite3.Result.Busy)
                {
                    // Force a rollback since most people don't know this function can fail
                    // Don't call Rollback() since the _transactionDepth is 0 and it won't try
                    // Calling rollback makes our _transactionDepth variable correct.
                    // Writes to the database only happen at depth=0, so this failure will only happen then.
                    try
                    {
                        Execute("rollback");
                    }
                    catch
                    {
                        // rollback can fail in all sorts of wonderful version-dependent ways. Let's just hope for the best
                    }
                }
                throw;
            }
        }

        private void DoSavePointExecute(string savepoint, string cmd)
        {
            // Validate the savepoint
            int firstLen = savepoint.IndexOf('D');
            if (firstLen >= 2 && savepoint.Length > firstLen + 1)
            {
                int depth;
                if (Int32.TryParse(savepoint.Substring(firstLen + 1), out depth))
                {
                    // TODO: Mild race here, but inescapable without locking almost everywhere.
                    if (0 <= depth && depth < _transactionDepth)
                    {
#if NETFX_CORE || USE_SQLITEPCL_RAW || NETCORE
                        Volatile.Write(ref _transactionDepth, depth);
#elif SILVERLIGHT
						_transactionDepth = depth;
#else
                        Thread.VolatileWrite (ref _transactionDepth, depth);
#endif
                        Execute(cmd + savepoint);
                        return;
                    }
                }
            }

            throw new ArgumentException("savePoint is not valid, and should be the result of a call to SaveTransactionPoint.", "savePoint");
        }

        /// <summary>
        /// Commits the transaction that was begun by <see cref="BeginTransaction"/>.
        /// </summary>
        public void Commit()
        {
            if (Interlocked.Exchange(ref _transactionDepth, 0) != 0)
            {
                try
                {
                    Execute("commit");
                }
                catch
                {
                    // Force a rollback since most people don't know this function can fail
                    // Don't call Rollback() since the _transactionDepth is 0 and it won't try
                    // Calling rollback makes our _transactionDepth variable correct.
                    try
                    {
                        Execute("rollback");
                    }
                    catch
                    {
                        // rollback can fail in all sorts of wonderful version-dependent ways. Let's just hope for the best
                    }
                    throw;
                }
            }
            // Do nothing on a commit with no open transaction
        }

        /// <summary>
        /// Executes <paramref name="action"/> within a (possibly nested) transaction by wrapping it in a SAVEPOINT. If an
        /// exception occurs the whole transaction is rolled back, not just the current savepoint. The exception
        /// is rethrown.
        /// </summary>
        /// <param name="action">
        /// The <see cref="Action"/> to perform within a transaction. <paramref name="action"/> can contain any number
        /// of operations on the connection but should never call <see cref="BeginTransaction"/> or
        /// <see cref="Commit"/>.
        /// </param>
        public void RunInTransaction(Action action)
        {
            try
            {
                var savePoint = SaveTransactionPoint();
                action();
                Release(savePoint);
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
        }

        /// <summary>
        /// Inserts all specified objects.
        /// </summary>
        /// <param name="objects">
        /// An <see cref="IEnumerable"/> of the objects to insert.
        /// <param name="runInTransaction"/>
        /// A boolean indicating if the inserts should be wrapped in a transaction.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>

        public int InsertAll(System.Collections.IEnumerable objects, bool runInTransaction = true)
        {
            var c = 0;
            if (runInTransaction)
            {
                RunInTransaction(() =>
                {
                    foreach (var r in objects)
                    {
                        c += Insert(r);
                    }
                });
            }
            else
            {
                foreach (var r in objects)
                {
                    c += Insert(r);
                }
            }
            return c;
        }

        public int InsertAll(params object[] objects)
        {
            var c = 0;
            foreach (var r in objects)
            {
                c += Insert(r);
            }
            return c;
        }

        /// <summary>
        /// Inserts all specified objects.
        /// </summary>
        /// <param name="objects">
        /// An <see cref="IEnumerable"/> of the objects to insert.
        /// </param>
        /// <param name="extra">
        /// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
        /// </param>
        /// <param name="runInTransaction">
        /// A boolean indicating if the inserts should be wrapped in a transaction.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public int InsertAll(System.Collections.IEnumerable objects, string extra, bool runInTransaction = true)
        {
            var c = 0;
            if (runInTransaction)
            {
                RunInTransaction(() =>
                {
                    foreach (var r in objects)
                    {
                        c += Insert(r, extra);
                    }
                });
            }
            else
            {
                foreach (var r in objects)
                {
                    c += Insert(r, extra);
                }
            }
            return c;
        }

        /// <summary>
        /// Inserts all specified objects.
        /// </summary>
        /// <param name="objects">
        /// An <see cref="IEnumerable"/> of the objects to insert.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <param name="runInTransaction">
        /// A boolean indicating if the inserts should be wrapped in a transaction.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public int InsertAll(System.Collections.IEnumerable objects, Type objType, bool runInTransaction = true)
        {
            var c = 0;
            if (runInTransaction)
            {
                RunInTransaction(() =>
                {
                    foreach (var r in objects)
                    {
                        c += Insert(r, objType);
                    }
                });
            }
            else
            {
                foreach (var r in objects)
                {
                    c += Insert(r, objType);
                }
            }
            return c;
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public int Insert(object obj, bool shouldnotify = true)
        {
            if (obj == null)
            {
                return 0;
            }
            return Insert(obj, "", Orm.GetType(obj), shouldnotify);
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// If a UNIQUE constraint violation occurs with
        /// some pre-existing object, this function deletes
        /// the old object.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <returns>
        /// The number of rows modified.
        /// </returns>
        public override int InsertOrReplace(object obj, bool shouldnotify = true)
        {
            if (obj == null)
            {
                return 0;
            }
            return Insert(obj, "OR REPLACE", Orm.GetType(obj), shouldnotify);
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public int Insert(object obj, Type objType, bool shouldnotify = true)
        {
            return Insert(obj, "", objType, shouldnotify);
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="extra">
        /// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>

        public override int Insert(object obj, string extra)
        {
            if (obj == null)
            {
                return 0;
            }
            return Insert(obj, extra, Orm.GetType(obj));
        }

        /// <summary>
        /// Inserts the given object (and updates its
        /// auto incremented primary key if it has one).
        /// The return value is the number of rows added to the table.
        /// </summary>
        /// <param name="obj">
        /// The object to insert.
        /// </param>
        /// <param name="extra">
        /// Literal SQL code that gets placed into the command. INSERT {extra} INTO ...
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows added to the table.
        /// </returns>
        public override int Insert(object obj, string extra, Type objType, bool shouldnotify = true)
        {
            if (obj == null || objType == null)
            {
                return 0;
            }
            return Insert(obj, extra, GetMapping(objType), shouldnotify);
        }

        public override int Insert(object obj, string extra, Base.TableMapping map, bool shouldnotify = true)
        {
            if (map.PK != null && map.HasAutoIncPK)
            {
                if (map.PK.ColumnType == typeof(Guid) && map.PK.GetValue(obj).Equals(Guid.Empty))
                {
                    map.PK.SetValue(obj, Guid.NewGuid());
                }
            }

            var replacing = string.Compare(extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;
            if (replacing && map.HasAutoIncPK)
            {
                if (map.PK.GetValue(obj) is int autoPk)
                {
                    if (autoPk <= 0)
                    {
                        extra = string.Empty;
                        replacing = false;
                    }
                }
            }

            var cols = replacing ? map.InsertOrReplaceColumns : map.InsertColumns;
            var vals = new object[cols.Length];
            if (obj is IGuid i_sync && i_sync.Guid == Guid.Empty)
            {
                i_sync.Guid = Guid.NewGuid();
            }
            for (var i = 0; i < vals.Length; i++)
            {
                vals[i] = cols[i].GetValue(obj);
            }
            int count;

            lock (this)
            {
                if (this.IsClosed)
                    RenewConnection();
                PreparedSqlLiteInsertCommand insertCmd = GetInsertCommand((Kit.Sql.Sqlite.TableMapping)map, extra);

                // We lock here to protect the prepared statement returned via GetInsertCommand.
                // A SQLite prepared statement can be bound for only one operation at a time.
                try
                {
                    count = insertCmd.ExecuteNonQuery(vals);
                    Log.Logger.Debug("INSERTED ROWS: [{0}]", count);
                }
                catch (SQLiteException ex)
                {
                    if (SQLite3.ExtendedErrCode(this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
                    {
                        throw NotNullConstraintViolationException.New(ex.Result, ex.Message, map, obj);
                    }
                    throw;
                }

                if (map.HasAutoIncPK)
                {
                    var id = SQLite3.LastInsertRowid(this.Handle);
                    if (id <= 0)
                    {
                    }
                    map.SetAutoIncPK(obj, id);
                }
                this.Close();
            }

            if (shouldnotify && count > 0)
                OnTableChanged((Kit.Sql.Sqlite.TableMapping)map, NotifyTableChangedAction.Insert, obj);

            return count;
        }

        private readonly Dictionary<Tuple<string, string>, PreparedSqlLiteInsertCommand> _insertCommandMap = new Dictionary<Tuple<string, string>, PreparedSqlLiteInsertCommand>();

        private PreparedSqlLiteInsertCommand GetInsertCommand(TableMapping map, string extra)
        {
            PreparedSqlLiteInsertCommand prepCmd;

            var key = Tuple.Create(map.MappedType.FullName, extra);

            lock (_insertCommandMap)
            {
                if (_insertCommandMap.TryGetValue(key, out prepCmd))
                {
                    return prepCmd;
                }
            }

            prepCmd = CreateInsertCommand(map, extra);

            lock (_insertCommandMap)
            {
                if (_insertCommandMap.TryGetValue(key, out var existing))
                {
                    prepCmd.Dispose();
                    return existing;
                }

                _insertCommandMap.Add(key, prepCmd);
            }

            return prepCmd;
        }

        private PreparedSqlLiteInsertCommand CreateInsertCommand(TableMapping map, string extra)
        {
            var cols = map.InsertColumns;
            string insertSql;
            if (cols.Length == 0 && map.Columns.Length == 1 && map.Columns[0].IsAutoInc)
            {
                insertSql = string.Format("insert {1} into \"{0}\" default values", map.TableName, extra);
            }
            else
            {
                var replacing = string.Compare(extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;

                if (replacing)
                {
                    cols = map.InsertOrReplaceColumns;
                }

                insertSql = string.Format("insert {3} into \"{0}\"({1}) values ({2})", map.TableName,
                    string.Join(",", (from c in cols
                                      select "\"" + c.Name + "\"").ToArray()),
                    string.Join(",", (from c in cols
                                      select "?").ToArray()), extra);
            }

            var insertCommand = new PreparedSqlLiteInsertCommand(this, insertSql);
            return insertCommand;
        }

        /// <summary>
        /// Updates all of the columns of a table using the specified object
        /// except for its primary key.
        /// The object is required to have a primary key.
        /// </summary>
        /// <param name="obj">
        /// The object to update. It must have a primary key designated using the PrimaryKeyAttribute.
        /// </param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public override int Update(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return Update(obj, Orm.GetType(obj));
        }

        public override int Update(object obj, string Condition, bool shouldnotify = true, params BaseTableQuery.Condition[] pconditions)
        {
            if (obj == null)
            {
                return 0;
            }
            ////////////////////////
            ///
            List<BaseTableQuery.Condition> conditions = new List<BaseTableQuery.Condition>(pconditions);
            List<object> args = new List<object>(conditions.Select(x => x.Value));

            object[] conditions_array = (this is SQLiteConnection ? conditions.ToArray() : args.ToArray());

            var Table = this.Table(obj.GetType()).Table;

            var cols = from p in Table.Columns
                       where p.Name != nameof(ISync.Guid) &&
                       !p.IsAutoInc
                       select p;
            var vals = from c in cols
                       where c.Name != nameof(ISync.Guid)
                       select c.GetValue(obj);
            var ps = new List<object>(vals);
            if (ps.Count == 0)
            {
                // There is a PK but no accompanying data,
                // so reset the PK to make the UPDATE work.
                cols = Table.Columns;
                vals = from c in cols
                       select c.GetValue(obj);
                ps = new List<object>(vals);
            }
            ps.AddRange(conditions.Select(x => x.Value));
            var q = string.Format("update \"{0}\" set {1} WHERE {2}",
                Table.TableName, string.Join(",", (from c in cols
                                                   select "\"" + c.Name + "\" = ? ").ToArray()), Condition);
            ///
            int rowsAffected = 0;
            try
            {
                rowsAffected = Execute(q, ps.ToArray());
            }
            catch (SQLiteException ex)
            {
                if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
                {
                    throw NotNullConstraintViolationException.New(ex, Table, obj);
                }

                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?", map.PK.GetValue(obj)));
                OnTableChanged((Kit.Sql.Sqlite.TableMapping)Table, NotifyTableChangedAction.Update, obj);
            }

            return rowsAffected;
        }

        public override int Update<T>(T obj, Expression<Func<T, bool>> predExpr, bool shouldnotify = true)

        {
            if (obj == null)
            {
                return 0;
            }
            ////////////////////////

            Expression pred = null;

            if (predExpr != null && predExpr.NodeType == ExpressionType.Lambda)
            {
                var lambda = (LambdaExpression)predExpr;

                pred = Expression.AndAlso(lambda.Body, lambda.Body);
            }

            List<object> args = new List<object>();
            SQLiteTableQuery<T> table = (SQLiteTableQuery<T>)this.Table<T>();
            List<BaseTableQuery.Condition> conditions = new List<BaseTableQuery.Condition>();

            var w = table.CompileExpr(pred, args, conditions);
            conditions.RemoveAll(x => x.Value is Kit.Sql.Base.TableMapping);
            conditions.RemoveAll(x => !x.IsComplete);
            conditions = conditions.DistinctBy(x => x.ColumnName).ToList();

            object[] conditions_array = (this is SQLServerTableQuery<T> ? conditions.ToArray() : args.ToArray());
            /// ///////////////////////77
            ///
            ///
            var cols = from p in table.Table.Columns
                       where p.Name != nameof(ISync.Guid)
                       select p;
            var vals = from c in cols
                       where c.Name != nameof(ISync.Guid)
                       select c.GetValue(obj);
            var ps = new List<object>(vals);
            if (ps.Count == 0)
            {
                // There is a PK but no accompanying data,
                // so reset the PK to make the UPDATE work.
                cols = table.Table.Columns;
                vals = from c in cols
                       select c.GetValue(obj);
                ps = new List<object>(vals);
            }
            ps.AddRange(conditions.Select(x => x.Value));
            var q = string.Format("update \"{0}\" set {1} WHERE {2}",
                table.Table.TableName, string.Join(",", (from c in cols
                                                         select "\"" + c.Name + "\" = ? ").ToArray()), w.CommandText);
            ///
            int rowsAffected = 0;
            try
            {
                rowsAffected = Execute(q, ps.ToArray());
            }
            catch (SQLiteException ex)
            {
                if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
                {
                    throw NotNullConstraintViolationException.New(ex, table.Table, obj);
                }

                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?", map.PK.GetValue(obj)));
                OnTableChanged((Kit.Sql.Sqlite.TableMapping)table.Table, NotifyTableChangedAction.Update, obj);
            }

            return rowsAffected;
        }

        /// <summary>
        /// Updates all of the columns of a table using the specified object
        /// except for its primary key.
        /// The object is required to have a primary key.
        /// </summary>
        /// <param name="obj">
        /// The object to update. It must have a primary key designated using the PrimaryKeyAttribute.
        /// </param>
        /// <param name="objType">
        /// The type of object to insert.
        /// </param>
        /// <returns>
        /// The number of rows updated.
        /// </returns>
        public override int Update(object obj, Type objType, bool shouldnotify = true)
        {
            int rowsAffected = 0;
            if (obj == null || objType == null)
            {
                return 0;
            }

            var map = GetMapping(objType);

            var pk = map.PK;

            //if (pk == null)
            //{
            //    throw new NotSupportedException("Cannot update " + map.TableName + ": it has no PK");
            //}

            var cols = from p in map.Columns
                       where p != pk && p.Name != nameof(ISync.Guid)
                       select p;
            var vals = from c in cols
                       where c.Name != nameof(ISync.Guid)
                       select c.GetValue(obj);
            var ps = new List<object>(vals);
            if (ps.Count == 0)
            {
                // There is a PK but no accompanying data,
                // so reset the PK to make the UPDATE work.
                cols = map.Columns;
                vals = from c in cols
                       select c.GetValue(obj);
                ps = new List<object>(vals);
            }
            string q = string.Empty;
            if (pk is not null)
            {
                ps.Add(pk.GetValue(obj));
                q = string.Format("update \"{0}\" set {1} where {2} = ? ", map.TableName, string.Join(",", (from c in cols
                                                                                                            select "\"" + c.Name + "\" = ? ").ToArray()), pk.Name);
            }
            else
            {
                q = string.Format("update \"{0}\" set {1}", map.TableName, string.Join(",", (from c in cols
                                                                                             select "\"" + c.Name + "\" = ? ").ToArray()));
            }

            try
            {
                rowsAffected = Execute(q, ps.ToArray());
            }
            catch (SQLiteException ex)
            {
                if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode(this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull)
                {
                    throw NotNullConstraintViolationException.New(ex, map, obj);
                }

                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?", map.PK.GetValue(obj)));
                OnTableChanged((Kit.Sql.Sqlite.TableMapping)map, NotifyTableChangedAction.Update, obj);
            }

            return rowsAffected;
        }

        /// <summary>
        /// Updates all specified objects.
        /// </summary>
        /// <param name="objects">
        /// An <see cref="IEnumerable"/> of the objects to insert.
        /// </param>
        /// <param name="runInTransaction">
        /// A boolean indicating if the inserts should be wrapped in a transaction
        /// </param>
        /// <returns>
        /// The number of rows modified.
        /// </returns>
        public int UpdateAll(System.Collections.IEnumerable objects, bool runInTransaction = true)
        {
            var c = 0;
            if (runInTransaction)
            {
                RunInTransaction(() =>
                {
                    foreach (var r in objects)
                    {
                        c += Update(r);
                    }
                });
            }
            else
            {
                foreach (var r in objects)
                {
                    c += Update(r);
                }
            }
            return c;
        }

        /// <summary>
        /// Deletes the given object from the database using its primary key.
        /// </summary>
        /// <param name="objectToDelete">
        /// The object to delete. It must have a primary key designated using the PrimaryKeyAttribute.
        /// </param>
        /// <returns>
        /// The number of rows deleted.
        /// </returns>
        public override int Delete(object objectToDelete)
        {
            var map = GetMapping(Orm.GetType(objectToDelete));
            var pk = map.PK;
            if (pk == null)
            {
                throw new NotSupportedException("Cannot delete " + map.TableName + ": it has no PK");
            }
            var q = string.Format("delete from \"{0}\" where \"{1}\" = ?", map.TableName, pk.Name);

            var count = Execute(q, pk.GetValue(objectToDelete));
            if (count > 0)
                OnTableChanged((Kit.Sql.Sqlite.TableMapping)map, NotifyTableChangedAction.Delete, objectToDelete);
            return count;
        }

        /// <summary>
        /// Deletes the object with the specified primary key.
        /// </summary>
        /// <param name="primaryKey">
        /// The primary key of the object to delete.
        /// </param>
        /// <returns>
        /// The number of objects deleted.
        /// </returns>
        /// <typeparam name='T'>
        /// The type of object.
        /// </typeparam>
        public int Delete<T>(object primaryKey)
        {
            return Delete(primaryKey, (Kit.Sql.Sqlite.TableMapping)GetMapping(typeof(T)));
        }

        /// <summary>
        /// Deletes the object with the specified primary key.
        /// </summary>
        /// <param name="primaryKey">
        /// The primary key of the object to delete.
        /// </param>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        /// <returns>
        /// The number of objects deleted.
        /// </returns>
        public int Delete(object primaryKey, TableMapping map)
        {
            var pk = map.PK;
            if (pk == null)
            {
                throw new NotSupportedException("Cannot delete " + map.TableName + ": it has no PK");
            }
            var q = string.Format("delete from \"{0}\" where \"{1}\" = ?", map.TableName, pk.Name);

            Guid guid_key = Guid.Empty;
            if (map.SyncMode.Direction != SyncDirection.NoSync)
            {
                guid_key = ExecuteScalar<Guid>($"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?",
                   primaryKey);
            }

            var count = Execute(q, primaryKey);
            if (count > 0 && map.SyncMode.Direction != SyncDirection.NoSync)
                OnTableChanged(map, NotifyTableChangedAction.Delete, guid_key);
            return count;
        }

        /// <summary>
        /// Deletes all the objects from the specified table.
        /// WARNING WARNING: Let me repeat. It deletes ALL the objects from the
        /// specified table. Do you really want to do that?
        /// </summary>
        /// <returns>
        /// The number of objects deleted.
        /// </returns>
        /// <typeparam name='T'>
        /// The type of objects to delete.
        /// </typeparam>
        public int DeleteAll<T>(bool shouldnotify = true)
        {
            var map = GetMapping(typeof(T));
            return DeleteAll((Kit.Sql.Sqlite.TableMapping)map, shouldnotify);
        }

        /// <summary>
        /// Deletes all the objects from the specified table.
        /// WARNING WARNING: Let me repeat. It deletes ALL the objects from the
        /// specified table. Do you really want to do that?
        /// </summary>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        /// <returns>
        /// The number of objects deleted.
        /// </returns>
        public int DeleteAll(TableMapping map, bool shouldnotify)
        {
            var query = string.Format("delete from \"{0}\"", map.TableName);
            OnTableDeleteAll(map, shouldnotify);
            var count = Execute(query);
            if (shouldnotify && count > 0)
                OnTableChanged(map, NotifyTableChangedAction.Delete, null);
            return count;
        }

        /// <summary>
        /// Backup the entire database to the specified path.
        /// </summary>
        /// <param name="destinationDatabasePath">Path to backup file.</param>
        /// <param name="databaseName">The name of the database to backup (usually "main").</param>
        public void Backup(string destinationDatabasePath, string databaseName = "main")
        {
            // Open the destination
            var r = SQLite3.Open(destinationDatabasePath, out var destHandle);
            if (r != SQLite3.Result.OK)
            {
                throw SQLiteException.New(r, "Failed to open destination database");
            }

            // Init the backup
            var backup = SQLite3.BackupInit(destHandle, databaseName, Handle, databaseName);
            if (backup == NullBackupHandle)
            {
                SQLite3.Close(destHandle);
                throw new Exception("Failed to create backup");
            }

            // Perform it
            SQLite3.BackupStep(backup, -1);
            SQLite3.BackupFinish(backup);

            // Check for errors
            r = SQLite3.GetResult(destHandle);
            string msg = "";
            if (r != SQLite3.Result.OK)
            {
                msg = SQLite3.GetErrmsg(destHandle);
            }

            // Close everything and report errors
            SQLite3.Close(destHandle);
            if (r != SQLite3.Result.OK)
            {
                throw SQLiteException.New(r, msg);
            }
        }

        ~SQLiteConnection()
        {
            Dispose(false);
        }

        public override void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Close()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposing)
        {
            var useClose2 = LibVersionNumber >= 3007014;

            if (_open && Handle != NullHandle)
            {
                try
                {
                    if (disposing)
                    {
                        lock (_insertCommandMap)
                        {
                            foreach (var sqlInsertCommand in _insertCommandMap.Values)
                            {
                                sqlInsertCommand.Dispose();
                            }
                            _insertCommandMap.Clear();
                        }

                        var r = useClose2 ? SQLite3.Close2(Handle) : SQLite3.Close(Handle);
                        if (r != SQLite3.Result.OK)
                        {
                            string msg = SQLite3.GetErrmsg(Handle);
                            throw SQLiteException.New(r, msg);
                        }
                    }
                    else
                    {
                        var r = useClose2 ? SQLite3.Close2(Handle) : SQLite3.Close(Handle);
                    }
                }
                finally
                {
                    Handle = NullHandle;
                    _open = false;
                }
            }
        }

        public event EventHandler<NotifyTableChangedEventArgs> TableChanged;

        private void OnTableDeleteAll(TableMapping table, bool shouldnotify)
        {
            if (!shouldnotify) return;
            Table<ChangesHistory>().Delete(x => x.TableName == table.TableName);
            if (table.SyncDirection != SyncDirection.NoSync)
                QueryScalars<Guid>($"SELECT SyncGuid FROM {table.TableName}")
                    .ForEach(x => Insert(new ChangesHistory(table.TableName, x, NotifyTableChangedAction.Delete, table.SyncMode.Order)));
        }

        public void OnTableChanged(TableMapping table, NotifyTableChangedAction action, object obj)
        {
            if (table.SyncDirection == SyncDirection.NoSync || table.SyncDirection == SyncDirection.Download)
            {
                return;
            }

             if (obj is Guid guid)
            {
                UpdateVersionControl(new ChangesHistory(
                    table.TableName
                    , guid
                    , action, table.SyncMode.Order));
            }

            if (obj is null)
            {
                UpdateVersionControl(new ChangesHistory(
                    table.TableName
                    , Guid.Empty
                    , action, table.SyncMode.Order));
            }
            if (obj is ISync isync)
            {
                UpdateVersionControl(new ChangesHistory(
                    table.TableName
                    , (obj as ISync).Guid
                    , action, table.SyncMode.Order));
            }
            var ev = TableChanged;
            if (ev != null)
                ev(this, new NotifyTableChangedEventArgs(table, action));
        }

        public void UpdateVersionControl(ChangesHistory VersionControl)
        {
            Table<ChangesHistory>().Delete(x => x.Guid == VersionControl.Guid, false);
            Table<SyncHistory>().Delete(x => x.Guid == VersionControl.Guid, false);
            Insert(VersionControl);
        }
    }
}