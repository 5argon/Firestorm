using System;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;

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
        //Check if a document is there or not
        if (setOption == SetOption.MergeAll)
        {
            //lol Android does not support custom verb for UnityWebRequest so we could not use PATCH 
            //(https://docs.unity3d.com/Manual/UnityWebRequest.html)
            //"custom verbs are permitted on all platforms except for Android"
            //(https://stackoverflow.com/questions/19797842/patch-request-android-volley)
            //(https://answers.unity.com/questions/1230067/trying-to-use-patch-on-a-unitywebrequest-on-androi.html)

            //TODO : Try using POST with X-HTTP-Method-Override: PATCH and see if Firebase's server supports overriding or not?
        }
        else if (setOption == SetOption.Overwrite)
        {
            //Getting fields of existing data.
            var snapshot = await GetSnapshotAsync();
            var fieldMask = snapshot.FieldsDocumentMaskJson();
            if(snapshot.IsEmpty)
            {
                //Create a document
            }
            else
            {
                //Patch a document, using fields from REMOTE so outliers are removed.
            }
        }
    }

    /// <summary>
    /// Deletes the entire document. Deleting fields is not implemented.
    /// </summary>
    public async Task DeleteAsync()
    {
        await FirestormConfig.Instance.UWRDelete(sb.ToString());
        Debug.Log($"Delete finished");
    }

    public async Task<FirestormDocumentSnapshot> GetSnapshotAsync()
    {
        Debug.Log($"do {sb.ToString()}");
        try
        {
            var uwr = await FirestormConfig.Instance.UWRGet(sb.ToString());
            Debug.Log($"done {uwr.downloadHandler.text}");
            return new FirestormDocumentSnapshot(uwr.downloadHandler.text);
        }
        catch (FirestormWebRequestException fe)
        {
            if (fe.ErrorCode == 404)
            {
                return FirestormDocumentSnapshot.Empty;
            }
            else
            {
                throw;
            }
        }
    }

}
