using CommunityToolkit.Maui.Views;
using Kit.Services.Interfaces;
using System.Windows.Input;

namespace Kit.Forms.Pages
{
    public class BasePopUp : Popup, ICrossWindow
    {

        #region ICrossWindow

        Task ICrossWindow.Close() => Close();

        Task ICrossWindow.Show() => Show(Microsoft.Maui.Controls.Application.Current.MainPage);

        Task ICrossWindow.ShowDialog() => ShowDialog();

        #endregion ICrossWindow

        public ICommand ClosedCommad;
        private readonly AutoResetEvent ShowDialogCallback;

        public BasePopUp()
        {
            //this.Background = Color.Transparent;
            //this.BackgroundColor = Color.Transparent;
            this.ShowDialogCallback = new AutoResetEvent(false);
            //this.Visual = VisualMarker.Material;
        }

        public virtual async Task<BasePopUp> ShowDialog(Page page = null)
        {
            await Show(page);
            await Task.Run(() => this.ShowDialogCallback.WaitOne());
            return this;
        }

        public virtual async Task<BasePopUp> Show(Page page)
        {
            page ??= Microsoft.Maui.Controls.Application.Current.MainPage;
            return await page.ShowPopupAsync(this).ContinueWith(t =>
            {
                return this;
            });
        }

        public virtual async Task<BasePopUp> Close()
        {
            await Task.Yield();
            Closing();
            await CloseAsync(this);
            this.ShowDialogCallback.Set();
            ClosedCommad?.Execute(this);
            return this;
        }

        protected virtual void Closing()
        {
        }

        private bool IsModalLocked { get; set; }
        public bool IsVisible { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public BasePopUp LockModal()
        {
            this.IsModalLocked = !this.IsModalLocked;
            this.CanBeDismissedByTappingOutsideOfPopup = !this.IsModalLocked;
            return this;
        }

        //protected override bool OnBackButtonPressed()
        //{
        //    if (this.IsModalLocked)
        //    {
        //        return true;
        //    }
        //    else
        //    {
        //        return false;
        //    }
        //}

        //public async void BackButtonPressed()
        //{
        //    if (!OnBackButtonPressed())
        //    {
        //        await this.Close();
        //    }
        //}
        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    CrossOnAppearing();
        //}
        public virtual void CrossOnAppearing()
        {

        }
        protected override void OnPropertyChanged(string propertyName = null)
        {
            //if (propertyName == "Background" || propertyName == "BackgroundColor")
            //{
            //    this.Background = null;
            //}
            base.OnPropertyChanged(propertyName);
        }
    }
}