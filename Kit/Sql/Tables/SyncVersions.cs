﻿using Kit.Daemon.Abstractions;
using Kit.Enums;
using Kit.Sql;
using Kit.Sql.Helpers;
using Kit.Sql.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;
using SQLServer;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Daemon.VersionControl
{
    [Preserve(AllMembers = true)]
    public class SyncVersions
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        [Unique,MaxLength(50)]
        public string Name { get; set; }
        public int Version { get; set; }
        public SyncVersionObject Type { get; set; }
        public SyncVersions() { }

        //protected override void CreateTable(SQLServerConnection SQLH)
        //{
        //    if (SQLH.TableExists("DESCARGAS_VERSIONES"))
        //        SQLH.EXEC("DROP TABLE DESCARGAS_VERSIONES");

        //    if (SQLH.TableExists("DISPOSITVOS_TABLETS"))
        //        SQLH.EXEC("DROP TABLE DISPOSITVOS_TABLETS");

        //    if (SQLH.TableExists("VERSION_CONTROL"))
        //        SQLH.EXEC("DROP TABLE VERSION_CONTROL");

        //    if (SQLH.TableExists("TRIGGERS_INFO"))
        //        SQLH.EXEC("DROP TABLE TRIGGERS_INFO");

        //    SQLH.EXEC(@"CREATE TABLE DAEMON_VERSION (
        //          ID INT IDENTITY(1,1) PRIMARY KEY,
        //          TABLA VARCHAR(100) DEFAULT '',
        //          VERSION VARCHAR(100) DEFAULT '0.0.0');");

        //}
        //public static void SaveVersion(SqlBase SQLH, string tableName)
        //{
        //    SQLH.EXEC($"INSERT INTO DAEMON_VERSION (TABLA,VERSION) VALUES('{tableName}','{Daemon.Current.DaemonConfig.DbVersion}');");
        //}
        public static SyncVersions GetVersion(SqlBase SQLH, TableMapping table)
        {
            if (!SQLH.TableExists(table.TableName))
            {
                return Default(table.TableName);
            }
            return SQLH.Table<SyncVersions>().FirstOrDefault(x => x.Name == table.TableName)
                   ?? Default(table.TableName);
        }

        private static SyncVersions Default(string TableName)
        {
            return new SyncVersions()
            {
                Name = TableName,
                Type = SyncVersionObject.Table,
                Version = 0
            };
        }
        //protected override void CreateTable(SQLiteConnection SQLH)
        //{
        //    //Just for sqlserver
        //    return;
        //}


    }
}