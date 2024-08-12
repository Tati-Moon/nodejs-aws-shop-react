using System.Reflection;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Text;

namespace BffWebService.Client
{
    internal static class ApiClientRequestMapping
    {
        private const string ContentTypeFormUrlEncoded = "application/json";

        public static HttpRequestMessage ToGet<T>(this ApiClientRequest<T> apiClientRequest)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(apiClientRequest);

            return BuildHttpRequestMessage(
                HttpMethod.Get,
                apiClientRequest.Path,
                apiClientRequest.Headers);
        }

        public static HttpRequestMessage ToDelete<T>(this ApiClientRequest<T> apiClientRequest)
           where T : class
        {
            ArgumentNullException.ThrowIfNull(apiClientRequest);

            return BuildHttpRequestMessage(
                HttpMethod.Delete,
                apiClientRequest.Path,
                apiClientRequest.Headers);
        }

        public static HttpRequestMessage ToPostAsForm<T>(this ApiClientRequest<T> apiClientRequest)
            where T : class
        {
            ArgumentNullException.ThrowIfNull(apiClientRequest);

            var queryString = ToQueryString(apiClientRequest.Query);

            return BuildHttpRequestMessage(
                HttpMethod.Post,
                apiClientRequest.Path,
                apiClientRequest.Headers,
                queryString,
                ContentTypeFormUrlEncoded);
        }

        public static HttpRequestMessage ToPutAsForm<T>(this ApiClientRequest<T> apiClientRequest)
         where T : class
        {
            ArgumentNullException.ThrowIfNull(apiClientRequest);

            var queryString = ToQueryString(apiClientRequest.Query);

            return BuildHttpRequestMessage(
                HttpMethod.Put,
                apiClientRequest.Path,
                apiClientRequest.Headers,
                queryString,
                ContentTypeFormUrlEncoded);
        }

        private static string ToQueryString<TEntity>(TEntity entity)
            where TEntity : class
        {
            return JsonSerializer.Serialize(entity);
        }

        private static IDictionary<string, string> ToDictionary<TEntity>(TEntity data)
            where TEntity : class
        {
            var dictionary = new Dictionary<string, string>();

            if (data == null)
            {
                return dictionary;
            }

            return data
                .GetType()
                .GetProperties()
                .Where(x => !x.GetIndexParameters().Any() && x.GetCustomAttribute<JsonIgnoreAttribute>() == null)
                .Select(x => new
                {
                    Name = x.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name ?? x.Name,
                    Value = x.GetValue(data, null)?.ToString()
                })
                .Where(x => !string.IsNullOrWhiteSpace(x.Value))
                .ToDictionary(x => x.Name, x => x.Value);
        }

        private static HttpRequestMessage BuildHttpRequestMessage(
            HttpMethod httpMethod, string path, IReadOnlyDictionary<string, string> keyValuePairs, string content = null, string contentType = null)
        {
            var httpRequestMessage = new HttpRequestMessage(httpMethod, new Uri(path, UriKind.Absolute));

            foreach (var keyValuePair in keyValuePairs ?? Enumerable.Empty<KeyValuePair<string, string>>())
            {
                httpRequestMessage.Headers.Add(keyValuePair.Key, keyValuePair.Value);
            }

            httpRequestMessage.Content = string.IsNullOrWhiteSpace(content) ? null : new StringContent(content, Encoding.UTF8, contentType);

            return httpRequestMessage;
        }
    }
}
