using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
{
    public abstract record GemtextNode(string Content);

    public record TextNode(string Content) : GemtextNode(Content);

    public record LinkNode(string Content, Uri Uri) : GemtextNode(Content);

    public record PreformatNode(string Content, string? AltText) : GemtextNode(Content);

    public record HeadingNode(string Content, int Level) : GemtextNode(Content);

    public record ListItemNode(string Content) : GemtextNode(Content);

    public record QuoteNode(string Content) : GemtextNode(Content);
}

