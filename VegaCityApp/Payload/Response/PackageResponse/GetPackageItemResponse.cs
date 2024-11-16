using VegaCityApp.Domain.Models;
using Newtonsoft.Json;


namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetPackageItemResponse : ResponseAPI
    {
        public Guid Id { get; set; }
        public Guid PackageId { get; set; }
        public string? Name { get; set; }
        public string? CCCDPassport { get; set; }  
        public string? Email { get; set; }
        public string? Status { get; set; }
        public string? Gender { get; set; }
        public bool? IsAdult { get; set; }
        public Guid? WalletId { get; set; }   
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

    public class GetListPackageItemResponse
    {
        public Guid Id { get; set; }
        public Guid? PackageId { get; set; }
        public string? VcardId { get; set; }
        public string CusName { get; set; } = null!;
        public string CusEmail { get; set; } = null!;
        public string CusCccdpassport { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;
        public DateTime CrDate { get; set; }
        public DateTime UpsDate { get; set; }
        public string? Status { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool? IsAdult { get; set; }
        public string? WalletTypeName { get; set; }
    }
}
