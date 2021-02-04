﻿using Android.Views;
using Kit.Services.Interfaces;
using Plugin.CurrentActivity;
using System;
using System.Collections.Generic;
using System.Text;

namespace Kit.Droid.Services
{
    public class BrightnessService : IBrightnessService
    {
        public void SetBrightness(float brightness)
        {
            Window? window = CrossCurrentActivity.Current.Activity.Window;
            WindowManagerLayoutParams attributesWindow = new WindowManagerLayoutParams();

            attributesWindow.CopyFrom(window.Attributes);
            attributesWindow.ScreenBrightness = brightness;

            window.Attributes = attributesWindow;
        }
        public float GetBrightness()
        {
            Window? window = CrossCurrentActivity.Current.Activity.Window;
            WindowManagerLayoutParams attributesWindow = new WindowManagerLayoutParams();

            attributesWindow.CopyFrom(window.Attributes);
            return attributesWindow.ScreenBrightness;
        }
    }
}
