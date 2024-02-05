using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Phobos.Core.Models;
using Phobos.Core.ViewModels;
using Phobos.Helpers;
using Phobos.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Phobos.UI.Controls
{
    public sealed partial class BookmarksList : UserControl
    {
        public BrowserViewModel ViewModel
        {
            get => (BrowserViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserTitleBar), null);

        public BookmarksList()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            //ViewModel.AddBookmark()
        }

        private void Remove(object sender, RoutedEventArgs e)
        {
            ViewModel.RemoveBookmark((sender as FrameworkElement).DataContext as Bookmark);
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            ViewModel.AddBookmark(NameBox.Text, UriHelpers.StringToUri(UriBox.Text));
        }

        private void TheList_ItemClick(object sender, ItemClickEventArgs e)
        {
            ViewModel.Uri = (e.ClickedItem as Bookmark).Uri;
            ViewModel.NavigateCommand.Execute(null);
        }
    }
}
