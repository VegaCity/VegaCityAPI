namespace VegaCityApp.API.Payload.Response.TransactionResponse
{
    public class StoreMoneyTransferRes
    {
        public Guid Id { get; set; }
        public Guid StoreId { get; set; }
        public Guid MarketZoneId { get; set; }
        public int Amount { get; set; }
        public bool IsIncrease { get; set; }
        public string? Description { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string Status { get; set; } = null!;
        public Guid TransactionId { get; set; }
    }
}
