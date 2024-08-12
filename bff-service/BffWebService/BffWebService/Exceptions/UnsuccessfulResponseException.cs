using System.Net;
using System.Runtime.Serialization;

namespace BffWebService.Exceptions
{
    [Serializable]
    public sealed class UnsuccessfulResponseException :
      Exception
    {
        public string RequestMethod { get; }

        public Uri RequestUrl { get; }

        public string RequestBody { get; }

        public string ResponseBody { get; }

        public IReadOnlyDictionary<string, string> ResponseHeaders { get; set; }

        public HttpStatusCode? ResponseStatusCode { get; set; }

        public UnsuccessfulResponseException(
            string requestMethod,
            Uri requestUrl,
            string requestBody,
            HttpStatusCode responseStatusCode,
            string responseStatusReasonPhrase,
            string responseBody,
            IReadOnlyDictionary<string, string> responseHeaders)
            : this($"Unsuccessful response: '{(int)responseStatusCode} {responseStatusReasonPhrase}'")
        {
            RequestMethod = requestMethod;
            RequestUrl = requestUrl;
            RequestBody = requestBody;
            ResponseBody = responseBody;
            ResponseHeaders = responseHeaders;
            ResponseStatusCode = responseStatusCode;
        }

        public UnsuccessfulResponseException()
        {
        }

        public UnsuccessfulResponseException(string message)
            : base(message)
        {
        }

        public UnsuccessfulResponseException(
            string message,
            HttpStatusCode responseStatusCode)
          : base(message)
        {
            ResponseStatusCode = responseStatusCode;
        }

        public UnsuccessfulResponseException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public UnsuccessfulResponseException(string message, string requestBody, Exception innerException)
           : base(message, innerException)
        {
            RequestBody = requestBody;
        }

        private UnsuccessfulResponseException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context) =>
            base.GetObjectData(info, context);
    }
}
