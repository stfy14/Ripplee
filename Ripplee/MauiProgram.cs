// Файл: MauiProgram.cs
using Microsoft.Extensions.Logging;
using CommunityToolkit.Maui;
using Ripplee.ViewModels;
using Ripplee.Views;
using Ripplee.Models; 
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
        private static string GetApiBaseAddress()
        {
            if (DeviceInfo.Platform == DevicePlatform.Android)
                return "https://10.0.2.2:7042"; // УБЕДИТЕСЬ, ЧТО ПОРТ ВЕРНЫЙ

            if (DeviceInfo.Platform == DevicePlatform.WinUI)
                return "https://localhost:7042"; // УБЕДИТЕСЬ, ЧТО ПОРТ ВЕРНЫЙ

            return "https://localhost:7042";
        }

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

            // --- НАЧАЛО БЛОКА ИЗМЕНЕНИЙ ---

            // Регистрируем HttpMessageHandler, который будет игнорировать ошибки
            // SSL-сертификата ТОЛЬКО в режиме отладки.
#if DEBUG
            builder.Services.AddSingleton<HttpClientHandler>(_ => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            });
#else
            builder.Services.AddSingleton<HttpClientHandler>();
#endif

            // Регистрируем HttpClient и говорим ему использовать наш настроенный handler
            builder.Services.AddSingleton(serviceProvider =>
            {
                var handler = serviceProvider.GetRequiredService<HttpClientHandler>();
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(GetApiBaseAddress())
                };
            });

            // --- КОНЕЦ БЛОКА ИЗМЕНЕНИЙ ---

            // --- Секция Dependency Injection ---
            builder.Services.AddSingleton<ChatApiClient>();
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<VoiceChatViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();

            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<VoiceChatPage>();
            builder.Services.AddTransient<OnboardingPage>();

            builder.Services.AddTransient<AppShell>();
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