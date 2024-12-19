using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Response
{
    public class LoginResponse
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Data Data { get; set; }
    }

    public class Tokens
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }

    public class Data
    {
        public Guid UserId { get; set; }
        public string Email { get; set; }
        public string RoleName { get; set; }
        public Guid RoleId { get; set; }
        public  Tokens Tokens { get; set; }
        public int? StoreType { get; set; }
        public bool? IsSession { get; set; }
    }
}
