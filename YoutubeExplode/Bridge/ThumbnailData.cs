using Lazy;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Bridge;

public class ThumbnailData(JToken content)
{
    [Lazy]
    public string? Url => content.GetPropertyOrNull("url")?.GetStringOrNull();

    [Lazy]
    public int? Width => content.GetPropertyOrNull("width")?.GetInt32OrNull();

    [Lazy]
    public int? Height => content.GetPropertyOrNull("height")?.GetInt32OrNull();
}
