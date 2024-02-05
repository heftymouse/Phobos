using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Diagnostics;
using Phobos.Core.Models;
using Phobos.Core.Services;

namespace Phobos.Core.ViewModels
{
    public partial class BrowserViewModel(GeminiService GeminiService, IFileService FileService, IAppDataService AppDataService) : ObservableObject
    {
        [ObservableProperty]
        Uri? uri;

        [ObservableProperty]
        ViewState state = ViewState.StartPage;

        [ObservableProperty]
        string errorText = string.Empty;

        [ObservableProperty]
        GeminiResponse currentPage;

        public History History { get; private set; } = new();

        public IReadOnlyList<Bookmark> Bookmarks => AppDataService.Bookmarks;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ForwardCommand))]
        bool canGoForward;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(BackCommand))]
        bool canGoBack;

        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ReloadCommand))]
        [NotifyCanExecuteChangedFor(nameof(SavePageCommand))]
        bool canReload = false;

        [RelayCommand]
        public async Task Navigate()
        {
            await GoToPageCommand.ExecuteAsync(null);
            this.History.ClearNextPages();
            this.History.Add(Uri!);
            this.CanGoBack = this.History.CanGoBack;
            this.CanGoForward = false;
            this.CanReload = true;
        }

        [RelayCommand]
        public async Task NavigateWithInput(string input)
        {
            this.Uri = new UriBuilder(this.Uri) { Query = input }.Uri;
            await NavigateCommand.ExecuteAsync(null);
        }

        [RelayCommand(CanExecute = nameof(CanGoBack))]
        public async Task Back()
        {
            this.Uri = this.History.PreviousPage();
            await GoToPageCommand.ExecuteAsync(null);
            this.CanGoBack = this.History.CanGoBack;
            this.CanGoForward = this.History.CanGoForward;
        }

        [RelayCommand(CanExecute = nameof(CanGoForward))]
        public async Task Forward()
        {
            this.Uri = this.History.NextPage();
            await GoToPageCommand.ExecuteAsync(null);
            this.CanGoBack = this.History.CanGoBack;
            this.CanGoForward = this.History.CanGoForward;
        }

        [RelayCommand(CanExecute = nameof(CanReload))]
        public async Task Reload()
        {
            if (Uri is null) throw new InvalidOperationException();
            await GoToPageCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task GoToPage()
        {
            try
            {
                GeminiResponse response = await Task.Run(() => GeminiService.RequestPageAsync(this.Uri!));

                switch (response.Type)
                {
                    case GeminiResponseType.Input:
                        this.State = response.Code == 0 ? ViewState.Input : ViewState.InputSensitive;
                        CurrentPage = response;
                        break;

                    case GeminiResponseType.Redirect:
                        this.Uri = response.Meta.Contains("://")
                        ? new Uri(response.Meta)
                        : new UriBuilder(this.Uri.GetLeftPart(UriPartial.Authority)) { Path = response.Meta }.Uri;
                        await GoToPage();
                        break;

                    case GeminiResponseType.Ok:
                        this.State = ViewState.Page;
                        CurrentPage = response;
                        break;

                    case GeminiResponseType.ClientCertificateRequired:
                        this.State = ViewState.ClientCertificate;
                        break;

                    case GeminiResponseType.TemporaryFailure:
                    case GeminiResponseType.PermanentFailure:
                        this.State = ViewState.Error;
                        this.ErrorText = $"Code {(int)response.Type}{response.Code} - {response.Meta}";
                        break;

                }
            }
            catch (Exception ex)
            {
                this.State = ViewState.InternalError;
                this.ErrorText = ex.ToString();
                Debug.WriteLine(ex);
            }
        }

        [RelayCommand(CanExecute = nameof(CanReload))]
        public async Task SavePage()
        {
            var splitPath = this.Uri!.LocalPath.Split('/');
            var lastSegment = splitPath[^1];
            var name = this.Uri!.Host;
            var extension = ".gmi";
            if(lastSegment.Length > 0)
            {
                var index = lastSegment.LastIndexOf(".");
                if(index == -1)
                {
                    name = lastSegment;
                }
                else
                {
                    name = lastSegment[0..index];
                    extension = lastSegment[index..];
                }
            }
            await FileService.SaveFileAsync(name, extension, this.CurrentPage.Body);
        }

        [RelayCommand]
        public void UpdateCertificate()
        {
            AppDataService.SetCertificate(this.CurrentPage.Certificate);
        }

        public void AddBookmark(string name, Uri uri)
        {
            AppDataService.AddBookmark(new(name, uri ?? this.Uri!));
        }

        public void RemoveBookmark(Bookmark bookmark) 
        {
            AppDataService.RemoveBookmark(bookmark);
        }
    }
}
