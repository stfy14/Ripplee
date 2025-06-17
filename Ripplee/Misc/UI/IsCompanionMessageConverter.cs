using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Ripplee.Models;

namespace Ripplee.Misc.UI
{
    public class IsCompanionMessageConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is MessageSenderType type && type == MessageSenderType.Companion;
        }
        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}