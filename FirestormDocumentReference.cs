using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine;

public struct FirestormDocumentReference
{
    internal StringBuilder stringBuilder;
    private string documentName;
    private string parent;
    public FirestormDocumentReference(FirestormCollectionReference collection, string name)
    {
        this.stringBuilder = collection.stringBuilder;
        this.parent = stringBuilder.ToString(); //save the parent collection path before appending.
        this.stringBuilder.Append($"/{name}");
        this.documentName = name;
    }

    public FirestormCollectionReference Collection(string name) => new FirestormCollectionReference(this, name);

    /// <summary>
    /// Either make a new document, overwrites or update a subset of fields depending on SetOption.
    /// Costs one read operation because it needs existing fields from the server.
    /// Not sure if array is supported or not by using List<T> in the data?
    /// 
    /// There is no AddAsync (to make a new document in a collection without naming it) implemented like in the real C# API.
    /// </summary>
    public async Task SetAsync<T>(T documentData, SetOption setOption) where T : class, new()
    {
        //Check if a document is there or not
        var snapshot = await GetSnapshotAsync();

        //Document "name" must not be set when creating a new one. The name should be in query parameter "documentId"
        //When updating the name must also be blank. It uses the name from REST URL already.
        string documentJson = JsonConvert.SerializeObject(documentData, Formatting.Indented, new DocumentConverter<T>(""));
        //File.WriteAllText(Application.dataPath + "/snap.txt", documentJson);
        if (snapshot.IsEmpty)
        {
            //Create a document 

            //Debug.Log($"Name {documentName} DOCJ {documentJson}");

            byte[] postData = Encoding.UTF8.GetBytes(documentJson);

            //The URL must NOT include document name
            var uwr = await FirestormConfig.Instance.UWRPost(parent, new (string, string)[]
            {
                ("documentId",documentName)
            }, postData);

            //It returns the same document but with the generated document ID in the case that createDocument request does not specify a name
            //We specify a name so there is no point to return any data.

            //return new FirestormDocumentSnapshot(uwr.downloadHandler.text).ConvertTo<T>();
        }
        else
        {
            //If there is a data.. we try to build the correct DocumentMask.

            string fieldMaskLocal = new FirestormDocumentSnapshot(documentJson).FieldsDocumentMaskJson();
            var localF = JsonConvert.DeserializeObject<DocumentMask>(fieldMaskLocal);
            var mergedFields = new HashSet<string>();

            if (setOption == SetOption.MergeAll)
            {
                //Getting fields of only local. The server will touch only field presents in the mask = merging.
                foreach (var f in localF.fieldPaths)
                {
                    mergedFields.Add(f);
                }
            }
            else if (setOption == SetOption.Overwrite)
            {
                //Getting fields of existing data.
                //Patch a document, using fields from remote combined with local.
                // Including all local fields ensure update
                // Including remote fields that does not intersect with local = delete on the server
                string fieldMaskRemote = snapshot.FieldsDocumentMaskJson();
                var remoteF = JsonConvert.DeserializeObject<DocumentMask>(fieldMaskRemote);
                foreach (var f in remoteF.fieldPaths)
                {
                    mergedFields.Add(f);
                }
                foreach (var f in localF.fieldPaths)
                {
                    mergedFields.Add(f);
                }
            }
            else
            {
                throw new NotImplementedException($"Set option {setOption} not implemented");
            }
            // var mergedMasks = new DocumentMask { fieldPaths = mergedFields.ToArray() };
            // var mergedMasksJson = JsonConvert.SerializeObject(mergedMasks);
            // var jo = JObject.Parse(mergedMasksJson);

            //Debug.Log($"Mask {mergedMasksJson} DOCJ {documentJson}");
            Debug.Log($"DOCJ {documentJson}");

            byte[] postData = Encoding.UTF8.GetBytes(documentJson);

            var updateMaskForUrl = mergedFields.Select(x => ("updateMask.fieldPaths", x)).ToArray();

            //lol Android does not support custom verb for UnityWebRequest so we could not use "PATCH"
            //(https://docs.unity3d.com/Manual/UnityWebRequest.html)
            //"custom verbs are permitted on all platforms except for Android"
            //(https://stackoverflow.com/questions/19797842/patch-request-android-volley)
            //(https://answers.unity.com/questions/1230067/trying-to-use-patch-on-a-unitywebrequest-on-androi.html)

            //TODO : Try using POST with X-HTTP-Method-Override: PATCH and see if Firebase's server supports overriding or not?
            var uwr = await FirestormConfig.Instance.UWRPatch(stringBuilder.ToString(), updateMaskForUrl, postData);

            //The patch request returns the same document.
            //return new FirestormDocumentSnapshot(uwr.downloadHandler.text).ConvertTo<T>();
        }
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
