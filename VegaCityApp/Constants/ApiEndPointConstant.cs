namespace VegaCityApp.API.Constants;

public static class ApiEndPointConstant
{
    static ApiEndPointConstant()
    {
    }

    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;

    public static class AuthenticationEndpoint
    {
        public const string Authentication = ApiEndpoint + "/auth";
        public const string Login = Authentication + "/login";
        public const string Register = Authentication + "/sign-up/landing-page";
        public const string ChangePassword = Authentication + "/change-password";
        public const string RefreshToken = Authentication + "/refresh-token";
        public const string GetRefreshTokenByEmail = Authentication + "/refresh-token/{email}";
    }
    public static class MarketZoneEndpoint
    {
        public const string CreateMarketZone = ApiEndpoint + "/market-zone";
        public const string UpdateMarketZone = ApiEndpoint + "/market-zone";
        public const string GetListMarketZone = ApiEndpoint + "/market-zones";
        public const string GetMarketZone = ApiEndpoint + "/market-zone/{id}";
        public const string DeleteMarketZone = ApiEndpoint + "/market-zone/{id}";
    }
    public static class EtagEndpoint
    {
        public const string CreateEtag = ApiEndpoint + "/etag";
        public const string DeleteEtag = ApiEndpoint + "/etag/{id}";
        public const string GenerateEtag = ApiEndpoint + "/generate-etag";
        public const string UpdateEtag = ApiEndpoint + "/etag/{id}";
        public const string SearchEtag = ApiEndpoint + "/etag";
        public const string SearchAllEtag = ApiEndpoint + "/etags";
        public const string ActivateEtag = ApiEndpoint + "/etag/{id}/activate";
        public const string ChargeMoneyETag = ApiEndpoint + "/etag/charge-money";
        public const string EtagPayment = ApiEndpoint + "/etag/payment";
    }
    public static class UserEndpoint
    {
        public const string GetAllClosingRequest = ApiEndpoint + "/user/closing-requests";
        public const string GetClosingRequest = ApiEndpoint + "/user/closing-request";
        public const string GetSession = ApiEndpoint + "/user/session/{id}";
        public const string DeleteSession = ApiEndpoint + "/user/session/{id}";
        public const string GetAllSessions = ApiEndpoint + "/user/sessions";
        public const string CreateSession = ApiEndpoint + "/user/{id}/session";
        public const string ApproveUser = ApiEndpoint + "/user/{userId}/approve-user";
        public const string GetListUser = ApiEndpoint + "/users";
        public const string GetListUserByRoleId = ApiEndpoint + "/users";
        public const string UpdateUserRoleById = ApiEndpoint + "/user";
        public const string GetUserInfo = ApiEndpoint + "/user/{id}";
        public const string UpdateUserProfile = ApiEndpoint + "/user/{id}";
        public const string DeleteUser = ApiEndpoint + "/user/{id}";
        public const string CreateUser = ApiEndpoint + "/user";
        public const string GetAdminWallet = ApiEndpoint + "/wallet";
        public const string GetChartDashboard = ApiEndpoint + "/transaction/dashboard";
        public const string ReAssignEmail = ApiEndpoint + "/user/{userId}/re-assign-email";
        public const string ResolveClosingRequest = ApiEndpoint + "/user/resolve-closing-request";
    }

    public static class PackageEndpoint
    {
        public const string UpdateRfId = ApiEndpoint + "/package-item/{id}/rfid";
        public const string ConfirmOrder = ApiEndpoint + "/order/confirm";
        public const string PackageItemPayment = ApiEndpoint + "/package-item/payment";
        public const string PrepareChargeMoney = ApiEndpoint + "/package-item/charge-money";
        public const string ActivePackageItem = ApiEndpoint + "/package-item/{id}/activate";
        public const string CreatePackage = ApiEndpoint + "/package";
        public const string UpdatePackage = ApiEndpoint + "/package/{id}";
        public const string GetListPackage = ApiEndpoint + "/packages";
        public const string GetPackageById = ApiEndpoint + "/package/{id}";
        public const string DeletePackage = ApiEndpoint + "/package/{id}";
        public const string GetListPackageType = ApiEndpoint + "/package-types";
        public const string GetPackageTypeById = ApiEndpoint + "/package-type/{id}";
        public const string CreatePackageType = ApiEndpoint + "/package-type";
        public const string UpdatePackageType = ApiEndpoint + "/package-type/{id}";
        public const string DeletePackageType = ApiEndpoint + "/package-type/{id}";
        public const string GetListPackageItem = ApiEndpoint + "/package-items";
        public const string GetPackageItemById = ApiEndpoint + "/package-item";
        public const string CreatePackageItem = ApiEndpoint + "/package-item";
        public const string UpdatePackageItem = ApiEndpoint + "/package-item/{id}";
        public const string MarkPackageItemLost = ApiEndpoint + "/package-item/mark-lost";

    }

