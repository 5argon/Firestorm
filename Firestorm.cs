using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine;

public class Firestorm
{
    public const string assetMenuName = nameof(Firestorm) + "/";
    public const string restApiBaseUrl = "https://firestore.googleapis.com/v1beta1";

    /// <summary>
    /// Start building the collection-document path here.
    /// </summary>
    public static FirestormCollection Collection(string name) => new FirestormCollection(name);
}

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
}

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

    public async Task<FirestormDocumentSnapshot> GetSnapshotAsync()
    {
    }

    /// <summary>
    /// The real C# API will be .WhereGreaterThan .WhereLessThan etc. methods. I am just too lazy to imitate them all.
    /// </summary>
    public async Task<FirestormDocumentSnapshot> GetSnapshotAsync(params (string fieldName, string operationString, object target)[] queries)
    {
    }
}

public struct FirestormDocumentSnapshot
{
    private string jsonString;

    /// <summary>
    /// Uses Unity's JsonUtility.
    /// </summary>
    public T ConvertTo<T>() => JsonUtility.FromJson<T>(jsonString);
}

public enum SetOption
{
    Overwrite,
    MergeAll
}