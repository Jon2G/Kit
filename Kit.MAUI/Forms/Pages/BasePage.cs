using Kit.Enums;
using Kit.Services.Interfaces;
using System;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace Kit.Forms.Pages
{
    public class BasePage : ContentPage, INotifyPropertyChanged, IDisposable, ICrossWindow, ICrossVisualElement
    {
        #region IDisposable

        public virtual void Dispose()
        {
            DisposeBindingContext();
        }
        public virtual void OnClose() { }
        protected override void OnParentSet()
        {
            base.OnParentSet();
            if (Parent == null)
            {
                OnClose();
                this.ShowDialogCallback?.Set();
                DisposeBindingContext();
            }
        }

        protected void DisposeBindingContext()
        {
            if (DisposeAlso != null)
            {
                foreach (IDisposable disposable in DisposeAlso)
                {
                    disposable?.Dispose();
                }
            }
            if (BindingContext != this && BindingContext is IDisposable disposableBindingContext)
            {
                disposableBindingContext.Dispose();
                BindingContext = null;
            }
        }

        protected virtual IDisposable[] DisposeAlso { get; }

        ~BasePage()
        {
            DisposeBindingContext();
        }

        #endregion IDisposable

        #region ICrossWindow

        public virtual Task Close()
        {
            if (Shell.Current is Shell shell)
            {
                return shell.Navigation.PopAsync();
            }
            return Navigation.PopModalAsync();
        }

        public virtual Task Show()
        {
            if (Shell.Current is Shell shell)
            {
                return shell.Navigation.PushAsync(this);
            }
            return Navigation.PushModalAsync(this);
        }

        public virtual async Task ShowDialog() => await Task.WhenAll(Show(), WaitUntilClose());

        #endregion ICrossWindow

        public object Auxiliar { get; set; }
        private AutoResetEvent ShowDialogCallback;

        public class PageOrientationEventArgs : EventArgs
        {
            public PageOrientationEventArgs(PageOrientation orientation)
            {
                Orientation = orientation;
            }

            public PageOrientation Orientation { get; }
        }

        public enum PageOrientation
        {
            Horizontal = 0,
            Vertical = 1,
        }

        private double _width;
        private double _height;

        public event EventHandler<PageOrientationEventArgs> OnOrientationChanged = (e, a) => { };

        private void InitOrientationPage()
        {
            _width = Width;
            _height = Height;
        }

        //protected override void OnSizeAllocated(double width, double height)
        //{
        //    try
        //    {
        //        double oldWidth = _width;
        //        const double sizenotallocated = -1;

        //        base.OnSizeAllocated(width, height);
        //        if (Equals(_width, width) && Equals(_height, height)) return;

        //        _width = width;
        //        _height = height;

        //        // ignore if the previous height was size unallocated
        //        if (Equals(oldWidth, sizenotallocated)) return;

        //        // Has the device been rotated ?
        //        if (!Equals(width, oldWidth))
        //            OnOrientationChanged.Invoke(this, new PageOrientationEventArgs(width < height ? PageOrientation.Vertical : PageOrientation.Horizontal));
        //    }
        //    catch (Exception e)
        //    {
        //        Log.Logger.Error(e, nameof(OnSizeAllocated));
        //    }

        //}
        public PageOrientation ActualOrientation
        {
            get => Width < Height ? PageOrientation.Vertical : PageOrientation.Horizontal;
        }

        public BasePage()
        {
            this.Visual = new VisualMarker.MaterialVisual();
            LockedOrientation = DeviceOrientation.Other;
            IsModalLocked = false;
            InitOrientationPage();
        }

        public DeviceOrientation LockedOrientation { get; private set; }

        protected BasePage LockOrientation(DeviceOrientation Orientation)
        {
            LockedOrientation = Orientation;
            if (Device.RuntimePlatform == Device.Android)
            {
                MessagingCenter.Send(this, LockedOrientation.ToString());
            }
            return this;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            CrossOnAppearing();
            if (HasAppeared)
            {
                return;
            }
            OnFirstAppearing();
            HasAppeared = true;
        }
        public virtual void CrossOnAppearing()
        {

        }
        public virtual void OnSleep() { }

        private bool HasAppeared;

        protected virtual void OnFirstAppearing()
        {
            Log.Logger.Debug("OnFirstAppearing - {0}", this);
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            if (LockedOrientation != DeviceOrientation.Other)
            {
                if (Device.RuntimePlatform == Device.Android)
                {
                    MessagingCenter.Send(this, nameof(DeviceOrientation.Other));
                }
                else if (Device.RuntimePlatform == Device.iOS)
                {
                }
            }
        }

        protected bool IsModalLocked { get; private set; }

        public BasePage LockModal()
        {
            IsModalLocked = !IsModalLocked;
            return this;
        }

        protected override bool OnBackButtonPressed()
        {
            if (Rg.Plugins.Popup.Services.PopupNavigation.Instance.PopupStack.LastOrDefault() is BasePopUp popUp)
            {
                popUp.BackButtonPressed();
                return true;
            }
            if (IsModalLocked)
            {
                return true;
            }
            bool close = base.OnBackButtonPressed();
            if (!close)
            {
                OnClose();
                ShowDialogCallback?.Set();
            }
            return close;
        }

        public async void MenuPrincipal()
        {
            await Navigation.PopToRootAsync(true);
        }

        public void SetScreenMode(ScreenMode Screen)
        {
            Kit.Tools.Instance.ScreenManager.SetScreenMode(Screen);
        }

        public async Task<BasePage> WaitUntilClose()
        {
            if (ShowDialogCallback is null)
            {
                this.ShowDialogCallback = new AutoResetEvent(false);
            }
            await Task.Run(() => this.ShowDialogCallback.WaitOne());
            return this;
        }

        #region INotifyPropertyChanged

        public new event PropertyChangedEventHandler PropertyChanged;

        //[Obsolete("Use Raise para mejor rendimiento evitando la reflección")]
        protected new void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        private void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }

        #endregion INotifyPropertyChanged

        #region PerfomanceHelpers

        protected void Raise<T>(Expression<Func<T>> propertyExpression)
        {
            if (this.PropertyChanged != null)
            {
                MemberExpression body = propertyExpression.Body as MemberExpression;
                if (body == null)
                    throw new ArgumentException("'propertyExpression' should be a member expression");

                ConstantExpression expression = body.Expression as ConstantExpression;
                if (expression == null)
                    throw new ArgumentException("'propertyExpression' body should be a constant expression");

                object target = Expression.Lambda(expression).Compile().DynamicInvoke();

                PropertyChangedEventArgs e = new PropertyChangedEventArgs(body.Member.Name);
                PropertyChanged(target, e);
            }
        }

        protected void Raise<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (Expression<Func<T>> propertyExpression in propertyExpressions)
            {
                Raise<T>(propertyExpression);
            }
        }

        #endregion PerfomanceHelpers
    }
}
