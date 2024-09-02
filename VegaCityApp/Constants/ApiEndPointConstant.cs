namespace VegaCityApp.API.Constants;

public static class ApiEndPointConstant
{
    static ApiEndPointConstant()
    {
    }

    public const string RootEndPoint = "/api";
    public const string ApiVersion = "/v1";
    public const string ApiEndpoint = RootEndPoint + ApiVersion;

    public static class Authentication
    {
        public const string AuthenticationEndpoint = ApiEndpoint + "/auth";
        public const string Login = AuthenticationEndpoint + "/login";
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
}