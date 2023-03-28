using Microsoft.Extensions.DependencyInjection;
using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using Twin.Core.Services;
using Twin.Core.ViewModels;
using Twin.Services;
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
        public IServiceProvider Services { get; private set; }
        
        public App()
        {
            this.InitializeComponent();
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.ConfigureServices();

            var appArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            m_window = appArgs.Kind switch
            {
                ExtendedActivationKind.Protocol => new((appArgs.Data as ProtocolActivatedEventArgs).Uri),
                ExtendedActivationKind.CommandLineLaunch => new(ParseInputUri((appArgs.Data as CommandLineActivatedEventArgs).Operation.Arguments)),
                _ => new()
            };


            m_window.Activate();
        }

        private void ConfigureServices()
        {
            this.Services = new ServiceCollection()
                .AddTransient<BrowserViewModel>() // viewmodels
                .AddSingleton<GeminiService>() // services
                .AddTransient<IDialogService, DialogService>()
                .BuildServiceProvider();
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
