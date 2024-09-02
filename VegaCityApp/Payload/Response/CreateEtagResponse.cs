namespace VegaCityApp.API.Payload.Response
{
    public class CreateEtagResponse
    {
        public string Message { get; set; }
        public int StatusCode { get; set;}

        public Guid? etagId { get; set; }
    }
}
