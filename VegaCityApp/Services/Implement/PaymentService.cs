    using AutoMapper;
    using System.Net;
    using VegaCityApp.API.Enums;
    using VegaCityApp.API.Payload.Request.Payment;
    using VegaCityApp.API.Payload.Response;
    using VegaCityApp.API.Payload.Response.OrderResponse;
    using VegaCityApp.API.Payload.Response.PaymentResponse;
    using VegaCityApp.API.Services.Interface;
    using VegaCityApp.API.Utils;
    using VegaCityApp.Domain.Models;
    using VegaCityApp.Repository.Interfaces;
    using static VegaCityApp.API.Constants.MessageConstant;

    namespace VegaCityApp.API.Services.Implement
    {
        public class PaymentService : BaseService<PaymentService>, IPaymentService
        {
            public PaymentService(IUnitOfWork<VegaCityAppContext> unitOfWork, ILogger<PaymentService> logger,
                IMapper mapper,
                IHttpContextAccessor httpContextAccessor) : base(unitOfWork, logger, httpContextAccessor, mapper)
            {
            }

            public async Task<ResponseAPI> MomoPayment(PaymentRequest request)
            {
                string orderInfo = "pay with MoMo";
                var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                                            (predicate: x => x.InvoiceId == request.InvoiceId && x.Status == OrderStatus.Pending);
                if (checkOrder == null)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.NotFound,
                        MessageResponse = "Order not found"
                    };
                }
                var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount + "&extraData="+ "&ipnUrl=" + PaymentMomo.ipnUrl + "&orderId=" + request.InvoiceId + "&orderInfo=" + orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode + "&redirectUrl=" + PaymentMomo.redirectUrl + "&requestId=" + request.InvoiceId + "&requestType=" + PaymentMomo.requestType;
                //create signature with sha256 and sercetkey
                string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
                //create momo payment request
                var momoPaymentRequest = new MomoPaymentRequest
                {
                    orderInfo = orderInfo,
                    partnerCode = PaymentMomo.MomoPartnerCode,
                    redirectUrl = PaymentMomo.redirectUrl,
                    ipnUrl = PaymentMomo.ipnUrl,
                    amount = checkOrder.TotalAmount,
                    orderId = request.InvoiceId,
                    requestId = request.InvoiceId,
                    requestType = PaymentMomo.requestType,
                    extraData = "",
                    partnerName = "MoMo Payment",
                    storeId = checkOrder.StoreId.ToString(),
                    orderGroupId = "",
                    autoCapture = PaymentMomo.autoCapture,
                    lang = PaymentMomo.lang,
                    signature = signature,

                };
                //call momo api
                var response = await CallApiUtils.CallApiEndpoint("https://test-payment.momo.vn/v2/gateway/api/create", momoPaymentRequest);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.InternalServerError,
                        MessageResponse = "Momo payment failed"
                    };
                }
                var momoPaymentResponse = await CallApiUtils.GenerateObjectFromResponse<MomoPaymentResponse>(response);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = "Momo payment success",
                    Data = momoPaymentResponse
                };
                
            }
                public async Task<ResponseAPI> CreateVnPayUrl(PaymentRequest request, HttpContext context)
                {
                    string TmnCode = "J5WEIXD3";
                    string HashSecret = "NSFR5ERYRKAL2D0TWU50VWBDTJGKZX6J";
                    string BaseUrl = "https://sandbox.vnpayment.vn/paymentv2/vpcpay.html";
                    string Version = "2.1.0";
                    string Command = "pay";
                    string CurrCode = "VND";
                    string Locale = "vn";
                    string PaymentBackReturnUrl = "https://sandbox.vnpayment.vn/merchantv2/";
                    var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                        (predicate: x => x.InvoiceId == request.InvoiceId && x.Status == OrderStatus.Pending);
                    if (checkOrder == null)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.NotFound,
                            MessageResponse = "Order not found"
                        };
                    }

                    var tick = TimeUtils.GetCurrentSEATime().ToString();
                    var vnpay = new VnPayLibrary();
                    vnpay.AddRequestData("vnp_Version", Version);
                    vnpay.AddRequestData("vnp_Command",Command);
                    vnpay.AddRequestData("vnp_TmnCode", TmnCode);
                    vnpay.AddRequestData("vnp_Amount", (checkOrder.TotalAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

                    vnpay.AddRequestData("vnp_CreateDate", TimeUtils.GetCurrentSEATime().ToString("yyyyMMddHHmmss"));
                    vnpay.AddRequestData("vnp_CurrCode", CurrCode);
                    vnpay.AddRequestData("vnp_IpAddr", VnPayUtils.GetIpAddress(context));
                    vnpay.AddRequestData("vnp_Locale",Locale);

                    vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng (InvoiceId):" + request.InvoiceId);
                    vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
                    vnpay.AddRequestData("vnp_ReturnUrl", PaymentBackReturnUrl);

                    vnpay.AddRequestData("vnp_TxnRef", tick); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

                    var paymentUrl = vnpay.CreateRequestUrl(BaseUrl, HashSecret);
                    //
                    if (paymentUrl == null)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.InternalServerError,
                            MessageResponse = PaymentMessage.PaymentFail
                        };
                    }
                
                    return new ResponseAPI()
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = PaymentMessage.PaymentSuccess,
                        Data = paymentUrl
                    };

                }



    }
}
