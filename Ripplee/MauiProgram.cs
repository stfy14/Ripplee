using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Microsoft.Maui.Handlers; // Для PickerHandler

#if ANDROID
// Эти using'и могут понадобиться для работы с нативными типами Android, если будете делать более сложную кастомизацию.
// Для h.PlatformView.Background = null; они обычно не требуются.
// using Android.Graphics.Drawables;
// using Microsoft.Maui.Controls.Platform;
#endif

namespace Ripplee // Убедитесь, что это ваш правильный namespace
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
                    Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) => // handler здесь это PickerHandler, view это Picker
                    {
                        if (handler.PlatformView != null) // PlatformView для Picker на Android это Android.Widget.EditText
                        {
                            handler.PlatformView.Background = null;
                        }
                    });
#endif
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif
            return builder.Build();
        }
    }
}