using Microsoft.UI.Xaml;
using Microsoft.Windows.AppLifecycle;
using System;
using Phobos.Core.Services;
using Phobos.Core.ViewModels;
using Phobos.Services;
using Windows.ApplicationModel.Activation;
using Jab;
using System.Diagnostics.CodeAnalysis;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Phobos
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : Application
    {
        public MainWindow mainWindow;
        public IServiceProvider Services { get; private set; }
        
        public App()
        {
            this.InitializeComponent();
        }

        [MemberNotNull(nameof(Services))]
        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            this.Services = new ServiceProvider();

            var appArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            mainWindow = appArgs.Kind switch
            {
                ExtendedActivationKind.Protocol => new((appArgs.Data as ProtocolActivatedEventArgs)!.Uri),
                ExtendedActivationKind.CommandLineLaunch => new(ParseInputUri((appArgs.Data as CommandLineActivatedEventArgs)!.Operation.Arguments)),
                _ => new()
            };


            mainWindow.Activate();
        }

        private Uri? ParseInputUri(string cliArgs)
        {
            try
            {
                return new Uri(cliArgs.Split()[0]);
            }
            catch
            {
                return null;
            }
        }
    }

    [ServiceProvider]
    [Transient(typeof(BrowserViewModel))]
    [Singleton(typeof(GeminiService))]
    [Singleton(typeof(IAppDataService), typeof(AppDataService))]
    [Singleton(typeof(IEnvironmentService), typeof(EnvironmentService))]
    [Scoped(typeof(XamlRootService))]
    [Scoped(typeof(IFileService), typeof(FileService))]
    public partial class ServiceProvider { }
}
