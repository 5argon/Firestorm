using UnityEngine.Networking;

public class Firestorm
{
    public const string assetMenuName = nameof(Firestorm) + "/";
    public const string restApiBaseUrl = "https://firestore.googleapis.com/v1beta1";

    /// <summary>
    /// Start building the collection-document path here.
    /// </summary>
    public static FirestormCollection Collection(string name) => new FirestormCollection(name);
}

public enum SetOption
{
    Overwrite,
    MergeAll
}