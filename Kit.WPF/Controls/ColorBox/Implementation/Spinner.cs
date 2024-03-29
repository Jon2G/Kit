﻿/*************************************************************************************

   Extended WPF Toolkit

   Copyright (C) 2007-2013 Xceed Software Inc.

   This program is provided to you under the terms of the Microsoft Public
   License (Ms-PL) as published at http://wpftoolkit.codeplex.com/license 

   For more features, controls, and fast professional support,
   pick up the Plus Edition at http://xceed.com/wpf_toolkit

   Stay informed: follow @datagrid on Twitter or Like http://facebook.com/datagrids

  ***********************************************************************************/

using System;
using System.Windows;
using System.Windows.Controls;

namespace Kit.WPF.Controls.ColorBox.Implementation
{
    internal class Spinner : ContentControl
    {
        static Spinner()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Spinner), new FrameworkPropertyMetadata(typeof(Spinner)));
        }

        public static readonly DependencyProperty ValidSpinDirectionProperty = DependencyProperty.Register("ValidSpinDirection", typeof(ValidSpinDirections), typeof(Spinner), new PropertyMetadata(ValidSpinDirections.Increase | ValidSpinDirections.Decrease, OnValidSpinDirectionPropertyChanged));
        public ValidSpinDirections ValidSpinDirection
        {
            get => (ValidSpinDirections)GetValue(ValidSpinDirectionProperty);
            set => SetValue(ValidSpinDirectionProperty, value);
        }
        public static void OnValidSpinDirectionPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }

        public event EventHandler<SpinEventArgs> Spin; 
        protected virtual void OnSpin(SpinEventArgs e)
        {
            Spin?.Invoke(this, e);
        }
    }
}
