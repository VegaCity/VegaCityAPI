using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPaymentService
    {
        Task<ResponseAPI> MomoPayment(PaymentRequest request);
        Task<ResponseAPI> UpdateOrderPaid(IPNMomoRequest req);
        Task<ResponseAPI> VnPayment(PaymentRequest request, HttpContext context); //need to response api

    }
}