    public static class ZoneEndPoint
    {
        public const string CreateZone = ApiEndpoint + "/zone";
        public const string UpdateZone = ApiEndpoint + "/zone/{id}";
        public const string SearchAllZone = ApiEndpoint + "/zones";
        public const string SearchZone = ApiEndpoint + "/zone/{id}";
        public const string DeleteZone = ApiEndpoint + "/zone/{id}";
    }
    public static class StoreEndpoint
    {
        public const string UpdateStore = ApiEndpoint + "/store/{id}";
        public const string GetListStore = ApiEndpoint + "/stores";
        public const string GetStore = ApiEndpoint + "/store/{id}";
        public const string DeleteStore = ApiEndpoint + "/store/{id}";
        public const string GetMenu = ApiEndpoint + "/store/{phone}/menu";
        public const string GetWalletStore = ApiEndpoint + "/store/wallet";
        public const string RequestCloseForStore = ApiEndpoint + "/store/{id}/request-close";
    }
    public static class HouseEndpoint
    {
        public const string UpdateHouse = ApiEndpoint + "/house/{id}";
        public const string GetListHouse = ApiEndpoint + "/houses";
        public const string GetHouse = ApiEndpoint + "/house/{id}";
        public const string DeleteHouse = ApiEndpoint + "/house/{id}";
        public const string CreateHouse = ApiEndpoint + "/house";
    }
    //order endpoit
    public static class OrderEndpoint
    {
        public const string ConfirmOrderForCashier = ApiEndpoint + "/order/cashier/confirm";
        public const string GetListOrder = ApiEndpoint + "/orders";
        public const string GetOrder = ApiEndpoint + "/order";
        public const string CancelOrder = ApiEndpoint + "/order/{id}";
        public const string CreateOrder = ApiEndpoint + "/order";
        public const string CreateOrderForCashier = ApiEndpoint + "/order/cashier";
        public const string UpdateOrder = ApiEndpoint + "/order";
        public const string ConfirmOrder = ApiEndpoint + "/order/confirm";
    }
    public static class PaymentEndpoint
    {
        public const string ZaloPayPayment = ApiEndpoint + "/payment/zalopay";
        public const string MomoPayment = ApiEndpoint + "/payment/momo";
        public const string VnPayPayment = ApiEndpoint + "/payment/vnpay";
        public const string VisaCardPayment = ApiEndpoint + "/payment/visa";
        public const string UpdateOrderPaid = ApiEndpoint + "/payment/momo/order";
        public const string UpdateOrderPaidZaloPay = ApiEndpoint + "/payment/zalopay/order";
        public const string UpdateOrderPaidForChargingMoney = ApiEndpoint + "/payment/momo/order/charge-money";
        public const string UpdateOrderPaidForChargingMoneyZaloPay = ApiEndpoint + "/payment/zalopay/order/charge-money";
        public const string UpdateOrderVnPaidForChargingMoney = ApiEndpoint + "/payment/vnpay/order/charge-money";
        // public const string UpdateOrderPaidOSForChargingMoney = ApiEndpoint + "/payment/payos/order/charge-money";
        public const string UpdateVnPayOrder = ApiEndpoint + "/payment/vnpay/order";
        public const string PayOSPayment = ApiEndpoint + "/payment/payos";
        public const string UpdatePayOSOrder = ApiEndpoint + "/payment/payos/order";
        public const string UpdateOrderPaidOSForChargingMoney = ApiEndpoint + "/payment/payos/order/charge-money";
    }
    public static class WalletTypeEndpoint
    {
        public const string RequestWithdrawMoneyWallet = ApiEndpoint + "/wallet/{walletid}/request-withdraw-money";
        public const string WithdrawMoneyWallet = ApiEndpoint + "/wallet/{walletid}/withdraw-money";
        public const string AddServiceStoreToWalletType = ApiEndpoint + "/wallet-type/{id}/service-store/{serviceStoreId}";
        public const string RemoveServiceStoreToWalletType = ApiEndpoint + "/wallet-type/{id}/service-store/{serviceStoreId}";
        public const string CreateWalletType = ApiEndpoint + "/wallet-type";
        public const string UpdateWalletType = ApiEndpoint + "/wallet-type/{id}";
        public const string DeleteWalletType = ApiEndpoint + "/wallet-type/{id}";
        public const string GetWalletTypeById = ApiEndpoint + "/wallet-type/{id}";
        public const string GetAllWalletType = ApiEndpoint + "/wallet-types";
    }
    public static class ServiceStoreEndpoint
    {
        public const string CreateServiceStore = ApiEndpoint + "/service-store";
        public const string UpdateServiceStore = ApiEndpoint + "/service-store/{id}";
        public const string DeleteServiceStore = ApiEndpoint + "/service-store/{id}";
        public const string GetServiceStoreById = ApiEndpoint + "/service-store/{id}";
        public const string GetAllServiceStore = ApiEndpoint + "/service-stores";
    }
    public static class TransactionEndpoint
    {
        public const string GetListTransaction = ApiEndpoint + "/transactions";
        public const string GetTransaction = ApiEndpoint + "/transaction/{id}";
        public const string DeleteTransaction = ApiEndpoint + "/transaction/{id}";
    }
    public static class ReportEndpoint
    {
        public const string CreateIssueType = ApiEndpoint + "/report/issue-type";
        public const string DeleteIssueType = ApiEndpoint + "/report/issue-type/{id}";
        public const string CreateReport = ApiEndpoint + "/report";
        public const string UpdateReport = ApiEndpoint + "/report/{id}";
        public const string GetListIssueType = ApiEndpoint + "/report/issue-types";
        public const string GetListReports = ApiEndpoint + "/reports";
    }
    public static class PromotionEndPoint
    {
        public const string CreatePromotion = ApiEndpoint + "/promotion";
        public const string UpdatePromotion = ApiEndpoint + "/promotion/{id}";
        public const string SearchAllPromotions = ApiEndpoint + "/promotions";
        public const string SearchPromotion = ApiEndpoint + "/promotion/{id}";
        public const string DeletePromotion = ApiEndpoint + "/promotion/{id}";
    }
}