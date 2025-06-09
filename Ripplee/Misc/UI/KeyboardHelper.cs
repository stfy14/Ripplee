#if ANDROID
using Android.Content;
using Android.Views.InputMethods;
#elif IOS || MACCATALYST
using UIKit;
#endif

namespace Ripplee.Misc.UI
{
    public static class KeyboardHelper
    {
        public static void HideKeyboard()
        {
            // Вызываем метод HideKeyboard() для текущего потока,
            // чтобы избежать проблем с вызовом из фоновых потоков.
            MainThread.InvokeOnMainThreadAsync(() =>
            {
#if ANDROID
                // Код для Android
                var context = Platform.AppContext;
                var inputMethodManager = context.GetSystemService(Context.InputMethodService) as InputMethodManager;
                if (inputMethodManager != null)
                {
                    var activity = Platform.CurrentActivity;
                    var token = activity?.CurrentFocus?.WindowToken;
                    inputMethodManager.HideSoftInputFromWindow(token, HideSoftInputFlags.None);
                    activity?.Window?.DecorView.ClearFocus(); // Снимаем фокус с поля ввода
                }
#elif IOS || MACCATALYST
                // Код для iOS и MacCatalyst
                // Находим активное окно и просим его завершить редактирование (спрятать клавиатуру)
                UIApplication.SharedApplication.KeyWindow?.EndEditing(true);
#endif
            });
        }
    }
}