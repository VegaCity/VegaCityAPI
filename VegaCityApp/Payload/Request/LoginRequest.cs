using System.ComponentModel.DataAnnotations;

namespace VegaCityApp.Payload.Request
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [MaxLength(50, ErrorMessage = "Username's max length is 50 characters")]
        public string username { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [MaxLength(64, ErrorMessage = "Password's max length is 64 characters")]
        public string password { get; set; }
    }
}
