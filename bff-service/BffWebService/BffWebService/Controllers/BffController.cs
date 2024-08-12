using BffWebService.Client;
using BffWebService.Client.Abstract;
using Microsoft.AspNetCore.Mvc;

namespace BffWebService.Controllers
{
    [ApiController]
    [Route("{recipientServiceName}")]
    public class BffController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IApiClient _apiClient;

        public BffController(
              IConfiguration configuration,
            IApiClient apiClient)
        {
            _configuration = configuration;
            _apiClient = apiClient;
        }

        private static IActionResult ConvertToIActionResult(ApiClientResponse apiClientResponse)
        {
            var contentResult = new ContentResult
            {
                Content = apiClientResponse.Data,
                StatusCode = (int)apiClientResponse.StatusCode,
                ContentType = "application/json"
            };

            //foreach (var header in apiClientResponse.Headers)
            //{
            //    Response.Headers[header.Key] = header.Value;
            //}

            //Response.Headers.Add("Access-Control-Allow-Origin", "*");
            //Response.Headers.Add("Access-Control-Allow-Headers", "*");
            //Response.Headers.Add("Access-Control-Allow-Methods", "*");

            return contentResult;
        }

        [HttpGet]
        [HttpPost]
        [HttpPut]
        [HttpDelete]
        public async Task<IActionResult> ForwardRequestAsync(string recipientServiceName)
        {
            var parts = recipientServiceName.Split('?', 2);
            var serviceName = parts[0];
            var additionalPath = parts.Length > 1 ? parts[1] : string.Empty;

            var recipientUrl = _configuration[$"RecipientUrls:{serviceName}"];
            if (string.IsNullOrEmpty(recipientUrl))
            {
                return StatusCode(502, "Cannot process request");
            }

            var requestMethod = Request.Method;
            var httpMethod = new HttpMethod(requestMethod);

            try
            {

                var apiClientRequest = new ApiClientRequest<string>
                {
                    Path = string.IsNullOrWhiteSpace(additionalPath)? $"{recipientUrl}" : $"{recipientUrl}/{additionalPath}",
                };

                var apiClientResponse = await _apiClient.MethodAsync<string>(apiClientRequest, httpMethod);

                return ConvertToIActionResult(apiClientResponse);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode(502, $"Request to recipient service failed: {ex.Message}");
            }
        }
    }
}