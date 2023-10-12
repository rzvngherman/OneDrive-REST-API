using OneDriveBackend.Controllers;
using System.Net;
using System.Text;

namespace OneDriveBackend.Services;

public interface IHttpService
{
    Task<string> GetAsync(string uri, Dictionary<string, string> headerValues = null);
    Task<string> PostAsync(string uri, string data, string contentType);
}

public class HttpService : IHttpService
{
    private readonly HttpClient _client;
    private bool _mockData;

    public HttpService(IWebHostEnvironment environment)
    {
        HttpClientHandler handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.All
        };

        _client = new HttpClient();
        _mockData = environment.IsDevelopment();
    }

    public async Task<string> GetAsync(string uri, Dictionary<string, string> headerValues = null)
    {
        if (_mockData)
        {
            return await GetMockAsync(uri);
        }

        if (headerValues is not null)
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

    private Task<string> GetMockAsync(string uri)
    {
        if (uri.EndsWith("?select=@microsoft.graph.downloadUrl,name"))
        {
            var success = "{\"@odata.context\":\"https://graph.microsoft.com/v1.0/$metadata#users('some_guid')/drive/root/$entity\",\"@microsoft.graph.downloadUrl\":\"https://download_url.com\",\"name\":\"filename.ext\"}";
            return Task.Run(() =>
            {
                return success;
            });
        }

        //var error = "{\"error\":{\"code\":\"InvalidAuthenticationToken\",\"message\":\"CompactToken parsing failed with error code: 80049217\",\"innerError\":{\"date\":\"2023-10-11T14:16:36\",\"request-id\":\"ad1110b0-cc67-47f1-99ae-c1ddcad4ea3e\",\"client-request-id\":\"ad1110b0-cc67-47f1-99ae-c1ddcad4ea3e\"}}}";
        throw new NotImplementedException();
    }
}
