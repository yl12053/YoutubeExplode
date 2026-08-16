using System.Xml.Linq;
using PowerKit.Extensions;

namespace YoutubeExplode.Utils;

public static class Xml
{
    public static XElement Parse(string source) =>
        XElement.Parse(source, LoadOptions.PreserveWhitespace).StripNamespaces();
}
