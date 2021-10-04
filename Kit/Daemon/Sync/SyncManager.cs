﻿using Kit.Daemon.Enums;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Kit.Daemon.Devices;
using Kit.Model;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Exceptions;
using Kit.Sql.Sqlite;
using Kit.Sql.SqlServer;
using Kit.Sql.Tables;
using static Kit.Daemon.Helpers.Helper;
using TableMapping = Kit.Sql.Base.TableMapping;

namespace Kit.Daemon.Sync
{
    public class SyncManager : ModelBase
    {
        public const int RegularPackageSize = 100;
        public bool ToDo { get; set; }
        public bool NothingToDo { get => !ToDo; }
        private Queue<ChangesHistory> Pendings;
        private int TotalPendientes;
        public SyncTarget CurrentDirection { get; internal set; }

        public float Progress
        {
            get
            {
                if (TotalPendientes > 0 && Processed > 0)
                {
                    return Processed / (float)TotalPendientes;
                }
                else
                {
                    return 0;
                }
            }
        }

        private int _Processed;

        private int Processed
        {
            get => _Processed;
            set
            {
                _Processed = value;
                Raise(() => this.Progress);
            }
        }

        public int PackageSize { get; private set; }
        private ChangesHistory _CurrentPackage;

        public ChangesHistory CurrentPackage
        {
            get => _CurrentPackage;
            set
            {
                _CurrentPackage = value;
                Raise(() => this.CurrentPackage);
            }
        }

        private string DownloadQuery
        {
            get;
            set;
        }

        private string UploadQuery
        {
            get;
            set;
        }

        public SyncManager()
        {
            this.Pendings = new Queue<ChangesHistory>();
            this.Processed = 0;
            this.PackageSize = RegularPackageSize;
            this.DownloadQuery = string.Empty;
            this.CurrentDirection = SyncTarget.NOT_SET;
        }

        public void SetPackageSize(int PackageSize = RegularPackageSize)
        {
            this.PackageSize = PackageSize;
            UploadQuery = null;
            DownloadQuery = string.Empty;
        }

