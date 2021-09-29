﻿using System;
using System.Diagnostics;

namespace Kit.Sql.Helpers
{
    public static class Sqlh
    {
        //public bool Debugging { get; protected set; }
        //private string _LibraryPath;
        //public string LibraryPath
        //{
        //    get => _LibraryPath ?? Environment.GetFolderPath(Environment.SpecialFolder.Personal);
        //    set => _LibraryPath = value;
        //}

        //public Sqlh(string LibraryPath, bool Debugging)
        //{
        //    this.LibraryPath = LibraryPath;
        //    this.Debugging = Debugging;
        //}

        //public void SetDebugging(bool Debugging)
        //{
        //    this.Debugging = Debugging;
        //}

        //public static Sqlh Init(string LibraryPath, bool Debugging)
        //{
        //    Set(new Sqlh(LibraryPath, Debugging));
        //    return Instance;
        //}

        //private static void Set(Sqlh Instance)
        //{
        //    _Instance = Instance;
        //}
        //private static Sqlh _Instance;
        //public static Sqlh Instance => _Instance;

        public static string FormatTime(TimeSpan TimeSpan)
        {
            return $"{TimeSpan:hh}:{TimeSpan:mm}:{TimeSpan:ss}";
        }

        public static string FormatTime(DateTime TimeSpan)
        {
            //using (SQLiteConnection lite = Conecction())
            //{
            //'2020-09-17T12:27:55'  Formato universal de fecha y hora sql server
            return TimeSpan.ToString("yyyy-MM-ddTHH:mm:ss");
            //}
        }

        public static object NullIfEmpty(string value)
        {
            if (string.IsNullOrEmpty(value?.Trim()))
            {
                return DBNull.Value;
            }
            return value;
        }

        public static bool ToBool(object valor, bool _default = false)
        {
            if (IsNull(valor))
            {
                return _default;
            }
            switch (valor)
            {
                case short:
                case int:
                    return Convert.ToInt32(valor) == 1;

                case bool boleano:
                    return boleano;

                default:
                    return _default;
            }
        }

        public static bool? ToBool(object valor, bool? _default = null)
        {
            if (IsNull(valor))
            {
                return _default;
            }
            switch (valor)
            {
                case short:
                case int:
                    return Convert.ToInt32(valor) == 1;

                case bool boleano:
                    return boleano;

                default:
                    return _default;
            }
        }

        public static bool IsNull(object value)
        {
            return value == DBNull.Value || value == null;
        }

        public static T IfNull<T>(object value, T ifnull)
        {
            return (T)(IsNull(value) ? ifnull : value);
        }
        public static object IfNull(object value, object ifnull)
        {
            return IfNull<object>(value, ifnull);
        }

        public static T Parse<T>(object obj, T ifnull)
        {
            if (IsNull(obj))
            {
                return ifnull;
            }
            return Parse<T>(obj);
        }

        public static object Parse(Type type, object obj)
        {
            try
            {
                if (type.IsEnum)
                {
                    if(obj is int enum_int)
                    {
                        return Enum.ToObject(type, enum_int);
                    }
                    return Enum.Parse(type, obj.ToString(), true);
                }

                switch (type.Name)
                {
                    case "Guid":
                        return Guid.Parse(obj.ToString());

                    case "String":
                        return Convert.ToString(obj);

                    case "Single":
                        return Convert.ToSingle(obj);

                    case "Int32":
                        return Convert.ToInt32(obj);

                    case "Int64":
                        return Convert.ToInt64(obj);

                    case "Boolean":
                        if (obj is string sb)
                        {
                            return sb == "1";
                        }
                        return Convert.ToBoolean(obj);

                    case "Double":
                        return Convert.ToDouble(obj);

                    default:
                        if (obj is IConvertible)
                        {
                            return Convert.ChangeType(obj, type);
                        }
                        if (Debugger.IsAttached)
                            Debugger.Break();
                        break;
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, $"Al convertir un dato desde Parse<T> el tipo de dato: {type.Name}=>{obj}");
            }

            return type.GetDefault();
        }

        public static T Parse<T>(object obj)
        {
            var type = typeof(T);
            return (T)Parse(type, obj);
        }
    }
}