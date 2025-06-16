using Phobos.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.System;

namespace Phobos.Services
{
    internal class EnvironmentService : IEnvironmentService
    {
        public string DataDirectory => ApplicationData.Current.LocalFolder.Path;

        public async Task LaunchFile(string path) => await Launcher.LaunchUriAsync(new Uri(path));
    }
}
