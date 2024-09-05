using System.Text;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using Microsoft.EntityFrameworkCore;
using VegaCityApp.API.Constants;
using VegaCityApp.Domain.Models;
using VegaCityApp.Repository.Implement;
using VegaCityApp.Repository.Interfaces;
using System.Reflection;
using VegaCityApp.Service.Interface;
using VegaCityApp.Service.Implement;
using Microsoft.Extensions.Configuration;
using VegaCityApp.API.Services.Interface;
using VegaCityApp.API.Services.Implement;

namespace VegaCityApp.API.Extensions;
public static class DependencyServices
{
    public static IServiceCollection AddUnitOfWork(this IServiceCollection services)
    {
        services.AddScoped<IUnitOfWork<VegaCityAppContext>, UnitOfWork<VegaCityAppContext>>();
        return services;
    }

    public static IServiceCollection AddDatabase(this IServiceCollection services)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(EnvironmentVariableConstant.Prefix).Build();
        services.AddDbContext<VegaCityAppContext>(options => options.UseSqlServer(CreateConnectionString(configuration)));
        return services;
    }

    private static string CreateConnectionString(IConfiguration configuration)
    {
        //string connectionString =
        //    $"Server={configuration.GetValue<string>(DatabaseConstant.Host)},{configuration.GetValue<string>(DatabaseConstant.Port)};User Id={configuration.GetValue<string>(DatabaseConstant.UserName)};Password={configuration.GetValue<string>(DatabaseConstant.Password)};Database={configuration.GetValue<string>(DatabaseConstant.Database)}";
        #region varDb
        string Host = "14.225.204.144";
        string UserName = "vegadb";
        string Password = "vega12345";
        string Database = "VegaCityApp";
        string Port = "1433";
        #endregion
        string connectionString =
            $"Server={Host},{Port};User Id={UserName};Password={Password};Database={Database}";
        return connectionString;
    }

    public static IServiceCollection AddServices(this IServiceCollection services, IConfiguration config)
    {
        #region Firebase
        //string firebaseCred = config.GetValue<string>("Authentication:FirebaseKey");
        //// string firebaseCred = config.GetValue<string>("AIzaSyCFJOGAnHOQaWntVhN1a16QINIAjVpWaXI");
        //FirebaseApp.Create(new AppOptions()
        //{
        //    Credential = GoogleCredential.FromJson(firebaseCred)
        //}, "[DEFAULT]");
        #endregion
        #region addScope
        services.AddScoped<IAccountService, AccountService>();
        services.AddScoped<IEtagService, EtagService>();
        services.AddScoped<IPackageService, PackageService>();
        #endregion
        return services;
    }

    public static IServiceCollection AddJwtValidation(this IServiceCollection services)
    {
        #region varKey
        string issuer = "VegaCityApp";
        string secretKey = "VegaCityApp";
        #endregion
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(EnvironmentVariableConstant.Prefix).Build();
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidIssuer = issuer,
                ValidateIssuer = true,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(secretKey))
            };
        });
        return services;
    }

    public static IServiceCollection AddConfigSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo() { Title = "Vega City App", Version = "v1" });
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
            options.MapType<TimeOnly>(() => new OpenApiSchema
            {
                Type = "string",
                Format = "time",
                Example = OpenApiAnyFactory.CreateFromJson("\"13:45:42.0000000\"")
            });
        });
        return services;
    }
}