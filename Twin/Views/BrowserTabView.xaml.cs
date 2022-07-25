using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserTabView : Page
    {
        public BrowserTabView()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (e.Parameter != null)
            {
                TabViewItem tab = new();
                Frame content = new();
                content.Navigate(typeof(BrowserView), e.Parameter);
                tab.Content = content;
            }
        }

        private void TabView_AddTabButtonClick(TabView sender, object args)
        {
            TabViewItem tab = new();
            tab.Header = "New tab";
            tab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
            Frame content = new();
            content.Navigate(typeof(BrowserView));
            tab.Content = content;
            (sender as TabView).TabItems.Add(tab);
        }

        private void TabView_TabCloseRequested(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }
    }
}
