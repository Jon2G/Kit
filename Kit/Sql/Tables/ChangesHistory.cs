﻿using System;
using System.Text;
using Kit.Daemon.Sync;
using Kit.Sql.Attributes;
using Kit.Sql.Base;
using Kit.Sql.Enums;
using Kit.Sql.Sqlite;

namespace Kit.Sql.Tables
{
    [Preserve(AllMembers = true)]
    /// <summary>
    /// A table that keeps track of every change made on sqlite databate
    /// </summary>
    public class ChangesHistory : ISync
    {
        [PrimaryKey, AutoIncrement, Column("SyncGuid")]
        public override Guid Guid { get; set; }

        /// <summary>
        /// Name of the table where te change has been made
        /// </summary>
        public string TableName { get; set; }

        /// <summary>
        /// Type of change
        /// </summary>
        public NotifyTableChangedAction Action { get; set; }

        public int Priority { get; set; }

        public DateTime Date { get; set; }
        public ChangesHistory()
        {
        }

        public ChangesHistory(string TableName, Guid SyncGuid, NotifyTableChangedAction Action, int Priority)
        {
            this.TableName = TableName;
            this.Guid = SyncGuid;
            this.Action = Action;
            this.Priority = Priority;
            this.Date = DateTime.Now;
        }

        public void Save(SQLiteConnection con)
        {
            con.InsertOrReplace(this);
        }

        public static void MarkAsSynced(SqlBase origin, Guid SyncGuid)
        {
            SyncHistory syncHistory = new SyncHistory
            {
                DeviceId = Daemon.Devices.Device.Current.DeviceId,
                Guid = SyncGuid
            };
            origin.Table<SyncHistory>().Delete(x => x.Guid == syncHistory.Guid);
            origin.Insert(syncHistory, string.Empty);
        }

        public static void MarkAsSynced(SqlBase origin, ISync ISync) => MarkAsSynced(origin, ISync.Guid);

        public void MarkAsSynced(SqlBase origin)
        {
            try
            {
                SyncHistory syncHistory = new SyncHistory
                {
                    DeviceId = Daemon.Devices.Device.Current.DeviceId,
                    Guid = this.Guid,
                    Date=DateTime.Now
                };
                origin.Table<SyncHistory>().Delete(x => x.Guid == syncHistory.Guid);
                origin.Insert(syncHistory, string.Empty);

                //if (connection is SqlServer SQLH)
                //{
                //    SQLH.EXEC("INSERT INTO DESCARGAS_VERSIONES(ID_DESCARGA,ID_DISPOSITIVO) VALUES(@ID_DESCARGA,@ID_DISPOSITIVO)"
                //            , System.Data.CommandType.Text, false,
                //            new SqlParameter("ID_DESCARGA", Id),
                //            new SqlParameter("ID_DISPOSITIVO", Device.Current.DeviceId));
                //}
                //else if (connection is SqLite SQLHLite)
                //{
                //    SQLHLite.EXEC($"DELETE FROM VERSION_CONTROL WHERE ID=?", Id);
                //}
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al marcar como finalizada la sincronización - [{0}]", this);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            try
            {
                switch (this.Action)
                {
                    case NotifyTableChangedAction.Update:
                        sb.Append("Actualizando");
                        break;

                    case NotifyTableChangedAction.Insert:
                        sb.Append("Descargando");
                        break;

                    case NotifyTableChangedAction.Delete:
                        sb.Append("Eliminando");
                        break;

                    default:
                        sb.Append("NONE");
                        break;
                }
                sb.Append(" ");
                switch (this.TableName?.ToUpper())
                {
                    case "LINEAS":
                        sb.Append("lineas");
                        break;

                    case "PRODS":
                        sb.Append("productos");
                        break;

                    case "ALMACEN":
                        sb.Append("almacenes");
                        break;

                    case "CLAVESADD":
                        sb.Append("códigos de barras");
                        break;

                    default:
                        sb.Append(this.TableName);
                        break;
                }
                sb.Append(" [").AppendFormat("{0:N}", Guid).Append("]");
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "Al convertir este pendiete en su representación ToString");
            }
            return sb.ToString();
        }
    }
}