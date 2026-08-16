using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using PowerKit.Extensions;
using YoutubeExplode.Bridge;
using YoutubeExplode.Utils;

namespace YoutubeExplode.Videos.ClosedCaptions;

public class ClosedCaptionController(HttpClient http) : VideoController(http)
{
    public async UniTask<ClosedCaptionTrackResponse> GetClosedCaptionTrackResponseAsync(
        string url,
        CancellationToken cancellationToken = default
    )
    {
        // Enforce known format
        var urlWithFormat = url.Pipe(s => UrlEx.SetQueryParameter(s, "format", "3"))
            .Pipe(s => UrlEx.SetQueryParameter(s, "fmt", "3"));

        return ClosedCaptionTrackResponse.Parse(
            await Http.GetStringAsync(urlWithFormat, cancellationToken)
        );
    }
}
