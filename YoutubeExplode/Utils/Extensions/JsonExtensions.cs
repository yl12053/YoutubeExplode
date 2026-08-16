using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace YoutubeExplode.Utils.Extensions;

public static class JsonExtensions
{
    extension(JToken token)
    {
        public IEnumerable<JToken> EnumerateDescendantProperties(string propertyName)
        {
            if (token is JObject obj)
            {
                var property = obj[propertyName];
                if (property is not null)
                {
                    yield return property;
                }

                foreach (var child in obj.Values())
                {
                    foreach (
                        var deepDescendant in child.EnumerateDescendantProperties(propertyName)
                    )
                    {
                        yield return deepDescendant;
                    }
                }
            }
            else if (token is JArray arr)
            {
                foreach (var item in arr)
                {
                    foreach (var deepDescendant in item.EnumerateDescendantProperties(propertyName))
                    {
                        yield return deepDescendant;
                    }
                }
            }
        }
    }
}
