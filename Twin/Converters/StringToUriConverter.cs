using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml.Data;

namespace Twin.Converters
{
    internal class UriToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null) return null;
            Uri uri = value as Uri;
            return $"gemini://{uri.Host}{uri.PathAndQuery}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            string uriString = value as string;
            if (uriString is (null or "")) return null;
            UriBuilder uriBuilder = new UriBuilder(uriString);
            uriBuilder.Scheme = "gemini";
            uriBuilder.Port = 1965;
            return uriBuilder.Uri;
        }
    }
}
