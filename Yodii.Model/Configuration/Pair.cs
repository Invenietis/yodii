using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public class Pair<TKey, TValue>
    {
        public TKey Item1 { get; set; }
        public TValue Item2 { get; set; }

        public Pair(TKey a, TValue b)
        {
            Item1 = a;
            Item2 = b;
        }
    }  
}
