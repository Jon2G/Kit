using Microsoft.Maui.Controls.PlatformConfiguration.WindowsSpecific;

namespace Kit.Forms.Controls.WebView
{
    public class KitWebView : Microsoft.Maui.Controls.WebView
    {
        private NavigationRequest CurrentRequest;
        protected readonly Queue<NavigationRequest> NavigationQueue;
        private bool _IsNavigating;
        public bool IsNavigating
        {
            get => _IsNavigating;
            private set
            {
                _IsNavigating = value;
            }
        }
        public bool ShowLoading { get; set; }

        public static readonly BindableProperty HomePageProperty = BindableProperty.Create(
            propertyName: nameof(HomePage), returnType: typeof(string), declaringType: typeof(KitWebView), defaultValue: string.Empty);
        public virtual string HomePage
        {
            get => (string)GetValue(HomePageProperty);
            set
            {
                SetValue(HomePageProperty, value);
                OnPropertyChanged(nameof(HomePage));
            }
        }

        public static readonly BindableProperty CommandProperty =
            BindableProperty.Create(nameof(KitWebView.FailureCommand), typeof(Kit.Extensions.Command<WebNavigationResult>),
                typeof(KitWebView), null);

        public Kit.Extensions.Command<WebNavigationResult> FailureCommand
        {
            get { return (Kit.Extensions.Command<WebNavigationResult>)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }



        public KitWebView()
        {
            this.IsPlatformEnabled = true;
            this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetIsJavaScriptAlertEnabled(true);
            this.On<Microsoft.Maui.Controls.PlatformConfiguration.Windows>().SetExecutionMode(WebViewExecutionMode.SeparateProcess);
            this.ShowLoading = true;
            this.Navigated += Browser_Navigated;
            this.NavigationQueue = new Queue<NavigationRequest>();

        }

        protected virtual async void Browser_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Result == WebNavigationResult.Timeout || e.Result == WebNavigationResult.Cancel ||
                e.Result == WebNavigationResult.Failure)
            {
                Log.Logger.Warning("Browser_Navigated {0}", e.Result);
                FailureCommand?.Execute(e.Result);
                this.CurrentRequest.Done();
                return;
            }
            await this.EvaluateJavaScriptAsync(@"window.onerror = function myErrorHandler(errorMsg, url, lineNumber) { console.log('Error occured: ' + errorMsg); return false; }");
            if (e.Url is null)
            {
                this.CurrentRequest?.Done();
                await GoTo(HomePage);
                return;
            }
            if (IsOn(e.Source as UrlWebViewSource, this.CurrentRequest))
            {
                this.CurrentRequest.Done();
            }
            else
            {
                Log.Logger.Debug($"Landed unexpectly at =>[{e.Url}]");
            }
        }

        public Task GoHome() => GoTo(HomePage);
        public async Task GoTo(string url)
        {
            await Task.Yield();
            string navigateUrl = url;
            if (!url.StartsWith(HomePage))
            {
                navigateUrl = HomePage;
                if (!navigateUrl.EndsWith("/"))
                {
                    navigateUrl += '/';
                }
                navigateUrl += url;
            }
            await Task.Run(() =>
            {
                while (this.IsNavigating)
                {
                }
            });
            var request = new NavigationRequest(navigateUrl);
            NavigationQueue.Enqueue(request);
            NavigateAsync();
#if ANDROID || IOS || MACCATALYST
            using (Acr.UserDialogs.UserDialogs.Instance.Loading("Espere un momento...", show: ShowLoading))
            {
                await request.Wait();
                await Task.Run(() =>
                {
                    while (!request.IsComplete)
                    {
                    }
                });
            }
#endif
        }
        private async void NavigateAsync()
        {
            this.IsNavigating = true;
            await Task.Yield();
            try
            {
                while (NavigationQueue.Any())
                {
                    this.CurrentRequest = NavigationQueue.Dequeue();
                    this.Source = new UrlWebViewSource()
                    {
                        Url = this.CurrentRequest.Url.AbsoluteUri
                    };
                    OnPropertyChanged(nameof(Source));
                    Log.Logger.Debug("Requested:{0}", CurrentRequest);
                    await this.CurrentRequest.Wait();
                    this.CurrentRequest.IsComplete = true;
                    Log.Logger.Debug("Complete:{0}", CurrentRequest);
                }
            }
            catch (Exception e)
            {
                Log.Logger.Error(e, "NavigateAsync");
            }
            finally
            {
                this.IsNavigating = false;
            }
        }


        private bool UriCompare(Uri uri1, Uri uri2)
        {
            var result = Uri.Compare(uri1, uri2,
                UriComponents.Host | UriComponents.PathAndQuery,
                UriFormat.SafeUnescaped, StringComparison.OrdinalIgnoreCase);
            return result == 0;
        }
        private bool IsOn(UrlWebViewSource source, NavigationRequest request)
        {
            if (source is null)
            {
                return false;
            }
            if (UriCompare(new Uri(source.Url), request.Url))
            {
                return true;
            }
            string path = new Uri(source.Url).AbsolutePath.ToLower();
            if ((path == "/default.aspx" || path == "/") && UriCompare(request.Url, new Uri(HomePage)))
            {
                return true;
            }

            return false;
        }



    }
}
