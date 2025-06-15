// Файл: MauiProgram.cs
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services;
using Ripplee.ViewModels;
using Ripplee.Views;
#if ANDROID
using Microsoft.Maui.LifecycleEvents;
using Android.Graphics.Drawables;
using Android.Widget;
using Microsoft.Maui.Handlers;
#endif

namespace Ripplee
{
    public static class MauiProgram
    {
        public static string GetApiBaseAdress()
        {
            // Используем http для локального дебага, так как настроили Android
            // на прием cleartext-трафика для нашего IP.
#if DEBUG
#if ANDROID
            return "http://10.0.2.2:5142";
#else
            // Для Windows или других платформ, если сервер запущен на той же машине
            return "http://localhost:5142";
#endif

#else
    // Адрес вашего реального сервера для Release-сборки
    return "http://91.192.168.52";
#endif
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

            // --- Блок регистрации HTTP-клиента (без изменений) ---
            builder.Services.AddHttpClient<ChatApiClient>(client =>
            {
                client.BaseAddress = new Uri(GetApiBaseAdress());
            })
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            })
#endif
            ;

            // --- Секция Dependency Injection (без изменений) ---
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IChatService, ChatService>();
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<VoiceChatViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();
            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<VoiceChatPage>();
            builder.Services.AddTransient<OnboardingPage>();
            builder.Services.AddTransient<AppShell>();
            builder.Services.AddTransient<SearchingPage>();
            builder.Services.AddTransient<SearchingViewModel>();

            // --- Кастомизация нативных контролов (без изменений) ---
            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                PickerHandler.Mapper.AppendToMapping("CustomPickerAppearance", (handler, view) =>
                {
                    if (handler.PlatformView is EditText editText)
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

                EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
                {
                    if (handler.PlatformView is EditText editText)
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