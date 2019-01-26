using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
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
    /// The REST API url up to ".../documents", then you add one more slash and continue building the url.
    /// You could do REST API in [this category](https://firebase.google.com/docs/firestore/reference/rest/?authuser=1#rest-resource-v1beta1projectsdatabasesdocuments) with this.
    /// </summary>
    public string RestDocumentBasePath => $"{Firestorm.restApiBaseUrl}/projects/{ProjectID}/databases/(default)/documents";

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


