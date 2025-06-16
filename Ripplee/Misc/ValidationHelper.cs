using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Serialization;

namespace Ripplee.Misc
{
    public static class ValidationHelper
    {
        private const int MinUsernameLength = 3;
        private const int MaxUsernameLength = 50;

        private static readonly Regex UsernamePatternRegex =
            new Regex(@"^(?=.*[a-zA-Zа-яА-ЯёЁ])(?!.*[_-]{2})[a-zA-Zа-яА-ЯёЁ0-9][a-zA-Zа-яА-ЯёЁ0-9_-]*[a-zA-Zа-яА-ЯёЁ0-9]$");
        private static readonly Regex UsernameAllowedCharsRegex = new Regex(@"^[a-zA-Zа-яА-ЯёЁ0-9_-]+$");


        public static bool ValidateUsername(string? username, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(username))
            {
                errorMessage = "Логин не может быть пустым.";
                return false;
            }

            if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
            {
                errorMessage = $"Логин должен содержать от {MinUsernameLength} до {MaxUsernameLength} символов.";
                return false;
            }

            if (!UsernameAllowedCharsRegex.IsMatch(username))
            {
                errorMessage = "Логин может содержать только русские/английские буквы, цифры, дефис (-) и подчеркивание (_).";
                return false;
            }

            if (username.StartsWith("-") || username.StartsWith("_") ||
                username.EndsWith("-") || username.EndsWith("_"))
            {
                errorMessage = "Логин не может начинаться или заканчиваться дефисом (-) или подчеркиванием (_).";
                return false;
            }

            if (username.Contains("--") || username.Contains("__") || username.Contains("-_") || username.Contains("_-"))
            {
                errorMessage = "Логин не может содержать два подряд идущих дефиса или подчеркивания.";
                return false;
            }

            if (!username.Any(char.IsLetter))
            {
                errorMessage = "Логин должен содержать хотя бы одну букву.";
                return false;
            }

            return true;
        }

        private const int MinPasswordLength = 8; 

        public static bool ValidatePassword(string? password, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(password))
            {
                errorMessage = "Пароль не может быть пустым.";
                return false;
            }

            if (password.Length < MinPasswordLength)
            {
                errorMessage = $"Пароль должен содержать не менее {MinPasswordLength} символов.";
                return false;
            }

            if (!password.Any(char.IsLetter))
            {
                errorMessage = "Пароль должен содержать хотя бы одну букву.";
                return false;
            }

            if (!password.Any(char.IsDigit))
            {
                errorMessage = "Пароль должен содержать хотя бы одну цифру.";
                return false;
            }

            return true;
        }
    }
}