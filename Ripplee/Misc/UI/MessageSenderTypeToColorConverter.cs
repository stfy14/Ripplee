using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics; // Для Colors
using Ripplee.Models; // Для MessageSenderType

namespace Ripplee.Misc.UI
{
    public class MessageSenderTypeToColorConverter : IValueConverter
    {
        public Color CurrentUserMessageColor { get; set; } = Color.FromArgb("#FF007ACC"); // Синий для своих
        public Color CompanionMessageColor { get; set; } = Color.FromArgb("#FF333333"); // Темно-серый для собеседника

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageSenderType senderType)
            {
                return senderType == MessageSenderType.CurrentUser ? CurrentUserMessageColor : CompanionMessageColor;
            }
            return CompanionMessageColor;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}