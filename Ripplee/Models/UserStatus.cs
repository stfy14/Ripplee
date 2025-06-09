namespace Ripplee.Models
{
    public enum UserStatus
    {
        Unauthenticated, // Пользователь еще не вошел (начальное состояние)
        Guest,           // Пользователь вошел как гость
        Registered       // Пользователь зарегистрирован и вошел
    }
}