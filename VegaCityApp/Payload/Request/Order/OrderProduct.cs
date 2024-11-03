namespace VegaCityApp.API.Payload.Request.Order
{
    public class OrderProduct
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
        public string Note { get; set; }
        public int Price { get; set; }
    }
}
