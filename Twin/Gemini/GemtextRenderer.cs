using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using Microsoft.UI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading.Tasks;
using Windows.UI;
using Microsoft.UI;

namespace Twin.Gemini
{
    internal class GemtextRenderer
    {
        public static List<Inline> Render(string input, Uri currentHost)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            List<Inline> result = new List<Inline>();

            using (StringReader reader = new(input))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.StartsWith("```"))
                    {
                        Span span = new();
                        span.FontFamily = new FontFamily("Cascadia Mono");
                        while ((line = reader.ReadLine()) != "```")
                        {
                            Run run = new();
                            run.Text = line + "\n";
                            span.Inlines.Add(run);
                        }
                        var splitLine = line.Split(null, 2);
                        if (splitLine.Length == 2)
                        {
                            ToolTipService.SetToolTip(span, splitLine[1]);
                            ToolTipService.SetPlacement(span, Microsoft.UI.Xaml.Controls.Primitives.PlacementMode.Top);
                        }
                        result.Add(span);
                    }
                    else if (line.StartsWith("#"))
                    {
                        Run run = new();
                        int count = 0;
                        foreach (char a in line[0..3])
                        {
                            if (a == '#') count++;
                        }
                        run.FontSize = count switch
                        {
                            1 => 40,
                            2 => 28,
                            3 => 20,
                            _ => throw new NotImplementedException()
                        };
                        run.FontWeight = FontWeights.SemiBold;
                        run.Text = line.TrimStart('#').TrimStart() + "\n";
                        result.Add(run);
                    }
                    else if (line.StartsWith("=>"))
                    {
                        string[] splitLine = line[2..^0].Trim().Split(null, 2);
                        Uri uri;
                        if (!splitLine[0].Contains("://"))
                        {
                            UriBuilder builder = new();
                            builder.Scheme = "gemini";
                            builder.Port = currentHost.Port;
                            builder.Host = currentHost.Host;
                            string[] pathAndQuery = splitLine[0].Split('?', 2);
                            if (splitLine[0].StartsWith('/'))
                            {
                                builder.Path = pathAndQuery[0];
                            }
                            else
                            {
                                builder.Path = $"{currentHost.AbsolutePath.TrimEnd('/')}/{pathAndQuery[0].TrimStart('/')}";
                            }
                            if (pathAndQuery.Length == 2)
                            {
                                builder.Query = pathAndQuery[1];
                            }
                            uri = builder.Uri;
                        }
                        else
                        {
                            uri = new Uri(splitLine[0]);
                        }
                        Hyperlink hyperlink = new();
                        hyperlink.NavigateUri = uri;
                        if (splitLine.Length == 2)
                        {
                            Run run = new();
                            run.Text = splitLine[1].Trim() + "\n";
                            hyperlink.Inlines.Add(run);
                        }
                        else
                        {
                            Run run = new();
                            run.Text = uri.PathAndQuery + "\n";
                            hyperlink.Inlines.Add(run);
                        }
                        ToolTipService.SetToolTip(hyperlink, $"{uri.Scheme}://{uri.Host}{(uri.Port != 1965 && uri.Port != -1 ? $":{uri.Port}" : ":1965")}{uri.PathAndQuery}");
                        result.Add(hyperlink);
                    }
                    else if (line.StartsWith("*"))
                    {
                        line = "•" + line.TrimStart('*');
                        Run run = new();
                        run.Text = line + "\n";
                        result.Add(run);
                    }
                    else if (line.StartsWith(">"))
                    {
                        var newLine = line.TrimStart('>').Trim();
                        InlineUIContainer quote = new()
                        {
                            Child = new Border()
                            {
                                BorderThickness = new Thickness(1, 0, 0, 0),
                                BorderBrush = new SolidColorBrush(Colors.WhiteSmoke),
                                Child = new TextBlock()
                                {
                                    Text = newLine
                                }
                            }
                        };
                    }
                    else
                    {
                        Run run = new();
                        run.Text = line + "\n";
                        result.Add(run);
                    }
                }
            }
            stopwatch.Stop();
            Debug.WriteLine($"Formatting done in {stopwatch.Elapsed.TotalSeconds} s");
            return result;
        }
    }
}
