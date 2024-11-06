using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Enums
{
    public class OrderStatus
    {
        public const string Pending = "PENDING";
        public const string Completed = "COMPLETED";
        public const string Canceled = "CANCELED";
    }
    public class SaleType
    {
        public const string PackageItemCharge = "PackageItem Charge";
        public const string Package = "Package";
        public const string StoreService = "StoreService";
        public const string PackageItemPayment = "PackageItem Payment";
        public const string Product = "Product";
    }
}
