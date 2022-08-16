using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using System.Collections.Generic;
using WinRT.Interop;

using Twin.Views;
using Twin.Helpers;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        static List<WindowTabViewPair> CurrentWindows = new();

        string appTitle
        {
            get
            {
                return this.Title;
            }
            set
            {
                this.Title = value;
            }
        }

        Uri startUri;
        public Uri StartUri
        {
            get
            {
                return startUri;
            }
            set
            {
                startUri = value;
                if (startUri != null)
                {
                    rootFrame.Navigate(typeof(BrowserTabView), value);
                    CurrentWindows.Add(new WindowTabViewPair(this, (rootFrame.Content as BrowserTabView).TabView));
                }
            }
        }

        public MainWindow()
        {
            appTitle = "Twin";
            this.InitializeComponent();
            new MicaHelper(this).TrySetMicaBackdrop();
            SetCustomTitlebar();
            rootFrame.Navigate(typeof(BrowserTabView));
            CurrentWindows.Add(new WindowTabViewPair(this, (rootFrame.Content as BrowserTabView).TabView));
            var b = CurrentWindows;
        }

        public MainWindow(UIElement element)
        {
            appTitle = "Twin";
            this.InitializeComponent();
            new MicaHelper(this).TrySetMicaBackdrop();
            SetCustomTitlebar();
            rootFrame.Content = element;
            if(element != null && (element.GetType() == typeof(BrowserTabView)))
            {
                CurrentWindows.Add(new WindowTabViewPair(this, (element as BrowserTabView).TabView));
            }
        }

        public static Window GetCurrentWindow(TabView tabView)
        {
            var a = CurrentWindows;
            return CurrentWindows.Find(e => e.TabView == tabView).Window;
        }

        ~MainWindow()
        {
            CurrentWindows.RemoveAll(e => e.Window == this);
        }

        private void SetCustomTitlebar()
        {
            //if (!AppWindowTitleBar.IsCustomizationSupported()) return;

            //IntPtr hWnd = WindowNative.GetWindowHandle(this);
            //WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            //var appWindow = AppWindow.GetFromWindowId(wndId);

            //var titlebar = appWindow.TitleBar;
            //titlebar.ExtendsContentIntoTitleBar = true;
            //titlebar.ButtonBackgroundColor = Colors.Transparent;
            //titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
            //titlebar.ButtonHoverBackgroundColor = ((Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListLowBrush"]).Color;
            //titlebar.ButtonPressedBackgroundColor = ((Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListMediumBrush"]).Color;

            //titlebar.SetDragRectangles(null);
        }

        readonly record struct WindowTabViewPair(Window Window, TabView TabView);
    }
}
