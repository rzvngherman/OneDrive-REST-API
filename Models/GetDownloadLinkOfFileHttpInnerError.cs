using Newtonsoft.Json;

namespace OneDriveBackend.Models;

public class GetDownloadLinkOfFileHttpInnerError
{
    public string date { get; set; }

    [JsonProperty("request-id")]
    public string RequestId { get; set; }

    [JsonProperty("client-request-id")]
    public string ClientRequestId { get; set; }
}
