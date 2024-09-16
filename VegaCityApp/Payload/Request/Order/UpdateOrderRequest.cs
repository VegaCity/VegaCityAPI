using VegaCityApp.API.Payload.Response.OrderResponse;

namespace VegaCityApp.API.Payload.Request.Order
{
    public class UpdateOrderRequest
    {
        public string? PaymentType { get; set; }
        public List<OrderProductFromPosRequest> NewProducts { get; set; }
        public int TotalAmount { get; set; }
        public Guid? EtagId { get; set; }
    }

    public class OrderPosUpdateRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
