using Phobos.Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace Phobos.Services
{
    internal class EnvironmentService : IEnvironmentService
    {
        public string DataDirectory => ApplicationData.Current.LocalFolder.Path;
    }
}
