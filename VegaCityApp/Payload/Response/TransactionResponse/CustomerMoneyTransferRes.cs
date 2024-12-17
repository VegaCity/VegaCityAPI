namespace VegaCityApp.API.Payload.Response.TransactionResponse
{
    public class CustomerMoneyTransferRes
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid MarketZoneId { get; set; }
        public Guid PackageOrderId { get; set; }
        public Guid TransactionId { get; set; }
        public int Amount { get; set; }
        public string Status { get; set; } = null!;
        public bool IsIncrease { get; set; }
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
    }
}
