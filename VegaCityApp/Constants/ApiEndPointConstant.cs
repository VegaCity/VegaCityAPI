﻿namespace VegaCityApp.API.Constants;

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
    }
    public static class EtagTypeEndpoint
    {
        public const string CreateEtagType = ApiEndpoint + "/etag-type";
        public const string DeleteEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string UpdateEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string SearchEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string SearchAllEtagType = ApiEndpoint + "/etag-types";
        public const string AddEtagTypeToPackage = ApiEndpoint + "/etag-type/{etagTypeId}/package/{packageId}";
        public const string RemoveEtagTypeFromPackage = ApiEndpoint + "/etag-type/{etagTypeId}/package/{packageId}";
    }
    public static class EtagEndpoint
    {
        public const string CreateEtag = ApiEndpoint + "/etag";
        public const string DeleteEtag = ApiEndpoint + "/etag/{id}";
        public const string GenerateEtag = ApiEndpoint + "/generate-etag";
        public const string UpdateEtag = ApiEndpoint + "/etag/{id}";
        public const string SearchEtag = ApiEndpoint + "/etag/{id}";
        public const string SearchAllEtag = ApiEndpoint + "/etags";
        public const string ActivateEtag = ApiEndpoint + "/etag/{id}/activate";
    }
    public static class UserEndpoint
    {
        public const string ApproveUser = ApiEndpoint + "/approve-user/{userId}";
        public const string GetListUser = ApiEndpoint + "/users";
        public const string GetListUserByRoleId = ApiEndpoint + "/users";
        public const string UpdateUserRoleById = ApiEndpoint + "/user";
        public const string GetUserInfo = ApiEndpoint + "/user/{id}";
        public const string UpdateUserProfile = ApiEndpoint + "/user/{id}";
        public const string DeleteUser = ApiEndpoint + "/user/{id}";
        public const string CreateUser = ApiEndpoint + "/user";

    }

    public static class PackageEndpoint
    {
        public const string CreatePackage = ApiEndpoint + "/package";
        public const string UpdatePackage = ApiEndpoint + "/package/{id}";
        public const string GetListPackage = ApiEndpoint + "/packages";
        public const string GetPackageById = ApiEndpoint + "/package/{id}";
        public const string DeletePackage = ApiEndpoint + "/package/{id}";
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
        public const string GetMenu = ApiEndpoint + "/store/{id}/menu";
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
    }
    public static class PaymentEndpoint
    {
        public const string MomoPayment = ApiEndpoint + "/payment/momo";
        public const string ZaloPayment = ApiEndpoint + "/payment/zalo";
        public const string VnPayPayment = ApiEndpoint + "/payment/vnpay";
        public const string VisaCardPayment = ApiEndpoint + "/payment/visa";
        public const string UpdateOrderPaid = ApiEndpoint + "/payment/momo/order";
        public const string UpdateVnPayOrder = ApiEndpoint + "/payment/vnpay/order";
    }
}