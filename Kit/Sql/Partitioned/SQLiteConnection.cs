﻿using System;
using System.IO;
using Kit.SetUpConnectionString;
using Kit.Sql.Enums;
using Kit.Sql.Readers;
using Kit.Sql.Tables;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Sql.Partitioned
{
    [Attributes.Preserve(AllMembers = true)]
    public class SQLiteConnection : Kit.Sql.Sqlite.SQLiteConnection
    {
        private readonly DirectoryInfo SchemaDirectory;

        public SQLiteConnection(DirectoryInfo SchemaDirectory, int DBVersion) :
            base(Path.Combine(SchemaDirectory.FullName, "Schema", "root.db"), DBVersion)
        {
            this.SchemaDirectory =
                new DirectoryInfo(Path.Combine(SchemaDirectory.FullName, "Schema"));
        }

        public override int InsertOrReplace(object obj, bool shouldnotify = true)
        {
            return base.InsertOrReplace(obj, shouldnotify);
        }

        public override int Insert(object obj, string extra)
        {
            throw new NotImplementedException();
        }

        public override int Insert(object obj, string extra, Type objType, bool shouldnotify = true)
        {
            return base.Insert(obj, extra, objType, shouldnotify);
        }

        public override int Delete(object objectToDelete)
        {
            throw new NotImplementedException();
        }

        public override T Find<T>(object pk)
        {
            throw new NotImplementedException();
        }

        public override CreateTableResult CreateTable(TableMapping table, CreateFlags createFlags = CreateFlags.None)
        {
            ToPartitionedDb(table.TableName);
            return base.CreateTable(table, createFlags);
        }

        internal void ToPartitionedDb(string tableTableName)
        {
            if (tableTableName == nameof(DatabaseVersion) ||
                tableTableName == nameof(ChangesHistory) ||
                tableTableName == nameof(Configuracion) ||
                tableTableName == nameof(SyncHistory)
            )
            {
                this.ConnectionString =
                    new Kit.Sql.Sqlite.SQLiteConnectionString(Path.Combine(this.SchemaDirectory.FullName, "root.db"));
                this.DatabasePath = this.ConnectionString.DatabasePath;
            }
            else
            {
                this.ConnectionString =
                    new Kit.Sql.Sqlite.SQLiteConnectionString(Path.Combine(this.SchemaDirectory.FullName, tableTableName + ".db"));
                this.DatabasePath = this.ConnectionString.DatabasePath;
            }
            RenewConnection();
        }

        public override bool TableExists(string TableName)
        {
            throw new NotImplementedException();
        }

        public override int EXEC(string sql)
        {
            throw new NotImplementedException();
        }

        public T Single<T>(string sql, string Schema) where T : IConvertible
        {
            ToPartitionedDb(Schema);
            return Single<T>(sql);
        }

        public override T Single<T>(string sql)
        {
            return base.Single<T>(sql);
        }

        public object Single(string sql, string Schema)
        {
            ToPartitionedDb(Schema);
            return Single(sql);
        }

        public override object Single(string sql)
        {
            return base.Single(sql);
        }

        public IReader Read(string sql, string Schema)
        {
            ToPartitionedDb(Schema);
            return Read(sql);
        }

        public override IReader Read(string sql)
        {
            return base.Read(sql);
        }
    }
}