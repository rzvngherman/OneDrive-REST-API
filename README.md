# OneDrive-REST-API

## Endpoint:
**POST**
'/OneDrive/01-GetDownloadLinkOfFile'

## Header Authorization: 
Token bearer

## Payload model ('GetDownloadLinkOfFileHttpReq'):
```json
{
  "fileName": "string",
  "path": "string",
  "showAllFields": false/true
}
```

## Response model: ('GetHttpResponse')
```json
{
  "mockData": false/true,
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
```

## Short description of endpoint:
- This endpoint makes a call to OneDrive REST API endpoint '/drive/root:/path/to/file'.
- Giving a filename and a path, endpoint will return download url for that file.
- 'mockData' from response, is true if environment variable is **'Development'** (ASPNETCORE_ENVIRONMENT)

## Docs:
- https://learn.microsoft.com/en-us/graph/api/resources/onedrive?view=graph-rest-1.0
- https://developer.microsoft.com/en-us/graph/graph-explorer


# Thank you!