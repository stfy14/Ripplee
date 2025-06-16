using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; 

namespace Ripplee.Misc.UI
{
    public class MuteButtonBackgroundColorConverter : IValueConverter
    {
        public Color MicOnBackgroundColor { get; set; } = Color.FromArgb("#2b2b2b");
        public Color MicOffBackgroundColor { get; set; } = Color.FromArgb("#4d3333");

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                return isMuted ? MicOffBackgroundColor : MicOnBackgroundColor;
            }
            return MicOnBackgroundColor; // По умолчанию
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}