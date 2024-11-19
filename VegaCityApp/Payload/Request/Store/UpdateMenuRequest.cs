using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request.Store
{
    public class UpdateMenuRequest
    {
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public int DateFilter { get; set; }
    }
}
