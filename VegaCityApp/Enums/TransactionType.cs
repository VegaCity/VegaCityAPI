namespace VegaCityApp.API.Enums
{
    public class TransactionType
    {
        public const string EndDayCheckWalletCashier = "EndDayCheckWalletCashier";
        public const string WithdrawMoney = "WithdrawMoney";
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
