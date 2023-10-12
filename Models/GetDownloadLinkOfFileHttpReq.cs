namespace OneDriveBackend.Models;

public class GetDownloadLinkOfFileHttpReq
{
    public string FileName { get; set; }
    public string Path { get; set; }
    public bool ShowAllFields { get; set; }
}
