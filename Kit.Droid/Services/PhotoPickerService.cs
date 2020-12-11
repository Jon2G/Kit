﻿using Android.Content;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Kit.Droid;
using Kit.Services.Interfaces;
using Xamarin.Forms;

namespace Kit.Droid.Services
{
    public class PhotoPickerService : IPhotoPickerService
    {
        // Field, property, and method for Picture Picker
        public const int PickImageId = 1000;

        public Task<Tuple<byte[], ImageSource>> GetImageAsync()
        {
            // Define the Intent for getting images
            Intent intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);

            ToolsImplementation tools =Kit.Tools.Instance as ToolsImplementation;
            // Start the picture-picker activity (resumes in MainActivity.cs)
            tools.MainActivity.StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), PickImageId);
            // Save the TaskCompletionSource object as a MainActivity property
            tools.MainActivity.PickImageTaskCompletionSource = new TaskCompletionSource<Tuple<byte[], ImageSource>>();

            ////ADD ON PLATAFORM SPECIFIC
            //    protected override void OnActivityResult(int requestCode, Result resultCode, Intent intent)
            //    {
            //    base.OnActivityResult(requestCode, resultCode, intent);

            //    if (requestCode == PickImageId)
            //    {
            //        if ((resultCode == Result.Ok) && (intent != null))
            //        {
            //            Android.Net.Uri uri = intent.Data;
            //            Stream stream = ContentResolver.OpenInputStream(uri);
            //            byte[] bits = null;
            //            try
            //            {
            //                using (var memoryStream = new MemoryStream())
            //                {
            //                    stream.CopyTo(memoryStream);
            //                    bits = memoryStream.ToArray();
            //                }
            //                Tuple<byte[], ImageSource> tuple = new Tuple<byte[], ImageSource>(bits,
            //                    ImageSource.FromStream(() => new MemoryStream(bits)));
            //                PickImageTaskCompletionSource.SetResult(tuple);
            //            }
            //            catch (Exception ex)
            //            {
            //                Log.LogMe(ex, "Al obtener la imagen despues de ser abierta");
            //            }
            //        }
            //        else
            //        {
            //            PickImageTaskCompletionSource.SetResult(null);
            //        }
            //    }
            //}
            /////////////////////////////


            // Return Task object
            return tools.MainActivity.PickImageTaskCompletionSource.Task;
        }
    }
}