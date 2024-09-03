using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
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
        IConfiguration configuration = new ConfigurationBuilder()
            .AddEnvironmentVariables(EnvironmentVariableConstant.Prefix).Build();
        JwtSecurityTokenHandler jwtHandler = new JwtSecurityTokenHandler();
        SymmetricSecurityKey secrectKey =
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetValue<string>(JwtConstant.SecretKey)));
        var credentials = new SigningCredentials(secrectKey, SecurityAlgorithms.HmacSha256Signature);
        string issuer = configuration.GetValue<string>(JwtConstant.Issuer);
        List<Claim> claims = new List<Claim>()
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.Name),
        };
        if (guidClaim != null) claims.Add(new Claim(guidClaim.Item1, guidClaim.Item2.ToString()));
        var expires = user.Role.Name.Equals(RoleEnum.Admin.GetDescriptionFromEnum())
            ? DateTime.Now.AddDays(1)
            : DateTime.Now.AddDays(30);
        var token = new JwtSecurityToken(issuer, null, claims, notBefore: DateTime.Now, expires);
        return jwtHandler.WriteToken(token);
    }
}