using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Phobos.UI.Controls;
using WinRT.Interop;

namespace Phobos.Services
{
    internal class XamlRootService
    {
        public XamlRoot Root;
        public nint Hwnd => (Root.Content as RootFrame).Hwnd;
    }
}
