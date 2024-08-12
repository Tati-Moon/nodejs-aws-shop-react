using BffWebService.Exceptions;
using System.Text.Json;

namespace BffWebService.Client
{
    public static class ApiClientResponseMapping
    {
        public static async Task<ApiClientResponse> ToApiClientResponseAsync(this HttpResponseMessage httpResponseMessage)
        {
            ArgumentNullException.ThrowIfNull(httpResponseMessage);

            var content = await httpResponseMessage?.Content?.ReadAsStringAsync();

            var headers = httpResponseMessage.Headers.ToDictionary(x => x.Key, x => x.Value.FirstOrDefault(), StringComparer.InvariantCultureIgnoreCase);
            var statusCode = httpResponseMessage.StatusCode;

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return new ApiClientResponse
                {
                    Data = content,
                    Headers = headers,
                    Method = httpResponseMessage.RequestMessage?.Method,
                    StatusCode = statusCode,
                    Uri = httpResponseMessage.RequestMessage?.RequestUri
                };
            }

            var requestMessage = httpResponseMessage.RequestMessage;

            throw new UnsuccessfulResponseException(
                requestMessage?.Method.Method,
                requestMessage?.RequestUri,
                requestMessage?.Content?.ToString(),
                statusCode,
                httpResponseMessage.ReasonPhrase,
                content,
                headers);
        }

        private static T SerializeData<T>(string content)
            where T : class
        {
            try
            {
                return !string.IsNullOrWhiteSpace(content)
                       ? JsonSerializer.Deserialize<T>(content, JsonSerializerOptionsFactory.Instance)
                       : null;
            }
            catch (JsonException ex)
            {
                throw new UnsuccessfulResponseException($"JsonException {ex.Message}", content, ex);
            }
        }
    }

    public class JsonSerializerOptionsFactory
    {
        public static readonly JsonSerializerOptions Instance = new()
        {
            PropertyNameCaseInsensitive = true
        };
    }
}
