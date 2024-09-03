using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IEtagService
    {
        Task<ResponseAPI> CreateEtagType(EtagTypeRequest req);
        Task<ResponseAPI> CreateEtag(EtagRequest req);
    }
}
