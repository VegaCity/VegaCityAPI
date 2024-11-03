namespace VegaCityApp.API.Payload.Request.Package
{
    public class ChargeMoneyRequest
    {
        public string CccdPassport { get; set; }
        public int ChargeAmount { get; set; }
        public string PaymentType { get; set; }
        public Guid PackageItemId { get; set; }
        public string? PromoCode { get; set; }
    }
}
