using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.ApplicationModel.DataTransfer;
using Microsoft.UI;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Windowing;
using System.Diagnostics;
using CommunityToolkit.WinUI.UI;
using WinRT.Interop;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin.Views
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BrowserTabView : Page
    {
        private const string DataIdentifier = "TwinTab";
        public TabView TabView;

        public BrowserTabView()
        {
            this.InitializeComponent();
            this.tabView.TabItems.Add(CreateBrowserTab());
            this.TabView = tabView;
        }
        
        public BrowserTabView(TabViewItem item)
        {
            this.InitializeComponent();
            this.tabView.TabItems.Add(item ?? CreateBrowserTab());
            this.TabView = tabView;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if(e.Parameter != null)
            {
                var args = e.Parameter;
                if (args != null)
                {
                    TabViewItem tab = new();
                    Frame content = new();
                    content.Navigate(typeof(BrowserView), args);
                    tab.Content = content;
                    tabView.TabItems.Add(tab);
                    tabView.SelectedItem = tab;
                }
            }
            base.OnNavigatedTo(e);
        }

        private void OnAddTab(TabView sender, object args)
        {
            var tab = CreateBrowserTab();
            sender.TabItems.Add(tab);
            sender.SelectedItem = tab;
        }

        private void OnCloseTab(TabView sender, TabViewTabCloseRequestedEventArgs args)
        {
            sender.TabItems.Remove(args.Tab);
        }

        private void OnTabDroppedOutside(TabView sender, TabViewTabDroppedOutsideEventArgs args)
        {
            if (sender.TabItems.Count > 1)
            {
                sender.TabItems.Remove(args.Tab);
                BrowserTabView newTabView = new(args.Tab);
                MainWindow newWindow = new(newTabView);
                var a = sender.TabItems.Contains(args.Tab);
                newWindow.Activate();
            }
        }

        private void OnTabStripDrop(object sender, DragEventArgs e)
        {
            // This event is called when we're dragging between different TabViews
            // It is responsible for handling the drop of the item into the second TabView

            if (e.DataView.Properties.TryGetValue(DataIdentifier, out object obj))
            {
                // Ensure that the obj property is set before continuing.
                if (obj == null)
                {
                    return;
                }

                var destinationTabView = sender as TabView;
                var destinationItems = destinationTabView.TabItems;

                if (destinationItems != null)
                {
                    // First we need to get the position in the List to drop to
                    var index = -1;

                    // Determine which items in the list our pointer is between.
                    for (int i = 0; i < destinationTabView.TabItems.Count; i++)
                    {
                        var item = destinationTabView.ContainerFromIndex(i) as TabViewItem;

                        if (e.GetPosition(item).X - item.ActualWidth < 0)
                        {
                            index = i;
                            break;
                        }
                    }

                    // The TabView can only be in one tree at a time. Before moving it to the new TabView, remove it from the old.
                    var args = (TabDropArgs)obj;
                    args.TabView.TabItems.Remove(args.Tab);

                    if (index < 0)
                    {
                        // We didn't find a transition point, so we're at the end of the list
                        destinationItems.Add(args.Tab);
                    }
                    else if (index < destinationTabView.TabItems.Count)
                    {
                        // Otherwise, insert at the provided index.
                        destinationItems.Insert(index, args.Tab);
                    }

                    // Select the newly dragged tab
                    destinationTabView.SelectedItem = args.Tab;
                }
            }
        }

        private void tabView_TabDragStarting(TabView sender, TabViewTabDragStartingEventArgs args)
        {
            // We can only drag one tab at a time, so grab the first one...
            var firstItem = new TabDropArgs(this.tabView, args.Tab);

            // ... set the drag data to the tab...
            args.Data.Properties.Add(DataIdentifier, firstItem);

            // ... and indicate that we can move it
            args.Data.RequestedOperation = DataPackageOperation.Move;
        }

        private void tabView_TabStripDragOver(object sender, DragEventArgs e)
        {
            if (e.DataView.Properties.ContainsKey(DataIdentifier))
            {
                e.AcceptedOperation = DataPackageOperation.Move;
            }
        }

        private void tabView_TabItemsChanged(TabView sender, IVectorChangedEventArgs args)
        {
            //if(sender.TabItems.Count == 0)
            //{
            //    IntPtr hWnd = WindowNative.GetWindowHandle(this);
            //    WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            //    var appWindow = AppWindow.GetFromWindowId(wndId);
            //    appWindow.Destroy();
            //}
        }

        private TabViewItem CreateBrowserTab(Uri parameter = null)
        {
            TabViewItem tab = new();
            tab.Header = "New tab";
            tab.IconSource = new SymbolIconSource() { Symbol = Symbol.Document };
            tab.Foreground = new SolidColorBrush(Windows.UI.Color.FromArgb(115, 58, 58, 58));
            Frame content = new();
            content.Navigate(typeof(BrowserView), parameter);
            tab.Content = content;
            return tab;
        }

        private void tabView_TabDragCompleted(TabView sender, TabViewTabDragCompletedEventArgs args)
        {
            if (args.DropResult == DataPackageOperation.Move)
            {
                var a = MainWindow.GetCurrentWindow(tabView);
                if (a != null && tabView.TabItems.Count == 0)
                {
                    a.Close();
                }
            }
        }
    }

    public readonly record struct TabDropArgs(TabView TabView, TabViewItem Tab);
}
