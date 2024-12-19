namespace VegaCityApp.API.Enums
{
    public class TransactionType
    {
        public const string EndDayCheckWalletCashierBalance = "EndDayCheckWalletCashierBalance";
        public const string EndDayCheckWalletCashierBalanceHistory = "EndDayCheckWalletCashierBalanceHistory";
        public const string WithdrawMoney = "WithdrawMoney";
        public const string ChargeMoney = "ChargeMoney";
        public const string Payment = "Payment";
        public const string StoreTransfer = "StoreTransfer";
        public const string ReceiveMoneyToCashier = "ReceiveMoneyToCashier";
        public const string RefundMoney = "RefundMoney";
        public const string SellingPackage = "SellingPackage";
        public const string SellingProduct = "SellingProduct";
        public const string SellingService = "SellingService";
        public const string TransferMoneyToVega = "TransferMoneyToVega";
        public const string TransferMoneyToStore = "TransferMoneyToStore";
        public const string FeeLost = "FeeLost";
    }
    public class TransactionStatus
    {
        public const string Success = "Success";
        public const string Pending = "Pending";
        public const string Fail = "Cancel";
    }
    public enum CurrencyEnum
    {
        VND = 1,
        USD = 2
    }
}
