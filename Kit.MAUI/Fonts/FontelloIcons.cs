﻿using Kit.MAUI.Fonts;
using Microsoft.Maui;using Microsoft.Maui.Controls;
using System;

[assembly: ExportFont(fontFileName: FontelloIcons.FontResourceName, Alias = FontelloIcons.Font)]
namespace Kit.MAUI.Fonts
{
    public static class FontelloIcons
    {
        public const string Ok = "\uE803";
        public const string Cross = "\uE804";
        public const string Camera = "\uE800";
        public const string RightArrow = "\uF105";
        public const string ThreeDots = "\uF0C9";
        public const string Font = "KitFontIcons";
        public const string FontResourceName = "kiticons4.ttf";
    }
}
