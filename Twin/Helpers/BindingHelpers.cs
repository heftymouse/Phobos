using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Helpers
{
    internal static class BindingHelpers
    {
        public static bool Inverse(this bool value)
        {
            return !value;
        }

        public static Visibility BoolToVisibility(this bool value, bool inverse = false)
        {
            if (inverse) value = !value;
            return value ? Visibility.Visible : Visibility.Collapsed;
        }

        public static bool IsNotEmpty(string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public static bool IsNull(object value, bool inverse = false)
        {
            if (inverse) return value is not null;
            return value is null;
        }
    }
}
