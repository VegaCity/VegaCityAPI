using System.ComponentModel.DataAnnotations;

namespace VegaCityApp.API.Payload.Request.Auth
{
    public class ReFreshTokenRequest
    {
        [Required]
        public string Email { get; set;}  
        public string? RefreshToken { get; set; }
    }
}
