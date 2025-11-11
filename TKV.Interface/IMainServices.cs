using TKV.Model.JsonModels;

namespace TKV.Interface;

public interface IMainServices
{
    public Task<JsonResponse> CreateRequest(RequestModel requestModel);
    public Task<JsonResponse> GetRequests();
}