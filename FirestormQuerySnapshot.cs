using System;
using System.Collections.Generic;
using E7.Firebase.LitJson;

namespace E7.Firebase
{

    [Serializable]
    public struct FirestormQuerySnapshot
    {
        private List<FirestormDocumentSnapshot> documents;
        public IEnumerable<FirestormDocumentSnapshot> Documents => documents;

        public FirestormQuerySnapshot(string collectionJson)
        {
            documents = new List<FirestormDocumentSnapshot>();
            var jo = JsonMapper.ToObject(collectionJson);
            if (jo.ContainsKey("documents"))
            {
                foreach (JsonData tk in jo["documents"])
                {
                    documents.Add(new FirestormDocumentSnapshot(tk.ToJson()));
                }
            }
            else if (jo.Count == 0)
            {
                return;
            }
            else
            {
                throw new FirestormException($"Did not expect non-empty and not having documents at root..");
            }
        }
    }

}