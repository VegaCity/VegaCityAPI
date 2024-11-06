using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IOrderService
    {
        Task<ResponseAPI> CreateOrder(CreateOrderRequest req);
        Task<ResponseAPI<IEnumerable<GetOrderResponse>>> SearchAllOrders(int size, int page);
        Task<ResponseAPI> SearchOrder(Guid? OrderId, string? InvoiceId);
        Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest req);
        Task<ResponseAPI> DeleteOrder(Guid OrderId);
        //Task<ResponseAPI> RemoveEtagTypeFromPackage(Guid etagId, Guid packageId);
        Task<ResponseAPI> CreateOrderForCashier(CreateOrderForCashierRequest req);
        Task<ResponseAPI> ConfirmOrderForCashier(ConfirmOrderForCashierRequest req);
        Task<ResponseAPI> ConfirmOrder(ConfirmOrderRequest req);
    }
}
