namespace VegaCityApp.API.Payload.Response.WalletResponse
{
    public class GetWalletResponse : ResponseAPI // add more
    {
        public Guid? WalletId { get; set; }
        public int? Balance { get; set; }

        public int? WalletType { get; set; }
    }
}
