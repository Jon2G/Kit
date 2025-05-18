using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Kit.Forms.Extensions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Xamarin.Essentials;

namespace Kit.Forms.Services.Interfaces
{
    public class ICameraService
    {
        PermissionStatus cameraOK;
        PermissionStatus storageOK;
        bool IsInited;
        bool IsOpen;
        public ICameraService()
        {
            this.IsInited = false;
            this.IsOpen = false;
        }
        public async Task Init()
        {
            await CrossMedia.Current.Initialize();
            cameraOK = await Permisos.GetPermissionStatus<Xamarin.Essentials.Permissions.Camera>();
            storageOK = await Permisos.GetPermissionStatus<Xamarin.Essentials.Permissions.StorageRead>();
            if (cameraOK != PermissionStatus.Granted || storageOK != PermissionStatus.Granted)
            {
                cameraOK = await Permisos.GetPermissionStatus<Xamarin.Essentials.Permissions.Camera>();
                storageOK = await Permisos.GetPermissionStatus<Xamarin.Essentials.Permissions.StorageRead>();
            }
            this.IsInited = true;
        }

        public async Task<MediaFile> TakePhoto()
        {
            if (!IsInited)
            {
                await Init();
            }
            if (cameraOK == PermissionStatus.Granted
                && storageOK == PermissionStatus.Granted
                && CrossMedia.Current.IsCameraAvailable
                && CrossMedia.Current.IsTakePhotoSupported)
            {
                StoreCameraMediaOptions options = new StoreCameraMediaOptions
                {
                    DefaultCamera = CameraDevice.Front, // Doesn't always work on Android, depends on Device
                    AllowCropping = true, // UWP & iOS only,
                    PhotoSize = PhotoSize.Medium, // if Custom, you can set CustomPhotoSize = percentage_value 
                    CompressionQuality = 90,
                    Directory = "DemoCamara",
                    Name = $"{Guid.NewGuid()}.jpg",
                    SaveToAlbum = true
                };

                MediaFile file = await CrossMedia.Current.TakePhotoAsync(options);
                return file;
            }

            return null;
        }
        public async Task<MediaFile> ChoosePhoto()
        {
            if (!IsInited)
            {
                await Init();
            }
            if (IsOpen)
            {
                return null;
            }
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                this.IsOpen = true;
                MediaFile file = await CrossMedia.Current.PickPhotoAsync();
                this.IsOpen = false;
                return file;
            }

            return null;
        }
        public async Task<List<MediaFile>> ChoosePhotos()
        {
            if (!IsInited)
            {
                await Init();
            }
            if (IsOpen)
            {
                return null;
            }
            if (CrossMedia.Current.IsPickPhotoSupported)
            {
                this.IsOpen = true;
                List<MediaFile> files = await CrossMedia.Current.PickPhotosAsync();
                this.IsOpen = false;
                return files;
            }
            return null;
        }
        public async Task<MediaFile> TakeVideo()
        {
            if (!IsInited)
            {
                await Init();
            }
            if (cameraOK == PermissionStatus.Granted
                && storageOK == PermissionStatus.Granted
                && CrossMedia.Current.IsTakeVideoSupported)
            {
                StoreVideoOptions options = new StoreVideoOptions { SaveToAlbum = true };
                MediaFile file = await CrossMedia.Current.TakeVideoAsync(options);
                return file;
            }

            return null;
        }
        public async Task<MediaFile> ChooseVideo()
        {
            if (!IsInited)
            {
                await Init();
            }
            if (CrossMedia.Current.IsPickVideoSupported)
            {
                MediaFile file = await CrossMedia.Current.PickVideoAsync();
                return file;
            }

            return null;
        }
    }
}

