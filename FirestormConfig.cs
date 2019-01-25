using UnityEngine;

[CreateAssetMenu(menuName = Firestorm.assetMenuName + nameof(FirestormConfig))]
public class FirestormConfig : ScriptableObject
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

//For play mode test ON THE REAL DEVICE the DEVELOPMENT_BUILD will be on
//You will be able to use service account on those tests.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    /// <summary>
    /// DO NOT commit this file to public repository!
    /// </summary>
    public TextAsset serviceAccountPrivateKey;

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