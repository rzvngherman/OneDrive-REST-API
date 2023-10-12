namespace OneDriveBackend.Models;

public class GetDownloadLinkOfFileHttpResponse
{
    public string DownloadUrl { get; set; }
    public string FileName { get; set; }
    public string RequestedPath { get; set; }
}
