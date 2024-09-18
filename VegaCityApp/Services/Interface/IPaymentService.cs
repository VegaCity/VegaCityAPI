using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPaymentService
    {
        Task<ResponseAPI> MomoPayment(PaymentRequest request);
    }
}
