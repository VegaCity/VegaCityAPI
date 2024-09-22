using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Enums
{
    public enum PaymentTypeEnum
    {
        ZaloPay,
        Momo,
        VnPay,
        VisaCard,
        Other,
    }
    public class PaymentType
    {
        public const string ZaloPay = "ZaloPay";
        public const string Momo = "Momo";
        public const string VnPay = "VnPay";
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
        public const string redirectUrl = "https://vega.vinhuser.one/api/v1/payment/momo/order";
        //after success payment, send a request to this url to update order or something
        public const string ipnUrl = "https://webhook.site/b3088a6a-2d17-4f8d-a383-71389a6c600b";
        public const string requestType = "payWithMethod";
        public const string paymentCode = "T8Qii53fAXyUftPV3m9ysyRhEanUs9KlOPfHgpMR0ON50U10Bh+vZdpJU7VY4z+Z2y77fJHkoDc69scwwzLuW5MzeUKTwPo3ZMaB29imm6YulqnWfTkgzqRaion+EuD7FN9wZ4aXE1+mRt0gHsU193y+yxtRgpmY7SDMU9hCKoQtYyHsfFR5FUAOAKMdw2fzQqpToei3rnaYvZuYaxolprm9+/+WIETnPUDlxCYOiw7vPeaaYQQH0BF0TxyU3zu36ODx980rJvPAgtJzH1gUrlxcSS1HQeQ9ZaVM1eOK/jl8KJm6ijOwErHGbgf/hVymUQG65rHU2MWz9U8QUjvDWA==";
        public const bool autoCapture = true;
        public const string lang = "vi";
    }
    public class PaymentZaloPay
    {
        public const string app_id = "2554";
        public const string app_user = "demo";
        public const string app_time = "1612138541";
        public const string key1 = "sdngKKJmqEMzvh5QQcdD2A9XBSKUNaYn";
        public const string key2 = "trMrHtvjo6myautxDUiAcYsVtaeQ8nhf";
        public const string create_order_url = "https://sb-openapi.zalopay.vn/v2/create";
    }
}
