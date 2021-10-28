﻿using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using AsyncAwaitBestPractices.MVVM;
using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Data;
using Kit.Extensions;
using Expression = System.Linq.Expressions.Expression;

namespace Kit.WPF.Controls
{
    public partial class ArrowButton : Button, INotifyPropertyChanged
    {
        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        ///[Obsolete("Use Raise para mejor rendimiento evitando la reflección")]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            handler?.Invoke(this, args);
        }
        #endregion

        #region PerfomanceHelpers
        public void Raise<T>(Expression<Func<T>> propertyExpression)
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

        public void Raise<T>(params Expression<Func<T>>[] propertyExpressions)
        {
            foreach (Expression<Func<T>> propertyExpression in propertyExpressions)
            {
                Raise<T>(propertyExpression);
            }
        }
        #endregion

        public static readonly DependencyProperty TextColorProperty = DependencyProperty.Register(
            name: nameof(TextColor),
            propertyType: typeof(Color),
            ownerType: typeof(ArrowButton), new PropertyMetadata(Colors.Black, (e, o) =>
              {
                  if (e is ArrowButton arrow)
                  {
                      Color color = (Color)o.NewValue;
                      arrow.TitleColor = color;
                      arrow.SubTitleColor = color;
                      arrow.TextColor = color;
                  }
              }));

        public Color TextColor
        {
            get => (Color)GetValue(TextColorProperty);
            set
            {
                SetValue(TextColorProperty, value);
                SetValue(SubTitleColorProperty, value);
                SetValue(TitleColorProperty, value);
                OnPropertyChanged();
                OnPropertyChanged(nameof(TitleColor));
                OnPropertyChanged(nameof(SubTitleColor));
            }
        }

