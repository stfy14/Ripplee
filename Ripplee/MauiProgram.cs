// Файл: MauiProgram.cs
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Ripplee.Services.Data;
using Ripplee.Services.Interfaces;
using Ripplee.Services.Services;
using Ripplee.ViewModels;
using Ripplee.Views;

#if ANDROID
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

            // --- НАЧАЛО БЛОКА РЕГИСТРАЦИИ HTTP-КЛИЕНТА (ИЗМЕНЕНО) ---

            // Регистрируем ChatApiClient как "типизированный клиент".
            // Это современный подход, который использует IHttpClientFactory для управления
            // жизненным циклом HttpClient и его обработчиков.
            builder.Services.AddHttpClient<ChatApiClient>(client =>
            {
                // Устанавливаем базовый адрес для всех запросов этого клиента
                client.BaseAddress = new Uri(GetApiBaseAdress());
            })
            // В режиме DEBUG мы добавляем специальный обработчик,
            // который разрешает использование самоподписанных или невалидных SSL-сертификатов.
            // ВАЖНО: Это не используется в Release-сборке.
#if DEBUG
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
            })
#endif
            ;

            // --- КОНЕЦ БЛОКА РЕГИСТРАЦИИ HTTP-КЛИЕНТА ---


            // --- Секция Dependency Injection (Остальные сервисы) ---
            builder.Services.AddSingleton<IUserService, UserService>();
            builder.Services.AddSingleton<IChatService, ChatService>();

            // ViewModels лучше регистрировать как Transient, чтобы для каждой новой страницы
            // создавался свой экземпляр ViewModel с чистым состоянием.
            builder.Services.AddTransient<MainViewModel>();
            builder.Services.AddTransient<SettingsViewModel>();
            builder.Services.AddTransient<VoiceChatViewModel>();
            builder.Services.AddTransient<OnboardingViewModel>();

            // Страницы тоже регистрируем как Transient.
            builder.Services.AddTransient<LoadingPage>();
            builder.Services.AddTransient<MainPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<VoiceChatPage>();
            builder.Services.AddTransient<OnboardingPage>();

            // Shell обычно регистрируется как Singleton или Scoped, но для MAUI
            // можно оставить и Transient, так как он создается один раз при старте.
            builder.Services.AddTransient<AppShell>();
            // --- Конец секции DI ---

            // --- Кастомизация нативных контролов ---
            builder.ConfigureMauiHandlers(handlers =>
            {
#if ANDROID
                // Кастомизация для Picker для удаления подчеркивания на Android
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

                // Кастомизация для Entry для удаления подчеркивания на Android
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