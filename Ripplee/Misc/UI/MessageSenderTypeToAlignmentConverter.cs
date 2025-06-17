using System;
using System.Globalization;
using Microsoft.Maui.Controls;
using Ripplee.Models; // Для MessageSenderType

namespace Ripplee.Misc.UI
{
    public class MessageSenderTypeToAlignmentConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is MessageSenderType senderType)
            {
                // Если параметр "Grid", то выравниваем весь Grid сообщения
                // иначе выравниваем сам бабл сообщения внутри Grid.Column="1"
                bool isGridAlignment = parameter is string paramStr && paramStr == "Grid";

                if (isGridAlignment)
                {
                    return senderType == MessageSenderType.CurrentUser ? LayoutOptions.End : LayoutOptions.Start;
                }
                else // Выравнивание бабла
                {
                    return senderType == MessageSenderType.CurrentUser ? LayoutOptions.End : LayoutOptions.Start;
                }
            }
            return LayoutOptions.Start;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => throw new NotImplementedException();
    }
}