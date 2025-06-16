using System.Globalization;
using Ripplee.Misc;

namespace Ripplee.Misc.UI
{
    [Preserve(AllMembers = true)]
    public class StepToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is int currentStep && parameter is string targetStepStr)
            {
                return currentStep == int.Parse(targetStepStr);
            }
            return false;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}