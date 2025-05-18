using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;
using ZXing.Net.Maui;
using ZXing.Net.Maui.Controls;

namespace Kit.MAUI.Forms.Pages
{
    public class ZXingScannerPage : ContentPage
    {
        readonly CameraBarcodeReaderView zxing;
        readonly ZXingDefaultOverlay defaultOverlay = null;

        public ZXingScannerPage(BarcodeReaderOptions options = null, View customOverlay = null)
            : base()
        {
            zxing = new CameraBarcodeReaderView
            {
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
                Options = options,
                AutomationId = "zxingScannerView"
            };

            zxing.SetBinding(CameraBarcodeReaderView.IsTorchOnProperty, new Binding(nameof(IsTorchOn)));
            zxing.SetBinding(CameraBarcodeReaderView.IsDetectingProperty, new Binding(nameof(IsScanning)));

            zxing.BarcodesDetected += (o, e) => OnScanResult?.Invoke(o, e);

            if (customOverlay == null)
            {
                defaultOverlay = new ZXingDefaultOverlay() { AutomationId = "zxingDefaultOverlay" };

                defaultOverlay.SetBinding(ZXingDefaultOverlay.TopTextProperty, new Binding(nameof(DefaultOverlayTopText)));
                defaultOverlay.SetBinding(ZXingDefaultOverlay.BottomTextProperty, new Binding(nameof(DefaultOverlayBottomText)));
                defaultOverlay.SetBinding(ZXingDefaultOverlay.ShowFlashButtonProperty, new Binding(nameof(DefaultOverlayShowFlashButton)));

                DefaultOverlayTopText = "Hold your phone up to the barcode";
                DefaultOverlayBottomText = "Scanning will happen automatically";
                DefaultOverlayShowFlashButton = true;

                defaultOverlay.FlashButtonClicked += (sender, e) =>
                    zxing.IsTorchOn = !zxing.IsTorchOn;

                Overlay = defaultOverlay;
            }
            else
            {
                Overlay = customOverlay;
            }

            var grid = new Grid
            {
                VerticalOptions = LayoutOptions.FillAndExpand,
                HorizontalOptions = LayoutOptions.FillAndExpand,
            };
            grid.Children.Add(zxing);
            grid.Children.Add(Overlay);

            // The root page of your application
            Content = grid;
        }

        #region Default Overlay Properties

        public static readonly BindableProperty DefaultOverlayTopTextProperty =
            BindableProperty.Create(nameof(DefaultOverlayTopText), typeof(string), typeof(ZXingScannerPage), string.Empty);
        public string DefaultOverlayTopText
        {
            get => (string)GetValue(DefaultOverlayTopTextProperty);
            set => SetValue(DefaultOverlayTopTextProperty, value);
        }

        public static readonly BindableProperty DefaultOverlayBottomTextProperty =
            BindableProperty.Create(nameof(DefaultOverlayBottomText), typeof(string), typeof(ZXingScannerPage), string.Empty);

        public string DefaultOverlayBottomText
        {
            get => (string)GetValue(DefaultOverlayBottomTextProperty);
            set => SetValue(DefaultOverlayBottomTextProperty, value);
        }

        public static readonly BindableProperty DefaultOverlayShowFlashButtonProperty =
            BindableProperty.Create(nameof(DefaultOverlayShowFlashButton), typeof(bool), typeof(ZXingScannerPage), false);

        public bool DefaultOverlayShowFlashButton
        {
            get => (bool)GetValue(DefaultOverlayShowFlashButtonProperty);
            set => SetValue(DefaultOverlayShowFlashButtonProperty, value);
        }

        #endregion

        public event EventHandler<BarcodeDetectionEventArgs> OnScanResult;

        public View Overlay { get; private set; }

        #region Functions

        public void ToggleTorch() => zxing.IsTorchOn = !zxing.IsTorchOn;

        protected override void OnAppearing()
        {
            base.OnAppearing();

            zxing.IsDetecting = true;
        }

        protected override void OnDisappearing()
        {
            zxing.IsDetecting = false;

            base.OnDisappearing();
        }

        public void PauseAnalysis()
        {
            if (zxing != null)
                zxing.IsDetecting = false;
        }

        public void ResumeAnalysis()
        {
            if (zxing != null)
                zxing.IsDetecting = true;
        }

        public void AutoFocus()
            => zxing?.AutoFocus();
        #endregion

        public static readonly BindableProperty IsTorchOnProperty =
            BindableProperty.Create(nameof(IsTorchOn), typeof(bool), typeof(ZXingScannerPage), false);

        public bool IsTorchOn
        {
            get => (bool)GetValue(IsTorchOnProperty);
            set => SetValue(IsTorchOnProperty, value);
        }

        public static readonly BindableProperty IsScanningProperty =
            BindableProperty.Create(nameof(IsScanning), typeof(bool), typeof(ZXingScannerPage), false);

        public bool IsScanning
        {
            get => (bool)GetValue(IsScanningProperty);
            set => SetValue(IsScanningProperty, value);
        }



        public static readonly BindableProperty ResultProperty =
            BindableProperty.Create(nameof(Result), typeof(Result), typeof(ZXingScannerPage), default(Result));

        public Result Result
        {
            get => (Result)GetValue(ResultProperty);
            set => SetValue(ResultProperty, value);
        }
    }
}