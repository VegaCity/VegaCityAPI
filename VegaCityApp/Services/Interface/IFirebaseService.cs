namespace VegaCityApp.API.Services.Interface
{
    public interface IFirebaseService
    {
        Task<string> SendOtpAsync(string phoneNumber);
        Task<bool> VerifyOtpAsync(string sessionInfo, string code);
    }
}
