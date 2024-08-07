namespace VegaCityApp.API.Payload.Response
{
    public class CreateWalletTypeResponse
    {
        public int StatusCode { get; set; }
        public string Message { get; set; }
        public Guid? WalletTypeId { get; set; }
    }
}
