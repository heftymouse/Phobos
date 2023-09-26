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
using Microsoft.UI.Input;
using System.Windows.Input;
using Twin.Core.ViewModels;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin.Controls
{
    public sealed partial class BrowserTitleBar : UserControl
    {
        public BrowserViewModel ViewModel
        {
            get => (BrowserViewModel)GetValue(ViewModelProperty);
            set => SetValue(ViewModelProperty, value);
        }

        public static readonly DependencyProperty ViewModelProperty =
            DependencyProperty.Register(nameof(ViewModel), typeof(BrowserViewModel), typeof(BrowserTitleBar), null);

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
                InputNonClientPointerSource.GetForWindowId(appWindow.Id).SetRegionRects(NonClientRegionKind.Caption, rectList.ToArray());
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

        private async void OnUriChanged(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            ViewModel.Uri = UriHelpers.StringToUri((args.Element as TextBox).Text);
            if (ViewModel.Uri is null) return;
            await ViewModel.NavigateCommand.ExecuteAsync(null);

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

        private void Button_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            MenuFlyout mf = new()
            {
                Placement = Microsoft.UI.Xaml.Controls.Primitives.FlyoutPlacementMode.BottomEdgeAlignedLeft
            };
            mf.Items.Clear();
            for (int i = 0; i <= ViewModel.History.CurrentPage; i++)
            {
                mf.Items.Add(new MenuFlyoutItem() { Text = UriHelpers.UriToString(ViewModel.History.Items[ViewModel.History.Items.Count - 1 - i]) });
            }
            mf.ShowAt(sender as FrameworkElement);
        }

        private void OnPanelButtonClicked(object sender, RoutedEventArgs e)
        {

        }
    }
}
