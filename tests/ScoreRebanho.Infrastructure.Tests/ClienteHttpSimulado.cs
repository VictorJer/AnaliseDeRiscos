using System.Net;
using System.Text;

namespace ScoreRebanho.Infrastructure.Tests;

internal sealed class ClienteHttpSimulado(Func<HttpRequestMessage, CancellationToken, Task<HttpResponseMessage>> responder) : HttpMessageHandler
{
    internal int Chamadas { get; private set; }
    internal Uri? UltimaUrl { get; private set; }
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Chamadas++;
        UltimaUrl = request.RequestUri;
        return responder(request, cancellationToken);
    }

    internal static HttpResponseMessage Json(string json, HttpStatusCode status = HttpStatusCode.OK) => new(status)
    {
        Content = new StringContent(json, Encoding.UTF8, "application/json")
    };
}
