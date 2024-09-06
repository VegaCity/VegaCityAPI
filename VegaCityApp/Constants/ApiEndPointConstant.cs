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
    }
    public static class EtagTypeEndpoint
    {
        public const string CreateEtagType = ApiEndpoint + "/etag-type";
        public const string DeleteEtagType = ApiEndpoint + "/etag-type";
    }
    public static class EtagEndpoint
    {
        public const string CreateEtag = ApiEndpoint + "/etag";
        public const string DeleteEtag = ApiEndpoint + "/etag";
    }
    public static class UserEndpoint
    {
        public const string ApproveUser = ApiEndpoint + "/approve-user";
        public const string GetListUser = ApiEndpoint + "/users";
        public const string GetListUserByRoleId = ApiEndpoint + "/users";
        public const string UpdateUserRoleById = ApiEndpoint + "/user";
        public const string GetUserInfo = ApiEndpoint + "/user";
        public const string UpdateUserProfile = ApiEndpoint + "/user";
        public const string DeleteUser = ApiEndpoint + "/user";
    }

    public static class packageEndpoint
    {
        public const string CreatePackage = ApiEndpoint + "/package";
        public const string UpdatePackage = ApiEndpoint + "/package";
        public const string GetListPackage = ApiEndpoint + "/packages";
        public const string GetPackageById = ApiEndpoint + "/package";
        public const string DeletePackage = ApiEndpoint + "/package";
    }
}