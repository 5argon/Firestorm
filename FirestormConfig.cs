using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using UnityEngine;
using UnityEngine.Networking;


[CreateAssetMenu(menuName = Firestorm.assetMenuName + nameof(FirestormConfig))]
public partial class FirestormConfig : ScriptableObject
{
    private static FirestormConfig instance;

    /// <summary>
    /// Get the config <see cref="ScriptableObject"> from <see cref="Resources"> folder. It is cached after the first load.
    /// </summary>
    public static FirestormConfig Instance
    {
        get
        {
            if (instance == null)
            {
                instance = Resources.Load<FirestormConfig>(nameof(FirestormConfig));
                if(instance == null)
                {
                    throw new FirestormException($"Scriptable object named {nameof(FirestormConfig)} is not available in Resources folder!");
                }
            }
            return instance;
        }
    }

#pragma warning disable 0649
    [SerializeField] private string projectId;
#pragma warning restore 0649

    /// <summary>
    /// See your Project ID in Settings page, it is not your project name.
    /// </summary>
    public string ProjectID => projectId;

    /// <summary>
    /// The REST API url up to ".../documents", then you add one more slash or colon and continue building the url.
    /// You could do REST API in [this category](https://firebase.google.com/docs/firestore/reference/rest/?authuser=1#rest-resource-v1beta1projectsdatabasesdocuments) with this.
    /// </summary>
    public string RestDocumentBasePath => $"{Firestorm.restApiBaseUrl}/projects/{ProjectID}/databases/(default)/documents";

    /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
    internal async Task<UnityWebRequest> UWRGet(string path)
    {
        UnityWebRequest uwr = UnityWebRequest.Get($"{FirestormConfig.Instance.RestDocumentBasePath}{path}");
        return await SetupAndSendUWRAsync(uwr);
    }

    /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
    internal async Task<UnityWebRequest> UWRPost(string path, Dictionary<string, string> postData)
    {
        UnityWebRequest uwr = UnityWebRequest.Post($"{FirestormConfig.Instance.RestDocumentBasePath}{path}", postData);
        return await SetupAndSendUWRAsync(uwr);
    }

    /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
    internal async Task<UnityWebRequest> UWRPut(string path, byte[] bodyData)
    {
        UnityWebRequest uwr = UnityWebRequest.Put($"{FirestormConfig.Instance.RestDocumentBasePath}{path}", bodyData);
        return await SetupAndSendUWRAsync(uwr);
    }

    /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
    internal async Task<UnityWebRequest> UWRDelete(string path)
    {
        Debug.Log($"Deleting {FirestormConfig.Instance.RestDocumentBasePath}{path}");
        UnityWebRequest uwr = UnityWebRequest.Delete($"{FirestormConfig.Instance.RestDocumentBasePath}{path}");
        return await SetupAndSendUWRAsync(uwr);
    }

    /// <summary>
    /// Put the login token in the REST request. This waits for the request's reponse completely.
    /// </summary>
    private async Task<UnityWebRequest> SetupAndSendUWRAsync(UnityWebRequest uwr)
    {
        var token = await FirebaseAuth.DefaultInstance.CurrentUser.TokenAsync(forceRefresh: true);
        uwr.SetRequestHeader("Authorization", $"Bearer {token}");
        Debug.Log($"Sending {uwr.uri} {uwr.url}");
        var ao = uwr.SendWebRequest();
        await ao.WaitAsync();
        Debug.Log($"Done! {ao.webRequest.isDone} {ao.webRequest.isHttpError} {ao.webRequest.isNetworkError} {ao.webRequest.error} {ao.webRequest.downloadHandler?.text}");
        if (ao.webRequest.isHttpError || ao.webRequest.isNetworkError)
        {
            throw new FirestormException($"UnityWebRequest error : {ao.webRequest.error}");
        }
        return ao.webRequest;
    }

//For play mode test ON THE REAL DEVICE the DEVELOPMENT_BUILD will be on
//You will be able to use service account on those tests.
#if UNITY_EDITOR || DEVELOPMENT_BUILD

#pragma warning disable 0649
    /// <summary>
    /// DO NOT commit this file to public repository!
    /// </summary>
    public TextAsset serviceAccountPrivateKey;
    public string superUserEmail;
    public string superUserPassword;
#pragma warning restore 0649

    public ServiceAccountPrivateKey ServiceAccountPrivateKey
    {
        get
        {
            if (serviceAccountPrivateKey == null)
            {
                throw new FirestormException($"{nameof(serviceAccountPrivateKey)} field of your {nameof(FirestormConfig)} file is not attached with the json text file! You should go create one at Project Settings > Service Account.");
            }
            return JsonUtility.FromJson<ServiceAccountPrivateKey>(serviceAccountPrivateKey.text);
        }
    }

#endif
}


