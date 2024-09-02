using VegaCityApp.API.Payload.Request;
using VegaCityApp.API.Payload.Response;

namespace VegaCityApp.API.Services.Interface
{
    public interface IEtagService
    {
        Task<CreateEtagTypeResponse> CreateEtagType(EtagTypeRequest req);
        Task<CreateEtagResponse> CreateEtag(EtagRequest req);
    }
}
