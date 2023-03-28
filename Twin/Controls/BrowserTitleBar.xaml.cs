using Microsoft.UI;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Twin.Helpers;
using Twin.Core.Models;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin.Controls
{
    public sealed partial class BrowserTitleBar : UserControl
    {
        #region DependencyProperties
        public bool IsBackEnabled
        {
            get { return (bool)GetValue(IsBackEnabledProperty); }
            set { SetValue(IsBackEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for MyProperty.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsBackEnabledProperty =
            DependencyProperty.Register("IsBackEnabled", typeof(bool), typeof(BrowserTitleBar), new PropertyMetadata(false));

        public bool IsForwardEnabled
        {
            get { return (bool)GetValue(IsForwardEnabledProperty); }
            set { SetValue(IsForwardEnabledProperty, value); }
        }

        // Using a DependencyProperty as the backing store for IsForwardEnabled.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty IsForwardEnabledProperty =
            DependencyProperty.Register("IsForwardEnabled", typeof(bool), typeof(BrowserTitleBar), new PropertyMetadata(false));

        public Uri Uri
        {
            get { return (Uri)GetValue(UriProperty); }
            set { SetValue(UriProperty, value); }
        }

        // Using a DependencyProperty as the backing store for DisplayUri.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty UriProperty =
            DependencyProperty.Register("Uri", typeof(Uri), typeof(BrowserTitleBar), new PropertyMetadata(""));

        public MenuFlyout MenuItems
        {
            get => (MenuFlyout)GetValue(MenuItemsProperty);
            set => SetValue(MenuItemsProperty, value);
        }

        public static readonly DependencyProperty MenuItemsProperty =
            DependencyProperty.Register(nameof(MenuItems), typeof(MenuFlyout), typeof(BrowserTitleBar), null);

        public History History
        {
            get => (History)GetValue(HistoryProperty);
            set => SetValue(HistoryProperty, value);
        }

        public static readonly DependencyProperty HistoryProperty =
            DependencyProperty.Register(nameof(History), typeof(History), typeof(BrowserTitleBar), null);

        public event EventHandler<Uri> UriChanged;

        public event RoutedEventHandler BackRequested;

        public event RoutedEventHandler ForwardRequested;

        public event RoutedEventHandler ReloadRequested;

        #endregion

        public BrowserTitleBar()
        {
            this.InitializeComponent();
            Loaded += OnSizeChanged;
            SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, object e)
        {
            if (!AppWindowTitleBar.IsCustomizationSupported()) return;
            var appWindow = (App.Current as App).m_window?.AppWindow;

            if (!appWindow.TitleBar.ExtendsContentIntoTitleBar) return;

            if (appWindow.Presenter.Kind == AppWindowPresenterKind.Overlapped)
            {
                SetDragRectangles(appWindow);
            }
        }

        private void SetDragRectangles(AppWindow appWindow)
        {
            if (AppWindowTitleBar.IsCustomizationSupported() && appWindow.TitleBar.ExtendsContentIntoTitleBar)
            {
                double scale = GetScaleAdjustment(appWindow);
                LeftInset.Width = new(appWindow.TitleBar.LeftInset / scale);
                RightInset.Width = new(appWindow.TitleBar.RightInset / scale);

                List<RectInt32> rectList = new()
                {
                    new()
                    {
                        X = 0,
                        Y = 0,
                        Width = (int)(this.ActualWidth * scale),
                        Height = (int)(10 * scale)
                    },

                    new()
                    {
                        X = (int)((LeftInset.ActualWidth + NavButtons.ActualWidth) * scale),
                        Y = 0,
                        Width = (int)(LeftDragArea.ActualWidth * scale),
                        Height = (int)(this.ActualHeight * scale)
                    },

                    new()
                    {
                        X = (int)((this.ActualWidth - RightInset.ActualWidth - RightDragArea.ActualWidth + FlyoutButton.ActualWidth) * scale),
                        Y = 0,
                        Width = (int)(RightDragArea.ActualWidth * scale),
                        Height = (int)(this.ActualHeight * scale)
                    }
                };
                appWindow.TitleBar.SetDragRectangles(rectList.ToArray());
            }
        }

        private double GetScaleAdjustment(AppWindow appWindow)
        {
            DisplayArea displayArea = DisplayArea.GetFromWindowId(appWindow.Id, DisplayAreaFallback.Primary);
            IntPtr hMonitor = Win32Interop.GetMonitorFromDisplayId(displayArea.DisplayId);

            // Get DPI.
            int result = GetDpiForMonitor(hMonitor, Monitor_DPI_Type.MDT_Default, out uint dpiX, out uint _);
            if (result != 0)
            {
                throw new Exception("Could not get DPI for monitor.");
            }

            uint scaleFactorPercent = (uint)(((long)dpiX * 100 + (96 >> 1)) / 96);
            return scaleFactorPercent / 100.0;
        }

        [DllImport("Shcore.dll", SetLastError = true)]
        internal static extern int GetDpiForMonitor(IntPtr hmonitor, Monitor_DPI_Type dpiType, out uint dpiX, out uint dpiY);

        internal enum Monitor_DPI_Type : int
        {
            MDT_Effective_DPI = 0,
            MDT_Angular_DPI = 1,
            MDT_Raw_DPI = 2,
            MDT_Default = MDT_Effective_DPI
        }

        private void OnUriChanged(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            Uri = UriHelpers.StringToUri((args.Element as TextBox).Text);
            if (Uri is null) return;
            if (UriChanged is null) return;

            UriChanged.Invoke(this, Uri);
            LoseFocus(args.Element);
        }

        // https://stackoverflow.com/questions/24251356/
        private void LoseFocus(object sender)
        {
            var control = sender as Control;
            var isTabStop = control.IsTabStop;
            control.IsTabStop = false;
            control.IsEnabled = false;
            control.IsEnabled = true;
            control.IsTabStop = isTabStop;
        }

        private void OnBackRequested(object sender, RoutedEventArgs e)
        {
            if (BackRequested is null) return;

            BackRequested.Invoke(this, e);
        }

        private void OnForwardRequested(object sender, RoutedEventArgs e)
        {
            if (ForwardRequested is null) return;

            ForwardRequested.Invoke(this, e);
        }

        private void OnReloadRequested(object sender, RoutedEventArgs e)
        {
            if (ReloadRequested is null) return;

            ReloadRequested.Invoke(this, e);
        }

        private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout mf = new()
            {
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.Bottom
            };
            mf.Items.Clear();
            for (int i = 0; i <= History.CurrentPage; i++)
            {
                mf.Items.Add(new MenuFlyoutItem() { Text = UriHelpers.UriToString(History.Items[History.Items.Count - 1 - i]) });
            }
            mf.ShowAt(sender as FrameworkElement);
        }

        private void OnPanelButtonClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
