namespace VegaCityApp.API.Payload.Request.Order
{
    public class OrderProductFromPosRequest
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string ProductCategory { get; set; }
        public int Price { get; set; }
        public string ImgUrl { get; set; }
        public int Quantity { get; set; }
    }
}
