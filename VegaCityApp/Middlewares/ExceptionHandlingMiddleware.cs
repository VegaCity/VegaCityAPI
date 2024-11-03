namespace VegaCityApp.API.Middlewares;

using System.Net;

using VegaCityApp.API.Payload.Response;

public class ExceptionHandlingMiddleware
{
	private readonly RequestDelegate _next;
	private readonly ILogger<ExceptionHandlingMiddleware> _logger;
	public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
	{
		_next = next;
		_logger = logger;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		try
		{
			await _next(context);
		}
		catch (Exception ex)
		{
            Exception exception = ex.InnerException ?? ex;
            await HandleExceptionAsync(context, exception);
		}
	}

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        var response = context.Response;
        var errorResponse = new ErrorResponse() { TimeStamp = DateTime.UtcNow, Error = exception.Message };

        switch (exception)
        {
            case BadHttpRequestException badHttpRequestException:
                // Lấy status code từ BadHttpRequestException
                response.StatusCode = badHttpRequestException.StatusCode;
                errorResponse.StatusCode = badHttpRequestException.StatusCode;
                _logger.LogInformation(exception.Message);
                break;

            // Thêm các exception tùy chỉnh khác tại đây nếu có
            // Ví dụ: case AppException appException: làm gì đó

            default:
                // Unhandled error
                response.StatusCode = (int)HttpStatusCode.InternalServerError;
                errorResponse.StatusCode = (int)HttpStatusCode.InternalServerError;
                _logger.LogError(exception.ToString());
                break;
        }

        var result = errorResponse.ToString();
        await context.Response.WriteAsync(result);
    }

}