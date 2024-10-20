using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Enums
{
    public static class PaymentTypeHelper
    {
        public static readonly string[] allowedPaymentTypes =
        {
            "ZaloPay",
            "Momo",
            "VnPay",
            "PayOS",
            "Cash"
        };
    }
    public static class SaleTypeHelper
    {
        public static readonly string[] allowedSaleType =
        {
            "Package",
            "EtagType",
            "Etag Charge",
        };
    }
    public class PaymentMomo
    {
        public const string MomoAccessKey = "F8BBA842ECF85";
        public const string MomoSecretKey = "K951B6PE1waDMi640xX08PD3vg6EkVlz";
        public const string MomoPartnerCode = "MOMO";
        public const string orderInfo = "Vega City Payment";
        public const string partnerName = "MoMo Payment";
        public const int orderExpireTime = 30;
        //after success payment, redirect to this url
        //public const string redirectUrl = "https://localhost:44395/api/v1/payment/momo/order";
        //public const string redirectUrlChargeMoney = "https://localhost:44395/api/v1/payment/momo/order/charge-money";
        public const string redirectUrl = "https://api.vegacity.id.vn/api/v1/payment/momo/order";
        public const string redirectUrlChargeMoney = "https://api.vegacity.id.vn/api/v1/payment/momo/order/charge-money";
        //after success payment, send a request to this url to update order or something
        public const string ipnUrl = "https://vegacity.id.vn/order-status?status=success&orderId=";
        public const string requestType = "payWithMethod";
        public const string paymentCode = "T8Qii53fAXyUftPV3m9ysyRhEanUs9KlOPfHgpMR0ON50U10Bh+vZdpJU7VY4z+Z2y77fJHkoDc69scwwzLuW5MzeUKTwPo3ZMaB29imm6YulqnWfTkgzqRaion+EuD7FN9wZ4aXE1+mRt0gHsU193y+yxtRgpmY7SDMU9hCKoQtYyHsfFR5FUAOAKMdw2fzQqpToei3rnaYvZuYaxolprm9+/+WIETnPUDlxCYOiw7vPeaaYQQH0BF0TxyU3zu36ODx980rJvPAgtJzH1gUrlxcSS1HQeQ9ZaVM1eOK/jl8KJm6ijOwErHGbgf/hVymUQG65rHU2MWz9U8QUjvDWA==";
        public const bool autoCapture = true;
        public const string lang = "vi";
    }
    public class PaymentZaloPay
    {
        public const int app_id = 2554;
        public const string app_user = "demo";
        public const string app_time = "1612138541";
        public const string key1 = "sdngKKJmqEMzvh5QQcdD2A9XBSKUNaYn";
        public const string key2 = "trMrHtvjo6myautxDUiAcYsVtaeQ8nhf";
        public const string create_order_url = "https://sb-openapi.zalopay.vn/v2/create";
        public const string redirectUrl = "https://api.vegacity.id.vn/api/v1/payment/zalopay/order";
        public const string ipnUrl = "https://vegacity.id.vn/order-status?status=success&orderId=";
    }

    public class VnPayConfig
    {
        public const string TmnCode = "J5WEIXD3";
        public const string HashSecret = "NSFR5ERYRKAL2D0TWU50VWBDTJGKZX6J";
        public const string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
        public const string Version = "2.1.0";
        public const string Command = "pay";
        public const string CurrCode = "VND";
        public const string Locale = "vn";
        //public const string PaymentBackReturnUrl = "https://localhost:44395/api/v1/payment/vnpay/order";//redirect sau khi thanh toan
        //public const string VnPaymentBackReturnUrl = "https://localhost:44395/api/v1/payment/vnpay/order/charge-money";
        public const string ipnUrl = "https://vegacity.id.vn/user/order-status?status=success&orderId=";
        public const string PaymentBackReturnUrl = "https://api.vegacity.id.vn/api/v1/payment/vnpay/order";
        public const string VnPaymentBackReturnUrlChargeMoney = "https://api.vegacity.id.vn/api/v1/payment/vnpay/order/charge-money";

    }

    public class PayOSConfiguration
    {
        public const string ClientId = "90c82845-c8fb-4644-912c-fcd6fb9a0b91";
        public const string ApiKey = "6fbfc454-452d-400c-a956-60713ed63655";
        public const string ChecksumKey = "e0df18284b37c90d88390be74568f6e0c4ff63c95cc4f1ee2eb6af30877d37a5";
        public const string CancelUrl = "https://example.com/cancel";
       // public const string ReturnUrl = "https://localhost:44395/api/v1/payment/payos/order"; //local
        //public const string ReturnUrlCharge = "https://localhost:44395/api/v1/payment/payos/order/charge-money"; //local
        public const string ReturnUrl = "https://api.vegacity.id.vn/api/v1/payment/payos/order"; //deploy
        public const string ReturnUrlCharge = "https://api.vegacity.id.vn/api/v1/payment/payos/order/charge-money"; //dploy
        public const string create_order_url = "https://api-merchant.payos.vn/v2/payment-requests";
        public const string ipnUrl = "https://vegacity.id.vn/order-status?status=success&orderId=";
    }
}
