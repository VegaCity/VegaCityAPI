using VegaCityApp.API.Payload.Response.OrderResponse;

namespace VegaCityApp.API.Payload.Request.Order
{
    public class UpdateOrderRequest
    {
        public double? VATRate { get; set; }
        public List<OrderPosResponse> NewProducts { get; set; }
        public Guid? EtagId { get; set; }
    }

    public class OrderPosUpdateRequest
    {
        public string ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
