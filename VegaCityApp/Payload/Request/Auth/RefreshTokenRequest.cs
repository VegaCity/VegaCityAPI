namespace VegaCityApp.API.Payload.Request.Auth
{
    public class ReFreshTokenRequest
    {
        public string Email { get; set;}  
        public string? RefreshToken { get; set; }
    }
}
