﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Helpers;
using Kit.Sql.Readers;
using Kit.Sql.Sqlite;
using Kit.Sql.Tables;
using static Kit.Sql.Base.BaseTableQuery;

namespace Kit.Sql.SqlServer
{
    /// <summary>
    /// An open connection to a SQLite database.
    /// </summary>
    [Preserve(AllMembers = true)]
    public class SQLServerConnection : SqlBase, IDisposable
    {
        public override string MappingSuffix => "_sqlserver";

        protected override Base.TableMapping _GetMapping(Type type, CreateFlags createFlags = CreateFlags.None)
        {
            return new Kit.Sql.SqlServer.TableMapping(type, createFlags);
        }

        #region SQLH

        public List<string> GetDatabasesNames()
        {
            return Lista<string>("SELECT name FROM sys.databases;");
        }

        public string GetDbName()
        {
            return Single<string>("SELECT DB_NAME() AS [Current Database];");
        }

        public bool TriggerExists(string TriggerName)
        {
            return Exists($"SELECT * FROM sys.objects WHERE [name] = N'{TriggerName}' AND [type] = 'TR'");
        }

        public List<Tuple<string, Type>> GetColumnsName(string consulta, params SqlParameter[] Parametros)
        {
            List<Tuple<string, Type>> listacolumnas = new List<Tuple<string, Type>>();
            DataTable dataTable = new DataTable();
            if (IsClosed)
                RenewConnection();
            using (SqlConnection con = Connection)
            {
                con.Open();
                using SqlCommand cmd = new SqlCommand(consulta, con);
                cmd.Parameters.AddRange(Parametros);
                using SqlDataAdapter dataContent = new SqlDataAdapter(cmd);
                dataContent.Fill(dataTable);

                foreach (DataColumn col in dataTable.Columns)
                {
                    listacolumnas.Add(new Tuple<string, Type>(col.ColumnName, col.DataType));
                }
            }

            return listacolumnas;
        }

        internal bool ExistsClusteredIndex(string tableName, string index_name)
        {
            return Exists($"SELECT * FROM sys.indexes WHERE name = '{index_name}' AND object_id = OBJECT_ID('dbo.{tableName}')");
        }

        internal int CreateClusteredIndex(string tableName, string column_name)
        {
            return CreateClusteredIndex(tableName, column_name, $"IX_{tableName}_{column_name}");
        }

        internal int CreateClusteredIndex(string tableName, string column_name, string index_name)
        {
            return EXEC($"create NONCLUSTERED INDEX {index_name} ON {tableName}({column_name})");
        }

        public string TipoDato(string Tabla, string Campo)
        {
            return Single<string>(
                @"SELECT DATA_TYPE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TABLA AND COLUMN_NAME = @CAMPO"
                , new SqlParameter("TABLA", Tabla)
                , new SqlParameter("CAMPO", Campo)
            );
        }

        public void ChangeConnectionString(string CadenaCon)
        {
            this.ConnectionString = new SqlConnectionStringBuilder(CadenaCon);
        }

        public SqlConnection Con()
        {
            return new SqlConnection(ConnectionString.ConnectionString);
        }

        //public void Querry(string sql, CommandType type = CommandType.StoredProcedure, bool Reportar = true)
        //{
        //    if (this.IsClosed)
        //        RenewConnection();

        //    using (var con = this.Connection)
        //    {
        //        con.Open();
        //        using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
        //        {
        //            cmd.ExecuteNonQuery();
        //            if (Reportar)
        //                ReportaTransaccion(cmd);
        //        }

        //        con.Close();
        //    }
        //}

        public object Single(string sql, params SqlParameter[] parameters)
        {
            return Single(sql, CommandType.Text, parameters);
        }

        public object Single(string sql, CommandType type, params SqlParameter[] parameters)
        {
            if (IsClosed)
                RenewConnection();
            return Connection.Single(sql, type, parameters);
        }

        public override T Single<T>(string sql)
        {
            return Single<T>(sql);
        }

        public override object Single(string sql)
        {
            return Single(sql, null);
        }

        public T Single<T>(string sql, CommandType type, params SqlParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(Single(sql, type, parameters));
        }

        public T Single<T>(string sql, params SqlParameter[] parameters) where T : IConvertible
        {
            return Sqlh.Parse<T>(Single(sql, parameters));
        }

        //public T Single<T>(string sql) where T : IConvertible
        //{
        //    T result = default;
        //    using (IReader reader = Read(sql))
        //    {
        //        if (reader.Read())
        //        {
        //            if (reader[0] != DBNull.Value)
        //            {
        //                result = Sqlh.Parse<T>(reader[0]);
        //            }
        //        }
        //    }

        //    return result;
        //}

        //public T Single<T>(string sql,
        //    params SqlParameter[] parametros) where T : IConvertible
        //{
        //    T result = default;
        //    try
        //    {
        //        using (IReader reader = Read(sql, parametros))
        //        {
        //            if (reader.Read())
        //            {
        //                result = Sqlh.Parse<T>(reader[0]);
        //            }
        //        }

        //        Log.Logger.Debug(sql);

        //    }
        //    catch (Exception ex)
        //    {
        //        Log.Logger.Error(ex, "Consulta fallida");
        //        Log.Logger.Error(sql);
        //        Log.AlertOnDBConnectionError(ex);
        //    }

        //    return result;
        //}

        // [Obsolete("No se deberia utilizar mas procedimientos alamcenados debido a su dificil versionamiento", false)]
        //public T Single<T>(string sql, params SqlParameter[] parametros) where T : IConvertible
        //{
        //    T result = default;
        //    using (IReader reader = Read(sql, parametros))
        //    {
        //        if (reader.Read())
        //        {
        //            result = Sqlh.Parse<T>(reader[0]);
        //        }
        //    }

        //    return result;
        //}

        public override int EXEC(string sql)
        {
            return EXEC(sql);
        }

        public int EXEC(string sql, params SqlParameter[] parametros)
        {
            return EXEC(sql, CommandType.Text, parametros);
        }

