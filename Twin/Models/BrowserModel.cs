using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using CommunityToolkit.Mvvm.ComponentModel;

using Twin.Gemini;

namespace Twin.Models
{
    internal partial class BrowserModel : ObservableObject
    {
        [ObservableProperty]
        Uri uri;

        [ObservableProperty]
        GeminiResponse currentPage;

        [ObservableProperty]
        bool isLoading = false;

        public History History = new();
    }
}
