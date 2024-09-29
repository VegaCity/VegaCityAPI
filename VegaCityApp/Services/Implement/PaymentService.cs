using AutoMapper;
using Microsoft.EntityFrameworkCore;
using System.Net;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Utils;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using static System.Runtime.CompilerServices.RuntimeHelpers;
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
            var checkOrder = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                                        (predicate: x => x.InvoiceId == request.InvoiceId && x.Status == OrderStatus.Pending);
            if (checkOrder == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = PaymentMessage.OrderNotFound
                };
            }
            
            //create momo payment request
            if (request.Key != null) {
                //checkKey
                if (request.Key.Split("_")[0] == "Momo")
                {
                    var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount
                            + "&extraData=" + "&ipnUrl=" + request.UrlIpn + "&orderId=" + request.InvoiceId
                            + "&orderInfo=" + PaymentMomo.orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode
                            + "&redirectUrl=" + request.UrlDirect + "&requestId=" + request.InvoiceId
                            + "&requestType=" + PaymentMomo.requestType;
                    //create signature with sha256 and sercetkey
                    string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
                    var momoPaymentRequest = new MomoPaymentRequest
                    {
                        orderInfo = PaymentMomo.orderInfo,
                        partnerCode = PaymentMomo.MomoPartnerCode,
                        redirectUrl = request.UrlDirect,
                        ipnUrl = request.UrlIpn,
                        amount = checkOrder.TotalAmount,
                        orderId = request.InvoiceId,
                        requestId = request.InvoiceId,
                        requestType = PaymentMomo.requestType,
                        extraData = "",
                        partnerName = PaymentMomo.partnerName,
                        storeId = "VegaCity",
                        orderGroupId = "",
                        autoCapture = PaymentMomo.autoCapture,
                        lang = PaymentMomo.lang,
                        signature = signature,
                        orderExpireTime = PaymentMomo.orderExpireTime
                    };
                    //call momo api
                    var response = await CallApiUtils.CallApiEndpoint("https://test-payment.momo.vn/v2/gateway/api/create", momoPaymentRequest);
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        return new ResponseAPI
                        {
                            StatusCode = HttpStatusCodes.InternalServerError,
                            MessageResponse = PaymentMessage.MomoPaymentFail
                        };
                    }
                    var momoPaymentResponse = await CallApiUtils.GenerateObjectFromResponse<MomoPaymentResponse>(response);
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.OK,
                        MessageResponse = PaymentMessage.MomoPaymentSuccess,
                        Data = momoPaymentResponse
                    };
                }
            } 
            else
            {
                //else
                var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount
                            + "&extraData=" + "&ipnUrl=" + PaymentMomo.ipnUrl + "&orderId=" + request.InvoiceId
                            + "&orderInfo=" + PaymentMomo.orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode
                            + "&redirectUrl=" + PaymentMomo.redirectUrl + "&requestId=" + request.InvoiceId
                            + "&requestType=" + PaymentMomo.requestType;
                //create signature with sha256 and sercetkey
                string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
                var momoPaymentRequest = new MomoPaymentRequest
                {
                    orderInfo = PaymentMomo.orderInfo,
                    partnerCode = PaymentMomo.MomoPartnerCode,
                    redirectUrl = PaymentMomo.redirectUrl,
                    ipnUrl = PaymentMomo.ipnUrl,
                    amount = checkOrder.TotalAmount,
                    orderId = request.InvoiceId,
                    requestId = request.InvoiceId,
                    requestType = PaymentMomo.requestType,
                    extraData = "",
                    partnerName = PaymentMomo.partnerName,
                    storeId = checkOrder.StoreId.ToString(),
                    orderGroupId = "",
                    autoCapture = PaymentMomo.autoCapture,
                    lang = PaymentMomo.lang,
                    signature = signature,
                    orderExpireTime = PaymentMomo.orderExpireTime
                };
                //call momo api
                var response = await CallApiUtils.CallApiEndpoint("https://test-payment.momo.vn/v2/gateway/api/create", momoPaymentRequest);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return new ResponseAPI
                    {
                        StatusCode = HttpStatusCodes.InternalServerError,
                        MessageResponse = PaymentMessage.MomoPaymentFail
                    };
                }
                var momoPaymentResponse = await CallApiUtils.GenerateObjectFromResponse<MomoPaymentResponse>(response);
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.OK,
                    MessageResponse = PaymentMessage.MomoPaymentSuccess,
                    Data = momoPaymentResponse
                };
            }
            return new ResponseAPI
            {
                StatusCode = HttpStatusCodes.BadRequest,
                MessageResponse = PaymentMessage.MomoPaymentFail
            };
        }
        public async Task<ResponseAPI> UpdateOrderPaid(IPNMomoRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PaymentMomo.ipnUrl
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError
                };
        }
        public async Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNMomoRequest req)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == req.orderId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            var etag = await _unitOfWork.GetRepository<Etag>().SingleOrDefaultAsync
                (predicate: x => x.Id == order.EtagId && !x.Deflag, include: etag => etag.Include(z => z.Wallet));
            //update wallet
            etag.Wallet.Balance += (int)req.amount;
            etag.Wallet.BalanceHistory += (int)req.amount;
            etag.Wallet.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Wallet>().UpdateAsync(etag.Wallet);
            //create deposite
            var newDeposit = new Deposit
            {
                Id = Guid.NewGuid(), // Tạo ID mới
                PaymentType = "Momo",
                Name = "Nạp tiền vào ETag với số tiền: " + req.amount,
                IsIncrease = "true", // Xác định rằng đây là nạp tiền
                Amount =(int) req.amount,
                CrDate = TimeUtils.GetCurrentSEATime(),
                UpsDate = TimeUtils.GetCurrentSEATime(),
                WalletId = etag.Wallet.Id,
                EtagId = etag.Id
            };
            await _unitOfWork.GetRepository<Deposit>().InsertAsync(newDeposit);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PaymentMomo.ipnUrl
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError
                };
        }
        public async Task<ResponseAPI> VnPayment(PaymentRequest req, HttpContext context)
        {
            var orderExisted = await _unitOfWork.GetRepository<Order>()
                .SingleOrDefaultAsync(predicate: x =>
                    x.InvoiceId == req.InvoiceId && x.Status == OrderStatus.Pending);
            if (orderExisted == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.NotFound,
                    MessageResponse = OrderMessage.NotFoundOrder
                };
            }
            var tick = TimeUtils.GetCurrentSEATime().ToString();
            var vnpay = new VnPayLibrary();
            vnpay.AddRequestData("vnp_Version", VnPayConfig.Version);
            vnpay.AddRequestData("vnp_Command", VnPayConfig.Command);
            vnpay.AddRequestData("vnp_TmnCode", VnPayConfig.TmnCode);
            vnpay.AddRequestData("vnp_Amount", (orderExisted.TotalAmount * 100).ToString()); //Số tiền thanh toán. Số tiền không mang các ký tự phân tách thập phân, phần nghìn, ký tự tiền tệ. Để gửi số tiền thanh toán là 100,000 VND (một trăm nghìn VNĐ) thì merchant cần nhân thêm 100 lần (khử phần thập phân), sau đó gửi sang VNPAY là: 10000000

            vnpay.AddRequestData("vnp_CreateDate", TimeUtils.GetCurrentSEATime().ToString("yyyyMMddHHmmss"));
            vnpay.AddRequestData("vnp_CurrCode", VnPayConfig.CurrCode);
            vnpay.AddRequestData("vnp_IpAddr", VnPayUtils.GetIpAddress(context));
            vnpay.AddRequestData("vnp_Locale", VnPayConfig.Locale);

            vnpay.AddRequestData("vnp_OrderInfo", "Thanh toán cho đơn hàng (InvoiceId):" + req.InvoiceId);
            vnpay.AddRequestData("vnp_OrderType", "other"); //default value: other
            vnpay.AddRequestData("vnp_ReturnUrl", VnPayConfig.PaymentBackReturnUrl);

            vnpay.AddRequestData("vnp_TxnRef", tick); // Mã tham chiếu của giao dịch tại hệ thống của merchant. Mã này là duy nhất dùng để phân biệt các đơn hàng gửi sang VNPAY. Không được trùng lặp trong ngày

            var paymentUrl = vnpay.CreateRequestUrl(VnPayConfig.BaseUrl, VnPayConfig.HashSecret);
            //
            if (paymentUrl == null)
            {
                return new ResponseAPI
                {
                    StatusCode = HttpStatusCodes.InternalServerError,
                    MessageResponse = PaymentMessage.vnPayFailed
                };
            }

            return new ResponseAPI()
            {
                StatusCode = HttpStatusCodes.OK,
                MessageResponse = PaymentMessage.VnPaySuccess,
                Data = new VnPaymentResponse()
                {
                    Success = true,
                    PaymentMethod = "VnPay",
                    OrderDescription = "Thanh toán VnPay cho đơn hàng :" + req.InvoiceId,
                    OrderId = req.InvoiceId,
                    Amount = orderExisted.TotalAmount,
                    VnPayResponse = paymentUrl,
                    CrDate = TimeUtils.GetCurrentSEATime()

                }
            };

        }

        public async Task<ResponseAPI> UpdateVnPayOrder(VnPayPaymentResponse req)
        {
            var invoiceId = req.vnp_OrderInfo.Split(":", 2);
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == invoiceId[1] && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            order.UpsDate = TimeUtils.GetCurrentSEATime();
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.NoContent,
                    MessageResponse = PaymentMomo.ipnUrl
                }
                : new ResponseAPI()
                {
                    StatusCode = HttpStatusCodes.InternalServerError
                };
        }
    }
}
