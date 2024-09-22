using AutoMapper;
using System.Net;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
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
            var rawSignature = "accessKey=" + PaymentMomo.MomoAccessKey + "&amount=" + checkOrder.TotalAmount 
                            + "&extraData="+ "&ipnUrl=" + PaymentMomo.ipnUrl + "&orderId=" + request.InvoiceId 
                            + "&orderInfo=" + orderInfo + "&partnerCode=" + PaymentMomo.MomoPartnerCode 
                            + "&redirectUrl=" + PaymentMomo.redirectUrl + "&requestId=" + request.InvoiceId
                            + "&requestType=" + PaymentMomo.requestType;
            //create signature with sha256 and sercetkey
            string signature = PasswordUtil.getSignature(rawSignature, PaymentMomo.MomoSecretKey);
            //create momo payment request
            var momoPaymentRequest = new MomoPaymentRequest
            {
                orderInfo = orderInfo,
                partnerCode = PaymentMomo.MomoPartnerCode,
                redirectUrl = PaymentMomo.redirectUrl,
                ipnUrl = PaymentMomo.ipnUrl + request.InvoiceId,
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
        public async Task<ResponseAPI> UpdateOrderPaid(string invoiceId)
        {
            var order = await _unitOfWork.GetRepository<Order>().SingleOrDefaultAsync
                (predicate: x => x.InvoiceId == invoiceId && x.Status == OrderStatus.Pending);
            order.Status = OrderStatus.Completed;
            _unitOfWork.GetRepository<Order>().UpdateAsync(order);
            return await _unitOfWork.CommitAsync() > 0
                ? new ResponseAPI()
                {
                    MessageResponse = OrderMessage.UpdateOrderSuccess,
                    StatusCode = HttpStatusCodes.OK,
                    Data = new
                    {
                        OrderId = order.Id,
                        invoiceId = order.InvoiceId
                    }
                }
                : new ResponseAPI()
                {
                    MessageResponse = OrderMessage.UpdateOrderFailed,
                    StatusCode = HttpStatusCodes.InternalServerError
                };
        }
    }
}
