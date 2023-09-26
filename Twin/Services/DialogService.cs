using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Twin.Core.Services;
using Twin.Helpers;
using Windows.System;

namespace Twin.Services
{
    internal class DialogService : IDialogService
    {
        private XamlRootService xamlRoot;

        public DialogService(XamlRootService xamlRoot)
        {
            this.xamlRoot = xamlRoot;
        }

        public async Task<string> ShowInputDialogForResult(string message, bool sensitive = false)
        {
            PasswordBox inputBox = new PasswordBox()
            {
                PlaceholderText = message,
                PasswordRevealMode = sensitive ? PasswordRevealMode.Peek : PasswordRevealMode.Visible
            };

            ContentDialog dialog = new ContentDialog()
            {
                XamlRoot = xamlRoot.Root,
                Style = Application.Current.Resources["DefaultContentDialogStyle"] as Style,
                Title = "This page is requesting input",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                Content = inputBox
            };

            var result = await dialog.ShowAsync();
            return string.IsNullOrEmpty(inputBox.Password) ? string.Empty : inputBox.Password;
        }
    }
}
