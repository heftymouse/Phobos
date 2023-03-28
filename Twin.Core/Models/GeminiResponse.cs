using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Core.Gemini
{
    public enum GeminiResponseType
    {
        Input = 1,
        Ok = 2,
        Redirect = 3,
        TemporaryFailure = 4,
        PermanentFailure = 5,
        ClientCertificateRequired = 6
    }

    public readonly record struct GeminiResponse(GeminiResponseType Type, int Code, string Meta, string Body);
}
