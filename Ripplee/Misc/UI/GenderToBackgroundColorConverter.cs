using System;
using System.Globalization;
using Microsoft.Maui.Graphics;
using Microsoft.Maui.Controls;

namespace Ripplee.Misc.UI
{
    public class GenderToBackgroundColorConverter : IValueConverter
    {
        public Color MaleActiveColor { get; set; } = Color.FromArgb("#2c3e50");     // Активный: Яркий синий
        public Color MaleInactiveColor { get; set; } = Color.FromArgb("#2a2a2a");   // Неактивный: Темный сине-серый

        public Color FemaleActiveColor { get; set; } = Color.FromArgb("#4e2b4f");   // Активный: Яркий розовый
        public Color FemaleInactiveColor { get; set; } = Color.FromArgb("#2a2a2a"); // Неактивный: Глубокий бордовый

        private readonly Color _fallbackColor = Color.FromArgb("#2a2a2a");


        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string selectedGender || parameter is not string buttonGender)
            {
                return _fallbackColor;
            }

            bool isButtonSelected = selectedGender.Equals(buttonGender, StringComparison.OrdinalIgnoreCase);

            switch (buttonGender.ToLower())
            {
                case "мужчина":
                    return isButtonSelected ? MaleActiveColor : MaleInactiveColor;

                case "женщина":
                    return isButtonSelected ? FemaleActiveColor : FemaleInactiveColor;

                default:
                    return _fallbackColor;
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}