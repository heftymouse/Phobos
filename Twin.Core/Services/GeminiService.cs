using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Buffers.Binary;
using System.Threading.Tasks;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Net.Sockets;
using Twin.Core.Gemini;
using CommunityToolkit.HighPerformance.Buffers;

namespace Twin.Core.Services
{
    public class GeminiService
    {
        public GeminiResponse RequestPageAsync(Uri uri)
        {
            using TcpClient client = new TcpClient(uri.Host, 1965);
            using SslStream sslStream = new SslStream(client.GetStream(), false, VerifyServerCert);

            sslStream.AuthenticateAsClient(uri.Host, null, SslProtocols.Tls13 | SslProtocols.Tls12, true);

            sslStream.Write(Encoding.UTF8.GetBytes(uri.ToString()));
            sslStream.Write("\r\n"u8);
            sslStream.Flush();

            using ArrayPoolBufferWriter<byte> buffer = new();
            while (true)
            {
                var span = buffer.GetSpan(4096);
                int bytesRead = sslStream.Read(span);
                buffer.Advance(bytesRead);
                if (bytesRead == 0) break;
            }

            var data = buffer.WrittenSpan;

            var headerEnd = data.IndexOf("\r\n"u8);
            var meta = Encoding.UTF8.GetString(data.Slice(3, headerEnd - 3));
            var body = Encoding.UTF8.GetString(data.Slice(headerEnd + 2));

            return new GeminiResponse((GeminiResponseType)data[0] - 48, data[1] - 48, meta, body);
        }

        static bool VerifyServerCert(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
        {
            //byte[] hash = new byte[64];
            //certificate.TryGetCertHash(HashAlgorithmName.SHA256, hash, out int bytesWritten);
            return true;
        }
    }
}
