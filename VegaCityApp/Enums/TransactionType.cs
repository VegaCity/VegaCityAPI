namespace VegaCityApp.API.Enums
{
    public class TransactionType
    {
        public const string EndDayCheckWalletCashier = "EndDayCheckWalletCashier";
        public const string WithdrawMoney = "WithdrawMoney";
        public const string ChargeMoney = "ChargeMoney";
        public const string Payment = "Payment";
        public const string StoreTransfer = "StoreTransfer";
    }
    public class TransactionStatus
    {
        public const string Success = "Success";
        public const string Pending = "Pending";
    }
    public enum CurrencyEnum
    {
        VND = 1,
        USD = 2
    }
}
