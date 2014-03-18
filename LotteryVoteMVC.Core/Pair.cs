using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LotteryVoteMVC.Core
{
    public class Pair<K, V>
    {
        public Pair() { }
        public Pair(K k, V v)
        {
            this.Key = k;
            this.Value = v;
        }

        public K Key { get; set; }
        public V Value { get; set; }
    }
}
