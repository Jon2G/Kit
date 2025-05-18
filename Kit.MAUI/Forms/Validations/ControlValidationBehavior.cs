using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using Xamarin.Forms;
using StackLayout = Xamarin.Forms.StackLayout;

namespace Kit.Forms.Validations
{
    [Preserve(AllMembers = true)]
    public class ControlValidationBehavior : Behavior<View>, IControlValidation
    {
        private View View { get; set; }

        #region Property
        private INotifyDataErrorInfo _NotifyErrors;
        private string BindingPath = "";
        private INotifyScrollToProperty _NotifyScroll;
        #endregion
        #region BindableProperty
        public static readonly BindableProperty BindablePropertyProperty =
            BindableProperty.Create(nameof(BindableProperty), typeof(BindableProperty),
                typeof(ControlValidationBehavior), null,
                defaultBindingMode: BindingMode.TwoWay,
                propertyChanged: OnBindablePropertyChanged);

        private static void OnBindablePropertyChanged(BindableObject bindable, object oldvalue, object newvalue)
        {
            // execute on bindable context changed method
            ControlValidationBehavior control = bindable as ControlValidationBehavior;
            if (control != null && control.View?.BindingContext != null)
            {
                control.CheckValidation();
            }
        }

        public BindableProperty BindableProperty
        {
            get { return (BindableProperty)GetValue(BindablePropertyProperty); }
            set { SetValue(BindablePropertyProperty, value); OnPropertyChanged(); }
        }
        #endregion
        #region Has Error
        public static readonly BindableProperty HasErrorProperty =
            BindableProperty.Create("HasError", typeof(bool),
                typeof(ControlValidationBehavior), false, defaultBindingMode: BindingMode.TwoWay);

        public bool HasError
        {
            get { return (bool)GetValue(HasErrorProperty); }
            private set { SetValue(HasErrorProperty, value); }
        }
        #endregion

        #region ErrorMessage

        public static readonly BindableProperty ErrorMessageProperty =
           BindableProperty.Create("ErrorMessage", typeof(string), typeof(ControlValidationBehavior), string.Empty);

        public string ErrorMessage
        {
            get { return (string)GetValue(ErrorMessageProperty); }
            set { SetValue(ErrorMessageProperty, value); }
        }
        #endregion

        #region ShowErrorMessage

        public static readonly BindableProperty ShowErrorMessageProperty =
           BindableProperty.Create("ShowErrorMessage", typeof(bool), typeof(ControlValidationBehavior), false, propertyChanged: OnShowErrorMessageChanged, defaultBindingMode: BindingMode.TwoWay);

        private static void OnShowErrorMessageChanged(BindableObject bindable, object oldValue, object newValue)
        {
            // execute on bindable context changed method
            ControlValidationBehavior control = bindable as ControlValidationBehavior;
            if (control != null && control.View?.BindingContext != null)
            {
                control.CheckValidation();
            }
        }

        public bool ShowErrorMessage
        {
            get { return (bool)GetValue(ShowErrorMessageProperty); }
            set { SetValue(ShowErrorMessageProperty, value); }
        }
        #endregion

        #region WrapView

        protected virtual void WrapView()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                Xamarin.Forms.Layout<View> OriginalParent = View.Parent as Xamarin.Forms.Layout<View>;
                int Index = OriginalParent.Children.FindIndexOf(x => x == View);
                Label alertLabel = new Label()
                {
                    TextColor = Color.Red,
                    FontSize = 14
                };
                alertLabel.SetValue(Label.BindingContextProperty, this);
                alertLabel.SetBinding(Label.TextProperty, path: nameof(ErrorMessage));
                alertLabel.SetBinding(Label.IsVisibleProperty, path: nameof(HasError));

