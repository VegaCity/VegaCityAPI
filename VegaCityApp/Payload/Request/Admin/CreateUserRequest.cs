namespace VegaCityApp.API.Payload.Request.Admin
{
    public class CreateUserRequest
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public DateTime? Birthday { get; set; }
        public int? Gender { get; set; }
        public string CccdPassport { get; set; }
        public string? Description { get; set; }
        public string? Address { get; set; }
        public string RoleName { get; set; }
        public string Email { get; set; }
    }
}
