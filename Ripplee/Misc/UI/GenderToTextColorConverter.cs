using System.Globalization;

namespace Ripplee.Misc.UI
{
    public class GenderToTextColorConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is string selectedGender && parameter is string buttonGender)
            {
                return selectedGender == buttonGender ? Colors.White : Color.FromArgb("#7c7c7c");
            }
            return Color.FromArgb("#7c7c7c"); 
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return null;
        }
    }
}