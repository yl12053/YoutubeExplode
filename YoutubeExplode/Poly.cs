using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Cysharp.Threading.Tasks.Linq;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode;

public static class Poly
{
    public static UniTask<T> AsUniTaskCpy<T>(this Task<T> task, bool useCurrentSynchronizationContext = true)
    {
        UniTaskCompletionSource<T> completionSource1 = new UniTaskCompletionSource<T>();
        task.ContinueWith((x, state) =>
        {
            UniTaskCompletionSource<T> completionSource2 = (UniTaskCompletionSource<T>) state;
            switch (x.Status)
            {
                case TaskStatus.RanToCompletion:
                    completionSource2.TrySetResult(x.Result);
                    break;
                case TaskStatus.Canceled:
                    completionSource2.TrySetCanceled();
                    break;
                case TaskStatus.Faulted:
                    completionSource2.TrySetException(x.Exception?.InnerException ?? x.Exception);
                    break;
                default:
                    throw new NotSupportedException();
            }
        }, completionSource1, TaskScheduler.Default);
        return completionSource1.Task;
    }
    
    extension<TSource>(IEnumerable<TSource> source)
    {
        public TSource MaxBy<TKey>(Func<TSource, TKey> selector)
            where TKey : IComparable<TKey>
        {
            using var enumerator = source.GetEnumerator();
            if (!enumerator.MoveNext())
                return default!;

            var maxElement = enumerator.Current;
            var maxKey = selector(maxElement);

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var key = selector(current);
                if (key.CompareTo(maxKey) > 0)
                {
                    maxKey = key;
                    maxElement = current;
                }
            }
            return maxElement;
        }

        public IEnumerable<(int i, TSource Item)> Index()
        {
            int index = 0;
            foreach (var item in source)
            {
                yield return (index++, item);
            }
        }
    }

    extension(HttpClient Http)
    {
        public UniTask<string> GetStringAsync(string requestUri, CancellationToken token)
        {
            return Http.GetStringAsync(requestUri).AsUniTaskCpy().AttachExternalCancellation(token);
        }

        public UniTask<Stream> GetStreamAsync(string requestUri, CancellationToken token)
        {
            return Http.GetStreamAsync(requestUri).AsUniTaskCpy().AttachExternalCancellation(token);
        }
    }

    extension(HttpContent content)
    {
        public UniTask<string> ReadAsStringAsync(CancellationToken token)
        {
            return content.ReadAsStringAsync().AsUniTaskCpy().AttachExternalCancellation(token);
        }
    }

    extension(JToken token)
    {
        public JToken? GetPropertyOrNull(string key)
        {
            return token[key];
        }

        public string? GetStringOrNull()
        {
            return token.Value<string>();
        }

        public int? GetInt32OrNull()
        {
            return token.Value<int>();
        }

        public long? GetInt64OrNull()
        {
            return token.Value<long>();
        }

        public bool? GetBooleanOrNull()
        {
            return token.Value<bool>();
        }

        public JArray? EnumerateArrayOrNull()
        {
            return token as JArray;
        }

        public IEnumerable<JToken> EnumerateArrayOrEmpty()
        {
            return token as JArray ?? Enumerable.Empty<JToken>();
        }

        public DateTimeOffset GetDateTimeOffset()
        {
            if (token.Type != JTokenType.String)
            {
                throw new InvalidOperationException(
                    $"The JSON value is not a string. Actual type: {token?.Type}"
                );
            }

            var str = token.ToString();

            if (
                DateTimeOffset.TryParse(
                    str,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var result
                )
            )
            {
                return result;
            }

            throw new FormatException(
                $"The JSON value '{str}' could not be parsed as DateTimeOffset."
            );
        }
    }

    extension<T>(IEnumerable<T> source)
        where T : class
    {
        public T? ElementAtOrNull(int index)
        {
            try
            {
                return source.ElementAt(0);
            }
            catch (ArgumentOutOfRangeException)
            {
                return null;
            }
        }

        public T? FirstOrNull()
        {
            try
            {
                return source.First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    extension<T>(IUniTaskAsyncEnumerable<T> source)
    {
        public IUniTaskAsyncEnumerable<TResult> SelectManyAsync<TResult>(
            Func<T, IEnumerable<TResult>> selector)
        {
            return UniTaskAsyncEnumerable.Create<TResult>(async (writer, cancellationToken) =>
            {
                await foreach (var item in source.WithCancellation(cancellationToken))
                {
                    var innerSequence = selector(item);
                    foreach (var innerItem in innerSequence)
                    {
                        await writer.YieldAsync(innerItem);
                    }
                }
            });
        }

        public IUniTaskAsyncEnumerable<Y> OfTypeAsync<Y>()
        {
            return UniTaskAsyncEnumerable.Create<Y>(async (writer, cancellationToken) =>
            {
                await foreach (var item in source.WithCancellation(cancellationToken))
                {
                    if (item is Y castedItem)
                    {
                        await writer.YieldAsync(castedItem);
                    }
                }
            });
        }

        public async UniTask<List<T>> ToListAsync(CancellationToken cancellationToken = default)
        {
            var list = new List<T>();
            await foreach (var item in source.WithCancellation(cancellationToken))
            {
                list.Add(item);
            }

            return list;
        }
        
        public IUniTaskAsyncEnumerable<T> TakeAsync(
            int count
        )
        {
            return UniTaskAsyncEnumerable.Create<T>(async (writer, cancellationToken) =>
            {
                if (count <= 0)
                    return;

                var currentCount = 0;

                await using var enumerator = source.GetAsyncEnumerator(cancellationToken);

                while (currentCount < count && await enumerator.MoveNextAsync())
                {
                    await writer.YieldAsync(enumerator.Current);
                    currentCount++;
                }
            });
        }

        public UniTask<List<T>>.Awaiter GetAwaiter() => source.ToListAsync().GetAwaiter();
    }
}
