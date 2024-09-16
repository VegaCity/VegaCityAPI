namespace VegaCityApp.API.Enums
{
    public enum OrderStatusEnum
    {
        Complete,
        Canceled,
        Pending
    }
    public class OrderStatus
    {
        public const string Pending = "PENDING";
        public const string Completed = "COMPLETED";
        public const string Canceled = "CANCELED";
    }
}
