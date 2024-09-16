using VegaCityApp.API.Payload.Request.Order;
using VegaCityApp.API.Payload.Response;
using VegaCityApp.API.Payload.Response.OrderResponse;
using VegaCityApp.Domain.Paginate;

namespace VegaCityApp.API.Services.Interface
{
    public interface IOrderService
    {
        Task<ResponseAPI> CreateOrder(CreateOrderRequest req);
        Task<IPaginate<GetOrderResponse>> SearchAllOrders(int size, int page);

        Task<ResponseAPI> SearchOrder(Guid OrderId);
        Task<ResponseAPI> UpdateOrder(string InvoiceId, UpdateOrderRequest req);
        Task<ResponseAPI> DeleteOrder(Guid OrderId);
        //Task<ResponseAPI> RemoveEtagTypeFromPackage(Guid etagId, Guid packageId);
    }
}
