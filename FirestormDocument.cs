using System;
using System.Collections.Generic;

using ValueObject = System.Collections.Generic.Dictionary<string, object>;

namespace E7.Firebase
{
    public struct FirestormDocument
    {
        public string name;
        public DateTime createTime;
        public DateTime updateTime;
        //The real content is here, but they are still in "___Value" x object format. Dict key is the field's name. Inner dict key is the ___Value text.
        public Dictionary<string, ValueObject> fields;
    }
}