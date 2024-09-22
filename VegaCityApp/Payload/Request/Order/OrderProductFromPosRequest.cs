namespace VegaCityApp.API.Payload.Request.Order
{
    public class OrderProductFromPosRequest
    {
        public string Id { get; set; } // product id, package id, etag id
        public string Name { get; set; } // product name, package name, etag name
        public string? ProductCategory { get; set; } // product category, etag type
        public int Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
    }
}
