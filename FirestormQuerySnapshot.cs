using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

[Serializable]
public struct FirestormQuerySnapshot 
{
    public List<FirestormDocumentSnapshot> documents;
    public IEnumerable<FirestormDocumentSnapshot> Documents => documents;

    public FirestormQuerySnapshot(string collectionJson)
    {
        documents = new List<FirestormDocumentSnapshot>();
        var jo = JObject.Parse(collectionJson);
        if(jo.ContainsKey("documents"))
        {
            foreach(var tk in jo["documents"].Children())
            {
                documents.Add(new FirestormDocumentSnapshot(tk.ToString()));
            }
        }
        else if(jo.HasValues == false)
        {
            return;
        }
        else
        {
            throw new FirestormException($"Did not expect non-empty and not having documents at root..");
        }
    }
}
