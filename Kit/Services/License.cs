﻿using System;
using System.Collections.Generic;
using System.Text;
using Kit;
using Kit.License;
using Kit.Services.Interfaces;
using BaseLicense = Kit.License.License;

namespace Kit.Services
{
    public class License
    {
        public static BaseLicense Instance(string AppName)
        {
#if NETSTANDARD1_0 || NETSTANDARD2_0
            return null;
#endif
#if NETCOREAPP
            return new NetCore.Services.License(AppName);
#endif
#if MONOANDROID

            return new Droid.Services.License(AppName);
#endif
#if __IOS__
            return new iOS.Services.License(AppName);
#endif
#if WINDOWS_UWP
            return new UWP.Services.License(AppName);
#endif
#if NET47
            return new WPF.Services.License(AppName);

#endif
        }
    }
}