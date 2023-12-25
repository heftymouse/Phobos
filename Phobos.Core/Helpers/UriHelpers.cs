using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Helpers
{
    internal class UriHelpers
    {
        public static string UriToString(Uri uri)
        {
            if (uri is null) return null;
            return $"gemini://{uri.Host}{(uri.Port == 1965 ? $":${uri.Port}" : string.Empty)}{uri.PathAndQuery}";
        }

        public static Uri StringToUri(string uri)
        {
            if (string.IsNullOrEmpty(uri)) return null;
            UriBuilder uriBuilder = new UriBuilder(uri);
            uriBuilder.Scheme = "gemini";
            uriBuilder.Port = uriBuilder.Port is not (1965 or 80 or -1) ? uriBuilder.Port : 1965;
            return uriBuilder.Uri;
        }
    }
}
