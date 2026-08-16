using System.Collections.Generic;

namespace YoutubeExplode.Bridge;

public interface IPlaylistData
{
    string? Title { get; }

    string? Author { get; }

    string? ChannelId { get; }

    string? Description { get; }

    int? Count { get; }

    IReadOnlyList<ThumbnailData> Thumbnails { get; }
}
