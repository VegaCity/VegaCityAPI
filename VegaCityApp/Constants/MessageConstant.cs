namespace VegaCityApp.API.Constants;

public static class MessageConstant
{
    public static class HttpStatusCodes
    {
        // 1xx Informational
        public const int Continue = 100;
        public const int SwitchingProtocols = 101;
        public const int Processing = 102;

        // 2xx Success
        public const int OK = 200;
        public const int Created = 201;
        public const int Accepted = 202;
        public const int NonAuthoritativeInformation = 203;
        public const int NoContent = 204;
        public const int ResetContent = 205;
        public const int PartialContent = 206;
        public const int MultiStatus = 207;
        public const int AlreadyReported = 208;
        public const int IMUsed = 226;

        // 3xx Redirection
        public const int MultipleChoices = 300;
        public const int MovedPermanently = 301;
        public const int Found = 302;
        public const int SeeOther = 303;
        public const int NotModified = 304;
        public const int UseProxy = 305;
        public const int TemporaryRedirect = 307;
        public const int PermanentRedirect = 308;

        // 4xx Client Error
        public const int BadRequest = 400;
        public const int Unauthorized = 401;
        public const int PaymentRequired = 402;
        public const int Forbidden = 403;
        public const int NotFound = 404;
        public const int MethodNotAllowed = 405;
        public const int NotAcceptable = 406;
        public const int ProxyAuthenticationRequired = 407;
        public const int RequestTimeout = 408;
        public const int Conflict = 409;
        public const int Gone = 410;
        public const int LengthRequired = 411;
        public const int PreconditionFailed = 412;
        public const int PayloadTooLarge = 413;
        public const int URITooLong = 414;
        public const int UnsupportedMediaType = 415;
        public const int RangeNotSatisfiable = 416;
        public const int ExpectationFailed = 417;
        public const int ImATeapot = 418;
        public const int MisdirectedRequest = 421;
        public const int UnprocessableEntity = 422;
        public const int Locked = 423;
        public const int FailedDependency = 424;
        public const int TooEarly = 425;
        public const int UpgradeRequired = 426;
        public const int PreconditionRequired = 428;
        public const int TooManyRequests = 429;
        public const int RequestHeaderFieldsTooLarge = 431;
        public const int UnavailableForLegalReasons = 451;

        // 5xx Server Error
        public const int InternalServerError = 500;
        public const int NotImplemented = 501;
        public const int BadGateway = 502;
        public const int ServiceUnavailable = 503;
        public const int GatewayTimeout = 504;
        public const int HTTPVersionNotSupported = 505;
        public const int VariantAlsoNegotiates = 506;
        public const int InsufficientStorage = 507;
        public const int LoopDetected = 508;
        public const int NotExtended = 510;
        public const int NetworkAuthenticationRequired = 511;
    }

    public static class LoginMessage
    {
        public const string InvalidUsernameOrPassword = "Tên đăng nhập hoặc mật khẩu không chính xác";
        public const string DeactivatedAccount = "Tài khoản đang bị vô hiệu hoá";
    }
    public static class EtagTypeMessage
    {
        public const string CreateSuccessFully = "Create EtagType Successfully !!";
        public const string CreateFail = "Create EtagType Fail :( !!";
        public const string NotFoundEtagType = "Not Found EtagType !!";
        public const string DeleteEtagTypeSuccessfully = "Delete EtagType Successfully !!";
        public const string DeleteEtagTypeFail = "Delete EtagType Fail :( !!";
        public const string UpdateSuccessFully = "Update EtagType Successfully !!";
        public const string UpdateFail = "Update EtagType Fail :( !!";
    }
    public static class EtagMessage
    {
        public const string UserNotFound = "User Not Found !!";
        public const string EtagTypeNotFound = "EtagType Not Found !!";
        public const string CreateFail = "Create Etag Fail :( !!";
        public const string MarketZoneNotFound = "MarketZone Not Found !!";
        public const string GetEtagsSuccess = "Get List Etags Successfully !!";
    }
    public static class UserMessage
    {   public const string UnauthorizedAccess = "Unauthorized Access !!";
        public const string EmailExistOrPhoneOrCCCDExit = "Email or PhoneNumber or CCCD already exist !!";
        public const string Ban = "Email is banned !!";
        public const string CreateUserFail = "Create User Fail !!";
        public const string InvalidEmail = "Invalid Email !!";
        public const string InvalidPhoneNumber = "Invalid PhoneNumber !!";
        public const string InvalidCCCD = "Invalid CCCD !!";
        public const string CreateSuccessfully = "Create User Successfully !!";
        public const string UserNotFound = "User Not Found !!";
        public const string ApproveSuccessfully = "Approve User Successfully !!";
        public const string ApproveFail = "Approve User Fail !!";
        public const string ApproveReject = "Approve User Reject !!";
        public const string Approved = "User is approved !!";
        public const string PendingVerify = "User is pending verify !!";
        public const string UserDisable = "User is disable !!";
        public const string UserBan = "User is ban !!";
        public const string WrongPassword = "Wrong Password !!";
        public const string LoginSuccessfully = "Login Successfully !!";
        public const string LoginFail = "Login Fail !!";
        public const string ChangePasswordSuccessfully = "Change Password Successfully !!";
        public const string OldPasswordNotDuplicate = "Old Password is not duplicate !!";
        public const string GetListSuccess = "Get User List Successfully !!";
        public const string NotFoundUser = "Not Found User !!";
        public const string UpdateUserSuccessfully = "Update User Successfully !!";
        public const string FailedToUpdate = "Update User Failed :(!";
        public const string DeleteUserSuccess = "Delete User Successfully!";
        public const string DeleteUserFail = "Delete User Fail!";
        public const string VegaCityResponse = "VegaCity Response";
        public const string PasswordIsChanged = "Password is changed!";
        public const string CreateWalletFail = "Create Wallet Fail !!";
    }

