using VegaCityApp.API.Payload.Request.Payment;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.PaymentResponse;

namespace VegaCityApp.API.Services.Interface
{
    public interface IPaymentService
    {
        Task<ResponseAPI> MomoPayment(PaymentRequest request);
        Task<ResponseAPI> CreateVnPayUrl(PaymentRequest request, HttpContext context); //need to response api
        //todo: show 
        //Task<ResponseAPI> PaymentExecute(IQueryCollection collections);
    }
}
