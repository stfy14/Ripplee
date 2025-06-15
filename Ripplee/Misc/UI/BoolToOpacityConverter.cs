using System.Globalization;

namespace Ripplee.Misc.UI
{
    public class BoolToOpacityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Если значение true, возвращаем 1 (полностью видимый).
            // Если false - возвращаем 0 (полностью прозрачный).
            return (value is bool b && b) ? 1 : 0;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}