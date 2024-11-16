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
        public static string HmacSHA512(string key, string data)
        {
            var hash = new StringBuilder();
            var keyBytes = Encoding.UTF8.GetBytes(key);
            var inputBytes = Encoding.UTF8.GetBytes(data);
            using (var hmac = new HMACSHA512(keyBytes))
            {
                var hashValue = hmac.ComputeHash(inputBytes);
                foreach (var theByte in hashValue)
                {
                    hash.Append(theByte.ToString("x2"));
                }
            }

            return hash.ToString();
        }
        public static String getSignature(String text, String key)
        {
            // change according to your needs, an UTF8Encoding
            // could be more suitable in certain situations
            ASCIIEncoding encoding = new ASCIIEncoding();

            Byte[] textBytes = encoding.GetBytes(text);
            Byte[] keyBytes = encoding.GetBytes(key);

            Byte[] hashBytes;

            using (HMACSHA256 hash = new HMACSHA256(keyBytes))
                hashBytes = hash.ComputeHash(textBytes);

            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        public enum ZaloPayHMAC
        {
            HMACMD5,
            HMACSHA1,
            HMACSHA256,
            HMACSHA512
        }
        public static string Compute(ZaloPayHMAC algorithm = ZaloPayHMAC.HMACSHA256, string key = "", string message = "")
        {
            byte[] keyByte = System.Text.Encoding.UTF8.GetBytes(key);
            byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
            byte[] hashMessage = null;

            switch (algorithm)
            {
                case ZaloPayHMAC.HMACMD5:
                    hashMessage = new HMACMD5(keyByte).ComputeHash(messageBytes);
                    break;
                case ZaloPayHMAC.HMACSHA1:
                    hashMessage = new HMACSHA1(keyByte).ComputeHash(messageBytes);
                    break;
                case ZaloPayHMAC.HMACSHA256:
                    hashMessage = new HMACSHA256(keyByte).ComputeHash(messageBytes);
                    break;
                case ZaloPayHMAC.HMACSHA512:
                    hashMessage = new HMACSHA512(keyByte).ComputeHash(messageBytes);
                    break;
                default:
                    hashMessage = new HMACSHA256(keyByte).ComputeHash(messageBytes);
                    break;
            }

            return BitConverter.ToString(hashMessage).Replace("-","").ToLower();
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
        public class UniqueIdGenerator
        {
            private static HashSet<string> generatedIds = new HashSet<string>();

            public static string GenerateUniqueRandomNumber(int length)
            {
                const string chars = "0123456789";
                Random random = new Random();
                string newId;

                // Keep generating until we have a unique ID
                do
                {
                    newId = new string(Enumerable.Repeat(chars, length)
                                                 .Select(s => s[random.Next(s.Length)]).ToArray());
                }
                while (generatedIds.Contains(newId));

                // Add the new unique ID to the set
                generatedIds.Add(newId);

                return newId;
            }
        }
    }
}
