using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
{
    public enum ViewState
    {
        StartPage,
        Page,
        UnsupportedFile,
        Input,
        InputSensitive,
        ClientCertificate,
        Error,
        InternalError
    }
}
