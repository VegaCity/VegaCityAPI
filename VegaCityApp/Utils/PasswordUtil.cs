using System.Security.Cryptography;
using System.Text;

namespace VegaCityApp.API.Utils
{
	public static class PasswordUtil
	{
		public static string HashPassword(string rawPassword)
		{
			using (var sha256 = SHA256.Create())
			{
				byte[] bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawPassword));
				return Convert.ToBase64String(bytes);
			}
		}
        public static string GenerateCharacter(int length)
        {
            // Chuỗi chứa các ký tự có thể có trong ETag
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            Random random = new Random();

            // Tạo chuỗi ngẫu nhiên từ các ký tự đã định nghĩa
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GenderateRandomNumber(int length)
        {
            const string chars = "0123456789";
            Random random = new Random();
            return new string(Enumerable.Repeat(chars, length)
                               .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public static string GeneratePinCode()
        {
            var random = new Random();
            return random.Next(100000, 999999).ToString();
        }
    }
}
