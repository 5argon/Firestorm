using System;
using System.Text;
using System.Threading.Tasks;

public struct FirestormDocument
{
    internal StringBuilder sb;
    public FirestormDocument(FirestormCollection sb, string name)
    {
        this.sb = sb.sb;
        this.sb.Append($"/{name}");
    }

    public FirestormCollection Collection(string name) => new FirestormCollection(this, name);
    
    /// <summary>
    /// Either make a new document, overwrites or update a subset of fields depending on SetOption.
    /// Not sure if array is supported or not by using List<T> in the data?
    /// 
    /// There is no AddAsync (to make a new document in a collection without naming it) implemented like in the real C# API.
    /// </summary>
    public async Task SetAsync<T>(T documentData, SetOption setOption)
    {
    }

    /// <summary>
    /// Deletes the entire document. Deleting fields is not implemented.
    /// </summary>
    public async Task DeleteAsync()
    {
        await FirestormConfig.Instance.UWRDelete(sb.ToString());
    }

    public async Task<FirestormDocumentSnapshot> GetSnapshotAsync()
    {
        throw new NotImplementedException();
    }

}