        public bool Download()
        {
            lock (DownloadQuery)
            {
                if (string.IsNullOrEmpty(DownloadQuery))
                {
                    DownloadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncTarget.Remote]);
                    Log.Logger.Information("Prepared {0} Download Query - [{1}]", "DAEMON", DownloadQuery);
                }
                return GetPendings(SyncTarget.Local);
            }
        }

        public bool Upload()
        {
            if (UploadQuery is null)
            {
                UploadQuery = PrepareQuery(Daemon.Current.DaemonConfig[SyncTarget.Local]);
                Log.Logger.Information("Prepared {0} Upload Query - [{1}]", "DAEMON", UploadQuery);
            }
            return GetPendings(SyncTarget.Remote);
        }

        private string PrepareQuery(SqlBase source)
        {
            string query = string.Empty;
            switch (source)
            {
                case SQLServerConnection:
                    query = "SELECT ";
                    if (PackageSize > 0)
                    {
                        query += $"TOP {this.PackageSize}";
                    }
                    query += $" SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid) order by Priority";
                    return query;

                case SQLiteConnection:
                    query =
                        $"select SyncGuid,TableName,Action from ChangesHistory c where not exists(select 1 from SyncHistory s where s.DeviceId = '{Device.Current.DeviceId}' and s.SyncGuid=c.SyncGuid) order by Priority";
                    if (PackageSize > 0)
                    {
                        query += $" limit {this.PackageSize}";
                    }
                    return query;
            }
            return query;
        }

        private bool GetPendings(SyncTarget SyncTarget)
        {
            try
            {
                Daemon.Current.IsAwake = true;
                if (Daemon.Current.IsSleepRequested)
                {
                    return false;
                }
                this.CurrentPackage = null;
                string query = string.Empty;
                switch (SyncTarget)
                {
                    case SyncTarget.Local:
                        query = DownloadQuery;
                        break;

                    case SyncTarget.Remote:
                        query = UploadQuery;
                        break;
                }
                var source = SyncTarget.InvertDirection();
                if (!string.IsNullOrEmpty(query))
                {
                    if (Daemon.Current.IsSleepRequested)
                    {
                        return false;
                    }
                    this.Pendings = new Queue<ChangesHistory>(Daemon.Current.DaemonConfig[source].RenewConnection()
                        .DeferredQuery<ChangesHistory>(query).ToList());
                }
                TotalPendientes = Pendings.Count;
                ToDo = TotalPendientes > 0;
                this.CurrentDirection = SyncTarget;
                if (ToDo && !Daemon.Current.IsSleepRequested)
                {
                    ToDo = ProcesarAcciones(SyncTarget);
                    return true;
                }
                return false;
            }
            catch (Exception ex)
            {
                Log.AlertOnDBConnectionError(ex);
                Log.Logger.Error(ex, $"Obteniendo pendientes {SyncTarget}");
            }
            return false;
        }

        private bool ProcesarAcciones(SyncTarget direccion)
        {
            Processed = 0;
            CurrentPackage = null;
            TableMapping table = null;
            SyncTarget source = direccion.InvertDirection();
            SqlBase source_con = Daemon.Current.DaemonConfig[source];
            SqlBase target_con = Daemon.Current.DaemonConfig[direccion];
            string condition = (source_con is SQLiteConnection ? "SyncGuid=?" : "SyncGuid=@SyncGuid");

            bool CanDo = false;

            while (Pendings.Any())
            {
                if (Daemon.Current.IsSleepRequested)
                {
                    Pendings.Clear();
                    return false;
                }
                try
                {
                    this.CurrentPackage = Pendings.Dequeue();

                    table = Daemon.Current.Schema[this.CurrentPackage.TableName, direccion];
                    if (table != null)
                    {
                        switch (table.SyncDirection)
                        {
                            case SyncDirection.TwoWay:
                                break;

                            case SyncDirection.Upload:
                                if (direccion != SyncTarget.Remote)
                                {
                                    continue;
                                }
                                break;

                            case SyncDirection.Download:
                                if (direccion != SyncTarget.Local)
                                {
                                    continue;
                                }
                                break;
                        }
                        //string key = source_con.GetTableMappingKey(this.CurrentPackage.TableName);
                        NotifyTableChangedAction action = CurrentPackage.Action;
                        string selection_list = table.SelectionList;
                        CommandBase command = source_con.CreateCommand($"SELECT {selection_list} FROM {table.TableName} WHERE {condition}",
                         new BaseTableQuery.Condition("SyncGuid", CurrentPackage.Guid));
                        MethodInfo method = command.GetType().GetMethod(nameof(CommandBase.ExecuteDeferredQuery), new[] { typeof(TableMapping) });
                        method = method.MakeGenericMethod(table.MappedType);
                        IEnumerable<dynamic> result = (IEnumerable<dynamic>)method.Invoke(command, new object[] { table });
                        if (!result.Any())
                        {
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (Daemon.Current.IsSleepRequested) { return false; }
                        dynamic i_result = result.First();
                        ISync read = Convert.ChangeType(i_result, typeof(ISync));
                        if (read is null && action == NotifyTableChangedAction.Delete)
                        {
                            if (target_con is SQLiteConnection lite)
                            {
                                lite.EXEC($"DELETE FROM {table.TableName} where SyncGuid=?", CurrentPackage.Guid);
                            }
                            else if (target_con is SQLServerConnection con)
                            {
                                con.EXEC($"DELETE FROM {table.TableName} where SyncGuid=@SyncGuid", new System.Data.SqlClient.SqlParameter("SyncGuid", CurrentPackage.Guid));
                            }
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (read is null)
                        {
                            Log.Logger.Warning("READ RESULTO EN NULL '{0}'", this.CurrentPackage.TableName);
                            CurrentPackage.MarkAsSynced(source_con);
                            continue;
                        }
                        if (read != null && CurrentPackage is not null)
                        {
                            switch (CurrentPackage.Action)
                            {
                                case NotifyTableChangedAction.Insert:
                                case NotifyTableChangedAction.Update:

                                    if (direccion == SyncTarget.Local || read.ShouldSync(source_con, target_con))
                                    {
                                        CanDo = true;
                                        object old_pk = null;

                                        if (table.PK != null)
                                        {
                                            old_pk = read.GetPk();
                                        }

                                        if (read.CustomUpload(source_con, target_con, table))
                                        {
                                            CurrentPackage.MarkAsSynced(source_con);
                                            Processed++;
                                            read.OnSynced(direccion, action);
                                            return true;
                                        }

                                        if (target_con is SQLiteConnection)
                                        {
                                            target_con.InsertOrReplace(read, false);
                                        }
                                        else
                                        {
                                            target_con.Table<ChangesHistory>().Delete(x => x.Guid == CurrentPackage.Guid);
                                            target_con.Insert(read, String.Empty, read.GetType(), false);
                                        }

                                        if (source_con is SQLiteConnection lite)
                                        {
                                            if (read.Affects(lite, old_pk))
                                            {
                                                CurrentPackage.MarkAsSynced(source_con);
                                                Processed++;
                                                read.OnSynced(direccion, action);
                                                return true;
                                            }
                                        }

                                        CurrentPackage.MarkAsSynced(source_con);
                                        Processed++;
                                        read.OnSynced(direccion, action);
                                    }
                                    break;

                                case NotifyTableChangedAction.Delete:
                                    target_con.Delete(read);
                                    read.OnSynced(direccion, action);
                                    CurrentPackage.MarkAsSynced(source_con);
                                    Processed++;
                                    break;
                            }
                        }
                    }
                    else
                    {
                        Log.Logger.Warning("TABLA NO ENCONTRADA EN EL SCHEMA DEFINIDO '{0}'", this.CurrentPackage.TableName);
                        CurrentPackage.MarkAsSynced(source_con);
                        Processed++;
                    }
                }
                catch (Exception ex)
                {
                    if (ex is SQLiteException)
                    {
                        if (ex.Message.Contains("no such table"))
                        {
                            target_con.CreateTable(table);
                        }
                    }
                    if (Debugger.IsAttached)
                    {
                        //Debugger.Break();
                    }
                    Log.Logger.Error(ex, "Al sincronizar - {0}", CurrentPackage);
                }
            }

            return CanDo;
        }

        private string BuildSqlServerQuery()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("SELECT ");
            if (PackageSize > 0)
            {
                sb.Append("TOP ").Append(PackageSize);
            }
            sb.Append(@" ID,ACCION,TABLA,LLAVE FROM VERSION_CONTROL WHERE NOT EXISTS(SELECT ID_DISPOSITIVO FROM DESCARGAS_VERSIONES WHERE DESCARGAS_VERSIONES.ID_DESCARGA = VERSION_CONTROL.ID AND ID_DISPOSITIVO ='")
                .Append(Device.Current.DeviceId).Append("') --ORDER BY TABLA DESC, LLAVE ASC;");
            return sb.ToString();
        }

        public void Reset()
        {
            this.Pendings.Clear();
            this.ToDo = false;
        }
    }
}