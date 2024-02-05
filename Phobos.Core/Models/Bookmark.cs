using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Phobos.Core.Models
{
    // todo: fix this when dapper supports not doing this
    public record Bookmark
    {
        public string Name { get; init; }
        public Uri Uri { get; init; }

        public Bookmark(string name, Uri uri)
        {
            Name = name;
            Uri = uri;
        }
    }
}
