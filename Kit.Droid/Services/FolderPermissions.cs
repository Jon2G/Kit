﻿using Acr.UserDialogs.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Kit.Services.Interfaces;

namespace Kit.Droid.Services
{
    public class FolderPermissions : IFolderPermissions
    {
        public override async Task<bool> CanRead(string directoryInfo)
        {
            await Task.Yield();
            Java.IO.File f = new Java.IO.File(directoryInfo);
            return f.CanRead();
        }

        public override async Task<bool> CanWrite(string directoryInfo)
        {
            await Task.Yield();
            Java.IO.File f = new Java.IO.File(directoryInfo);
            return f.CanWrite();
        }

        public override async Task<bool> TryToUnlock(string Path)
        {
            await Task.Yield();
            try
            {
                Java.IO.File f = new Java.IO.File(Path);
                f.SetWritable(true);
                f.SetReadable(true);
                return true;
            }
            catch (Exception ex)
            {
             Kit.Sql.Log.LogMe(ex);
            }
            return false;
        }
    }
}
