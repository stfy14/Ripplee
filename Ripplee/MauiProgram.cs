// Файл: MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers;

#if ANDROID
using Android.Widget; 
#endif

// Using'и для Windows и iOS пока не трогаем, если там не делаем специфичных смещений

namespace Ripplee
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                })
                .UseMauiCommunityToolkit()
                .ConfigureMauiHandlers(handlers =>
                {
#if ANDROID
                    PickerHandler.Mapper.AppendToMapping("CustomPickerAppearance", (handler, view) => 
                    {
                        if (handler.PlatformView is EditText editText && view is Picker mauiPicker)
                        {
                            editText.Background = null;

                            var context = editText.Context;
                            if (context?.Resources?.DisplayMetrics != null)
                            {
                                float density = context.Resources.DisplayMetrics.Density;
                                
                                int topPadding = (int)(12 * density);   
                                int leftPadding = (int)(16 * density);                               
                                int rightPadding = editText.PaddingRight; 
                                int bottomPadding = editText.PaddingBottom; 

                                editText.SetPadding(leftPadding, topPadding, rightPadding, bottomPadding);
                            }
                        }
                    });
#endif
                    // Здесь могут быть обработчики для других платформ, если решим их добавлять
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}