    public static class RoleMessage
    {
        public const string GetListByRoleSuccessfully = "Get List User By Role Successfully !!";
        public const string GetListByRoleFail = "Get List User By Role Fail !!";
        public const string GetListByRoleNotFound = "Not Found User By Role !!";
    }

    public static class walletMessage
    {
        public const string CreateWalletSuccessfully = "Create Wallet Successfully !!";
        public const string NotFound = "Not Found Wallet !!";
        public const string FoundSuccess = "Get Wallet Successfully !!";
    }

    public static class OrderMessage
    {
        public const string CreateOrderSuccessfully = "Create Order Successfully !!";
        public const string CreateOrderFail = "Create Order Fail !!";
        public const string GetOrdersSuccessfully = "Get Orders Successfully !!";
        public const string GetOrdersFail = "Get Orders Fail !!";
    }

    public static class PackageMessage
    {
        public const string CreatePackageSuccessfully = "Create Package Successfully !!";
        public const string CreatePackageFail = "Create Package Fail !!";
        public const string GetPackagesSuccessfully = "Get Packages Successfully !!";
        public const string GetPackagesFail = "Get Packages Fail !!";
        public const string NotFoundPackage = "Not Found Package !!";
        public const string FoundPackage = "Get Package Successfully !!";
        public const string UpdatePackageSuccessfully = "Update Package Successfully !!";
        public const string MKZoneNotFound = "MarketZone Was Not Found !!";
        public const string ExistedPackageName = "Package Name is existed !!";
        public const string NotFoundETagType = "ETagType Was Not Found !!";
        public const string UpdatePackageFailed = "Update Package Failed !!";
        public const string EndateInThePast = "End date cannot be in the past.";
        public const string SameStrAndEndDate = "StartDate and EndDate must Not be Same";
        public const string durationLimit = "The duration between start and end date must be at least 48 hours.";
        public const string InvalidDuration = "Invalid DateTime Duration";
        public const string DeleteSuccess = "Delete Package Successfully!";
        public const string DeleteFail = "Delete Package Failed!";
    }

    public static class ZoneMessage
    {
        public const string CreateZoneSuccess = "A New Zone Has Created Successfully!";
        public const string CreateZoneFail = "Failed To Create New Zone";
        public const string UpdateZoneSuccess = "Update Zone Successfully!";
        public const string UpdateZoneFail = "Failed To Update Zone";
        public const string SearchZonesSuccess = "Get List Zones Successfully!";
        public const string SearchZonesFail = "Get List Zones Failed";
        public const string SearchZoneSuccess = "Get Zone Successfully!";
        public const string SearchZoneFail = "Get Zone Failed";
        public const string DeleteZoneSuccess = "Delete Zone Successfully!";
        public const string DeleteZoneFailed = "Failed To Delete Zone";
        public const string HouseStillExist = "Failed To Update Location Since House Still Intact!";
        public const string ZoneExisted = "Zone Name Existed!";
    }
    public static class StoreMessage
    {
        public const string UpdateStoreSuccesss = "Update Store Successfully!";
        public const string UpdateStoreFailed = "Update Store Failed";
        public const string GetListStoreSuccess = "Get List Store Successfully!";
        public const string GetListStoreFailed = "Get List Store Failed";
        public const string GetStoreSuccess = "Get Store Successfully!";
        public const string GetStoreFail = "Get Store Detail Failed!";
        public const string NotFoundStore = "Not Found Store";
        public const string DeletedSuccess = "Delete Store Successfully!";
        public const string DeleteFailed = "Delete Store Failed";
        public const string InvalidStoreType = "Invalid Store Type";
    }
    public static class HouseMessage
    {
        public const string CreateHouseSuccessfully = "Create House Successfully !!";
        public const string CreateHouseFail = "Create House Fail !!";
        public const string GetHousesSuccessfully = "Get Houses Successfully !!";
        public const string GetHousesFail = "Get Houses Fail !!";
        public const string NotFoundHouse = "Not Found House !!";
        public const string FoundHouse = "Get House Successfully !!";
        public const string UpdateHouseSuccessfully = "Update House Successfully !!";
        public const string MKZoneNotFound = "MarketZone Was Not Found !!";
        public const string ExistedHouseName = "House Name is existed !!";
        public const string UpdateHouseFailed = "Update House Failed !!";
        public const string DeleteSuccess = "Delete House Successfully!";
        public const string DeleteFail = "Delete House Failed!";
        public const string NotFoundZone = "Zone Not Found!";
        public const string HouseDeleted = "House is deleted!";
    }
}