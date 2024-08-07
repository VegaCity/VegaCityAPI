using Microsoft.AspNetCore.Authorization;
using VegaCityApp.API.Enums;
using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Validators;

public class CustomAuthorizeAttribute : AuthorizeAttribute
{
	public CustomAuthorizeAttribute(params RoleEnum[] roleEnums)
	{
		var allowedRolesAsString = roleEnums.Select(x => x.GetDescriptionFromEnum());
		Roles = string.Join(",", allowedRolesAsString);
	}
}