namespace VegaCityApp.API.Payload.Response
{
    public class CreateEtagTypeResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public Guid? EtagTypeId { get; set; }
    }
}
