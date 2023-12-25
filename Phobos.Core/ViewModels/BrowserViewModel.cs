using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mime;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Phobos.Core.Models;
using Phobos.Core.Services;

namespace Phobos.Core.ViewModels
{
    public partial class BrowserViewModel : ObservableObject
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

        private GeminiService GeminiService { get; init; }
        private IFileService FileService { get; init; }

        public BrowserViewModel(GeminiService geminiService, IFileService fileService)
        {
            GeminiService = geminiService;
            FileService = fileService;
        }

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
                GeminiResponse response = await Task.Run(() => this.GeminiService.RequestPageAsync(this.Uri!));

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
    }
}
