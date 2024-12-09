using System.ComponentModel;

namespace VegaCityApp.API.Payload.Request.Admin
{
    public class AdminChartDurationRequest
    {
        [DefaultValue("2024-07-01")]
        public string? StartDate { get; set; } = "2024-07-01";
        [DefaultValue("2025-03-03")]
        public string EndDate { get; set; } = "2025-03-03";
        [DefaultValue(365)]
        public int? Days { get; set; } = 365; //12thang

        // public String? Month { get; set; }
        [DefaultValue("All")]
        public string? SaleType { get; set; }  //12thang

        [DefaultValue("Month")]
        public string? GroupBy { get; set; } = "Month";
    }
}
