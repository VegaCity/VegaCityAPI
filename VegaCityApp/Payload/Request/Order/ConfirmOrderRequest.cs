namespace VegaCityApp.API.Payload.Request.Order
{
    public class ConfirmOrderRequest
    {
        public string InvoiceId { get; set; }
        public string TransactionId { get; set; }
    }
}
