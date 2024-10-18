using Microsoft.AspNetCore.Mvc;
using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPaymentService
    {
        Task<ResponseAPI> MomoPayment(PaymentRequest request);
        Task<ResponseAPI> UpdateOrderPaid(IPNMomoRequest req);
        Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNMomoRequest req);
        Task<ResponseAPI> VnPayment(PaymentRequest request, HttpContext context); //need to response api
        Task<ResponseAPI> UpdateVnPayOrder(VnPayPaymentResponse req);
        Task<ResponseAPI> UpdateOrderPaidForChargingMoney([FromQuery] VnPayPaymentResponse req);
        Task<ResponseAPI> PayOSPayment(PaymentRequest req);
        Task<ResponseAPI> UpdatePayOSOrder(string code, string id, string status, string orderCode);
        Task<ResponseAPI> UpdateOrderPaidOSForChargingMoney(string code, string id, string status, string orderCode);
        Task<ResponseAPI> ZaloPayPayment(PaymentRequest req);
        Task<ResponseAPI> UpdateOrderPaid(IPNZaloPayRequest req);
        Task<ResponseAPI> UpdateOrderPaidForChargingMoney(IPNZaloPayRequest req);
    }
}
