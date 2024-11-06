using System.Text.Json.Serialization;
using Hangfire;
using NLog;
using NLog.Web;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Converter;
using VegaCityApp.API.Extensions;
using VegaCityApp.API.Middlewares;
using VegaCityApp.API.Services;

var logger = LogManager.LoadConfiguration(string.Concat(Directory.GetCurrentDirectory(), "/nlog.config")).GetCurrentClassLogger();
try
{
    var builder = WebApplication.CreateBuilder(args);
    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    // Add services to the container.
    builder.Services.AddCors(options =>
    {
        options.AddPolicy(name: CorsConstant.PolicyName,
            policy => { policy.WithOrigins("*").AllowAnyHeader().AllowAnyMethod(); });
    });
    builder.Services.AddControllers().AddJsonOptions(x =>
    {
        x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        x.JsonSerializerOptions.Converters.Add(new TimeOnlyJsonConverter());
        x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });
    builder.Services.AddDatabase();
    builder.Services.AddUnitOfWork();
    builder.Services.AddServices(builder.Configuration);
    builder.Services.AddJwtValidation();
    builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddConfigSwagger();
    builder.Services.AddPayOS(builder.Configuration);
    var app = builder.Build();

    // Configure the HTTP request pipeline.
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseMiddleware<ExceptionHandlingMiddleware>();


    //app.UseHttpsRedirection();
    app.UseCors(CorsConstant.PolicyName);
    app.UseAuthentication();
    app.UseAuthorization();
    
    app.UseHangfireDashboard();
    app.MapHangfireDashboard();
    
    //BackgroundJobs.RecurringJobs();
    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stop program because of exception");
}
finally
{
    LogManager.Shutdown();
}