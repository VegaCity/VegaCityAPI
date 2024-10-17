namespace VegaCityApp.API.Payload.Response.PaymentResponse
{
    public class ZaloPayPaymentResponse
    {
        public int returncode { get; set; }
        public string return_message { get; set; }
        public int sub_return_code { get; set; }
        public string sub_return_message { get; set; }
        public string order_url { get; set; }
        public string zp_trans_token { get; set; }
        public string order_token { get; set; }
        public string qr_code { get; set; }
    }
}
