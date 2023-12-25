using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.UI;
using Phobos.Core.Models;
using Microsoft.UI.Text;
using Phobos.Helpers;

namespace Phobos.Gemini
{
    internal class GemtextRenderer
    {
        public static void Render(List<GemtextNode> document, InlineCollection result, Action<Uri> linkClick)
        {
            Stopwatch stopwatch = new();
            stopwatch.Start();
            result.Clear();

            foreach (GemtextNode node in document)
            {
                switch (node.Type)
                {
                    case GemtextNodeType.Preformat:
                        {
                            Span span = new()
                            {
                                FontFamily = new FontFamily("Cascadia Mono, Consolas"),
                            };
                            span.Inlines.Add(new Run() { Text = node.Content });
                            if ((node as PreformatNode).AltText != null)
                            {
                                ToolTipService.SetToolTip(span, (node as PreformatNode).AltText);
                            }
                            result.Add(span);
                            break;
                        }

                    case GemtextNodeType.Heading:
                        {
                            Run run = new()
                            {
                                Text = node.Content,
                                FontSize = (node as HeadingNode).Level switch
                                {
                                    1 => 40,
                                    2 => 28,
                                    3 => 20,
                                    _ => throw new NotImplementedException(),
                                },
                                FontWeight = FontWeights.SemiBold
                            };
                            result.Add(run);
                            break;
                        }

                    case GemtextNodeType.Link:
                        {
                            var linkNode = node as LinkNode;
                            Hyperlink link = new();
                            link.Inlines.Add(new Run() { Text = linkNode.Content });
                            link.Click += (sender, args) => { linkClick(linkNode.Uri); };
                            ToolTipService.SetToolTip(link, UriHelpers.UriToString(linkNode.Uri));
                            result.Add(link);
                            break;
                        }

                    case GemtextNodeType.ListItem:
                        {
                            Run run = new() { Text = $"• {node.Content}" };
                            result.Add(run);
                            break;
                        }

                    case GemtextNodeType.Quote:
                        {
                            Run run = new() { Text = node.Content, FontStyle = Windows.UI.Text.FontStyle.Italic };
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
