using CommunityToolkit.Mvvm.ComponentModel;

namespace Twin.Core.Models
{
    public partial class History : ObservableObject
    {
        List<Uri> historyList = new();

        public IReadOnlyList<Uri> Items => historyList.AsReadOnly();
        public int CurrentPage = -1;

        [ObservableProperty]
        bool canGoForward;

        [ObservableProperty]
        bool canGoBack;

        public void Add(Uri uri)
        {
            historyList.Add(uri);
            CurrentPage = historyList.Count - 1;
            CanGoBack = historyList.Count > 1;
            CanGoForward = false;
        }

        public Uri NextPage()
        {
            if (!CanGoForward) throw new InvalidOperationException();
            CurrentPage++;
            CanGoForward = historyList.Count - 1 > CurrentPage;
            CanGoBack = true;
            return historyList[CurrentPage];
        }

        public Uri PreviousPage()
        {
            if (!CanGoBack) throw new InvalidOperationException();
            CurrentPage--;
            CanGoBack = CurrentPage - 1 >= 0;
            CanGoForward = true;
            return historyList[CurrentPage];
        }

        public void ClearNextPages()
        {
            if (CurrentPage == historyList.Count - 1) return;

            historyList.RemoveRange(CurrentPage + 1, historyList.Count - (CurrentPage + 1));
        }
    }
}