        public static readonly DependencyProperty ArrowColorProperty = DependencyProperty.Register(
            name: nameof(ArrowColor), propertyType: typeof(Color),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(Colors.Blue, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.ArrowColor = (Color)o.NewValue;
            }));

        public Color ArrowColor
        {
            get => (Color)GetValue(ArrowColorProperty);
            set
            {
                SetValue(ArrowColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TitleColorProperty = DependencyProperty.Register(
            name: nameof(TitleColor), propertyType: typeof(Color),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(Colors.Black, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.TitleColor = (Color)o.NewValue;
            }));

        public Color TitleColor
        {
            get => (Color)GetValue(TitleColorProperty);
            set
            {
                SetValue(TitleColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty SubTitleColorProperty = DependencyProperty.Register(
            name: nameof(SubTitleColor), propertyType: typeof(Color),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(Colors.Black, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleColor = (Color)o.NewValue;
            }));

        public Color SubTitleColor
        {
            get => (Color)GetValue(SubTitleColorProperty);
            set
            {
                SetValue(SubTitleColorProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            name: nameof(Title), propertyType: typeof(string),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(string.Empty, (e, o) => { if (e is ArrowButton arrow) arrow.Title = o.NewValue?.ToString(); }));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set
            {
                SetValue(TitleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty SubTitleProperty = DependencyProperty.Register(
            name: nameof(SubTitle), propertyType: typeof(string),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(string.Empty, (e, o) => { if (e is ArrowButton arrow) arrow.SubTitle = o.NewValue?.ToString(); }));

        public string SubTitle
        {
            get => (string)GetValue(SubTitleProperty);
            set
            {
                SetValue(SubTitleProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty SubTitleFontSizeProperty = DependencyProperty.Register(
            name: nameof(SubTitleFontSize), propertyType: typeof(double),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(Colors.Black, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleFontSize = (double)o.NewValue;
            }));

        [TypeConverter(typeof(FontSizeConverter))]
        public double SubTitleFontSize
        {
            get => (double)GetValue(SubTitleFontSizeProperty);
            set
            {
                SetValue(SubTitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TitleFontSizeProperty = DependencyProperty.Register(
            name: nameof(TitleFontSize), propertyType: typeof(double),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(14d, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontSize = (double)o.NewValue;
            }));

        [TypeConverter(typeof(FontSizeConverter))]
        public double TitleFontSize
        {
            get => (double)GetValue(TitleFontSizeProperty);
            set
            {
                SetValue(TitleFontSizeProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty TitleFontAttributesProperty = DependencyProperty.Register(
            name: nameof(TitleFontAttributes), propertyType: typeof(FontWeight),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(FontWeights.Bold, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontAttributes = (FontWeight)o.NewValue;
            }));

        public FontWeight TitleFontAttributes
        {
            get { return (FontWeight)GetValue(TitleFontAttributesProperty); }
            set { SetValue(TitleFontAttributesProperty, value); OnPropertyChanged(); }
        }
        public static readonly DependencyProperty SubTitleFontAttributesProperty = DependencyProperty.Register(
            name: nameof(SubTitleFontAttributes), propertyType: typeof(FontWeight),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(FontWeights.Bold, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.SubTitleFontAttributes = (FontWeight)o.NewValue;
            }));
        public FontWeight SubTitleFontAttributes
        {
            get { return (FontWeight)GetValue(SubTitleFontAttributesProperty); }
            set { SetValue(SubTitleFontAttributesProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty TitleFontFamilyProperty = DependencyProperty.Register(
            name: nameof(TitleFontFamily), propertyType: typeof(string),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(string.Empty, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontFamily = o.NewValue?.ToString();
            }));

        public string TitleFontFamily
        {
            get { return (string)GetValue(TitleFontFamilyProperty); }
            set { SetValue(TitleFontFamilyProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty SubtitleFontFamilyProperty = DependencyProperty.Register(
            name: nameof(SubtitleFontFamily), propertyType: typeof(string),
            ownerType: typeof(ArrowButton),
             new PropertyMetadata(string.Empty, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.TitleFontFamily = o.NewValue?.ToString();
            }));

        public string SubtitleFontFamily
        {
            get { return (string)GetValue(SubtitleFontFamilyProperty); }
            set { SetValue(SubtitleFontFamilyProperty, value); OnPropertyChanged(); }
        }

        public static readonly DependencyProperty IconProperty = DependencyProperty.Register(
            name: nameof(Icon), propertyType: typeof(ImageSource), ownerType: typeof(ArrowButton),
             new PropertyMetadata(null, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.Icon = o.NewValue as ImageSource;
            }));

        [TypeConverter(typeof(ImageSourceConverter))]
        public ImageSource Icon
        {
            get => (ImageSource)GetValue(IconProperty);
            set
            {
                SetValue(CommandProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IconHeightRequestProperty = DependencyProperty.Register(
            name: nameof(IconHeightRequest), propertyType: typeof(double),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(20d, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.IconHeightRequest = (double)o.NewValue;
            }));

        public double IconHeightRequest
        {
            get => (double)GetValue(IconHeightRequestProperty);
            set
            {
                SetValue(IconHeightRequestProperty, value);
                OnPropertyChanged();
            }
        }

        public static readonly DependencyProperty IsArrowVisibleProperty = DependencyProperty.Register(
            name: nameof(IsArrowVisible), propertyType: typeof(bool),
            ownerType: typeof(ArrowButton), 
             new PropertyMetadata(true, (e, o) =>
            {
                if (e is ArrowButton arrow) arrow.IsArrowVisible = (bool)o.NewValue;
            }));

        public bool IsArrowVisible
        {
            get => (bool)GetValue(IsArrowVisibleProperty);
            set
            {
                SetValue(IsArrowVisibleProperty, value);
                OnPropertyChanged();
            }
        }

        public ICommand TouchedCommand { get; }

        public ArrowButton()
        {
            this.TouchedCommand = new Command(Touched);
            InitializeComponent();
        }

        private void Touched()
        {
            this.Command?.Execute(this.CommandParameter);
        }
    }
}