namespace VegaCityApp.API.Payload.Request.Store
{
    public class GetWalletStoreRequest
    {
        public string? StoreName { get; set; }
        public string PhoneNumber { get; set; }
        public string? Status { get; set; }
    }
}
