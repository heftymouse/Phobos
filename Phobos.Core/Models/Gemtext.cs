using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
{
    public enum GemtextNodeType
    {
        Text,
        Link,
        Preformat,
        Heading,
        ListItem,
        Quote
    }

    public abstract record GemtextNode
    {
        public GemtextNodeType Type { get; init; }
        public string Content { get; init; }

        protected GemtextNode(string content, GemtextNodeType type)
        {
            this.Content = content;
            this.Type = type;
        }
    }

    public record TextNode(string Content) : GemtextNode(Content, GemtextNodeType.Text);

    public record LinkNode(string Content, Uri Uri) : GemtextNode(Content, GemtextNodeType.Link);

    public record PreformatNode(string Content, string? AltText) : GemtextNode(Content, GemtextNodeType.Preformat);

    public record HeadingNode(string Content, int Level) : GemtextNode(Content, GemtextNodeType.Heading);

    public record ListItemNode(string Content) : GemtextNode(Content, GemtextNodeType.ListItem);

    public record QuoteNode(string Content) : GemtextNode(Content, GemtextNodeType.Quote);
}

