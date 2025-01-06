namespace VegaCityApp.API.Payload.Response.WalletResponse
{
    public class GetBalanceEndDayResponse
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public string UserName { get; set; }
        public Guid? StoreId { get; set; }
        public DateTime? DateCheck { get; set; }
        public int? Balance { get; set; }
        public int? BalanceHistory { get; set; }

    }
}
