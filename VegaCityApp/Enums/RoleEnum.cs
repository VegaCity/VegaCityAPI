using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Enums
{
    public enum RoleEnum
    {
        Admin,
        CashierWeb,
        Store,
        CashierApp,
        AdminSystem,
    }
    public static class RoleHelper
    {
        public static readonly string[] allowedRoles =
        {
            RoleEnum.CashierWeb.GetDescriptionFromEnum(),
            RoleEnum.Store.GetDescriptionFromEnum(),
            RoleEnum.Admin.GetDescriptionFromEnum(),
            RoleEnum.CashierApp.GetDescriptionFromEnum(),
            RoleEnum.AdminSystem.GetDescriptionFromEnum(),
        };
    }
}
