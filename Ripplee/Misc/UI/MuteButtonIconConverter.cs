using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Ripplee.Misc.UI
{
    public class MuteButtonIconConverter : IValueConverter
    {
        public string MicOnIconSource { get; set; } = "mic_on_icon.svg";
        public string MicOffIconSource { get; set; } = "mic_off_icon.svg";

        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isMuted)
            {
                return isMuted ? MicOffIconSource : MicOnIconSource;
            }
            return MicOnIconSource; 
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}