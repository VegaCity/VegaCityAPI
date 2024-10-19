namespace VegaCityApp.API.Payload.Response.TransactionResponse
{
    public class TransactionResponse
    {
        public Guid Id { get; set; }
        public string Type { get; set; }
        public Guid? WalletId { get; set; }
        public Guid? StoreId { get; set; }
        public string? Status { get; set; }
        public bool IsIncrease { get; set; }
        public string? Description { get; set; }
        public DateTime CrDate { get; set; }
        public int Amount { get; set; }
        public string? Currency { get; set; }
    }
}
