namespace VegaCityApp.API.Payload.Request.Order
{
    public class OrderProductFromPosRequest
    {
        public string? Id { get; set; } // product id, service Id
        public string? Name { get; set; } // product name, serviceName
        public string? ProductCategory { get; set; } // product category
        public int Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
    }
    public class OrderProductFromCashierRequest
    {
        public string Id { get; set; } // package id
        public string Name { get; set; } // package name
        public int Price { get; set; }
        public string? ImgUrl { get; set; }
        public int Quantity { get; set; }
    }
}
