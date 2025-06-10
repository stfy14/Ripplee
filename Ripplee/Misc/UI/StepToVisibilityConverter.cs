using System.Globalization;
using Ripplee.Misc;

namespace Ripplee.Misc.UI
{
    [Preserve(AllMembers = true)]
    public class StepToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // value - это наш CurrentStepIndex из ViewModel (int)
            // parameter - это номер шага, который мы передаем из XAML (string)
            if (value is int currentStep && parameter is string targetStepStr)
            {
                // Сравниваем текущий шаг с целевым. Если они равны, возвращаем true (блок будет виден).
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