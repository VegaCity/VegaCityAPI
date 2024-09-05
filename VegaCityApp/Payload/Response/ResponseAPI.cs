using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response
{
    public class ResponseAPI
    {
        public int StatusCode { get; set; }
        public string MessageResponse { get; set; }
        public Object Data { get; set; }
    }
}
