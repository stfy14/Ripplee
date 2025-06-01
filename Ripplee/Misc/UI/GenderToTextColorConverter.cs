// Файл: Ripplee/Misc/UI/GenderToTextColorConverter.cs
using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Ripplee.Misc.UI
{
    public class GenderToTextColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Console.WriteLine($"Text Convert: value={value}, param={parameter}"); // Можно добавить для отладки
            if (value is string selectedGender && parameter is string buttonGender)
            {
                // Активная кнопка: белый текст
                // Неактивная кнопка: твой текст #7c7c7c
                return selectedGender == buttonGender ? Colors.White : Color.FromArgb("#7c7c7c");
            }
            return Color.FromArgb("#7c7c7c"); // Цвет по умолчанию для текста неактивной кнопки
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}