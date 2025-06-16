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
        public string Name { get; set; }
        public Uri Uri { get; set; }

        public Bookmark(string name, Uri uri)
        {
            Name = name;
            Uri = uri;
        }
    }
}
