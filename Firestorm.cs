using Firebase;
using Firebase.Auth;
using UnityEngine;

namespace E7.Firestorm
{
    public static class Firestorm
    {
        public const string assetMenuName = nameof(Firestorm) + "/";
        public const string restApiBaseUrl = "https://firestore.googleapis.com/v1beta1";

        /// <summary>
        /// Start building the collection-document path here.
        /// </summary>
        public static FirestormCollectionReference Collection(string name) => new FirestormCollectionReference(name);

        //Prevents garbage collection bug
        private static FirebaseApp appInstance;
        private static FirebaseAuth authInstance;

        public static void CreateEditModeInstance()
        {
            DisposeEditModeInstance();
            appInstance = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, $"TestInstance {UnityEngine.Random.Range(10000, 99999)}");
        }

        public static void DisposeEditModeInstance()
        {
            //Somehow this crashes Unity lol... the network is left running in the other thread it seems.
            appInstance?.Dispose();
        }

        public static FirebaseAuth AuthInstance
        {
            get
            {
                if (Application.isPlaying)
                {
                    appInstance = FirebaseApp.DefaultInstance;
                    authInstance = FirebaseAuth.GetAuth(appInstance);
                    return authInstance;
                }
                else
                {
                    //Firebase said you should not use default instance in editor (edit mode test also)
                    //https://firebase.google.com/docs/unity/setup#desktop_workflow
                    if (appInstance == null)
                    {
                        throw new FirestormException($"You forgot to call CreateEditModeInstance before running in edit mode");
                    }
                    authInstance = FirebaseAuth.GetAuth(appInstance);
                    return authInstance;
                }
            }
        }

        public static class IndexManager
        {
        }
    }

}