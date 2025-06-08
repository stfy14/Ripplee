// Файл: MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Ripplee.ViewModels;
using Ripplee.Views;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services;
using Microsoft.Maui.Handlers; 

#if ANDROID
using Android.Widget; // <-- И ВОТ ЭТА СТРОКА нужна для EditText
#endif

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
                .UseMauiCommunityToolkit();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            // --- Секция Dependency Injection ---
            builder.Services.AddSingleton<IChatService, ChatService>();
            builder.Services.AddSingleton<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddSingleton<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<AuthPage>();
            // --- Конец секции DI ---

            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                // Теперь этот код не будет вызывать ошибок
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
            });

            return builder.Build();
        }
    }
}