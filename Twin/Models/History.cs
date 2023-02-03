using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Models
{
    public partial class History : ObservableObject
    {
        List<Uri> historyList = new();

        public IReadOnlyList<Uri> Items;
        public int CurrentPage;

        [ObservableProperty]
        bool canGoForward;

        [ObservableProperty]
        bool canGoBack;

        public History()
        {
            Items = historyList.AsReadOnly();
        }

        public void Add(Uri uri)
        {
            historyList.Add(uri);
            CurrentPage = historyList.Count - 1;
            CanGoBack = historyList.Count > 1;
            CanGoForward = false;
        }

        public Uri NextPage()
        {
            if (!CanGoForward) return null;
            CurrentPage++;
            CanGoForward = historyList.Count - 1 > CurrentPage;
            CanGoBack = true;
            return historyList[CurrentPage];
        }

        public Uri PreviousPage()
        {
            if (!CanGoBack) return null;
            CurrentPage--;
            CanGoBack = CurrentPage - 1 >= 0;
            CanGoForward = true;
            return historyList[CurrentPage];
        }
    }
}
