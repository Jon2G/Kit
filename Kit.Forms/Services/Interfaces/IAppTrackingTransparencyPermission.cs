﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace Kit.Forms.Services.Interfaces
{
    public interface IAppTrackingTransparencyPermission
    {
        /// <summary>
        /// This method checks if current status of the permission
        /// </summary>
        /// <returns></returns>
        Task<PermissionStatus> CheckStatusAsync();

        /// <summary>
        /// Requests the user to accept or deny a permission
        /// </summary>
        /// <returns></returns>
        void RequestAsync(Action<PermissionStatus> completion);
    }
}