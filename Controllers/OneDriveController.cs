using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;

namespace OneDriveDownloaded.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OneDriveController : ControllerBase
    {
        private const string ROOT_ADDRESS = "https://graph.microsoft.com/v1.0/me";
        private const string DRIVE_ROOT_PATH = "drive/root:";
        private const string LIST_OF_FIELD_TO_BE_READ = "?select=@microsoft.graph.downloadUrl,name";

        private readonly ILogger<OneDriveController> _logger;
        private readonly HttpService _httpService;

        public OneDriveController(ILogger<OneDriveController> logger)
        {
            _logger = logger;
            _httpService = new HttpService();
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

    public class HttpService
    {
        private readonly HttpClient _client;

        public HttpService()
        {
            HttpClientHandler handler = new HttpClientHandler
            {
                AutomaticDecompression = DecompressionMethods.All
            };

            _client = new HttpClient();
        }

        public async Task<string> GetAsync(string uri, Dictionary<string, string> headerValues = null)
        {
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