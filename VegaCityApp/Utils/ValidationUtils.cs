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
            if(cccd.Length == 12)
            return Regex.IsMatch(cccd, @"^\d{12}$");
            else if(cccd.Length >= 6 && cccd.Length <= 9)
            return Regex.IsMatch(cccd, @"^(?=.*[A-Za-z])(?=.*\d)[A-Za-z\d]{6,9}$");
            return false;
        }
        public static bool CheckNumber(int number)
        {
            return number >= 0;
        }
    }
}
