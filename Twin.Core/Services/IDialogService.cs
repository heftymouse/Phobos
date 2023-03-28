using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Core.Services
{
    public interface IDialogService
    {
        public Task<string> ShowInputDialogForResult(string message, bool sensitive = false);
    }
}
