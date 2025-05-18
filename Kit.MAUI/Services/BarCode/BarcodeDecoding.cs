
using ImageProcessing.JPEGCodec;
using ImageProcessing.PNGCodec;
using ImageProcessing.TGACodec;
using MoyskleyTech.ImageProcessing.Image;
using BarcodeFormat = ZXing.BarcodeFormat;
using BinaryBitmap = ZXing.BinaryBitmap;
using DecodeHintType = ZXing.DecodeHintType;
using HybridBinarizer = ZXing.Common.HybridBinarizer;
using MultiFormatReader = ZXing.MultiFormatReader;
using Result = ZXing.Result;
using RGBLuminanceSource = ZXing.RGBLuminanceSource;

namespace Kit.Services.BarCode
{
    [Preserve()]
    public class BarcodeDecoding
    {
        public BarcodeDecoding()
        {
        }

        public Result Decode(BinaryBitmap image, BarcodeFormat format, KeyValuePair<DecodeHintType, object>[] aditionalHints = null)
        {
            MultiFormatReader r = GetReader(format, aditionalHints);
            Result result = r.decode(image);
            return result;
        }

        public Result Decode(Stream stream, BarcodeFormat format, KeyValuePair<DecodeHintType, object>[] aditionalHints = null)
        {
            BinaryBitmap image = GetImage(stream);
            if (image is null)
            {
                return null;
            }
            return Decode(image, format, aditionalHints);
        }

        public Result Decode(FileInfo file, BarcodeFormat format, KeyValuePair<DecodeHintType, object>[] aditionalHints = null)
        {
            MultiFormatReader r = GetReader(format, aditionalHints);

            BinaryBitmap image = GetImage(file);
            if (image is null)
            {
                return null;
            }
            Result result = r.decode(image);

            return result;
        }

        private MultiFormatReader GetReader(BarcodeFormat? format, KeyValuePair<DecodeHintType, object>[] aditionalHints)
        {
            var reader = new MultiFormatReader();

            var hints = new Dictionary<DecodeHintType, object>();

            if (format.HasValue)
            {
                hints.Add(DecodeHintType.POSSIBLE_FORMATS, new List<BarcodeFormat>() { format.Value });
            }
            if (aditionalHints != null)
            {
                foreach (var ah in aditionalHints)
                {
                    hints.Add(ah.Key, ah.Value);
                }
            }

            reader.Hints = hints;

            return reader;
        }

        private BinaryBitmap GetImage(FileInfo fileName)
        {
            try
            {
                using (var stream = fileName.OpenRead())
                {
                    return GetImage(stream);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "BarcodeDecoding.GetImage");
                Tools.Instance.Dialogs.CustomMessageBox.Show(ex.Message);
            }
            return null;
        }

        private BinaryBitmap GetImage(Stream mstream)
        {
            PngCodec.Register();
            JPEGCodec.Register();
            var png = new Hjg.Pngcs.Chunks.PngChunkIHDR(new Hjg.Pngcs.ImageInfo(10, 10, 8, true));
            BinaryBitmap bBitmap = null;
            try
            {
                using (Stream stream = mstream)
                {
                    stream.Position = 0;
                    Image<Pixel> result;

                    BitmapFactory factory = new BitmapFactory();
                    factory.AddCodec(new BitmapCodec());
                    factory.AddCodec(new PngCodec());
                    factory.AddCodec(new JPEGCodec());
                    factory.AddCodec(new TGACodec());
                    result = factory.Decode(stream);
                    result = result.GetBitmap(0, 0, result.Width, result.Height);

                    byte[] rgbBytes = GetRgbBytes(result);
                    var bin = new HybridBinarizer(new RGBLuminanceSource(rgbBytes, result.Width, result.Height));
                    Log.Logger.Debug("Memory:" + rgbBytes.Length);
                    Log.Logger.Debug("Size:" + (result.Width * result.Height) * 3);
                    bBitmap = new BinaryBitmap(bin);
                }
            }
            catch (Exception ex)
            {
                Log.Logger.Error(ex, "BarcodeDecoding.GetImage");
                Tools.Instance.Dialogs.CustomMessageBox.Show(ex.Message);
            }
            return bBitmap;
        }

        private byte[] GetRgbBytes(Image<Pixel> image)
        {
            var rgbBytes = new List<byte>();
            for (int y = 0; y < image.Height; y++)
            {
                for (int x = 0; x < image.Width; x++)
                {
                    var c = image.Get(x, y);
                    rgbBytes.AddRange(new[] { c.R, c.G, c.B });
                }
            }
            return rgbBytes.ToArray();
        }
    }
}