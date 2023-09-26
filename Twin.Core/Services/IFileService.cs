using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Core.Services
{
    public interface IFileService
    {
        Task SaveFileAsync(string name, string extension, byte[] data);
    }
}
