// Файл: MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Ripplee.ViewModels;
using Ripplee.Views;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services;
using Microsoft.Maui.Handlers;
using Ripplee.Services.Data;
using Microsoft.Extensions.Http;

#if ANDROID
using Android.Widget; // <-- Для EditText (используется Entry и Picker)
using Android.Graphics.Drawables; // Может понадобиться для более тонкой настройки фона
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

            builder.Services.AddHttpClient<ChatApiClient>(client =>
            {
                // В будущем здесь будет базовый адрес твоего API
                // client.BaseAddress = new Uri("https://api.ripplee.com/");
            });

            // Регистрируем сервисы
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            // Регистрируем ViewModel'и
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<VoiceChatViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();

            // Регистрируем страницы
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<VoiceChatPage>();
            builder.Services.AddTransient<OnboardingPage>();
            // --- Конец секции DI ---

            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                // Кастомизация для Picker (ваш существующий код)
                PickerHandler.Mapper.AppendToMapping("CustomPickerAppearance", (handler, view) =>
                {
                    if (handler.PlatformView is EditText editText && view is Picker mauiPicker)
                    {
                        editText.Background = null; // Убираем фон (и подчеркивание)

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

                EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    if (handler.PlatformView is EditText editText) // Entry на Android это EditText
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