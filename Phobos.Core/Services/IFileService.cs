using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Services
{
    public interface IFileService
    {
        Task SaveFileAsync(string name, string extension, byte[] data);
        Task<string> SaveTempFileAsync(string name, byte[] data);
    }
}
