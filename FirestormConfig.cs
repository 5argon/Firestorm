using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Firebase.Auth;
using LitJson;
using UnityEngine;
using UnityEngine.Networking;

namespace E7.Firestorm
{
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
                    if (instance == null)
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
        /// No trailing and starting slash
        /// </summary>
        public string DocumentPathFromProjects => $"projects/{ProjectID}/databases/(default)/documents";

        /// <summary>
        /// The REST API url from http:// up to ".../documents", then you add one more slash or colon and continue building the url.
        /// You could do REST API in [this category](https://firebase.google.com/docs/firestore/reference/rest/?authuser=1#rest-resource-v1beta1projectsdatabasesdocuments) with this.
        /// </summary>
        public string RestDocumentBasePath => $"{Firestorm.restApiBaseUrl}/{DocumentPathFromProjects}";


        /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
        internal async Task<UnityWebRequest> UWRGet(string path)
        {
            UnityWebRequest uwr = UnityWebRequest.Get($"{FirestormConfig.Instance.RestDocumentBasePath}{path}");
            return await SetupAndSendUWRAsync(uwr);
        }

        /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
        internal async Task<UnityWebRequest> UWRPost(string path, (string, string)[] queryParameters, byte[] postData)
        {
            return await SetupAndSendUWRAsync(BuildPost(path, queryParameters, postData));
        }

        private UnityWebRequest BuildPost(string path, (string, string)[] queryParameters, byte[] postData)
        {
            var queryUrlBuilder = new StringBuilder();
            if (queryParameters != null && queryParameters.Length > 0)
            {
                queryUrlBuilder.Append("?");
                foreach (var kvp in queryParameters)
                {
                    queryUrlBuilder.Append($"{kvp.Item1}={kvp.Item2}");
                    queryUrlBuilder.Append("&");
                }
            }

            //UWR did not put the dictionary in the URL as could be interpreted from https://docs.unity3d.com/ScriptReference/Networking.UnityWebRequest.Post.html
            //It LOOKS LIKE query parameter but they are somewhere in the form data not on the URL
            UnityWebRequest uwr = UnityWebRequest.Post($"{FirestormConfig.Instance.RestDocumentBasePath}{path}{queryUrlBuilder.ToString()}", "");
            uwr.uploadHandler = new UploadHandlerRaw(postData);
            return uwr;
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
            //Debug.Log($"Deleting {FirestormConfig.Instance.RestDocumentBasePath}{path}");
            UnityWebRequest uwr = UnityWebRequest.Delete($"{FirestormConfig.Instance.RestDocumentBasePath}{path}");
            return await SetupAndSendUWRAsync(uwr);
        }

        /// <param name="path">To append to the base document path. It must include a slash, because the other possibility is the colon.</param>
        internal async Task<UnityWebRequest> UWRPatch(string path, (string, string)[] queryParameters, byte[] postData)
        {
            var uwr = BuildPost(path, queryParameters, postData);

            // Unity give us no PATCH constructor
            uwr.method = "PATCH";

            // This does not work to fix Android problem (https://stackoverflow.com/questions/25163131/httpurlconnection-invalid-http-method-patch)
            // I thought it could help fake PATCH on Android while being a POST request
            // But CFS server do not look at the override at all and returns bad request for POST which updates the data.
            // I left it here just in case it is supported in the future.
            uwr.SetRequestHeader("X-HTTP-Method-Override", "PATCH");

            return await SetupAndSendUWRAsync(uwr);
        }

        /// <summary>
        /// Put the login token in the REST request. This waits for the request's reponse completely.
        /// </summary>
        private async Task<UnityWebRequest> SetupAndSendUWRAsync(UnityWebRequest uwr)
        {
            //Debug.Log($"Checking user in the Auth instance {Firestorm.AuthInstance.App.Name} -> {Firestorm.AuthInstance?.CurrentUser?.UserId}");
            if (Firestorm.AuthInstance.CurrentUser == null)
            {
                throw new FirestormException($"Login with FirebaseAuth first!");
            }
            var token = await Firestorm.AuthInstance.CurrentUser.TokenAsync(forceRefresh: false);
            uwr.SetRequestHeader("Authorization", $"Bearer {token}");
            Debug.Log($"Sending {uwr.uri} {uwr.url}");
            var ao = uwr.SendWebRequest();
            await ao.WaitAsync();
            Debug.Log($"Done! {ao.webRequest.isDone} {ao.webRequest.isHttpError} {ao.webRequest.isNetworkError} {ao.webRequest.error} {ao.webRequest.downloadHandler?.text}");
            if (ao.webRequest.isHttpError || ao.webRequest.isNetworkError)
            {
                ErrorMessage googleError = default;
                if (ao.webRequest.isHttpError && ao.webRequest.downloadHandler != null)
                {
                    //Getting Google's error message
                    JsonData jt = JsonMapper.ToObject(ao.webRequest.downloadHandler.text);
                    if (jt.IsArray)
                    {
                        jt = jt[0];
                    }
                    else if (jt.IsObject == false)
                    {
                        throw new FirestormException($"Not expecting {jt.GetJsonType()} from the server..");
                    }

                    googleError = JsonMapper.ToObject<ErrorMessage>(jt.ToJson());

                    if (googleError != null)
                    {
                        if (googleError.error.code == 404 && googleError.error.status == "NOT_FOUND")
                        {
                            throw new FirestormDocumentNotFoundException(googleError.error.message);
                        }
                        if (googleError.error.code == 400 && googleError.error.message.Contains("The query requires an index"))
                        {
                            throw new FirestormPleaseCreateCompositeIndexException($"{googleError.error.message}");
                        }
                    }
                }

                string googleErrorString = "";
                if(googleError!= null)
                {
                    googleErrorString = $"{googleError.error.message}\nStatus code : {googleError.error.status}";
                }

                throw new FirestormWebRequestException(uwr, $"UnityWebRequest error : {ao.webRequest.error}\n{googleError}");
            }
            return ao.webRequest;
        }

#pragma warning disable 0649
        // private class ErrorMessages
        // {
        //     public ErrorMessage[] errors;
        // }

        /// <summary>
        /// https://cloud.google.com/apis/design/errors
        /// </summary>
        [Serializable]
        private class ErrorMessage
        {
            public Status error;
        }

        [Serializable]
        private class Status
        {
            public int code;
            public string message;
            public string status;
        }
#pragma warning restore 0649

        //For play mode test ON THE REAL DEVICE the DEVELOPMENT_BUILD will be on
        //You will be able to use super user account on those tests.
#if UNITY_EDITOR || DEVELOPMENT_BUILD

#pragma warning disable 0649
        // /// <summary>
        // /// DO NOT commit this file to public repository!
        // /// </summary>
        // public TextAsset serviceAccountPrivateKey;

        //This two string would not be in the real build according to preprocessor.
        public string superUserEmail;
        public string superUserPassword;
#pragma warning restore 0649

        // public ServiceAccountPrivateKey ServiceAccountPrivateKey
        // {
        //     get
        //     {
        //         if (serviceAccountPrivateKey == null)
        //         {
        //             throw new FirestormException($"{nameof(serviceAccountPrivateKey)} field of your {nameof(FirestormConfig)} file is not attached with the json text file! You should go create one at Project Settings > Service Account.");
        //         }
        //         return JsonUtility.FromJson<ServiceAccountPrivateKey>(serviceAccountPrivateKey.text);
        //     }
        // }

#endif
    }



}