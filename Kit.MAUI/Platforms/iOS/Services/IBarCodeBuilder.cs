using Kit.Services.BarCode;
using UIKit;
using ZXing.Common;
using ZXing.Net.Maui;
using BarcodeFormat = ZXing.BarcodeFormat;

[assembly: Dependency(typeof(Kit.iOS.Services.BarCodeBuilder))]
namespace Kit.iOS.Services
{

    public class BarCodeBuilder : IBarCodeBuilder
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

                //barcodeWriter.Renderer = new BitmapRenderer();
                UIImage bitmap = barcodeWriter.Write(Value);
                MemoryStream stream = (MemoryStream)bitmap.AsPNG().AsStream(); // this is the diff between iOS and Android
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

