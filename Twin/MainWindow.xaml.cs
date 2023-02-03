using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;

using Twin.Views;
using Twin.Helpers;
using System.Diagnostics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        public AppWindow AppWindow;

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
                    rootFrame.Navigate(typeof(BrowserView), value);
                }
            }
        }

        public MainWindow()
        {
            appTitle = "Twin";
            this.InitializeComponent();
            new MicaHelper(this).TrySetMicaBackdrop();
            SetCustomTitlebar();
            rootFrame.Navigate(typeof(BrowserView));
        }

        private void SetCustomTitlebar()
        {
            if (!AppWindowTitleBar.IsCustomizationSupported()) return;

            IntPtr hWnd = WindowNative.GetWindowHandle(this);
            WindowId wndId = Win32Interop.GetWindowIdFromWindow(hWnd);
            AppWindow = AppWindow.GetFromWindowId(wndId);

            var titlebar = AppWindow.TitleBar;
            titlebar.ExtendsContentIntoTitleBar = true;
            titlebar.PreferredHeightOption = TitleBarHeightOption.Tall;
            titlebar.ButtonBackgroundColor = Colors.Transparent;
            titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titlebar.ButtonHoverBackgroundColor = ((Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListLowBrush"]).Color;
            titlebar.ButtonPressedBackgroundColor = ((Microsoft.UI.Xaml.Media.SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListMediumBrush"]).Color;
        }
    }
}
