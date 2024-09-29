namespace VegaCityApp.API.Payload.Request.Etag
{
    public class ChargeMoneyEtagRequest
    {
        public Guid UserId { get; set; }
        public string EtagCode { get; set; }
        public int ChargeAmount { get; set; }
        public string CCCD { get; set; }
        public string PaymentType { get; set; }

    }
}