                StackLayout stack = new StackLayout() { Spacing = 0, Padding = 0 };
                OriginalParent.Children.RemoveAt(Index);
                stack.Children.Add(View);
                stack.Children.Add(alertLabel);
                OriginalParent.Children.Insert(Index, stack);
            });
        }
        #endregion

        #region Behavior




        #endregion

        private void SuscribeOnParentSet(bool handle)
        {
            // Find the handler method
            MethodInfo method = typeof(ControlValidationBehavior).GetMethod(nameof(OnParentSet),
                BindingFlags.NonPublic | BindingFlags.Instance);
            // Subscribe to the event
            EventInfo eventInfo = View.GetType().GetEvent("ParentSet", BindingFlags.Instance | BindingFlags.NonPublic);
            Type type = eventInfo.EventHandlerType;
            Delegate handler = Delegate.CreateDelegate(type, this, method);
            MethodInfo addMethod = handle ? eventInfo.GetAddMethod(true) : eventInfo.GetRemoveMethod(true);
            addMethod.Invoke(View, new[] { handler });
        }
        protected override void OnAttachedTo(BindableObject bindable)
        {
            base.OnAttachedTo(bindable);
            bindable.BindingContextChanged += this.Bindable_BindingContextChanged;
            View = bindable as View;
            SuscribeOnParentSet(true);
            CheckValidation();
        }

        void OnParentSet(object sender, EventArgs e)
        {
            SuscribeOnParentSet(false);
            WrapView();
        }

        protected override void OnDetachingFrom(View bindable)
        {
            base.OnDetachingFrom(bindable);
            bindable.BindingContextChanged -= this.Bindable_BindingContextChanged;
            SuscribeOnParentSet(false);
            if (View == bindable)
            {
                bindable = null;
            }
        }

        #region Override Binding context change property
        private void Bindable_BindingContextChanged(object sender, EventArgs e)
        {
            CheckValidation();
        }


        /// <summary>
        /// Method will subscibe and unsubsribe Error changed event
        /// Get bindable property path of text property
        /// </summary>
        public void CheckValidation()
        {
            // Reset variables values
            ErrorMessage = "";
            HasError = false;
            BindingPath = "";

            if (_NotifyErrors != null)
            {
                // Unsubscribe event
                _NotifyErrors.ErrorsChanged += _NotifyErrors_ErrorsChanged;
                _NotifyErrors = null; // Set null value on binding context change          
            }

            // Remove notify scroll to property
            if (_NotifyScroll != null)
            {
                _NotifyScroll.ScrollToProperty -= NotifyScroll_ScrollToProperty;
                _NotifyScroll = null;
            }

            // Do nothing if show error message property value is false
            if (!this.ShowErrorMessage || this.View is null)
                return;

            if (this.View.BindingContext != null && this.View.BindingContext is INotifyDataErrorInfo)
            {
                // Get 
                _NotifyErrors = this.View.BindingContext as INotifyDataErrorInfo;

                if (_NotifyErrors == null)
                    return;

                // Subscribe event
                _NotifyErrors.ErrorsChanged += _NotifyErrors_ErrorsChanged;

                // Remove notify scroll to property
                if (this.View.BindingContext is INotifyScrollToProperty)
                {
                    _NotifyScroll = this.View.BindingContext as INotifyScrollToProperty;
                    _NotifyScroll.ScrollToProperty += NotifyScroll_ScrollToProperty;
                }

                this.BindingPath = this.View.GetBindingPath(BindableProperty);
                SetPlaceHolder();

            }
        }

        // Scroll to control when request for scroll to property
        private void NotifyScroll_ScrollToProperty(string PropertyName)
        {
            // If property is requested property
            if (this.BindingPath.Equals(PropertyName))
            {
                // Get scroll 
                ScrollView _scroll = this.View.FindParent<ScrollView>();
                _scroll?.ScrollToAsync(this.View, ScrollToPosition.Center, true);
            }
        }

        /// <summary>
        /// Method will fire on property changed
        /// Check validation of text property
        /// Set validation if any validation message on property changed event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _NotifyErrors_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            // Error changed
            if (e.PropertyName.Equals(this.BindingPath))
            {
                // Get errors
                string errors = _NotifyErrors
                            .GetErrors(e.PropertyName)
                            ?.Cast<string>()
                            .FirstOrDefault();

                // If has error
                // assign validation values
                if (!string.IsNullOrEmpty(errors))
                {
                    HasError = true; //set has error value to true
                    ErrorMessage = errors; // assign error
                }
                else
                {
                    // reset error message and flag
                    HasError = false;
                    ErrorMessage = "";
                }
            }
        }

        private void SetPlaceHolder()
        {
            if (!string.IsNullOrEmpty(BindingPath) && this.View.BindingContext != null)
            {
                // Get display attributes
                var _attributes = this.View.BindingContext.GetType()
                    .GetRuntimeProperty(BindingPath)
                    .GetCustomAttribute<DisplayAttribute>();

            }
        }
        #endregion

    }
}
