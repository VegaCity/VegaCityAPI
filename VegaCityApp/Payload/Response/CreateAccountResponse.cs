using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response
{
    public class CreateAccountResponse
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Guid UserId { get; set; }
        public Guid AccountId { get; set; }
    }
}
