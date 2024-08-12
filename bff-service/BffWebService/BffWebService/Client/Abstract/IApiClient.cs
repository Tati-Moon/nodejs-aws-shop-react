namespace BffWebService.Client.Abstract
{
    public interface IApiClient
    {
        Task<ApiClientResponse> MethodAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest, HttpMethod httpMethod)
           where TEntity : class;

        Task<ApiClientResponse> GetAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest)
            where TEntity : class;

        Task<ApiClientResponse> PostAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest)
            where TEntity : class;
    }
}
