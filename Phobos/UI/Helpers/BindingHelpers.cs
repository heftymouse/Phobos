using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.UI.Helpers
{
    internal static class BindingHelpers
    {
        public static bool Inverse(this bool value) => !value;

        public static bool IsNotEmpty(string value) => !string.IsNullOrEmpty(value);

        public static bool IsNull(object value, bool inverse = false) 
            => (value is null) ^ inverse;
    }
}
