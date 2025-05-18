using System;
using System.IO;
using Android.Graphics;
using Kit.Droid.Services;
using Kit.Services.BarCode;
using Xamarin.Forms;
using ZXing;
using ZXing.Common;
using ZXing.Mobile;

[assembly: Dependency(typeof(BarCodeBuilder))]
namespace Kit.Droid.Services
{
    public class BarCodeBuilder :IBarCodeBuilder
    {
        public MemoryStream Generate(BarcodeFormat Formato, string Value, int Width = 350, int Height = 350, int Margin = 10,
            EncodingOptions Options = null)
        {
            try
            {
                Options ??= new ZXing.Common.EncodingOptions();
                Options.Width = Width;
                Options.Height = Width;
                Options.Margin = Width;

                BarcodeWriter barcodeWriter = new BarcodeWriter
                {
                    Format = Formato,
                    Options = Options
                };

                barcodeWriter.Renderer = new BitmapRenderer();
                Bitmap bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = new MemoryStream();
                bitmap.Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);  // this is the diff between iOS and Android
                stream.Position = 0;
                return stream;
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "Al generar un código QR");
                return null;
            }

        }

    }
}
