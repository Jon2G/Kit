﻿using System;
using System.Globalization;
using Microsoft.Maui;using Microsoft.Maui.Controls;

namespace Kit.MAUI.Converters
{
    public class IsEqualConverter : IValueConverter
    {
        public bool IsReversed { get; set; }
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool r = value.Equals(parameter);
            return IsReversed ? !r : r;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}