using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using VegaCityApp.API.Constants;
using VegaCityApp.API.Enums;
using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Utils;

public class JwtUtil
{
    private JwtUtil()
    {
    }

    public static string GenerateJwtToken(User user, Tuple<string, Guid> guidClaim)
    {
        #region varKey
        string issuerKey = "VegaCityApp";
        string secretKey = "VegaCityAppsecretKey";
        #endregion
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(EnvironmentVariableConstant.Prefix).Build();
        JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
        SymmetricSecurityKey secrectKey =
            //new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>(JwtConstant.SecretKey)));
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
        //string issuer = configuration.GetValue<string>(JwtConstant.Issuer);
        string issuer = issuerKey;
        List<Claim> claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Jti, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
        };
        if (guidClaim != null) claims.Add(new Claim(guidClaim.Item1, guidClaim.Item2.ToString()));
        var expires = user.Role.Name.Equals(RoleEnum.Admin.GetDescriptionFromEnum())
            ? TimeUtils.GetCurrentSEATime().AddDays(1)
            : TimeUtils.GetCurrentSEATime().AddMinutes(180);
        var token = new JwtSecurityToken(issuer, null, claims, notBefore: TimeUtils.GetCurrentSEATime().AddHours(-7), expires.AddHours(-7), credentials);
        return jwtHandler.WriteToken(token);
    }
    public static string GenerateRefreshToken(User user, Tuple<string, Guid> guidClaim, DateTime? expireDay)
    {
        #region varKey
        string issuerKey = "VegaCityApp";
        string secretKey = "VegaCityAppsecretKey";
        #endregion
        JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
        SymmetricSecurityKey secrectKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
        string issuer = issuerKey;
        List<Claim> claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
        };
        if (guidClaim != null) claims.Add(new Claim(guidClaim.Item1, guidClaim.Item2.ToString()));
        DateTime expires = (DateTime)expireDay;
        var token = new JwtSecurityToken(issuer, null, claims, notBefore: TimeUtils.GetCurrentSEATime().AddHours(-7), expires.AddHours(-7), credentials);
        return jwtHandler.WriteToken(token);
    }
    //function decode jwt token to get expire date
    public static DateTime GetExpireDate(string token)
    {
        JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
        JwtSecurityToken jwtToken = jwtHandler.ReadJwtToken(token);
        return TimeUtils.ConvertToSEATime(jwtToken.ValidTo);
    }
}