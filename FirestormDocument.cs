using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using UnityEngine;

public struct FirestormDocument
{
    internal StringBuilder stringBuilder;
    private string documentName;
    private string parent;
    public FirestormDocument(FirestormCollection collection, string name)
    {
        this.stringBuilder = collection.stringBuilder;
        this.parent = stringBuilder.ToString(); //save the parent collection path before appending.
        this.stringBuilder.Append($"/{name}");
        this.documentName = name;
    }

    public FirestormCollection Collection(string name) => new FirestormCollection(this, name);

    /// <summary>
    /// Either make a new document, overwrites or update a subset of fields depending on SetOption.
    /// Costs one read operation because it needs existing fields from the server.
    /// Not sure if array is supported or not by using List<T> in the data?
    /// 
    /// There is no AddAsync (to make a new document in a collection without naming it) implemented like in the real C# API.
    /// </summary>
    public async Task<T> SetAsync<T>(T documentData, SetOption setOption) where T : class, new()
    {
        //Check if a document is there or not

        var snapshot = await GetSnapshotAsync();
        if (snapshot.IsEmpty)
        {
            //Create a document 

            //Document "name" must not be set when creating a new one. The name should be in query parameter "documentId"
            string documentJson = JsonConvert.SerializeObject(documentData, Formatting.Indented, new DocumentConverter<T>(""));
            Debug.Log($"Name {documentName} DOCJ {documentJson}");

            byte[] postData = Encoding.UTF8.GetBytes(documentJson);

            var uwr = await FirestormConfig.Instance.UWRPost(parent, new Dictionary<string, string>
            {
                ["documentId"] = documentName,
            }, postData);
            return new FirestormDocumentSnapshot(uwr.downloadHandler.text).ConvertTo<T>();
        }

        //If there is a data.. we try to build the correct DocumentMask.

        if (setOption == SetOption.MergeAll)
        {
            //lol Android does not support custom verb for UnityWebRequest so we could not use PATCH 
            //(https://docs.unity3d.com/Manual/UnityWebRequest.html)
            //"custom verbs are permitted on all platforms except for Android"
            //(https://stackoverflow.com/questions/19797842/patch-request-android-volley)
            //(https://answers.unity.com/questions/1230067/trying-to-use-patch-on-a-unitywebrequest-on-androi.html)

            //TODO : Try using POST with X-HTTP-Method-Override: PATCH and see if Firebase's server supports overriding or not?
            throw new NotImplementedException();
        }
        else if (setOption == SetOption.Overwrite)
        {
            //Getting fields of existing data.
            else
            {
                //Patch a document, using fields from REMOTE so outliers are removed.

                var fieldMask = snapshot.FieldsDocumentMaskJson();
                throw new NotImplementedException();
            }
        }
        throw new NotImplementedException($"Set option {setOption} not implemented");
    }

    /// <summary>
    /// Deletes an entire document. Deleting fields is not implemented.
    /// </summary>
    public async Task DeleteAsync()
    {
        await FirestormConfig.Instance.UWRDelete(stringBuilder.ToString());
        //Debug.Log($"Delete finished");
    }

    public async Task<FirestormDocumentSnapshot> GetSnapshotAsync()
    {
        Debug.Log($"do {stringBuilder.ToString()}");
        try
        {
            var uwr = await FirestormConfig.Instance.UWRGet(stringBuilder.ToString());
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
