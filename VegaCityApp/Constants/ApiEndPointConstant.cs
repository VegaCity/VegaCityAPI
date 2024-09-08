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
    }
    public static class EtagTypeEndpoint
    {
        public const string CreateEtagType = ApiEndpoint + "/etag-type";
        public const string DeleteEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string UpdateEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string SearchEtagType = ApiEndpoint + "/etag-type/{id}";
        public const string SearchAllEtagType = ApiEndpoint + "/etag-types";
    }
    public static class EtagEndpoint
    {
        public const string CreateEtag = ApiEndpoint + "/etag";
        public const string DeleteEtag = ApiEndpoint + "/etag";
    }
    public static class UserEndpoint
    {
        public const string ApproveUser = ApiEndpoint + "/approve-user/{userId}";
        public const string GetListUser = ApiEndpoint + "/users";
        public const string GetListUserByRoleId = ApiEndpoint + "/users";
        public const string UpdateUserRoleById = ApiEndpoint + "/user";
        public const string GetUserInfo = ApiEndpoint + "/user/{id}";
        public const string UpdateUserProfile = ApiEndpoint + "/user/{id}";
        public const string DeleteUser = ApiEndpoint + "/user";
    }

    public static class PackageEndpoint
    {
        public const string CreatePackage = ApiEndpoint + "/package";
        public const string UpdatePackage = ApiEndpoint + "/package/{id}";
        public const string GetListPackage = ApiEndpoint + "/packages";
        public const string GetPackageById = ApiEndpoint + "/package/{id}";
        public const string DeletePackage = ApiEndpoint + "/package/{id}";
    }
    public static class StoreEndpoint
    {
        //store crud endponit like above

        public const string UpdateStore = ApiEndpoint + "/store/{id}";
        public const string GetListStore = ApiEndpoint + "/stores";
        public const string GetStore = ApiEndpoint + "/store/{id}";

        public const string DeleteStore = ApiEndpoint + "/store/{id}";

    }
}