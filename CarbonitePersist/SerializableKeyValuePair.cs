using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

namespace CarbonitePersist
{
    public struct SerializableKeyValuePair<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }
    }
}
