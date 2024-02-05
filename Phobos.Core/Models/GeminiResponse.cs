using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
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

    public record GeminiResponse(
        GeminiResponseType Type,
        int Code,
        string Meta,
        byte[] Body,
        CertificateStatus CertificateStatus,
        Certificate Certificate
    );
}
