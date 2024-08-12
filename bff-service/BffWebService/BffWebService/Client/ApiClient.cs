using BffWebService.Client.Abstract;
using BffWebService.Exceptions;

namespace BffWebService.Client
{
    public class ApiClient(
        HttpClient httpClient) : IApiClient
    {
        private readonly HttpClient _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));

        public async Task<ApiClientResponse> GetAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest)
            where TEntity : class
        {
            using var httpRequestMessage = apiClientRequest.ToGet();

            return await HandleHttpRequestAsync(httpRequestMessage);
        }

        public async Task<ApiClientResponse> DeleteAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest)
            where TEntity : class
        {
            using var httpRequestMessage = apiClientRequest.ToDelete();

            return await HandleHttpRequestAsync(httpRequestMessage);
        }

        public async Task<ApiClientResponse> MethodAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest, HttpMethod httpMethod) where TEntity : class
        {
            if (httpMethod == HttpMethod.Get)
            {
                return await GetAsync<TEntity>(apiClientRequest);
            }
            else if (httpMethod == HttpMethod.Post)
            {
                return await PostAsync<TEntity>(apiClientRequest);
            }
            else if (httpMethod == HttpMethod.Put)
            {
                return await PutAsync<TEntity>(apiClientRequest);
            }
            else if (httpMethod == HttpMethod.Delete)
            {
                return await DeleteAsync<TEntity>(apiClientRequest);
            }

            return null;
        }

        public async Task<ApiClientResponse> PostAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest) where TEntity : class
        {
            using var httpRequestMessage = apiClientRequest.ToPostAsForm();

            return await HandleHttpRequestAsync(httpRequestMessage);
        }

        public async Task<ApiClientResponse> PutAsync<TEntity>(ApiClientRequest<TEntity> apiClientRequest) where TEntity : class
        {
            using var httpRequestMessage = apiClientRequest.ToPutAsForm();

            return await HandleHttpRequestAsync(httpRequestMessage);
        }

        private async Task<ApiClientResponse> HandleHttpRequestAsync(HttpRequestMessage httpRequestMessage)
        {
            ArgumentNullException.ThrowIfNull(httpRequestMessage);

            try
            {
                using var responseMessage = await _httpClient.SendAsync(httpRequestMessage);

                return await responseMessage.ToApiClientResponseAsync();
            }
            catch (Exception exception) when (exception is not UnsuccessfulResponseException)
            {
                throw new UnsuccessfulResponseException("Low-level HTTP request failure", exception);
            }
            catch (Exception exception) when (exception is UnsuccessfulResponseException)
            {
                throw;
            }
        }
    }
}
