using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Response
{
    public class LoginResponse
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public string AccessToken { get; set; }
        public RoleEnum Role { get; set; }
    }
}
