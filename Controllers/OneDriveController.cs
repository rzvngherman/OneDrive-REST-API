using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.ComponentModel.Design;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Hosting;

namespace OneDriveBackend.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OneDriveController : ControllerBase
    {
        private const string ROOT_ADDRESS = "https://graph.microsoft.com/v1.0/me";
        private const string DRIVE_ROOT_PATH = "drive/root:";
        private const string LIST_OF_FIELD_TO_BE_READ = "?select=@microsoft.graph.downloadUrl,name";

        private readonly ILogger<OneDriveController> _logger;
        private readonly IHttpService _httpService;

        public OneDriveController(ILogger<OneDriveController> logger, IHttpService httpService)
        {
            _logger = logger;
            _httpService = httpService;
        }

        /// <summary>
        /// Using 'OneDrive REST API'.
        /// Calling '/drive/root:/path/to/file'.
        /// Request example:
        /// {
        ///     "FileName" : "a.rar",
        ///     "Path" : "from_d/r",
        ///     "ShowAllFields" : true/false
        /// }
        /// </summary>
        /// <param name="authorization"></param>
        /// <returns>GetDownloadLinkOfFileHttpResponse</returns>
        [Route("01-GetDownloadLinkOfFile")]
        [HttpPost]
        public async Task<GetHttpResponse> Get01DownloadLinkOfFile([FromBody] GetDownloadLinkOfFileHttpReq req, [FromHeader] string authorization)
        {
            //add header
            var headerValues = new Dictionary<string, string>();
            headerValues.Add("Authorization", authorization);

            // '/drive/root:/path/to/file' -> OneDrive REST API: Access a driveItem by path under the root.
            // example of file 'from_d/r/a.rar'
            var url5 = Path.Combine(ROOT_ADDRESS, DRIVE_ROOT_PATH, req.Path, req.FileName);
            if (!req.ShowAllFields)
            {
                url5 = Path.Combine(url5, LIST_OF_FIELD_TO_BE_READ);
            }
            var response5 = await _httpService.GetAsync(url5, headerValues);

            var response = new GetHttpResponse();

            bool isSuccess = CheckIfIsSuccess(response5);
            if (isSuccess)
            {
                response.Content = new GetDownloadLinkOfFileHttpResponse
                {
                    DownloadUrl = GetDownloadUrl(response5),
                    FileName = GetValueForField(response5, "name"),
                    PathToFolder = req.Path,
                };
                return response;
            }

            var err = JsonConvert.DeserializeObject<GetHttpResponse>(response5);
            return err;
        }

        private bool CheckIfIsSuccess(string httpResponse)
        {
            JObject fileInfo = JObject.Parse(httpResponse);
            return fileInfo["@odata.context"] is not null;
        }

        private string GetDownloadUrl(string httpResponse)
        {
            return GetValueForField(httpResponse, "@microsoft.graph.downloadUrl");
        }

        private string GetValueForField(string httpResponse, string fieldName)
        {
            JObject fileInfo = JObject.Parse(httpResponse);
            return fileInfo[fieldName]?.ToString();
        }
    }

    public interface IHttpService
    {
        Task<string> GetAsync(string uri, Dictionary<string, string> headerValues = null);
        Task<string> PostAsync(string uri, string data, string contentType);
    }
    public class HttpService : IHttpService
    {
        private readonly HttpClient _client;
        private bool _isLocal;

        public HttpService(IWebHostEnvironment environment)
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            _client = new HttpClient();
            _isLocal = environment.IsDevelopment();
        }

        public async Task<string> GetAsync(string uri, Dictionary<string, string> headerValues = null)
        {
            if(_isLocal)
            {
                return await GetAsyncLocal(uri);
            }

            if(headerValues is not null)
            {
                foreach (var headerVal in headerValues)
                {
                    _client.DefaultRequestHeaders.Remove(headerVal.Key);
                    _client.DefaultRequestHeaders.Add(headerVal.Key, headerVal.Value);
                }
            }
            using HttpResponseMessage response = await _client.GetAsync(uri);
            return await response.Content.ReadAsStringAsync();
        }

        public async Task<string> PostAsync(string uri, string data, string contentType)
        {
            using HttpContent content = new StringContent(data, Encoding.UTF8, contentType);

            HttpRequestMessage requestMessage = new HttpRequestMessage()
            {
                Content = content,
                Method = HttpMethod.Post,
                RequestUri = new Uri(uri)
            };

            using HttpResponseMessage response = await _client.SendAsync(requestMessage);
            return await response.Content.ReadAsStringAsync();
        }

        private Task<string> GetAsyncLocal(string uri)
        {
            //test
            var error = "{\"error\":{\"code\":\"InvalidAuthenticationToken\",\"message\":\"CompactToken parsing failed with error code: 80049217\",\"innerError\":{\"date\":\"2023-10-11T14:16:36\",\"request-id\":\"ad1110b0-cc67-47f1-99ae-c1ddcad4ea3e\",\"client-request-id\":\"ad1110b0-cc67-47f1-99ae-c1ddcad4ea3e\"}}}";
            var success = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users('7c4cf661-71cd-4efe-a507-989232c71451')/drive/root/$entity\",\"@microsoft.graph.downloadUrl\":\"https://levi9-my.sharepoint.com/personal/r_gherman_levi9_com/_layouts/15/download.aspx?UniqueId=c361056b-b70b-4fe6-b14a-458d287c6550&Translate=false&tempauth=eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiIwMDAwMDAwMy0wMDAwLTBmZjEtY2UwMC0wMDAwMDAwMDAwMDAvbGV2aTktbXkuc2hhcmVwb2ludC5jb21ANDA3NTg0ODEtNzM2NS00NDJjLWFlOTQtNTYzZWQxNjA2MjE4IiwiaXNzIjoiMDAwMDAwMDMtMDAwMC0wZmYxLWNlMDAtMDAwMDAwMDAwMDAwIiwibmJmIjoiMTY5NzAzMzgzOSIsImV4cCI6IjE2OTcwMzc0MzkiLCJlbmRwb2ludHVybCI6IjNQZCtoQThSQmVtMm5MYVV4VkoyTjlFN1JCQXhldTQxaDJ3NTQxSnlGM1U9IiwiZW5kcG9pbnR1cmxMZW5ndGgiOiIxNDgiLCJpc2xvb3BiYWNrIjoiVHJ1ZSIsImNpZCI6IjNmeWt4SUhSZFVDUWxXdWVnSHVibHc9PSIsInZlciI6Imhhc2hlZHByb29mdG9rZW4iLCJzaXRlaWQiOiJPV0ppTVdaa01EZ3RNREEwTVMwME4yRTVMVGsyTUdJdE1UbGhNVEppTlRneU16Tm0iLCJhcHBfZGlzcGxheW5hbWUiOiJHcmFwaCBFeHBsb3JlciIsImdpdmVuX25hbWUiOiJSYXp2YW4iLCJmYW1pbHlfbmFtZSI6IkdoZXJtYW4iLCJzaWduaW5fc3RhdGUiOiJbXCJrbXNpXCJdIiwiYXBwaWQiOiJkZThiYzhiNS1kOWY5LTQ4YjEtYThhZC1iNzQ4ZGE3MjUwNjQiLCJ0aWQiOiI0MDc1ODQ4MS03MzY1LTQ0MmMtYWU5NC01NjNlZDE2MDYyMTgiLCJ1cG4iOiJyLmdoZXJtYW5AbGV2aTkuY29tIiwicHVpZCI6IjEwMDMwMDAwQTdBMEMzNDAiLCJjYWNoZWtleSI6IjBoLmZ8bWVtYmVyc2hpcHwxMDAzMDAwMGE3YTBjMzQwQGxpdmUuY29tIiwic2NwIjoibXlmaWxlcy5yZWFkIGFsbHByb2ZpbGVzLnJlYWQiLCJ0dCI6IjIiLCJpcGFkZHIiOiIyMC4xOTAuMTYwLjI2In0.HKb4_2kqVqm6YrgK-MtDFy8q61wGSdtjXEi7TMZ1TOU&ApiVersion=2.0\",\"name\":\"a.rar\"}";
            //bool isSuccess3 = CheckIfIsSuccess(error);
            //bool isSuccess2 = CheckIfIsSuccess(success);
            //end-test

            throw new NotImplementedException();
        }
    }
    
    public class GetHttpResponse
    {
        public GetDownloadLinkOfFileHttpResponse Content { get; set; }
        public GetDownloadLinkOfFileHttpError Error { get; set; }
    }

    public class GetDownloadLinkOfFileHttpResponse
    {
        public string DownloadUrl { get; set; }
        public string FileName { get; set; }
        public string PathToFolder { get; set; }
    }

    public class GetDownloadLinkOfFileHttpReq
    {
        public string FileName { get; set;}
        public string Path { get; set; }
        public bool ShowAllFields { get; set; }
    }

    public class GetDownloadLinkOfFileHttpError
    {
        public string Code { get; set; }
        public string Message { get; set; }
        public GetDownloadLinkOfFileHttpInnerError InnerError { get; set; }
    }
    public class GetDownloadLinkOfFileHttpInnerError
    {
        public string date { get; set; }

        [JsonProperty("request-id")]
        public string RequestId { get; set; }

        [JsonProperty("client-request-id")]
        public string ClientRequestId { get; set; }
    }
}