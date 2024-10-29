using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using VegaCityApp.API.Payload.Request.Firebase;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Services.Implement
{
    public class FirebaseService : IFirebaseService
    {
        private readonly FirebaseSetting _firebaseSetting;
        public FirebaseService()
        {
            _firebaseSetting = JsonUtil.GetFromAppSettings<FirebaseSetting>("Firebase") ?? throw new InvalidOperationException();
        }
        public async Task<string> SendOtpAsync(string phoneNumber)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:sendVerificationCode?key={_firebaseSetting.ApiKey}";
            var payload = new
            {
                phoneNumber = phoneNumber
            };

            var response = await CallApiUtils.CallApiEndpoint(url, payload);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                dynamic jsonResponse = JsonConvert.DeserializeObject(responseContent);
                return jsonResponse.sessionInfo;
            }
            else
            {
                throw new Exception("Gửi OTP thất bại");
            }
        }

        // Phương thức xác minh OTP
        public async Task<bool> VerifyOtpAsync(string sessionInfo, string code)
        {
            var url = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPhoneNumber?key={_firebaseSetting.ApiKey}";
            var payload = new
            {
                sessionInfo = sessionInfo,
                code = code
            };

            var response = await CallApiUtils.CallApiEndpoint(url, payload);

            if (response.IsSuccessStatusCode)
            {
                return true; // OTP hợp lệ
            }
            else
            {
                return false; // OTP không hợp lệ
            }
        }
    }
}
