using System;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using UnityEngine;

public struct FirestormCollection
{
    internal StringBuilder stringBuilder;
    public FirestormCollection(string name)
    {
        stringBuilder = new StringBuilder($"/{name}");
    }

    public FirestormCollection(FirestormDocument sb, string name)
    {
        this.stringBuilder = sb.stringBuilder;
        this.stringBuilder.Append($"/{name}");
    }

    public FirestormDocument Document(string name) => new FirestormDocument(this, name);

    /// <summary>
    /// Get all documents in the collection.
    /// Pagination, ordering, field masking, missing document, and transactions are not supported.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync()
    {
        var uwr = await FirestormConfig.Instance.UWRGet(stringBuilder.ToString());
        Debug.Log($"Getting query snapshot : {uwr.downloadHandler.text} {uwr.error}");
        return new FirestormQuerySnapshot(uwr.downloadHandler.text);
    }

    public async Task<FirestormDocument> AddAsync<T>(T documentData) where T : class, new()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// The real C# API will be .WhereGreaterThan .WhereLessThan etc. methods. I am just too lazy to imitate them all.
    /// </summary>
    public async Task<FirestormQuerySnapshot> GetSnapshotAsync(params (string fieldName, string operationString, object target)[] queries)
    {
        throw new NotImplementedException();
    }
}
