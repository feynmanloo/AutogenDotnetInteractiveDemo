namespace AutogenDotnetInteractiveDemo;

/// <summary>
/// 重写SemanticKernel的请求处理
/// </summary>
public sealed class OpenAiHttpClientHandler : HttpClientHandler
{
    public OpenAiHttpClientHandler()
    {
    }
    
    public OpenAiHttpClientHandler(string url)
    {
        _url = url;
    }
    
    private readonly string _url = "http://localhost:11434";

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // string url = _url ?? "http://localhost:11434"; //"https://aihubmix.com";
        UriBuilder uriBuilder = new UriBuilder(_url + request.RequestUri?.LocalPath);
        // if (request.RequestUri?.LocalPath == "/v1/chat/completions")
        // {
        //     uriBuilder = new UriBuilder(_uri.TrimEnd('/') + "/v1/chat/completions");
        //     request.RequestUri = uriBuilder.Uri;
        // }
        // else if (
        //          request.RequestUri?.LocalPath == "/v1/embeddings")
        // {
        //     uriBuilder = new UriBuilder(_uri.TrimEnd('/') + "/v1/embeddings");
        //     request.RequestUri = uriBuilder.Uri;
        // }
        request.RequestUri = uriBuilder.Uri;
        // return await base.SendAsync(request, cancellationToken);
        var response = await base.SendAsync(request, cancellationToken);
        // var s = await response.Content.ReadAsStringAsync(cancellationToken);
        return response;
    }
}