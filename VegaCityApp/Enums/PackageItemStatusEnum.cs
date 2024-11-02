namespace VegaCityApp.API.Enums
{
    public enum PackageItemStatusEnum
    {
        Active = 1,
        Inactive = 0,
        Expired = 2,
        Blocked = -1
    }
     public enum PackageItemParentStatusEnum
    {
        True = 1,
        False = 0,
    }
    public enum PackageItemStatus
    {
        Active,
        Inactive,
        Expired,
        Blocked
    }
}
