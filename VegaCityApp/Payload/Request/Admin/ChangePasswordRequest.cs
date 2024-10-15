namespace VegaCityApp.API.Payload.Request.Admin
{
    public class ChangePasswordRequest
    {
        public Guid apiKey { get; set; }
        public string Email { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }
}
