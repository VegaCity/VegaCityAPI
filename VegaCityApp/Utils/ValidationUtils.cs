using System.Text.RegularExpressions;

namespace VegaCityApp.API.Utils
{
    public class ValidationUtils
    {
        public static bool IsPhoneNumber(string phoneNumber)
        {
            return Regex.IsMatch(phoneNumber, @"^\d{10}$");
        }

        public static bool IsEmail(string email)
        {
            return Regex.IsMatch(email, "^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\\.[a-zA-Z]{2,}$");
        }

        public static bool IsCCCD(string cccd)
        {
            return Regex.IsMatch(cccd, @"^\d{12}$");
        }
    }
}
