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
using Twin.Core.Gemini;
using Twin.Core.Models;
using Twin.Core.Services;

namespace Twin.Core.ViewModels
{
    public partial class BrowserViewModel : ObservableObject
    {
        [ObservableProperty]
        Uri? uri;

        [ObservableProperty]
        GeminiResponse currentPage;

        [ObservableProperty]
        bool isPanelOpen = false;

        public History History { get; private set; } = new();
        private GeminiService GeminiService { get; init; }
        private IDialogService DialogService;

        [ActivatorUtilitiesConstructor]
        public BrowserViewModel(GeminiService geminiService, IDialogService dialogService)
        {
            GeminiService = geminiService;
            DialogService = dialogService;
        }

        [RelayCommand]
        public async Task Navigate()
        {
            await GoToPageCommand.ExecuteAsync(null);
            this.History.ClearNextPages();
            this.History.Add(Uri!);

        }

        [RelayCommand]
        public async Task Back()
        {
            this.Uri = this.History.PreviousPage();
            await GoToPageCommand.ExecuteAsync(null);
        }

        [RelayCommand]
        public async Task Forward()
        {
            this.Uri = this.History.NextPage();
            await GoToPageCommand.ExecuteAsync(null);
        }

        [RelayCommand]
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
                        var input = await DialogService.ShowInputDialogForResult(response.Meta, response.Code != 0);
                        this.Uri = new Uri(this.Uri!, $"?{input}");
                        await GoToPage();
                        break;

                    case GeminiResponseType.Ok:
                        var contentType = new ContentType(response.Meta);
                        if (contentType.MediaType == "text/gemini")
                        {
                            CurrentPage = response;
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }
    }
}
