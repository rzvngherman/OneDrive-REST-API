using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using OneDriveBackend.Models;
using OneDriveBackend.Services;
using System.Net;
using System.Text;

namespace OneDriveBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class OneDriveController : ControllerBase
{
    private const string ROOT_ADDRESS = "https://graph.microsoft.com/v1.0/me";
    private const string DRIVE_ROOT_PATH = "drive/root:";
    private const string LIST_OF_FIELD_TO_BE_READ = "?select=@microsoft.graph.downloadUrl,name";

    private readonly ILogger<OneDriveController> _logger;
    private readonly IHttpService _httpService;
    private bool _mockData;

    public OneDriveController(ILogger<OneDriveController> logger, IHttpService httpService, IWebHostEnvironment environment)
    {
        _logger = logger;
        _httpService = httpService;
        _mockData = environment.IsDevelopment();
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
        var url = Path.Combine(ROOT_ADDRESS, DRIVE_ROOT_PATH, req.Path, req.FileName);
        if (!req.ShowAllFields)
        {
            url = Path.Combine(url, LIST_OF_FIELD_TO_BE_READ);
        }
        var response5 = await _httpService.GetAsync(url, headerValues);

        var response = new GetHttpResponse(_mockData);

        bool isSuccess = CheckIfIsSuccess(response5);
        if (isSuccess)
        {
            response.Content = new GetDownloadLinkOfFileHttpResponse
            {
                DownloadUrl = GetDownloadUrl(response5),
                FileName = GetValueForField(response5, "name"),
                RequestedPath = req.Path,
            };
            return response;
        }

        var err = JsonConvert.DeserializeObject<GetHttpResponse>(response5);
        err.MockData = _mockData;
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