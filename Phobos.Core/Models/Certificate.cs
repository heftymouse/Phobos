using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
{
    public record Certificate(string Subject, byte[] Thumbprint);

    public enum CertificateStatus
    {
        Ok,
        New,
        Mismatch
    }
}
