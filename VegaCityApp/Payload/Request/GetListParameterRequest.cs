namespace VegaCityApp.API.Payload.Request
{
    public class GetListParameterRequest
    {
        public int? Page { get; set; }
        public int? PageSize { get; set; }
        public string? Search { get; set; }
    }
}
