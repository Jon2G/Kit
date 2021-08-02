﻿using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Kit
{
    public class EnterKeyTraversal
    {
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void ue_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            FrameworkElement ue = e.OriginalSource as FrameworkElement;

            if (e.Key == Key.Enter)
            {
                e.Handled = true;
                if (ue.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next)))
                {
                    OpenFocused(Keyboard.FocusedElement);
                }
            }
        }

        private static void OpenFocused(IInputElement focusedElement)
        {
            switch (focusedElement)
            {
                case TextBox txt:
                    return;

                case ComboBox combo:
                    //combo.IsDropDownOpen = true;
                    return;
            }
        }

        private static void ue_Unloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement ue = sender as FrameworkElement;
            if (ue == null) return;

            ue.Unloaded -= ue_Unloaded;
            ue.PreviewKeyDown -= ue_PreviewKeyDown;
        }

        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool),

            typeof(EnterKeyTraversal), new UIPropertyMetadata(false, IsEnabledChanged));

        private static void IsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Atach(d, (bool)e.NewValue);
        }

        private static void Atach(DependencyObject d, bool NewValue)
        {
            FrameworkElement ue = d as FrameworkElement;
            if (ue == null) return;

            if (NewValue)
            {
                ue.Loaded += Ue_Loaded;

                ue.Unloaded += ue_Unloaded;
                ue.PreviewKeyDown += ue_PreviewKeyDown;
            }
            else
            {
                ue.Loaded -= Ue_Loaded;
                ue.Unloaded -= ue_Unloaded;
                ue.PreviewKeyDown -= ue_PreviewKeyDown;
            }
        }

        private static void Ue_Loaded(object sender, RoutedEventArgs e)
        {
            Atach(sender as DependencyObject, true);
        }
    }
}