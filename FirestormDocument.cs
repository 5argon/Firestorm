using System;
using System.Collections.Generic;

using ValueObject = System.Collections.Generic.Dictionary<string, object>;

namespace E7.Firebase
{
    /// <summary>
    /// The document name will be a path from "projects/..." to the document. This is used in a commit operation which requires a whole document.
    /// </summary>
    public struct FirestormDocumentForCommit
    {
        public string name;
        public Dictionary<string, ValueObject> fields;

        public FirestormDocumentForCommit(string path, FirestormDocument fd)
        {
            this.name = path;
            this.fields = fd.fields;
        }
    }

    /// <summary>
    /// Use for receiving document from server, it will contains create and update time.
    /// </summary>
    public struct FirestormDocument
    {
        public string name;
        public DateTime createTime;
        public DateTime updateTime;
        //The real content is here, but they are still in "___Value" x object format. Dict key is the field's name. Inner dict key is the ___Value text.
        public Dictionary<string, ValueObject> fields;
    }
}