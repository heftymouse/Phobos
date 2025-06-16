using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Phobos.Core.Models;
using Microsoft.UI.Text;
using Phobos.Helpers;
using Windows.UI.Text;
using Microsoft.UI;

namespace Phobos.Gemini
{
    internal class GemtextRenderer
    {
        static FontFamily monoFont = new("Cascadia Mono, Consolas");
        static SolidColorBrush grayBrush = new(Colors.Gray);

        public static void Render(List<GemtextNode> document, InlineCollection result, Action<Uri> linkClick)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            result.Clear();

            foreach (GemtextNode node in document)
            {
                switch (node)
                {
                    case PreformatNode pf:
                        {
                            Span span = new();
                            span.Inlines.Add(new Run() { Text = node.Content, FontFamily = monoFont });
                            if (pf.AltText != null)
                            {
                                span.Inlines.Add(new Run() { Text = pf.AltText, FontStyle = FontStyle.Italic, Foreground = grayBrush });
                            }
                            result.Add(span);
                            break;
                        }

                    case HeadingNode h:
                        {
                            Run run = new()
                            {
                                Text = node.Content,
                                FontSize = h.Level switch
                                {
                                    1 => 36,
                                    2 => 28,
                                    3 => 20,
                                    _ => throw new NotImplementedException(),
                                },
                                FontWeight = FontWeights.SemiBold
                            };
                            result.Add(run);
                            break;
                        }

                    case LinkNode ln:
                        {
                            Hyperlink link = new();
                            link.Inlines.Add(new Run() { Text = ln.Content });
                            link.Click += (sender, args) => { linkClick(ln.Uri); };
                            ToolTipService.SetToolTip(link, UriHelpers.UriToString(ln.Uri));
                            result.Add(link);
                            break;
                        }

                    case ListItemNode:
                        {
                            Run run = new() { Text = $"• {node.Content}" };
                            result.Add(run);
                            break;
                        }

                    case QuoteNode:
                        {
                            Run run = new() { Text = node.Content, FontStyle = FontStyle.Italic };
                            result.Add(run);
                            break;
                        }

                    default:
                        {
                            if (!string.IsNullOrEmpty(node.Content))
                                result.Add(new Run() { Text = node.Content });
                            break;
                        }
                }

                result.Add(new LineBreak());
            }

            stopwatch.Stop();
            Debug.WriteLine($"Conversion done in {stopwatch.Elapsed.TotalSeconds} s");
        }
    }
}
