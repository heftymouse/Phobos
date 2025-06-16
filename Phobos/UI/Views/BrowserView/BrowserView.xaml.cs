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
using CommunityToolkit.Mvvm.Input;
using System.Windows.Input;
using Windows.Security.Cryptography.Certificates;
using System.Diagnostics;

namespace Phobos.UI.Views
{
    public sealed partial class BrowserView : Page, ITabContext
    {
        BrowserViewModel vm;
        List<XamlUICommand> commands = new();
        Dictionary<KeyboardAccelerator, ICommand> acceleratorCommandMap = new();

        public BrowserView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            vm = IocHelpers.GetServicesForTab(this).GetRequiredService<BrowserViewModel>();
            vm.PropertyChanged += OnViewmodelPropertyChanged;

            AddCommandWithAccelerator(
                new XamlUICommand { Label = "Forward", IconSource = new SymbolIconSource { Symbol = Symbol.Forward }, Command = vm.ForwardCommand },
                VirtualKey.Right, VirtualKeyModifiers.Control, vm.ForwardCommand);

            AddCommandWithAccelerator(
                new XamlUICommand { Label = "Back", IconSource = new SymbolIconSource { Symbol = Symbol.Back }, Command = vm.BackCommand },
                VirtualKey.Left, VirtualKeyModifiers.Control, vm.BackCommand);

            AddCommandWithAccelerator(
                new XamlUICommand { Label = "Reload", IconSource = new SymbolIconSource { Symbol = Symbol.Refresh }, Command = vm.ReloadCommand },
                VirtualKey.R, VirtualKeyModifiers.Control, vm.ReloadCommand);

            AddCommandWithAccelerator(
                new XamlUICommand { Label = "Save", IconSource = new SymbolIconSource { Symbol = Symbol.Save }, Command = vm.SavePageCommand },
                VirtualKey.S, VirtualKeyModifiers.Control, vm.SavePageCommand);

            AddCommandWithAccelerator(
                new XamlUICommand
                {
                    Label = "New Window",
                    IconSource = new SymbolIconSource { Symbol = Symbol.Add },
                    Command = new RelayCommand(() =>
                    {
                        var w = new MainWindow();
                        w.Activate();
                    })
                },
                VirtualKey.N, VirtualKeyModifiers.Control, new RelayCommand(() =>
                {
                    var w = new MainWindow();
                    w.Activate();
                }));
        }

        private void AddCommandWithAccelerator(XamlUICommand command, VirtualKey key, VirtualKeyModifiers modifiers, ICommand mappedCommand)
        {
            var cmdAccel = new KeyboardAccelerator() { Key = key, Modifiers = modifiers };
            var pageAccel = new KeyboardAccelerator() { Key = key, Modifiers = modifiers };
            pageAccel.Invoked += OnKeyboardAccelerator;
            command.KeyboardAccelerators.Add(cmdAccel);
            this.KeyboardAccelerators.Add(pageAccel);
            acceleratorCommandMap[pageAccel] = mappedCommand;
            commands.Add(command);
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

        private void OnViewmodelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(vm.CurrentPage):
                {
                    if (vm.State != ViewState.Page)
                        return;

                    PageContent.ChangeView(0, 0, 1, true);
                    certMismatchAlert.IsOpen = false;
                    newCertAlert.IsOpen = false;
                    switch(vm.CurrentPage.CertificateStatus)
                    {
                        case CertificateStatus.New:
                            newCertAlert.IsOpen = true;
                            break;
                        case CertificateStatus.Mismatch:
                            certMismatchAlert.IsOpen = true;
                            break;
                    }
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

        private void OnKeyboardAccelerator(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            if (acceleratorCommandMap.TryGetValue(sender, out var command))
            {
                if (command.CanExecute(null))
                    command.Execute(null);
            }
            args.Handled = true;
        }

        private void OnResetZoom(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var scrollViewer = args.Element as ScrollViewer;
            scrollViewer?.ChangeView(null, null, 1);
        }
    }
}