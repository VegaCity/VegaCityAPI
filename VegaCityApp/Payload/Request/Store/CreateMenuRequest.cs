using VegaCityApp.API.Enums;

namespace VegaCityApp.API.Payload.Request.Store
{
    public class CreateMenuRequest
    {
        public string? ImageUrl { get; set; }
        public string Name { get; set; } = null!;
        public DateFilterEnum DateFilter { get; set; }
    }
}
