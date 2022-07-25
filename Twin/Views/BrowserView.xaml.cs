using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Dispatching;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Pickers;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using WinRT.Interop;

using Twin.Models;
using Twin.Helpers;
using Twin.Converters;
using Twin.Gemini;

namespace Twin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserView : Page
    {
        BrowserModel model = new();

        public BrowserView()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Enabled;
        }

        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                await GoToPage(e.Parameter as Uri);
            }
        }

        private async void OnNavigateUrl(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            UriBuilder uriBuilder = new UriBuilder((args.Element as TextBox).Text);
            uriBuilder.Scheme = "gemini";
            uriBuilder.Port = uriBuilder.Port is not (1965 or 80 or -1) ? uriBuilder.Port : 1965;
            await GoToPage(uriBuilder.Uri);
            model.History.Add(uriBuilder.Uri);
        }

        private async Task GoToPage(Uri uri)
        {
            if (uri == null) return;
            DispatcherQueue.TryEnqueue(DispatcherQueuePriority.High,
                async () =>
                {
                    try
                    {
                        model.IsLoading = true;
                        model.CurrentPage = await GeminiRequest.RequestPageAsync(uri);
                        switch (model.CurrentPage.Type)
                        {
                            case GeminiResponseType.Input:
                                var result = await GetPageInput();
                                if (result == null) return;
                                UriBuilder builder = new(uri);
                                builder.Query = result;
                                await GoToPage(builder.Uri);
                                return;
                            case GeminiResponseType.Redirect:
                                await GoToPage(new Uri(model.CurrentPage.Meta));
                                return;
                            case GeminiResponseType.TemporaryFailure:
                                await ShowErrorDialog(uri, true);
                                model.IsLoading = false;
                                return;
                            case GeminiResponseType.PermanentFailure:
                                await ShowErrorDialog(null, false);
                                model.IsLoading = false;
                                return;

                        }
                        model.Uri = uri;
                        contentBox.Inlines.Clear();
                            // this is dumbass
                        foreach (Inline i in GemtextHelper.Format(model.CurrentPage.Body, model.Uri))
                        {
                            if (i.GetType() == typeof(Hyperlink))
                            {
                                var link = i as Hyperlink;
                                if (link.NavigateUri.Scheme == "gemini")
                                {
                                    (i as Hyperlink).NavigateUri = null;
                                }
                                    (i as Hyperlink).Click += OnLinkClicked;
                            }
                            contentBox.Inlines.Add(i);
                        }
                        model.IsLoading = false;
                        Bindings.Update();
                    }
                    catch (Exception e)
                    {
                        string formattedText = $"{e.InnerException?.Message ?? e.Message}\n\n{e.InnerException?.StackTrace ?? e.StackTrace}";
                        ScrollViewer scrollViewer = new()
                        {
                            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                            Content = new TextBlock()
                            {
                                FontFamily = new FontFamily("Cascadia Mono"),
                                Text = formattedText
                            }
                        };
                        ContentDialog dialog = new()
                        {
                            XamlRoot = this.XamlRoot,
                            Title = "An internal error occurred",
                            Content = scrollViewer,
                            PrimaryButtonText = "OK",
                            SecondaryButtonText = "Copy",
                        };
                        dialog.SecondaryButtonClick += (sender, e) =>
                        {
                            DataPackage data = new();
                            data.SetData(StandardDataFormats.Text, formattedText);
                            Clipboard.SetContent(data);
                        };

                        model.IsLoading = false;
                        await dialog.ShowAsync();
                    }
                });
        }

        private async void OnGoBack(object sender, RoutedEventArgs e)
        {
            await GoToPage(model.History.PreviousPage());
        }

        private async void OnGoForward(object sender, RoutedEventArgs e)
        {
            await GoToPage(model.History.NextPage());
        }

        private async void OnReload(object sender, RoutedEventArgs e)
        {
            await GoToPage(model.Uri);
        }

        private async void OnLinkClicked(object sender, RoutedEventArgs e)
        {
            var link = sender as Hyperlink;
            var uri = new Uri(ToolTipService.GetToolTip(link) as string);
            if (uri.Scheme == "gemini")
            {
                model.History.Add(uri);
                await GoToPage(uri);
            }
        }

        private async Task<string> GetPageInput()
        {
            TextBox textBox = new();
            textBox.PlaceholderText = model.CurrentPage.Meta;

            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Title = "This page is requesting user input",
                Content = textBox,
                PrimaryButtonText = "Submit",
                CloseButtonText = "Close"
            };

            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                return textBox.Text;
            }
            else
            {
                return null;
            }
        }

        private async void SavePage(object sender, RoutedEventArgs e)
        {
            FileSavePicker savePicker = new();
            WinRT.Interop.InitializeWithWindow.Initialize(savePicker, GetActiveWindow());

            savePicker.SuggestedStartLocation = PickerLocationId.Downloads;
            savePicker.SuggestedFileName = model.Uri.AbsoluteUri.TrimEnd('/').Split('/')[^1];
            savePicker.FileTypeChoices.Add("Gemini Document", new List<string>() { ".gmi" });
            Windows.Storage.StorageFile file = await savePicker.PickSaveFileAsync();
            if (file != null)
            {
                // Prevent updates to the remote version of the file until
                // we finish making changes and call CompleteUpdatesAsync.
                Windows.Storage.CachedFileManager.DeferUpdates(file);
                // write to file
                await Windows.Storage.FileIO.WriteTextAsync(file, model.CurrentPage.Body);
                // Let Windows know that we're finished changing the file so
                // the other app can update the remote version of the file.
                // Completing updates may require Windows to ask for user input.
                Windows.Storage.Provider.FileUpdateStatus status =
                    await Windows.Storage.CachedFileManager.CompleteUpdatesAsync(file);
                if (status != Windows.Storage.Provider.FileUpdateStatus.Complete)
                {
                    ContentDialog dialog = new()
                    {
                        XamlRoot = this.XamlRoot,
                        Title = "An error occured when trying to save this page.",
                        PrimaryButtonText = "OK",
                    };

                    await dialog.ShowAsync();
                }
            }
        }

        [DllImport("User32.dll")]
        private static extern IntPtr GetActiveWindow();

        private async Task ShowErrorDialog(Uri uri, bool retry)
        {
            ContentDialog dialog = new()
            {
                XamlRoot = this.XamlRoot,
                Title = "An error occured",
                Content = $"Server returned code {(int)model.CurrentPage.Type}{model.CurrentPage.Code}\n{model.CurrentPage.Meta}",
                PrimaryButtonText = "OK",
            };
            if (retry && uri != null)
            {
                dialog.SecondaryButtonText = "Retry";
                dialog.SecondaryButtonClick += async (sender, e) => await GoToPage(uri);
            }

            await dialog.ShowAsync();
        }

        private async void ShowInfoDialog(object sender, RoutedEventArgs e)
        {
            await aboutDialog.ShowAsync();
        }
    }
}
