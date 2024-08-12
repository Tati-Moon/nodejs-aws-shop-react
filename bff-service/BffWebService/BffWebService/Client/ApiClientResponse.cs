using System.Net;

namespace BffWebService.Client
{
    public class ApiClientResponse
    {
        public string Content { get; set; }
        public string Data { get; set; }
        public IReadOnlyDictionary<string, string> Headers { get; set; }
        public HttpMethod Method { get; set; }
        public HttpStatusCode StatusCode { get; set; }
        public Uri Uri { get; set; }
    }
}
