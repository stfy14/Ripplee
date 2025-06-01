// Файл: Ripplee/Misc/UI/GenderToBackgroundColorConverter.cs
// (Если ты решил переименовать GenderToColorConverter.cs)
// или продолжай вносить изменения в существующий GenderToColorConverter.cs

using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Ripplee.Misc.UI
{
    public class GenderToBackgroundColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string selectedGender && parameter is string buttonGender)
            {
                return selectedGender == buttonGender ? Color.FromArgb("#2c3e50") : Color.FromArgb("#2a2a2a");
            }
            return Color.FromArgb("#2a2a2a"); 
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}