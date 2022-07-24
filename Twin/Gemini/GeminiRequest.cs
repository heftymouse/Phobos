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

namespace Twin.Gemini
{
    internal class GeminiRequest
    {
        public static async Task<GeminiResponse> RequestPageAsync(Uri uri)
        {
            using (TcpClient client = new TcpClient(uri.Host, 1965))
            {
                using (SslStream sslStream = new SslStream(client.GetStream(),
                false,
                new RemoteCertificateValidationCallback(VerifyServerCert)
                ))
                {
                    await sslStream.AuthenticateAsClientAsync(uri.Host, null, SslProtocols.Tls13, true);

                    await sslStream.WriteAsync(Encoding.UTF8.GetBytes(uri.OriginalString + "\r\n"));
                    await sslStream.FlushAsync();
                    byte[] statusBytes = new byte[2];
                    await sslStream.ReadAsync(statusBytes, 0, statusBytes.Length);
                    string meta = "";
                    string data = "";
                    sslStream.ReadByte();
                    int nextByte;
                    for(int i = 1; i <= 1024; i++)
                    {
                        nextByte = sslStream.ReadByte();
                        if (nextByte == (int)'\r') break;
                        meta += (char)nextByte;
                    }
                    sslStream.ReadByte();
                    //client.ReceiveTimeout = 5000;
                    //sslStream.ReadTimeout = 500;
                    while (true)
                    {
                        byte[] arr = new byte[4096];
                        int bytesRead = await sslStream.ReadAsync(arr, 0, arr.Length);
                        data += Encoding.UTF8.GetString(arr);
                        if (bytesRead == 0) break;
                        data.TrimEnd('\0');
                    }

                    data.TrimEnd('\0');
                    return new GeminiResponse((GeminiResponseType)statusBytes[0] - 48, statusBytes[1] - 48, meta, data);
                }
            }
        }

        static bool VerifyServerCert(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            //byte[] hash = new byte[64];
            //certificate.TryGetCertHash(HashAlgorithmName.SHA256, hash, out int bytesWritten);
            return true;
        }
    }
}
