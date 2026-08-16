using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using PowerKit.Extensions;

namespace YoutubeExplode.Common;

/// <summary>
/// Generic collection of items returned by a single request.
/// </summary>
public class Batch<T>(IReadOnlyList<T> items)
    where T : IBatchItem
{
    /// <summary>
    /// Items included in the batch.
    /// </summary>
    public IReadOnlyList<T> Items { get; } = items;
}

public static class Batch
{
    public static Batch<T> Create<T>(IReadOnlyList<T> items)
        where T : IBatchItem => new(items);
}

public static class BatchExtensions
{
    extension<T>(IUniTaskAsyncEnumerable<Batch<T>> source)
        where T : IBatchItem
    {
        public IUniTaskAsyncEnumerable<T> FlattenAsync() => source.SelectManyAsync(b => b.Items);
    }
}
