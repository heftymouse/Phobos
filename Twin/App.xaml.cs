using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using Windows.ApplicationModel.Activation;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Twin
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public MainWindow m_window;

        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="args">Details about the launch request and process.</param>
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            var appArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            m_window = appArgs.Kind switch
            {
                ExtendedActivationKind.Protocol => new((appArgs.Data as ProtocolActivatedEventArgs).Uri),
                ExtendedActivationKind.CommandLineLaunch => new(ParseInputUri((appArgs.Data as CommandLineActivatedEventArgs).Operation.Arguments)),
                _ => new()
            };

            m_window.Activate();
        }

        private Uri ParseInputUri(string cliArgs)
        {
            try
            {
                var uri = new Uri(cliArgs.Split()[0]);
                return uri;
            }
            catch
            {
                return null;
            }
        }
    }
}