        public int EXEC(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parametros)
        {
            LastException = null;
            int Rows = Error;
            try
            {
                if (this.IsClosed)
                    RenewConnection();
                using (var con = Con())
                {
                    con.Open();

                    try
                    {
                        using SqlCommand cmd = new SqlCommand(sql, con)
                        { CommandType = commandType };
                        if (parametros.Any(x => x.Value is null))
                        {
                            foreach (SqlParameter t in parametros)
                            {
                                if (t.Value is null)
                                {
                                    t.Value = DBNull.Value;
                                }

                                if (!parametros.Any(x => x.Value is null))
                                    break;
                            }
                        }

                        cmd.Parameters.AddRange(parametros);

                        ReportaTransaccion(cmd);
                        Rows = cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        LastException = ex;
                        Log.AlertOnDBConnectionError(LastException);
                    }
                    finally
                    {
                        con.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                if (Log.IsDBConnectionError(ex))
                {
                    Daemon.Daemon.OffLine = true;
                }
            }
            return Rows;
        }

        internal SqlServerInformation GetServerInformation()
        {
            return Con().GetServerInformation();
        }

        public int EXEC(SqlConnection connection, string procedimiento, CommandType commandType = CommandType.Text,
            params SqlParameter[] parametros)
        {
            int Rows = -1;

            if (connection is null && IsClosed)
                RenewConnection();

            using (SqlConnection con = connection ?? Connection)
            {
                con.Open();
                using SqlCommand cmd = new SqlCommand(procedimiento, con)
                { CommandType = commandType };
                if (parametros.Any(x => x.Value is null))
                {
                    foreach (SqlParameter t in parametros)
                    {
                        if (t.Value is null)
                        {
                            t.Value = DBNull.Value;
                        }

                        if (!parametros.Any(x => x.Value is null))
                            break;
                    }
                }

                cmd.Parameters.AddRange(parametros);
                try
                {
                    Rows = cmd.ExecuteNonQuery();
                }
                catch (Exception ex)
                {
                    Log.Logger.Error(ex, "Transaccion fallida reportada");
                    ReportaTransaccion(cmd);
                    if (Tools.Debugging)
                    {
                        throw;
                    }

                    LastException = ex;
                    Log.AlertOnDBConnectionError(LastException);
                    Rows = Error;
                }

                ReportaTransaccion(cmd);
                con.Close();
            }

            return Rows;
        }

        public List<T> Lista<T>(string sql, CommandType type = CommandType.Text, int index = 0,
            params SqlParameter[] parameters) where T : IConvertible
        {
            List<T> result = new List<T>();
            try
            {
                if (IsClosed)
                    RenewConnection();
                result = Connection.Lista<T>(sql, type, index, parameters);
                Log.Logger.Debug(sql);
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, sql);
            }

            return result;
        }

        public List<T> Lista<T>(string sql, int indice = 0, params SqlParameter[] parameters) where T : IConvertible
        {
            return Lista<T>(sql, CommandType.Text, indice, parameters);
        }

        public List<T> Lista<T>(string sql, params SqlParameter[] parameters) where T : IConvertible
        {
            return Lista<T>(sql, CommandType.Text, 0, parameters);
        }

        public override List<T> Lista<T>(string sql)
        {
            return Lista<T>(sql, CommandType.Text, 0);
        }

        public Tuple<T, Q> Tuple<T, Q>(string sql, CommandType type = CommandType.Text,
            params SqlParameter[] parameters)
            where T : IConvertible
            where Q : IConvertible
        {
            Tuple<T, Q> result = new Tuple<T, Q>(default(T), default(Q));
            if (IsClosed)
                RenewConnection();
            using (SqlConnection con = Connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
                {
                    cmd.Parameters.AddRange(parameters);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    if (reader.Read())
                    {
                        result = new Tuple<T, Q>(Sqlh.Parse<T>(reader[0]), Sqlh.Parse<Q>(reader[1]));
                    }
                }
                con.Close();
            }
            return result;
        }

        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, CommandType type = CommandType.Text, params SqlParameter[] parameters)
            where T : IConvertible
            where Q : IConvertible
        {
            List<Tuple<T, Q>> result = new List<Tuple<T, Q>>();
            if (IsClosed)
                RenewConnection();
            using (SqlConnection con = Connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = type })
                {
                    cmd.Parameters.AddRange(parameters);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new Tuple<T, Q>(Sqlh.Parse<T>(reader[0]), Sqlh.Parse<Q>(reader[1])));
                    }
                }
                con.Close();
            }
            return result;
        }

        public List<Tuple<object, object>> ListaTupla(string sql, params SqlParameter[] parameters)
        {
            List<Tuple<object, object>> result = new List<Tuple<object, object>>();
            if (IsClosed)
                RenewConnection();
            using (SqlConnection con = Connection)
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(sql, con) { CommandType = CommandType.Text })
                {
                    cmd.Parameters.AddRange(parameters);
                    using SqlDataReader reader = cmd.ExecuteReader();
                    while (reader.Read())
                    {
                        result.Add(new Tuple<object, object>((reader[0]), (reader[1])));
                    }
                }
                con.Close();
            }
            return result;
        }

        public List<Tuple<T, Q>> ListaTupla<T, Q>(string sql, params SqlParameter[] parameters)
            where T : IConvertible
            where Q : IConvertible
        {
            return ListaTupla<T, Q>(sql, CommandType.Text, parameters);
        }

        public DataTable DataTable(string Querry, string TableName = null, params SqlParameter[] parameters)
        {
            return DataTable(Querry, CommandType.Text, TableName, parameters);
        }

        public DataTable DataTable(string Querry, CommandType commandType = CommandType.StoredProcedure, string TableName = null, params SqlParameter[] parameters)
        {
            DataTable result = new DataTable(TableName);
            if (IsClosed)
                RenewConnection();
            using (SqlConnection con = this.Con())
            {
                con.Open();
                using (SqlCommand cmd = new SqlCommand(Querry, con) { CommandType = commandType })
                {
                    if (parameters != null)
                        cmd.Parameters.AddRange(parameters);
                    try
                    {
                        result.Load(cmd.ExecuteReader());
                    }
                    catch (Exception ex)
                    {
                        ReportaTransaccion(cmd);
                        Log.Logger.Error(ex, "");
                        throw;
                    }
                }
                con.Close();
            }
            return result;
        }

        public override IReader Read(string sql)
        {
            return Read(sql, CommandType.Text);
        }

        public IReader Read(string sql, params SqlParameter[] parameters)
        {
            return Read(sql, CommandType.Text, parameters);
        }

        public IReader Read(string sql, CommandType commandType = CommandType.Text, params SqlParameter[] parameters)
        {
            try
            {
                //con.Open();
                SqlCommand cmd = new SqlCommand(sql, Con()) { CommandType = commandType, CommandTimeout = this.CommandTimeout };
                if (parameters != null)
                {
                    cmd.Parameters.AddRange(parameters);
                }
                if (cmd.Connection.State != ConnectionState.Open)
                    cmd.Connection.Open();
                ReportaTransaccion(cmd);
                return new Reader(cmd, this);
            }
            catch (Exception ex)
            {
                Log.Logger.Error("Transaccion fallida reportada");
                Log.Logger.Error(sql);
                if (!Log.AlertOnDBConnectionError(ex) && Tools.Debugging)
                {
                    throw;
                }
                this.LastException = ex;
                return new FakeReader();
            }
        }

        public void RunBatch(string Batch, bool Reportar = false)
        {
            Batch += "\nGO";   // make sure last batch is executed.
            try
            {
                foreach (string line in Batch.Split(new string[2] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    if (line.ToUpperInvariant().Trim() == "GO")
                    {
                        if (!string.IsNullOrEmpty(Batch))
                            EXEC(Batch, CommandType.Text);
                        Batch = string.Empty;
                    }
                    else
                    {
                        Batch += line + "\n";
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex.Message);
            }
        }

        public async Task<Reader> AsyncLeector(string sql, CommandType commandType = CommandType.StoredProcedure, bool Reportar = true, params SqlParameter[] parameters)
        {
            if (IsClosed)
                RenewConnection();
            using SqlCommand cmd = new SqlCommand(sql, this.Connection) { CommandType = commandType };
            try
            {
                cmd.Parameters.AddRange(parameters);
                cmd.Connection.Open();
                if (Reportar)
                    ReportaTransaccion(cmd);
                return await new Reader().AsyncReader(cmd);
            }
            catch (Exception)
            {
                if (Tools.Debugging)
                {
                    throw;
                }
                Log.Logger.Error("Transaccion fallida reportada");
                Log.Logger.Error(GetCommandText(cmd));
                return null;
            }
        }

        public Task<Reader> AsyncLeector(string sql, params SqlParameter[] parameters)
        {
            return AsyncLeector(sql, CommandType.Text, false, parameters);
        }

        public override bool Exists(string sql, params Condition[] parametros)
        {
            return Exists(sql, GetParameters(parametros));
        }

        public bool Exists(string sql, params SqlParameter[] parametros)
        {
            bool result = false;
            using (IReader reader = Read(sql, CommandType.Text, parametros))
            {
                if (reader is FakeReader)
                {
                    throw LastException ?? new Exception("Reader no read");
                }
                if (reader != null)
                {
                    result = reader.Read();
                }
            }
            return result;
        }

        public bool ExisteCampo(string Tabla, string Campo)
        {
            return Exists($@"SELECT 1 FROM sys.columns WHERE name = N'{Campo}' AND Object_ID = Object_ID(N'{Tabla}')");
        }

        public bool TieneIdentidad(string Tabla)
        {
            return Exists("SELECT * from syscolumns where id = Object_ID(@TABLE_NAME) and colstat & 1 = 1",
                new SqlParameter("TABLE_NAME", Tabla));
        }

        public void ReportaTransaccion(SqlCommand cmd)
        {
            string sql = GetCommandText(cmd);
            Log.Logger?.Debug("Executing:[{0}]", sql);
        }

        private string GetCommandText(SqlCommand sqc)
        {
            StringBuilder sbCommandText = new StringBuilder();
            sbCommandText.AppendLine("GO");
            //sbCommandText.AppendLine("-- INICIA");

            // params
            for (int i = 0; i < sqc.Parameters.Count; i++)
                LogParameterToSqlBatch(sqc.Parameters[i], sbCommandText);
            //sbCommandText.AppendLine("-- END PARAMS");

            // command
            if (sqc.CommandType == CommandType.StoredProcedure)
            {
                sbCommandText.Append("EXEC ");

                bool hasReturnValue = false;
                for (int i = 0; i < sqc.Parameters.Count; i++)
                {
                    if (sqc.Parameters[i].Direction == ParameterDirection.ReturnValue)
                        hasReturnValue = true;
                }
                if (hasReturnValue)
                {
                    sbCommandText.Append("@returnValue = ");
                }

                sbCommandText.Append(sqc.CommandText + (sqc.Parameters.Count > 0 ? " " : ""));

                bool hasPrev = false;
                for (int i = 0; i < sqc.Parameters.Count; i++)
                {
                    SqlParameter cParam = sqc.Parameters[i];
                    if (cParam.Direction != ParameterDirection.ReturnValue)
                    {
                        if (hasPrev)
                            sbCommandText.Append(", ");

                        sbCommandText.Append("@" + cParam.ParameterName);
                        sbCommandText.Append(" = ");
                        sbCommandText.Append("@" + cParam.ParameterName);

                        if (cParam.Direction.HasFlag(ParameterDirection.Output))
                            sbCommandText.Append(" OUTPUT");

                        hasPrev = true;
                    }
                }
            }
            else
            {
                sbCommandText.Append(sqc.CommandText);
            }

            //sbCommandText.AppendLine("-- RESULTS");
            //sbCommandText.Append("SELECT 1 as Executed");
            for (int i = 0; i < sqc.Parameters.Count; i++)
            {
                SqlParameter cParam = sqc.Parameters[i];

                if (cParam.Direction == ParameterDirection.ReturnValue)
                {
                    sbCommandText.AppendLine(", @returnValue as ReturnValue");
                }
                else if (cParam.Direction.HasFlag(ParameterDirection.Output))
                {
                    sbCommandText.AppendLine(", ");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(" as [");
                    sbCommandText.Append(cParam.ParameterName);
                    sbCommandText.Append(']');
                }
            }
            sbCommandText.Append(";");

            //sbCommandText.AppendLine("-- END COMMAND");
            return sbCommandText.ToString();
        }

        private void LogParameterToSqlBatch(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.Append("DECLARE ");
            if (param.Direction == ParameterDirection.ReturnValue)
            {
                sbCommandText.AppendLine("@returnValue INT;");
            }
            else
            {
                sbCommandText.Append("@" + param.ParameterName);

                sbCommandText.Append(' ');
                try
                {
                    if (param.SqlDbType != SqlDbType.Structured)
                    {
                        LogParameterType(param, sbCommandText);
                        sbCommandText.Append(" = ");
                        LogQuotedParameterValue(param.Value, sbCommandText);

                        sbCommandText.AppendLine(";");
                    }
                    else
                    {
                        LogStructuredParameter(param, sbCommandText);
                    }
                }
                catch (Exception)
                {
                    sbCommandText.AppendLine($" sql_variant ={param.Value};");
                }
            }
        }

        private void LogStructuredParameter(SqlParameter param, StringBuilder sbCommandText)
        {
            sbCommandText.AppendLine(" {List Type};");
            DataTable dataTable = (DataTable)param.Value;

            for (int rowNo = 0; rowNo < dataTable.Rows.Count; rowNo++)
            {
                sbCommandText.Append("INSERT INTO ");
                sbCommandText.Append(param.ParameterName);
                sbCommandText.Append(" VALUES (");

                bool hasPrev = false;
                for (int colNo = 0; colNo < dataTable.Columns.Count; colNo++)
                {
                    if (hasPrev)
                    {
                        sbCommandText.Append(", ");
                    }
                    LogQuotedParameterValue(dataTable.Rows[rowNo].ItemArray[colNo], sbCommandText);
                    hasPrev = true;
                }
                sbCommandText.AppendLine(");");
            }
        }

        private const string DATETIME_FORMAT_ROUNDTRIP = "o";

        private void LogQuotedParameterValue(object value, StringBuilder sbCommandText)
        {
            try
            {
                if (value == null)
                {
                    sbCommandText.Append("NULL");
                }
                else
                {
                    value = UnboxNullable(value);

                    if (value is string
                        || value is char
                        || value is char[]
                        || value is System.Xml.Linq.XElement
                        || value is System.Xml.Linq.XDocument)
                    {
                        sbCommandText.Append("N'");
                        sbCommandText.Append(value.ToString().Replace("'", "''"));
                        sbCommandText.Append('\'');
                    }
                    else if (value is bool)
                    {
                        // True -> 1, False -> 0
                        sbCommandText.Append(Convert.ToInt32(value));
                    }
                    else if (value is sbyte
                        || value is byte
                        || value is short
                        || value is ushort
                        || value is int
                        || value is uint
                        || value is long
                        || value is ulong
                        || value is float
                        || value is double
                        || value is decimal)
                    {
                        sbCommandText.Append(value.ToString());
                    }
                    else if (value is DateTime time)
                    {
                        // SQL Server only supports ISO8601 with 3 digit precision on datetime,
                        // datetime2 (>= SQL Server 2008) parses the .net format, and will
                        // implicitly cast down to datetime.
                        // Alternatively, use the format string "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffK"
                        // to match SQL server parsing
                        sbCommandText.Append("CAST('");
                        sbCommandText.Append(time.ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append("' as datetime2)");
                    }
                    else if (value is DateTimeOffset offset)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(offset.ToString(DATETIME_FORMAT_ROUNDTRIP));
                        sbCommandText.Append('\'');
                    }
                    else if (value is Guid guid)
                    {
                        sbCommandText.Append('\'');
                        sbCommandText.Append(guid.ToString());
                        sbCommandText.Append('\'');
                    }
                    else if (value is byte[] data)
                    {
                        sbCommandText.Append("BYNARY DATA");
                        //if (data.Length == 0)
                        //{
                        //    sbCommandText.Append("NULL");
                        //}
                        //else
                        //{
                        //    sbCommandText.Append("0x");
                        //    foreach (byte t in data)
                        //    {
                        //        sbCommandText.Append(t.ToString("h2"));
                        //    }
                        //}
                    }
                    else if (value == DBNull.Value)
                    {
                        sbCommandText.Append("NULL");
                    }
                    else
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(value.GetType().ToString());
                        sbCommandText.Append(" *" + "/ N'");
                        sbCommandText.Append(value.ToString());
                        sbCommandText.Append('\'');
                    }
                }
            }
            catch (Exception ex)
            {
                sbCommandText.AppendLine("/* Exception occurred while converting parameter: ");
                sbCommandText.AppendLine(ex.ToString());
                sbCommandText.AppendLine("*/");
            }
        }

        private object UnboxNullable(object value)
        {
            Type typeOriginal = value.GetType();
            if (typeOriginal.IsGenericType
                && typeOriginal.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                // generic value, unboxing needed
                return typeOriginal.InvokeMember("GetValueOrDefault",
                    System.Reflection.BindingFlags.Public |
                    System.Reflection.BindingFlags.Instance |
                    System.Reflection.BindingFlags.InvokeMethod,
                    null, value, null);
            }
            else
            {
                return value;
            }
        }

        private void LogParameterType(SqlParameter param, StringBuilder sbCommandText)
        {
            switch (param.SqlDbType)
            {
                // variable length
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.Binary:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append('(');
                        sbCommandText.Append(param.Size);
                        sbCommandText.Append(')');
                    }
                    break;

                case SqlDbType.VarChar:
                case SqlDbType.NVarChar:
                case SqlDbType.VarBinary:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append("(MAX");
                        //sbCommandText.Append("/* Specified as ");
                        //sbCommandText.Append(param.Size);
                        //sbCommandText.Append(" */");
                        sbCommandText.Append(")");
                    }
                    break;
                // fixed length
                case SqlDbType.Text:
                case SqlDbType.NText:
                case SqlDbType.Bit:
                case SqlDbType.TinyInt:
                case SqlDbType.SmallInt:
                case SqlDbType.Int:
                case SqlDbType.BigInt:
                case SqlDbType.SmallMoney:
                case SqlDbType.Money:
                case SqlDbType.Decimal:
                case SqlDbType.Real:
                case SqlDbType.Float:
                case SqlDbType.Date:
                case SqlDbType.DateTime:
                case SqlDbType.DateTime2:
                case SqlDbType.DateTimeOffset:
                case SqlDbType.UniqueIdentifier:
                case SqlDbType.Image:
                    {
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
                // Unknown
                case SqlDbType.SmallDateTime:
                    break;

                case SqlDbType.Variant:
                    break;

                case SqlDbType.Xml:
                    break;

                case SqlDbType.Udt:
                    break;

                case SqlDbType.Structured:
                    break;

                case SqlDbType.Time:
                    break;

                case SqlDbType.Timestamp:
                default:
                    {
                        sbCommandText.Append("/* UNKNOWN DATATYPE: ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                        sbCommandText.Append(" *" + "/ ");
                        sbCommandText.Append(param.SqlDbType.ToString().ToUpper());
                    }
                    break;
            }
        }

        public void CrearCampo(string Tabla, string Campo, string TipoDato, bool Nulleable)
        {
            Execute($"alter table [{Tabla}] add [{Campo}] {TipoDato} {(Nulleable ? "" : "NOT NULL")}");
        }

        #endregion SQLH

        private System.Diagnostics.Stopwatch _sw;
        private long _elapsedMilliseconds = 0;

        private int _transactionDepth = 0;
        private Random _rand = new Random();

        /// <summary>
        /// Gets or sets the database path used by this connection.
        /// </summary>
        public new SqlConnectionStringBuilder ConnectionString { get; set; }

        /// <summary>
        /// Whether Trace lines should be written that show the execution time of queries.
        /// </summary>
        public bool TimeExecution { get; set; }

        /// <summary>
        /// Whether to write queries to <see cref="Tracer"/> during execution.
        /// </summary>
        public bool Trace { get; set; }

        /// <summary>
        /// The delegate responsible for writing trace lines.
        /// </summary>
        /// <value>The tracer.</value>
        public Action<string> Tracer { get; set; }

        /// <summary>
        ///// Whether to store DateTime properties as ticks (true) or strings (false).
        ///// </summary>
        //public bool StoreDateTimeAsTicks { get; private set; }

        ///// <summary>
        ///// Whether to store TimeSpan properties as ticks (true) or strings (false).
        ///// </summary>
        //public bool StoreTimeSpanAsTicks { get; private set; }

        ///// <summary>
        ///// The format to use when storing DateTime properties as strings. Ignored if StoreDateTimeAsTicks is true.
        ///// </summary>
        ///// <value>The date time string format.</value>
        //public string DateTimeStringFormat { get; private set; }

        ///// <summary>
        ///// The DateTimeStyles value to use when parsing a DateTime property string.
        ///// </summary>
        ///// <value>The date time style.</value>
        //internal System.Globalization.DateTimeStyles DateTimeStyle { get; private set; }

        public SqlConnection Connection { get; private set; }

        public int CommandTimeout { get; set; } = 30;
        public string DataBaseName { get; private set; }
        public string Server { get; private set; }
        public string Port { get; private set; }
        public string User { get; private set; }
        public string Password { get; private set; }

        public SQLServerConnection(string DataBaseName, string Server, string Port = null, string User = null, string Password = null)
            : this(BuildConnectionString(DataBaseName, Server, Port, User, Password))
        {
            this.DataBaseName = DataBaseName;
            this.Server = Server;
            this.Port = Port;
            this.User = User;
            this.Password = Password;
        }

        public override SqlBase CheckTables(int DBVersion, params Type[] Tables)
        {
            CreateTable<SyncVersions>();
            CreateTable<ChangesHistory>();
            CreateTable<SyncHistory>();
            CreateTable<SyncDevicesInfo>();
            CreateTable<DeviceInformation>();
            foreach (Type table in Tables)
            {
                var map = this.GetMapping(table);
                SyncVersions version = SyncVersions.GetVersion(this, map);
                if (version.Version != DBVersion)
                {
                    this.CreateTable(map);
                    version.Version = DBVersion;
                    version.Save(this);
                }
            }
            return this;
        }

        private static SqlConnectionStringBuilder BuildConnectionString
            (string DataBaseName, string Server, string Port = null, string User = null, string Password = null)
        {
            StringBuilder ConnectionString = new StringBuilder();
            ConnectionString.Append("Data Source=TCP:")
                .Append(Server)
                .Append((!string.IsNullOrEmpty(Port?.Trim()) ? "," + Port : ""))//no puerto no lo pongo
                .Append(";Initial Catalog=")
                .Append(DataBaseName);
            if (string.IsNullOrEmpty(User?.Trim()) && string.IsNullOrEmpty(Password?.Trim()))//no user provided,default authentication
            {
                ConnectionString.Append(";Integrated Security=True;");
            }
            else
            {
                ConnectionString.Append(";Integrated Security=False;Persist Security Info=True;User ID=")
                    .Append(User)
                    .Append(";Password=")
                    .Append(Password).Append(";");
            }

            string[] args = ConnectionString.
                Replace(Environment.NewLine, "").
                Replace('\n', ' ').
                Replace('\r', ' ').ToString().
                Split(';');

            return new SqlConnectionStringBuilder(string.Join(";" + Environment.NewLine, args));
        }

        /// <summary>
        /// Constructs a new SQLiteConnection and opens a SQLite database specified by databasePath.
        /// </summary>
        /// <param name="connectionString">
        /// Details on how to find and open the database.
        /// </param>
        public SQLServerConnection(string connectionString) : this(new SqlConnectionStringBuilder(connectionString))
        {
        }

        public SQLServerConnection(SqlConnectionStringBuilder connectionString)
        {
            if (connectionString == null)
                throw new ArgumentNullException(nameof(connectionString));
            if (connectionString.ConnectionString == null)
                throw new InvalidOperationException("ConnectionString must be specified");
            ConnectionString = connectionString;
            Tracer = line => Debug.WriteLine(line);
            Connection = new SqlConnection(ConnectionString.ConnectionString);
        }

        public override SqlBase RenewConnection()
        {
            if (IsOpen)
            {
                Connection.Close();
            }
            Connection?.Dispose();
            Connection = new SqlConnection(ConnectionString.ConnectionString);
            return this;
        }

        public void ChangeCatalog(string newcatalog)
        {
            this.ConnectionString.InitialCatalog = newcatalog;
            this.RenewConnection();
        }

        public static Exception TestConnection(SqlConnection sql)
        {
            try
            {
                using (SqlConnection con = sql)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", con) { CommandType = CommandType.Text })
                    {
                        cmd.ExecuteScalar();
                    }
                    con.Close();
                }
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        public Exception TestConnection(string sqlConnection)
        {
            try
            {
                if (IsClosed)
                    RenewConnection();
                using (SqlConnection con = new SqlConnection(sqlConnection))
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", con) { CommandType = CommandType.Text })
                    {
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        public Exception TestConnection()
        {
            try
            {
                if (IsClosed)
                    RenewConnection();
                using (SqlConnection con = Connection)
                {
                    con.Open();
                    using (SqlCommand cmd = new SqlCommand("SELECT 1", con) { CommandType = CommandType.Text })
                    {
                        cmd.ExecuteScalar();
                    }
                }
            }
            catch (Exception e)
            {
                return e;
            }
            return null;
        }

        public void SetCacheIdentity(bool enabled)
        {
            EXEC($"ALTER DATABASE SCOPED CONFIGURATION SET IDENTITY_CACHE={(enabled ? "ON" : "OFF")};");
        }

        public override bool ViewExists(string viewName)
        {
            return Exists($"SELECT name FROM sys.views WHERE name = @ViewName",
            new SqlParameter("ViewName", viewName));
        }

        public override bool TableExists(string tablename)
        {
            return Exists($"SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = @Tablename",
                new SqlParameter("Tablename", tablename));
        }

        /// <summary>
        /// Convert an input string to a quoted SQL string that can be safely used in queries.
        /// </summary>
        /// <returns>The quoted string.</returns>
        /// <param name="unsafeString">The unsafe string to quote.</param>
        private static string Quote(string unsafeString)
        {
            if (unsafeString == null)
                return "NULL";
            var safe = unsafeString.Replace("'", "''");
            return "'" + safe + "'";
        }

        /// <summary>
        /// Sets a busy handler to sleep the specified amount of time when a table is locked.
        /// The handler will sleep multiple times until a total time of <see cref="BusyTimeout"/> has accumulated.
        /// </summary>
        //public TimeSpan BusyTimeout {
        //	get { return _busyTimeout; }
        //	set {
        //		_busyTimeout = value;
        //		if (Handle != NullHandle) {
        //			SQLite3.BusyTimeout (Handle, (int)_busyTimeout.TotalMilliseconds);
        //		}
        //	}
        //}

        //internal struct IndexedColumn
        //{
        //	public int Order;
        //	public string ColumnName;
        //}

        //internal struct IndexInfo
        //{
        //	public string IndexName;
        //	public string TableName;
        //	public bool Unique;
        //	public List<IndexedColumn> Columns;
        //}

        /// <summary>
        /// Executes a "drop table" on the database.  This is non-recoverable.
        /// </summary>
        public int DropTable<T>()
        {
            return DropTable(GetMapping(typeof(T)));
        }

        /// <summary>
        /// Executes a "drop table" on the database.  This is non-recoverable.
        /// </summary>
        /// <param name="map">
        /// The TableMapping used to identify the table.
        /// </param>
        public int DropTable(Base.TableMapping map)
        {
            if (TableExists(map.TableName))
            {
                var query = string.Format("drop table \"{0}\"", map.TableName);
                return Execute(query);
            }

            return 0;
        }

        public SQLServerConnection CreateDatabase()
        {
            try
            {
                if (!DatabaseExists(this.DataBaseName))
                {
                    this.ChangeCatalog("master");
                    Execute($"CREATE DATABASE {this.DataBaseName}");
                    this.ChangeCatalog(this.DataBaseName);
                }
            }
            catch (Exception e)
            {
                throw SqlServerException.New(SQLite3.Result.CannotOpen, e.Message);
            }

            return this;
        }

        private bool DatabaseExists(string DbName)
        {
            ChangeCatalog("master");
            bool exists = Exists("SELECT 1 WHERE  DB_ID(@DbName) IS NOT NULL", new SqlParameter("DbName", DbName));
            ChangeCatalog(DbName);
            return exists;
        }

        //public bool Exists(string sql, params SqlParameter[] parameters)
        //{
        //    bool result = false;
        //    using (var reader = Read(sql, parameters))
        //    {
        //        if (reader != null)
        //        {
        //            result = reader.Read();
        //        }
        //    }
        //    return result;
        //}
        //private SqlDataReader Read(string sql, params SqlParameter[] parameters)
        //{
        //    try
        //    {
        //        SqlCommand cmd = new SqlCommand(sql, Connection) { CommandType = CommandType.Text };
        //        if (parameters != null)
        //        {
        //            cmd.Parameters.AddRange(parameters);
        //        }
        //        cmd.Connection.Open();
        //        return cmd.ExecuteReader();
        //    }
        //    catch (Exception ex)
        //    {
        //        throw ex;
        //    }
        //}

        /// <summary>
        /// Executes a "create table if not exists" on the database. It also
        /// creates any specified indexes on the columns of the table. It uses
        /// a schema automatically generated from the specified type. You can
        /// later access this schema by calling GetMapping.
        /// </summary>
        /// <returns>
        /// Whether the table was created or migrated.
        /// </returns>
        public CreateTableResult CreateTable<T>()
        {
            return CreateTable(typeof(T));
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
        public override CreateTableResult CreateTable(Base.TableMapping map, CreateFlags createFlags = CreateFlags.None)
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

                // Build query.
                var query = "create table \"" + map.TableName + "\" (\n";
                var decls = map.Columns.Select(p => Orm.SqlDecl(p));
                var decl = string.Join(",\n", decls.ToArray());
                query += decl;
                query += ")";
                if (map.WithoutRowId)
                {
                    query += " without rowid";
                }
                Execute(query);
            }
            else
            {
                result = CreateTableResult.Migrated;
                MigrateTable(map, existingCols);
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
        public CreateTablesResult CreateTables<T, T2>()
            where T : new()
            where T2 : new()
        {
            return CreateTables(typeof(T), typeof(T2));
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
        public CreateTablesResult CreateTables<T, T2, T3>()
            where T : new()
            where T2 : new()
            where T3 : new()
        {
            return CreateTables(typeof(T), typeof(T2), typeof(T3));
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
        public CreateTablesResult CreateTables<T, T2, T3, T4>()
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
        {
            return CreateTables(typeof(T), typeof(T2), typeof(T3), typeof(T4));
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
        public CreateTablesResult CreateTables<T, T2, T3, T4, T5>()
            where T : new()
            where T2 : new()
            where T3 : new()
            where T4 : new()
            where T5 : new()
        {
            return CreateTables(typeof(T), typeof(T2), typeof(T3), typeof(T4), typeof(T5));
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
        public CreateTablesResult CreateTables(params Type[] types)
        {
            var result = new CreateTablesResult();
            foreach (Type type in types)
            {
                var aResult = CreateTable(type);
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

        [Preserve(AllMembers = true)]
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
            var query = @"SELECT
		c.ORDINAL_POSITION as cid,
	c.COLUMN_NAME as name,
	c.DATA_TYPE as type,
	IIF(c.IS_NULLABLE='YES',0,1) AS notnull,
	c.COLUMN_DEFAULT as dflt_value,
    CASE WHEN pk.COLUMN_NAME IS NOT NULL THEN 1 ELSE 0 END AS pk
FROM INFORMATION_SCHEMA.COLUMNS c
LEFT JOIN (
            SELECT ku.TABLE_CATALOG,ku.TABLE_SCHEMA,ku.TABLE_NAME,ku.COLUMN_NAME
            FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS AS tc
            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE AS ku
                ON tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                AND tc.CONSTRAINT_NAME = ku.CONSTRAINT_NAME
         )   pk
ON  c.TABLE_CATALOG = pk.TABLE_CATALOG
            AND c.TABLE_SCHEMA = pk.TABLE_SCHEMA
            AND c.TABLE_NAME = pk.TABLE_NAME
            AND c.COLUMN_NAME = pk.COLUMN_NAME
WHERE
	c.TABLE_NAME = @tablename
	ORDER BY c.TABLE_SCHEMA,c.TABLE_NAME, c.ORDINAL_POSITION ";
            return Query<ColumnInfo>(query, new SqlParameter("tablename", tableName));
        }

        private void MigrateTable(Base.TableMapping map, List<ColumnInfo> existingCols)
        {
            var toBeAdded = new List<Base.TableMapping.Column>();

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

            foreach (var p in toBeAdded)
            {
                string addCol;
                if (p.ColumnType == typeof(Guid))
                {
                    addCol = "alter table \"" + map.TableName + "\" add " + Orm.SqlDecl(p);
                    Execute(addCol);
                    continue;
                }

                addCol = "alter table \"" + map.TableName + "\" add " + Orm.SqlDecl(p) + (p.IsNullable ? " DEFAULT NULL" : "");
                Execute(addCol);
            }
        }

        /// <summary>
        /// Creates a new SQLiteCommand. Can be overridden to provide a sub-class.
        /// </summary>
        /// <seealso cref="SQLiteCommand.OnInstanceCreated"/>
        protected virtual SQLServerCommand NewCommand(string CommandText, params SqlParameter[] parameters)
        {
            if (this.IsClosed)
                this.RenewConnection();
            return new SQLServerCommand(this, CommandText, parameters);
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
            SqlParameter[] parameters = new SqlParameter[ps.Length];
            if (ps.Length > 0)
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    Condition condition = (Condition)(ps[i]);
                    if (condition.Value is null)
                    {
                        condition.SetValue(DBNull.Value);
                        parameters[i] = new SqlParameter(condition.ColumnName, DBNull.Value);
                        continue;
                    }
                    parameters[i] = new SqlParameter(condition.ColumnName, condition.Value);
                }
            }
            return CreateCommand(cmdText, parameters);
        }

        public SQLServerCommand CreateCommand(string cmdText, params SqlParameter[] ps)
        {
            return NewCommand(cmdText, ps);
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
        public SQLServerCommand CreateCommand(string cmdText, Dictionary<string, object> args)
        {
            if (!IsOpen)
                throw SqlServerException.New(SQLite3.Result.Error, "Cannot create commands from unopened database");

            var cmd = NewCommand(cmdText);
            foreach (var kv in args)
            {
                cmd.Parameters.Add(new SqlParameter(kv.Key, kv.Value));
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
        public int Execute(string query, params SqlParameter[] args)
        {
            var cmd = CreateCommand(query, args);

            if (TimeExecution)
            {
                if (_sw == null)
                {
                    _sw = new Stopwatch();
                }
                _sw.Reset();
                _sw.Start();
            }
            Log.Logger.Debug("Executing:[{0}]", cmd.CommandText);
            var r = cmd.ExecuteNonQuery();

            if (TimeExecution)
            {
                _sw.Stop();
                _elapsedMilliseconds += _sw.ElapsedMilliseconds;
                Tracer?.Invoke(string.Format("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
            }

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
        public T ExecuteScalar<T>(string query, params SqlParameter[] args)
        {
            var cmd = CreateCommand(query, args);

            if (TimeExecution)
            {
                if (_sw == null)
                {
                    _sw = new Stopwatch();
                }
                _sw.Reset();
                _sw.Start();
            }

            var r = cmd.ExecuteScalar<T>();

            if (TimeExecution)
            {
                _sw.Stop();
                _elapsedMilliseconds += _sw.ElapsedMilliseconds;
                Tracer?.Invoke(string.Format("Finished in {0} ms ({1:0.0} s total)", _sw.ElapsedMilliseconds, _elapsedMilliseconds / 1000.0));
            }

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
        public List<T> Query<T>(string query, params SqlParameter[] args) where T : new()
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
        public List<T> QueryScalars<T>(string query, params SqlParameter[] args)
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
        public IEnumerable<T> DeferredQuery<T>(string query, params SqlParameter[] args) where T : new()
        {
            var cmd = CreateCommand(query, args);
            return cmd.ExecuteDeferredQuery<T>();
        }

        public override IEnumerable<T> DeferredQuery<T>(string query, params object[] args)
        {
            var parameters = args?.Select(x => (SqlParameter)x);
            return DeferredQuery<T>(query, parameters?.ToArray());
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
        /// A <see cref="Base.TableMapping"/> to use to convert the resulting rows
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
        public List<object> Query(Base.TableMapping map, string query, params SqlParameter[] args)
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
        /// A <see cref="Base.TableMapping"/> to use to convert the resulting rows
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
        public IEnumerable<object> DeferredQuery(Base.TableMapping map, string query, params SqlParameter[] args)
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
            return new SQLServerTableQuery<T>(this);
        }

        public override BaseTableQuery Table(Type Type)
        {
            var queryType = typeof(SQLServerTableQuery<>);
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
            return Query<T>(map.GetByPrimaryKeySql, new SqlParameter(map.PK.Name, pk)).First();
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
        public object Get(object pk, Base.TableMapping map)
        {
            return Query(map, map.GetByPrimaryKeySql, new SqlParameter(map.PK.Name, pk)).First();
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
            return Query<T>(map.GetByPrimaryKeySql, new SqlParameter(map.PK.Name, pk))
                .FirstOrDefault();
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
        public object Find(object pk, Base.TableMapping map)
        {
            return Query(map, map.GetByPrimaryKeySql,
                new SqlParameter(map.PK.Name, pk)).FirstOrDefault();
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
        public T FindWithQuery<T>(string query, params SqlParameter[] args) where T : new()
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
        public object FindWithQuery(Base.TableMapping map, string query, params SqlParameter[] args)
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
        /// Whether <see cref="SQLServerConnection"/> has been disposed and the database is closed.
        /// </summary>
        public override bool IsClosed
        {
            get
            {
                bool closed = (Connection.State == ConnectionState.Closed || Connection.State == ConnectionState.Broken);
                return closed;
            }
        }

        public bool IsOpen => !IsClosed;

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
                    var sqlExp = ex as SqlServerException;
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
                var sqlExp = ex as SqlServerException;
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
            catch (SqlServerException)
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
            catch (SqlServerException ex)
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
        public int Insert(object obj)
        {
            if (obj == null)
            {
                return 0;
            }
            return Insert(obj, "", Orm.GetType(obj));
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

            var map = GetMapping(objType);
            return Insert(obj, extra, map, shouldnotify);
        }

        public override int Insert(object obj, string extra, Base.TableMapping map, bool shouldnotify = true)
        {
            if (map.SyncDirection != SyncDirection.NoSync)
            {
            }

            if (map.PK != null && map.PK.IsAutomatic)
            {
                if (map.PK.GetValue(obj).Equals(Guid.Empty))
                {
                    map.PK.SetValue(obj, Guid.NewGuid());
                }
            }

            //if (map.SyncGuid is TableMapping.GuidColumn SyncGuid && SyncGuid.GetValue() == Guid.Empty)
            //{
            //    SyncGuid.SetValue(null, null);
            //}

            //if (map.SyncGuid is TableMapping.GuidColumn SyncGuid)
            //{
            //    SyncGuid.SetValue(null, null);
            //}

            var replacing = string.Compare(extra, "OR REPLACE", StringComparison.OrdinalIgnoreCase) == 0;
            if (replacing)
            {
                Delete(obj);
                extra = string.Empty;
            }

            var cols = replacing ? map.InsertOrReplaceColumns : map.InsertColumns;
            var vals = new object[cols.Length];
            SqlParameter[] parameters = new SqlParameter[vals.Length];
            for (var i = 0; i < vals.Length; i++)
            {
                //if (cols[i] is TableMapping.GuidColumn)
                //{
                //    cols[i] = map.SyncGuid;
                //}
                vals[i] = cols[i].GetValue(obj);
                if (vals[i] is null)
                {
                    vals[i] = DBNull.Value;
                }
                parameters[i] = new SqlParameter(cols[i].Name, vals[i]);
            }

            var insertCmd = GetInsertCommand(map, extra);
            int count;

            lock (insertCmd)
            {
                // We lock here to protect the prepared statement returned via GetInsertCommand.
                // A SQLite prepared statement can be bound for only one operation at a time.
                try
                {
                    if (map.HasAutoIncPK)
                    {
                        count = 0;
                        long pk = insertCmd.ExecuteNonQueryAndRecoverLastScopeIdentity(parameters);
                        if (pk > 0)
                            count = 1;
                        map.SetAutoIncPK(obj, pk);
                    }
                    else
                    {
                        count = insertCmd.ExecuteNonQuery(parameters);
                    }
                }
                catch (SqlServerException)
                {
                    //if (SQLite3.ExtendedErrCode (this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
                    //	throw NotNullConstraintViolationException.New (ex.Result, ex.Message, map, obj);
                    //}
                    throw;
                }
            }
            if (!shouldnotify && obj is ISync isync)
            {
                Table<ChangesHistory>().Delete(x => x.Guid == isync.Guid);
            }
            return count;
        }

        private readonly Dictionary<Tuple<string, string>, PreparedSqlServerInsertCommand> _insertCommandMap = new Dictionary<Tuple<string, string>, PreparedSqlServerInsertCommand>();

        private PreparedSqlServerInsertCommand GetInsertCommand(Base.TableMapping map, string extra)
        {
            PreparedSqlServerInsertCommand prepCmd;

            var key = System.Tuple.Create(map.MappedType.FullName, extra);

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

        private PreparedSqlServerInsertCommand CreateInsertCommand(Base.TableMapping map, string extra)
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

                insertSql = string.Format("insert {3} into \"{0}\"({1}) values ({2})"
                    , map.TableName
                    , string.Join(",", (from c in cols
                                        select "\"" + c.Name + "\"").ToArray())
                    , string.Join(",", (from c in cols
                                        select "@" + c.Name).ToArray()), extra);
            }

            var insertCommand = new PreparedSqlServerInsertCommand(this, insertSql);
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

            object[] conditions_array = (this is SQLServerConnection ? conditions.ToArray() : args.ToArray());

            var Table = this.Table(obj.GetType()).Table;

            var cols = from p in Table.Columns
                       where p.Name != nameof(ISync.Guid) && !p.IsPK && !p.IsAutomatic
                       select p;
            var vals = from c in cols
                       where c.Name != nameof(ISync.Guid) && !c.IsPK && !c.IsAutomatic
                       select c.GetValue(obj);
            var ps = new List<SqlParameter>(vals.Count());
            for (int i = 0; i < vals.Count(); i++)
            {
                object val = vals.ElementAt(i);
                ps.Add(new SqlParameter(cols.ElementAt(i).Name, val ?? DBNull.Value));
            }

            ps.AddRange(conditions.Select(x => new SqlParameter(x.ColumnName, x.Value)));
            var q = string.Format("update \"{0}\" set {1} WHERE {2}",
                Table.TableName, string.Join(",", (from c in cols
                                                   select "\"" + c.Name + "\" =@" + c.Name).ToArray()), Condition);
            ///
            int rowsAffected = 0;
            try
            {
                rowsAffected = Execute(q, ps.DistinctBy(x => x.ParameterName).ToArray());
            }
            catch (SqlServerException)
            {
                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?", map.PK.GetValue(obj)));
                OnTableChanged((Kit.Sql.SqlServer.TableMapping)Table, NotifyTableChangedAction.Update);
            }

            return rowsAffected;
        }

        private SqlParameter[] GetParameters(Condition[] ps)
        {
            SqlParameter[] parameters = new SqlParameter[ps.Length];
            if (ps.Length > 0)
            {
                for (int i = 0; i < ps.Length; i++)
                {
                    Condition condition = (Condition)(ps[i]);
                    if (condition.Value is null)
                    {
                        condition.SetValue(DBNull.Value);
                        parameters[i] = new SqlParameter(condition.ColumnName, DBNull.Value);
                        continue;
                    }
                    parameters[i] = new SqlParameter(condition.ColumnName, condition.Value);
                }
            }
            return parameters;
        }

        private SqlParameter[] GetParameters(List<object> ps)
        {
            SqlParameter[] parameters = new SqlParameter[ps.Count];
            if (ps.Count > 0)
            {
                for (int i = 0; i < ps.Count; i++)
                {
                    Condition condition = (Condition)(ps[i]);
                    if (condition.Value is null)
                    {
                        condition.SetValue(DBNull.Value);
                        parameters[i] = new SqlParameter(condition.ColumnName, DBNull.Value);
                        continue;
                    }
                    parameters[i] = new SqlParameter(condition.ColumnName, condition.Value);
                }
            }
            return parameters;
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

            if (pk == null)
            {
                throw new NotSupportedException("Cannot update " + map.TableName + ": it has no PK");
            }

            var cols = (from p in map.Columns
                        where p != pk
                        select p);
            var vals = (from c in cols
                        select c.GetValue(obj));

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
            ps.Add(pk.GetValue(obj));

            List<SqlParameter> parmeters = new List<SqlParameter>(ps.Count);
            for (var i = 0; i < cols.Count(); i++)
            {
                parmeters.Add(new SqlParameter(cols.ElementAt(i).Name, ps[i]));
            }
            parmeters.Add(new SqlParameter(pk.Name, pk.GetValue(obj)));

            var q = string.Format("update \"{0}\" set {1} where {2} = @{2} ", map.TableName
                , string.Join(",", (from c in cols select "\"" + c.Name + "\" = @" + c.Name + " ").ToArray()), pk.Name);

            try
            {
                rowsAffected = Execute(q, parmeters.ToArray());
            }
            catch (SqlServerException)
            {
                //if (ex.Result == SQLite3.Result.Constraint && SQLite3.ExtendedErrCode (this.Handle) == SQLite3.ExtendedResult.ConstraintNotNull) {
                //	throw NotNullConstraintViolationException.New (ex, map, obj);
                //}

                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=@{map.PK.Name}",
                //    new SqlParameter(map.PK.Name, map.PK.GetValue(obj))));
                OnTableChanged(map, NotifyTableChangedAction.Update);
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
            SQLServerTableQuery<T> table = (SQLServerTableQuery<T>)this.Table(typeof(T));
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
            var q = string.Format("update \"{0}\" set {1} {2}",
                table.Table.TableName, string.Join(",", (from c in cols
                                                         select "\"" + c.Name + "\" = ? ").ToArray()), w.CommandText);
            ///
            int rowsAffected = 0;
            try
            {
                rowsAffected = Execute(q, GetParameters(ps));
            }
            catch (SqlServerException)
            {
                throw;
            }

            if (shouldnotify && rowsAffected > 0)
            {
                //map.SyncGuid.SetValue(obj, ExecuteScalar<Guid>(
                //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=?", map.PK.GetValue(obj)));
                OnTableChanged(table.Table, NotifyTableChangedAction.Update);
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
            var q = string.Format("delete from \"{0}\" where \"{1}\" = @{1}", map.TableName, pk.Name);

            if (map.SyncDirection != SyncDirection.NoSync)
            {
                Guid deleted = ExecuteScalar<Guid>(
                    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=@{map.PK.Name}",
                    new SqlParameter(map.PK.Name, map.PK.GetValue(objectToDelete)));
            }
            //if (deleted != Guid.Empty)
            //    map.SyncGuid.SetValue(objectToDelete, deleted);

            var count = Execute(q, new SqlParameter(map.PK.Name, map.PK.GetValue(objectToDelete)));
            if (count > 0)
                OnTableChanged(map, NotifyTableChangedAction.Delete);
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
            return Delete(primaryKey, GetMapping(typeof(T)));
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
        public int Delete(object primaryKey, Base.TableMapping map)
        {
            var pk = map.PK;
            if (pk == null)
            {
                throw new NotSupportedException("Cannot delete " + map.TableName + ": it has no PK");
            }
            var q = string.Format("delete from \"{0}\" where \"{1}\" = @{1}", map.TableName, pk.Name);

            //map.SyncGuid.SetValue(null, ExecuteScalar<Guid>(
            //    $"SELECT SyncGuid from {map.TableName} where {map.PK.Name}=@{map.PK.Name}",
            //    new SqlParameter(map.PK.Name, primaryKey)));

            var count = Execute(q, new SqlParameter(map.PK.Name, primaryKey));
            if (count > 0)
                OnTableChanged(map, NotifyTableChangedAction.Delete);
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
        public int DeleteAll<T>()
        {
            var map = GetMapping(typeof(T));
            return DeleteAll(map);
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
        public int DeleteAll(Base.TableMapping map)
        {
            var query = string.Format("delete from \"{0}\"", map.TableName);
            OnTableDeleteAll(map);
            var count = Execute(query);
            if (count > 0)
                OnTableChanged(map, NotifyTableChangedAction.Delete);
            return count;
        }

        /// <summary>
        /// Backup the entire database to the specified path.
        /// </summary>
        /// <param name="destinationDatabasePath">Path to backup file.</param>
        /// <param name="databaseName">The name of the database to backup (usually "main").</param>
        //public void Backup (string destinationDatabasePath, string databaseName = "main")
        //{
        //	// Open the destination
        //	var r = SQLite3.Open (destinationDatabasePath, out var destHandle);
        //	if (r != SQLite3.Result.OK) {
        //		throw SqlServerException.New (r, "Failed to open destination database");
        //	}

        //	// Init the backup
        //	var backup = SQLite3.BackupInit (destHandle, databaseName, Handle, databaseName);
        //	if (backup == NullBackupHandle) {
        //		SQLite3.Close (destHandle);
        //		throw new Exception ("Failed to create backup");
        //	}

        //	// Perform it
        //	SQLite3.BackupStep (backup, -1);
        //	SQLite3.BackupFinish (backup);

        //	// Check for errors
        //	r = SQLite3.GetResult (destHandle);
        //	string msg = "";
        //	if (r != SQLite3.Result.OK) {
        //		msg = SQLite3.GetErrmsg (destHandle);
        //	}

        //	// Close everything and report errors
        //	SQLite3.Close (destHandle);
        //	if (r != SQLite3.Result.OK) {
        //		throw SqlServerException.New (r, msg);
        //	}
        //}

        ~SQLServerConnection()
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
            if (IsOpen)
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

                        Connection.Close();
                        Connection.Dispose();
                    }
                    else
                    {
                        Connection.Close();
                    }
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public event EventHandler<NotifyTableChangedEventArgs> TableChanged;

        private void OnTableDeleteAll(Base.TableMapping table)
        {
            Table<ChangesHistory>().Delete(x => x.TableName == table.TableName);
            QueryScalars<Guid>($"SELECT SyncGuid FROM {table.TableName}")
                .ForEach(x => Insert(new ChangesHistory(table.TableName, x, NotifyTableChangedAction.Delete, table.SyncMode.Order)));
        }

        private void OnTableChanged(Base.TableMapping table, NotifyTableChangedAction action)
        {
            if (table.SyncDirection == SyncDirection.NoSync)
            {
                return;
            }

            //UpdateVersionControl(new ChangesHistory(
            //    table.TableName
            //    , (Guid)table.SyncGuid.GetValue(null)
            //    , action));
            var ev = TableChanged;
            if (ev != null)
                ev(this, new NotifyTableChangedEventArgs(table, action));
        }

        private void UpdateVersionControl(ChangesHistory VersionControl)
        {
            Table<ChangesHistory>()
                .Delete(x => x.Guid == VersionControl.Guid);
            Insert(VersionControl);
        }
    }
}