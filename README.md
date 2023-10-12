# OneDrive-REST-API

Endpoint:
POST
'/OneDrive/01-GetDownloadLinkOfFile'

Heard Authorization: token bearer

Payload model (model name 'GetDownloadLinkOfFileHttpReq'):
{
  "fileName": "string",
  "path": "string",
  "showAllFields": true
}

Response model: (model name 'GetHttpResponse')
{
  "mockData": true,
  "content": {
    "downloadUrl": "string",
    "fileName": "string",
    "requestedPath": "string"
  },
  "error": {
    "code": "string",
    "message": "string",
    "innerError": {
      "date": "string",
      "requestId": "string",
      "clientRequestId": "string"
    }
  }
}

Short description of endpoint:
- This endpoint makes a call to OneDrive REST API endpoint '/drive/root:/path/to/file'.
- Giving a filename and a path, endpoint will return download url for that file.

END.