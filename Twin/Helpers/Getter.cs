using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Twin.Helpers
{
    internal class Getter<T>
    {
        private Func<T> factory;

        public T Value => factory();

        public Getter(Func<T> factory)
        {
            this.factory = factory;
        }
    }
}
