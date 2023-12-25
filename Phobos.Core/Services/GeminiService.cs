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
using Phobos.Core.Models;
using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.HighPerformance;

namespace Phobos.Core.Services
{
	public class GeminiService
	{
		public GeminiResponse RequestPageAsync(Uri uri)
		{
			using TcpClient client = new TcpClient(uri.Host, uri.Port == -1 ? 1965 : uri.Port);
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
			var body = data.Slice(headerEnd + 2).AsBytes().ToArray();

			return new GeminiResponse((GeminiResponseType)data[0] - 48, data[1] - 48, meta, body);
		}

		static bool VerifyServerCert(object sender, X509Certificate? certificate, X509Chain? chain, SslPolicyErrors errors)
		{
			//byte[] hash = new byte[64];
			//certificate.TryGetCertHash(HashAlgorithmName.SHA256, hash, out int bytesWritten);
			return true;
		}

		public static List<GemtextNode> ParseBody(char[] body, Uri rootHost)
		{
			Stopwatch stopwatch = new();
			stopwatch.Start();
			List<GemtextNode> result = new();

			bool isPreformat = false;
			StringBuilder preformatBuilder = new();
			string preformatAlt = null;

			bool isQuote = false;
			StringBuilder quoteBuilder = new();

			var lines = body.AsSpan().EnumerateLines();
			foreach (var line in lines)
			{
                if (isQuote && line.Length > 0 && line[0] is not '>')
                {
                    isQuote = false;
                    result.Add(new QuoteNode(quoteBuilder.ToString()));
                }

                if (line.Length == 0)
				{
					result.Add(new TextNode(""));
					continue;
				}

				if (line.StartsWith("```".AsSpan()))
				{
					if (isPreformat)
					{
						isPreformat = false;
						result.Add(new PreformatNode(preformatBuilder.ToString(), preformatAlt));
					}
					else
					{
						isPreformat = true;
						Span<Range> ranges = new Range[2];
						var num = line.SplitAny(ranges, ReadOnlySpan<char>.Empty, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
						preformatBuilder = new();
						preformatAlt = num == 2 ? line[ranges[1]].ToString() : null;
					}
				}

				else if (isPreformat)
				{
					preformatBuilder.Append(line);
					preformatBuilder.Append('\n');
				}

				else if (line[0] is '#')
				{
					var count = line.Slice(0, 3).LastIndexOf('#') + 1;
					var heading = line.Slice(count).Trim().ToString();
					HeadingNode node = new(heading, count);
					result.Add(node);
				}

				else if (line.StartsWith("=>".AsSpan()))
				{
					var text = line.TrimStart(['=', '>']);
					Span<Range> ranges = new Range[2];
					var num = text.SplitAny(ranges, ReadOnlySpan<char>.Empty, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
					var uriString = text[ranges[0]].ToString();
					var uri = uriString.Contains("://")
						? new Uri(uriString)
						: new Uri(rootHost, uriString);
					var description = num == 2 ? text[ranges[1]].ToString() : uriString;
					result.Add(new LinkNode(description, uri));
				}

				else if (line[0] is '*')
				{
					result.Add(new ListItemNode(line.Slice(1).Trim().ToString()));
				}

				else if (line[0] is '>')
				{
					if(!isQuote)
					{
						isQuote = true;
						quoteBuilder = new();
					}	
					var text = line.TrimStart('>').Trim();
					quoteBuilder.Append(text);
					quoteBuilder.Append('\n');
				}

				else
					result.Add(new TextNode(line.ToString()));
			}

			stopwatch.Stop();
			Debug.WriteLine($"Parsing done in {stopwatch.Elapsed.TotalSeconds} s");
			return result;
		}
	}
}
