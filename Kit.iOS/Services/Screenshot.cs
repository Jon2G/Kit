﻿using Foundation;
using Kit.iOS.Services;
using Kit.Services.Interfaces;
using System;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using UIKit;
[assembly: Xamarin.Forms.Dependency(typeof(Screenshot))]
namespace Kit.iOS.Services
{
    public class Screenshot : IScreenshot
    {
        public async Task<byte[]> Capture()
        {
            await Task.Yield();
            UIImage capture = UIScreen.MainScreen.Capture();
            using (NSData data = capture.AsPNG())
            {
                byte[] bytes = new byte[data.Length];
                Marshal.Copy(data.Bytes, bytes, 0, Convert.ToInt32(data.Length));
                return bytes;
            }
        }
    }
}
