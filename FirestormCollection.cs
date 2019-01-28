using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct FirestormCollection
{
    internal StringBuilder sb;
    public FirestormCollection(string name)
    {
        sb = new StringBuilder($"/{name}");
    }

    public FirestormCollection(FirestormDocument sb, string name)
    {
        this.sb = sb.sb;
        this.sb.Append($"/{name}");
    }

    public FirestormDocument Document(string name) => new FirestormDocument(this, name);

    /// <summary>
    /// Get all documents in the collection.
    /// Pagination, ordering, field masking, missing document, and transactions are not supported.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync()
    {
        var uwr = await FirestormConfig.Instance.UWRGet(sb.ToString());
        Debug.Log($"Getting query snapshot : {uwr.downloadHandler.text}");
        return JsonUtility.FromJson<FirestormQuerySnapshot>(uwr.downloadHandler.text);
    }

    /// <summary>
    /// The real C# API will be .WhereGreaterThan .WhereLessThan etc. methods. I am just too lazy to imitate them all.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync(params (string fieldName, string operationString, object target)[] queries)
    {
        throw new NotImplementedException();
    }
}
