﻿using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;

namespace Plugin.Xamarin.Tools.Shared
{
    public abstract class AbstractTools : ITools
    {
        public bool Debugging { get; protected set; }
        private string _LibraryPath;
        public string LibraryPath
        {
            get => _LibraryPath ?? System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
        }
        public AbstractTools()
        {

        }

        public abstract ITools InitAll(string LogDirectory, bool AlertAfterCritical = false);
        public abstract ITools InitLoggin(string LogDirectory = "Logs", bool AlertAfterCritical = false);
        public abstract void SetDebugging(bool Debugging);
        public abstract void CriticalAlert(object sender, EventArgs e);
        public void SetLibraryPath(string LibraryPath)
        {
            this._LibraryPath = LibraryPath;
        }
    }
}
