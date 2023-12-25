using Microsoft.UI.Xaml;
using Microsoft.UI;
using Microsoft.UI.Windowing;
using System;
using WinRT.Interop;
using Phobos.UI.Views;
using Phobos.Helpers;
using Microsoft.UI.Xaml.Media;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Phobos
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {

        public MainWindow(Uri startUri = null)
        {
            this.Title = "Phobos";
            this.InitializeComponent();
            SetCustomTitlebar();
            RootFrame.Hwnd = WindowNative.GetWindowHandle(this);
            RootFrame.Navigate(typeof(BrowserView), startUri);
        }

        private void SetCustomTitlebar()
        {
            if (!AppWindowTitleBar.IsCustomizationSupported()) return;

            var titlebar = AppWindow.TitleBar;
            titlebar.ExtendsContentIntoTitleBar = true;
            titlebar.PreferredHeightOption = TitleBarHeightOption.Tall;
            titlebar.IconShowOptions = IconShowOptions.HideIconAndSystemMenu;
            titlebar.ButtonBackgroundColor = Colors.Transparent;
            titlebar.ButtonInactiveBackgroundColor = Colors.Transparent;
            titlebar.ButtonHoverBackgroundColor = ((SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListLowBrush"]).Color;
            titlebar.ButtonPressedBackgroundColor = ((SolidColorBrush)App.Current.Resources.ThemeDictionaries["SystemControlBackgroundListMediumBrush"]).Color;
        }
    }
}
