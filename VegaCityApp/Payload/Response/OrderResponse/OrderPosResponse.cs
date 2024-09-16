namespace VegaCityApp.API.Payload.Response.OrderResponse
{
    public class OrderPosResponse
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductCategory { get; set; }
        public int Price { get; set; }
        public int Quantity {get; set; }
    }
}
