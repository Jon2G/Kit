using System;
using System.IO;
using System.Threading.Tasks;
using Kit.Enums;
using Xamarin.Essentials;

namespace Kit.Forms.Services
{
    public static class CameraService
    {
        public static async Task<FileInfo> TakePhotoAsync()
        {
            try
            {
                var photo = await MediaPicker.CapturePhotoAsync();
                var file = await LoadPhotoAsync(photo);
                Console.WriteLine($"CapturePhotoAsync COMPLETED: {file}");
                return file;
            }
            catch (FeatureNotSupportedException)
            {
                // Feature is now supported on the device
            }
            catch (PermissionException)
            {
                // Permissions not granted
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CapturePhotoAsync THREW: {ex.Message}");
            }

            return null;
        }

        public static async Task<FileInfo> LoadPhotoAsync(this FileResult photo)
        {
            await Task.Yield();
            // canceled
            if (photo == null)
            {
                return null;
            }
            // save the file into local storage
            DirectoryInfo tmpDir = new DirectoryInfo(Tools.Instance.TemporalPath);
            if (!tmpDir.Exists)
            {
                tmpDir.Create();
            }
            string newFile = Path.Combine(tmpDir.FullName, photo.FileName);
            FileInfo file = new FileInfo(newFile);
            if (Tools.Instance.RuntimePlatform==RuntimePlatform.iOS&&file.Exists)
            {
                return file;
            }
            using (Stream stream= await photo.OpenReadAsync())
            {
                using (FileStream newStream = new FileStream(file.FullName, FileMode.OpenOrCreate))
                {
                    await stream.CopyToAsync(newStream);
                }
            }

            file.Refresh();
            return file;
        }
    }
}
