using System;
using System.Collections.Generic;

[Serializable]
public struct FirestormQuerySnapshot 
{
    public List<FirestormDocumentSnapshot> documents;
    public IEnumerable<FirestormDocumentSnapshot> Documents => documents;
}
