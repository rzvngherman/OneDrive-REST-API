using OneDriveBackend.Controllers;

namespace OneDriveBackend.Models;

public class GetHttpResponse
{
    public bool MockData { get; set; }
    public GetDownloadLinkOfFileHttpResponse Content { get; set; }
    public GetDownloadLinkOfFileHttpError Error { get; set; }

    public GetHttpResponse(bool mockData)
    {
        MockData = mockData;
    }
}
