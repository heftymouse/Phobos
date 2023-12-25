using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Mime;
using Phobos.Core.ViewModels;
using Phobos.Gemini;
using Phobos.Helpers;
using System.Text;
using Phobos.Core.Services;
using Windows.System;
using Microsoft.UI.Xaml.Media;
using Phobos.Core.Models;

namespace Phobos.UI.Views
{
    public sealed partial class BrowserView : Page, ITabContext
    {
        BrowserViewModel vm;
        List<XamlUICommand> commands = new();

        public BrowserView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            vm = IocHelpers.GetServicesForTab(this).GetRequiredService<BrowserViewModel>();
            vm.PropertyChanged += OnViewmodelPropertyChanged;
            var forwardCommand = new XamlUICommand
            {
                Label = "Forward",
                IconSource = new SymbolIconSource { Symbol = Symbol.Forward },
                Command = vm.ForwardCommand
            };
            forwardCommand.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Left, Modifiers = VirtualKeyModifiers.Control });
            commands.Add(forwardCommand);

            var backCommand = new XamlUICommand
            {
                Label = "Back",
                IconSource = new SymbolIconSource { Symbol = Symbol.Back },
                Command = vm.BackCommand
            };
            backCommand.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.Right, Modifiers = VirtualKeyModifiers.Control });
            commands.Add(backCommand);

            var reloadCommand = new XamlUICommand
            {
                Label = "Reload",
                IconSource = new SymbolIconSource { Symbol = Symbol.Refresh },
                Command = vm.ReloadCommand
            };
            reloadCommand.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.R, Modifiers = VirtualKeyModifiers.Control });

            commands.Add(reloadCommand);

            var saveCommand = new XamlUICommand
            {
                Label = "Save",
                IconSource = new SymbolIconSource { Symbol = Symbol.Save },
                Command = vm.SavePageCommand
            };
            saveCommand.KeyboardAccelerators.Add(new KeyboardAccelerator() { Key = VirtualKey.S, Modifiers = VirtualKeyModifiers.Control });

            commands.Add(saveCommand);
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                vm.Uri = e.Parameter as Uri;
                await vm.NavigateCommand.ExecuteAsync(null);
            }
        }

        private void OnViewmodelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentPage):
                {
                    if (vm.State != ViewState.Page)
                        return;

                    PageContent.ChangeView(0, 0, 1, true);
                    var contentType = new ContentType(vm.CurrentPage.Meta);
                    if (contentType.MediaType == "text/gemini")
                    {
                        var document = GeminiService.ParseBody(Encoding.UTF8.GetChars(vm.CurrentPage.Body, 0, vm.CurrentPage.Body.Length), vm.Uri);
                        GemtextRenderer.Render(document, contentBox.Inlines, OnLinkClicked);
                    }
                    else if(contentType.MediaType.StartsWith("text"))
                    {
                        contentBox.Inlines.Clear();
                        contentBox.Inlines.Add(new Run() { Text = Encoding.UTF8.GetString(vm.CurrentPage.Body), FontFamily = new FontFamily("Cascadia Mono, Consolas") });
                    }
                    else
                    {
                       vm.State = ViewState.UnsupportedFile;
                    }
                    break;
                }
            }
        }

        private async void OnLinkClicked(Uri uri)
        {
            if (uri.Scheme == "gemini")
            {
                vm.Uri = uri;
                await vm.NavigateCommand.ExecuteAsync(null);
            }
            else
            {
                _ = Launcher.LaunchUriAsync(uri);
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetRootForTab(this.XamlRoot);
        }

        Visibility isRetryVisible(GeminiResponseType type)
        {
            return type == GeminiResponseType.TemporaryFailure ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}