using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Ripplee.Misc.UI
{
    public class IsNotNullToBoolConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value != null;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}