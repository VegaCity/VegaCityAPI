namespace VegaCityApp.API.Payload.Request.Etag
{
    public class ChargeMoneyEtagRequest
    {
        public string EtagCode { get; set; }
        public int ChargeAmount { get; set; }
        public string CccdPassport { get; set; }
        public string PaymentType { get; set; }
    }
}
