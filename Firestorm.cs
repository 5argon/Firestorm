using Firebase;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Networking;

public class Firestorm
{
    public const string assetMenuName = nameof(Firestorm) + "/";
    public const string restApiBaseUrl = "https://firestore.googleapis.com/v1beta1";

    /// <summary>
    /// Start building the collection-document path here.
    /// </summary>
    public static FirestormCollection Collection(string name) => new FirestormCollection(name);

    private static FirebaseApp editModeInstance;
    public static void CreateEditModeInstance()
    {
        DisposeEditModeInstance();
        editModeInstance = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, $"TestInstance {Random.Range(10000, 99999)}");
    }

    public static void DisposeEditModeInstance() => editModeInstance?.Dispose();

    public static FirebaseAuth AuthInstance
    {
        get
        {
            if (Application.isPlaying)
            {
                return FirebaseAuth.DefaultInstance;
            }
            else
            {
                //Firebase said you should not use default instance in editor (edit mode test also)
                //https://firebase.google.com/docs/unity/setup#desktop_workflow
                if(editModeInstance == null)
                {
                    throw new FirestormException($"You forgot to call CreateEditModeInstance before running in edit mode");
                }
                return FirebaseAuth.GetAuth(editModeInstance);
            }
        }
    }
}

public enum SetOption
{
    //Mask field follows the one on the server. So it deletes fields on the server on PATCH request
    Overwrite,
    //Mask field follows all of the one to write.
    MergeAll
}