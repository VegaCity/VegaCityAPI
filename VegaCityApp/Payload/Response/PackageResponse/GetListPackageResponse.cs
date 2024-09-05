using VegaCityApp.Domain.Models;

namespace VegaCityApp.API.Payload.Response.PackageResponse
{
    public class GetListPackageResponse : ResponseAPI
    {
        public List<Package> Packages { get; set; }


    }
}
