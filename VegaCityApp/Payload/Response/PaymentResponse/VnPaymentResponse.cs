namespace VegaCityApp.API.Payload.Response.PaymentResponse
{
    public class VnPaymentResponse
    {
        public bool Success { get; set; }
        public string PaymentMethod { get; set; }
        public string OrderDescription { get; set; }
        public string OrderId { get; set; }
        public int Amount { get; set; }
        public DateTime CrDate { get; set; }
        //public string PaymentId { get; set; }
        //public string TransactionId { get; set; }
        //public string Token { get; set; }
        public string VnPayResponse { get; set; }
    }

    public class VnPayPaymentResponse
    {
        public int vnp_Amount { get; set; }
        public string vnp_BankCode { get; set; }
        public string vnp_BankTranNo { get; set; }

        public string vnp_CardType { get; set; }

        public string vnp_OrderInfo { get; set; }

        public string vnp_PayDate { get; set; }

        public int vnp_ResponseCode { get; set; }

        public string vnp_TmnCode { get; set; }
        public long vnp_TransactionNo { get; set; }

        public int vnp_TransactionStatus { get; set; }

        public DateTime vnp_TxnRef { get; set; }

        public string vnp_SecureHash { get; set; }
    }
}
