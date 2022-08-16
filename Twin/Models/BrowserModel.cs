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
        public Uri uri;

        [ObservableProperty]
        public GeminiResponse currentPage;

        [ObservableProperty]
        public bool isLoading;

        public History History = new();
    }

    internal class History
    {
        List<Uri> historyList = new();
        int currentPage;
        public bool CanGoForward;
        public bool CanGoBack;

        public void Add(Uri uri)
        {
            historyList.Add(uri);
            currentPage = historyList.Count - 1;
            CanGoBack = historyList.Count > 1;
            CanGoForward = false;
        }

        public Uri NextPage()
        {
            if (!CanGoForward) return null;
            currentPage++;
            CanGoForward = historyList.Count - 1 > currentPage;
            CanGoBack = true;
            return historyList[currentPage];
        }

        public Uri PreviousPage()
        {
            if (!CanGoBack) return null;
            currentPage--;
            CanGoBack = currentPage - 1 >= 0;
            CanGoForward = true;
            return historyList[currentPage];
        }
    }
}
