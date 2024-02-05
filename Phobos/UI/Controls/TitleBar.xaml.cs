using Microsoft.UI;
using Microsoft.UI.Input;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Hosting;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Phobos.UI.Controls
{
    public sealed partial class TitleBar : UserControl
    {
        public TitleBar()
        {
            this.InitializeComponent();
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (!AppWindowTitleBar.IsCustomizationSupported()) return;
            var appWindow = (App.Current as App).m_window?.AppWindow;

            if (!appWindow.TitleBar.ExtendsContentIntoTitleBar) return;

            appWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Standard;

            var m = (int)(32 * GetScaleAdjustment(appWindow));

            if (appWindow.Presenter.Kind == AppWindowPresenterKind.Overlapped)
            {
                RectInt32[] dragRects = [new() { X = 0, Y = 0, Width = m, Height = m }];
                InputNonClientPointerSource.GetForWindowId(appWindow.Id).SetRegionRects(NonClientRegionKind.Passthrough, dragRects);
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Debug.WriteLine((sender as UIElement).TransformToVisual(this.XamlRoot.Content).TransformPoint(new Point(0, 0)));
        }
    }
}
