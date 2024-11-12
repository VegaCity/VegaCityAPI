
using VegaCityApp.API.Utils;

namespace VegaCityApp.API.Enums
{
    public enum StoreTypeEnum
    {
        Food = 0,
        Clothing = 1,
        Service = 2,
        Other = 3
    }
    public static class StoreTypeHelper
    {
        public static readonly int[] allowedStoreTypes =
        {
            (int)StoreTypeEnum.Food,
            (int)StoreTypeEnum.Clothing,
            (int) StoreTypeEnum.Service,
            (int) StoreTypeEnum.Other
        };
    }
}
