using OneDriveBackend.Controllers;

namespace OneDriveBackend.Models;

public class GetDownloadLinkOfFileHttpError
{
    public string Code { get; set; }
    public string Message { get; set; }
    public GetDownloadLinkOfFileHttpInnerError InnerError { get; set; }
}
